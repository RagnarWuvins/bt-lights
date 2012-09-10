package bneumann.meisterlampe;

import android.os.Bundle;
import android.preference.PreferenceManager;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Point;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.widget.FrameLayout;
import android.widget.TextView;

public class MLCoordView extends Activity implements OnTouchListener
{

	public static final String XY_CHANNEL = "xyChannel";
	public static final String XY_VALUE = "xyValue";
	public static final String SET_CHANNEL_VALUE = "sendChannelValue";
	private TextView m_xyDebug;
	private TimeController m_drawView;
	private FrameLayout m_frame;
	private SharedPreferences m_pref;
	private int mChannels;
	private Intent mSendEvent;

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_mlcoord_view);
		
		m_pref = PreferenceManager.getDefaultSharedPreferences(this);
		mChannels = m_pref.getInt("NUMBER_OF_CHANNELS", MLStartupActivity.NUMBER_OF_CHANNELS);
		
		m_drawView = new TimeController(this, mChannels);
		m_drawView.setBackgroundColor(Color.BLACK);

		m_frame = (FrameLayout) findViewById(R.id.CoordFrame);
		m_frame.addView(m_drawView);

//		m_xyDebug = (TextView) findViewById(R.id.xyDebug);
//		m_xyDebug.setText("What you see is not a test");
		m_drawView.setOnTouchListener(this);
		m_drawView.setFocusable(true);
		mSendEvent = new Intent(SET_CHANNEL_VALUE);

	}
	public boolean onTouch(View v, MotionEvent event)
	{
		// MotionEvent object holds X-Y values
		if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE)
		{
			
//			String text = "x = " + event.getX() + "\ny = " + event.getY() + "\nChannel = " + curc + "\nValue = " + curv;
//			m_xyDebug.setText(text);
			// get the touch point and tell the Controller where we are
			m_drawView.setPoint(event.getX(), event.getY());
			int curc = m_drawView.getCurrentChannel();
			int curv = m_drawView.getCurrentValue();	
			// inform the main Activity so they can send the bluetooth command out
			mSendEvent.putExtra(XY_CHANNEL, curc);
			mSendEvent.putExtra(XY_VALUE, curv);
			sendBroadcast(mSendEvent);
		}

		return true;
	}
}
