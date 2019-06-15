#include "CarController.h"
#include "Wire.h"
#include <Adafruit_MotorShield.h>
#include "utility/Adafruit_MS_PWMServoDriver.h"

CarController carController(4, 1, 3, 2);
boolean carIsMoving;
int loopCounter;
//boolean readyForNewCommand;
  
void setup() {
  // put your setup code here, to run once:
  Serial.begin(57600);
  Serial1.begin(57600);
  
  carController.Init();
  carIsMoving = false;
  //readyForNewCommand = true;
  Serial.println("Setup");
  //randomSeed(analogRead(1));
  loopCounter = 0;
}

void loop() 
{
  Serial.print("start loop ");
  Serial.println(loopCounter, DEC);
  unsigned long loopStartTime = millis();
  /*
  If a command is already started wait for it to complete
  */
//  if (readyForNewCommand == false) 
//  {
//    Serial.println("Not ready for new command");
//  }
//  else
//  {
//    readyForNewCommand = false;
    /*
      If the connection is stable read the commands coming from the bluetooth server (iPhone or Android device)
    */
    
    if (Serial1.available() == 3) 
    {
      //Serial.println("All bytes are available.");  
      byte receivedBytes[3];
      Serial1.readBytes(receivedBytes, sizeof(byte)*3);
      
      carIsMoving = true;
      byte commandByte = receivedBytes[0];
      /*
      Possible command bytes:
      0x11 pos pos velocities
      0x1f pos neg velocities
      0xff neg neg velocities
      0xf1 neg pos velocities
      */
      int frontLeft = (int) receivedBytes[1];
      int frontRight = (int)receivedBytes[2]; 
      
      boolean speedValueReceived = false;
      
      if (commandByte == 0xfe)
      {
         if (receivedBytes[1] == 0xfe && receivedBytes[2]== 0xfe)
         {
            //RESET
            Serial.println("I received the reset command");
             
            carController.SetFrontRightWheelSpeed(100);
            delay(3000);
            carController.SetFrontRightWheelSpeed(0);
            carController.SetFrontLeftWheelSpeed(100);
            delay(3000);
            carController.SetFrontLeftWheelSpeed(0);
            carController.SetRearRightWheelSpeed(100);
            delay(3000);
            carController.SetRearRightWheelSpeed(0);
            carController.SetRearLeftWheelSpeed(100);
            delay(3000);
            carController.SetRearLeftWheelSpeed(0);
         }
      }
      else if (commandByte == 0x11)
      {
        speedValueReceived = true;
      }
      else if (commandByte == 0x1f)
      {
        frontRight = -frontRight;
        speedValueReceived = true;
      }
      else if (commandByte == 0xff)
      {
        frontLeft = -frontLeft;
        frontRight = -frontRight;
        speedValueReceived = true;
      }
      else if (commandByte = 0xf1)
      {
        frontLeft = -frontLeft;
        speedValueReceived = true;
      }
      
      int rearLeft = frontLeft;
      int rearRight = frontRight;
      
      if (speedValueReceived)
      {
        carController.SetFrontRightWheelSpeed(frontRight);
        carController.SetFrontLeftWheelSpeed(frontLeft);
        carController.SetRearRightWheelSpeed(rearRight);
        carController.SetRearLeftWheelSpeed(rearLeft);

        Serial.print(" Robot speeds values are: FR: ");
        Serial.print(frontRight);
        Serial.print(", FL: ");
        Serial.print(frontLeft);
        Serial.print(", RR: ");
        Serial.print(rearRight);
        Serial.print(", RL: ");
        Serial.println(rearLeft);
      }
      else
      {
        Serial.println("I received other 3 bytes that were not a velocity command");
        Serial.println(receivedBytes[0]);
        Serial.println(receivedBytes[1]);
        Serial.println(receivedBytes[2]);
      }
      
      /*
      After I received the car velocities and I set them on the robot,
      it's time to send the sensor data to the server in order to decide
      which are the correct wheels velocities
      */
      byte ackData[1] = {0xfe};
      Serial1.write(ackData, 1);
      unsigned long loopTime = millis() - loopStartTime;
      
      if (loopTime < 500)
      {
        delay(500 - loopTime);
      }
    }
    else
    {
      int numOfAvailableBytes = Serial1.available();
      if (numOfAvailableBytes == 0) {
        Serial.println("There are no bytes available");
      }
      else if (numOfAvailableBytes > 0 && numOfAvailableBytes < 4)
      {
        Serial.print("There are only ");
        Serial.print(Serial1.available());
        Serial.println(" bytes in the serial buffer");
      }
      else if (numOfAvailableBytes > 4)
      {
        Serial.println("ERROR: Too much Serial Data in the buffer!!! ");
      }
    }
    
//    readyForNewCommand = true;
//  }

  Serial.print("end   loop ");
  Serial.println(loopCounter, DEC);
  loopCounter++;
}
