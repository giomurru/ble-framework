#include "IRSensor.h"
#include "Arduino.h"

IRSensor::IRSensor(int pin)
{
  _pin = pin;
}

/*The measured distance from the range 0 to 400 Centimeters*/
int IRSensor::rawValue(void)
{
  int v = analogRead(_pin);
  if ( v < 80 )
  {
    v = 80;
  }
  else if ( v > 500 )
  {
    v = 500;
  }
  	
  return v;
}

byte IRSensor::byteValue(void)
{
  int v = analogRead(_pin);
  if ( v < 80 )
  {
    v = 80;
  }
  else if ( v > 500 )
  {
    v = 500;
  }
  byte db = map(v, 80, 500, 1, 254);
  return db;
}
