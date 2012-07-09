package bneumann.btlights;

import java.io.IOException;
import java.io.OutputStream;
import java.util.UUID;
import bneumann.btlights.R.menu;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.Toast;

public class BTLightsActivity extends Activity
{
	BluetoothAdapter btAdapter;
	BluetoothSocket btSocket;
	private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB"); // no
																									// :/
	String TAG = "Meister Lampe";
	private OutputStream outStream;
	BluetoothDevice btDevice = null;
	boolean socketConnected = false;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);
		loadPreferences();
		if (Properties.btConnectAtStartup)
		{
			connect();
		}
	}

	/*
	 * This is the menu call
	 */
	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		MenuInflater inflater = new MenuInflater(this);
		inflater.inflate(R.menu.bt_menu, menu);
		return true;
	}

	/*
	 * This is the menu handler
	 */
	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		// Handle item selection
		switch (item.getItemId())
		{
		case R.id.menu_about:
			break;
		case R.id.menu_settings:
			Intent intent = new Intent(BTLightsActivity.this, BTModule.class);
			startActivityForResult(intent, 1);
			break;
		default:

		}
		return true;
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		if (resultCode == Activity.RESULT_OK)
		{
			// Get the device MAC address
			Properties.btAddress = data.getExtras().getString(BTModule.EXTRA_DEVICE_ADDRESS);
			try
			{
				Utils.showToast(this, "Can connect to " + Properties.btAddress + " now", Toast.LENGTH_LONG);
				btAdapter.cancelDiscovery();
			}
			catch (Exception exp)
			{
				Utils.showToast(this, "Shit address (" + Properties.btAddress + ") is invalid?!", Toast.LENGTH_LONG);
			}
			savePreferences();
		}
	}

	public void connect()
	{
		// Attempt to connect to the device
		if (Properties.btAddress == "")
		{
			Utils.showToast(this, "No address for device found. Use Bluetooth setup first");
			Log.d(TAG, "No address for device found. Use Bluetooth setup first");
			return;
		}
		try
		{
			// Get the BLuetoothDevice object
			btAdapter = BluetoothAdapter.getDefaultAdapter();
			btDevice = btAdapter.getRemoteDevice(Properties.btAddress);
			btSocket = btDevice.createRfcommSocketToServiceRecord(MY_UUID);
		}
		catch (IOException e)
		{
			Log.e(TAG, "ON RESUME: Socket creation failed.", e);
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
			Log.d(TAG, "ON RESUME: BT connection established, data transfer link open.");
			socketConnected = true;
		}
		catch (IOException e)
		{
			Log.e(TAG, "Failed to connect socket!", e);
			try
			{
				btSocket.close();
				btSocket.connect();
				socketConnected = false;
			}
			catch (IOException e2)
			{
				Log.e(TAG, "ON RESUME: Unable to close socket during connection failure", e2);
			}
		}
	}

	public void sendData(int cla, int mode, int address, int value)
	{
		// Create a data stream so we can talk to server.
		try
		{
			outStream = btSocket.getOutputStream();
		}
		catch (IOException e)
		{
			Log.e(TAG, "ON RESUME: Output stream creation failed.", e);
		}

		int claNmod = (cla << 8) | mode;
		byte[] msgBuffer = { (byte) claNmod, (byte) ((address >> 8) & 0xFF), (byte) (address & 0xFF), (byte) value, (byte) (value + claNmod), (byte) 0xD, (byte) 0xA };
		try
		{
			outStream.write(msgBuffer);
		}
		catch (IOException e)
		{
			Log.e(TAG, "ON RESUME: Exception during write.", e);
		}
	}

	public void sequencer_callback(View v)
	{
		if (Properties.btAddress != "")
		{
			if (!socketConnected)
			{
				connect();
			} else
			{
				sendData(1, 6, 0xFFFF, 3);
				sendData(0, 4, 0xFFFF, 3);
			}

		}
	}

	public void test_callback(View v)
	{
		if (Properties.btAddress != "")
		{
			if (!socketConnected)
			{
				connect();
			} else
			{
				sendData(0, 9, 0x0001, 0);
				sendData(0, 9, 0x0002, 50);
				sendData(0, 9, 0x0004, 100);
				sendData(0, 9, 0x0008, 150);
				sendData(0, 9, 0x0010, 200);
				sendData(0, 9, 0x0020, 250);
				sendData(0, 9, 0x0040, 300);
				sendData(0, 9, 0x0080, 350);
				sendData(0, 9, 0x0100, 400);
				sendData(0, 9, 0x0200, 450);
				sendData(0, 9, 0x0400, 500);
				sendData(0, 9, 0x0800, 550);
				sendData(0, 9, 0x1000, 600);
				sendData(0, 9, 0x2000, 650);
				sendData(0, 9, 0x4000, 700);
				sendData(0, 9, 0x8000, 750);
				sendData(0, 4, 0xFFFF, 3);
			}

		}
	}

	public void functiongrid_callback(View v)
	{
		Intent intent = new Intent(BTLightsActivity.this, FunctionGrid.class);
		startActivity(intent);
	}

	public void about_callback(View v)
	{
		Intent intent = new Intent(BTLightsActivity.this, AboutBTLightsActivity.class);
		startActivity(intent);
	}

	public void loadPreferences()
	{

		SharedPreferences settings = getSharedPreferences(Properties.prefFilename, 0);
		Properties.btAddress = settings.getString("btAddress", "");
		Properties.btConnectAtStartup = settings.getBoolean("btConnectAtStartup", false);
	}

	public void savePreferences()
	{
		SharedPreferences settings = getSharedPreferences(Properties.prefFilename, 0);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString("btAddress", Properties.btAddress);
		editor.putBoolean("btConnectAtStartup", Properties.btConnectAtStartup);

		// after adding all fields, save
		editor.commit();
	}
}