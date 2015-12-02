void setup() {
  
  Serial.begin(115200);  //Initial the serial.
  pinMode(A0, INPUT);    //1st analogue input from filter-rectifier pair.
  pinMode(2, OUTPUT);    //Serial Flag LED. If it is off then the controller isn't working.
  pinMode(3, OUTPUT);    //Working Flag LED. If it is off then the controller isn't working.
  
}

void loop() {
  int neural_0=0;        //Filter-rectifier analog value.
  byte ready_byte=B00000000;      //Stores Serial.read() value. Looks for 0xA1.
  byte data_send_HB=B00000000; //Data to be sent by serial by buffer. High Byte
  byte data_send_LB=B00000000; //Data to be sent by serial by buffer. Low Byte
  while(1){
  while (!Serial.available()) {  //While the serial is not available, Serial Flag LED blinks at 1.5 Hz until available.
     digitalWrite(2,HIGH);
     delay(667);
     digitalWrite(2,LOW); 
  }
  
  digitalWrite(2,HIGH);

  digitalWrite(3,LOW); //Working Flag LED goes low to indicate read is happening
  neural_0 = analogRead(A0); //Reads filter-rectifier analog value
  data_send_HB=byte(neural_0); //Setting the data_send buffer
  data_send_LB=byte(neural_0>>8);
  digitalWrite(3,HIGH); //Working Flag LED goes high to indicate read has happened
  
  
  while (ready_byte!=B10000001){ //B10000001=0xA1
      digitalWrite(3,LOW); //Working Flag LED goes low to indicate send is happening
      delay(20);
      ready_byte=Serial.read(); 
      digitalWrite(3,HIGH); //Working Flag LED goes high to indicate send has happening
  }
  Serial.write(data_send_HB);
  delay(20);
  Serial.write(data_send_LB);
  digitalWrite(3,LOW); //Working Flag LED goes low to indicate send has happened
  delay(40);
  digitalWrite(3,HIGH); //Working Flag LED goes low to indicate send has happened
  
  delay(100);
 
}
}
