package bneumann.meisterlampe;

import java.util.Set;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.Bundle;
import android.preference.ListPreference;
import android.preference.PreferenceActivity;
import android.preference.PreferenceManager;
import android.widget.Toast;

public class Settings extends PreferenceActivity implements OnSharedPreferenceChangeListener
{

	public static final String CONNECT_ON_STARTUP = "connect_at_startup";
	public static final String NUM_OF_CHANNELS = "num_of_channels";
	public static final String DEFAULT_DEVICE = "default_device";
	public static final String DEFAULT_DEVICE_NAME = "default_device_name";
	public static final String FIRST_TIME_STARTUP = "first_time_start";

	private ListPreference mDeviceList;
	private CharSequence[] mEntries;
	private CharSequence[] mEntryValues;

	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		addPreferencesFromResource(R.xml.mlsettings);
		getPreferenceScreen().getSharedPreferences().registerOnSharedPreferenceChangeListener(this);
		mDeviceList = (ListPreference) findPreference(DEFAULT_DEVICE);
		BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();
		mEntries = new String[0];
		mEntryValues = new String[0];
		if (btAdapter != null)
		{
			Set<BluetoothDevice> devices = btAdapter.getBondedDevices();
			mEntries = new String[devices.size()];
			mEntryValues = new String[devices.size()];
			int counter = 0;
			for (BluetoothDevice bt : devices)
			{
				mEntries[counter] = bt.getName();
				mEntryValues[counter] = bt.getAddress();
				counter++;
			}
		} else
		{
			mEntries[0] = "Nothing";
			mEntryValues[0] = "here";
		}
		mDeviceList.setEntries(mEntries);
		mDeviceList.setEntryValues(mEntryValues);
		SharedPreferences globalSettings = PreferenceManager.getDefaultSharedPreferences(this);
		mDeviceList.setSummary(globalSettings.getString(DEFAULT_DEVICE_NAME, "No device set yet"));
	}

	public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key)
	{
		if (key.equals(CONNECT_ON_STARTUP))
		{
			Toast.makeText(getApplicationContext(), "The Value is: " + sharedPreferences.getBoolean(key, false), Toast.LENGTH_SHORT).show();
		}

		if (key.equals(NUM_OF_CHANNELS))
		{
			Toast.makeText(getApplicationContext(), "The Value is: " + sharedPreferences.getString(key, "10"), Toast.LENGTH_SHORT).show();
		}
		if (key.equals(DEFAULT_DEVICE))
		{			
			SharedPreferences.Editor ed = sharedPreferences.edit();
			CharSequence defaultDevice = mEntries[mDeviceList.findIndexOfValue(sharedPreferences.getString(key, "10"))];
			ed.putString(DEFAULT_DEVICE_NAME, (String) defaultDevice);
			ed.commit();
			mDeviceList.setSummary(defaultDevice);
		}
	}

}
