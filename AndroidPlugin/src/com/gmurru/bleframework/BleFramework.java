package com.gmurru.bleframework;

import java.util.List;
import java.util.ArrayList;
import java.util.Arrays;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.Context;
import android.content.ServiceConnection;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.pm.PackageManager;

import android.os.IBinder;

import android.util.Log;
import com.unity3d.player.UnityPlayer;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattService;
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;
import android.bluetooth.BluetoothManager;
import android.app.Activity;

public class BleFramework
{
    /*
    Definition of the BLE Unity message methods used to communicate back with Unity. 
    */
    public static final String BLEUnityMessageName_OnBleDidInitialize = "OnBleDidInitialize";
    public static final String BLEUnityMessageName_OnBleDidConnect = "OnBleDidConnect";
    public static final String BLEUnityMessageName_OnBleDidCompletePeripheralScan = "OnBleDidCompletePeripheralScan";
    public static final String BLEUnityMessageName_OnBleDidDisconnect = "OnBleDidDisconnect";
    public static final String BLEUnityMessageName_OnBleDidReceiveData = "OnBleDidReceiveData";

    /*
    Singleton instance.
    */
    private static volatile BleFramework _instance;

    /*
    The context represents the Unity native activity that is actually running.
    */
	private Context _context;

    /*
    Static variables 
    */
    private static final String TAG = BleFramework.class.getSimpleName();
    private static final int REQUEST_ENABLE_BT = 1;
    private static final long SCAN_PERIOD = 3000;
    public static final int REQUEST_CODE = 30;

    /*
    Static list contained all the discovered bluetooth devices
    */
    private static List<BluetoothDevice> _mDevice = new ArrayList<BluetoothDevice>();

    /*
    The characteristic used to transmit data
    */
    private BluetoothGattCharacteristic _characteristicTx = null;
    /*
    The latest received data
    */
    private byte[] _dataRx;

    /*
    Bluetooth service 
    */
    private RBLService _mBluetoothLeService;
    /*
    Bluetooth adapter
    */
    private BluetoothAdapter _mBluetoothAdapter;

    /*
    Bluetooth device address and name to which the app is currently connected
    */
    private String _mDeviceAddress;
    private String _mDeviceName;

    /*
    Boolean variables used to estabilish the status of the connection
    */
    private boolean _flag = true;
    private boolean _connState = false;

    /*
    The service connection containing the actions definition onServiceConnected and onServiceDisconnected
    */
    private final ServiceConnection _mServiceConnection = new ServiceConnection() 
    {
        @Override
        public void onServiceConnected(ComponentName componentName, IBinder service) 
        {
            _mBluetoothLeService = ((RBLService.LocalBinder) service).getService();
            if (!_mBluetoothLeService.initialize()) 
            {
                Log.e(TAG, "onServiceConnected: Unable to initialize Bluetooth");
            }
            else
            {
                Log.d(TAG, "onServiceConnected: Bluetooth initialized correctly");
            }
        }

        @Override
        public void onServiceDisconnected(ComponentName componentName) 
        {
            Log.d(TAG, "onServiceDisconnected: Bluetooth disconnected");
            _mBluetoothLeService = null;
        }
    };

    /*
    Callback called when the scan of bluetooth devices is finished
    */
    private BluetoothAdapter.LeScanCallback _mLeScanCallback = new BluetoothAdapter.LeScanCallback() 
    {
        @Override
        public void onLeScan(final BluetoothDevice device, final int rssi, byte[] scanRecord) 
        {
            Activity activity = (Activity) _context;
            activity.runOnUiThread(new Runnable() {
                @Override
                public void run() 
                {
                    Log.d(TAG, "onLeScan: run()");
                    if (device != null) 
                    {
                        Log.d(TAG, "onLeScan: device is not null");
                        if (_mDevice.indexOf(device) == -1)
                        {
                            Log.d(TAG, "onLeScan: add device to _mDevice");
                            _mDevice.add(device);
                        }
                    }
                    else
                    {
                        Log.e(TAG, "onLeScan: device is null");
                    }
                }
            });
        }
    };

    

    /*
    Callback called when the bluetooth device receive relevant updates about connection, disconnection, service discovery, data available, rssi update
    */
    private final BroadcastReceiver mGattUpdateReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();

            if (RBLService.ACTION_GATT_CONNECTED.equals(action)) {
                _flag = true;
                _connState = true;

                //send the reset signal to the robot
                byte buf[] = new byte[] { (byte) 0x04, (byte) 0x00, (byte) 0x00 };
                _characteristicTx.setValue(buf);
                _mBluetoothLeService.writeCharacteristic(_characteristicTx);

                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidConnect, "Success");

                Log.d(TAG, "Connection estabilished with: " + _mDeviceAddress);
                startReadRssi();
            } 
            else if (RBLService.ACTION_GATT_DISCONNECTED.equals(action)) 
            {
                _flag = false;
                _connState = false;

                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidDisconnect, "Success");

                Log.d(TAG, "Connection lost");
            } 
            else if (RBLService.ACTION_GATT_SERVICES_DISCOVERED.equals(action)) 
            {
                getGattService(_mBluetoothLeService.getSupportedGattService());
            } 
            else if (RBLService.ACTION_DATA_AVAILABLE.equals(action)) 
            {
                _dataRx = intent.getByteArrayExtra(RBLService.EXTRA_DATA);

                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidReceiveData, "3");
            }
            else if (RBLService.ACTION_GATT_RSSI.equals(action)) 
            {
                String rssiData = intent.getStringExtra(RBLService.EXTRA_DATA);
                Log.d(TAG, "RSSI: " + rssiData);
            }
        }
    };

    /*
    METHODS DEFINITION
    */

    /*
    Method used to create a filter for the bluetooth actions that you like to receive
    */
    private static IntentFilter makeGattUpdateIntentFilter() 
    {
        final IntentFilter intentFilter = new IntentFilter();

        intentFilter.addAction(RBLService.ACTION_GATT_CONNECTED);
        intentFilter.addAction(RBLService.ACTION_GATT_DISCONNECTED);
        intentFilter.addAction(RBLService.ACTION_GATT_SERVICES_DISCOVERED);
        intentFilter.addAction(RBLService.ACTION_DATA_AVAILABLE);
        intentFilter.addAction(RBLService.ACTION_GATT_RSSI);

        return intentFilter;
    }

    /*
    Method used to start receiving updates for bluetooth actions and filter them
    */
    private void RegisterGattUpdateReceiver()
    {
        Activity activity = (Activity) _context;
        if (!_mBluetoothAdapter.isEnabled()) 
        {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            activity.startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
        }

        activity.registerReceiver(mGattUpdateReceiver, makeGattUpdateIntentFilter());
    }

    /*
    Start reading RSSI: information about bluetooth signal intensity
    */
    private void startReadRssi() 
    {
        new Thread() 
        {
            public void run() 
            {
                while (_flag) 
                {
                    _mBluetoothLeService.readRssi();
                    try 
                    {
                        sleep(500);
                    } 
                    catch (InterruptedException e) 
                    {
                        e.printStackTrace();
                    }
                }
            };
        }.start();
    }

    /*
    Method used to initialize the characteristic for data transmission
    */
    private void getGattService(BluetoothGattService gattService) 
    {
        if (gattService == null)
            return;

        _characteristicTx = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_TX);

        BluetoothGattCharacteristic characteristicRx = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_RX);
        _mBluetoothLeService.setCharacteristicNotification(characteristicRx,true);
        _mBluetoothLeService.readCharacteristic(characteristicRx);
    }

    
    /*
    Method used to scan for available bluetooth low energy devices
    */
    private void scanLeDevice() 
    {
        new Thread() 
        {

            @Override
            public void run() 
            {

                if (_mBluetoothAdapter == null) 
                {
                    Log.e(TAG, "scanLeDevice: Abort _mBluetoothAdapter is null");
                    return;
                }
                else
                {
                    Log.d(TAG, "scanLeDevice: _mBluetoothAdapter StartLeScan");
                    _mBluetoothAdapter.startLeScan(_mLeScanCallback);
                }

                try 
                {
                    Log.d(TAG, "scanLeDevice: scan for 3 seconds then abort");
                    Thread.sleep(SCAN_PERIOD);
                } 
                catch (InterruptedException e) 
                {
                    Log.e(TAG, "scanLeDevice: InterruptedException");
                    e.printStackTrace();
                }

                Log.d(TAG, "scanLeDevice: _mBluetoothAdapter StopLeScan");
                _mBluetoothAdapter.stopLeScan(_mLeScanCallback);

                Log.d(TAG, "scanLeDevice: _mDevice size is " + _mDevice.size());

                String mDeviceJson = _GetListOfDevices();
                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidCompletePeripheralScan, mDeviceJson);
            }
        }.start();
    }

    
    /*
    Private Constructor. It is used by the getInstance method to create a singleton instance of BleFramework class.
    */
    private BleFramework(Context context)
    {
        Log.d(TAG, "BleFramework: Singleton Creation");
        this._context = context;
    }

    /*
    Singleton initialization. Create an instance of BleFramework class only if it doesn't exist yet.
    */
    public static BleFramework getInstance(Context context) 
    {
        if (_instance == null ) 
        {
            synchronized (BleFramework.class) 
            {
                if (_instance == null) 
                {
                    Log.d(TAG, "BleFramework: Creation of _instance");
                    _instance = new BleFramework(context);
                }
            }
        }

        return _instance;
    }

    /*
    Public methods that can be directly called by Unity
    */
    public void _InitBLEFramework()
    {
        System.out.println("Android Executing: _InitBLEFramework");

        if (!_context.getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE)) 
        {
            Log.e(TAG,"_InitBLEFramework: FEATURE_BLUETOOTH_LE is not found");
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: FEATURE_BLUETOOTH_LE");
            return;
        }

        Activity activity = (Activity) _context;
        final BluetoothManager mBluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
        _mBluetoothAdapter = mBluetoothManager.getAdapter();
        
        if (_mBluetoothAdapter == null) 
        {
            Log.e(TAG,"_InitBLEFramework: _mBluetoothAdapter is null");
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: Context.BLUETOOTH_SERVICE");
        }
        else
        {
            Log.d(TAG,"_InitBLEFramework: _mBluetoothAdapter found");
            Intent gattServiceIntent = new Intent(activity, RBLService.class);
            activity.bindService(gattServiceIntent, _mServiceConnection, activity.BIND_AUTO_CREATE);
            RegisterGattUpdateReceiver();
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Success");
        }

        
    }

    public void _ScanForPeripherals()
    {
        if (_connState == false) 
        {
            Log.d(TAG, "_ScanForPeripherals: Launching scanLeDevice");
            scanLeDevice();
        } 
        else 
        {
            Log.d(TAG, "_ScanForPeripherals: Disconnect and close Ble service");
            _mBluetoothLeService.disconnect();
            _mBluetoothLeService.close();
        }
    }

    public boolean _IsDeviceConnected()
    {
        Log.d(TAG,"_IsDeviceConnected");
        return _connState;
    }

    public boolean _SearchDeviceDidFinish()
    {
        Log.d(TAG,"_SearchDeviceDidFinish");
        return true;
    }

    public String _GetListOfDevices()
    {
        String jsonListString;

        if (_mDevice.size() > 0)
        {
            Log.d(TAG,"_GetListOfDevices");
            String[] uuidsArray = new String[_mDevice.size()];

            for (int i = 0; i < _mDevice.size(); i++) 
            {

                BluetoothDevice bd = _mDevice.get(i);
                /*
                ParcelUuid[] puiids = bd.getUuids();
                if (puuids!=null)
                {
                    String uuid = puiids[0].getUuid().toString();
                    Log.d(TAG, "scanLeDevice: Adding " +uuid+" to array");
                    uuidsArray[i] = uuid;
                }
                */

                uuidsArray[i] = bd.getAddress();
            }
            Log.d(TAG, "_GetListOfDevices: Building JSONArray");
            JSONArray uuidsJSON = new JSONArray(Arrays.asList(uuidsArray));
            Log.d(TAG, "_GetListOfDevices: Building JSONObject");
            JSONObject dataUuidsJSON = new JSONObject();

            try
            {
                Log.d(TAG, "_GetListOfDevices: Try inserting uuuidsJSON array in the JSONObject");
                dataUuidsJSON.put("data", uuidsJSON);
            }
            catch (JSONException e)
            {
                Log.e(TAG, "_GetListOfDevices: JSONException");
                e.printStackTrace();
            }

            jsonListString = dataUuidsJSON.toString();

            Log.d(TAG, "_GetListOfDevices: sending found devices in JSON: " + jsonListString);

        }
        else
        {
            jsonListString = "NO DEVICE FOUND";
            Log.d(TAG, "_GetListOfDevices: no device was found");
        }

        return jsonListString;
    }

    public boolean _ConnectPeripheralAtIndex(int peripheralIndex)
    {
        Log.d(TAG,"_ConnectPeripheralAtIndex: " + peripheralIndex);
        BluetoothDevice device = _mDevice.get(peripheralIndex);
        _mDeviceAddress = device.getAddress();
        _mDeviceName = device.getName();
        return _mBluetoothLeService.connect(_mDeviceAddress);
    }

    public boolean _ConnectPeripheral(String peripheralID)
    {
        Log.d(TAG,"_ConnectPeripheral: " + peripheralID);

        for (BluetoothDevice device : _mDevice)
        {
            if (device.getAddress().equals(peripheralID)) 
            {
                _mDeviceAddress = device.getAddress();
                _mDeviceName = device.getName();
                return _mBluetoothLeService.connect(_mDeviceAddress);
            }
        }
             
        return false;
    }

    public byte[] _GetData()
    {
        Log.d(TAG,"_GetData: ");
        return _dataRx;
    }
    public void _SendData(String data)
    {
        Log.d(TAG,"_SendData: " + data);
    }

    public void _OnPause(boolean pause)
    {
        if (pause == true)
        {
            _flag = false;
            Activity activity = (Activity) _context;
            activity.unregisterReceiver(mGattUpdateReceiver);
        }
        else
        {
            RegisterGattUpdateReceiver();
        }
    }
}