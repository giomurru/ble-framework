namespace BLEFramework.Unity
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using BLEFramework.MiniJSON;
	
	public class BLEControllerEventHandler : MonoBehaviour {
	
		
		
		//native events
		public delegate void OnBleDidConnectEventDelegate(string error);
		public static event OnBleDidConnectEventDelegate OnBleDidConnectEvent;
		
		public delegate void OnBleDidDisconnectEventDelegate(string error);
		public static event OnBleDidDisconnectEventDelegate OnBleDidDisconnectEvent;
		
		public delegate void OnBleDidReceiveDataEventDelegate(byte[] data, int numOfBytes);
		public static event OnBleDidReceiveDataEventDelegate OnBleDidReceiveDataEvent;
		
		public delegate void OnBleDidInitializeEventDelegate(string error);
		public static event OnBleDidInitializeEventDelegate OnBleDidInitializeEvent;
		
		public delegate void OnBleDidCompletePeripheralScanEventDelegate(List<object> peripherals, string error);
		public static event OnBleDidCompletePeripheralScanEventDelegate OnBleDidCompletePeripheralScanEvent;
		

        //Instance methods used by iOS Unity Send Message
        void OnBleDidInitializeMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidInitialize(message);
		}

		public static void OnBleDidInitialize(string message)
		{
            string errorMessage = message != "Success" ? message : null;
            OnBleDidInitializeEvent?.Invoke(errorMessage);
        }


        void OnBleDidConnectMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidConnect(message);
		}
		public static void OnBleDidConnect(string message)
		{
            string errorMessage = message != "Success" ? message : null;
            OnBleDidConnectEvent?.Invoke(errorMessage);
        }

		void OnBleDidDisconnectMessage(string message)
		{
			BLEControllerEventHandler.OnBleDidDisconnect(message);
		}
		public static void OnBleDidDisconnect(string message)
		{
            string errorMessage = message != "Success" ? message : null;
            OnBleDidDisconnectEvent?.Invoke(errorMessage);
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
                    OnBleDidReceiveDataEvent?.Invoke(data, numOfBytes);
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
            string errorMessage = message != "Success" ? message : null;
            List<object> peripheralsList = new List<object>();
            string peripheralJsonList = (errorMessage == null) ? BLEController.GetListOfDevices() : null;
            if (peripheralJsonList != null)
            {
                Dictionary<string, object> dictObject = Json.Deserialize(peripheralJsonList) as Dictionary<string, object>;

                object receivedByteDataArray;
                if (dictObject.TryGetValue("data", out receivedByteDataArray))
                {
                    peripheralsList = (List<object>)receivedByteDataArray;
                }
            }

            OnBleDidCompletePeripheralScanEvent?.Invoke(peripheralsList, errorMessage);
        }
	}
}