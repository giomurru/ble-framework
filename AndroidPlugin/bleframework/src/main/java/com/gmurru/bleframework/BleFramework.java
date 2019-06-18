package com.gmurru.bleframework;

import java.util.ArrayList;
import java.util.Arrays;

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

import android.content.Intent;
import android.content.IntentFilter;
import android.content.Context;
import android.content.ServiceConnection;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.pm.PackageManager;

import android.os.IBinder;

import android.util.Log;
////import com.unity3d.player.UnityPlayer;

import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattService;
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;
import android.app.Activity;

/**
 * Created by Fenix on 24/09/2017.
 */

public class BleFramework
{
    private Activity _unityActivity;
    /*
    Singleton instance.
    */
    private static volatile BleFramework _instance;

    /* Old method used by UnityPlayer
    Definition of the BLE Unity message methods used to communicate back with Unity.

    public static final String BLEUnityMessageName_OnBleDidInitialize = "OnBleDidInitialize";
    public static final String BLEUnityMessageName_OnBleDidConnect = "OnBleDidConnect";
    public static final String BLEUnityMessageName_OnBleDidCompletePeripheralScan = "OnBleDidCompletePeripheralScan";
    public static final String BLEUnityMessageName_OnBleDidDisconnect = "OnBleDidDisconnect";
    public static final String BLEUnityMessageName_OnBleDidReceiveData = "OnBleDidReceiveData";
    */

    public interface UnityCallback {
        public void sendMessage(String message);
    }

    private UnityCallback onBleDidConnect;
    private UnityCallback onBleDidDiscoverService;
    private UnityCallback onBleDidDisconnect;
    private UnityCallback onBleDidReceiveData;
    /*
    Static variables
    */
    private static final String TAG = BleFramework.class.getSimpleName();
    private static final int REQUEST_ENABLE_BT = 1;
    private static final long SCAN_PERIOD = 5000;
    public static final int REQUEST_CODE = 30;

    /*
    List containing all the discovered bluetooth devices
    */
    private ArrayList<BluetoothDevice> _mDevice = new ArrayList<BluetoothDevice>();

    /*
    The latest received data
    */
    private byte[] _dataRx = new byte[3];
    private byte[] _dataTx;
    /*
    Bluetooth service
    */
    private RBLService _mBluetoothLeService;

    private Map<UUID, BluetoothGattCharacteristic> _map = new HashMap<UUID, BluetoothGattCharacteristic>();

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
    private boolean _connState = false;
    private boolean _flag = true;
    // Flag used to understand if search of devices is in progress
    private boolean _searchingDevice = false;

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
                //finish();
            }
            else
            {
                Log.d(TAG, "onServiceConnected: Bluetooth initialized correctly");
                // do not connect automatically
                _mBluetoothLeService.connect(_mDeviceAddress);
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
    Callback called when the bluetooth device receive relevant updates about connection, disconnection, service discovery, data available, rssi update
    */
    private final BroadcastReceiver _mGattUpdateReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();

            if (RBLService.ACTION_GATT_CONNECTED.equals(action))
            {
                _connState = true;
                _flag = true;
                Log.d(TAG, "Connection estabilished with: " + _mDeviceAddress);
                Log.d(TAG, "Send BLEUnityMessageName_OnBleDidConnect success signal to Unity");
                //UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidConnect, "Success");
                onBleDidConnect.sendMessage("Success");
                //startReadRssi();
            }
            else if (RBLService.ACTION_GATT_DISCONNECTED.equals(action))
            {
                _connState = false;
                _flag = false;
                ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidDisconnect, "Success");
                onBleDidDisconnect.sendMessage("Success");
                Log.d(TAG, "Connection lost");
            }
            else if (RBLService.ACTION_GATT_SERVICES_DISCOVERED.equals(action))
            {
                Log.d(TAG, "Service discovered! Registering GattService ACTION_GATT_SERVICES_DISCOVERED");
                BluetoothGattService gattService = _mBluetoothLeService.getSupportedGattService();
                if (gattService != null) {
                    //getGattService(gattService);
                    onBleDidDiscoverService.sendMessage("Success");
                } else {
                    onBleDidDiscoverService.sendMessage("Failure");
                }
            }
            else if (RBLService.ACTION_DATA_AVAILABLE.equals(action))
            {
                Log.d(TAG, "New Data received by the server");
                _dataRx = intent.getByteArrayExtra(RBLService.EXTRA_DATA);
                ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidReceiveData, String.valueOf(_dataRx.length));
                onBleDidReceiveData.sendMessage(String.valueOf(_dataRx.length));
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

    private BleFramework(Activity activity)
    {
        Log.d(TAG, "BleFramework: saving unityActivity in private var.");
        this._unityActivity = activity;
    }

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

        return intentFilter;
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
        BluetoothGattCharacteristic characteristic = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_TX);
        _map.put(characteristic.getUuid(), characteristic);

        BluetoothGattCharacteristic characteristicRx = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_RX);
        _mBluetoothLeService.setCharacteristicNotification(characteristicRx,
                true);
        _mBluetoothLeService.readCharacteristic(characteristicRx);
    }


    /*
    Callback called when the scan of bluetooth devices is finished
    */
    private BluetoothAdapter.LeScanCallback mLeScanCallback = new BluetoothAdapter.LeScanCallback()
    {
        @Override
        public void onLeScan(final BluetoothDevice device, final int rssi, byte[] scanRecord)
        {
            _unityActivity.runOnUiThread(new Runnable() {
                @Override
                public void run()
                {
                    if (device != null && device.getName()!=null)
                    {
                        Log.d(TAG, "found device " + device.getName() + "with address " + device.getAddress());
                        //don't include duplicates
                        if (_mDevice.indexOf(device) == -1) {
                            _mDevice.add(device);
                        }
                    }
                }
            });
        }
    };

    private void unregisterBleUpdatesReceiver()
    {
        Log.d(TAG,"unregisterBleUpdatesReceiver:");
        _flag = false;
        _unityActivity.unregisterReceiver(_mGattUpdateReceiver);
    }

    private void registerBleUpdatesReceiver()
    {
        Log.d(TAG,"registerBleUpdatesReceiver:");
        _unityActivity.registerReceiver(_mGattUpdateReceiver, makeGattUpdateIntentFilter());
    }


    /*
    Public methods that can be directly called by Unity
    */
    public void _InitBLEFramework(final UnityCallback callback)
    {
        System.out.println("Android Executing: _InitBLEFramework");

        if (!_unityActivity.getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE))
        {
            Log.d(TAG,"onCreate: fail: missing FEATURE_BLUETOOTH_LE");
            ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: missing Bluetooth LE");
            callback.sendMessage("Fail: missing Bluetooth LE");
            return;
        }

        final BluetoothManager mBluetoothManager = (BluetoothManager) _unityActivity.getSystemService(Context.BLUETOOTH_SERVICE);
        _mBluetoothAdapter = mBluetoothManager.getAdapter();
        if (_mBluetoothAdapter == null)
        {
            Log.d(TAG,"onCreate: fail: _mBluetoothAdapter is null");
            callback.sendMessage("Fail: missing Bluetooth adapter");
            ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: missing Bluetooth adapter");
            return;
        }
        if (!_mBluetoothAdapter.isEnabled())
        {
            Log.d(TAG,"registerBleUpdatesReceiver: WARNING: _mBluetoothAdapter is not enabled!");
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            _unityActivity.startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
            callback.sendMessage("Fail: Bluetooth is not enabled");
            ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: Bluetooth is not enabled");
            return;
        }

        registerBleUpdatesReceiver();

        Log.d(TAG,"onCreate: _mBluetoothAdapter correctly initialized");
        callback.sendMessage("Success");
        ////UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Success");

    }

    public void _ScanForPeripherals(final UnityCallback callback)
    {
        Log.d(TAG, "_ScanForPeripherals: Launching thread for scanning peripherals");
        new Thread() {
            @Override
            public void run() {
                _searchingDevice = true;
                _mBluetoothAdapter.startLeScan(mLeScanCallback);

                try {
                    Thread.sleep(SCAN_PERIOD);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                _searchingDevice = false;
                _mBluetoothAdapter.stopLeScan(mLeScanCallback);
                // Launch the callback to Unity on the ui thread
                _unityActivity.runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        String message = "Fail: No device found";
                        if (_mDevice.size() > 0) {
                            message = "Success";
                        }
                        Log.d(TAG, message);
                        callback.sendMessage(message);
                    }
                });
            }
        }.start();
    }

    public boolean _IsDeviceConnected()
    {
        Log.d(TAG,"_IsDeviceConnected");
        return _connState;
    }

    public boolean _SearchDeviceDidFinish()
    {
        Log.d(TAG,"_SearchDeviceDidFinish");
        return !_searchingDevice;
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
        }
        else
        {
            jsonListString = "{\"data\":[]}";
        }
        Log.d(TAG, "_GetListOfDevices: sending found devices in JSON: " + jsonListString);
        return jsonListString;
    }

    public boolean _ConnectPeripheralAtIndex(int peripheralIndex, final UnityCallback connectCallback, final UnityCallback disconnectCallback, final UnityCallback receiveDataCallback, final UnityCallback readyForDataCallback)
    {
        boolean success = false;
        Log.d(TAG,"_ConnectPeripheralAtIndex: " + String.valueOf(peripheralIndex));
        BluetoothDevice device = _mDevice.get(peripheralIndex);
        _mDeviceAddress = device.getAddress();
        _mDeviceName = device.getName();

        if (_mDeviceAddress != null && _mDeviceName != null) {
            onBleDidConnect = connectCallback;
            onBleDidDisconnect = disconnectCallback;
            onBleDidReceiveData = receiveDataCallback;
            onBleDidDiscoverService = readyForDataCallback;
            Intent gattServiceIntent = new Intent(_unityActivity, RBLService.class);
            _unityActivity.bindService(gattServiceIntent, _mServiceConnection, Activity.BIND_AUTO_CREATE);
            success = true;
        }
        return success;
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

                Intent gattServiceIntent = new Intent(_unityActivity, RBLService.class);
                _unityActivity.bindService(gattServiceIntent, _mServiceConnection, Activity.BIND_AUTO_CREATE);

                return true;
            }
        }

        return false;
    }

    public byte[] _GetData()
    {
        Log.d(TAG,"_GetData: ");
        return _dataRx;
    }

    public void _SendData(byte[] data)
    {
        Log.d(TAG,"_SendData: ");
        BluetoothGattService gattService = _mBluetoothLeService.getSupportedGattService();
        BluetoothGattCharacteristic characteristicTx = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_TX);
        if (characteristicTx != null) {

            Log.d(TAG, "Set data in the _characteristicTx");
            _dataTx = Arrays.copyOf(data, data.length);

            if (characteristicTx.setValue(_dataTx)) {
                _mBluetoothLeService.writeCharacteristic(characteristicTx);
                BluetoothGattCharacteristic characteristicRx = gattService.getCharacteristic(RBLService.UUID_BLE_SHIELD_RX);
                _mBluetoothLeService.setCharacteristicNotification(characteristicRx,
                        true);
                //_mBluetoothLeService.readCharacteristic(characteristicRx);
            } else {
                Log.d(TAG,"Error: failed setValue on the characteristic");
            }

        } else {
            Log.d(TAG, "Error: the BluetoothGattCharacteristic is NULL");
        }

    }

    /*
     *  Public static methods
     */
    public static BleFramework getInstance(Activity activity)
    {
        if (_instance == null )
        {
            synchronized (BleFramework.class)
            {
                if (_instance == null)
                {
                    Log.d(TAG, "BleFramework: Creation of _instance");
                    _instance = new BleFramework(activity);
                }
            }
        }

        return _instance;
    }
}
