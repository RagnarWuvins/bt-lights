package bneumann.meisterlampe;

import java.util.ArrayList;
import bneumann.meisterlampe.MainButton.Functions;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.drawable.Drawable;
import android.os.Build;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup.MarginLayoutParams;
import android.view.animation.Animation;
import android.view.animation.Animation.AnimationListener;
import android.view.animation.AnimationUtils;
import android.widget.Toast;

@SuppressLint("ShowToast")
public class MainActivity extends Activity
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
	public static ArrayList<String> errorLog = new ArrayList<String>();
	private static BluetoothService mMLBluetoothService;
	public static Lamp connectedLamp;

	protected String EXTRA_DEVICE_ADDRESS;
	public static int REQUEST_NUMBER = 0;
	public static final String NEW_LOG_ENTRY = "new_log_entry";
	public static final String RESET_HARDWARE = "reset_hardware";
	private boolean mConnectAtStartup = false;
	private Toast mToast; // Toast object to prevent overlapping Toasts
	private String mDefaultDevice;
	private CommandHandler mCommandHandler;

	protected boolean reconnectTask = false; // set to true if the system should try to reconnect after STATE_NONE
	public boolean mState = false;
	private boolean mFirstTimeStartup;

	private AppContext mContext;

	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		this.mContext = new AppContext();

		setContentView(R.layout.main);

		SubButton sb = (SubButton) findViewById(R.id.SettingsButton);
		final FunctionWheel fw = (FunctionWheel) findViewById(R.id.FunctionWheel);

		fw.setEnabled(false);

		android.view.ViewGroup.MarginLayoutParams lp = (MarginLayoutParams) sb.getLayoutParams();
		lp.topMargin = fw.getMargin();
		lp.width = fw.getSize();
		Drawable d = getResources().getDrawable(R.drawable.img_setting);
		int minHeight = d.getMinimumHeight();
		int minWidth = d.getMinimumWidth();
		double scale = (double) minHeight / (double) minWidth;
		lp.height = (int) (((double) fw.getSize()) * scale);
		Log.v(TAG, "height: " + lp.height + " width: " + lp.width);
		sb.setLayoutParams(lp);

		// load settings
		connectedLamp = new Lamp();
		loadSettings();

		// mOutputText = (TextView) findViewById(R.id.outputText);

		mToast = Toast.makeText(this, "", Toast.LENGTH_SHORT); // init Toast
		// mChannelTable = (TableLayout) findViewById(R.id.channelTable);

		// initialize bluetooth adapter
		this.mContext.BTAdapter = BluetoothAdapter.getDefaultAdapter();

		// if (this.mContext.BTAdapter == null)
		if (!this.mContext.Emulator && this.mContext.BTAdapter == null)
		{
			Log.d(TAG, getString(R.string.no_bluetooth));
			sb.setEnabled(false);
			fw.setEnabled(false);
			Toast.makeText(this, R.string.no_bluetooth, Toast.LENGTH_LONG).show();
			// Setup a commandhandler that doesn't do anything for debug
			mCommandHandler = new CommandHandler(this, null);
			return;
		}
	}

	public void onStart()
	{
		super.onStart();
	}

	public void onDestroy()
	{
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

	/** Called when the activity resumes from another task (subactivity) **/
	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		switch (requestCode)
		{
		case REQUEST_ENABLE:
			switch (resultCode)
			{
			case RESULT_OK:
				onResumeBluetoothEnable();
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
		// mProgressDialog = ProgressDialog.show(this, "", getString(R.string.trying_to_connect), false, true);
		// registerReceivers();

		// Initialize the MLBluetoothService to perform bluetooth connections
		if (mMLBluetoothService == null)
		{
			mMLBluetoothService = new BluetoothService(this);
		}

		if (mDefaultDevice != null)
		{
			BluetoothDevice device = this.mContext.BTAdapter.getRemoteDevice(mSettings.getString(Settings.DEFAULT_DEVICE, ""));

			if (mMLBluetoothService.getState() != BluetoothService.STATE_CONNECTED)
			{
				mMLBluetoothService.connect(device, true);
			}
			else
			{
				// mProgressDialog.cancel();
			}

		}
		else
		{
			mToast.setText(R.string.no_default_device_found);
			// start to discover bluetooth devices
			this.mContext.BTAdapter.startDiscovery();
		}
		// create the command handler and setup the interface
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
		mCommandHandler.AddChannelCommandReceivedListener(connectedLamp);
		mCommandHandler.AddGlobalCommandReceivedListener(connectedLamp);
	}

	public void onResumeBluetoothEnable()
	{
		enableFunctions();
	}

	public void onFunctionClick(View view)
	{
		final MainButton button = (MainButton) view;
		Animation onClickAnim = AnimationUtils.loadAnimation(this, R.anim.onclick);
		button.startAnimation(onClickAnim);
		onClickAnim.setAnimationListener(new AnimationListener()
		{			
			public void onAnimationStart(Animation animation)
			{				
			}
			
			public void onAnimationRepeat(Animation animation)
			{
			}
			
			public void onAnimationEnd(Animation animation)
			{
				Functions cs = MainButton.Functions.values()[button.Function];
				switch (cs)
				{
				case FUNC:
					break;
				case LEVEL:
					break;
				case POWER:
					break;
				case SETTINGS:
					onSettingsClick(button);
					break;
				default:
					break;
				}
			}
		});

	}
	
	public void onSettingsClick(MainButton button)
	{
		Intent intent = new Intent(this, SetupActivity.class);
		this.startActivity(intent);
	}
	
	public void onConnectClick(View view)
	{
		if (this.mContext.Emulator)
		{
			enableFunctions();
			return;
		}
		if (!this.mContext.BTAdapter.isEnabled())
		{
			Intent enabler = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			startActivityForResult(enabler, REQUEST_ENABLE);
			return;
		}
		enableFunctions();
	}

	/**
	 * This function is called from two places:<br>
	 * <b>onResumeBluetoothEnable</b> -> Ask user if he wants to enable BT<br>
	 * <b>onSetupClick</b> -> BT is already on
	 */
	private void enableFunctions()
	{
		FunctionWheel fw = (FunctionWheel) findViewById(R.id.FunctionWheel);
		if (this.mFirstTimeStartup)
		{
			Toast.makeText(this, R.string.connect_first_time, Toast.LENGTH_SHORT).show();
			fw.setChildEnabled(3, true);
		}
		else
		{
			fw.setEnabled(true);
		}
	}

	public void loadSettings()
	{
		this.mSettings = PreferenceManager.getDefaultSharedPreferences(this);
		// TODO: this might be an android issue?!
		connectedLamp.SetNumberOfChannels(Integer.parseInt(mSettings.getString(Settings.NUM_OF_CHANNELS, "10")));
		this.mConnectAtStartup = mSettings.getBoolean(Settings.CONNECT_ON_STARTUP, false);
		this.mDefaultDevice = mSettings.getString(Settings.DEFAULT_DEVICE, null);
		this.mFirstTimeStartup = mSettings.getBoolean(Settings.FIRST_TIME_STARTUP, true);
	}

	public static BluetoothService getBluetoothService()
	{
		return mMLBluetoothService;
	}
}