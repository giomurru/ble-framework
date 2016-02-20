using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BLEFramework.Unity;
using UnityEngine.UI;

public class SearchBleDevicesListResultBehaviour : MonoBehaviour {

	public GameObject buttonPrefab;
	public GameObject buttonsPanel;
	public GameObject infoMessage;
	
	void OnEnable()
	{
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent += HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanErrorEvent += HandleOnBleDidCompletePeripheralScanErrorEvent;
		AppControllerBehaviour.DidStartPeripheralScanEvent += HandleDidStartPeripheralScanEvent;
		
		BLEControllerEventHandler.OnBleDidConnectEvent += HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent += HandleOnBleDidDisconnectEvent;
	}

	void HandleOnBleDidDisconnectEvent ()
	{
		infoMessage.SetActive(true);
		infoMessage.GetComponent<Text>().text = "Device did disconnect.";
	}
	
	void HandleOnBleDidConnectEvent ()
	{
		RemoveButtons();
		infoMessage.SetActive(true);
		infoMessage.GetComponent<Text>().text = "Device did connect.";
	}
	
	void HandleOnBleDidCompletePeripheralScanErrorEvent(string errorMessage)
	{
		//it means no devices were found
		if (errorMessage == "0")
		{
			infoMessage.SetActive(true);
			infoMessage.GetComponent<Text>().text = "No BLE devices found.";
		}
	}
	
	void HandleOnBleDidCompletePeripheralScanEvent (List<object> peripherals)
	{
		RefreshButtonsOnScreen(peripherals);
	}
	
	void OnDisable()
	{
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent -= HandleOnBleDidCompletePeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanErrorEvent -= HandleOnBleDidCompletePeripheralScanErrorEvent;
		AppControllerBehaviour.DidStartPeripheralScanEvent -= HandleDidStartPeripheralScanEvent;
		
		BLEControllerEventHandler.OnBleDidConnectEvent -= HandleOnBleDidConnectEvent;
		BLEControllerEventHandler.OnBleDidDisconnectEvent -= HandleOnBleDidDisconnectEvent;
	}
	
	
	void RemoveButtons()
	{
		//remove the buttons from the panel to make the room for the new buttons that will come
		foreach (Transform t in buttonsPanel.transform)
		{
			Destroy(t.gameObject);
		}
	}
	void HandleDidStartPeripheralScanEvent ()
	{
		RemoveButtons();	
	}
	
	
	void RefreshButtonsOnScreen(List<object> peripherals)
	{		
		infoMessage.SetActive(false);
		
		int j = 0;
		
		foreach (string s in peripherals)
		{
			GameObject instanceRow = Instantiate(buttonPrefab, new Vector3 (0,0,0), Quaternion.identity) as GameObject; 
			instanceRow.name = s;
			instanceRow.GetComponent<BleDeviceConnectButtonBehaviour>().LoadDataInButton(s, j);
			instanceRow.transform.SetParent(buttonsPanel.gameObject.transform, false);
			j++;
		}
		
		if (j == 0)
		{
			infoMessage.SetActive(true);
			infoMessage.GetComponent<Text>().text = "No BLE devices found.";
		}
		
		buttonsPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, j * 150.0f);
	}
}
