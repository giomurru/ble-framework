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

    public delegate void DidReceiveDataEventDelegate(byte[] data, int numOfBytes);
    public static event DidReceiveDataEventDelegate DidReceiveDataEvent;

    public delegate void ConnectionLostEventDelegate();
    public static event ConnectionLostEventDelegate ConnectionLostEvent;

    public delegate void ConnectionEstablishedEventDelegate();
    public static event ConnectionEstablishedEventDelegate ConnectionEstablishedEvent;

    public GameObject searchBleDevicesListResult;
	public GameObject searchBleDevicesButton;
    public GameObject disconnectButton;
	public GameObject infoMessage;

    

    public const int AppBuildNumber = 33;

    public const string AppBundleVersion = "1.0";

    void Awake()
    {
        searchBleDevicesButton.SetActive(false);
        disconnectButton.SetActive(false);
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
        DisconnectButtonBehaviour.DisconnectButtonEvent += HandleDisconnectButtonEvent;
		BleDeviceConnectButtonBehaviour.BleDevicesListButtonConnectEvent += HandleBleDevicesListButtonConnectEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent += HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidInitializeEvent += HandleOnBleDidInitializeEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent += HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent += HandleOnBleDidDisconnectEvent;
		BLEControllerEventHandler.OnBleDidReceiveDataEvent += HandleOnBleDidReceiveDataEvent;
	}

	void HandleOnBleDidReceiveDataEvent (byte[] data, int numOfBytes)
	{
		Debug.Log("AppControllerBehavior: HandleOnBleDidReceiveDataEvent: size: " + numOfBytes);
        DidReceiveDataEvent?.Invoke(data, numOfBytes);
	}

	void HandleOnBleDidDisconnectEvent (string errorMessage)
	{
        if (errorMessage != null)
        {
            Debug.Log("Error during disconnection: " + errorMessage);
            return;
        }
        searchBleDevicesButton.SetActive(true);
        disconnectButton.SetActive(false);
        infoMessage.GetComponent<Text>().text = "Device did disconnect.";
        
        ConnectionLostEvent?.Invoke();
    }

	void HandleOnBleDidConnectEvent (string errorMessage)
	{
        if (errorMessage != null)
        {
            Debug.Log("Error during connection: " + errorMessage);
            return;
        }
		searchBleDevicesButton.SetActive(false);
        disconnectButton.SetActive(true);
        infoMessage.GetComponent<Text>().text = "Device did connect.";
        
        ConnectionEstablishedEvent?.Invoke();
        
	}

	void HandleBleDevicesListButtonConnectEvent (int buttonIndex)
	{
		Debug.Log ("AppControllerBehavior: HandleBleDevicesListButtonConnectEvent: Calling connect peripheral at index: " + buttonIndex);
		bool success = BLEController.ConnectPeripheralAtIndex(buttonIndex);
        if (!success)
        {
            HandleOnBleDidConnectEvent("Error invoking ConnectPeripheralAtIndex " + buttonIndex);
        }
    }

	void HandleOnBleDidInitializeEvent (string errorMessage)
	{
        bool initDidSucceed = errorMessage == null;
		infoMessage.GetComponent<Text>().text = initDidSucceed ? "BleFramework initialization: SUCCESS" : ("BleFramework initialization: " + errorMessage);
		
		Debug.Log("AppControllerBehavior: HandleOnBleDidInitializeEvent: The BLE module did initialize correctly");
        searchBleDevicesButton.SetActive(initDidSucceed);
    }

	void HandleOnBleDidCompletePeripheralScanEvent (List<object> peripherals, string errorMessage)
	{
        if (errorMessage != null)
        {
            infoMessage.GetComponent<Text>().text = errorMessage;
        }
        else
        {
            infoMessage.GetComponent<Text>().text = peripherals.Count == 0 ? "No BLE devices found." : "Scan completed.";
        }
        searchBleDevicesButton.GetComponent<Button>().enabled = true;   
        DestroyActivityIndicator();
    }

	void HandleSearchBLEDevicesEvent ()
	{
        DidStartPeripheralScanEvent?.Invoke();
        ShowActivityIndicatorEvent?.Invoke();

        searchBleDevicesButton.GetComponent<Button>().enabled = false;
		
		BLEController.ScanForPeripherals();
        infoMessage.GetComponent<Text>().text = "Scanning for devices...";

    }

    void HandleDisconnectButtonEvent()
    {
        BLEController.Disconnect();
    }
	
	void OnDisable()
	{
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent -= HandleSearchBLEDevicesEvent;
        DisconnectButtonBehaviour.DisconnectButtonEvent -= HandleDisconnectButtonEvent;
        BleDeviceConnectButtonBehaviour.BleDevicesListButtonConnectEvent -= HandleBleDevicesListButtonConnectEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent -= HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidInitializeEvent -= HandleOnBleDidInitializeEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent -= HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent -= HandleOnBleDidDisconnectEvent;
		BLEControllerEventHandler.OnBleDidReceiveDataEvent -= HandleOnBleDidReceiveDataEvent;
	}
}
