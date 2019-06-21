using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BLEFramework.Unity;
using UnityEngine.UI;

public class SearchBleDevicesListResultBehaviour : MonoBehaviour {

	public GameObject buttonPrefab;
	public GameObject buttonsPanel;
	public GameObject infoMessage;
	
	void OnEnable()
	{
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent += HandleOnBleDidCompletePeripheralScanEvent;
		AppControllerBehaviour.DidStartPeripheralScanEvent += HandleDidStartPeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent += HandleOnBleDidConnectEvent;
	}
	
	void HandleOnBleDidConnectEvent (string errorMessage)
	{
        if (errorMessage == null)
        {
            RemoveButtons();
        }
	}
	
	void HandleOnBleDidCompletePeripheralScanEvent (List<object> peripherals, string errorMessage)
	{
        if (errorMessage == null)
        {
            RefreshButtonsOnScreen(peripherals);
        }
	}
	
	void OnDisable()
	{
		BLEControllerEventHandler.OnBleDidCompletePeripheralScanEvent -= HandleOnBleDidCompletePeripheralScanEvent;
		AppControllerBehaviour.DidStartPeripheralScanEvent -= HandleDidStartPeripheralScanEvent;
		BLEControllerEventHandler.OnBleDidConnectEvent -= HandleOnBleDidConnectEvent;
	}
	
	
	void RemoveButtons()
	{
		//remove the buttons from the panel to make the room for the new buttons that will come
		foreach (Transform t in buttonsPanel.transform)
		{
			Destroy(t.gameObject);
		}
	}
	void HandleDidStartPeripheralScanEvent ()
	{
		RemoveButtons();	
	}
	
	
	void RefreshButtonsOnScreen(List<object> peripherals)
	{		
		int j = 0;
		
		foreach (string s in peripherals)
		{
			GameObject instanceRow = Instantiate(buttonPrefab, new Vector3 (0.0f,j*150.0f,0.0f), Quaternion.identity) as GameObject; 
            instanceRow.GetComponent<BleDeviceConnectButtonBehaviour>().title = s;
            instanceRow.GetComponent<BleDeviceConnectButtonBehaviour>().index = j;
            instanceRow.transform.SetParent(buttonsPanel.transform, false);
            j++;
		}

        infoMessage.GetComponent<Text>().text = (j == 0) ? "No BLE devices found." : "Scan completed.";

        // This part of code creates a bug on UI
        // TODO: when you add buttons you change the panel size and should update the scrollview content.
        //Rect panelRect = buttonsPanel.GetComponent<RectTransform>().rect;
        //float minimumPanelHeight = j * 150.0f;
        //float panelHeight = panelRect.height < minimumPanelHeight ? minimumPanelHeight : panelRect.height;
        //buttonsPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(panelRect.width, panelHeight);
    }
}
