package bneumann.meisterlampe;

import java.util.ArrayList;
import java.util.List;
import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.widget.ImageView;

public class DrawView extends ImageView implements OnTouchListener
{

	private static final String TAG = "DrawView";

	private List<Point> mPoints = new ArrayList<Point>();
	private Paint mPaintLine = new Paint();
	private Paint mPaintChannel = new Paint();
	private int[] mChannelValues = new int[Lamp.NUMBER_OF_CHANNELS];

	public DrawView(Context context, AttributeSet attrs)
	{
		super(context, attrs);
		setFocusable(true);
		setFocusableInTouchMode(true);

		this.setOnTouchListener(this);

		mPaintLine = new Paint(Paint.ANTI_ALIAS_FLAG);
		mPaintLine.setStyle(Paint.Style.STROKE);
		mPaintLine.setStrokeWidth(10);
		mPaintLine.setColor(getResources().getColor(R.color.standardText));	
		
		mPaintChannel = new Paint(Paint.ANTI_ALIAS_FLAG);
		mPaintChannel.setStyle(Paint.Style.STROKE);		
		mPaintChannel.setStrokeWidth(10);
		mPaintChannel.setColor(Color.BLUE);	
		
	}

	@Override
	public void onDraw(Canvas canvas) {

	    canvas.drawLine(0, 0, this.getWidth(), 0, mPaintLine);
	    canvas.drawLine(this.getWidth(), 0, this.getWidth(), this.getHeight(), mPaintLine);	    
	    canvas.drawLine(this.getWidth(), this.getHeight(), 0, this.getHeight(), mPaintLine);
	    canvas.drawLine(0, this.getHeight(), 0, 0, mPaintLine);	   
	    
	    
	    int chanWidth = this.getWidth() / (Lamp.NUMBER_OF_CHANNELS + 1);
	    for(int i = 0; i < Lamp.NUMBER_OF_CHANNELS; i++)
	    {
	    	//TODO: Normalize to height!
	    	canvas.drawLine(i*chanWidth + chanWidth, this.getHeight(), i*chanWidth + chanWidth, this.getHeight() - this.mChannelValues[i], mPaintChannel);
	    }
	    
	    //canvas.drawPath(path, paint);
	}
	
	public void setChannelValues(int[] values)
	{
		this.mChannelValues = values;
	}
	
	class Point {
	    float x, y;
	    float dx, dy;

	    @Override
	    public String toString() {
	        return x + ", " + y;
	    }
	}

	public boolean onTouch(View view, MotionEvent event)
	{
		Point point = new Point();
		point.x = event.getX();
		point.y = event.getY();
		mPoints.add(point);
		invalidate();
		Log.d(TAG, "point: " + point);		
		if(event.getAction() == MotionEvent.ACTION_DOWN)
		{
			mPoints.removeAll(mPoints);
		}
		return true;
	}
}

class Point
{
	float x, y;

	@Override
	public String toString()
	{
		return x + ", " + y;
	}
}