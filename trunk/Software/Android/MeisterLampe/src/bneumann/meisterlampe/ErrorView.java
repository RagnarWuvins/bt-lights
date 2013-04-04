package bneumann.meisterlampe;

import java.util.ArrayList;
import android.os.Bundle;
import android.app.ListActivity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;
import android.view.Menu;
import android.widget.ArrayAdapter;

public class MLErrorLog extends ListActivity
{
	private ArrayList<String> value;
	private ArrayAdapter<String> adapter;
	int clickCounter = 0;
	protected String TAG = "ListActivity debug";
	public static final String READER_READY = "awaiting_error_log";
	private ArrayList<String> mErrorLogStrings;
	private static final String[] ERRORS = { 
								"Unknown command",
								"Corrupt command",
								"Not an internal command (AT command)",
								"Too long or bad command",
								"Function is not valid",
								"Value assertion error",
								"Receive race condition",
					            "Read buffer out of range",
					            "Type cast failed",
					            "Wrong mode set. Will be set to NOOP instead",
					            "This is an unhandled channel or global command",
								};
	private static final String NO_ERROR = "No errors happend ;)";	
	
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		mErrorLogStrings = new ArrayList<String>();
		IntentFilter filter = new IntentFilter(MLStartupActivity.NEW_LOG_ENTRY);
		registerReceiver(mReceiver, filter);
		
		mErrorLogStrings.add(NO_ERROR);
		setContentView(R.layout.activity_error_log);
		adapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, mErrorLogStrings);
		setListAdapter(adapter);		
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.activity_error_log, menu);
		return true;
	}

	@Override
	public void onStart()
	{
		super.onStart();
		// byte[] getErrorLog = { 0x13, -1, -1, 0x00, 0x13, 0x0D, 0x0A };
		// MLStartupActivity.sendMessage(getErrorLog);	
		MLStartupActivity.REQUEST_NUMBER = 3;
		Intent i = new Intent();
		i.setAction(READER_READY);
		sendBroadcast(i);
	}

	@Override
	public void onRestart()
	{
		super.onRestart();

	}

	@Override
	public void onDestroy()
	{
		super.onDestroy();
		unregisterReceiver(mReceiver);
		finish();

	}

	private final BroadcastReceiver mReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			Log.i(TAG, "Broadcast received");
			final String action = intent.getAction();

			if (action.equals(MLStartupActivity.NEW_LOG_ENTRY))
			{				
				long[] extraBundle =  intent.getLongArrayExtra("EXTRA_ERROR_LOG");
				long x = extraBundle[1];
				int seconds = (int) (x % 60);
			    x /= 60;
			    int minutes = (int) (x % 60);
			    x /= 60;
			    int hours = (int) (x % 24);
				String time = String.format( "%02d:%02d:%02d.", hours, minutes, seconds ); 
				String errorName = "";
				if (extraBundle[0] < ERRORS.length)
				{
					errorName = ERRORS[(int) extraBundle[0]];
				}
				else
				{
					Log.e(TAG, "Unknown error occured, maybe unsupported hardware?");
					errorName = String.format("%d", extraBundle[0]);
				}
				mErrorLogStrings.remove(NO_ERROR);
				mErrorLogStrings.add("Error: " + errorName + " occured at " + time); 
				adapter = new ArrayAdapter<String>(context, android.R.layout.simple_list_item_1, mErrorLogStrings);
				setListAdapter(adapter);
			}
		}

	};

}
