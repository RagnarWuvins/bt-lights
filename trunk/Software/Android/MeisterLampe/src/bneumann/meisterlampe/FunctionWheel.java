package bneumann.meisterlampe;

import android.content.Context;
import android.content.res.Configuration;
import android.graphics.Point;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.util.Log;
import android.view.Display;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

public class FunctionWheel extends ViewGroup
{

	/** These are used for computing child frames based on their gravity. */
	private final Rect mTmpChildRect = new Rect(0, 100, 100, 0);
	private Point mScreen = new Point();
	public double Scale = 0.9;
	private Context mContext;
	private int mOrientation;
	private boolean mEnabled = true;

	public FunctionWheel(Context context, AttributeSet attrs)
	{
		super(context, attrs);
		this.mScreen.x = context.getResources().getDisplayMetrics().widthPixels;
		this.mScreen.y = context.getResources().getDisplayMetrics().heightPixels;
		this.mContext = context;
	}

	/**
	 * Returns the width and height size of the wheel. It depends on the Scale parameter.
	 * 
	 * @return Scale * Screenwidth
	 */
	public int getSize()
	{
		if(this.mOrientation == Configuration.ORIENTATION_LANDSCAPE)
		{
			return (int) (((double) this.mScreen.y) * this.Scale);
		}
		else
		{
			return (int) (((double) this.mScreen.x) * this.Scale);
		}
	}

	public int getMargin()
	{
		return this.mScreen.x - this.getSize();
	}

	public void setChildEnabled(int[] index, boolean state)
	{
		for(int i = 0; i < index.length; i++)
		{
			setChildEnabled(index[i], state);
		}
	}
	
	public void setChildEnabled(int index, boolean state)
	{
		final int count = getChildCount();
		if (index >= count || index < 0)
		{
			return;
		}
		((Button)this.getChildAt(index)).setEnabled(state);
	}
	
	public boolean getChildEnabled(int index)
	{
		final int count = getChildCount();
		if (index >= count || index < 0)
		{
			return false;
		}
		return ((Button)this.getChildAt(index)).isEnabled();
	}
	
	public void setEnabled(boolean state)
	{
		this.mEnabled = state;
		final int count = getChildCount();
		
		for (int i = 0; i < count; i++)
		{
			final View child = getChildAt(i);
			if (!(child instanceof MainButton))
			{
				continue;
			}
			MainButton button = (MainButton) child;
			button.setEnabled(state);
		}
	}
	
	public boolean getEnabled()
	{
		return this.mEnabled;
	}
	
	/**
	 * Any layout manager that doesn't scroll will want this.
	 */
	@Override
	public boolean shouldDelayChildPressedState()
	{
		return false;
	}

	@Override
	protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{
		super.onMeasure(widthMeasureSpec, heightMeasureSpec);
		// Report our final dimensions.
		setMeasuredDimension(this.getSize(), this.getSize());
	}

	@Override
	protected void onLayout(boolean changed, int left, int top, int right, int bottom)
	{
		final int count = getChildCount();
		int kidSize = this.getSize() / 2;
		this.mOrientation = this.mContext.getResources().getConfiguration().orientation;
		
		for (int i = 0; i < count; i++)
		{
			final View child = getChildAt(i);
			if (!(child instanceof MainButton))
			{
				continue;
			}
			MainButton button = (MainButton) child;
			if (button.getVisibility() != GONE)
			{
				// Place the child.
				switch (button.HorizontalPosition + button.VerticalPosition)
				{
				case 0:
					button.layout(0, 0, kidSize, kidSize);
					break;
				case 1:
					button.layout(0, kidSize, kidSize, this.getSize());
					break;
				case 2:
					button.layout(kidSize, 0, this.getSize(), kidSize);
					break;
				case 3:
					button.layout(kidSize, kidSize, this.getSize(), this.getSize());
					break;
				}

			}
		}
	}
}
