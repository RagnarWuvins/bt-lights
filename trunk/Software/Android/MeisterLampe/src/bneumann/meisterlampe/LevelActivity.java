package bneumann.meisterlampe;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup.LayoutParams;
import android.widget.ToggleButton;
import bneumann.protocol.Frame;
import bneumann.protocol.Package;

public class LevelActivity extends Activity
{

	protected static final String TAG = "LevelActivity";
	private BluetoothService mBluetoothService;
	private boolean mIsBound;
	private String mDefaultDevice;
	private SharedPreferences mSettings;
	private Object points;
	private boolean mToggleState = false;
	
	private BroadcastReceiver mMessageReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			String action = intent.getAction();
			if (action.equals(BluetoothService.RX_NEW_PACKAGE))
			{
				updateDrawView();
			}
		}
	};
	
	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_level);
		loadSettings();
		DrawView iv = (DrawView)findViewById(R.id.imageView1);
		LayoutParams params = iv.getLayoutParams();		
		iv.setLayoutParams(params);
		registerReceiver(mMessageReceiver, new IntentFilter(BluetoothService.RX_NEW_PACKAGE));
		
		doBindService();
	}
	
	protected void updateDrawView()
	{
		DrawView iv = (DrawView)findViewById(R.id.imageView1);
		int[] values = this.mBluetoothService.getLamp().getChannelValues();
		iv.setChannelValues(values);
		iv.invalidate();
	}

	@Override
	protected void onDestroy()
	{		
		super.onDestroy();
		unregisterReceiver(mMessageReceiver);
		doUnbindService();
		overridePendingTransition(R.anim.fadein, R.anim.fadeout);
	}
	
	@Override
	public void onBackPressed()
	{
		super.onBackPressed();
		doUnbindService();
		overridePendingTransition(android.R.anim.fade_in, android.R.anim.fade_out);
	}
	
	public void onToggleTrace(View view)
	{
		ToggleButton tg = (ToggleButton) view;
		this.mToggleState = tg.isChecked();
		
		Package p = new Package();
		Frame f = new Frame();
		f.setFunction(Lamp.CHANNEL_TRACER);
		f.setByte(2, (byte)100);
		f.setValue(this.mToggleState  ? 1 : 0);
		p.add(f);
		this.mBluetoothService.write(p);
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
	
	void doBindService()
	{
		Intent intent = new Intent(this, BluetoothService.class);
		intent.putExtra(BluetoothService.CONNECTION_ADDRESS, this.mDefaultDevice);
		this.mIsBound = bindService(intent, mServiceConnection, BIND_AUTO_CREATE);
	}

	void doUnbindService()
	{
		if(this.mIsBound)
		{
			unbindService(mServiceConnection);
			this.mIsBound = false;
		}
	}
	
	public void loadSettings()
	{
		this.mSettings = getSharedPreferences(SetupActivity.SHARED_PREFERENCES, MODE_PRIVATE);
		this.mDefaultDevice = mSettings.getString(SetupActivity.DEFAULT_DEVICE_ADDRESS, null);
	}

}
