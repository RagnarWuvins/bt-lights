package bneumann.meisterlampe;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.graphics.Typeface;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.View;
import android.view.animation.Animation;
import android.view.animation.Animation.AnimationListener;
import android.view.animation.AnimationUtils;
import android.widget.TextView;
import bneumann.meisterlampe.MainButton.Functions;
import bneumann.protocol.Frame;
import bneumann.protocol.Package;

public class FunctionActivity extends Activity
{

	protected static final String TAG = "FunctionSelection";
	private boolean mIsBound = false;
	private String mDefaultDevice;
	private SharedPreferences mSettings;
	private BluetoothService mBluetoothService;
	
	
	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_function);
		loadSettings();
		TextView tv = (TextView) findViewById(R.id.FunctionSelectionDescription);
		if (!tv.isInEditMode())
		{
			Typeface type = Typeface.createFromAsset(getAssets(), "Angelic Serif.ttf"); 
			tv.setTypeface(type);
		}
		
		doBindService();
	}
	
	@Override
	protected void onDestroy()
	{
		super.onDestroy();		
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
				Log.d(TAG,"Function: " + cs);
				switch (cs)
				{
				case FUNC:
					onSinePress();
					break;
				case UP:
					onSawtoothClick();
					break;
				case DOWN:
					onInverseSawtoothClick();
					break;
				case BACK:
					finish();
					onDestroy();
					break;
				default:
					break;
				}
			}
		});
	}
	
	protected void onSinePress()
	{
		Package output = new Package();
		Frame f;
		for(int i = 0; i < Lamp.NUMBER_OF_CHANNELS; i++)
		{
			f = new Frame();
			f.setFunction(Lamp.SET_FUNCTION);
			f.setChannel(i);
			f.setValue(Lamp.FUNC_SINE);
			output.add(f);
		}
		f = new Frame();
		f.setFunction(Lamp.RESET_ALL_TIMER);
		output.add(f);
		mBluetoothService.write(output);
	}

	protected void onInverseSawtoothClick()
	{
		Package output = new Package();
		Frame f;
		for(int i = 0; i < Lamp.NUMBER_OF_CHANNELS; i++)
		{
			f = new Frame();
			f.setFunction(Lamp.SET_FUNCTION);
			f.setChannel(i);
			f.setValue(Lamp.FUNC_SAW_REV);
			output.add(f);
		}
		f = new Frame();
		f.setFunction(Lamp.RESET_ALL_TIMER);
		output.add(f);
		mBluetoothService.write(output);
	}

	protected void onSawtoothClick()
	{
		Package output = new Package();
		Frame f;
		for(int i = 0; i < Lamp.NUMBER_OF_CHANNELS; i++)
		{
			f = new Frame();
			f.setFunction(Lamp.SET_FUNCTION);
			f.setChannel(i);
			f.setValue(Lamp.FUNC_SAW);
			output.add(f);
		}
		f = new Frame();
		f.setFunction(Lamp.RESET_ALL_TIMER);
		output.add(f);
		mBluetoothService.write(output);
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
