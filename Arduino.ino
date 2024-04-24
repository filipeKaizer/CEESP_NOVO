#include <ModbusMaster.h>
#include <EEPROM.h>
#include "Wire.h"

//----------------------------------- DEFINIÇÕES -----------------------------------//
ModbusMaster node;          // instantiate ModbusMaster object

#define MAX485_DE                               3         // Pino de sentido de transmissão
#define ledRed                                  A0        // led - Red
#define ledGreen                                A1        // led - Green
#define ledBlue                                 A2        // led - Blue
#define sensorRPM                               2         // Sensor de RPM, HIGH quando interrompido
#define MMW2_ID                                 1         // ID do dispositivo medidor MMW02
#define AddrNum                                 80        // Numero de endereços obtidos pelo MMW02
#define refresh                                 1         // Tempo entre atualizações de dados do arduino (ms)
#define TIMEOUT_INTERVAL                        1000      // Tempo entre atualizações
#define refreshConnection                       400       // Tempo de espera para o software enviar valores de teste
#define Conf_INA                                0
#define Conf_eeprom_INA                         5
#define INA260_ADDRESS                          (0x41)    // 1000000 (A0+A1=GND) ou (0x41) 1000001 (A0=VCC , A1=GND)
#define INA260_REG_CONFIG                       (0x00)
#define INA260_CONFIG_AVGRANGE_4                (0x0200)  // Average mode 4
#define INA260_CONFIG_BVOLTAGETIME_8244US       (0x01C0)  // 8.244ms
#define INA260_CONFIG_SCURRENTTIME_8244US       (0x0038)  // 8.244ms
#define INA260_CONFIG_MODE_SANDBVOLT_CONTINUOUS (0x0007)  // Shunt Current and Bus Voltage,Continuous
#define INA260_VOLTAGE_MULTIPLIER               1         // Multiplicador definido com base do divisor de tensão (1 quando não houver)
//--------------------------------------------------------------------------------//

//----------------------------------- VARIAVEIS ----------------------------------//
bool Debug = false;                     // Flag que habilita a função debug. 
int Erro = 0;                           // Codigo de erro
unsigned long lastRequestTime = 0;      // Variavel usada para o tempo de resposta

// Estrutura de cada leitura do multimedidor
struct medidor {
  String name;
  float value;
} Medidor[AddrNum];

// Estrutura do RPM
struct rpm {
  volatile unsigned long pulseCount = 0;  // Contagem de pulsos do RPM
  volatile unsigned long lastTime = 0;    // Ultima leitura
  volatile unsigned long interval = 1000; // Intervalo de leitura em milissegundos
  float valor;
} RPM;

struct ext {
  float current;
  float voltage;
} Excitatriz;
//--------------------------------------------------------------------------------//

//------------------------------------ COMANDOS ----------------------------------//
/*
  snd      -> envio de dados concatenados 
  serial   -> Envio de dados como lista
  test     -> Função de compatibilidade
  debug    -> Ativa a função de debug, mostrando a leitura em tempo real
*/
//--------------------------------------------------------------------------------//

//------------------------------------- SETUP ------------------------------------//
void setup(){
  clearValues();                                // Limpa os registros de valores

  pinMode(ledRed, OUTPUT);                      // LED - Red
  pinMode(ledGreen, OUTPUT);                    // LED - Green
  pinMode(ledBlue, OUTPUT);                     // LED - Blue

  pinMode(sensorRPM, INPUT);                    // RPM 
  attachInterrupt(digitalPinToInterrupt(sensorRPM), countPulse, FALLING); // Ativa a interrupção no pino do sensor
  pinMode(MAX485_DE, OUTPUT);                   // Pino de controle do MAX485
  digitalWrite(MAX485_DE, 0);                   // Sentido do MAX485    
   
  Serial1.begin(9600, SERIAL_8N1);              // Serial para comunicação com o MMW02
  Serial.begin(9600);                           // Monitor serial para visualização de dados.
  Wire.begin();                                 // Inicia o Wire para obtenção dos dados do INA
  
  node.begin(MMW2_ID, Serial1);                 // Inicia o objeto node para comunicação com o MMW02
  node.preTransmission(preTransmission);        // Modbus
  node.postTransmission(postTransmission);      // Modbus
 }
// --------------------------------------------------------------------------------//

//-------------------------------------- LOOP -------------------------------------//
void loop() {
 switch(softwareReq()){
    case 1:
      ledState('t');
      testaConexao();
    break;
    case 2:
      ledState('s');
      enviaValores();
    break;
    case 4:
      ledState('s');
      mostraSerial();
    break;
    default:
     ledState('l');
     Leitura();
     excitatriz();
     readRPM();
     if (Debug)
       debug();
    break;    
  }
  delay(refresh);
 }

//------------------------------------ COMANDO -------------------------------------//
int softwareReq(){
  if (Serial.available() > 0){
    String msg = Serial.readStringUntil('\n');

    if (msg == "test"){
      return 1;  
    } else if (msg == "snd") {
      return 2; 
    } else if (msg == "serial") {
      return 4;
    } else if (msg == "debug") {
      ledState('d');
      Debug = !Debug;
      return 0;
    } else {
      ledState('e');
      return 0;
    }
  } else {
    return 0;
  }
 }
//----------------------------------------------------------------------------------//

//------------------------------------ LEITURA -------------------------------------//
void Leitura(){
  uint16_t result, result2;
  float floatValue;
  uint8_t readStatus;
  
  // Faça a leitura apenas depois de TIMEOUT_INTERVAL
  if (millis() - lastRequestTime > TIMEOUT_INTERVAL) {
    readStatus = node.readInputRegisters(2, AddrNum);
    float res = 0;
    if (readStatus == node.ku8MBSuccess) {
      for (int address = 0, pos = 1; address < AddrNum; address += 2, pos++) {
          // Obtem os dados em buffer
          result = node.getResponseBuffer(address);
          result2 = node.getResponseBuffer(address + 1);
          // É baseado em caracteristica, usa unificação inteira
          if (address >= 34 && address <= 40) {
            res = unificarInt(result, result2);
          } else {
          // É baseado em flutuante, usa unificação flutuante
            res = unificarFloat(result, result2);
          }
          // Adiciona os valores no registro
          Medidor[pos].name = getName(address);
          Medidor[pos].value = res;
      }
    } else {
      Erro = readStatus;
    }
    lastRequestTime = millis();
  }
 }
//----------------------------------------------------------------------------------//

//------------------------------------- ENVIO --------------------------------------//
void enviaValores(){
  bool hasError = false;
  String mensagem="";
  String id="";
  for (int i=0; i < AddrNum; i++){
    if(i!= 0 && i!=AddrNum){
      mensagem += ";";
    }
    mensagem = mensagem + Medidor[i].name + "=" + (String)Medidor[i].value;
  }
  Serial.println(mensagem);
 }
//----------------------------------------------------------------------------------//

//------------------------------------ MOSTRAR -------------------------------------//
void mostraSerial() {
  int ignorados = 0; 
  
  for (int i = 0; i < AddrNum; i++) {
    if (Medidor[i].name != "") {
      Serial.print("\t");
      Serial.print(Medidor[i].name);
    } else
      ignorados++;
  }
  Serial.print("\tIgnorados\n");
  for (int i = 0; i < AddrNum; i++) {
    if (Medidor[i].name != "") {
      Serial.print("\t");
      Serial.print(Medidor[i].value);
    }
  }
  Serial.print("\t");
  Serial.print(ignorados);
  Serial.print("\n");
 }
//----------------------------------------------------------------------------------//

//----------------------------------- DEBUG MODE -----------------------------------//
void debug() {
    // Dados coletados do modbus 
    Serial.print("\n\tMULTIMEDIDOR\t\n");
    if (Erro > 10) {
      Serial.print("Erro: \t");
      Serial.print(Erro);
      Serial.print("\n");
    } else {
      mostraSerial();
    }

    // RPM
    Serial.print("\n\tRPM\t\n");
    if (RPM.valor < 10) {
      Serial.print("Erro: \t");
      Serial.print("Motor parado ou lento (");
      Serial.print(RPM.valor);
      Serial.print("RPM)\n");
    } else {
      Serial.print("RPM: \t");
      Serial.print(RPM.valor);
      Serial.print("RPM\n");
    }

    // Excitatriz
    Serial.print("\n\tEXCITATRIZ\t\n");
    if (Excitatriz.voltage == 0 && Excitatriz.current == 0) {
      Serial.print("Erro: \t");
      Serial.print("Falha na leitura da excitatriz.\n");
    } else {
      Serial.print("I: \t");
      Serial.print(Excitatriz.current);
      Serial.print("A\n");
      Serial.print("V: \t");
      Serial.print(Excitatriz.voltage);
      Serial.print("V\n");
    }
    delay(200);
 }
//----------------------------------------------------------------------------------//

//-------------------------------------- RPM ---------------------------------------//
void countPulse() { // Função de interrupção para contar os pulsos
  RPM.pulseCount++;
 }

void readRPM() {
  unsigned long currentTime = millis();
  
  if (currentTime - RPM.lastTime >= RPM.interval) {
    float rpm = (float)RPM.pulseCount / (float(RPM.interval) / 60000.0); // Converte para minutos
    if (rpm <= 1.0) {
      Erro = 10;
    }  
    RPM.valor = rpm;
    // Reinicia o contador de pulsos
    RPM.pulseCount = 0;
    RPM.lastTime = currentTime;
  }
 }
//----------------------------------------------------------------------------------//

//------------------------------------ CONEXÃO -------------------------------------//
void testaConexao(){ // Função responsavel por testar conexão com o computador via serial.
    if (Serial.available() > 0) {
      String dadosRecebidos = Serial.readStringUntil('\n');
  
      // Parsing dos valores (supõe que os valores são separados por vírgula)
      int posicaoVirgula = dadosRecebidos.indexOf(',');
      if (posicaoVirgula != -1) {
       int valor1 = dadosRecebidos.substring(0, posicaoVirgula).toInt();
       int valor2 = dadosRecebidos.substring(posicaoVirgula + 1).toInt();
  
       int result = (valor1 % valor2) * (valor1 + valor2);
  
       Serial.println(result);
      }
   }
   delay(refreshConnection);
 }
//----------------------------------------------------------------------------------//

//-------------------------------- EXCITATRIZ (INA) --------------------------------//
void excitatriz() {
  Excitatriz.current = 0.001 * getcurrent(INA260_ADDRESS);
  Excitatriz.voltage = getvoltage(INA260_ADDRESS);
 }

void Ina_begin(uint8_t addr){
  uint16_t conf;
  
  if (EEPROM.read(Conf_eeprom_INA) == 0x02){
    conf = word(EEPROM.read(Conf_INA), EEPROM.read(Conf_INA+1));
  }
  
  else{
    conf =   INA260_CONFIG_AVGRANGE_4 |
             INA260_CONFIG_BVOLTAGETIME_8244US |
             INA260_CONFIG_SCURRENTTIME_8244US |
             INA260_CONFIG_MODE_SANDBVOLT_CONTINUOUS;
    }
  
  Wire.beginTransmission(addr);
  #if ARDUINO >= 100
    Wire.write(0x00);                       // Register
    Wire.write((conf >> 8) & 0xFF);       // Upper 8-bits
    Wire.write(conf & 0xFF);              // Lower 8-bits
  #else
    Wire.send(0x00);                        // Register
    Wire.send(conf >> 8);                 // Upper 8-bits
    Wire.send(conf & 0xFF);               // Lower 8-bits
  #endif
  Wire.endTransmission();
 }

float getvoltage(uint8_t addr){
  int16_t valor;
  float   tensao;
  
  Wire.beginTransmission(addr);
  #if ARDUINO >= 100
    Wire.write(0x02);                       // Register  0x02 registrador tensão
  #else
    Wire.send(0x02);                        // Register  0x02 registrador tensão
  #endif
  Wire.endTransmission();
  
  delay(1); // Max 12-bit conversion time is 586us per sample

  Wire.requestFrom(addr, (uint8_t)2);  
  #if ARDUINO >= 100
    // Shift values to create properly formed integer
    valor = ((Wire.read() << 8) | Wire.read());
  #else
    // Shift values to create properly formed integer
    valor = ((Wire.receive() << 8) | Wire.receive());
  #endif

  tensao = (valor*0.00125/0.909) * INA260_VOLTAGE_MULTIPLIER;
  if(tensao<0)tensao=0;
  delay(10);
  return tensao;
  }

float getcurrent(uint8_t addr){
  
  int16_t valor;
  float corrente;
  Wire.endTransmission();
  Wire.beginTransmission(addr);
  #if ARDUINO >= 100
    Wire.write(0x01);                       // Register  0x02 registrador tensão
  #else
    Wire.send(0x01);                        // Register  0x02 registrador tensão
  #endif
  Wire.endTransmission();
  
  delay(1); // Max 12-bit conversion time is 586us per sample

  Wire.requestFrom(addr, (uint8_t)2);  
  #if ARDUINO >= 100
    // Shift values to create properly formed integer
    valor = ((Wire.read() << 8) | Wire.read());
  #else
    // Shift values to create properly formed integer
    valor = ((Wire.receive() << 8) | Wire.receive());
  #endif

  corrente = valor* 1.25;
  if(corrente<0) corrente=0;
  delay(10);
  return corrente;
  }
//----------------------------------------------------------------------------------//

//----------------------------------- AUXILIARES -----------------------------------//
String getName(int address) {
  switch(address) {
      case 2:
        return "Vm";
      break;
      case 4:
        return "Va";
      break;
      case 6:
        return "Vb";
      break;
      case 8:
        return "Vc";
      break;
      case 10:
        return "Im";
      break;
      case 12:
        return "Ia";
      break;
      case 14:
        return "Ib";
      break;
      case 16:
        return "Ic";
      break;
      case 26:
        return "FPt";
      break;  
      case 28:
        return "FPa";
      break;
      case 30:
        return "FPb";
      break;
      case 32:
        return "FPc";
      break;
      case 36:
        return "CFPt";
      break;
      case 38:
        return "CFPa";
      break;
      case 40:
        return "CFPb";
      break;
      case 42:
        return "CFPc";
      break;
      case 66:
        return "F";
      break;
      default:
        return "";
      break;
    }
 }

void clearValues () {
  for (int add = 0; add < AddrNum; add++) {
    Medidor[add].value = 0;   
  }

  RPM.valor = 0;

  Excitatriz.voltage = 0;
  Excitatriz.current = 0;
 }
//----------------------------------------------------------------------------------//

//----------------------------------- UNIFICAÇÃO -----------------------------------//  
int unificarInt(uint16_t a, uint16_t b){
  uint32_t combinado = ((uint32_t)a << 16) | b; // Deslocamento de 16 bits de A e 
  int f;                                        // concatenação de 16 bits de B.
  memcpy(&f, &combinado, sizeof f);
  return f;
 }
float unificarFloat(uint16_t a, uint16_t b){            
  uint32_t combinado = ((uint32_t)a << 16) | b;
  float f;
  memcpy(&f, &combinado, sizeof f);
  return f;
 }
//----------------------------------------------------------------------------------//

//--------------------------------- LED INDICADOR ----------------------------------//
// 'e' - erro; 's' - envio; 'l' - leitura; 't' - teste; 'd' - debug
void ledState(char state) {
    switch(state) {
      case 'e': led(255, 0, 0); break;   // Erro    -> Vermelho
      case 's': led(0, 255, 0); break;   // Envio   -> Verde
      case 'l': led(0, 0, 255); break;   // Leitura -> Azul
      case 't': led(255, 255, 0); break; // Teste   -> Amarelo
      case 'd': led(255, 0, 255); break; // Debug   -> Roxo
    }
 }

void led(int red, int green, int blue) {
  analogWrite(ledRed, red);
  analogWrite(ledGreen, green);
  analogWrite(ledBlue, blue);
 }
//----------------------------------------------------------------------------------//

//--------------------------------- MODBUS MASTER ---------------------------------//
void   preTransmission(){
  digitalWrite(MAX485_DE, 1);     // define direção de comunicação em alto (envio)
 }
void   postTransmission(){
  digitalWrite(MAX485_DE, 0);     // define direção de comunicação em baixo (recepção)
 }
//---------------------------------------------------------------------------------//
