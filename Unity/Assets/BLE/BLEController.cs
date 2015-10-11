using UnityEngine;
using System.Collections;
// We need this one for importing our IOS functions
using System.Runtime.InteropServices;

public class BLEController
{
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
	private static extern void _ConnectToDevice(int device);
	
	[DllImport ("__Internal")]
	private static extern void _SendData(string buffer);
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
		#endif
		
		return listOfDevices;
	}
	
	public static void ConnectToDevice(int device)
	{
		// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
		#if UNITY_IPHONE
		// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_ConnectToDevice(device);
		}
		#endif
	}
	
	public static void SendData(string data)
	{
		// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
		#if UNITY_IPHONE
		// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_SendData(data);
		}
		#endif
	}
}
	
	