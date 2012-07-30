package bneumann.meisterlampe;

import java.lang.reflect.Array;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.UUID;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.database.DataSetObserver;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.preference.Preference;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.ListAdapter;
import android.widget.ListView;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.TextView;
import android.widget.Toast;

public class MLStartupActivity extends Activity implements OnSeekBarChangeListener
{
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
	public static ArrayList<String> errorLog = new ArrayList<String>();
	private MLBluetoothService mChatService;
	private static byte[] mInRxBuffer = new byte[1024];
	private static int mInIndex = 0;
	protected String EXTRA_DEVICE_ADDRESS;
	private boolean mOnOffFlag = true;
	private static int REQUEST_NUMBER = 0;
	private MLStartupActivity context;
	private ProgressDialog dialog;
	private Intent errorLogIntent = null;
	public static final String NEW_LOG_ENTRY = "new_log_entry";
	public static final String RESET_HARDWARE = "reset_hardware";
	private SeekBar bar;
	
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
			if (action.equals(MLErrorLog.READER_READY))
			{
				triggerAction(0);
			}
			if (action.equals(RESET_HARDWARE))
			{
				triggerAction(1);
			}
		}
	};
	

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		context = this;
		
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
		bar = (SeekBar)findViewById(R.id.valuebar); // make seekbar object
        bar.setOnSeekBarChangeListener(this); // set seekbar listener.
	}

	public void onStart()
	{
		super.onStart();
		mInIndex = 0;
		// start request to enable bluetooth
		if (mBtAdapter.isEnabled())
		{
			setupML();
		} else
		{
			Intent enabler = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			startActivityForResult(enabler, REQUEST_ENABLE);
			return;
		}

	}

	public void onDestroy()
	{
		// important: unregister the Receiver for other purposes
		unregisterReceiver(mReceiver);
		super.onDestroy();
		if (mChatService != null)
		{
			mChatService.stop();
		}
	}
	
	public void onResume()
	{
		super.onResume();
		if (dialog != null)
		{
			dialog.dismiss();
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.activity_startup_menu, menu);
		return true;
	}
	
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
	    // Handle item selection
	    switch (item.getItemId()) {
	        case R.id.menu_error_log:
	    		dialog = ProgressDialog.show(this, "", "Loading. Please wait...", true, true);
	    		errorLog = new ArrayList<String>();
	    		errorLogIntent = new Intent(context, MLErrorLog.class);					
	    		startActivity(errorLogIntent);
	            return true;
	        case R.id.menu_reset:
	        	AlertDialog.Builder builder = new AlertDialog.Builder(this);
	        	builder.setMessage("Are you sure you want to reset?")
	        	       .setCancelable(false)
	        	       .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
	        	           public void onClick(DialogInterface dialog, int id) {
	        	        	   Intent i = new Intent();
	        	        	   i.setAction(MLStartupActivity.RESET_HARDWARE);
	        	        	   sendBroadcast(i);
	        	        	   dialog.cancel();
	        	           }
	        	       })
	        	       .setNegativeButton("No", new DialogInterface.OnClickListener() {
	        	           public void onClick(DialogInterface dialog, int id) {
	        	                dialog.cancel();
	        	           }
	        	       });
	        	AlertDialog alert = builder.create();
	        	alert.show();
	        	return true;
	        default:
	        	Toast.makeText(this, "Nothing here yet", Toast.LENGTH_SHORT).show();
	            return super.onOptionsItemSelected(item);
	    }
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
				setupML();
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

//	@Override
//    public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
//    	// TODO Auto-generated method stub
//
//		SeekBar bar = (SeekBar)findViewById(R.id.valuebar); // make seekbar object
//    	// change progress text label with current seekbar value
//    	Log.i(TAG, "Value of slider: " + progress);
//
//    }
	
	public void setupML()
	{
		showDevices(mBtAdapter.getBondedDevices());
		IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		registerReceiver(mReceiver, filter);
		
		filter = new IntentFilter(MLErrorLog.READER_READY);
		registerReceiver(mReceiver, filter);
		
		filter = new IntentFilter(RESET_HARDWARE);
		registerReceiver(mReceiver, filter);
		
		// Initialize the MLBluetoothService to perform bluetooth connections
		mChatService = new MLBluetoothService(this, mHandler);

		if (!MLSettings.getString("MLAddress", "").isEmpty())
		{
			BluetoothDevice device = mBtAdapter.getRemoteDevice(MLSettings.getString("MLAddress", ""));
			mChatService.connect(device, true);
		} else
		{
			// start to discover bluetooth devices
			mBtAdapter.startDiscovery();
			devices.clear();
			deviceAdresses.clear();
			showDevices(devices);
		}
	}

	public void start_callback(View v)
	{
		byte[] message = null;
		byte[] onCommand = { 0x02, -1, -1, 0x00, 0x02, 0x0D, 0x0A };
		byte[] offCommand = { 0x03, -1, -1, 0x00, 0x03, 0x0D, 0x0A };
		byte[] getCCCommand = { 0x10, -1, -1, 0x00, 0x10, 0x0D, 0x0A };
		
		if (mOnOffFlag)
		{
			message = offCommand;
		}

		else
		{
			message = onCommand;
		}

		mOnOffFlag = !mOnOffFlag;

		sendMessage(message);		
		sendMessage(getCCCommand);
	}

	public void help_callback(View v)
	{		
		triggerAction(2);
	}
		
	public void quit_callback(View v)
	{
		finish();
	}

	public void triggerAction(int actionID)
	{
		if (actionID == 0)
		{
			byte[] getErrorLog = { 0x13, -1, -1, 0x00, 0x13, 0x0D, 0x0A };
			sendMessage(getErrorLog);			
		}
		if (actionID == 1)
		{
			byte[] doReset = { 0x15, -1, -1, 0x00, 0x15, 0x0D, 0x0A };
			sendMessage(doReset);
		}
		if (actionID == 2)
		{
			byte[] setAllToSweep = { 0x4, -1, -1, 0x03, 0x7, 0x0D, 0x0A };
			sendMessage(setAllToSweep);
		}
	}
	
	/**
	 * Sends a message.
	 * 
	 * @param message
	 *            A string of text to send.
	 */
	private void sendMessage(String message)
	{
		// Check that we're actually connected before trying anything
		if (mChatService.getState() != MLBluetoothService.STATE_CONNECTED)
		{
			Toast.makeText(this, R.string.not_connected, Toast.LENGTH_SHORT).show();
			return;
		}

		// Check that there's actually something to send
		if (message.length() > 0)
		{
			// Get the message bytes and tell the MLBluetoothService to write
			byte[] send = message.getBytes();
			mChatService.write(send);

		}
	}

	private void sendMessage(byte[] message)
	{
		// Check that we're actually connected before trying anything
		if (mChatService.getState() != MLBluetoothService.STATE_CONNECTED)
		{
			Toast.makeText(this, R.string.not_connected, Toast.LENGTH_SHORT).show();
			return;
		}
		REQUEST_NUMBER = message[0] & 0x0F;
		// Check that there's actually something to send
		if (message.length > 0)
		{
			mChatService.write(message);

		}
	}

	// The Handler that gets information back from the MLBluetoothService
	private final Handler mHandler = new Handler()
	{
		@Override
		public void handleMessage(Message msg)
		{
			if (dialog != null)
			{
				dialog.dismiss();
			}
			switch (msg.what)
			{
			case MESSAGE_STATE_CHANGE:
				Log.i(TAG, "MESSAGE_STATE_CHANGE: " + msg.arg1);
				switch (msg.arg1)
				{
				case MLBluetoothService.STATE_CONNECTED:
					setOutputText(getResources().getString(R.string.connected));
					break;
				case MLBluetoothService.STATE_CONNECTING:
					setOutputText(getResources().getString(R.string.not_connected));
					break;
				case MLBluetoothService.STATE_LISTEN:
				case MLBluetoothService.STATE_NONE:
					setOutputText(getResources().getString(R.string.none));
					break;
				}
				break;
			case MESSAGE_WRITE:
				byte[] writeBuf = (byte[]) msg.obj;
				break;
			case MESSAGE_READ:
				byte[] readBuf = (byte[]) msg.obj;				
				if (REQUEST_NUMBER == 3)
				{
					Log.i(TAG, "Incoming error log data");
					long errorEntry = byteToLong(Arrays.copyOfRange(readBuf, 0, readBuf.length - 2));
					long sysTime = (errorEntry & 0x00FFF);
					long fwError = (errorEntry & 0xFF000) >> 12;
					errorLog.add("Error number: " + fwError + "\nOccured after: " + sysTime);
					//showDevices(errorLog);
					
					Intent i = new Intent();
					i.setAction(NEW_LOG_ENTRY);
					i.putExtra("EXTRA_ERROR_LOG", errorLog);
					context.sendBroadcast(i);
				} else
				{
					setOutputText("Commands received: " + (long) ((readBuf[3] & 0xFF) * 256 + (readBuf[4] & 0xFF)));

				}
				break;
			case MESSAGE_DEVICE_NAME:
				break;
			case MESSAGE_TOAST:
				break;
			}
		}
	};

	private long byteToLong(byte[] array)
	{
		return byteToLong(array, false);
	}

	private long byteToLong(byte[] array, boolean MSBfirst)
	{
		long value = 0;
		if (MSBfirst)
		{
			for (int i = 0; i < array.length; i++)
			{
				value += ((long) array[i] & 0xffL) << (8 * i);
			}
		} else
		{
			for (int i = 0; i < array.length; i++)
			{
				value |= (array[i] & 0xff) << (4 * (array.length - i - 1));
				// value = (value << 8) + (array[i] & 0xff);
			}

		}
		return value;
	}

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
			EXTRA_DEVICE_ADDRESS = deviceAdresses.get(arg2);
			setOutputText(EXTRA_DEVICE_ADDRESS);
			BluetoothDevice device = mBtAdapter.getRemoteDevice(EXTRA_DEVICE_ADDRESS);
			setPref("MLAddress", EXTRA_DEVICE_ADDRESS);
			// Attempt to connect to the device
			mChatService.connect(device, false);
		}
	};

	/** Sets some text to the output text line **/
	public void setOutputText(String string)
	{
		TextView outputText = (TextView) findViewById(R.id.outputText);
		outputText.setText(string);
	}

	public void setPref(String name, String value)
	{
		SharedPreferences.Editor editor = MLSettings.edit();
		editor.putString(name, value);
		editor.commit();
	}

	public void setPref(String name, float value)
	{
		SharedPreferences.Editor editor = MLSettings.edit();
		editor.putFloat(name, value);
		editor.commit();
	}

	public void setPref(String name, Boolean value)
	{
		SharedPreferences.Editor editor = MLSettings.edit();
		editor.putBoolean(name, value);
		editor.commit();
	}

	public void setPref(String name, int value)
	{
		SharedPreferences.Editor editor = MLSettings.edit();
		editor.putInt(name, value);
		editor.commit();
	}

	public void setPref(String name, long value)
	{
		SharedPreferences.Editor editor = MLSettings.edit();
		editor.putLong(name, value);
		editor.commit();
	}

	@Override
	public void onStartTrackingTouch(SeekBar arg0)
	{
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onStopTrackingTouch(SeekBar arg0)
	{
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser)
	{
		bar = (SeekBar)findViewById(R.id.valuebar); // make seekbar object
//    	// change progress text label with current seekbar value
    	
    	byte checksum = (byte) ((progress + 0x06) & 0xFF);
    	byte[] message = new byte[]{0x06, -1, -1, (byte)progress, checksum , 0xD, 0xA};
    	Log.i(TAG, "Value of slider: " + progress + "Checksum: " + checksum);
    	sendMessage(message);
		
	}
}