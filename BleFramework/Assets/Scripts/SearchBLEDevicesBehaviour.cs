using UnityEngine;

public class SearchBLEDevicesBehaviour : MonoBehaviour 
{
	public delegate void SearchBLEDevicesEventDelegate();
	public static event SearchBLEDevicesEventDelegate SearchBLEDevicesEvent;
	// Use this for initialization
	public void ExecuteSearchBLEDevices()
	{
        SearchBLEDevicesEvent?.Invoke();
	}
}
