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
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;
//import android.os.ParcelUuid;
import android.bluetooth.BluetoothManager;
import android.app.Activity;

public class BleFramework
{
    private static volatile BleFramework instance;
	private Context context;

    public static final String BLEUnityMessageName_OnBleDidInitialize = "OnBleDidInitialize";
    public static final String BLEUnityMessageName_OnBleDidConnect = "OnBleDidConnect";
    public static final String BLEUnityMessageName_OnBleDidCompletePeripheralScan = "OnBleDidCompletePeripheralScan";
    public static final String BLEUnityMessageName_OnBleDidDisconnect = "OnBleDidDisconnect";
    public static final String BLEUnityMessageName_OnBleDidReceiveData = "OnBleDidReceiveData";

    private final static String TAG = BleFramework.class.getSimpleName();

    private RBLService mBluetoothLeService;
    private BluetoothAdapter mBluetoothAdapter;
    public static List<BluetoothDevice> mDevice = new ArrayList<BluetoothDevice>();

    private static final int REQUEST_ENABLE_BT = 1;
    private static final long SCAN_PERIOD = 3000;
    public static final int REQUEST_CODE = 30;
    private String mDeviceAddress;
    private String mDeviceName;
    private boolean flag = true;
    private boolean connState = false;

    private final ServiceConnection mServiceConnection = new ServiceConnection() 
    {
        @Override
        public void onServiceConnected(ComponentName componentName, IBinder service) 
        {
            mBluetoothLeService = ((RBLService.LocalBinder) service).getService();
            if (!mBluetoothLeService.initialize()) 
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
            mBluetoothLeService = null;
        }
    };

    private BluetoothAdapter.LeScanCallback mLeScanCallback = new BluetoothAdapter.LeScanCallback() 
    {
        @Override
        public void onLeScan(final BluetoothDevice device, final int rssi, byte[] scanRecord) 
        {
            Activity activity = (Activity) context;
            activity.runOnUiThread(new Runnable() {
                @Override
                public void run() 
                {
                    Log.d(TAG, "onLeScan: run()");
                    if (device != null) 
                    {
                        Log.d(TAG, "onLeScan: device is not null");
                        if (mDevice.indexOf(device) == -1)
                        {
                            Log.d(TAG, "onLeScan: add device to mDevice");
                            mDevice.add(device);
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

    private void startReadRssi() 
    {
        new Thread() 
        {
            public void run() 
            {
                while (flag) 
                {
                    mBluetoothLeService.readRssi();
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

    private final BroadcastReceiver mGattUpdateReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();

            if (RBLService.ACTION_GATT_CONNECTED.equals(action)) {
                flag = true;
                connState = true;

                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidConnect, "Success");

                Log.d(TAG, "Connection estabilished with: " + mDeviceAddress);
                startReadRssi();
            } 
            else if (RBLService.ACTION_GATT_DISCONNECTED.equals(action)) 
            {
                flag = false;
                connState = false;

                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidDisconnect, "Success");

                Log.d(TAG, "Connection lost");
            } 
            else if (RBLService.ACTION_GATT_RSSI.equals(action)) 
            {
                String rssiData = intent.getStringExtra(RBLService.EXTRA_DATA);
                Log.d(TAG, "RSSI: " + rssiData);
            }
        }
    };

    private static IntentFilter makeGattUpdateIntentFilter() 
    {
        final IntentFilter intentFilter = new IntentFilter();

        intentFilter.addAction(RBLService.ACTION_GATT_CONNECTED);
        intentFilter.addAction(RBLService.ACTION_GATT_DISCONNECTED);
        intentFilter.addAction(RBLService.ACTION_GATT_RSSI);

        return intentFilter;
    }

    private void RegisterGattUpdateReceiver()
    {
        Activity activity = (Activity) context;
        if (!mBluetoothAdapter.isEnabled()) 
        {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            activity.startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
        }

        activity.registerReceiver(mGattUpdateReceiver, makeGattUpdateIntentFilter());
    }

    private void scanLeDevice() 
    {
        new Thread() 
        {

            @Override
            public void run() 
            {

                if (mBluetoothAdapter == null) 
                {
                    Log.e(TAG, "scanLeDevice: Abort mBluetoothAdapter is null");
                    return;
                }
                else
                {
                    Log.d(TAG, "scanLeDevice: mBluetoothAdapter StartLeScan");
                    mBluetoothAdapter.startLeScan(mLeScanCallback);
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

                Log.d(TAG, "scanLeDevice: mBluetoothAdapter StopLeScan");
                mBluetoothAdapter.stopLeScan(mLeScanCallback);

                Log.d(TAG, "scanLeDevice: mDevice size is " + mDevice.size());

                String mDeviceJson = _GetListOfDevices();
                UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidCompletePeripheralScan, mDeviceJson);
            }
        }.start();
    }

    

    private BleFramework(Context context)
    {
        Log.d(TAG, "BleFramework: Singleton Creation");
        this.context = context;
    }

    public static BleFramework getInstance(Context context) 
    {
        if (instance == null ) 
        {
            synchronized (BleFramework.class) 
            {
                if (instance == null) 
                {
                    Log.d(TAG, "BleFramework: Creation of instance");
                    instance = new BleFramework(context);
                }
            }
        }

        return instance;
    }

    public void _InitBLEFramework()
    {
        System.out.println("Android Executing: _InitBLEFramework");

        if (!context.getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE)) 
        {
            Log.e(TAG,"_InitBLEFramework: FEATURE_BLUETOOTH_LE is not found");
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: FEATURE_BLUETOOTH_LE");
            return;
        }

        Activity activity = (Activity) context;
        final BluetoothManager mBluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
        mBluetoothAdapter = mBluetoothManager.getAdapter();
        
        if (mBluetoothAdapter == null) 
        {
            Log.e(TAG,"_InitBLEFramework: mBluetoothAdapter is null");
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Fail: Context.BLUETOOTH_SERVICE");
        }
        else
        {
            Log.d(TAG,"_InitBLEFramework: mBluetoothAdapter found");
            Intent gattServiceIntent = new Intent(activity, RBLService.class);
            activity.bindService(gattServiceIntent, mServiceConnection, activity.BIND_AUTO_CREATE);
            RegisterGattUpdateReceiver();
            UnityPlayer.UnitySendMessage("BLEControllerEventHandler", BLEUnityMessageName_OnBleDidInitialize, "Success");
        }

        
    }

    public void _ScanForPeripherals()
    {
        if (connState == false) 
        {
            Log.d(TAG, "_ScanForPeripherals: Launching scanLeDevice");
            scanLeDevice();

            /*
            try {
                Thread.sleep(SCAN_PERIOD);
            } catch (InterruptedException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }

            Activity activity = (Activity) context;
            Intent intent = new Intent(context, Device.class);
            activity.startActivityForResult(intent, REQUEST_CODE);
            */
        } 
        else 
        {
            Log.d(TAG, "_ScanForPeripherals: Disconnect and close Ble service");
            mBluetoothLeService.disconnect();
            mBluetoothLeService.close();
        }
    }

    public boolean _IsDeviceConnected()
    {
        Log.d(TAG,"_IsDeviceConnected");
        return connState;
    }

    public boolean _SearchDeviceDidFinish()
    {
        Log.d(TAG,"_SearchDeviceDidFinish");
        return true;
    }

    public String _GetListOfDevices()
    {
        String jsonListString;

        if (mDevice.size() > 0)
        {
            Log.d(TAG,"_GetListOfDevices");
            String[] uuidsArray = new String[mDevice.size()];

            for (int i = 0; i < mDevice.size(); i++) 
            {

                BluetoothDevice bd = mDevice.get(i);
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
        BluetoothDevice device = mDevice.get(peripheralIndex);
        mDeviceAddress = device.getAddress();

        return mBluetoothLeService.connect(mDeviceAddress);
    }

    public boolean _ConnectPeripheral(String peripheralID)
    {
        Log.d(TAG,"_ConnectPeripheral: " + peripheralID);

        for (BluetoothDevice device : mDevice)
        {
            if (device.getAddress().equals(peripheralID)) 
            {
                mDeviceAddress = device.getAddress();
                return mBluetoothLeService.connect(mDeviceAddress);
            }
        }
             
        return false;
    }

    public void _SendData(String data)
    {
        Log.d(TAG,"_SendData: " + data);
    }

    public void _OnPause(boolean pause)
    {
        if (pause == true)
        {
            flag = false;
            Activity activity = (Activity) context;
            activity.unregisterReceiver(mGattUpdateReceiver);
        }
        else
        {
            RegisterGattUpdateReceiver();
        }
    }

    

	// Return the battery level as a float between 0 and 1 (1 being fully charged, 0 fulled discharged)
    /*
    public float GetBatteryPct()
    {
    	Intent batteryStatus = GetBatteryStatusIntent();

    	int level = batteryStatus.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
		int scale = batteryStatus.getIntExtra(BatteryManager.EXTRA_SCALE, -1);

		float batteryPct = level / (float)scale;
		return batteryPct;
    }

    // Return whether or not we're currently on charge
    public boolean IsBatteryCharging()
    {
    	Intent batteryStatus = GetBatteryStatusIntent();

    	int status = batteryStatus.getIntExtra(BatteryManager.EXTRA_STATUS, -1);
    	return status == BatteryManager.BATTERY_STATUS_CHARGING || 
    	       status == BatteryManager.BATTERY_STATUS_FULL;
    }

    private Intent GetBatteryStatusIntent()
    {
 		IntentFilter ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
		return context.registerReceiver(null, ifilter);
    }
    */
}