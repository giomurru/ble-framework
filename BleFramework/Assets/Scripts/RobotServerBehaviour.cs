using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BLEFramework.Unity;

public class RobotServerBehaviour : MonoBehaviour
{
    private byte[] lastCommand;

    private byte[] resetCommand = { 0xfe, 0xfe, 0xfe };
    private byte[][] listOfCommands = {
        new byte[] { 0x11, 0x00, 0xff }, // left command
        new byte[] { 0x11, 0xff, 0x00 }, // right command
        new byte[] { 0x11, 0xff, 0xff }, // forward command
        new byte[] { 0x11, 0x00, 0x00 }  // stop command
    };

    private int currentCommandIndex;

    private bool isTrasmissionActive;

    private Coroutine transmissionCoroutine;

    private void OnEnable()
    {
        AppControllerBehaviour.DidReceiveDataEvent += HandleDidReceiveDataEvent;
        AppControllerBehaviour.ConnectionEstablishedEvent += HandleConnectionEstablishedEvent;
        AppControllerBehaviour.ConnectionLostEvent += HandleConnectionLostEvent;
    }

    private void OnDisable()
    {
        AppControllerBehaviour.DidReceiveDataEvent -= HandleDidReceiveDataEvent;
        AppControllerBehaviour.ConnectionEstablishedEvent -= HandleConnectionEstablishedEvent;
        AppControllerBehaviour.ConnectionLostEvent -= HandleConnectionLostEvent;
    }
    private void Awake()
    {
        currentCommandIndex = 0;
    }

    IEnumerator SendLastCommandAfterDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        BLEController.SendData(lastCommand);
    }

    void HandleDidReceiveDataEvent(byte[] data, int numOfBytes)
    {
        Debug.Log("AppControllerBehavior: HandleOnBleDidReceiveDataEvent: size: " + numOfBytes);

        if (numOfBytes == 1 && data[0] == 0xfe)
        {
            Debug.Log("Command correctly received. Can send next command.");
            if (currentCommandIndex >= listOfCommands.Length) { currentCommandIndex = 0; }
            lastCommand = listOfCommands[currentCommandIndex];
            currentCommandIndex += 1;
            //send last command in 3 seconds!
            if (isTrasmissionActive)
            {
                transmissionCoroutine = StartCoroutine(SendLastCommandAfterDelay(3));
            }
        }
        else if (isTrasmissionActive && numOfBytes == 1 && data[0] == 0xdf)
        {
            Debug.Log("There was an error sending the command. Retry to send last command.");
            BLEController.SendData(lastCommand);
        }
    }

    void HandleConnectionEstablishedEvent()
    {
        lastCommand = resetCommand;
        isTrasmissionActive = true;
        BLEController.SendData(lastCommand);
    }

    void HandleConnectionLostEvent()
    {
        isTrasmissionActive = false;
        StopCoroutine(transmissionCoroutine);
    }

    public void StopRobot()
    {
        isTrasmissionActive = false;
        BLEController.SendData(resetCommand);
    }
    
}
