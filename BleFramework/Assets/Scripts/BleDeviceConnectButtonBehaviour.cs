using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BleDeviceConnectButtonBehaviour : MonoBehaviour {

	
	public delegate void BleDevicesListButtonConnectEventDelegate(int buttonIndex);
	public static event BleDevicesListButtonConnectEventDelegate BleDevicesListButtonConnectEvent;

	public GameObject buttonTitle;

    public string title;
	public int index;
    bool needsUpdate = false;

    private void Awake()
    {
        needsUpdate = true;
    }
    public void ExecuteBleDevicesListButtonAction()
	{
		if (BleDevicesListButtonConnectEvent!=null)
		{
			BleDevicesListButtonConnectEvent(index);
		}
	}

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            Debug.Log("Setting button title to:" + title);
            buttonTitle.GetComponent<Text>().text = title;
        }
    }
}
