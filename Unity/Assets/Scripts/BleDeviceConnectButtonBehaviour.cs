using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BleDeviceConnectButtonBehaviour : MonoBehaviour {

	
	public delegate void BleDevicesListButtonConnectEventDelegate(uint buttonIndex);
	public static event BleDevicesListButtonConnectEventDelegate BleDevicesListButtonConnectEvent;

	public GameObject buttonTitle;

	uint buttonIndex;
	
	public void ExecuteBleDevicesListButtonAction()
	{
		if (BleDevicesListButtonConnectEvent!=null)
		{
			BleDevicesListButtonConnectEvent(buttonIndex);
		}
	}	
	
	public void LoadDataInButton(string deviceID, uint row)
	{
		transform.GetComponent<RectTransform>().localPosition =  new Vector3( 0.0f , -150.0f*row , 0.0f );
		//transform.GetComponent<RectTransform>().localScale = new Vector3( 1.0f, 1.0f, 1.0f);
		buttonTitle.GetComponent<Text>().text = deviceID;
		buttonIndex = row;
	}
}
