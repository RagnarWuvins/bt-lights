package bneumann.meisterlampe;

import java.io.Console;
import java.util.ArrayList;
import android.os.Bundle;
import android.app.Activity;
import android.app.ListActivity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.ArrayAdapter;

public class MLErrorLog extends ListActivity
{
	private ArrayList<String> value;
	private ArrayAdapter<String> adapter;
	int clickCounter = 0;
	protected String TAG = "ListActivity debug";
	public static final String READER_READY = "awaiting_error_log";
	
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		value = new ArrayList<String>();
		IntentFilter filter = new IntentFilter(MLStartupActivity.NEW_LOG_ENTRY);
		registerReceiver(mReceiver, filter);
		
		value.add("No errors happend ;)");
		setContentView(R.layout.activity_error_log);
		adapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, value);
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
				ArrayList<String> extraStrings = intent.getStringArrayListExtra("EXTRA_ERROR_LOG");
				adapter = new ArrayAdapter<String>(context, android.R.layout.simple_list_item_1, extraStrings);
				setListAdapter(adapter);
			}
		}

	};

}
