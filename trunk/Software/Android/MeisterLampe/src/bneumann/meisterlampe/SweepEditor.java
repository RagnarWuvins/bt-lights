package bneumann.meisterlampe;

import java.util.ArrayList;
import bneumann.meisterlampe.CommandHandler.COMMANDS;
import bneumann.meisterlampe.CommandHandler.Channels;
import bneumann.meisterlampe.Lamp.Channel;
import bneumann.meisterlampe.TimeController.FadingRect;
import android.os.Bundle;
import android.os.Handler;
import android.os.SystemClock;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Point;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.FrameLayout;
import android.widget.Toast;

public class SweepEditor extends Activity
{

	public static final String XY_CHANNEL = "xyChannel";
	public static final String XY_VALUE = "xyValue";
	public static final String SET_CHANNEL_VALUE = "sendChannelValue";
	private static final String TAG = "SweepEditor";
	protected static final String SETUP_CHANNEL_NUM = "openChannelSetup";
	private ParameterEditor mDelayView, mPeriodView;
	private OnTouchListener mDelayTouchListener, mPeriodTouchListener;
	private int mChannels;
	private BluetoothService mMLBluetoothService;
	private Lamp mLamp;
	private CommandHandler mCommandHandler;
	private final int LOWPASS_LENGTH = 50;
	private FrameLayout mUpperframe, mLowerframe;
	private Button mRestartSweep;
	private CheckBox mMoveAll;
	private boolean mMoveAllEnable;

	
	
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.sweepeditor);
		mLamp = MainActivity.connectedLamp;
		mChannels = mLamp.GetNumberOfChannels();
		mMoveAllEnable = false;
		
		mDelayView = new ParameterEditor(this, ParameterEditor.TYPE_DELAY);
		mPeriodView = new ParameterEditor(this, ParameterEditor.TYPE_PERIOD);
		mDelayView.setBackgroundColor(Color.BLACK);
		mPeriodView.setBackgroundColor(Color.BLACK);
		mPeriodView.ChangeGridColor(Color.GREEN);
		
		mUpperframe = (FrameLayout) findViewById(R.id.upperframe);
		mLowerframe = (FrameLayout) findViewById(R.id.lowerframe);
		mUpperframe.addView(mDelayView);
		mLowerframe.addView(mPeriodView);
		
		mRestartSweep = (Button)findViewById(R.id.se_restartsweep);
		mRestartSweep.setOnClickListener(new View.OnClickListener()
        {
            public void onClick(View v)
            {
            	mCommandHandler.ResetTimers();
            }
        });
		
		mMoveAll = (CheckBox)findViewById(R.id.se_moveall);
		mMoveAll.setChecked(mMoveAllEnable);
		mMoveAll.setOnClickListener(new View.OnClickListener()
        {
            public void onClick(View v)
            {
            	mMoveAllEnable = !mMoveAllEnable;
            	mMoveAll.setChecked(mMoveAllEnable);
            }
        });

		CreateTouchlistener();
		
		// setup TouchListener and lowpass filter:
		mDelayView.setOnTouchListener(mDelayTouchListener);
		
		// setup TouchListener and lowpass filter:
		mPeriodView.setOnTouchListener(mPeriodTouchListener);

		mDelayView.setFocusable(true);
		mPeriodView.setFocusable(true);
		
		new Intent(SET_CHANNEL_VALUE);
		mMLBluetoothService = MainActivity.getBluetoothService();
		mCommandHandler = new CommandHandler(this, mMLBluetoothService);
	}
	
	public void onResume()
	{
		super.onResume();
		mDelayView.setOnTouchListener(mDelayTouchListener);
		mPeriodView.setOnTouchListener(mPeriodTouchListener);
	}
	
	public void onDestroy()
	{
		super.onDestroy();
		mCommandHandler.dispose();
	}	

	private void CreateTouchlistener()
	{
		mDelayTouchListener = new OnTouchListener()
		{			
			
			public boolean onTouch(View v, MotionEvent event)
			{
				if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE)
				{
					Log.i(TAG, "Moving...");
					int xCoord = Math.round(event.getX());
					int yCoord = Math.round(event.getY());
					mDelayView.InputMotion(xCoord, yCoord);
				}
				// enable moving and catching all events				
				return true;
			}
		};
		
		mPeriodTouchListener = new OnTouchListener()
		{			
			public boolean onTouch(View v, MotionEvent event)
			{
				if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE)
				{
					Log.i(TAG, "Moving...");
					int xCoord = Math.round(event.getX());
					int yCoord = Math.round(event.getY());
					mPeriodView.InputMotion(xCoord, yCoord);
				}
				// enable moving and catching all events				
				return true;
			}
		};
	}
	
	public class ParameterEditor extends View
	{
		private static final int TYPE_PERIOD = 0;
		private static final int TYPE_DELAY = 1;
		private static final float DEFAULT_CHAN_STROKE = 2;
		private static final int DEFAULT_CHAN_COLOR = Color.BLUE;
		private static final String TAG = "ParameterEditor";
		private int mType;
		private Point m_point;
		private int mWidth;
		private int mHeight;
		private Paint mChannelPaint;

		/**
		 * Constructor for time control, takes the number of channels
		 * @param context Context of the apps main Activity
		 * @param channels number of channels to display (always 0-x)
		 */
		public ParameterEditor(Context context, int type)
		{
			super(context);
			m_point = new Point(0, 0);
			mType = type;
			DisplayMetrics metrics = context.getResources().getDisplayMetrics();			
			mWidth = metrics.widthPixels;
			mHeight = metrics.heightPixels;
			mChannelPaint = new Paint();
			mChannelPaint.setStrokeWidth(DEFAULT_CHAN_STROKE);
			mChannelPaint.setColor(DEFAULT_CHAN_COLOR);
		}

		public void onDraw(Canvas canvas)
		{
			super.onDraw(canvas);
			drawChannels(canvas);
		}
		
		public void ChangeGridColor(int paintColor)
		{
			mChannelPaint.setColor(paintColor);
		}
		
		public void InputMotion(int x, int y)
		{
			int chan = x / (getMeasuredWidth() / (mLamp.channels.length + 1));
			int value = (int)(255 - (float)y / (float)getMeasuredHeight() * 255);
			if(chan >= 0 && chan < mLamp.channels.length)
			{
				switch (mType)
				{
				case TYPE_DELAY:
					if(mMoveAllEnable)
					{
						mLamp.SetAllProperty(COMMANDS.CMD_SET_DELAY, value);
					}
					else
					{
						mLamp.channels[chan].setDelay(value);
					}
					break;
				case TYPE_PERIOD:
					if(mMoveAllEnable)
					{
						mLamp.SetAllProperty(COMMANDS.CMD_SET_PERIOD, value);
					}
					else
					{
						mLamp.channels[chan].setPeriod(value);
					}
					break;
				default:
					Log.e(TAG, "Unsupported parameter " + mType + " for editor to input motion");
					break;
				}				
			}
			invalidate();
		}
		
		/**
		 * Draws the channels as columns.
		 * 
		 * @param canvas
		 *            Standard canvas for the view to draw on
		 */
		private void drawChannels(Canvas canvas)
		{
			int layoutWidth = getMeasuredWidth() / (mLamp.channels.length + 1);
			int layoutHeight = getMeasuredHeight();
			int lastChannelXPosition = 0;
			int lastChannelYPosition = 0;
			for (int i = 0; i < mLamp.channels.length; i++)
			{
				int typeParameter = 0;
				switch (mType)
				{
				case TYPE_DELAY:
					typeParameter = mLamp.channels[i].getDelay();
					break;
				case TYPE_PERIOD:
					typeParameter = mLamp.channels[i].getPeriod();
					break;
				default:
					Log.e(TAG, "Unsupported parameter " + mType + " for editor to draw channels");
					break;
				}
				int currentXPosition = i * layoutWidth + layoutWidth;
				int currentYPosition = (int) (layoutHeight * ((255 - (float)typeParameter) / 255.0));
				canvas.drawCircle( currentXPosition, currentYPosition, 10, mChannelPaint);
				canvas.drawLine(currentXPosition, 0, currentXPosition, layoutHeight, mChannelPaint);
				if (i > 0)
				{
					canvas.drawLine(lastChannelXPosition, lastChannelYPosition, currentXPosition, currentYPosition, mChannelPaint);
				}
				lastChannelXPosition = currentXPosition;
				lastChannelYPosition = currentYPosition;
			}
		}
	}
}
