#include "CarController.h"
#include "Arduino.h"

CarController::CarController(int frontLeft, int rearLeft, int frontRight, int rearRight)
{
	// Create the motor shield object with the default I2C address
	_AFMS = Adafruit_MotorShield();

        _4wd = true;
	// Or, create it with a different I2C address (say for stacking)
	// Adafruit_MotorShield AFMS = Adafruit_MotorShield(0x61);

	// Select which 'port' M1, M2, M3 or M4. In this case, M1
	_frontLeftWheelMotor = _AFMS.getMotor(frontLeft);
	// You can also make another motor on port M2
	_frontRightWheelMotor = _AFMS.getMotor(frontRight);

        _rearLeftWheelMotor = _AFMS.getMotor(rearLeft);
        _rearRightWheelMotor = _AFMS.getMotor(rearRight); 
}

void CarController::Init()
{
	_AFMS.begin();  // create with the default frequency 1.6KHz
}

void CarController::SetRearRightWheelSpeed(int wheelSpeed)
{
	//do something
    if (wheelSpeed > 0)
    {
      _rearRightWheelMotor->run(FORWARD);
      _rearRightWheelMotor->setSpeed(wheelSpeed);

    }
    else if (wheelSpeed < 0)
    {
      _rearRightWheelMotor->run(BACKWARD);
      _rearRightWheelMotor->setSpeed(-wheelSpeed);
    }
    else
    {
      _rearRightWheelMotor->run(RELEASE);
    }
}

void CarController::SetRearLeftWheelSpeed(int wheelSpeed)
{
	//do something
    if (wheelSpeed > 0)
    {
      _rearLeftWheelMotor->run(FORWARD);
      _rearLeftWheelMotor->setSpeed(wheelSpeed);

    }
    else if (wheelSpeed < 0)
    {
      _rearLeftWheelMotor->run(BACKWARD);
      _rearLeftWheelMotor->setSpeed(-wheelSpeed);
    }
    else
    {
      _rearLeftWheelMotor->run(RELEASE);
    }
}

void CarController::SetFrontLeftWheelSpeed(int wheelSpeed)
{
	//do something
    if (wheelSpeed > 0)
    {
      _frontLeftWheelMotor->run(FORWARD);
      _frontLeftWheelMotor->setSpeed(wheelSpeed);

    }
    else if (wheelSpeed < 0)
    {
      _frontLeftWheelMotor->run(BACKWARD);
      _frontLeftWheelMotor->setSpeed(-wheelSpeed);
    }
    else
    {
      _frontLeftWheelMotor->run(RELEASE);
    }
}


void CarController::SetFrontRightWheelSpeed(int wheelSpeed)
{
	//do something
    if (wheelSpeed > 0)
    {
      _frontRightWheelMotor->run(FORWARD);
      _frontRightWheelMotor->setSpeed(wheelSpeed);

    }
    else if (wheelSpeed < 0)
    {
      _frontRightWheelMotor->run(BACKWARD);
      _frontRightWheelMotor->setSpeed(-wheelSpeed);
    }
    else
    {
      _frontRightWheelMotor->run(RELEASE);
    }
}

void CarController::SetRightWheelSpeed(int wheelSpeed)
{
   SetFrontRightWheelSpeed(wheelSpeed);
   SetRearRightWheelSpeed(wheelSpeed);
}

void CarController::SetLeftWheelSpeed(int wheelSpeed)
{
   SetFrontLeftWheelSpeed(wheelSpeed);
   SetRearLeftWheelSpeed(wheelSpeed);
}
void CarController::Stop()
{
  _frontLeftWheelMotor->run(RELEASE);
  _frontRightWheelMotor->run(RELEASE);
  _rearLeftWheelMotor->run(RELEASE);
  _rearRightWheelMotor->run(RELEASE);
}
