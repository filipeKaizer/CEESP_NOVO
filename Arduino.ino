#include <ModbusMaster.h>
#include <EEPROM.h>
#include "Wire.h"

//----------------------------------- DEFINIÇÕES -----------------------------------//
ModbusMaster node;          // instantiate ModbusMaster object

#define MAX485_DE                               3      // Pino de sentido de transmissão
#define ledRed                                  A0     // led - Red
#define ledGreen                                A1     // led - Green
#define ledBlue                                 A2     // led - Blue
#define sensorRPM                               2      // Sensor de RPM, HIGH quando interrompido
#define MMW2_ID                                 1      // ID do dispositivo medidor MMW02
#define AddrNum                                 70     // Numero de endereços obtidos pelo MMW02
#define refresh                                 60     // Tempo entre atualizações de dados do arduino (60 -> 60ms)
#define refreshConnection                       500 // Tempo de espera para o software enviar valores de teste
#define Conf_INA                                0
#define Conf_eeprom_INA                         5
#define INA260_ADDRESS                          (0x41)    // 1000000 (A0+A1=GND) ou (0x41) 1000001 (A0=VCC , A1=GND)
#define INA260_REG_CONFIG                       (0x00)
#define INA260_CONFIG_AVGRANGE_4                (0x0200)  // Average mode 4
#define INA260_CONFIG_BVOLTAGETIME_8244US       (0x01C0)  // 8.244ms
#define INA260_CONFIG_SCURRENTTIME_8244US       (0x0038)  // 8.244ms
#define INA260_CONFIG_MODE_SANDBVOLT_CONTINUOUS (0x0007)  //Shunt Current and Bus Voltage,Continuous
//--------------------------------------------------------------------------------//

//----------------------------------- VARIAVEIS ----------------------------------//
bool Debug = false;                     // Flag que habilita a função debug. 
volatile unsigned long pulseCount = 0;  // Contagem de pulsos do RPM
volatile unsigned long lastTime = 0;    // Ultima leitura
volatile unsigned long interval = 1000; // Intervalo de leitura em milissegundos

//// Especifica o formato de cada leitura
struct Valor {
  int add;
  float valor;
};
//// Especifica um vetor da struct valor
Valor valores[AddrNum];
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
  pinMode(ledRed, OUTPUT);                      // LED - Red
  pinMode(ledGreen, OUTPUT);                    // LED - Green
  pinMode(ledBlue, OUTPUT);                     // LED - Blue

  pinMode(sensorRPM, INPUT);                    // RPM 
  attachInterrupt(digitalPinToInterrupt(sensorRPM), countPulse, FALLING); // Ativa a interrupção no pino do sensor
  pinMode(MAX485_DE, OUTPUT);                   // Pino de controle do MAX485
  digitalWrite(MAX485_DE, 0);                   // Sentido do MAX485    
   
  Serial1.begin(9600, SERIAL_8N1);              // Serial para comunicação com o MMW02
  Serial.begin(9600);                           // Monitor serial para visualização de dados.
  Wire.begin();
  
  node.begin(MMW2_ID, Serial1);                 // Inicia o objeto node para comunicação com o MMW02
  node.preTransmission(preTransmission);        // Modbus
  node.postTransmission(postTransmission);      // Modbus
 }
// --------------------------------------------------------------------------------//

//-------------------------------------- LOOP -------------------------------------//
void loop() {
  int req = softwareReq(); //Verifica requisição do software
  
  switch(req){
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
// ---------------------------------------------------------------------------------//

//----------------------------------- DEBUG MODE -----------------------------------//
void debug() {
    // Limpar tela
    Serial.write(27);
    Serial.print("[2J");
    Serial.write(27);
    Serial.print("[H");

    // Dados coletados do modbus 
    Serial.print("\n\tMULTIMEDIDOR\t\n");
    if (valores[0].valor != 0) {
      Serial.print("Erro: \t");
      Serial.print(valores[0].valor);
      Serial.print("\n");
    } else {
      mostraSerial();
    }

    // RPM
    Serial.print("\n\tRPM\t\n");
    if (valores[44].valor < 10) {
      Serial.print("Erro: \t");
      Serial.print("Motor parado ou lento (");
      Serial.print(valores[44].valor);
      Serial.print("RPM)\n");
    } else {
      Serial.print("RPM: \t");
      Serial.print(valores[findIndex(44)].valor);
      Serial.print("RPM\n");
    }

    // Excitatriz
    Serial.print("\n\tEXCITATRIZ\t\n");
    if (valores[findIndex(46)].valor == 0 && valores[findIndex(48)].valor == 0) {
      Serial.print("Erro: \t");
      Serial.print("Falha na leitura da excitatriz.\n");
    } else {
      Serial.print("I: \t");
      Serial.print(valores[findIndex(46)].valor);
      Serial.print("A\n");
      Serial.print("V: \t");
      Serial.print(valores[findIndex(48)].valor);
      Serial.print("V\n");
    }
 }
//----------------------------------------------------------------------------------//

//------------------------------------ LEITURA -------------------------------------//
void Leitura(){
  uint16_t result, result2;
  float floatValue;
  uint8_t readStatus;
  
  readStatus = node.readInputRegisters(2, AddrNum);

  if (readStatus == node.ku8MBSuccess) {
    float res = 0;

    // Adiciona o codigo de erro 0 (success)
    valores[0].add = 0;
    valores[0].valor = 0;

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
        valores[pos].add = address+2;
        valores[pos].valor = res;
    }
  } else {
    valores[0].add = 0;
    valores[0].valor = readStatus;
    
    ledState('e');
  }
 }
//----------------------------------------------------------------------------------//

//-------------------------------------- RPM ---------------------------------------//
void countPulse() { // Função de interrupção para contar os pulsos
  pulseCount++;
 }

void readRPM() {
  unsigned long currentTime = millis();
  
  if (currentTime - lastTime >= interval) {
    float rpm = (float)pulseCount / (float(interval) / 60000.0); // Converte para minutos
    
    if (rpm <= 1.0) {
      valores[0].add = 0;
      valores[0].valor = 10; 
    }  
    
    valores[AddrNum - 2].add = 44;
    valores[AddrNum - 2].valor = rpm;

    // Reinicia o contador de pulsos
    pulseCount = 0;
    lastTime = currentTime;
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

//------------------------------------- ENVIO --------------------------------------//
void enviaValores(){
  bool hasError = false;
  String mensagem="";
  String id="";
  for (int i=0; i < AddrNum; i++){
    if(i!= 0 && i!=AddrNum){
      mensagem += ";";
    }
    id = "";
    switch(valores[i].add) {
      case 0:
        if (!hasError) {
          id = "Erro";
          hasError = true;
        }
      break;
      case 2:
        id = "Vm";
      break;
      case 4:
        id = "Va";
      break;
      case 6:
        id = "Vb";
      break;
      case 8:
        id = "Vc";
      break;
      case 10:
        id = "Im";
      break;
      case 12:
        id = "Ia";
      break;
      case 14:
        id = "Ib";
      break;
      case 16:
        id = "Ic";
      break;
      case 26:
        id = "FPt";
      break;  
      case 28:
        id = "FPa";
      break;
      case 30:
        id = "FPb";
      break;
      case 32:
        id = "FPc";
      break;
      case 36:
        id = "CFPt";
      break;
      case 38:
        id = "CFPa";
      break;
      case 40:
        id = "CFPb";
      break;
      case 42:
        id = "CFPc";
      break;
      case 44:
        id = "RPM";
      break;
      case 46:
        id = "ExI";
      break;
      case 48:
        id = "ExV";
      break;
      case 66:
        id = "F";
      break;
    }
    mensagem = mensagem + id + "=" + (String)valores[i].valor;
  }
  Serial.println(mensagem);
 }
//----------------------------------------------------------------------------------//

//------------------------------------ MOSTRAR -------------------------------------//
void mostraSerial() {
  int ignorados = 0; 
  for (int add = 0; add < AddrNum/2; add++) {
    bool valid = true;
    switch(valores[add].add) {
      case 0:
        Serial.print("Erro: ");
      break;  
      case 2:
        Serial.print("Vm: ");
      break;
      case 4:
        Serial.print("Va: ");
      break;
      case 6:
        Serial.print("Vb: ");
      break;
      case 8:
        Serial.print("Vc: ");
      break;
      case 10:
        Serial.print("Im: ");
      break;
      case 12:
        Serial.print("Ia: ");
      break;
      case 14:
        Serial.print("Ib: ");
      break;
      case 16:
        Serial.print("Ic: ");
      break;
      case 26:
        Serial.print("FPt: ");
      break;  
      case 28:
        Serial.print("FPa: ");
      break;
      case 30:
        Serial.print("FPb: ");
      break;
      case 32:
        Serial.print("FPc: ");
      break;
      case 36:
        Serial.print("CFPt: ");
      break;
      case 38:
        Serial.print("CFPa: ");
      break;
      case 40:
        Serial.print("CFPb: ");
      break;
      case 42:
        Serial.print("CFPc: ");
      break;
      case 44:
        Serial.print("RPM: ");
      break;
      case 46:
        Serial.print("ExI: ");
      break;
      case 48:
        Serial.print("ExV: ");
      break;
      case 66:
        Serial.print("F: ");
      break;
      default:
        valid = false;
        ignorados++;
      break;
    }
    if (valid) {
    Serial.print("\t");
    Serial.print(valores[add].valor);
    Serial.print("\n");
    }
  }
  Serial.print("Ignorados: ");
  Serial.print("\t");
  Serial.print(ignorados);
  Serial.print("\n");
 }
//----------------------------------------------------------------------------------//

//-------------------------------- EXCITATRIZ (INA) --------------------------------//
void excitatriz() {
  valores[AddrNum - 4].add = 46;
  valores[AddrNum - 4].valor = getcurrent(INA260_ADDRESS);

  valores[AddrNum - 6].add = 48;
  valores[AddrNum - 6].valor = 1000 * getvoltage(INA260_ADDRESS);
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

  tensao = valor*0.00125/0.909;
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

//---------------------------------- BUSCA INDEX -----------------------------------//
int findIndex(int id) {
  int i = 0;
  for (; i < AddrNum; i++) {
    if (valores[i].add == id) 
      return i;
  }
  return 0;
 }
//---------------------------------------------------------------------------------//

//--------------------------------- MODBUS MASTER ---------------------------------//
void   preTransmission(){
  digitalWrite(MAX485_DE, 1);     // define direção de comunicação em alto (envio)
 }
void   postTransmission(){
  digitalWrite(MAX485_DE, 0);     // define direção de comunicação em baixo (recepção)
 }
//---------------------------------------------------------------------------------//
