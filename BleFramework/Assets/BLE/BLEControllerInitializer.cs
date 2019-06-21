namespace BLEFramework.Unity
{
	using UnityEngine;
	using System.Collections;
	
	public class BLEControllerInitializer : MonoBehaviour 
	{
		
		public static BLEControllerInitializer Instance;
		
		
		void Awake()
		{
			if(Instance == null)
			{
				Instance = this;
				gameObject.name = "BLEControllerInitializer";
				DontDestroyOnLoad(this.gameObject);
			} 
			else 
			{
				Destroy(this.gameObject);
			}
		}
		
		public void InitBLEFramework()
		{
			BLEController.InitBLEFramework();
		}
			
	}
}