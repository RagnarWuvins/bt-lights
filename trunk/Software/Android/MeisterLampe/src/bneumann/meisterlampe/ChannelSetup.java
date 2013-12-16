package bneumann.meisterlampe;

import android.os.Bundle;
import android.app.Activity;
import android.app.ProgressDialog;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import android.widget.TextView;

public class ChannelSetup extends Activity implements OnItemSelectedListener
{
	protected static final String TAG = "MLChannelSetup";
	private int mChannel;
	private TextView mChannelName;
	private Spinner mModeSelector;
	//private BluetoothService mMLBluetoothService;
	//private CommandHandler mCommandHandler;
	private int mMode;
	private ProgressDialog mProgress;

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_mlchannel_setup);

		Intent intent = getIntent();
		mChannel = intent.getExtras().getInt(CoordView.SETUP_CHANNEL_NUM);
		mChannelName = (TextView) findViewById(R.id.channelName);
		mModeSelector = (Spinner) findViewById(R.id.modeSelector);

		// get comm interface:
		//mMLBluetoothService = MainActivity.getBluetoothService();
		//mCommandHandler = new CommandHandler(this, mMLBluetoothService);
		
		registerReceivers();
		
		//mCommandHandler.GetChannelMode(mChannel);
		mProgress = ProgressDialog.show(this, "", getString(R.string.getting_channel_values), false, true);
	}
	
	@Override
	public void onDestroy()
	{
		super.onDestroy();
		unregisterReceiver(mReceiver);
		mProgress.dismiss();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.activity_mlchannel_setup, menu);
		return true;
	}

	public void onItemSelected(AdapterView<?> arg0, View arg1, int arg2, long arg3)
	{
		// TODO Auto-generated method stub
		//mCommandHandler.SetChannelMode(mCommandHandler.ChannelToAddress(mChannel), arg2);
	}

	public void onNothingSelected(AdapterView<?> arg0)
	{
		// TODO Auto-generated method stub

	}
	
	private void registerReceivers()
	{
		
		IntentFilter filter = new IntentFilter(CommandHandler.CH_VAL_RX);
		registerReceiver(mReceiver, filter);

		filter = new IntentFilter(CommandHandler.CH_MODE_RX);
		registerReceiver(mReceiver, filter);
	}
	
	private BroadcastReceiver mReceiver = new BroadcastReceiver()
	{
		
		@Override
		public void onReceive(Context context, Intent intent)
		{
			if(intent.getAction().equals(CommandHandler.CH_MODE_RX))
			{
				mMode = intent.getIntExtra(CommandHandler.CH_MODE_RX_EXTRA, 0);
			}	
			fillElements();
			mProgress.dismiss();
		}		
	};

	private void fillElements()
	{
		// fill elements
		mChannelName.setText("Setup channel: " + mChannel);
		ArrayAdapter<CharSequence> adapter = ArrayAdapter.createFromResource(this, R.array.Modes, android.R.layout.simple_spinner_item);
		adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
		mModeSelector.setAdapter(adapter);
		final ChannelSetup listener = this;
		mModeSelector.post(new Runnable()
		{
			public void run()
			{
				mModeSelector.setOnItemSelectedListener(listener);
			}
		});

		mModeSelector.setSelection(mMode);
	}

}
