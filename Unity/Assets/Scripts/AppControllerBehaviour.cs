using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AppControllerBehaviour : MonoBehaviour 
{
	
	public GameObject searchDevicesListResult;
	
	void Start () {
	}
	
	void OnEnable()
	{
		BLEControllerInitializer.SearchBLEDevicesScanDidFinishEvent += HandleSearchBLEDevicesScanDidFinishEvent;
		BLEControllerInitializer.SearchBLEDevicesScanDidFailEvent += HandleSearchBLEDevicesScanDidFailEvent;
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent += HandleSearchBLEDevicesEvent;
	}

	void HandleSearchBLEDevicesEvent ()
	{
		BLEControllerInitializer.Instance.SearchBLEDevices();
	}
	
	void OnDisable()
	{
		BLEControllerInitializer.SearchBLEDevicesScanDidFinishEvent -= HandleSearchBLEDevicesScanDidFinishEvent;
		BLEControllerInitializer.SearchBLEDevicesScanDidFailEvent -= HandleSearchBLEDevicesScanDidFailEvent;
		SearchBLEDevicesBehaviour.SearchBLEDevicesEvent -= HandleSearchBLEDevicesEvent;
	}
	
	
	void HandleSearchBLEDevicesScanDidFailEvent()
	{
		searchDevicesListResult.GetComponent<Text>().text = "I didn't find any BLE devices";
	}
	
	void HandleSearchBLEDevicesScanDidFinishEvent(List<object> result)
	{
		string listOfDevices = "";
		foreach (string s in result)
		{
			listOfDevices = listOfDevices + s + "\n";
		}
		searchDevicesListResult.GetComponent<Text>().text = listOfDevices;	
	}
}
