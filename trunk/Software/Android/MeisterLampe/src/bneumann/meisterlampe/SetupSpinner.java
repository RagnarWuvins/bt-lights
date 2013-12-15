package bneumann.meisterlampe;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.util.AttributeSet;
import android.widget.Spinner;

public class SetupSpinner extends Spinner
{

	public SetupSpinner(Context context, AttributeSet attrs)
	{
		super(context, attrs);
	}

	public SetupSpinner(Context context, AttributeSet attrs, int defStyle)
	{
		super(context, attrs, defStyle);
		// TODO Auto-generated constructor stub
	}

	@Override
	protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{

		int desiredWidth = 1000;
		int widthMode = MeasureSpec.getMode(widthMeasureSpec);
		int widthSize = MeasureSpec.getSize(widthMeasureSpec);

		Drawable d = getResources().getDrawable(R.drawable.button_dropdown);
		int h = d.getIntrinsicHeight();
		int w = d.getIntrinsicWidth();

		double scale = (double) h / (double) w;

		int width;

		// Measure Width
		if (widthMode == MeasureSpec.EXACTLY)
		{
			// Must be this size
			width = widthSize;
		} else if (widthMode == MeasureSpec.AT_MOST)
		{
			// Can't be bigger than...
			width = Math.min(desiredWidth, widthSize);
		} else
		{
			// Be whatever you want
			width = desiredWidth;
		}

		// MUST CALL THIS
		setMeasuredDimension(width, (int) (((double) width) * scale));
	}

}
