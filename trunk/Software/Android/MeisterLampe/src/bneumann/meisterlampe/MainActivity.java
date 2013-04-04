package bneumann.meisterlampe;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.ObjectOutputStream;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import javax.xml.parsers.ParserConfigurationException;
import org.xml.sax.SAXException;
import bneumann.meisterlampe.CommandHandler.COMMANDS;
import bneumann.meisterlampe.CommandHandler.MODE;
import bneumann.meisterlampe.Lamp.AcknowledgeReceivedListener;
import bneumann.meisterlampe.Lamp.Channel;
import bneumann.meisterlampe.Lamp.ChannelPropertyChangeListener;
import bneumann.meisterlampe.MainActivity.MyVisCap;
import android.annotation.SuppressLint;
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
import android.media.audiofx.Visualizer;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.TableLayout;
import android.widget.TableLayout.LayoutParams;
import android.widget.TableRow;
import android.widget.TextView;
import android.widget.Toast;

@SuppressLint("ShowToast")
public class MainActivity extends Activity implements AcknowledgeReceivedListener, ChannelPropertyChangeListener
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
	// public static int numberOfChannels = 10;
	public BluetoothAdapter mBtAdapter;
	public static ArrayList<String> errorLog = new ArrayList<String>();
	private static BluetoothService mMLBluetoothService;
	public static Lamp connectedLamp;

	protected String EXTRA_DEVICE_ADDRESS;
	private int mOnOffFlag = 0;
	public static int REQUEST_NUMBER = 0;
	private Intent errorLogIntent = null;
	private Intent xyTab = null;
	public static final String NEW_LOG_ENTRY = "new_log_entry";
	public static final String RESET_HARDWARE = "reset_hardware";
	private boolean mConnectAtStartup = false;
	private Toast mToast; // Toast object to prevent overlapping Toasts
	private ProgressDialog mProgressDialog;
	private String mDefaultDevice;
	private CommandHandler mCommandHandler;

	private TableLayout mChannelTable;
	private static TextView mOutputText;
	protected boolean reconnectTask = false; // set to true if the system should try to reconnect after STATE_NONE

	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		// load settings
		
		connectedLamp = new Lamp();
		connectedLamp.AddAcknowledgeReceivedListener(this);
		connectedLamp.AddChannelPropertyChangeListener(this);
		loadSettings();

		
		setContentView(R.layout.main);
		mOutputText = (TextView) findViewById(R.id.outputText);

		mToast = Toast.makeText(this, "", Toast.LENGTH_SHORT); // init Toast
		mChannelTable = (TableLayout) findViewById(R.id.channelTable);

		SetupButtons();

		// initialize bluetooth adapter
		mBtAdapter = BluetoothAdapter.getDefaultAdapter();

		if (mBtAdapter == null)
		{
			Log.d(TAG, "This device does not support bluetooth!");
			Toast.makeText(this, "This device does not support bluetooth!", Toast.LENGTH_LONG).show();
			// Setup a commandhandler that doesn't do anything for debug
			mCommandHandler = new CommandHandler(this, null);
			return;
		}
		
		// Visualizer test code:
		Visualizer vis = new Visualizer(0);
		MyVisCap vc = new MyVisCap();
		vis.setDataCaptureListener(vc, 100000, false, true);
		
	}

	public class MyVisCap implements Visualizer.OnDataCaptureListener
	{

		public void onFftDataCapture(Visualizer vis, byte[] fft, int samplingRate)
		{
			// TODO Auto-generated method stub
			Log.i(TAG, fft.toString());
			
		}

		public void onWaveFormDataCapture(Visualizer vis, byte[] waveform, int samplingRate)
		{
			// TODO Auto-generated method stub
			
		}
		
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
			OpenBT();

		}
		else
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
			connectedLamp.errors.clear();
			mCommandHandler.GetErrorLog();
			try
			{
				Thread.sleep(500);
			}
			catch (InterruptedException e)
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			errorLogIntent = new Intent(getApplicationContext(), ErrorView.class);
			startActivity(errorLogIntent);
			return true;
		case R.id.menu_reset:
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.setMessage("Are you sure you want to reset?").setCancelable(false).setPositiveButton("Yes", new DialogInterface.OnClickListener()
			{
				public void onClick(DialogInterface dialog, int id)
				{
					Intent i = new Intent();
					i.setAction(MainActivity.RESET_HARDWARE);
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
			Intent settingsIntent = new Intent(getApplicationContext(), Settings.class);
			startActivity(settingsIntent);
			return true;
		case R.id.menu_reconnect:
			reconnectTask = true;
			if (mBtAdapter.isEnabled())
			{
				mMLBluetoothService.disconnect();
			}
			else
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
				OpenBT();
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

	public void OpenBT()
	{
		mProgressDialog = ProgressDialog.show(this, "", getString(R.string.trying_to_connect), false, true);
		registerReceivers();

		// Initialize the MLBluetoothService to perform bluetooth connections
		if (mMLBluetoothService == null)
		{
			mMLBluetoothService = new BluetoothService(this, mHandler);
		}

		if (mDefaultDevice != null)
		{
			BluetoothDevice device = mBtAdapter.getRemoteDevice(mSettings.getString(Settings.DEFAULT_DEVICE, ""));

			if (mMLBluetoothService.getState() != BluetoothService.STATE_CONNECTED)
			{
				mMLBluetoothService.connect(device, true);
			}
			else
			{
				mProgressDialog.cancel();
			}

		}
		else
		{
			mToast.setText(R.string.no_default_device_found);
			// start to discover bluetooth devices
			mBtAdapter.startDiscovery();
		}
		// create the command handler and setup the interface
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
		mCommandHandler.AddChannelCommandReceivedListener(connectedLamp);
		mCommandHandler.AddGlobalCommandReceivedListener(connectedLamp);
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

		Date date = new Date((long) (connectedLamp.SysTime * 1000));
		date.setHours(date.getHours() - 1);
		String formattedDate = new SimpleDateFormat("HH:mm:ss").format(date);

		lampText.setText(String.format("Commands: %d, Time: %s, Version: %d.%d\n----------------------------", connectedLamp.CommandCounter, formattedDate,
				connectedLamp.HWVersion, connectedLamp.HWBuild));
		lampText.setTextColor(Color.WHITE);

		for (int i = 0; i < connectedLamp.GetNumberOfChannels(); i++)
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

			channelText.setText(String.format("Channel: %d, Mode: %d, Value: %d\nMax: %d, Min: %d, Delay: %d\nOffset: %d, Rise: %d Period: %d", ls.ID, ls.getMode(), ls.getValue(),
					ls.getMax(), ls.getMin(), ls.getDelay(), ls.getRise(), ls.getOffset(), ls.getPeriod()));
			channelText.setTextColor(Color.WHITE);

			if (!trsExist)
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

		filter = new IntentFilter(ErrorView.READER_READY);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(RESET_HARDWARE);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(MLCoordView.SET_CHANNEL_VALUE);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(CommandHandler.CH_SETTING_RECEIVED);
		registerReceiver(mReceiver, filter);

	}

	private void SetupButtons()
	{
		// Setup buttons
		Button TimeControllerButton = (Button) findViewById(R.id.main_xyTab);
		TimeControllerButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				xyTab = new Intent(getApplicationContext(), MLCoordView.class);
				startActivity(xyTab);
			}
		});

		Button ChannelSetupButton = (Button) findViewById(R.id.main_channels);
		ChannelSetupButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				Intent mIntent = new Intent(getApplicationContext(), ChannelOverview.class);
				startActivity(mIntent);
			}
		});

		Button DelayEditorButton = (Button) findViewById(R.id.main_sweepeditor);
		DelayEditorButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				Intent mIntent = new Intent(getApplicationContext(), SweepEditor.class);
				startActivity(mIntent);
			}
		});

		Button SweepButton = (Button) findViewById(R.id.main_help);
		SweepButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				// triggerAction(2, null);
				int i = 0;
				for (Channel ch : connectedLamp.channels)
				{
					ch.setDelay(i++ * 20);
				}
			}
		});

		Button ReadoutButton = (Button) findViewById(R.id.main_readout);
		ReadoutButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				mCommandHandler.ReadOut();
				connectedLamp.SaveData();
			}
		});

		Button QuitButton = (Button) findViewById(R.id.main_quit);
		QuitButton.setOnClickListener(new View.OnClickListener()
		{
			public void onClick(View v)
			{
				// mCommandHandler.GetCommandCounter();
				// mCommandHandler.ResetTimers();
				connectedLamp.SetAllProperty(COMMANDS.CMD_SET_MODE, mOnOffFlag);
				Log.i(TAG, "Flag is now: " + mOnOffFlag);
				if (mOnOffFlag == MODE.Names.length - 1)
				{
					mOnOffFlag = 0;
				}
				else
				{
					mOnOffFlag++;
				}

			}
		});
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
				case BluetoothService.STATE_CONNECTED:
					setOutputText(getResources().getString(R.string.connected));
					try
					{
						Thread.sleep(200);
					}
					catch (InterruptedException e)
					{
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					mProgressDialog.dismiss();
					break;
				case BluetoothService.STATE_CONNECTING:
					setOutputText(getResources().getString(R.string.not_connected));
					break;
				case BluetoothService.STATE_LISTEN:
				case BluetoothService.STATE_NONE:
					setOutputText(getResources().getString(R.string.none));
					try
					{
						Thread.sleep(200);
					}
					catch (InterruptedException e)
					{
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					mProgressDialog.dismiss();
					if (reconnectTask)
					{
						OpenBT();
						reconnectTask = false;
					}
					break;
				}
				break;
			case MESSAGE_WRITE:
				break;
			}
		}
	};

	/** Sets some text to the output text line **/
	private static void setOutputText(String string)
	{
		Log.d(TAG, string);
		mOutputText.setText(string);
	}

	public void loadSettings()
	{
		mSettings = PreferenceManager.getDefaultSharedPreferences(this);
		// TODO: this might be an android issue?!
		connectedLamp.SetNumberOfChannels(Integer.parseInt(mSettings.getString(Settings.NUM_OF_CHANNELS, "10")));
		mConnectAtStartup = mSettings.getBoolean(Settings.CONNECT_ON_STARTUP, false);
		mDefaultDevice = mSettings.getString(Settings.DEFAULT_DEVICE, null);
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
			if (action.equals(ErrorView.READER_READY))
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
		}
	};

	public static BluetoothService getBluetoothService()
	{
		return mMLBluetoothService;
	}

	public void OnAcknowledgeReceived()
	{
		setupChannelTable(mChannelTable);
	}

	public void ChannelPropertyChangeReceived(Channel ch, int channelProperty, int value)
	{
		mCommandHandler.SetChannelProptery(ch.ID, channelProperty, value);
	}

}