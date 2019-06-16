namespace BLEFramework.Unity
{
	using UnityEngine;	
	using System.Collections;
	using System.Runtime.InteropServices;


	public class BLEController : MonoBehaviour
    {
        const string bleFrameworkPathName = "com.gmurru.bleframework.BleFramework";
        // Use this #if so that if you run this code on a different platform, you won't get errors.
#if UNITY_IPHONE
		[DllImport ("__Internal")]
		private static extern void _InitBLEFramework();
		
		// For the most part, your imports match the function defined in the iOS code, except char* is replaced with string here so you get a C# string.    
		[DllImport ("__Internal")]
		private static extern void _ScanForPeripherals();
		
		[DllImport ("__Internal")]
		private static extern bool _IsDeviceConnected();
		
		[DllImport ("__Internal")]
		private static extern bool _SearchDevicesDidFinish();
		
		[DllImport ("__Internal")]
		private static extern string _GetListOfDevices();
		
		[DllImport ("__Internal")]
		private static extern bool _ConnectPeripheralAtIndex(int peripheralIndex);
		
		[DllImport ("__Internal")]
		private static extern bool _ConnectPeripheral(string peripheralID);
		
		[DllImport ("__Internal")]
		private static extern void _SendData(byte[] buffer, int length);

		[DllImport ("__Internal")]
		private static extern int _GetData(byte[] data, int size);

#elif UNITY_ANDROID

        class InitBLEFrameworkCallback : AndroidJavaProxy
        {
            private System.Action<string> initializeHandler;
            public InitBLEFrameworkCallback(System.Action<string> initializeHandlerIn) : base(bleFrameworkPathName + "$InitBLEFrameworkCallback")
            {
                initializeHandler = initializeHandlerIn;
            }
            public void onBleDidInitialize(string message)
            {
                Debug.Log("onBleDidInitialize: " + message);
                if (initializeHandler != null)
                {
                    initializeHandler(message);
                }
            }
        }

        static AndroidJavaClass _pluginClass;
        static AndroidJavaObject _pluginInstance;

        public static AndroidJavaClass PluginClass
        {
            get
            {
                if (_pluginClass == null)
                {
                    _pluginClass = new AndroidJavaClass(bleFrameworkPathName);
                }
                return _pluginClass;
            }
        }

        public static AndroidJavaObject PluginInstance
        {
            get
            {
                if (_pluginInstance == null)
                {
                    AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
                    _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance", activity);
                }
                return _pluginInstance;
            }
        }

#endif


        // Now make methods that you can provide the iOS functionality
        public static void InitBLEFramework ()
		{
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_InitBLEFramework();
			}
			#elif UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android)
	        {
                System.Action<string> callback = ((string message) =>
                {
                    BLEControllerEventHandler.OnBleDidInitialize(message);
                });
                PluginInstance.Call("_InitBLEFramework", new object[] { new InitBLEFrameworkCallback(callback)});
	        }
			#endif
		}
		
		public static void ScanForPeripherals()
		{
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_ScanForPeripherals();
			}
			#elif UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android)
            {
                PluginInstance.Call("_ScanForPeripherals");
            }
			#endif
		}
		
		public static bool IsDeviceConnected()
		{
			bool isConnected = false;
            // We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				isConnected = _IsDeviceConnected();
			}
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                isConnected = PluginInstance.Call<bool>("_IsDeviceConnected");
            }
#endif
			
			return isConnected;
		}
		
		public static bool SearchDevicesDidFinish()
		{
			bool searchDevicesDidFinish = false;
            // We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				searchDevicesDidFinish = _SearchDevicesDidFinish();
			}
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                searchDevicesDidFinish = PluginInstance.Call<bool>("_SearchDevicesDidFinish");
            }
#endif
			
			return searchDevicesDidFinish;
		}
		
		public static string GetListOfDevices()
		{
			string listOfDevices = "";
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				listOfDevices = _GetListOfDevices();
			}
			#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                listOfDevices = PluginInstance.Call<string>("_GetListOfDevices");
            }
			#endif

			return listOfDevices;
		}
		
		public static bool ConnectPeripheralAtIndex(int peripheralIndex)
		{
			bool result = false;
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				result = _ConnectPeripheralAtIndex(peripheralIndex);
			}
			#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                result = PluginInstance.Call<bool>("_ConnectPeripheralAtIndex", peripheralIndex);
            }
            #endif

			return result;
		}
		
		public static bool ConnectPeripheral(string peripheralID)
		{
			bool result = false;
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				result =  _ConnectPeripheral(peripheralID);
			}
			#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                result = PluginInstance.Call<bool>("_ConnectPeripheral", peripheralID);
            }
			#endif
			
			return result;
		}

		public static byte[] GetData(int length)
		{
            byte[] data = new byte[length];
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
                Debug.Log("inside public static byte[] GetData()");
				int result = _GetData(data, length);
                if (result == 0)
                {
                    Debug.Log("success in getting data");
                } else
                {
                    Debug.Log("failure in getting data");
                }
			}
			#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                data = PluginInstance.Call<byte[]>("_GetData");
            }
			#endif
			
			return data;
		}

		public static void SendData(byte[] data)
		{
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_SendData(data, data.Length);
			}
			#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                PluginInstance.Call("_SendData", data);
            }
			#endif
		}
	}
}