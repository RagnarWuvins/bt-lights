package bneumann.meisterlampe;

import android.annotation.TargetApi;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Build;

public class GeneralNotification extends Notification
{

	private Notification mNotification = null;

	public GeneralNotification(BluetoothService btservice, String label, String text)
	{
		if(Build.VERSION.SDK_INT < Build.VERSION_CODES.HONEYCOMB)
		{
			initFroyo(btservice, label, text);
		}
		else
		{
			initHoneycomb(btservice, label, text);
		}
	}

	@SuppressWarnings("deprecation")
	@TargetApi(Build.VERSION_CODES.HONEYCOMB)
	private void initHoneycomb(BluetoothService btservice, String label, String text)
	{
		Notification.Builder nBuilder = new Notification.Builder(btservice);
		nBuilder.setContentTitle(label);
		nBuilder.setContentText(text);
		nBuilder.setSmallIcon(android.R.drawable.ic_menu_compass);
		nBuilder.setLargeIcon(BitmapFactory.decodeResource(btservice.getResources(), android.R.drawable.ic_dialog_email));
		mNotification = nBuilder.getNotification();
	}
	
	@SuppressWarnings("deprecation")
	@TargetApi(Build.VERSION_CODES.FROYO)
	private void initFroyo(BluetoothService btservice, String label, String text)
	{
		// Set the icon, scrolling text and timestamp
		mNotification = new Notification(android.R.drawable.ic_menu_compass, label + ", " + text, System.currentTimeMillis());
		
		Intent notificationIntent = new Intent(btservice, MainActivity.class);
		notificationIntent.setFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT | Intent.FLAG_ACTIVITY_SINGLE_TOP);

		// The PendingIntent to launch our activity if the user selects this
		// notification
		PendingIntent contentIntent = PendingIntent.getActivity(btservice, 0, notificationIntent, 0);
		// Set the info for the views that show in the notification panel.
		mNotification.setLatestEventInfo(btservice, label, text, contentIntent);
	}
	
	public void show(NotificationManager nManager, int NotificationID)
	{
		// last flag is for tablets
		mNotification.flags = Notification.FLAG_ONGOING_EVENT | Notification.FLAG_NO_CLEAR | Notification.FLAG_ONLY_ALERT_ONCE;

		// Send the notification.
		nManager.notify(NotificationID, mNotification);
	}
}
