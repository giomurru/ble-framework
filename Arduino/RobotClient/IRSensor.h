#ifndef IRSensor_h
#define IRSensor_h

#include "Arduino.h"

class IRSensor
{
  public:
    IRSensor(int pin);
    int rawValue(void);
    byte byteValue(void);
  private:
    int _pin;//pin number of Arduino that is connected with SIG pin of IR sensor.
    int value;//the raw value of the sensor
};

#endif
