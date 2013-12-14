package bneumann.meisterlampe;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.drawable.LayerDrawable;
import android.graphics.drawable.RotateDrawable;
import android.util.AttributeSet;
import android.widget.Button;

public class MainButton extends Button
{
	/** Shows if the button is left (0) or right (2) */
	public int HorizontalPosition;
	/** Shows if the button is top (0) or bottom (1) */
	public int VerticalPosition;
	/**
	 * Shows the enum for the function func 0 power 1 settings 2 level 3
	 */
	public int Function;
	private RotateDrawable mBackground;

	public MainButton(Context context, AttributeSet attrs)
	{
		super(context, attrs);

		TypedArray tAttributes = context.obtainStyledAttributes(attrs, R.styleable.MainButton, 0, 0);
		this.Function = tAttributes.getInteger(R.styleable.MainButton_image, 0);
		this.HorizontalPosition = tAttributes.getInteger(R.styleable.MainButton_horizontalPosition, 0);
		this.VerticalPosition = tAttributes.getInteger(R.styleable.MainButton_verticalPosition, 0);

		int funcId = 0;
		switch (this.Function)
		{
		case 0:
			funcId = R.drawable.button_func;
			break;
		case 1:
			funcId = R.drawable.button_power;
			break;
		case 2:
			funcId = R.drawable.button_settings;
			break;
		case 3:
			funcId = R.drawable.button_level;
			break;
		default:
			break;
		}
		this.setBackgroundResource(funcId);
		LayerDrawable layer = (LayerDrawable) this.getBackground();
		this.mBackground = (RotateDrawable) layer.getDrawable(0);
		this.ApplyRotation();
		tAttributes.recycle();
	}

	private void ApplyRotation()
	{
		// -------
		// |00|01|
		// -------
		// |10|11|
		// -------
		int rotation = 0;
		switch (this.HorizontalPosition + this.VerticalPosition)
		{
		case 0:
			break;
		case 1:
			rotation = 7500;
			break;
		case 2:
			rotation = 2500;
			break;
		case 3:
			rotation = 5000;
			break;
		default:
			break;
		}

		this.mBackground.setLevel(rotation);
	}
}
