using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BleDeviceConnectButtonBehaviour : MonoBehaviour {

	
	public delegate void BleDevicesListButtonConnectEventDelegate(int buttonIndex);
	public static event BleDevicesListButtonConnectEventDelegate BleDevicesListButtonConnectEvent;

	public GameObject buttonTitle;

    public string title;
	public int index;
	
	public void ExecuteBleDevicesListButtonAction()
	{
		if (BleDevicesListButtonConnectEvent!=null)
		{
			BleDevicesListButtonConnectEvent(index);
		}
	}

    void Start()
    {
        buttonTitle.GetComponent<Text>().text = title;
    }
}
