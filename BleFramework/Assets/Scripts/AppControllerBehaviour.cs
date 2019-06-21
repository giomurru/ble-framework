using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using BLEFramework.Unity;

public class AppControllerBehaviour : MonoBehaviour 
{
	//other events
	public delegate void ShowActivityIndicatorEventDelegate();
	public static event ShowActivityIndicatorEventDelegate ShowActivityIndicatorEvent;
	
	public delegate void DestroyActivityIndicatorEventDelegate();
	public static event DestroyActivityIndicatorEventDelegate DestroyActivityIndicatorEvent;
	
	public delegate void DidStartPeripheralScanEventDelegate();
	public static event DidStartPeripheralScanEventDelegate DidStartPeripheralScanEvent;
	
	public GameObject searchBleDevicesListResult;
	public GameObject searchBleDevicesButton;
	public GameObject infoMessage;

    private byte[] lastCommand;

    private byte[] resetCommand = { 0xfe, 0xfe, 0xfe };
    private byte[][] listOfCommands = {
        new byte[] { 0x11, 0x00, 0xff }, // left command
        new byte[] { 0x11, 0xff, 0x00 }, // right command
        new byte[] { 0x11, 0xff, 0xff }, // forward command
        new byte[] { 0x11, 0x00, 0x00 }  // stop command
    };

    private int currentCommandIndex;

    public const int AppBuildNumber = 33;

    public const string AppBundleVersion = "1.0";

    void Awake()
    {
        currentCommandIndex = 0;
        searchBleDevicesButton.GetComponent<Button>().enabled = false;
    }
	
	void Start () 
	{
		StartCoroutine(InitializeBLEFramework());
		//bool value = BLEController.IsDeviceConnected();
		//infoMessage.GetComponent<Text>().text = "Device is connected = "+value;
	}
	
	IEnumerator InitializeBLEFramework()
	{
		//wait for BLEControllerInitializer to Awake
		while (BLEControllerInitializer.Instance == null)
		{
			yield return null;
		}
		//Init BLEFramework
		infoMessage.GetComponent<Text>().text = "Calling InitBLEFramework";
		BLEControllerInitializer.Instance.InitBLEFramework();
	}
	
	void DestroyActivityIndicator()
	{
		if (DestroyActivityIndicatorEvent!=null)
		{
			DestroyActivityIndicatorEvent();
		}
	}
	
	void OnEnable()
	{
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent += HandleSearchBLEDevicesEvent;
		BleDeviceConnectButtonBehaviour.BleDevicesListButtonConnectEvent += HandleBleDevicesListButtonConnectEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent += HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanErrorEvent += HandleOnBleDidCompletePeripheralScanErrorEvent;
		BLEControllerEventHandler.OnBleDidInitializeEvent += HandleOnBleDidInitializeEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent += HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent += HandleOnBleDidDisconnectEvent;
		BLEControllerEventHandler.OnBleDidReceiveDataEvent += HandleOnBleDidReceiveDataEvent;
	}

	void HandleOnBleDidReceiveDataEvent (byte[] data, int numOfBytes)
	{
		Debug.Log("AppControllerBehavior: HandleOnBleDidReceiveDataEvent: size: " + numOfBytes);

        if (numOfBytes == 1 && data[0] == 0xfe)
        {
            Debug.Log("Command correctly received. Can send next command.");
            if (currentCommandIndex >= listOfCommands.Length) { currentCommandIndex = 0; }
            lastCommand = listOfCommands[currentCommandIndex];
            currentCommandIndex += 1;
            //send last command in 3 seconds!
            StartCoroutine(SendLastCommandAfterDelay(3));
        } else if (numOfBytes == 1 && data[0] == 0xdf)
        {
            Debug.Log("There was an error sending the command. Retry to send last command.");
            BLEController.SendData(lastCommand);
        }
	}

    IEnumerator SendLastCommandAfterDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        BLEController.SendData(lastCommand);
    }

	void HandleOnBleDidDisconnectEvent ()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
        infoMessage.GetComponent<Text>().text = "Device did disconnect.";
    }

	void HandleOnBleDidConnectEvent ()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
        infoMessage.GetComponent<Text>().text = "Device did connect.";
        lastCommand = resetCommand;
		BLEController.SendData(resetCommand);
	}

	void HandleBleDevicesListButtonConnectEvent (int buttonIndex)
	{
		Debug.Log ("AppControllerBehavior: HandleBleDevicesListButtonConnectEvent: Calling connect peripheral at index: " + buttonIndex);
		bool result = BLEController.ConnectPeripheralAtIndex(buttonIndex);
		
		if (result)
		{
			Debug.Log ("AppControllerBehavior: HandleBleDevicesListButtonConnectEvent: Call is success");
		}
		else
		{
			Debug.Log ("AppControllerBehavior: HandleBleDevicesListButtonConnectEvent: Call did fail");
		}
	}

	void HandleOnBleDidInitializeEvent (string errorMessage)
	{
        bool initDidSucceed = errorMessage == null;
		infoMessage.GetComponent<Text>().text = initDidSucceed ? "BleFramework initialization: SUCCESS" : ("BleFramework initialization: " + errorMessage);
		
		Debug.Log("AppControllerBehavior: HandleOnBleDidInitializeEvent: The BLE module did initialize correctly");
		searchBleDevicesButton.GetComponent<Button>().enabled = initDidSucceed;
	}

	void HandleOnBleDidCompletePeripheralScanEvent (List<object> peripherals)
	{
		int i = 0;
		foreach (string s in peripherals)
		{
			Debug.Log ("AppControllerBehavior: HandleOnBleDidCompletePeripheralScanEvent: Device "+i+": " + s);
			i++;
		}
		DestroyActivityIndicator();
	}
	
	
	void HandleOnBleDidCompletePeripheralScanErrorEvent (string message)
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
        infoMessage.GetComponent<Text>().text = message;
        DestroyActivityIndicator();
		
		Debug.Log ("AppControllerBehavior: HandleOnBleDidCompletePeripheralScanErrorEvent: Error scanning for BLE peripherals: "+message);
	}

	void HandleSearchBLEDevicesEvent ()
	{
		if (DidStartPeripheralScanEvent!=null)
		{
			DidStartPeripheralScanEvent();
		}
		
		if (ShowActivityIndicatorEvent!=null)
		{
			ShowActivityIndicatorEvent();
		}
		
		searchBleDevicesButton.GetComponent<Button>().enabled = false;
		
		BLEController.ScanForPeripherals();
        infoMessage.GetComponent<Text>().text = "Scanning for devices...";

    }
	
	void OnDisable()
	{
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent -= HandleSearchBLEDevicesEvent;
		BleDeviceConnectButtonBehaviour.BleDevicesListButtonConnectEvent -= HandleBleDevicesListButtonConnectEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent -= HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanErrorEvent -= HandleOnBleDidCompletePeripheralScanErrorEvent;
		BLEControllerEventHandler.OnBleDidInitializeEvent -= HandleOnBleDidInitializeEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent -= HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent -= HandleOnBleDidDisconnectEvent;
		BLEControllerEventHandler.OnBleDidReceiveDataEvent -= HandleOnBleDidReceiveDataEvent;
	}
}
