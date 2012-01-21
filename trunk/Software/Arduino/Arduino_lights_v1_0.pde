int LED = 13;
int TRIAC = 4;
int TEST = 10;

double counter = 0;
unsigned long timeMeas = 0;

volatile int tState = LOW;
volatile int state = LOW;
volatile int testState = HIGH;

void setup()
{
  Serial.begin(9600);
  pinMode(LED, OUTPUT);
  pinMode(TRIAC, OUTPUT);
  pinMode(TEST, OUTPUT);
  attachInterrupt(0, blink, RISING);
}

void loop()
{
  digitalWrite(LED, state);  
  digitalWrite(TRIAC,tState);

}

void blink()
{
  //digitalWrite(TEST,testState);
  testState = !testState;    
  digitalWrite(TRIAC,!tState);
  delayMicroseconds(100);
  counter++;  
  if (counter >= 1200)
  {    

    state = !state;
    counter = 0;
    Serial.println(millis()-timeMeas);
    timeMeas = millis();
  }

}



