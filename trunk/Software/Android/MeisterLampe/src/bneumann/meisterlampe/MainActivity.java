package bneumann.meisterlampe;

import java.util.ArrayList;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup.MarginLayoutParams;
import android.view.animation.Animation;
import android.view.animation.Animation.AnimationListener;
import android.view.animation.AnimationUtils;
import android.widget.Toast;
import bneumann.meisterlampe.MainButton.Functions;
import bneumann.protocol.Package;

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

	public static ArrayList<String> errorLog = new ArrayList<String>();
	private BluetoothService mBluetoothService;
	private String mDefaultDevice;
	public static Lamp mLamp;

	protected String EXTRA_DEVICE_ADDRESS;
	public static int REQUEST_NUMBER = 0;
	public static final String NEW_LOG_ENTRY = "new_log_entry";
	public static final String RESET_HARDWARE = "reset_hardware";

	protected boolean reconnectTask = false; // set to true if the system should try to reconnect after STATE_NONE
	public boolean mState = false;
	private boolean mFirstTimeStartup;

	private AppContext mContext;

	private BroadcastReceiver mMessageReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			String action = intent.getAction();
			if (action.equals(BluetoothService.CONNECTION_STATE_CHANGE))
			{
				int state = intent.getIntExtra(BluetoothService.CONNECTION_STATE_CHANGE, -1);
				onBluetoothStateChange(state);
			}
			if (action.equals(BluetoothService.RX_NEW_PACKAGE))
			{
				byte[] encodedBytes = intent.getByteArrayExtra(BluetoothService.RX_NEW_PACKAGE);
				try
				{
					Package p = new Package(encodedBytes);
					mLamp.Update(p);
					ArrayList<String> als = mLamp.getErrorLog();
				}
				catch (Exception e)
				{
					e.printStackTrace();
				}
			}
		}
	};

	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		this.mContext = new AppContext();

		setContentView(R.layout.main);

		SubButton sb = (SubButton) findViewById(R.id.SettingsButton);
		final FunctionWheel fw = (FunctionWheel) findViewById(R.id.FunctionWheel);

		// deactivate wheel
		fw.setEnabled(false);

		// setting the connect button
		// TODO: rework this part to be in XML
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
		mLamp = new Lamp();
		loadSettings();

		Toast.makeText(this, "", Toast.LENGTH_SHORT); // init Toast

		// initialize bluetooth adapter
		this.mContext.BTAdapter = BluetoothAdapter.getDefaultAdapter();

		// Register all receivers for this activity
		registerReceiver(mMessageReceiver, new IntentFilter(BluetoothService.RX_NEW_PACKAGE));
		registerReceiver(mMessageReceiver, new IntentFilter(BluetoothService.CONNECTION_STATE_CHANGE));

		if (!this.mContext.Emulator && this.mContext.BTAdapter == null)
		{
			Log.d(TAG, getString(R.string.no_bluetooth));
			sb.setEnabled(false);
			fw.setEnabled(false);
			Toast.makeText(this, R.string.no_bluetooth, Toast.LENGTH_LONG).show();
			return;
		}
	}

	public void onStart()
	{
		super.onStart();
	}

	public void onDestroy()
	{
		// stop bluetooth connection
		doUnbindService();
		super.onDestroy();
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
					onPowerButton(button);
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

	public void onPowerButton(MainButton button)
	{
		byte errorlog = Lamp.GET_ERRORLOG;
		byte version = Lamp.GET_SYSTEM_VERSION;
		byte cc = Lamp.GET_COMMAND_COUNTER;
		byte time = Lamp.GET_SYSTEM_TIME;
		byte[] out = {00, 00, 00, 02, time, 00, 00, 00};
		this.mBluetoothService.write(out);
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
		if (this.mFirstTimeStartup)
		{
			Toast.makeText(this, R.string.connect_first_time, Toast.LENGTH_SHORT).show();			
		}
		else
		{
			// TODO: do NOT set functions enabled before connection, just start the service. Has to be done in Service feedback
			doBindService();
		}
	}

	private ServiceConnection mServiceConnection = new ServiceConnection()
	{
		public void onServiceDisconnected(ComponentName name)
		{
			Log.d(TAG, "Service disconnected");
		}

		public void onServiceConnected(ComponentName name, IBinder service)
		{
			// casting awesomeness to get the service and attach it to the
			mBluetoothService = ((BluetoothService.BluetoothServiceBinder) service).getService();
			Log.d(TAG, "Service connected");			
		}
	};

	protected void onBluetoothStateChange(int state)
	{
		FunctionWheel fw = (FunctionWheel) findViewById(R.id.FunctionWheel);
		switch (state)
		{
		case BluetoothService.STATE_NONE:
			fw.setEnabled(false);
			break;
		case BluetoothService.STATE_CONNECTING:
			fw.setEnabled(false);
			break;
		case BluetoothService.STATE_CONNECTED:			
			fw.setEnabled(true);
			break;
		}
		// the setup button should be enabled regardless of the BT state (to choose another device)
		fw.setChildEnabled(3, true);
	}

	public void loadSettings()
	{
		this.mSettings = getSharedPreferences(SetupActivity.SHARED_PREFERENCES, MODE_PRIVATE);
		this.mDefaultDevice = mSettings.getString(SetupActivity.DEFAULT_DEVICE_ADDRESS, null);
		this.mFirstTimeStartup = mSettings.getBoolean(SetupActivity.FIRST_TIME_STARTUP, true);
	}

	void doBindService()
	{
		Intent intent = new Intent(this, BluetoothService.class);
		intent.putExtra(BluetoothService.CONNECTION_ADDRESS, this.mDefaultDevice);
		bindService(intent, mServiceConnection, BIND_AUTO_CREATE);
	}

	void doUnbindService()
	{
		unbindService(mServiceConnection);
	}
}