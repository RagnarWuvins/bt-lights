package bneumann.btlights;

import java.io.IOException;
import java.io.OutputStream;
import java.util.UUID;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

public class BTLightsActivity extends Activity
{

	BluetoothAdapter mBluetoothAdapter;
	BluetoothSocket btSocket;
	private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB"); // no idea where this comes from :/
	String MLAdress = null;
	String TAG = "Meister Lampe";
	private OutputStream outStream;
	boolean adressAvailable = false;
	BluetoothDevice device = null;
	boolean socketConnected = false;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);
		Intent intent = new Intent(BTLightsActivity.this, BTModule.class);
		startActivityForResult(intent, 1);

	}

	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		if (resultCode == Activity.RESULT_OK)
		{
			// Get the device MAC address
			String address = data.getExtras().getString(BTModule.EXTRA_DEVICE_ADDRESS);
			try
			{
				// Get the BLuetoothDevice object
				mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
				device = mBluetoothAdapter.getRemoteDevice(address);
				Utils.showToast(this, "Can connect to " + address + " now", Toast.LENGTH_LONG);
				adressAvailable = true;
				MLAdress = address;
				mBluetoothAdapter.cancelDiscovery();
			} catch (Exception exp)
			{
				Utils.showToast(this, "Shit address (" + address + ") is invalid?!", Toast.LENGTH_LONG);
				adressAvailable = false;
			}
		}
	}

	public void connect()
	{
		// Attempt to connect to the device
		if (!adressAvailable)
		{
			return;
		}
		try
		{
			btSocket = device.createRfcommSocketToServiceRecord(MY_UUID);
		} catch (IOException e)
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
		mBluetoothAdapter.cancelDiscovery();

		// Blocking connect, for a simple client nothing else can happen
		// until a successful
		// connection is made, so we don't care if it blocks.

		try
		{
			btSocket.connect();
			Log.e(TAG, "ON RESUME: BT connection established, data transfer link open.");
			socketConnected = true;
		} catch (IOException e)
		{
			Log.e(TAG, "Failed to connect socket!", e);
			try
			{
				btSocket.close();
				btSocket.connect();
				socketConnected = false;
			} catch (IOException e2)
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
		} catch (IOException e)
		{
			Log.e(TAG, "ON RESUME: Output stream creation failed.", e);
		}

		int claNmod = (cla << 8) | mode;
		byte[] msgBuffer = { 
				(byte) claNmod,
				(byte) ((address >> 8) & 0xFF),
				(byte) (address & 0xFF),
				(byte) value, 
				(byte) (value + claNmod), 
				(byte) 0xD, 
				(byte) 0xA
				};
		try
		{
			outStream.write(msgBuffer);
		} catch (IOException e)
		{
			Log.e(TAG, "ON RESUME: Exception during write.", e);
		}
	}

	public void sequencer_callback(View v)
	{
		if (MLAdress != null)
		{
			if (!socketConnected)
			{
				connect();
			} else
			{
				sendData(0, 4, 0xFFFF, 3);
			}

		}
	}

	public void test_callback(View v)
	{
		if (MLAdress != null)
		{
			if (!socketConnected)
			{
				connect();
			} else
			{
				sendData(0, 4, 0xFFFF, 4);
			}

		}
	}
	
	public void setup_callback(View v)
	{
		Intent intent = new Intent(BTLightsActivity.this, BTModule.class);
		startActivityForResult(intent, 1);
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
}