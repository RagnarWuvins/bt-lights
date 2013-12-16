package bneumann.meisterlampe;

import java.util.Set;
import android.os.Build;
import android.os.Bundle;
import android.preference.PreferenceActivity;
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
import android.annotation.TargetApi;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ClipData.Item;
import android.content.SharedPreferences;

public class SetupActivity extends PreferenceActivity
{
	public static final String SHARED_PREFERENCES = "shared_prefs_file";
	public static final String CONNECT_ON_STARTUP = "connect_at_startup";
	public static final String NUM_OF_CHANNELS = "num_of_channels";
	public static final String DEFAULT_DEVICE_ADDRESS = "default_device";
	public static final String DEFAULT_DEVICE_NAME = "default_device_name";
	public static final String FIRST_TIME_STARTUP = "first_time_start";

	private ListArrayAdapter mListAdapter;
	private DropDownArrayAdapter mDropAdapter;
	private BluetoothAdapter mBtAdapter;
	private BluetoothService mBtService;
	private BluetoothDeviceModel mDefaultDevice;
	
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
		
		if(Build.VERSION.SDK_INT < Build.VERSION_CODES.HONEYCOMB)
		{
			this.initFroyo();
		}
		else
		{
			this.initHoneyComb();
		}
		
		this.mBtAdapter = BluetoothAdapter.getDefaultAdapter();
		this.mBtService = new BluetoothService(getApplicationContext());
		
		this.mListAdapter = new ListArrayAdapter(getApplicationContext());
		this.mDropAdapter = new DropDownArrayAdapter(getApplicationContext());

		ListView lv = (ListView) findViewById(android.R.id.list);
		lv.setAdapter(mListAdapter);

		// setup on item click listener to identify the BT device we want to add
		lv.setOnItemClickListener(new OnItemClickListener()
		{
			public void onItemClick(AdapterView<?> parent, View view, int position, long id)
			{
				BluetoothDeviceModel bdm = (BluetoothDeviceModel) parent.getItemAtPosition(position);
				// Cancel discovery to be ready for pairing
				mBtAdapter.cancelDiscovery();
				BluetoothDevice bd = mBtAdapter.getRemoteDevice(bdm.getAddress());
				Toast.makeText(getApplicationContext(), "BondState: " + bd.getBondState(), Toast.LENGTH_SHORT).show();
				switch (bd.getBondState())
				{
				case BluetoothDevice.BOND_BONDED:
					activateBondedDevice(bd);
					break;
				case BluetoothDevice.BOND_NONE:
					bondAndActivateDevice(bd);
					break;
				case BluetoothDevice.BOND_BONDING:
				default:
					break;
				}
			}
		});

		SetupSpinner sp = (SetupSpinner) findViewById(R.id.SetupDevice);
		sp.setAdapter(mDropAdapter);
		this.listPairedDevices();

		if(this.mDropAdapter.getPositionByString(this.mDefaultDevice.getName()) >= 0)
		{
			sp.setSelection(this.mDropAdapter.getPositionByString(this.mDefaultDevice.getName()));
		}		
		
		// Register the BroadcastReceiver
		IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		registerReceiver(mReceiver, filter);

	}
	
	protected void activateBondedDevice(BluetoothDevice bd)
	{		
		SharedPreferences settings = getSharedPreferences(SHARED_PREFERENCES, MODE_PRIVATE);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(DEFAULT_DEVICE_NAME, bd.getName());
		editor.putString(DEFAULT_DEVICE_ADDRESS, bd.getAddress());
		editor.commit();
		this.mBtService.connect(bd, false);
	}

	protected void bondAndActivateDevice(BluetoothDevice bd)
	{
		SharedPreferences settings = getSharedPreferences(SHARED_PREFERENCES, MODE_PRIVATE);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(DEFAULT_DEVICE_NAME, bd.getName());
		editor.putString(DEFAULT_DEVICE_ADDRESS, bd.getAddress());
		editor.commit();
		this.mBtService.connect(bd, false);
	}

	@TargetApi(Build.VERSION_CODES.FROYO)	
	private void initFroyo()
	{		
		setContentView(R.layout.setup);
		
		// Get settings:
		SharedPreferences sharedPrefs = this.getSharedPreferences(SHARED_PREFERENCES, MODE_PRIVATE);
		mDefaultDevice = new BluetoothDeviceModel(sharedPrefs.getString(DEFAULT_DEVICE_NAME, ""), sharedPrefs.getString(DEFAULT_DEVICE_ADDRESS, ""));
		
	}
	
	@TargetApi(Build.VERSION_CODES.HONEYCOMB)
	private void initHoneyComb()
	{
		// Display the fragment as the main content.
		getFragmentManager().beginTransaction().replace(android.R.id.content, new SetupFragment()).commit();
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

		this.mListAdapter.clear();

		boolean state = this.mBtAdapter.startDiscovery();
		if (state)
		{
			Toast.makeText(this, "Scanning has been startetd", Toast.LENGTH_SHORT).show();
		}
	}

	public void listPairedDevices()
	{
		Set<BluetoothDevice> pairedDevices = this.mBtAdapter.getBondedDevices();
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
		}
		else
		{
			BluetoothDeviceModel bdm = new BluetoothDeviceModel("No devices paired yet.", "Please pair first.");
			this.mDropAdapter.add(bdm);
		}		
	}

}
