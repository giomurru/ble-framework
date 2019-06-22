/*	RHT03-Example-Serial.cpp
	Jim Lindblom <jim@sparkfun.com>
	August 31, 2015
    
    Ported to Arduino by Shawn Hymel
    October 28, 2016
	https://github.com/sparkfun/SparkFun_RHT03_Arduino_Library
	
	This a simple example sketch for the SparkFunRHT03 Ardiuno
	library.
	
	Looking at the front (grated side) of the RHT03, the pinout is as follows:
	 1     2        3       4
	VCC  DATA  No-Connect  GND
	
	Connect the data pin to Arduino pin D4. Power the RHT03 off the 3.3V bus.
	
	A 10k pullup resistor can be added to the data pin, though it seems to
	work without it.
	
    Development environment specifics:
	Arduino IDE v1.6.5
	Distributed as-is; no warranty is given.  
*/

// Include the library:
#include "SparkFun_RHT03.h"
/////////////////////
// Pin Definitions //
/////////////////////
const int RHT03_DATA_PIN = 40; // RHT03 data pin

///////////////////////////
// RHT03 Object Creation //
///////////////////////////
RHT03 rht; // This creates a RTH03 object, which we'll use to interact with the sensor

bool ble_active = false;
float latestHumidity = 0.0;
float latestTempC = 0.0;

void setup()
{
	Serial.begin(9600); // Serial is used to print sensor readings.
	Serial1.begin(57600);
	// Call rht.begin() to initialize the sensor and our data pin
	rht.begin(RHT03_DATA_PIN);
	
}

void loop()
{
  int updateRet = rht.update();

  // If successful, the update() function will return 1.
  // If update fails, it will return a value <0
  if (updateRet == 1)
  {
    Serial.println("reading temperature and humidity levels");
    // The humidity(), tempC(), and tempF() functions can be called -- after 
    // a successful update() -- to get the last humidity and temperature
    
    // value 
    latestHumidity = rht.humidity();
    latestTempC = rht.tempC();
    Serial.println("Humidity: " + String(latestHumidity, 1) + " %");
    Serial.println("Temp (C): " + String(latestTempC, 1) + " deg C");

    if(Serial1.available()!=3) {
      delay(1000);
      return;
    }
    Serial.println("command received");
    byte receivedBytes[3];
    Serial1.readBytes(receivedBytes, sizeof(byte)*3);
  
    byte meteoData[3];
    if (receivedBytes[0] == 0xfe && receivedBytes[1] == 0xfe && receivedBytes[2]== 0xfe)
    {
      Serial.println("start data transmission");
      // Start Data transmission
      ble_active = true;
      //Note: this is a default value for the meteoData to be transmitted if the rht.update() fails
      //Real meteoData will be set later.
      meteoData[0] = 0x33;
      meteoData[1] = 0x33;
      meteoData[2] = 0x33;
    } else if (receivedBytes[0] == 0x01 && receivedBytes[1] == 0x02 && receivedBytes[2]== 0x03) {
      // Stop Data transmission
      Serial.println("stop data transmission");
      ble_active = false;
      meteoData[0] = 0x01;
      meteoData[1] = 0x01;
      meteoData[2] = 0x01;
    } else if (receivedBytes[0] == 0x00 && receivedBytes[1] == 0x00 && receivedBytes[3] == 0x00) {
      Serial.println("update data");
      // Ack packet
      // Continue transmission
      //Note: this is a default value for the meteoData to be transmitted if the rht.update() fails
      //Real meteoData will be set later.
      meteoData[0] = 0x44;
      meteoData[1] = 0x44;
      meteoData[2] = 0x44;
    } else {
      Serial.println("something is wrong with data");
      //Something is wrong.
      meteoData[0] = 0x77;
      meteoData[1] = 0x77;
      meteoData[2] = 0x77;
      //please send me Start signal again
      ble_active = false;
    }
  
    if (ble_active) {
      meteoData[0] = (byte) latestHumidity + 0.5;
      meteoData[1] = (byte) latestTempC > 0 ? 0x00 : 0xff;
      meteoData[2] = (byte) (abs(latestTempC) + 0.5);
    } else {
      Serial.println("Error updating temperature and humidity levels");
    }
  
    Serial1.write(meteoData, 3);
  }
  else
  { 
    Serial.println("no sensor read. wait for " + String(RHT_READ_INTERVAL_MS) + " ms");
    // If the update failed, try delaying for RHT_READ_INTERVAL_MS ms before
    // trying again.
    delay(RHT_READ_INTERVAL_MS);
  }
  delay(1000);
}
