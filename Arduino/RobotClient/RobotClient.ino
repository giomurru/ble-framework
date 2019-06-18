#include "CarController.h"
#include "Wire.h"
#include <Adafruit_MotorShield.h>
#include "utility/Adafruit_MS_PWMServoDriver.h"

CarController carController(4, 1, 3, 2);
int loopCounter;
//boolean readyForNewCommand;
  
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  Serial1.begin(57600);
  
  carController.Init();
  //readyForNewCommand = true;
  Serial.println("Ready");
  //randomSeed(analogRead(1));
  loopCounter = 0;
}

bool isValidVelocityCommand(int command) {
  return command >= 0 && command <= 0xff;
}

void loop() 
{
  Serial.print("start loop ");
  Serial.println(loopCounter, DEC);
  Serial.println("waiting for a command");
  while(Serial1.available()!=3);
  Serial.println("command received");
  byte receivedBytes[3];
  Serial1.readBytes(receivedBytes, sizeof(byte)*3);
  byte commandByte = receivedBytes[0];
  /*
  Possible command bytes:
  0x11 pos pos velocities
  0x1f pos neg velocities
  0xff neg neg velocities
  0xf1 neg pos velocities
  */
  int leftWheelVelocity = (int) receivedBytes[1];
  int rightWheelVelocity = (int)receivedBytes[2]; 

  boolean speedValueReceived = false;
  boolean resetReceived = false;
  // Check is the received speed is valid
  if (isValidVelocityCommand(leftWheelVelocity) && isValidVelocityCommand(rightWheelVelocity)) {
    //action based on the command
    if (commandByte == 0xfe)
    {
       if (receivedBytes[1] == 0xfe && receivedBytes[2]== 0xfe)
       {
          //RESET
          Serial.println("I received the reset command. Stop the Car.");
          carController.Stop();
          resetReceived = true;
       } else {
          Serial.println("Unknown command");
       }
    }
    else if (commandByte == 0x11) // left and right are positive velocities
    {
      speedValueReceived = true;
    }
    else if (commandByte == 0x1f) //left is positive velocity and right is negative velocity
    {
      rightWheelVelocity = -rightWheelVelocity;
      speedValueReceived = true;
    }
    else if (commandByte == 0xff) // left and right are negative velocities
    {
      leftWheelVelocity = -leftWheelVelocity;
      rightWheelVelocity = -rightWheelVelocity;
      speedValueReceived = true;
    }
    else if (commandByte = 0xf1) // left is negative velocity and right is positive velocity
    {
      leftWheelVelocity = -leftWheelVelocity;
      speedValueReceived = true;
    }
  }
  

  byte ackData[1];
  if (speedValueReceived)
  {
    carController.SetRightWheelSpeed(rightWheelVelocity);
    carController.SetLeftWheelSpeed(leftWheelVelocity);
    ackData[0] = 0xfe; // signal success
  } else if (resetReceived) {
    Serial.print("prepare ackData packet");
    ackData[0] = 0xfe; // signal success
  } else {
    ackData[0] = 0xdf; // signal an error (server should retry sending the data)
  }

  Serial.print("send ackData packet");
  Serial1.write(ackData, 1);
  
  Serial.print("end loop ");
  Serial.println(loopCounter, DEC);
  loopCounter++;
}
