namespace BLEFramework.Unity
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using BLEFramework.MiniJSON;
	
	public class BLEControllerEventHandler : MonoBehaviour {
	
		
		
		//native events
		public delegate void OnBleDidConnectEventDelegate();
		public static event OnBleDidConnectEventDelegate OnBleDidConnectEvent;
		
		public delegate void OnBleDidDisconnectEventDelegate();
		public static event OnBleDidDisconnectEventDelegate OnBleDidDisconnectEvent;
		
		public delegate void OnBleDidReceiveDataEventDelegate(byte[] data, int numOfBytes);
		public static event OnBleDidReceiveDataEventDelegate OnBleDidReceiveDataEvent;
		
		public delegate void OnBleDidInitializeEventDelegate();
		public static event OnBleDidInitializeEventDelegate OnBleDidInitializeEvent;
		
		public delegate void OnBleDidCompletePeripheralScanEventDelegate(List<object> peripherals);
		public static event OnBleDidCompletePeripheralScanEventDelegate OnBleDidCompletePeripheralScanEvent;
		
		//errors
		public delegate void OnBleDidInitializeErrorEventDelegate(string errorMessage);
		public static event OnBleDidInitializeErrorEventDelegate OnBleDidInitializeErrorEvent;
		
		public delegate void OnBleDidConnectErrorEventDelegate(string errorMessage);
		public static event OnBleDidConnectErrorEventDelegate OnBleDidConnectErrorEvent;
				
		public delegate void OnBleDidDisconnectErrorEventDelegate(string errorMessage);
		public static event OnBleDidDisconnectErrorEventDelegate OnBleDidDisconnectErrorEvent;
		
		public delegate void OnBleDidCompletePeripheralScanErrorEventDelegate(string errorMessage);
		public static event OnBleDidCompletePeripheralScanErrorEventDelegate OnBleDidCompletePeripheralScanErrorEvent;
		

        //Instance methods used by iOS Unity Send Message
        void OnBleDidInitializeMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidInitialize(message);
		}

		public static void OnBleDidInitialize(string message)
		{
			if (message=="Success")
			{
				if (OnBleDidInitializeEvent!=null)
				{
					OnBleDidInitializeEvent();
				}
			}
			else if (OnBleDidInitializeErrorEvent!=null)
			{
				OnBleDidInitializeErrorEvent(message);
			}
		}


        void OnBleDidConnectMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidConnect(message);
		}
		public static void OnBleDidConnect(string message)
		{
			if (message=="Success")
			{
				if (OnBleDidConnectEvent!=null)
				{
					OnBleDidConnectEvent();
				}
			}
			else if (OnBleDidConnectErrorEvent!=null)
			{
				OnBleDidConnectErrorEvent(message);
			}
		}

		void OnBleDidDisconnectMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidDisconnect(message);
		}
		public static void OnBleDidDisconnect(string message)
		{
			if (message=="Success")
			{
				if (OnBleDidDisconnectEvent!=null)
				{
					OnBleDidDisconnectEvent();
				}
			}
			else if (OnBleDidDisconnectErrorEvent!=null)
			{
				OnBleDidDisconnectErrorEvent(message);
			}
		}

		void OnBleDidReceiveDataMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidReceiveData(message);
		}
		public static void OnBleDidReceiveData(string message)
		{
			int numOfBytes = 0;
			if (int.TryParse(message, out numOfBytes))
			{
                if (numOfBytes != 0)
                {
                    Debug.Log("BLEController.GetData(); start");
                    byte[] data = BLEController.GetData(numOfBytes);
                    Debug.Log("BLEController.GetData(); end");
                    if (OnBleDidReceiveDataEvent != null)
                    {
                        OnBleDidReceiveDataEvent(data, numOfBytes);
                    }
                } else
                {
                    Debug.Log("WARNING: did receive OnBleDidReceiveData even if numOfBytes is zero");
                }
			}
			

		}

		void OnBleDidCompletePeripheralScanMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidCompletePeripheralScan(message);
		}
		public static void OnBleDidCompletePeripheralScan(string message)
		{
			if (message != "Success") {
				Debug.Log("OnBleDidCompletePeripheralScan: message is not success");
				if (OnBleDidCompletePeripheralScanErrorEvent!=null) {
					Debug.Log("OnBleDidCompletePeripheralScan: call OnBleDidCompletePeripheralScanErrorEvent: " + message);
					OnBleDidCompletePeripheralScanErrorEvent(message);
				}
			}
			else {
				Debug.Log("call BLEController.GetListOfDevices()");
				string peripheralJsonList = BLEController.GetListOfDevices();
				Debug.Log("the json list is "+ peripheralJsonList);
				if (peripheralJsonList != null) {
					Dictionary<string, object> dictObject = Json.Deserialize(peripheralJsonList) as Dictionary<string, object>;

					object receivedByteDataArray;
					List<object> peripheralsList = new List<object>();
					if (dictObject.TryGetValue("data", out receivedByteDataArray)) {
						Debug.Log("OnBleDidCompletePeripheralScan: I received the peripheral list");
						peripheralsList = (List<object>)receivedByteDataArray;
						Debug.Log("OnBleDidCompletePeripheralScan: I set the peripheral list");
					}

					if (OnBleDidCompletePeripheralScanEvent != null) {
						Debug.Log("OnBleDidCompletePeripheralScan: I call OnBleDidCompletePeripheralScanEvent");
						OnBleDidCompletePeripheralScanEvent(peripheralsList);
					}
				} else {
					if (OnBleDidCompletePeripheralScanErrorEvent != null)
					{
						Debug.Log("OnBleDidCompletePeripheralScan: call OnBleDidCompletePeripheralScanErrorEvent: " + message);
						OnBleDidCompletePeripheralScanErrorEvent("0");
					}
				}
			}
		}
	}
}