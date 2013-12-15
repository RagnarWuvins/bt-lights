package bneumann.meisterlampe;

import java.util.Set;
import android.os.Bundle;
import android.view.View;
import android.view.animation.Animation;
import android.view.animation.AnimationUtils;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.Toast;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ClipData.Item;

public class SetupActivity extends Activity
{
	private ListArrayAdapter mListAdapter;
	private DropDownArrayAdapter mDropAdapter;
	private BluetoothAdapter btAdapter;
	
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
				BluetoothDeviceModel bdm = new BluetoothDeviceModel(device.getName(), device.getAddress());
				mListAdapter.add(bdm);
			}
		}
	};
	

	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.setup);

		this.btAdapter = BluetoothAdapter.getDefaultAdapter();

		this.mListAdapter = new ListArrayAdapter(getApplicationContext());
		this.mDropAdapter = new DropDownArrayAdapter(getApplicationContext());
		
		ListView lv = (ListView) findViewById(R.id.SetupDeviceList);
		lv.setAdapter(mListAdapter);
		lv.setOnItemClickListener(new OnItemClickListener()
		{
			public void onItemClick(AdapterView<?> parent, View view, int position, long id)
			{
				BluetoothDeviceModel bdm = (BluetoothDeviceModel) parent.getItemAtPosition(position);
				// Cancel discovery to be ready for pairing
				btAdapter.cancelDiscovery();
				BluetoothDevice bd = btAdapter.getRemoteDevice(bdm.getAddress());
				Toast.makeText(getApplicationContext(), "BondState: " + bd.getBondState(), Toast.LENGTH_SHORT).show();
			}
		});

		SetupSpinner sp = (SetupSpinner) findViewById(R.id.SetupDevice);
		sp.setAdapter(mDropAdapter);

		this.listPairedDevices();
		
		this.mListAdapter.add(mDropAdapter.getItem(0));

		// Register the BroadcastReceiver
		IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		registerReceiver(mReceiver, filter);
	}

	@Override
	protected void onDestroy()
	{
		unregisterReceiver(mReceiver);
		super.onDestroy();
	}

	public void onScanClick(View view)
	{
		Animation onClickAnim = AnimationUtils.loadAnimation(this, R.anim.onclick);
		view.startAnimation(onClickAnim);

		this.mListAdapter = new ListArrayAdapter(getApplicationContext());

		boolean state = this.btAdapter.startDiscovery();
		if (state)
		{
			Toast.makeText(this, "Scanning has been startetd", Toast.LENGTH_SHORT).show();
		}
	}

	public void listPairedDevices()
	{
		Set<BluetoothDevice> pairedDevices = this.btAdapter.getBondedDevices();
		// If there are paired devices
		if (pairedDevices.size() > 0)
		{
			// Loop through paired devices
			for (BluetoothDevice device : pairedDevices)
			{
				// Add the name and address to an array adapter to show in a
				// ListView
				BluetoothDeviceModel bdm = new BluetoothDeviceModel(device.getName(), device.getAddress());
				this.mDropAdapter.add(bdm);
			}
		} else
		{
			BluetoothDeviceModel bdm = new BluetoothDeviceModel("No devices paired yet.", "Please pair first.");
			this.mDropAdapter.add(bdm);
		}
	}

}
