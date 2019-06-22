using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BLEFramework.Unity;
using System.Linq;

public class MeteoStationBehaviour : MonoBehaviour
{
    public GameObject meteoStationBoard;

    private byte[] lastCommand;

    private byte[] resetCommand = { 0xfe, 0xfe, 0xfe };
    private byte[] stopCommand = { 0x01, 0x02, 0x03 };
    private byte[] updateDataCommand = { 0x00, 0x00, 0x00 };

    private readonly byte[] stopAck = { 0x01, 0x01, 0x01 };

    private readonly byte[] resetError = { 0x33, 0x33, 0x33 };
    private readonly byte[] updateDataError = { 0x44, 0x44, 0x44};
    private readonly byte[] unknownError = { 0x77, 0x77, 0x77 };

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
        meteoStationBoard.GetComponent<Text>().text = "Reading sensors...";
    }

    IEnumerator SendUpdateDataCommand(int delay)
    {
        yield return new WaitForSeconds(delay);
        BLEController.SendData(updateDataCommand);
    }

    void HandleDidReceiveDataEvent(byte[] data, int numOfBytes)
    {
        Debug.Log("AppControllerBehavior: HandleOnBleDidReceiveDataEvent: size: " + numOfBytes);

        if (numOfBytes == 3)
        {
            if (data.SequenceEqual(resetError))
            {
                Debug.Log("Reset Error");
                // error resetting
                // try sending reset again
                //BLEController.SendData(resetCommand);
            }
            else if (data.SequenceEqual(updateDataError))
            {
                Debug.Log("Update Data Error");
                // error sending update data command
                // try sending update data again
                //BLEController.SendData(updateDataCommand);
            }
            else if (data.SequenceEqual(unknownError))
            {
                Debug.Log("Unknown Error");
                // unknown error
                // try sending reset signal
                //BLEController.SendData(resetCommand);
            }
            else // The data is valid
            {
                if (data.SequenceEqual(stopAck))
                {
                    Debug.Log("Stop Ack");
                    // error sending stop
                    // try sending stop again
                    isTrasmissionActive = false;
                }
                else
                {
                    Debug.Log("Updating temperature and humidity values");
                    byte humidity = data[0];
                    byte temperatureSign = data[1];
                    byte temperatureAbsValue = data[2];
                    int percentHumidity = humidity;
                    int temperature = temperatureSign == 0xff ? -temperatureAbsValue : temperatureAbsValue;
                    string text = "Humidity: " + percentHumidity + "%\nTemperature: " + temperature + " C";
                    Debug.Log(text);
                    meteoStationBoard.GetComponent<Text>().text = text;
                }
                
                if (isTrasmissionActive)
                {
                    transmissionCoroutine = StartCoroutine(SendUpdateDataCommand(10));
                }
            }
            //Debug.Log("Command correctly received. Can send next command.");
            //if (currentCommandIndex >= listOfCommands.Length) { currentCommandIndex = 0; }
            //lastCommand = listOfCommands[currentCommandIndex];
            //currentCommandIndex += 1;
            ////send last command in 3 seconds!
            
        }
    }

    void HandleConnectionEstablishedEvent()
    {
        isTrasmissionActive = true;
        BLEController.SendData(resetCommand);
    }

    void HandleConnectionLostEvent()
    {
        isTrasmissionActive = false;
        StopCoroutine(transmissionCoroutine);
    }

    public void StopMeteoUpdates()
    {
        BLEController.SendData(stopCommand);
    }

}
