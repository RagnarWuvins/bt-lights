package bneumann.meisterlampe;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import bneumann.meisterlampe.CommandHandler.GLOBAL_COMMANDS;
import bneumann.meisterlampe.CommandHandler.GlobalCommandReceivedListener;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.app.ListActivity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;
import android.view.Menu;
import android.widget.ArrayAdapter;

public class ErrorView extends ListActivity implements GlobalCommandReceivedListener
{
	
	private ArrayAdapter<String> adapter;
	int clickCounter = 0;
	protected String TAG = "ListActivity debug";
	public static final String READER_READY = "awaiting_error_log";
	private volatile ArrayList<int[]> mErrorLog;
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
	
	private Lamp connectedLamp;
	private BluetoothService mMLBluetoothService;
	private CommandHandler mCommandHandler;
	private int mErrorCounter;
	private ArrayList<int[]> mTempList;
	
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		mErrorLogStrings = new ArrayList<String>();
		mErrorLog = new ArrayList<int[]>();
		connectedLamp = MainActivity.connectedLamp;
		
		mErrorLogStrings.add(NO_ERROR);
		setContentView(R.layout.activity_error_log);
		adapter = new ArrayAdapter<String>(this, R.layout.errorlist, mErrorLogStrings);
		setListAdapter(adapter);
		mMLBluetoothService = MainActivity.getBluetoothService();
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
		mCommandHandler.AddGlobalCommandReceivedListener(this);
	}

	private void parseLog(ArrayList<int[]> errors)
	{
		adapter = new ArrayAdapter<String>(this, R.layout.errorlist, mErrorLogStrings);
		setListAdapter(adapter);
	}
	
	
		
	private class Error
	{
		public String errorName = "";
		public String timeString = "";
		public String counterString = "";
		private Error(int[] values)
		{
			int time = values[1];
			int error = values[0];
			int counter = values[2];

			int seconds = (int) (time % 60);
			time /= 60;
			int minutes = (int) (time % 60);
			time /= 60;
			int hours = (int) (time % 24);
			this.timeString = String.format("%02d:%02d:%02d", hours, minutes, seconds);
			this.counterString = String.format("%d", counter);
			if (error < ERRORS.length)
			{
				this.errorName = ERRORS[error];
			}
			else
			{
				Log.e(TAG, "Unknown error occured, maybe unsupported hardware?");
				this.errorName = String.format("%d", error);
			}
		}
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
		MainActivity.REQUEST_NUMBER = 3;
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
		finish();

	}

	public void OnCommandReceive(Command command)
	{
		if (command.mode == GLOBAL_COMMANDS.ERROR)
		{
			mErrorCounter++;
			mErrorLog.add(new int[]{command.errorNum, command.errorTime, mErrorCounter});
			// copy the list to make it thread save
			mTempList = new ArrayList<int[]>();			
			mErrorLogStrings = new ArrayList<String>();
			for(int[] entry : mErrorLog)
			{
				mTempList.add(entry);
				Error er = new Error(entry);
				mErrorLogStrings.add(String.format("[%s][%s] %s", er.counterString, er.timeString, er.errorName)); 
			}
			
			Handler refresh = new Handler(Looper.getMainLooper());
			refresh.post(new Runnable()
			{
				public void run()
				{					
					parseLog(mTempList);					
				}	
			});
			
		}		
	}
}
