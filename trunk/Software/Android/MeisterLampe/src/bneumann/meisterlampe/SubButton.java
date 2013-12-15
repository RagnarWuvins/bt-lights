package bneumann.meisterlampe;

import android.content.Context;
import android.content.res.AssetManager;
import android.content.res.TypedArray;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.util.AttributeSet;
import android.widget.Button;

public class SubButton extends Button
{
	private String mFontName;
	private float mFontSize;

	public SubButton(Context context, AttributeSet attrs)
	{
		super(context, attrs);
		this.setBackgroundResource(R.drawable.button_setup);
		TypedArray tAttributes = context.obtainStyledAttributes(attrs, R.styleable.SubButton, 0, 0);

		this.mFontName = tAttributes.getString(R.styleable.SubButton_fontType);
		this.mFontSize = tAttributes.getFloat(R.styleable.SubButton_fontSize, 12);
		AssetManager asm = getResources().getAssets();
		Typeface font = Typeface.createFromAsset(asm, this.mFontName);
		this.setTypeface(font);
		this.setTextSize(this.mFontSize);

		tAttributes.recycle();
	}

	@Override
	protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{

		int desiredWidth = 1000;
		int widthMode = MeasureSpec.getMode(widthMeasureSpec);
		int widthSize = MeasureSpec.getSize(widthMeasureSpec);

		Drawable d = getResources().getDrawable(R.drawable.button_setup);
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
