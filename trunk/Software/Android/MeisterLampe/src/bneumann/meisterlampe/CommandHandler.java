package bneumann.meisterlampe;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Date;
import java.util.Iterator;
import bneumann.meisterlampe.Lamp.Channel;
import android.content.Context;
import android.os.Handler;
import android.os.Message;
import android.util.Log;

public class CommandHandler
{
	// Define our custom Listener interface
	public interface ChannelCommandReceivedListener
	{
		public abstract void OnCommandReceive(int command, int address, int payload);
	}	
	
	public interface GlobalCommandReceivedListener
	{
		public abstract void OnCommandReceive(Command command);
	}	
	
	private ArrayList<ChannelCommandReceivedListener> mChannelListener;
	private ArrayList<GlobalCommandReceivedListener> mGlobalListener;
	
	public static final class CLASS
	{
		/** Channel command class */
		public static final int CLA_CHANNEL = 0x00;
		/** Global command class */
		public static final int CLA_GLOBAL = 0x01;
		/** AT (Bluetooth module) command class */
		public static final int CLA_AT = 0x02;
		/** Direct port command class */
		public static final int CLA_DIRECT = 0x03;
		/** Names of the classes */
		public static final String[] Names = new String[]{"CHANNEL", "GLOBAL", "AT", "DIRECT"};
	}

	public static final class MODE
	{
		/** no change of current mode */
		public static final int MOD_NOOP = 0x00;
		/** set fix value */
		public static final int MOD_DIRECT = 0x01;
		/** set to on */
		public static final int MOD_ON = 0x02;
		/** set to off */
		public static final int MOD_OFF = 0x03;
		/** set a preprogrammed function */
		public static final int MOD_FUNC = 0x04;		
		/** Names of the modes */
		public static final String[] Names = new String[]{"NOOP", "DIRECT", "ON", "OFF", "FUNC"};
	}
	
	public static final String[] FUNCTION_NAMES = {"No operation", "Direct mode", "On", "Off", "Sweep"};;
	
	public static final class COMMANDS
	{
		public static final int CMD_SET_MODE = 0x00;   // 0x00: Set the channel mode
		public static final int CMD_GET_MODE = 0x01;    // 0x01: Get the channel mode
		public static final int CMD_SET_VAL = 0x02;     // 0x02: Set current value
		public static final int CMD_GET_VAL = 0x03;     // 0x03: Get current value
		public static final int CMD_SET_MAX = 0x04;     // 0x04: Set the maximum value
		public static final int CMD_GET_MAX = 0x05;     // 0x05: Get the maximum value
		public static final int CMD_SET_MIN = 0x06;     // 0x06: Set the minimum value
		public static final int CMD_GET_MIN = 0x07;     // 0x07: Get the minimum value
		public static final int CMD_SET_DELAY = 0x08;   // 0x08: Set the timer delay
		public static final int CMD_GET_DELAY = 0x09;   // 0x09: Get the timer delay
		public static final int CMD_SET_PERIOD = 0x0A;  // 0x0A: Set the timer period
		public static final int CMD_GET_PERIOD = 0x0B;  // 0x0B: Get the timer period
		public static final int CMD_SET_RISE = 0x0C;    // 0x0C: Set rise modifier
		public static final int CMD_GET_RISE = 0x0D;    // 0x0D: Get rise modifier
		public static final int CMD_SET_OFFSET = 0x0E;  // 0x0E: Set offset modifier
		public static final int CMD_GET_OFFSET = 0x0F;  // 0x0F: Get offset modifier
		public static final int CMD_RESTART = 0x10;     // 0x10: Reset the channel timer
		public static final String[] Names = new String[]{"SET_MODE", "GET_MODE", "SET_VAL", "GET_VAL", "SET_MAX", "GET_MAX", "SET_MIN",
			"GET_MIN", "SET_DELAY", "GET_DELAY", "SET_PERIOD", "GET_PERIOD", "SET_RISE", "GET_RISE", "SET_OFFSET", "GET_OFFSET", "RESTART"};
	}

	private final int[] mChGetter = { COMMANDS.CMD_GET_MODE, COMMANDS.CMD_GET_VAL, COMMANDS.CMD_GET_PERIOD, COMMANDS.CMD_GET_MIN, COMMANDS.CMD_GET_MAX, COMMANDS.CMD_GET_DELAY, COMMANDS.CMD_GET_RISE,  COMMANDS.CMD_GET_OFFSET};
		
	public static final class GLOBAL_COMMANDS
	{
		/** Get the command counter */
		public static final int GET_CC = 0x00;
		/** Reset the command counter */
		public static final int RESET_CC = 0x01;
		/** get the current cpu usage */
		public static final int CPU = 0x02;
		/** trace out the error log */
		public static final int ERROR = 0x03;
		/** reset all channels */
		public static final int RESET_ALL = 0x04;
		/** Do a hardware reset */
		public static final int RESET_SYSTEM = 0x05;
		/** Get the time on the board */
		public static final int GET_SYS_TIME = 0x06;
		/** Get the hardware version */
		public static final int GET_VERSION = 0x07;
		/** Reset the BT module only */
		public static final int RESET_BT = 0x08;
	}
	
	private final int[] mGlGetter = {GLOBAL_COMMANDS.GET_SYS_TIME, GLOBAL_COMMANDS.GET_VERSION};
	

	public static final class Channels
	{
		public static final int CHANNEL_0 = 0x0001;
		public static final int CHANNEL_1 = 0x0002;
		public static final int CHANNEL_2 = 0x0004;
		public static final int CHANNEL_3 = 0x0008;
		public static final int CHANNEL_4 = 0x0010;
		public static final int CHANNEL_5 = 0x0020;
		public static final int CHANNEL_6 = 0x0040;
		public static final int CHANNEL_7 = 0x0080;
		public static final int CHANNEL_8 = 0x0100;
		public static final int CHANNEL_9 = 0x0200;
		public static final int CHANNEL_ALL(){
			int address = 0;
			for(Channel ch : MainActivity.connectedLamp.channels)
			{
				address |= ch.ID;
			}
			return address;
		}
		public static final int CHANNEL_NONE = 0x0000;
		public static final int CHANNEL_MAX = 0xFFFF;
	}

	/** Bluetooth service to send the commands */
	private BluetoothService mbtService;

	protected static Command command;

	// Internal states
	private static byte[] mReadBuffer;
	private ArrayList<byte[]> mWriteQueue;
	private SendCommandQueue mQueueWorker;
	private static final int mQueueDelay = 50; // delay of the message queue in ms
	private static final String TAG = "CommandHandler";


	// public constants for broadcast receive
	public static final String CH_VAL_RX = "chValRx";
	public static final String CH_VAL_RX_EXTRA = "chValRxExtra";
	public static final String CH_MODE_RX = "chModeRx";
	public static final String CH_MODE_RX_EXTRA = "chModeRxExtra";
	public static final String CH_SETTING_RECEIVED = "chSettingRx";
	public static final String CH_SETTING_RECEIVED_EXTRA = "chSettingRxExtra";

	private static Context mContext;

	public static enum FUNCTIONS
	{
		FUNC_FADE, FUNC_SAW, FUNC_SAW_REV,
	}

	public static String[] Functions = new String[FUNCTIONS.values().length];

	/**
	 * Constructor for the command handlings class of the Meister Lampe BT Module
	 */
	public CommandHandler(Context context, BluetoothService btService)
	{		
		mChannelListener = new ArrayList<ChannelCommandReceivedListener>();
		mGlobalListener = new ArrayList<GlobalCommandReceivedListener>();
		mWriteQueue = new ArrayList<byte[]>();
		command = new Command();
		mbtService = btService;
		mContext = context;
		if (btService != null)
		{
			mbtService.AddHandler(mHandler);
		}
		Functions = new String[] { context.getResources().getString(R.string.func_sweeping), context.getResources().getString(R.string.func_fading_in),
				context.getResources().getString(R.string.func_fading_out), };
		Log.d(TAG, "Command handler started, communication available: " + (mbtService != null));
	}

	/**
	 * This has to be called when changing to another activity!
	 */
	public void dispose()
	{

	}

	public void AddChannelCommandReceivedListener (ChannelCommandReceivedListener listener) 
    {
        // Store the listener object
        this.mChannelListener.add(listener);
    }
	
	public void AddGlobalCommandReceivedListener (GlobalCommandReceivedListener listener) 
    {
        // Store the listener object
        this.mGlobalListener.add(listener);
    }
	
	public int ChannelToAddress(int channel)
	{
		return (0x1 << channel) & 0xFFFF;
	}

	public void SetChannelProptery(int channel, int property, int value)
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, property, channel, value);
		sendCommand(setValue);
	}
	
	public void GetChannelProptery(int channel, int property)
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, property, channel);
		sendCommand(setValue);
	}
	
	public void GetCommandCounter()
	{
		byte[] setValue = createCommand(CLASS.CLA_GLOBAL, GLOBAL_COMMANDS.GET_CC, 0);
		sendCommand(setValue);
	}
	
	public void GetErrorLog()
	{
		byte[] setValue = createCommand(CLASS.CLA_GLOBAL, GLOBAL_COMMANDS.ERROR, 0);
		sendCommand(setValue);
	}

	public void AllOff()
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_MODE, Channels.CHANNEL_ALL(), MODE.MOD_ON);
		sendCommand(setValue);
	}

	public void AllOn()
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_MODE, Channels.CHANNEL_ALL(), MODE.MOD_OFF);
		sendCommand(setValue);
	}
	
	public void ResetTimers()
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_RESTART, Channels.CHANNEL_ALL());
		sendCommand(setValue);
	}

	public void ReadOut()
	{
		for (int i = 0; i < mChGetter.length; i++)
		{
			byte[] setValue = createCommand(CLASS.CLA_CHANNEL, mChGetter[i], Channels.CHANNEL_ALL());
			sendCommand(setValue);
		}
		
		for (int i = 0; i < mGlGetter.length; i++)
		{
			byte[] setValue = createCommand(CLASS.CLA_GLOBAL, mGlGetter[i], Channels.CHANNEL_NONE);
			sendCommand(setValue);
		}
		
		// request the acknowledge:
		byte[] setValue = createCommand(CLASS.CLA_GLOBAL, GLOBAL_COMMANDS.GET_CC, Channels.CHANNEL_NONE);
		sendCommand(setValue);
	}

	private byte[] createCommand(int cla, int mode, int address)
	{
		return createCommand(cla, mode, address, 0);
	}

	private byte[] createCommand(int cla, int mode, int address, int value)
	{
		Log.d(TAG, "Sending command: [Class: " + CLASS.Names[cla] + " Command: " + COMMANDS.Names[mode] + " Address: " + address + " Value: " + value + "]");
		byte claB = (byte)cla;
		byte mod = (byte)mode;
		byte chanHigh = (byte) ((address & 0xFF00) >> 0x08);
		byte chanLow = (byte) (address & 0x00FF);
		byte checksum = (byte) ((value + claB) & 0xFF);
		return new byte[] { claB, mod, chanHigh, chanLow, (byte) value, checksum, 0x0D, 0x0A };
	}

	private void sendCommand(final byte[] command)
	{
		mWriteQueue.add(command);
		if (mQueueWorker == null)		
		{
			mQueueWorker = new SendCommandQueue();
		}
		if(!mQueueWorker.isAlive())
		{
			mQueueWorker.run();
		}
	}
	
	private class SendCommandQueue extends Thread
	{
		public void run()
		{
			Iterator<byte[]> it = mWriteQueue.iterator();
			while (!Thread.currentThread().isInterrupted() && it.hasNext())
			{
				try
				{
					byte[] tmp = it.next();
					it.remove();
					if (mbtService != null)
					{
						mbtService.write(tmp);					
					}
					this.sleep(mQueueDelay);
				}
				catch (Exception ex)
				{
					Log.e(TAG, "Couldn't finish the commandlist");
				}

			}
		}
	}

	// The Handler that gets information back from the MLBluetoothService
	private final Handler mHandler = new Handler()
	{
		@Override
		public void handleMessage(Message msg)
		{
			switch (msg.what)
			{
			case MainActivity.MESSAGE_READ:
				mReadBuffer = (byte[]) msg.obj;				
				command.splitCommand(mReadBuffer);
				switch (command.cla)
				{
				case CLASS.CLA_CHANNEL:					
					for (ChannelCommandReceivedListener listener : mChannelListener) 
					{
					    listener.OnCommandReceive(command.mode, command.addressInteger, command.value);
					}
					break;
				case CLASS.CLA_GLOBAL:
					for (GlobalCommandReceivedListener listener : mGlobalListener) 
					{
					    listener.OnCommandReceive(command);
					}
					break;
				case CLASS.CLA_DIRECT:
					break;
				case CLASS.CLA_AT:
					break;
				default:
					break;
				}
				break;
			}
		}
	};

}
