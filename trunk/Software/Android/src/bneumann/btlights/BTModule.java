package bneumann.btlights;

import java.util.Set;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.view.View.OnClickListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;

public class BTModule extends Activity
{

	private BluetoothAdapter mBluetoothAdapter;
	private BluetoothDevice mmDevice = null;
	private String searchName;
	private boolean receiverCreated;
	private TextView output1;
	private TextView output2;
	private ArrayAdapter<String> listedDevices;
	private Button connectTarget;
	// static variables
	// Return Intent extra
	public static String EXTRA_DEVICE_ADDRESS = "device_address";
	public static int BT_ERROR = -1;
	public static int BT_SUCCESS = 1;
	public static int BT_DISABLED = 2;
	public static int BT_ENABLED = 3;

	/*
	 * start of override functions
	 */
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_INDETERMINATE_PROGRESS);
		setContentView(R.layout.btmodule);
		mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
		searchName = this.getString(R.string.target);
		mmDevice = null;
		output1 = (TextView) findViewById(R.id.out1);
		output2 = (TextView) findViewById(R.id.out2);
		// setup listview
		listedDevices = new ArrayAdapter<String>(this, R.layout.device_name);
		ListView newDevicesListView = (ListView) findViewById(R.id.bm_devicelist);
		newDevicesListView.setAdapter(listedDevices);
		// setup button
		connectTarget = (Button) findViewById(R.id.bm_connect);
		connectTarget.setOnClickListener(new OnClickListener()
		{
			public void onClick(View v)
			{
				finish();
			}
		});
		// newDevicesListView.setOnItemClickListener(mDeviceClickListener);
		output1.setText("");
		output2.setText("");
		// emulate setup
		// Find and set up the ListView for paired devices
		int status = connect();
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		if (requestCode == BT_ENABLED)
		{
			int status = connect();
			if (status != BT_SUCCESS)
			{

				writeOut("Connect failed");
				this.finish();
			}
		}
	}

	@Override
	protected void onDestroy()
	{
		super.onDestroy();

		// Make sure we're not doing discovery anymore
		if (mBluetoothAdapter != null)
		{
			mBluetoothAdapter.cancelDiscovery();
		}

		// Unregister broadcast listeners
		if (receiverCreated)
		{
			this.unregisterReceiver(mReceiver);
		}
	}

	/*
	 * Class methods
	 */
	public int connect()
	{
		writeOut("Looking for BT adapter");
		int retState = BT_SUCCESS;
		if (!mBluetoothAdapter.isEnabled())
		{
			writeOut("BT not ready, activating");
			Intent enableBluetooth = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			this.startActivityForResult(enableBluetooth, BT_ENABLED);
			return BT_DISABLED;
		}
		writeOut("Looking for target");
		Set<BluetoothDevice> pairedDevices = mBluetoothAdapter.getBondedDevices();
		if (pairedDevices.size() > 0)
		{
			for (BluetoothDevice device : pairedDevices)
			{
				String devName = device.getName();
				if (devName.equals(searchName))
				{
					writeOut("Target found in paried list");
					mmDevice = device;
					break;
				}
			}
			if (mmDevice == null)
			{
				retState = getDevice();
			}
		} else
		{
			retState = getDevice();
		}
		return retState;
	}

	private int getDevice()
	{
		int retState = BT_SUCCESS;
		if (mBluetoothAdapter.startDiscovery())
		{
			writeOut("Starting search for " + this.getText(R.string.target));
			// Register the BroadcastReceiver to look for devices
			IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
			this.registerReceiver(mReceiver, filter);
			// Register the BroadcastReceiver to log when we're done looking
			filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
			this.registerReceiver(mReceiver, filter);
			this.receiverCreated = true;
			retState = BT_SUCCESS;
		} else
		{
			retState = BT_ERROR;
		}
		return retState;
	}

	public void throwError(int errorID)
	{
		Utils.showToast(this, "Error occured with number: " + errorID);
	}

	private void foundTarget(String devName, String devAdd)
	{
		mBluetoothAdapter.cancelDiscovery();
		writeOut("Sending address " + devAdd + " of " + devName + " out");
		// Create the result Intent and include the MAC address
		Intent outIntent = new Intent();
		outIntent.putExtra(EXTRA_DEVICE_ADDRESS, devAdd);

		// Set result and finish this Activity
		setResult(Activity.RESULT_OK, outIntent);
	}

	// Create a BroadcastReceiver for ACTION_FOUND
	private final BroadcastReceiver mReceiver = new BroadcastReceiver()
	{
		public void onReceive(Context context, Intent intent)
		{
			String action = intent.getAction();
			// When discovery finds a device
			if (BluetoothDevice.ACTION_FOUND.equals(action))
			{
				// Get the BluetoothDevice object from the Intent
				BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
				// Add the name and address to an array adapter to show in a
				// ListView
				String devName = device.getName();
				String devAdd = device.getAddress();
				String status = "new";
				if (device.getBondState() == BluetoothDevice.BOND_BONDED)
				{
					status = "paired";
				}
				listedDevices.add(devName + " (" + status + ")");
				if (devName.equals(searchName))
				{
					writeOut("We found: " + searchName);
					foundTarget(devName, devAdd);
				}
			} else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.equals(action))
			{
				writeOut("We're done looking");
			}
		}
	};

	private void writeOut(String text)
	{
		writeOut(text, true);
	}

	private void writeOut(String text, boolean append)
	{
		String oldText = (String) output1.getText();
		String newText = text;
		if (append)
		{
			if (oldText != "")
			{
				newText = oldText + "\n" + text;
			}
		}
		output1.setText(newText);
		output2.setText(text);
	}

}
