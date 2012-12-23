package bneumann.meisterlampe;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.List;
import java.util.Set;
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
import android.graphics.Color;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Gravity;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.TableLayout;
import android.widget.TableLayout.LayoutParams;
import android.widget.TableRow;
import android.widget.TextView;
import android.widget.Toast;

public class MLStartupActivity extends Activity
{
	private static final int REQUEST_ENABLE = 0;
	private static final String TAG = "MeisterLampe startup";
	private SharedPreferences mSettings;
	public static final int MESSAGE_STATE_CHANGE = 0;
	public static final int MAX_CHANNEL_VALUE = 255;
	public static final int MESSAGE_WRITE = 1;
	public static final int MESSAGE_READ = 2;
	public static final int MESSAGE_TOAST = 3;
	public static final int MESSAGE_DEVICE_NAME = 4;
	public static final String DEVICE_NAME = null;
	public static final String TOAST = null;
	public static int numberOfChannels = 10;
	public BluetoothAdapter mBtAdapter;
	public static ArrayList<String> errorLog = new ArrayList<String>();
	private static MLBluetoothService mMLBluetoothService;
	public static Lamp connectedLamp;
	
	protected String EXTRA_DEVICE_ADDRESS;
	private boolean mOnOffFlag = true;
	public static int REQUEST_NUMBER = 0;
	private Intent errorLogIntent = null;
	private Intent xyTab = null;
	public static final String NEW_LOG_ENTRY = "new_log_entry";
	public static final String RESET_HARDWARE = "reset_hardware";
	private boolean mConnectAtStartup = false;
	private SeekBar bar;
	private Toast mToast; // Toast object to prevent overlapping Toasts
	private ProgressDialog mProgressDialog;
	private String mDefaultDevice;
	private CommandHandler mCommandHandler;
	
	private TableLayout mChannelTable;
	
	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		// load settings
		loadSettings();
		connectedLamp = new Lamp(numberOfChannels);

		setContentView(R.layout.main);
		// initialize bluetooth adapter
		mBtAdapter = BluetoothAdapter.getDefaultAdapter();
		if (mBtAdapter == null)
		{
			Log.d(TAG, "This device does not support bluetooth!");
			Toast.makeText(this, "This device does not support bluetooth!", Toast.LENGTH_LONG).show();
			return;
		}
		mToast = Toast.makeText(this, "", Toast.LENGTH_SHORT); // initialize 1
																// Toast and 1
																// Toast only!
		mChannelTable = (TableLayout) findViewById(R.id.channelTable);
		setupChannelTable(mChannelTable);
	}

	public void onStart()
	{
		super.onStart();
		if (!mConnectAtStartup)
		{
			return;
		}
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
		if (mMLBluetoothService != null)
		{
			mMLBluetoothService.stop();
		}
	}

	public void onResume()
	{
		super.onResume();
		loadSettings();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.activity_startup_menu, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		// Handle item selection
		switch (item.getItemId())
		{
		case R.id.menu_error_log:
			errorLog = new ArrayList<String>();
			errorLogIntent = new Intent(getApplicationContext(), MLErrorLog.class);
			startActivity(errorLogIntent);
			return true;
		case R.id.menu_reset:
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.setMessage("Are you sure you want to reset?").setCancelable(false).setPositiveButton("Yes", new DialogInterface.OnClickListener()
			{
				public void onClick(DialogInterface dialog, int id)
				{
					Intent i = new Intent();
					i.setAction(MLStartupActivity.RESET_HARDWARE);
					sendBroadcast(i);
					dialog.cancel();
				}
			}).setNegativeButton("No", new DialogInterface.OnClickListener()
			{
				public void onClick(DialogInterface dialog, int id)
				{
					dialog.cancel();
				}
			});
			AlertDialog alert = builder.create();
			alert.show();
			return true;
		case R.id.menu_settings:
			Intent settingsIntent = new Intent(getApplicationContext(), MLSettings.class);
			startActivity(settingsIntent);
			return true;
		case R.id.menu_reconnect:
			if (mBtAdapter.isEnabled())
			{
				setupML();
			} else
			{
				Intent enabler = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
				startActivityForResult(enabler, REQUEST_ENABLE);
			}
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

	public void setupML()
	{
		mProgressDialog = ProgressDialog.show(this, "", getString(R.string.trying_to_connect), false, true);
		registerReceivers();

		// Initialize the MLBluetoothService to perform bluetooth connections
		if (mMLBluetoothService == null)
		{
			mMLBluetoothService = new MLBluetoothService(this, mHandler);
		}

		if (mDefaultDevice != null)
		{
			BluetoothDevice device = mBtAdapter.getRemoteDevice(mSettings.getString(MLSettings.DEFAULT_DEVICE, ""));

			if (mMLBluetoothService.getState() != mMLBluetoothService.STATE_CONNECTED)
			{
				mMLBluetoothService.connect(device, true);
			}
			else
			{
				mProgressDialog.cancel();
			}
			
		} else
		{
			mToast.setText(R.string.no_default_device_found);
			// start to discover bluetooth devices
			mBtAdapter.startDiscovery();
		}
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
	}
	
	public void setupChannelTable(TableLayout tl)
	{
		int thID = 300;
		int thText = 301;
		TableRow th = new TableRow(this);
		TextView lampText = new TextView(this);
		boolean trsExist = false;
		
		if (findViewById(thID) == null)
		{
			th.setLayoutParams(new LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.WRAP_CONTENT));
			th.setId(thID);
			lampText.setId(thText);			
			th.addView(lampText);	
			tl.addView(th, new TableLayout.LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.WRAP_CONTENT));
		}
		else
		{
			th = (TableRow) findViewById(thID);
			lampText = (TextView) findViewById(thText);
			trsExist = true;
		}
		
		Date date = new Date((long)(connectedLamp.SysTime*1000));
		date.setHours(date.getHours() - 1);
		String formattedDate = new SimpleDateFormat("HH:mm:ss").format(date);
		
		lampText.setText(String.format("Commands: %d, Time: %s, Version: %d.%d\n----------------------------",
				 connectedLamp.CommandCounter, formattedDate, connectedLamp.HWVersion, connectedLamp.HWBuild));
		lampText.setTextColor(Color.WHITE);
		
		for(int i = 0; i < numberOfChannels; i++ )
		{
			Lamp.Channel ls = connectedLamp.channels[i];
			
			int trID = 100 + i;
			int cvID = 200 + i;
			
			TableRow tr = new TableRow(this);			
			TextView channelText = new TextView(this);
			if (!trsExist)
			{
				/* Create a new row to be added. */
				tr.setLayoutParams(new LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.WRAP_CONTENT));
				tr.setId(trID);
				channelText.setId(cvID);
			}
			else
			{
				tr = (TableRow) findViewById(trID);
				channelText = (TextView) findViewById(cvID);				
			}			
			
			channelText.setText(String.format("Channel: %d, Mode: %d, Value: %d\nMax: %d, Min: %d, Delay: %d\nOffset: %d, Rise: %d Period: %d",
					ls.ID, ls.mode, ls.value, ls.max, ls.min, ls.delay, ls.rise, ls.offset, ls.period));
			channelText.setTextColor(Color.WHITE);

			if(!trsExist)
			{
				tr.addView(channelText);			
				/* Add row to TableLayout. */				
				tl.addView(tr, new TableLayout.LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.WRAP_CONTENT));
			}
		}
	}
	

	private void registerReceivers()
	{
		IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(MLErrorLog.READER_READY);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(RESET_HARDWARE);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(MLCoordView.SET_CHANNEL_VALUE);
		registerReceiver(mReceiver, filter);
		
		filter = new IntentFilter(CommandHandler.CH_SETTING_RECEIVED);
		registerReceiver(mReceiver, filter);
		
	}

	public void start_callback(View v)
	{
		if (mOnOffFlag)
		{
			mCommandHandler.AllOff();
		}

		else
		{
			mCommandHandler.AllOn();
		}

		mOnOffFlag = !mOnOffFlag;
		mCommandHandler.GetCommandCounter();
	}

	public void xyTab_callback(View v)
	{
		xyTab = new Intent(this, MLCoordView.class);
		startActivity(xyTab);
	}

	public void help_callback(View v)
	{
		triggerAction(2, null);
	}

	public void quit_callback(View v)
	{
		triggerAction(3, null);
	}

	public void readout_callback(View v)
	{
		mCommandHandler.ReadOut();
	}
	
	public void triggerAction(int actionID, Bundle extras)
	{
		if (actionID == 0)
		{
			byte[] getErrorLog = { 0x01, 0x03, -1, -1, 0x00, 0x01, 0x0D, 0x0A };
			mMLBluetoothService.sendMessage(getErrorLog);
		}
		if (actionID == 1)
		{
			byte[] doReset = { 0x01, 0x05, -1, -1, 0x00, 0x01, 0x0D, 0x0A };
			mMLBluetoothService.sendMessage(doReset);
		}
		if (actionID == 2)
		{
			byte[] setAllToSweep = { 0x00, 0x00, -1, -1, 0x04, 0x4, 0x0D, 0x0A };
			mMLBluetoothService.sendMessage(setAllToSweep);
		}
		if (actionID == 3)
		{
			byte[] getCommandCounter = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x0D, 0x0A };
			mMLBluetoothService.sendMessage(getCommandCounter);
		}
	}


	// The Handler that gets information back from the MLBluetoothService
	private final Handler mHandler = new Handler()
	{
		@Override
		public void handleMessage(Message msg)
		{
			switch (msg.what)
			{			
			case MESSAGE_STATE_CHANGE:
				int state = msg.arg1;
				Log.i(TAG, "MESSAGE_STATE_CHANGE: " + state);
				switch (state)
				{
				case MLBluetoothService.STATE_CONNECTED:
					setOutputText(getResources().getString(R.string.connected));
					mProgressDialog.dismiss();
					break;
				case MLBluetoothService.STATE_CONNECTING:
					setOutputText(getResources().getString(R.string.not_connected));
					break;
				case MLBluetoothService.STATE_LISTEN:
				case MLBluetoothService.STATE_NONE:
					setOutputText(getResources().getString(R.string.none));
					mProgressDialog.dismiss();
					break;
				}
				break;			
			case MESSAGE_WRITE:
				byte[] writeBuf = (byte[]) msg.obj;
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


	/** Sets some text to the output text line **/
	public void setOutputText(String string)
	{
		Log.d(TAG, string);
		TextView outputText = (TextView) findViewById(R.id.outputText);
		outputText.setText(string);
	}

	public void loadSettings()
	{
		mSettings = PreferenceManager.getDefaultSharedPreferences(this);
		// TODO: this might be an android issue?!
		numberOfChannels = Integer.parseInt(mSettings.getString(MLSettings.NUM_OF_CHANNELS, "10"));
		mConnectAtStartup = mSettings.getBoolean(MLSettings.CONNECT_ON_STARTUP, false);
		mDefaultDevice = mSettings.getString(MLSettings.DEFAULT_DEVICE, null);
	}

	public void setPref(String name, String value)
	{
		SharedPreferences.Editor editor = mSettings.edit();
		editor.putString(name, value);
		editor.commit();
	}

	public void setPref(String name, float value)
	{
		SharedPreferences.Editor editor = mSettings.edit();
		editor.putFloat(name, value);
		editor.commit();
	}

	public void setPref(String name, Boolean value)
	{
		SharedPreferences.Editor editor = mSettings.edit();
		editor.putBoolean(name, value);
		editor.commit();
	}

	public void setPref(String name, int value)
	{
		SharedPreferences.Editor editor = mSettings.edit();
		editor.putInt(name, value);
		editor.commit();
	}

	public void setPref(String name, long value)
	{
		SharedPreferences.Editor editor = mSettings.edit();
		editor.putLong(name, value);
		editor.commit();
	}

	private final BroadcastReceiver mReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			final String action = intent.getAction();
			Bundle extras = intent.getExtras();
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
					mMLBluetoothService.stop();
					break;
				case BluetoothAdapter.STATE_ON:
					setOutputText("Bluetooth on");
					break;
				case BluetoothAdapter.STATE_TURNING_ON:
					setOutputText("Turning Bluetooth on...");
					mMLBluetoothService.start();
					break;
				}
			}
			if (action.equals(BluetoothDevice.ACTION_FOUND))
			{
				
			}
			if (action.equals(MLErrorLog.READER_READY))
			{
				triggerAction(0, extras);
			}
			if (action.equals(MLCoordView.SET_CHANNEL_VALUE))
			{
				triggerAction(3, extras);
			}
			if (action.equals(RESET_HARDWARE))
			{
				triggerAction(1, extras);
			}
			if(action.equals(CommandHandler.CH_SETTING_RECEIVED))
			{
				setupChannelTable(mChannelTable);
			}
		}
	};

	
	public static MLBluetoothService getBluetoothService()
	{
		return mMLBluetoothService;
	}
}