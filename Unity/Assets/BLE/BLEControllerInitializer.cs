using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BLE.MiniJSON;

public class BLEControllerInitializer : MonoBehaviour {

public delegate void SearchBLEDevicesScanDidFinishEventDelegate(List<object> result);
public static event SearchBLEDevicesScanDidFinishEventDelegate  SearchBLEDevicesScanDidFinishEvent;


public delegate void SearchBLEDevicesScanDidFailEventDelegate();
public static event SearchBLEDevicesScanDidFailEventDelegate  SearchBLEDevicesScanDidFailEvent;

public static BLEControllerInitializer Instance;

void Awake()
{
	if(Instance == null)
	{
		Instance = this;
		gameObject.name = "BLEControllerInitializer";
		GameObject.DontDestroyOnLoad(this.gameObject);
	} 
	else 
	{
		GameObject.Destroy(this.gameObject);
	}
}

void Start()
{
		BLEController.InitBLEFramework();
}


public void SearchBLEDevices()
{
	BLEController.ScanForPeripherals();
	StartCoroutine(SearchBLEDevicesCallback());
}

IEnumerator SearchBLEDevicesCallback()
{
	while (!BLEController.SearchDevicesDidFinish())
	{
		yield return null;
	}
	
	string result = BLEController.GetListOfDevices();
	
	Debug.Log("Result of GetListOfDevices:\n"+result+"\n");	
	if (result!=null)
	{
		Debug.Log ("SearchBLEDevicesCallback: Calling Deserialize result");
		Dictionary<string, object> dictObject = Json.Deserialize(result) as Dictionary<string, object>;
		
		object deviceIDSObject;
		List<object> deviceIDS = new List<object>();
		if (dictObject.TryGetValue ("DeviceIDS", out deviceIDSObject)) 
		{
			deviceIDS = (List<object>) deviceIDSObject;
		}
		
		if (SearchBLEDevicesScanDidFinishEvent!=null && deviceIDS!=null)
		{
			Debug.Log ("SearchBLEDevicesCallback: SearchBLEDevicesScanDidFinishEvent");
				
			SearchBLEDevicesScanDidFinishEvent(deviceIDS);
		}
		else if (deviceIDS==null && SearchBLEDevicesScanDidFailEvent!=null)
		{
			Debug.Log ("SearchBLEDevicesCallback: responseObject is null. Calling Fail Event.");
				
			SearchBLEDevicesScanDidFailEvent();
		}
	}
	else
	{
		if (SearchBLEDevicesScanDidFailEvent!=null)
		{
			SearchBLEDevicesScanDidFailEvent();
		}	
	}
}


		
}
