package bneumann.btlights;

import android.app.Activity;
import android.content.Context;
import android.widget.Toast;

public class Utils {

	public static void showToast(Activity caller, CharSequence text)
	{
		showToast(caller, text, Toast.LENGTH_SHORT);
	}
	public static void showToast(Activity caller, CharSequence text, int duration)
	{
		Context context = caller.getApplicationContext();
		Toast toast = Toast.makeText(context, text, duration);
		toast.show();
	}
}
