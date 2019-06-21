/*	SparkFunRHT03.cpp
	Jim Lindblom <jim@sparkfun.com>
	August 31, 2015
    
    Ported to Arduino by Shawn Hymel
    October 28, 2016
	https://github.com/sparkfun/SparkFun_RHT03_Particle_Library
	
	This is the main header file for the SparkFunRHT03 Particle
	library.
	
	Development environment specifics:
	Arduino IDE v1.6.5
	Distributed as-is; no warranty is given. 
*/

#include "Arduino.h"

#define RHT_READ_INTERVAL_MS 1000

#ifndef RHT03_h
#define RHT03_h

class RHT03
{
public:
    RHT03();
    void begin(int dataPin);
    int update();
    float tempC();
    float tempF();
    float humidity();

private:
    int _dataPin;
    int _humidity;
    int _temperature;
    
    enum dataByteLocations { HUMIDITY_H, HUMIDITY_L, TEMP_H, TEMP_L, CHECKSUM };
    
    bool checksum(byte check, byte * data, unsigned int datalen);
    int errorExit(int code);
    bool waitForRHT(int pinState, unsigned int timeout = 1000);
};

#endif