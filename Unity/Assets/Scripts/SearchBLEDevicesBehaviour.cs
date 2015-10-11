using UnityEngine;
using System.Collections;

public class SearchBLEDevicesBehaviour : MonoBehaviour 
{
	public delegate void SearchBLEDevicesEventDelegate();
	public static event SearchBLEDevicesEventDelegate SearchBLEDevicesEvent;
	// Use this for initialization
	public void ExecuteSearchBLEDevices()
	{
		if (SearchBLEDevicesEvent!=null)
		{
			SearchBLEDevicesEvent();
		}
	}
}
