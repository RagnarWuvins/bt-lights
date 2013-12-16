/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package bneumann.meisterlampe;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Date;
import java.util.Iterator;
import java.util.UUID;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.content.Context;
import android.os.Handler;
import android.util.Log;

/**
 * This class does all the work for setting up and managing Bluetooth connections with other devices. It has a thread that listens for incoming connections, a
 * thread for connecting with a device, and a thread for performing data transmissions when connected.
 */
public class BluetoothService
{
	// Debugging
	private static final String TAG = "BluetoothService";
	private static final boolean D = true;

	// Unique UUID for this application
	private static final UUID MY_UUID_SECURE = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");
	private static final UUID MY_UUID_INSECURE = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

	// Member fields
	private final BluetoothAdapter mAdapter;
	private static ArrayList<Handler> mHandler;
	private ConnectThread mConnectThread;
	private ConnectedThread mConnectedThread;
	private int mState;
	private ArrayList<DataReceivedListener> mDataReceivedListener = new ArrayList<DataReceivedListener>();

	// Constants that indicate the current connection state
	public static final int STATE_NONE = 0; // we're doing nothing
	public static final int STATE_CONNECTING = 1; // now initiating an outgoing
													// connection
	public static final int STATE_CONNECTED = 2; // now connected to a remote
													// device

	// Define our custom Listener interface
	public interface DataReceivedListener
	{
		public abstract void OnDataReceive(byte[] command);
	}

	public void AddDataReceivedListener(DataReceivedListener listener)
	{
		// Store the listener object
		this.mDataReceivedListener.add(listener);
	}

	/**
	 * Constructor. Prepares a new BluetoothChat session.
	 * 
	 * @param context
	 *            The UI Activity Context
	 * @param handler
	 *            A Handler to send messages back to the UI Activity
	 */
	public BluetoothService(Context context)
	{
		mAdapter = BluetoothAdapter.getDefaultAdapter();
		mState = STATE_NONE;
		mHandler = new ArrayList<Handler>();
	}

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

		// Give the new state to the Handler so the UI Activity can update
		sendMessages(MainActivity.MESSAGE_STATE_CHANGE, state, -1, null);
	}

	private void sendMessages(int what, int arg1, int arg2, Object obj)
	{
		Iterator<Handler> it = mHandler.iterator();
		while (it.hasNext())
		{
			it.next().obtainMessage(what, arg1, arg2, obj).sendToTarget();
		}
	}

	/**
	 * Return the current connection state.
	 */
	public synchronized int getState()
	{
		return mState;
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
	 * 
	 * @param device
	 * @param secure
	 */
	public synchronized void disconnect()
	{
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
	 * Stop all threads
	 */
	public synchronized void stop()
	{
		if (D)
			Log.d(TAG, "stop");

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

	public boolean sendMessage(byte[] message)
	{
		boolean intState = false;
		// Check that we're actually connected before trying anything
		if (getState() != STATE_CONNECTED)
		{
			intState = false;
			return intState;
		}

		// Check that there's actually something to send
		if (message.length > 0)
		{
			write(message);
			intState = true;
		}
		return intState;
	}

	/**
	 * Indicate that the connection attempt failed and notify the UI Activity.
	 */
	private void connectionFailed()
	{
		// FIXME: Return connection fail message	
	}

	/**
	 * This thread runs while attempting to make an outgoing connection with a device. It runs straight through; the connection either succeeds or fails.
	 */
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
				if (secure)
				{
					tmp = device.createRfcommSocketToServiceRecord(MY_UUID_SECURE);
				}
				else
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
				// Close the socket
				try
				{
					mmSocket.close();
				}
				catch (IOException e2)
				{
					Log.e(TAG, "unable to close() " + mSocketType + " socket during connection failure", e2);
				}
				Log.e(TAG, "Socket opening faile because of: " + e);
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
	 * This thread runs during a connection with a remote device. It handles all incoming and outgoing transmissions.
	 */
	private class ConnectedThread extends Thread
	{
		private final BluetoothSocket mmSocket;
		private final InputStream mmInStream;
		private final OutputStream mmOutStream;
		private boolean stopWorker;
		private int readBufferPosition;
		private byte[] readBuffer;
		private final byte CR, LF;
		private int ACKcounter = 0;

		public ConnectedThread(BluetoothSocket socket, String socketType)
		{
			Log.d(TAG, "create ConnectedThread: " + socketType);
			mmSocket = socket;
			InputStream tmpIn = null;
			OutputStream tmpOut = null;

			// START NEW DRIVER
			CR = 0xA; // This is the ASCII code for a carriage return
			LF = 0xD; // This is the ASCII code for a line feed

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
							byte b = packetBytes[i];
							byte b4 = i == 0 ? 0 : packetBytes[i - 1];
							if ((b == CR) && (b4 == LF))
							{
								byte[] encodedBytes = new byte[readBufferPosition];
								System.arraycopy(readBuffer, 0, encodedBytes, 0, encodedBytes.length - 1);
								final StringBuilder sb = new StringBuilder();
								for (byte by : encodedBytes)
								{
									sb.append(String.format("%02X ", by));
								}
								// Log.d("ReceiveModule","Received: " + sb + " at " + TimeDiff());
								readBufferPosition = 0;
								for (DataReceivedListener listener : mDataReceivedListener)
								{
									listener.OnDataReceive(encodedBytes);
								}
								ACKcounter++;
								Log.d("BluetoothService", "DATA received: " + ACKcounter);
							}
							else
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
				// Log.d("SendModule", "Sent: " + sb + " at " + TimeDiff());

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
