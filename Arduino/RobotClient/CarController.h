#ifndef CarController_h
#define CarController_h
#include <Adafruit_MotorShield.h>

#include "Arduino.h"

class CarController
{
  public:
        CarController(int frontLeft, int rearLeft, int frontRight, int rearRight);
	void Init();
        void SetRightWheelSpeed(int wheelSpeed);
        void SetLeftWheelSpeed(int wheelSpeed);
        void SetFrontRightWheelSpeed(int wheelSpeed);
	void SetFrontLeftWheelSpeed(int wheelSpeed);
        void SetRearLeftWheelSpeed(int wheelSpeed);
        void SetRearRightWheelSpeed(int wheelSpeed);
        void Stop();
  private:
        Adafruit_MotorShield _AFMS;
	Adafruit_DCMotor *_frontLeftWheelMotor;
	Adafruit_DCMotor *_frontRightWheelMotor;
        Adafruit_DCMotor *_rearLeftWheelMotor;
        Adafruit_DCMotor *_rearRightWheelMotor;
        bool _4wd;
};

#endif
