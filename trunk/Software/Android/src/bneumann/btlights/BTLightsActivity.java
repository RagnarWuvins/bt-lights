package bneumann.btlights;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Toast;

public class BTLightsActivity extends Activity
{

	BluetoothAdapter mBluetoothAdapter;

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
				BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(address);
			} catch (Exception exp)
			{
				Utils.showToast(this, "Shit address (" + address + ") is invalid?!", Toast.LENGTH_LONG);
			}
			// Attempt to connect to the device
			// TODO: here we finally connect!!!
		}
	}

	public void sequencer_callback(View v)
	{
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