package bneumann.btlights;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.UUID;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import bneumann.btlights.R.menu;
import bneumann.btlights.Utils;
import bneumann.btlights.Utils.BTError;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.net.Uri;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.Window;
import android.widget.Toast;

public class BTLightsActivity extends Activity
{
	BluetoothAdapter btAdapter;
	BluetoothSocket btSocket;

	String TAG = "Meister Lampe";
	BluetoothDevice btDevice = null;
	boolean socketConnected = false;
	BTError btError;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		Log.i(TAG, "Meister Lampe is going rabbit wild now...");
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_INDETERMINATE_PROGRESS);
		setContentView(R.layout.main);
		Utils.loadPreferences(this);
		if (Properties.settings_connect_startup)
		{
			if (!Utils.getAdapterStatus())
			{
				Utils.enableAdapter(this);
				return;
			}
			btError = connect();
			if (btError == BTError.BT_NOERROR)
			{
				Utils.showToast(this, "Device connected...");
			} else
			{
				Utils.showToast(this, "Connect faile with: " + btError.name());
			}
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
		Intent intent;
		// Handle item selection
		switch (item.getItemId())
		{
		case R.id.menu_about:
			break;
		case R.id.menu_settings:
			intent = new Intent(BTLightsActivity.this, Settings.class);
			startActivity(intent);
			break;
		case R.id.menu_log:
			intent = new Intent(BTLightsActivity.this, LogViewer.class);
			startActivity(intent);
			break;
		default:
			break;
		}
		return true;
	}

	public void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		if (resultCode == Activity.RESULT_OK)
		{
			// Get the device MAC address
			Properties.settings_device_id = data.getExtras().getString(BTModule.EXTRA_DEVICE_ADDRESS);
			try
			{
				Utils.showToast(this, "Can connect to " + Properties.settings_device_id + " now", Toast.LENGTH_LONG);
				if (btAdapter != null)
				{
					btAdapter.cancelDiscovery();
				}
				connect();
			}
			catch (Exception exp)
			{
				Utils.showToast(this, "Shit address (" + Properties.settings_device_id + ") is invalid?!", Toast.LENGTH_LONG);
			}
			Utils.savePreferences(this);
		}
	}

	public BTError connect()
	{
		return Utils.connect(Properties.settings_device_id);
	}

	public void sequencer_callback(View v)
	{
		if (Properties.settings_device_id != "")
		{
			if (btError != BTError.BT_NOERROR)
			{
				btError = connect();
			} else
			{
				Utils.sendData(0, 12, 0xFFFF, 0);
			}

		}
	}

	public void test_callback(View v)
	{
		if (Properties.settings_device_id != "")
		{
			if (btError != BTError.BT_NOERROR)
			{
				btError = connect();
			} else
			{
				Utils.sendData(0, 9, 0x0001, 0);
				Utils.sendData(0, 9, 0x0002, 10);
				Utils.sendData(0, 9, 0x0004, 20);
				Utils.sendData(0, 9, 0x0008, 30);
				Utils.sendData(0, 9, 0x0010, 40);
				Utils.sendData(0, 9, 0x0020, 50);
				Utils.sendData(0, 9, 0x0040, 60);
				Utils.sendData(0, 9, 0x0080, 70);
				Utils.sendData(0, 9, 0x0100, 80);
				Utils.sendData(0, 9, 0x0200, 90);
				Utils.sendData(0, 9, 0x0400, 100);
				Utils.sendData(0, 9, 0x0800, 110);
				Utils.sendData(0, 9, 0x1000, 120);
				Utils.sendData(0, 9, 0x2000, 130);
				Utils.sendData(0, 9, 0x4000, 140);
				Utils.sendData(0, 9, 0x8000, 150);
				Utils.sendData(0, 4, 0xFFFF, 3);
				Utils.sendData(0, 12, 0xFFFF, 0);
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