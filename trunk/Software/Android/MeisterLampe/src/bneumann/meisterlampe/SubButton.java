package bneumann.meisterlampe;

import android.content.Context;
import android.content.res.AssetManager;
import android.content.res.TypedArray;
import android.graphics.Color;
import android.graphics.Typeface;
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

}
