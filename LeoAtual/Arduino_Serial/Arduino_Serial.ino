
// Example 5 - Receive with start- and end-markers combined with parsing

const byte numChars = 32;
char receivedChars[numChars];
char tempChars[numChars];        // temporary array for use when parsing

      // variables to hold the parsed data
char SALA[numChars] = {};
//char SALA;
int N_SALA;
int ESTADO = 0;
float floatFromPC = 0.0;

boolean newData = false;

int LED_SALA_DE_REUNIOES = 2;
int LED_SALA_PRINCIPAL = 3;
int LED_CORREDOR_DE_BAIAS = 4;

//============

void setup() {
    Serial.begin(9600);
    Serial.println("This demo expects 3 pieces of data - text, an integer and a floating point value");
    Serial.println("Enter data in this style <HelloWorld, 12, 24.7>  ");
    Serial.println();
}

//============

void loop() {
    recvWithStartEndMarkers();
    if (newData == true) {
        strcpy(tempChars, receivedChars);
            // this temporary copy is necessary to protect the original data
            //   because strtok() used in parseData() replaces the commas with \0
        parseData();
        showParsedData();
        correspondencia();
        ativa_rele();
        newData = false;
    }
}

//============

void recvWithStartEndMarkers() {
    static boolean recvInProgress = false;
    static byte ndx = 0;
    char startMarker = '<';
    char endMarker = '>';
    char rc;

    while (Serial.available() > 0 && newData == false) {
        rc = Serial.read();

        if (recvInProgress == true) {
            if (rc != endMarker) {
                receivedChars[ndx] = rc;
                ndx++;
                if (ndx >= numChars) {
                    ndx = numChars - 1;
                }
            }
            else {
                receivedChars[ndx] = '\0'; // terminate the string
                recvInProgress = false;
                ndx = 0;
                newData = true;
            }
        }

        else if (rc == startMarker) {
            recvInProgress = true;
        }
    }
}

//============

void parseData() {      // split the data into its parts

    char * strtokIndx; // this is used by strtok() as an index

    strtokIndx = strtok(tempChars,",");      // get the first part - the string
    strcpy(SALA, strtokIndx); // copy it to messageFromPC
 
    strtokIndx = strtok(NULL, ","); // this continues where the previous call left off
    ESTADO = atoi(strtokIndx);     // convert this part to an integer

    //strtokIndx = strtok(NULL, ",");
    //floatFromPC = atof(strtokIndx);     // convert this part to a float

}

//============

void showParsedData() {
    Serial.print("SALA: ");
    Serial.println(SALA);
    Serial.print("ESTADO: ");
    Serial.println(ESTADO);
    //Serial.print("Float ");
    //Serial.println(floatFromPC);
}

void correspondencia(){
    if ((SALA[0] == 'R')||(SALA[0] == 'r')){
      //Serial.println("ENTROU R");
      N_SALA = 0;}
    else if ((SALA[0] == 'P')||(SALA[0] == 'p')){
      //Serial.println("ENTROU P");
      N_SALA = 1;}
    else if ((SALA[0] == 'C')||(SALA[0] == 'c')){
      //Serial.println("ENTROU C");
      N_SALA = 2;}
    //Serial.println("\nSALA DA CORRESPONDENCIA");
    //Serial.print("-");
    //Serial.print(SALA);
    //Serial.println("-");
    //Serial.println("N_SALA:");
    //Serial.println(N_SALA);
    //Serial.println();
  }

void ativa_rele(){
    switch(N_SALA){
        case 0:
          Serial.println("ATIVO SALA REUNIÕES");
          estado(ESTADO);
          digitalWrite(LED_SALA_DE_REUNIOES,ESTADO);
          break;

        case 1:
          Serial.println("ATIVO SALA PRINCIPAL");
          estado(ESTADO);
          digitalWrite(LED_SALA_PRINCIPAL,ESTADO);
          break;

        case 2:
          Serial.println("ATIVO CORREDOR DE BAIAS");
          estado(ESTADO);
          digitalWrite(LED_CORREDOR_DE_BAIAS,ESTADO);
          break;

        default:
          Serial.println("CASO NÃO IDENTIFICADO");
          break;
      }
  }

void estado(int estado){
    if (estado == 0)
      Serial.println("DESLIGADO");
    else
      Serial.println("LIGADO");
  }
