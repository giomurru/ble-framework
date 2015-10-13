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
	
	void Awake()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = false;
	}
	
	void Start () 
	{
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
		BLEControllerEventHandler.OnBleDidInitializeErrorEvent += HandleOnBleDidInitializeErrorEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent += HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent += HandleOnBleDidDisconnectEvent;
	}

	void HandleOnBleDidDisconnectEvent ()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
	}

	void HandleOnBleDidConnectEvent ()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
	}

	void HandleBleDevicesListButtonConnectEvent (uint buttonIndex)
	{
		Debug.Log ("AppControllerBehavior: HandleBleDevicesListButtonConnectEvent: Calling connect peripheral at index");
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

	void HandleOnBleDidInitializeErrorEvent (string errorMessage)
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = false;
		Debug.Log ("AppControllerBehavior: HandleOnBleDidInitializeErrorEvent: Error Message: " + errorMessage);
	}

	void HandleOnBleDidInitializeEvent ()
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
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
	
	
	void HandleOnBleDidCompletePeripheralScanErrorEvent (string errorCode)
	{
		searchBleDevicesButton.GetComponent<Button>().enabled = true;
		
		DestroyActivityIndicator();
		
		Debug.Log ("AppControllerBehavior: HandleOnBleDidCompletePeripheralScanErrorEvent: Error scanning for BLE peripherals: "+errorCode);
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
		
	}
	
	void OnDisable()
	{
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent -= HandleSearchBLEDevicesEvent;
		BleDeviceConnectButtonBehaviour.BleDevicesListButtonConnectEvent -= HandleBleDevicesListButtonConnectEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent -= HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanErrorEvent -= HandleOnBleDidCompletePeripheralScanErrorEvent;
		BLEControllerEventHandler.OnBleDidInitializeEvent -= HandleOnBleDidInitializeEvent;
		BLEControllerEventHandler.OnBleDidInitializeErrorEvent -= HandleOnBleDidInitializeErrorEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent -= HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent -= HandleOnBleDidDisconnectEvent;
	}
}
