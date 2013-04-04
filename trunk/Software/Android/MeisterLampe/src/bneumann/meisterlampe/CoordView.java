package bneumann.meisterlampe;

import bneumann.meisterlampe.CommandHandler.COMMANDS;
import android.os.Bundle;
import android.os.SystemClock;
import android.app.Activity;
import android.content.Intent;
import android.graphics.Color;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.widget.FrameLayout;
import android.widget.Toast;

public class MLCoordView extends Activity implements OnTouchListener
{

	public static final String XY_CHANNEL = "xyChannel";
	public static final String XY_VALUE = "xyValue";
	public static final String SET_CHANNEL_VALUE = "sendChannelValue";
	private static final String TAG = "MLCoordView";
	protected static final String SETUP_CHANNEL_NUM = "openChannelSetup";
	private TimeController m_drawView;
	private FrameLayout m_frame;
	private int mChannels;
	private BluetoothService mMLBluetoothService;
	private CommandHandler mCommandHandler;
	private long mLowPass;
	private final int LOWPASS_LENGTH = 50;
	private int[] mHistX = new int[LOWPASS_LENGTH];
	private int[] mHistY = new int[LOWPASS_LENGTH]; 
	private int mEventCounter = 0;
	private boolean mLockTouch = true;
	private static int lastChannel = 0;
	
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_mlcoord_view);
		mChannels = MainActivity.connectedLamp.GetNumberOfChannels();
		
		m_drawView = new TimeController(this, mChannels);
		m_drawView.setBackgroundColor(Color.BLACK);

		m_frame = (FrameLayout) findViewById(R.id.CoordFrame);
		m_frame.addView(m_drawView);

		// setup TouchListener and lowpass filter:
		mLowPass = SystemClock.uptimeMillis();
		m_drawView.setOnTouchListener(this);
							

		m_drawView.setFocusable(true);
		new Intent(SET_CHANNEL_VALUE);
		mMLBluetoothService = MainActivity.getBluetoothService();
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
	}
	public void onResume()
	{
		super.onResume();
		mHistX = new int[LOWPASS_LENGTH];
		mHistY = new int[LOWPASS_LENGTH];
		mEventCounter = 0;
		mLockTouch = true;
		m_drawView.setOnTouchListener(this);
	}
	
	public void onDestroy()
	{
		super.onDestroy();
		mCommandHandler.dispose();
	}
	
	public boolean onTouch(View v, MotionEvent event)
	{		
		// Prevent overflow of events
		if (event.getEventTime() - mLowPass < 10)
		{
			return true;
		}
		mLowPass = event.getEventTime();
		// MotionEvent object holds X-Y values
		if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE)
		{
			if(mEventCounter >= LOWPASS_LENGTH)
			{
				mEventCounter = 0;
			}
			int xCoord = Math.round(event.getX());
			int yCoord = Math.round(event.getY());
			mHistX[mEventCounter] = xCoord;
			mHistY[mEventCounter] = yCoord;
			// get the touch point and tell the Controller where we are
			m_drawView.setPoint(xCoord, yCoord);
			int curc = m_drawView.getCurrentChannel();
			int curv = m_drawView.getCurrentValue();	
			
			if(curc != lastChannel)
			{
				m_drawView.enableFading();
				lastChannel = curc;
			}
			
			// inform the main Activity so they can send the bluetooth command out		
			mCommandHandler.SetChannelProptery(mCommandHandler.ChannelToAddress(curc), COMMANDS.CMD_SET_VAL, curv);
			
			// release screen if finger not moving	
			int diff = 0;
			for (int index = 0; index < LOWPASS_LENGTH - 1; index++)
			{
				diff += mHistX[index + 1] - mHistX[index];
				diff += mHistY[index + 1] - mHistY[index];
			}
			if(diff == 0)
			{
				mLockTouch = false;
			}
			mEventCounter++;
		}
		else
		{
			mHistX = new int[LOWPASS_LENGTH];
			mHistY = new int[LOWPASS_LENGTH];
			mEventCounter = 0;
			mLockTouch = true;
		}
		if(!mLockTouch)
		{
			m_drawView.setOnTouchListener(null);
		}
		return true;
	}
}
