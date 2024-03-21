////////////////////////////////   Modbus Master   ////////////////////////////////
#include <ModbusMaster.h>
ModbusMaster node;          // instantiate ModbusMaster object

#define MAX485_DE      3    // Pino de sentido de transmissão
#define ledRed         A0   // led - Red
#define ledGreen       A1   // led - Green
#define ledBlue        A2   // led - Blue
#define sensorRPM      2    // Sensor de RPM, HIGH quando interrompido
#define sensorSignal   10   // Sensor de RPM paralelo ao 2

#define MMW2_ID        1    // ID do dispositivo medidor MMW02
#define AddrNum        82   // Numero de endereços obtidos pelo MMW02
#define refresh        60   // Tempo entre atualizações de dados do arduino (60 -> 60ms)
#define tempoLimite    200  // Tempo máximo de espera do envio de dados pelo MW02 (100 -> 0,1seg)
#define refreshConnection 500 // Tempo de espera para o software enviar valores de teste


//---------------------------------------------------------------------------------
///////////////////////////////   Variaveis   /////////////////////////////////////
bool ConEst = false;        // Flag que controla o envio de dados para o computador. Se não houver um, o mesmo não envia
bool Debug = false;         // Flag que habilita a função debug. 

// Variáveis para o cálculo das RPM
volatile unsigned long pulseCount = 0;
volatile unsigned long lastTime = 0;
volatile unsigned long interval = 1000; // Intervalo de leitura em milissegundos

//// Especifica o formato de cada leitura
struct Valor {
  int add;
  float valor;
};

//// Especifica um vetor da struct valor
Valor valores[AddrNum];


///////////////////////// COMANDOS //////////////////////////////////// 
/*
  snd      -> envio de dados concatenados 
  serial   -> Envio de dados como lista
  test     -> Função de compatibilidade
  debug    -> Ativa a função de debug, mostrando leitura em tempo real
*/

//////////////////////////////// SETUP ////////////////////////////////
void setup(){
  pinMode(MAX485_DE, OUTPUT);                   // Pino de controle do MAX485
  pinMode(ledRed, OUTPUT);                      // LED - Red
  pinMode(ledGreen, OUTPUT);                    // LED - Green
  pinMode(ledBlue, OUTPUT);                     // LED - Blue

  pinMode(sensorRPM, INPUT);                // RPM 
  attachInterrupt(digitalPinToInterrupt(sensorRPM), countPulse, FALLING); // Ativa a interrupção no pino do sensor
  
  digitalWrite(MAX485_DE, 0);                   // Sentido do MAX485     
   
  Serial.begin(9600);                           // Monitor serial para visualização de dados.
  Serial1.begin(9600, SERIAL_8N1);              // Serial para comunicação com o MMW02
  
  node.begin(MMW2_ID, Serial1);                 // Inicia o objeto node para comunicação com o MMW02
  node.preTransmission(preTransmission);
  node.postTransmission(postTransmission);
}
// ----------------------------------------------------------------------------------

//////////////////////////////// LOOP ///////////////////////////////////////////////
void loop()
{
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
     readRPM();
    break;    
  }

  delay(refresh);
}
// ------------------------------------------------------------------------------------

//////////////////////// LEITURA/ESCRITA //////////////////////////////////////////////
void Escrita(uint32_t address, uint8_t value) {
    preTransmission();
    node.writeSingleRegister(address,value);
    postTransmission();
}

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

        //Função Debug
        if (Debug) {
          ledState('d');
          Serial.print("Valor: ");
          Serial.println(res);
        }
    }
  } else {
    if (Debug) {
      ledState('d');
      Serial.print("Erro: ");
      Serial.println(readStatus);
    }

    valores[0].add = 0;
    valores[0].valor = readStatus;
    ledState('e');
  }
}
//--------------------------------------------------------------------------------------

//////////////////////////////// RPM ///////////////////////////////////////////////////
// Função de interrupção para contar os pulsos
void countPulse() {
  pulseCount++;
}

void readRPM() {
  unsigned long currentTime = millis();
  
  // Calcula as RPM a cada intervalo de tempo
  if (currentTime - lastTime >= interval) {
    // Calcula as RPM
    float rpm = (float)pulseCount / (float(interval) / 60000.0); // Converte para minutos

    // Exibe as RPM no Monitor Serial
    if (Debug) {
      Serial.print("RPM: ");
      Serial.println(rpm);
    }
    
    valores[AddrNum - 1].add = 44;
    valores[AddrNum - 1].valor = rpm;

    // Reinicia o contador de pulsos
    pulseCount = 0;
    lastTime = currentTime;
  }
}


//////////////////////////////// MODBUS MASTER /////////////////////////////////////////
void   preTransmission(){
  digitalWrite(MAX485_DE, 1);     // define direção de comunicação em alto (envio)
}
void   postTransmission(){
  digitalWrite(MAX485_DE, 0);     // define direção de comunicação em baixo (recepção)
}

//-------------------------------------------------------------------------------------

//////////////////////////////////// CONEXÃO //////////////////////////////////////////

// Função responsavel por testar conexão com o computador via serial.
void testaConexao(){
    if (Serial.available() > 0) {
      String dadosRecebidos = Serial.readStringUntil('\n');
  
      // Parsing dos valores (supõe que os valores são separados por vírgula)
      int posicaoVirgula = dadosRecebidos.indexOf(',');
      if (posicaoVirgula != -1) {
       int valor1 = dadosRecebidos.substring(0, posicaoVirgula).toInt();
       int valor2 = dadosRecebidos.substring(posicaoVirgula + 1).toInt();
  
       int result = (valor1 % valor2) * (valor1+valor2);
  
       Serial.println(result);
      }
   }
   delay(refreshConnection);
}



//// FUNÇÃO RESPONSÁVEL POR OBTER OS COMANDOS DO COMPUTADOR
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

// FUNÇÃO RESPONSÁVEL PELA CONCATENAÇÃO E ENVIO DE DADOS AO COMPUTADOR //////
// Envia valores para o software
void enviaValores(){
  String mensagem="";
  String id="";
  for (int i=0; i<AddrNum/2; i++){
    if(i!= 0 && i!=AddrNum){
      mensagem += ";";
    }
    id = "";

    switch(valores[i].add) {
      case 0:
        id = "Erro";
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
      case 66:
        id = "F";
      break;
    }
    
    mensagem = mensagem + id + "=" + (String)valores[i].valor;
  }
  Serial.println(mensagem);
}

// Mostra valores com legenda no monitor serial
void mostraSerial() {
  for (int add = 0; add < AddrNum/2; add++) {
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
      case 66:
        Serial.print("F: ");
      break;
    }
    Serial.print("\t");
    Serial.print(valores[add].valor);
    Serial.print("\n");
  }
}
//-------------------------------------------------------------------------

////////////////// LED INDICADOR //////////////////////////////////////////
// 'e' - erro; 's' - envio; 'l' - leitura; 't' - teste; 'd' - debug
void ledState(char state) {
    switch(state) {
      case 'e': led(255, 0, 0); break;
      case 's': led(0, 255, 0); break;
      case 'l': led(0, 0, 255); break;
      case 't': led(255, 255, 0); break;
      case 'd': led(255, 0, 255); break;
    }
}

void led(int red, int green, int blue) {
  analogWrite(ledRed, red);
  analogWrite(ledGreen, green);
  analogWrite(ledBlue, blue);
}

/////////////////// FUNÇÕES RESPONSÁVEIS PELA UNIFICAÇÃO //////////////////  
int unificarInt(uint16_t a, uint16_t b){
  uint32_t combinado = ((uint32_t)a << 16) | b;
  int f;
  memcpy(&f, &combinado, sizeof f);
  return f;
}

float unificarFloat(uint16_t a, uint16_t b){            
  uint32_t combinado = ((uint32_t)a << 16) | b;
  float f;
  memcpy(&f, &combinado, sizeof f);
  return f;
}
