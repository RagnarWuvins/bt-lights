package bneumann.meisterlampe;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.UUID;
import android.annotation.SuppressLint;
import android.app.NotificationManager;
import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Binder;
import android.os.Build;
import android.os.IBinder;
import android.util.Log;
import bneumann.protocol.Package;

/**
 * This class does all the work for setting up and managing Bluetooth
 * connections with other devices. It has a thread that listens for incoming
 * connections, a thread for connecting with a device, and a thread for
 * performing data transmissions when connected.
 */
public class BluetoothService extends Service
{
	// Debugging
	private static final String TAG = "BluetoothService";
	private static final boolean D = true;

	// Unique UUID for this application
	private static final UUID MY_UUID_SECURE = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");
	private static final UUID MY_UUID_INSECURE = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

	// Constants for other Applications
	/** EXTRA FLAG: Bluetooth address of the device that is to be connected */
	public static final String CONNECTION_ADDRESS = "connection_address";
	/** EXTRA FLAG: Tells the service if it should be connected securely */
	public static final String CONNECTION_SECURE = "connection_secure";
	/** Action for new received package */
	public static final String RX_NEW_PACKAGE = "rx_new_package";
	/** Action for new state change */
	public static final String CONNECTION_STATE_CHANGE = "state_change";
	// FIXME: rework the states OR send another event
	public static final String CONNECTION_FAILED = "connection_failed";
	public static final String CONNECTION_DONE = "connection_done";

	// Member fields
	private IBinder mBinder = new BluetoothServiceBinder();
	private BluetoothAdapter mAdapter;
	private ConnectThread mConnectThread;
	private ConnectedThread mConnectedThread;
	private int mState;
	private NotificationManager mNM;
	private int NOTIFICATION = 42;
	private Lamp mLamp;
	private BluetoothDevice mBluetoothDevice;

	// Constants that indicate the current connection state
	public static final int STATE_NONE = 0; // we're doing nothing
	public static final int STATE_CONNECTING = 1; // now initiating an outgoing
													// connection
	public static final int STATE_CONNECTED = 2; // now connected to a remote
													// device
	public static final int STATE_CONNECTION_LOST = 3; // connection could not
														// be established or has
														// been lost

	private BroadcastReceiver mMessageReceiver = new BroadcastReceiver()
	{
		@Override
		public void onReceive(Context context, Intent intent)
		{
			// TODO: for API > 11 it is possible to listen to connection
			// changes. How cool would that be!
			String action = intent.getAction();
			if (action.equals(BluetoothAdapter.ACTION_STATE_CHANGED))
			{
				int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, -1);
				// remap Bluetoothadapter state to internal Service state
				if (state == BluetoothAdapter.STATE_OFF)
				{
					setState(STATE_NONE);
					disconnect(false);
				} else
				{
					// if we already were connected feel free to reconnect
					if (mBluetoothDevice != null)
					{
						connect(mBluetoothDevice, true);
					}
				}
			}
		}
	};

	// ---------------------------------------
	// Start of the Service overridden methods
	// ---------------------------------------

	@Override
	public void onCreate()
	{		
		this.mAdapter = BluetoothAdapter.getDefaultAdapter();
		this.mState = STATE_NONE;
		this.mLamp = new Lamp();

		// register for BluetoothAdapter change of state
		registerReceiver(mMessageReceiver, new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED));
		
		// showing the notification manager to signal we are here
		this.mNM = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
		showNotification("Bla", "test");
		
		super.onCreate();
	}

	@Override
	public IBinder onBind(Intent intent)
	{
		// initFromIntent(intent);
		return mBinder;
	}

	// We need to implement a binder who returns this class at onConnection!
	public class BluetoothServiceBinder extends Binder
	{
		BluetoothService getService()
		{
			return BluetoothService.this;
		}
	}

	/***
	 * Starts the service. The intent should have CONNECTION_ADDRESS and
	 * CONNECTION_SECURE as extras
	 */
	@Override
	public int onStartCommand(Intent intent, int flags, int startId)
	{
		initFromIntent(intent);
		return super.onStartCommand(intent, flags, startId);
	}

	@Override
	public void onDestroy()
	{
		disconnect(false);
		unregisterReceiver(mMessageReceiver);
		this.mNM.cancelAll();
		super.onDestroy();
	}

	// ---------------------------------------
	// Here some helper functions
	// ---------------------------------------

	private void initFromIntent(Intent intent)
	{
		if (intent.hasExtra(BluetoothService.CONNECTION_ADDRESS))
		{
			String deviceAddress = intent.getStringExtra(BluetoothService.CONNECTION_ADDRESS);
			boolean secure = intent.getBooleanExtra(BluetoothService.CONNECTION_SECURE, true);
			mBluetoothDevice = this.mAdapter.getRemoteDevice(deviceAddress);
			connect(mBluetoothDevice, secure);
		}
	}

	/**
	 * Show a notification while this service is running.
	 */
	// TODO: fix for newer versions
	private void showNotification(String label, String text)
	{
		// Set the icon, scrolling text and timestamp
		GeneralNotification notification = new GeneralNotification(this, label, text);
		notification.show(this.mNM, NOTIFICATION);
	}

	// ---------------------------------------
	// Lamp connection management
	// ---------------------------------------

	private synchronized boolean updateLamp(byte[] encodedBytes)
	{
		Package p;
		boolean status = false;
		try
		{
			p = new Package(encodedBytes);
			mLamp.update(p);
			status = true;
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}
		return status;

	}

	public synchronized Lamp getLamp()
	{
		return this.mLamp;
	}

	public synchronized void queryLampUpdate()
	{
		this.write(this.mLamp.getUpdatePackage().getBytes());
	}

	public synchronized void queryErrorLog()
	{
		this.write(this.mLamp.getErrorLogRequestPackage().getBytes());
	}

	// ---------------------------------------
	// Here comes the Bluetooth code
	// ---------------------------------------

	/**
	 * Set the current state of the chat connection
	 * 
	 * @param state
	 *            An integer defining the current connection state
	 */
	private synchronized void setState(int state)
	{
		if (D)
			Log.d(TAG, "setState() " + mState + " -> " + state);
		mState = state;
		Intent intent = new Intent(CONNECTION_STATE_CHANGE);
		intent.putExtra(CONNECTION_STATE_CHANGE, state);
		sendBroadcast(intent);
	}

	/**
	 * Return the current connection state.
	 */
	public synchronized int getState()
	{
		return mState;
	}

	/**
	 * Force sending a broadcast with our current state
	 */
	public synchronized void queryState()
	{
		this.setState(this.mState);
	}

	/**
	 * Start the ConnectThread to initiate a connection to a remote device.
	 * 
	 * @param device
	 *            The BluetoothDevice to connect
	 * @param secure
	 *            Socket Security type - Secure (true) , Insecure (false)
	 */
	public synchronized void connect(BluetoothDevice device, boolean secure)
	{
		if (D)
			Log.d(TAG, "connect to: " + device);

		// Cancel any thread attempting to make a connection
		if (mState == STATE_CONNECTING)
		{
			if (mConnectThread != null)
			{
				mConnectThread.cancel();
				mConnectThread = null;
			}
		}

		// Cancel any thread currently running a connection
		if (mConnectedThread != null)
		{
			mConnectedThread.cancel();
			mConnectedThread = null;
		}

		// Start the thread to connect with the given device
		mConnectThread = new ConnectThread(device, secure);
		mConnectThread.start();
		setState(STATE_CONNECTING);
	}

	/***
	 * Disconnect and stop all threads
	 */
	public synchronized void disconnect(boolean stopService)
	{
		if (D)
			Log.d(TAG, "disconnect");

		if (mConnectThread != null)
		{
			mConnectThread.cancel();
			mConnectThread = null;
		}

		if (mConnectedThread != null)
		{
			mConnectedThread.cancel();
			mConnectedThread = null;
		}
		setState(STATE_NONE);
		// stopping the service
		if (stopService)
		{
			stopSelf();
		}
	}

	/**
	 * Start the ConnectedThread to begin managing a Bluetooth connection
	 * 
	 * @param socket
	 *            The BluetoothSocket on which the connection was made
	 * @param device
	 *            The BluetoothDevice that has been connected
	 */
	public synchronized void connected(BluetoothSocket socket, BluetoothDevice device, final String socketType)
	{
		if (D)
			Log.d(TAG, "connected, Socket Type:" + socketType);

		// Cancel the thread that completed the connection
		if (mConnectThread != null)
		{
			mConnectThread.cancel();
			mConnectThread = null;
		}

		// Cancel any thread currently running a connection
		if (mConnectedThread != null)
		{
			mConnectedThread.cancel();
			mConnectedThread = null;
		}

		// Start the thread to manage the connection and perform transmissions
		mConnectedThread = new ConnectedThread(socket, socketType);
		mConnectedThread.start();

		setState(STATE_CONNECTED);
	}

	/**
	 * Write to the ConnectedThread in an unsynchronized manner
	 * 
	 * @param out
	 *            The bytes to write
	 * @see ConnectedThread#write(byte[])
	 */
	public void write(byte[] out)
	{
		// Create temporary object
		ConnectedThread r;
		// Synchronize a copy of the ConnectedThread
		synchronized (this)
		{
			if (mState != STATE_CONNECTED)
			{
				return;
			}
			r = mConnectedThread;
		}
		// Perform the write unsynchronized
		r.write(out);
	}

	public void write(Package data)
	{
		write(data.getBytes());
	}

	/**
	 * Indicate that the connection attempt failed and notify the UI Activity.
	 */
	private void connectionFailed()
	{
		setState(STATE_CONNECTION_LOST);
	}

	/**
	 * This thread runs while attempting to make an outgoing connection with a
	 * device. It runs straight through; the connection either succeeds or
	 * fails.
	 */
	@SuppressLint("NewApi")
	private class ConnectThread extends Thread
	{
		private final BluetoothSocket mmSocket;
		private final BluetoothDevice mmDevice;
		private String mSocketType;

		public ConnectThread(BluetoothDevice device, boolean secure)
		{
			mmDevice = device;
			BluetoothSocket tmp = null;
			mSocketType = secure ? "Secure" : "Insecure";

			// Get a BluetoothSocket for a connection with the
			// given BluetoothDevice
			try
			{
				// Connection unsecure is only allowed from API 10 upwards :|
				// (lint suppression on top!)
				if (secure || (Build.VERSION.SDK_INT < Build.VERSION_CODES.GINGERBREAD_MR1))
				{
					tmp = device.createRfcommSocketToServiceRecord(MY_UUID_SECURE);
				} else
				{
					tmp = device.createInsecureRfcommSocketToServiceRecord(MY_UUID_INSECURE);
				}
			}
			catch (IOException e)
			{
				Log.e(TAG, "Socket Type: " + mSocketType + "create() failed", e);
			}
			mmSocket = tmp;
		}

		public void run()
		{
			Log.i(TAG, "BEGIN mConnectThread SocketType:" + mSocketType);
			setName("ConnectThread" + mSocketType);

			// Always cancel discovery because it will slow down a connection
			mAdapter.cancelDiscovery();

			// Make a connection to the BluetoothSocket
			try
			{
				// This is a blocking call and will only return on a
				// successful connection or an exception
				mmSocket.connect();
			}
			catch (IOException e)
			{
				Log.e(TAG, "unable to connect " + mSocketType + " socket during connection, closing socket first", e);
				// Close the socket
				try
				{
					mmSocket.close();
				}
				catch (IOException e2)
				{
					Log.e(TAG, "unable to close() " + mSocketType + " socket during connection failure", e2);
				}
				Log.e(TAG, "Socket opening failed because of: " + e);
				connectionFailed();
				return;
			}

			// Reset the ConnectThread because we're done
			synchronized (BluetoothService.this)
			{
				mConnectThread = null;
			}

			// Start the connected thread
			connected(mmSocket, mmDevice, mSocketType);
		}

		public void cancel()
		{
			try
			{
				mmSocket.close();
			}
			catch (IOException e)
			{
				Log.e(TAG, "close() of connect " + mSocketType + " socket failed", e);
			}
		}
	}

	/**
	 * This thread runs during a connection with a remote device. It handles all
	 * incoming and outgoing transmissions.
	 */
	private class ConnectedThread extends Thread
	{
		private final BluetoothSocket mmSocket;
		private final InputStream mmInStream;
		private final OutputStream mmOutStream;
		private boolean stopWorker;
		private int readBufferPosition;
		private byte[] readBuffer;

		public ConnectedThread(BluetoothSocket socket, String socketType)
		{
			Log.d(TAG, "create ConnectedThread: " + socketType);
			mmSocket = socket;
			InputStream tmpIn = null;
			OutputStream tmpOut = null;

			stopWorker = false;
			readBufferPosition = 0;
			readBuffer = new byte[1024];
			// END NEW DRIVER

			// Get the BluetoothSocket input and output streams
			try
			{
				tmpIn = socket.getInputStream();
				tmpOut = socket.getOutputStream();
			}
			catch (IOException e)
			{
				Log.e(TAG, "temp sockets not created", e);
			}

			mmInStream = tmpIn;
			mmOutStream = tmpOut;
		}

		public void run()
		{
			while (!Thread.currentThread().isInterrupted() && !stopWorker)
			{
				try
				{
					int bytesAvailable = mmInStream.available();
					if (bytesAvailable > 0)
					{
						byte[] packetBytes = new byte[bytesAvailable];
						mmInStream.read(packetBytes);
						for (int i = 0; i < bytesAvailable; i++)
						{
							// check if we might have read stupid data:
							if (readBuffer[0] > 10 || readBuffer[1] > 10)
							{
								readBuffer = new byte[readBuffer.length];
								readBufferPosition = 0;
								break;
							}
							byte b = packetBytes[i];
							int packageLength = readBuffer[3] & 0xff;
							if (readBufferPosition >= packageLength * 4 - 1 && readBufferPosition > 3)
							{
								byte[] encodedBytes = new byte[packageLength * 4];
								System.arraycopy(readBuffer, 0, encodedBytes, 0, encodedBytes.length - 1);
								updateLamp(encodedBytes);
								Intent intent = new Intent(RX_NEW_PACKAGE);
								intent.putExtra(RX_NEW_PACKAGE, encodedBytes);
								sendBroadcast(intent);
								readBuffer = new byte[readBuffer.length];
								readBufferPosition = 0;
							} else
							{
								readBuffer[readBufferPosition++] = b;
							}
						}
					}
				}
				catch (IOException ex)
				{
					stopWorker = true;
				}
			}
		}

		/**
		 * Write to the connected OutStream.
		 * 
		 * @param buffer
		 *            The bytes to write
		 */
		public void write(byte[] buffer)
		{
			try
			{
				mmOutStream.write(buffer);
				final StringBuilder sb = new StringBuilder();
				for (byte by : buffer)
				{
					sb.append(String.format("%02X ", by));
				}
				Log.d("SendModule", "Sent: " + sb);

			}
			catch (IOException e)
			{
				Log.e(TAG, "Exception during write", e);
			}
		}

		public void cancel()
		{
			try
			{
				mmSocket.close();
			}
			catch (IOException e)
			{
				Log.e(TAG, "close() of connect socket failed", e);
			}
		}
	}

}
