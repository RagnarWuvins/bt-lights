package bneumann.btlights;

import java.io.IOException;
import java.io.OutputStream;
import java.util.UUID;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Window;
import android.widget.Toast;

public class Utils {

	public static BTError btError = BTError.BT_NOERROR;
	private static BTState btStatus = BTState.BS_NOT_CONNECTED;
	public static BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();
	public static BluetoothDevice btDevice;
	public static BluetoothSocket btSocket;
	private static String TAG = "Utils";
	private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");
	private static OutputStream outStream;
	
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
	
	public static void enableAdapter(Activity act)
	{
		if (!btAdapter.isEnabled())
		{
			Log.d(TAG, "BT not ready, asking user to activate");
			Intent enableBluetooth = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			act.startActivityForResult(enableBluetooth, 0);
		}
	}
	
	public static boolean getAdapterStatus()
	{
		return btAdapter.isEnabled();
	}
	
	public static BTError connect(String deviceAdress)
	{
		// Attempt to connect to the device
		if (deviceAdress == "")
		{
			Log.d(TAG , "No address for device found. Use Bluetooth setup first");
			btStatus = BTState.BS_NOT_CONNECTED;
			return BTError.BT_NOADDRESS;
		}
		try
		{
			// Get the BLuetoothDevice object			
			btDevice = btAdapter.getRemoteDevice(deviceAdress);
			btSocket = btDevice.createRfcommSocketToServiceRecord(MY_UUID);
		}
		catch (IOException e)
		{
			Log.e(TAG, "Socket creation failed.", e);
			btStatus = BTState.BS_ERROR;
			return BTError.BT_CONNECT_FAILED;
		}

		// Discovery may be going on, e.g., if you're running a 'scan for
		// devices' search
		// from your handset's Bluetooth settings, so we call
		// cancelDiscovery(). It doesn't
		// hurt to call it, but it might hurt not to... discovery is a
		// heavyweight process;
		// you don't want it in progress when a connection attempt is made.
		btAdapter.cancelDiscovery();

		// Blocking connect, for a simple client nothing else can happen
		// until a successful
		// connection is made, so we don't care if it blocks.

		try
		{
			btSocket.connect();
			Log.d(TAG, "BT connection established, data transfer link open.");
			btStatus = BTState.BS_CONNECTED;
			btError = BTError.BT_NOERROR;
		}
		catch (IOException e)
		{
			Log.d(TAG, "Failed to connect socket! Trying again", e);
			try
			{
				btSocket.close();
				btSocket.connect();
				btError = BTError.BT_NOERROR;
			}
			catch (IOException e2)
			{
				Log.e(TAG, "Unable to close socket during connection failure", e2);
				btError = BTError.BT_CLOSE_FAILED;
				btStatus = BTState.BS_ERROR;
			}
		}
		return btError;
	}
	
	public static BTError sendData(int cla, int mode, int address, int value)
	{
		
		// Create a data stream so we can talk to server.
		try
		{
			outStream = btSocket.getOutputStream();
			Log.d(TAG, "outstream initiated");
		}
		catch (IOException e)
		{			
			Log.e(TAG, "Output stream creation failed.", e);
			btStatus = BTState.BS_ERROR;
			return BTError.BT_SOCKET_CREATION_FAILED;
		}

		int claNmod = (cla << 8) | mode;
		byte[] msgBuffer = { (byte) claNmod, (byte) ((address >> 8) & 0xFF), (byte) (address & 0xFF), (byte) value, (byte) (value + claNmod), (byte) 0xD, (byte) 0xA };
		try
		{
			outStream.write(msgBuffer);
				try
				{
					Thread.sleep(10);
				}
				catch (InterruptedException e)
				{
					Log.w(TAG, "Message wait could not be executed");
				}
				
		}
		catch (IOException e)
		{
			Log.e(TAG, "Exception during write.", e);
			btStatus = BTState.BS_ERROR;
			return BTError.BT_WRITE_FAILED;
		}
		btStatus = BTState.BS_CONNECTED;
		Log.d(TAG, "Command has been written");
		return BTError.BT_NOERROR;
	}

	
	public static void loadPreferences(Context context)
	{
		SharedPreferences settings = PreferenceManager.getDefaultSharedPreferences(context);
		Properties.settings_device_activate_bluetooth = settings.getBoolean("settings_device_activate_bluetooth", true);
		Properties.settings_connect_startup = settings.getBoolean("settings_connect_startup", true);
		Properties.settings_device_id = settings.getString("settings_device_id", "");
		Log.i(TAG, "Settings loaded");
	}

	public static void savePreferences(Context context)
	{
		SharedPreferences settings = PreferenceManager.getDefaultSharedPreferences(context);
		SharedPreferences.Editor editor = settings.edit();
		editor.putBoolean("settings_device_activate_bluetooth", Properties.settings_device_activate_bluetooth);
		editor.putBoolean("settings_connect_startup", Properties.settings_connect_startup);
		editor.putString("settings_device_id", Properties.settings_device_id);

		// after adding all fields, save
		editor.commit();
		Log.i(TAG, "Settings saved");
	}
	
	public static boolean getBluetoothState()
	{
		if (btStatus != BTState.BS_CONNECTED)
		{
			return false;
		}
		return true;
	}
	
	public static enum BTError
	{
		BT_NOERROR,
		BT_NOADDRESS,
		BT_CONNECT_FAILED,
		BT_CLOSE_FAILED,
		BT_SOCKET_CREATION_FAILED,
		BT_WRITE_FAILED,
	}
	
	public static enum BTState
	{
		BS_NOT_CONNECTED,
		BS_CONNECTED,
		BS_ERROR,
	}
}
