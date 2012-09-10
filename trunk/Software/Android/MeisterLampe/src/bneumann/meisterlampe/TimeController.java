package bneumann.meisterlampe;

import java.util.ArrayList;
import java.util.Date;
import java.util.Timer;
import java.util.TimerTask;
import junit.framework.Assert;
import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Point;
import android.os.Handler;
import android.util.AttributeSet;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.View;

public class TimeController extends View
{

	private static final float DEFAULT_CHAN_STROKE = 2;
	private static final int DEFAULT_CHAN_COLOR = Color.BLUE;
	private static final String TAG = null;
	private Paint mPaint;
	private Point m_point;
	private int mWidth;
	private int mHeight;
	private int mChannels;
	private Paint mChannelPaint;
	private int mCurrentChannel;
	private int mChanWidth;
	private ArrayList<FadingRect> mFadingChannels;

	public TimeController(Context context, int channels)
	{
		super(context);
		m_point = new Point(0, 0);
		mPaint = new Paint();
		mPaint.setColor(Color.WHITE);
		DisplayMetrics metrics = context.getResources().getDisplayMetrics();
		mWidth = metrics.widthPixels;
		mHeight = metrics.heightPixels;
		mChannels = channels;
		mChannelPaint = new Paint();
		mChannelPaint.setStrokeWidth(DEFAULT_CHAN_STROKE);
		mChannelPaint.setColor(DEFAULT_CHAN_COLOR);
		mChanWidth = mWidth / channels;
		Canvas canvas = new Canvas();
		mFadingChannels = new ArrayList<FadingRect>(channels);
		for (int i = 0; i < channels; i++)
		{
			mFadingChannels.add(new FadingRect(i));
		}
	}

	public void onDraw(Canvas canvas)
	{
		super.onDraw(canvas);
		drawChannels(canvas);
		for (int i = 0; i < mChannels; i++)
		{
			mFadingChannels.get(i).drawCurrentChannel(canvas);
		}
		
	}

	public void setPoint(float x, float y)
	{
		m_point.x = (int) x;
		m_point.y = (int) y;
		mFadingChannels.get(getCurrentChannel()).setAlpha(getCurrentValue());
	}

	/**
	 * Draws the channels as columns.
	 * 
	 * @param canvas
	 *            Standard canvas for the view to draw on
	 */
	private void drawChannels(Canvas canvas)
	{
		int chanWidth = mWidth / mChannels;
		for (int i = 0; i < mChannels + 1; i++)
		{
			canvas.drawLine(i * chanWidth, 0, i * chanWidth, mHeight, mChannelPaint);
		}
	}

	/**
	 * Get the current selected channel from this view
	 * 
	 * @return number of channel
	 */
	public int getCurrentChannel()
	{
		int curChannel = (m_point.x / mChanWidth);
		if (curChannel > MLStartupActivity.NUMBER_OF_CHANNELS - 1)
		{
			Log.w(TAG,"Wrong channel addressed. Maximum number of channels is: " + MLStartupActivity.NUMBER_OF_CHANNELS);
			curChannel =  MLStartupActivity.NUMBER_OF_CHANNELS - 1;
		}
		return curChannel;
	}

	public int getCurrentValue()
	{
		
		return MLStartupActivity.MAX_CHANNEL_VALUE - (int) (((double) m_point.y / (double) mHeight) * (double) MLStartupActivity.MAX_CHANNEL_VALUE);
	}

	public class FadingRect
	{
		private static final int CYCLE_TIME = 20;
		private static final int FADING_STEP = 20;
		protected static final String TAG = "FadingRect";
		private int mChannel;
		private Paint mPaint;
		private int mAlpha;
		private Handler mHandler;
		private Runnable mAnimationTask;

		private FadingRect(int channel)
		{
			mAlpha = 0;
			mPaint = new Paint();
			mPaint.setStrokeWidth(mChannelPaint.getStrokeWidth());
			mPaint.setColor(mChannelPaint.getColor());
			mChannel = channel;
			mHandler = new Handler();
			// this is a new thread that fades out the alpha
			mAnimationTask = new Runnable()
			{
				public void run()
				{
					// retrigger until we are below 0
					mHandler.postDelayed(mAnimationTask, CYCLE_TIME);
					mAlpha -= FADING_STEP;
					if (mAlpha < 0)
					{
						mAlpha = 0;
						stop();
					}					
					mPaint.setAlpha(mAlpha);
					invalidate();
				}
			};
			// start the fading
			mHandler.postDelayed(mAnimationTask, CYCLE_TIME);
		}

		public void setAlpha(int value)
		{
			if (value > 255 || value < 0)
			{
				return;
			}
			mAlpha = value;
			// if we set a new alpha we retrigger the fading;
			start();
		}

		/**
		 * Draw method that should be called per channel in onDraw() of the View routine
		 * @param canvas
		 */
		public void drawCurrentChannel(Canvas canvas)
		{
			int left = mChannel * mChanWidth;
			int right = (mChannel + 1) * mChanWidth;
			int top = 0;
			int bottom = mHeight;

			mPaint.setAlpha(mAlpha);
			canvas.drawRect(left, top, right, bottom, mPaint);			
		}

		/**
		 * Start the fading
		 */
		private void start()
		{
			mHandler.postDelayed(mAnimationTask, CYCLE_TIME);
		}
		/**
		 * Stop the fading
		 */
		private void stop()
		{
			mHandler.removeCallbacks(mAnimationTask);
		}

	}
}
