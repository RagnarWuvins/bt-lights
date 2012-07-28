package bneumann.meisterlampe;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.UUID;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.database.DataSetObserver;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.ListAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

public class MLStartupActivity extends Activity
{
	private static UUID ML_UUID = UUID.fromString("5c7b4740-d6a0-11e1-9b23-0800200c9a66"); // generated
																							// with
																							// some
																							// internet
																							// UUID
																							// generator
	private static final int REQUEST_ENABLE = 0;
	private static final String TAG = "MeisterLampe startup";
	public static final int MESSAGE_STATE_CHANGE = 0;
	public static final int MESSAGE_WRITE = 1;
	public static final int MESSAGE_READ = 2;
	public static final int MESSAGE_TOAST = 3;
	public static final int MESSAGE_DEVICE_NAME = 4;
	public static final String DEVICE_NAME = null;
	public static final String TOAST = null;
	public SharedPreferences MLSettings;
	public BluetoothAdapter mBtAdapter;
	public static ArrayList<String> devices = new ArrayList<String>();
	public static ArrayList<String> deviceAdresses = new ArrayList<String>();
	private final BroadcastReceiver mReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			final String action = intent.getAction();

			if (action.equals(BluetoothAdapter.ACTION_STATE_CHANGED))
			{
				final int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR);
				switch (state)
				{
				case BluetoothAdapter.STATE_OFF:
					setOutputText("Bluetooth off");
					break;
				case BluetoothAdapter.STATE_TURNING_OFF:
					setOutputText("Turning Bluetooth off...");
					break;
				case BluetoothAdapter.STATE_ON:
					setOutputText("Bluetooth on");
					break;
				case BluetoothAdapter.STATE_TURNING_ON:
					setOutputText("Turning Bluetooth on...");
					break;
				}
			}
			if (action.equals(BluetoothDevice.ACTION_FOUND))
			{
				BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
				String bondState = device.getBondState() == BluetoothDevice.BOND_BONDED ? "Paired" : "Unpaired";
				String devString = device.getName() + " (" + bondState + ")" + "\n" + device.getAddress();
				if (!deviceAdresses.contains(device.getAddress()))
				{
					deviceAdresses.add(device.getAddress());
					devices.add(devString);
					showDevices(devices);
				}
			}
		}
	};

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		// load settings
		MLSettings = PreferenceManager.getDefaultSharedPreferences(this);
		setContentView(R.layout.main);
		// initialize bluetooth adapter
		mBtAdapter = BluetoothAdapter.getDefaultAdapter();
		if (mBtAdapter == null)
		{
			Log.d(TAG, "This device does not support bluetooth!");
			Toast.makeText(this, "This device does not support bluetooth!", Toast.LENGTH_LONG).show();
			return;
		}

	}

	public void onStart()
	{
		super.onStart();
		// start request to enable bluetooth
		if (!mBtAdapter.isEnabled())
		{
			Intent enabler = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			startActivityForResult(enabler, REQUEST_ENABLE);
		}
		showDevices(mBtAdapter.getBondedDevices());
		IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		registerReceiver(mReceiver, filter);
		// start to discover bluetooth devices
		mBtAdapter.startDiscovery();
		devices.clear();
		deviceAdresses.clear();
		showDevices(devices);		

	}

	public void onDestroy()
	{
		// important: unregister the Receiver for other purposes
		unregisterReceiver(mReceiver);
		super.onDestroy();
	}

	/** Called when the activity resumes from another task (subactivity) **/
	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		switch (requestCode)
		{
		case REQUEST_ENABLE:
			switch (resultCode)
			{
			case RESULT_OK:
				break;
			case RESULT_CANCELED:
				break;
			default:
				Log.d(TAG, "Some weird resultCode came back: " + resultCode);
				break;
			}
			break;
		default:
			break;
		}
	}

	// The Handler that gets information back from the BluetoothChatService
	private final Handler mHandler = new Handler()
	{
		@Override
		public void handleMessage(Message msg)
		{
			switch (msg.what)
			{
			case MESSAGE_STATE_CHANGE:
				Log.i(TAG, "MESSAGE_STATE_CHANGE: " + msg.arg1);
				break;
			default:
				Log.i(TAG, "MESSAGE_STATE_CHANGE: " + msg.arg1);
				break;
			}
		}
	};

	private void showDevices(List<String> devices)
	{
		ListView outputList = (ListView) findViewById(R.id.outputList);
		outputList.setClickable(true);
		outputList.setOnItemClickListener(myClickListener);
		outputList.setAdapter(new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, devices));
	}

	private void showDevices(Set<BluetoothDevice> devices)
	{

		List<String> deviceList = new ArrayList<String>();
		for (BluetoothDevice bt : devices)
		{
			deviceList.add(bt.getName() + "\n" + bt.getAddress());
			deviceAdresses.add(bt.getAddress());
		}
		showDevices(deviceList);
	}

	public OnItemClickListener myClickListener = new OnItemClickListener()
	{

		@Override
		public void onItemClick(AdapterView<?> arg0, View arg1, int arg2, long arg3)
		{
			mBtAdapter.cancelDiscovery();
			String test = deviceAdresses.get(arg2);
			setOutputText(test);
		}
	};

	/** Sets some text to the output text line **/
	public void setOutputText(String string)
	{
		TextView outputText = (TextView) findViewById(R.id.outputText);
		outputText.setText(string);
	}
}