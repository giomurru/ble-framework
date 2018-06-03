namespace BLEFramework.Unity
{
	using UnityEngine;	
	using System.Collections;
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
		private static extern bool _ConnectPeripheralAtIndex(int peripheralIndex);
		
		[DllImport ("__Internal")]
		private static extern bool _ConnectPeripheral(string peripheralID);
		
		[DllImport ("__Internal")]
		private static extern void _SendData(byte[] buffer);

		[DllImport ("__Internal")]
		private static extern byte[] _GetData();

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
	        /*
   				AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
   				AndroidJavaObject ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
 
   				var jc = new AndroidJavaClass("com.gmurru.bleframework.BleFrameworkManager");
   				jc.CallStatic("launchActivity", ajo);
   				*/
   				/*
				using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            	{
					using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
	                {
						using (AndroidJavaClass bleFrameworkManagerClass = new AndroidJavaClass("com.gmurru.bleframework.BleFrameworkManager"))
						{
							using (AndroidJavaObject androidPlugin = bleFrameworkManagerClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
	                    	{
								androidPlugin.Call("launchActivity", currentActivity);
							}
	                    }
	                }
	            }
	            */
	        	
				using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            	{
					using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
	                {
						using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
						{
							using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
	                    	{
	                    		Debug.Log("Calling initBleFramework from androidPlugin");
								androidPlugin.Call("_InitBLEFramework");
							}
	                    }
	                }
	            }
	            /*
	            using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
	            {
					using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
	                {
						using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
						{
							using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
	                    	{
								isConnected=androidPlugin.Call<bool>("_IsDeviceConnected");
							}
	                    }
	                }
	            }
	            */
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
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							androidPlugin.Call("_ScanForPeripherals");
						}
                    }
                }
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

			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							isConnected=androidPlugin.Call<bool>("_IsDeviceConnected");

						}
                    }
                }
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

			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							searchDevicesDidFinish=androidPlugin.Call<bool>("_SearchDevicesDidFinish");
						}
					}
				}
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
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							listOfDevices=androidPlugin.Call<string>("_GetListOfDevices");
						}	
					}	
				}
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
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
            				result=androidPlugin.Call<bool>("_ConnectPeripheralAtIndex",peripheralIndex);
            			}
            		}
            	}
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
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							result=androidPlugin.Call<bool>("_ConnectPeripheral",peripheralID);
						}
					}
				}
            }
			#endif
			
			return result;
		}

		public static byte[] GetData()
		{
			byte[] result = new byte[3];
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				result = _GetData();
			}
			#elif UNITY_ANDROID
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							result=androidPlugin.Call<byte[]>("_GetData");
						}
					}
				}
            }
			#endif
			
			return result;
		}

		public static void SendData(byte[] data)
		{
			// We check for UNITY_IPHONE again so we don't try this if it isn't iOS platform.
			#if UNITY_IPHONE
			// Now we check that it's actually an iOS device/simulator, not the Unity Player. You only get plugins on the actual device or iOS Simulator.
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_SendData(data);
			}
			#elif UNITY_ANDROID
			using (AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        	{
				using (AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
					using (AndroidJavaClass bleFrameworkClass = new AndroidJavaClass("com.gmurru.bleframework.BleFramework"))
					{
						using (AndroidJavaObject androidPlugin = bleFrameworkClass.CallStatic<AndroidJavaObject>("getInstance", currentActivity))
                    	{
							androidPlugin.Call("_SendData",data);
						}
					}
				}
            }
			#endif
		}
	}
}
	
	