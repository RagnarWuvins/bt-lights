package bneumann.meisterlampe;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.os.SystemClock;
import android.util.Log;

public class CommandHandler
{
	private static final class CLASS
	{
		/** Channel command class */
		private static final int CLA_CHANNEL = 0x00;
		/** Global command class */
		private static final int CLA_GLOBAL = 0x01;
		/** AT (Bluetooth module) command class */
		private static final int CLA_AT = 0x02;
		/** Direct port command class */
		private static final int CLA_DIRECT = 0x03;
	}

	private static final class MODE
	{
		/** no change of current mode */
		private static final int MOD_NOOP = 0x00;
		/** set fix value */
		private static final int MOD_DIRECT = 0x01;
		/** set to on */
		private static final int MOD_ON = 0x02;
		/** set to off */
		private static final int MOD_OFF = 0x03;
		/** set a preprogrammed function */
		private static final int MOD_FUNC = 0x04;
	}

	private static final class COMMANDS
	{
		private static final int CMD_SET_MODE = 0x00;   // 0x00: Set the channel mode
		private static final int CMD_GET_MODE = 0x01;    // 0x01: Get the channel mode
		private static final int CMD_SET_VAL = 0x02;     // 0x02: Set current value
		private static final int CMD_GET_VAL = 0x03;     // 0x03: Get current value
		private static final int CMD_SET_MAX = 0x04;     // 0x04: Set the maximum value
		private static final int CMD_GET_MAX = 0x05;     // 0x05: Get the maximum value
		private static final int CMD_SET_MIN = 0x06;     // 0x06: Set the minimum value
		private static final int CMD_GET_MIN = 0x07;     // 0x07: Get the minimum value
		private static final int CMD_SET_DELAY = 0x08;   // 0x08: Set the timer delay
		private static final int CMD_GET_DELAY = 0x09;   // 0x09: Get the timer delay
		private static final int CMD_SET_PERIOD = 0x0A;  // 0x0A: Set the timer period
		private static final int CMD_GET_PERIOD = 0x0B;  // 0x0B: Get the timer period
		private static final int CMD_SET_RISE = 0x0C;    // 0x0C: Set rise modifier
		private static final int CMD_GET_RISE = 0x0D;    // 0x0D: Get rise modifier
		private static final int CMD_SET_OFFSET = 0x0E;  // 0x0E: Set offset modifier
		private static final int CMD_GET_OFFSET = 0x0F;  // 0x0F: Get offset modifier
		private static final int CMD_RESTART = 0x10;     // 0x10: Reset the channel timer
	}

	private final int[] mChGetter = { COMMANDS.CMD_GET_MODE, COMMANDS.CMD_GET_VAL, COMMANDS.CMD_GET_PERIOD, COMMANDS.CMD_GET_MIN, COMMANDS.CMD_GET_MAX, COMMANDS.CMD_GET_DELAY, COMMANDS.CMD_GET_RISE,  COMMANDS.CMD_GET_OFFSET};
		
	private static final class GLOBAL_COMMANDS
	{
		/** Get the command counter */
		private static final int GET_CC = 0x00;
		/** Reset the command counter */
		private static final int RESET_CC = 0x01;
		/** get the current cpu usage */
		private static final int CPU = 0x02;
		/** trace out the error log */
		private static final int ERROR = 0x03;
		/** reset all channels */
		private static final int RESET_ALL = 0x04;
		/** Do a hardware reset */
		private static final int RESET_SYSTEM = 0x05;
		/** Get the time on the board */
		private static final int GET_SYS_TIME = 0x06;
		/** Get the hardware version */
		private static final int GET_VERSION = 0x07;
		/** Reset the BT module only */
		private static final int RESET_BT = 0x08;
	}
	
	private final int[] mGlGetter = {GLOBAL_COMMANDS.GET_CC, GLOBAL_COMMANDS.GET_SYS_TIME, GLOBAL_COMMANDS.GET_VERSION};
	

	/** Length of commands */
	public static final int COMMAND_LENGTH = 0x08;
	/** Maximum number of channels */
	public static final int MAX_CHANNELS = 0x10;

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
		public static final int CHANNEL_ALL = 0xFFFF;
		public static final int CHANNEL_NONE = 0x0000;
	}

	/** Bluetooth service to send the commands */
	private MLBluetoothService mbtService;

	// Internal states
	private static byte[] mReadBuffer;
	private static final String TAG = "CommandHandler";
	private static final int BYTE_CLA = 0;
	private static final int BYTE_MOD = 1;
	private static final int BYTE_ADD_HIGH = 2;
	private static final int BYTE_ADD_LOW = 3;
	private static final int BYTE_VAL = 4;
	private static final int BYTE_CRC = 5;
	private static final int HIGH_LIM = 0xFE; // highest possible limit in HW
	private static final int LOW_LIM = 0x03; // lowest possible limit in HW
	private static final int LIGHT_ON = 0x02; // Maximum allowed value
	private static final int LIGHT_OFF = 0x01; // Minimum allowed value

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

	private class Command
	{
		public int cla;
		public int mode;
		public int address;
		public int addressInteger;
		public int value;
		public int errorNum;
		public int errorTime;
		public long sysTime;
		private int checksum;
		private byte[] rawCommand;

		public void splitCommand(byte[] command)
		{
			rawCommand = command;
			cla = rawCommand[BYTE_CLA];
			mode = rawCommand[BYTE_MOD];
			address = ((rawCommand[BYTE_ADD_HIGH] << 8) + rawCommand[BYTE_ADD_LOW]) & 0xFFFF;
			for (int i = 0; i < MAX_CHANNELS; i++)
			{
				if (((address >> i) & 0x1) == 1)
				{
					addressInteger = i;
					break;
				}
			}
			value = rawCommand[BYTE_VAL] & 0xFF;
			checksum = rawCommand[BYTE_CRC];
			// in case of error we use other values:
			errorNum = rawCommand[BYTE_ADD_HIGH] & 0xFF;
			errorTime = ((rawCommand[BYTE_ADD_LOW] & 0xFF) << 16) | ((rawCommand[BYTE_VAL] & 0xFF) << 8) | (rawCommand[BYTE_CRC] & 0xFF);
			sysTime = ((((long)rawCommand[BYTE_ADD_HIGH] & 0xFF) << 24) | (rawCommand[BYTE_ADD_LOW] & 0xFF) << 16) | ((rawCommand[BYTE_VAL] & 0xFF) << 8) | (rawCommand[BYTE_CRC] & 0xFF);
		}
	}

	/**
	 * Constructor for the command handlings class of the Meister Lampe BT Module
	 */
	public CommandHandler(Context context, MLBluetoothService btService)
	{
		mbtService = btService;
		mContext = context;
		mReadBuffer = new byte[COMMAND_LENGTH];
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

	public int ChannelToAddress(int channel)
	{
		return (0x1 << channel) & 0xFFFF;
	}
	
	public boolean SetChannelValue(int channel, int value)
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_VAL, channel, value);
		return sendCommand(setValue);
	}

	public void GetChannelValue(int channel)
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_GET_VAL, channel);
		sendCommand(setValue);
	}

	public boolean SetChannelMode(int channel, int mode)
	{
		byte[] setMode = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_MODE, channel, mode);
		return sendCommand(setMode);
	}

	public void GetChannelMode(int channel)
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_GET_MODE, channel);
		sendCommand(setValue);
	}

	public void GetCommandCounter()
	{
		byte[] setValue = createCommand(CLASS.CLA_GLOBAL, GLOBAL_COMMANDS.GET_CC, 0);
		sendCommand(setValue);
	}

	public void AllOff()
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_MODE, Channels.CHANNEL_ALL, MODE.MOD_ON);
		sendCommand(setValue);
	}

	public void AllOn()
	{
		byte[] setValue = createCommand(CLASS.CLA_CHANNEL, COMMANDS.CMD_SET_MODE, Channels.CHANNEL_ALL, MODE.MOD_OFF);
		sendCommand(setValue);
	}

	public void ReadOut()
	{
		for (int i = 0; i < mChGetter.length; i++)
		{
			byte[] setValue = createCommand(CLASS.CLA_CHANNEL, mChGetter[i], Channels.CHANNEL_ALL);
			sendCommand(setValue);
		}
		
		for (int i = 0; i < mGlGetter.length; i++)
		{
			byte[] setValue = createCommand(CLASS.CLA_GLOBAL, mGlGetter[i], Channels.CHANNEL_NONE);
			sendCommand(setValue);
		}
	}

	private byte[] createCommand(int cla, int mode, int address)
	{
		return createCommand(cla, mode, address, 0);
	}

	private byte[] createCommand(int cla, int mode, int address, int value)
	{
		Log.d(TAG, "Sending command: [Class: " + cla + " Mode: " + mode + " Address: " + address + " Value: " + value + "]");
		byte claB = (byte)cla;
		byte mod = (byte)mode;
		byte chanHigh = (byte) ((address & 0xFF00) >> 0x08);
		byte chanLow = (byte) (address & 0x00FF);
		byte checksum = (byte) ((value + claB) & 0xFF);
		return new byte[] { claB, mod, chanHigh, chanLow, (byte) value, checksum, 0x0D, 0x0A };
	}

	private boolean sendCommand(final byte[] command)
	{
		mbtService.sendMessage(command);
//		if (mbtService != null)
//		{
//			new Thread(new Runnable()
//			{
//				public void run()
//				{
//					mbtService.sendMessage(command);
//				}
//			}).start();
//			return true;
//		}
		return true;
	}

	private static void GlobalCommandHandler(Command command)
	{
		Intent i = new Intent();
		switch (command.mode)
		{
		case GLOBAL_COMMANDS.ERROR:
			Log.d(TAG, "Error incoming:\nError: " + command.errorNum + "\nTime: " + command.errorTime);
			i.setAction(MLStartupActivity.NEW_LOG_ENTRY);
			i.putExtra("EXTRA_ERROR_LOG", new long[] { command.errorNum, command.errorTime });
			mContext.getApplicationContext().sendBroadcast(i);
			break;
		case GLOBAL_COMMANDS.GET_CC:
			Log.d(TAG, "Command Counter incoming " + command.value);
			MLStartupActivity.connectedLamp.CommandCounter = command.value;
			indicateSettingReceived(command);
			break;
		case GLOBAL_COMMANDS.GET_VERSION:
			MLStartupActivity.connectedLamp.HWVersion = command.value >> 4;
			MLStartupActivity.connectedLamp.HWBuild = command.value & 0x0F;
			indicateSettingReceived(command);
			break;
		case GLOBAL_COMMANDS.GET_SYS_TIME:
			MLStartupActivity.connectedLamp.SysTime = command.sysTime;
			indicateSettingReceived(command);
			break;
		default:
			break;
		}
	}

	/**
	 * This function is used to send a broadcast when an answer to a request comes in.
	 * 
	 * @param command
	 *            command from the BT receiver
	 */
	private static void ChannelCommandHandler(Command command)
	{
		Intent intent = new Intent();
		switch (command.mode)
		{
		case COMMANDS.CMD_GET_VAL:
			intent.setAction(CH_VAL_RX);
			intent.putExtra(CH_VAL_RX_EXTRA, command.value);
			mContext.getApplicationContext().sendBroadcast(intent);
			MLStartupActivity.connectedLamp.channels[command.addressInteger].value = command.value;
			break;
		case COMMANDS.CMD_GET_MODE:
			intent.setAction(CH_MODE_RX);
			intent.putExtra(CH_MODE_RX_EXTRA, command.value);
			mContext.getApplicationContext().sendBroadcast(intent);
			MLStartupActivity.connectedLamp.channels[command.addressInteger].mode = command.value;
			break;
		case COMMANDS.CMD_GET_DELAY:
			MLStartupActivity.connectedLamp.channels[command.addressInteger].delay = command.value;
			indicateSettingReceived(command);
			break;
		case COMMANDS.CMD_GET_MAX:			
			MLStartupActivity.connectedLamp.channels[command.addressInteger].max = command.value;
			indicateSettingReceived(command);
			break;
		case COMMANDS.CMD_GET_MIN:			
			MLStartupActivity.connectedLamp.channels[command.addressInteger].min = command.value;
			indicateSettingReceived(command);
			break;
		case COMMANDS.CMD_GET_PERIOD:			
			MLStartupActivity.connectedLamp.channels[command.addressInteger].period = command.value;
			indicateSettingReceived(command);
			break;
		case COMMANDS.CMD_GET_RISE:			
			MLStartupActivity.connectedLamp.channels[command.addressInteger].rise = command.value;
			indicateSettingReceived(command);
			break;
		case COMMANDS.CMD_GET_OFFSET:			
			MLStartupActivity.connectedLamp.channels[command.addressInteger].offset = command.value;
			indicateSettingReceived(command);
			break;
		default:
			break;
		}

	}

	private static void indicateSettingReceived(Command command)
	{
		Intent intent = new Intent();
		intent.setAction(CH_SETTING_RECEIVED);
		intent.putExtra(CH_SETTING_RECEIVED_EXTRA, command.value);
		mContext.getApplicationContext().sendBroadcast(intent);
	}
	
	// The Handler that gets information back from the MLBluetoothService
	private final Handler mHandler = new Handler()
	{
		@Override
		public void handleMessage(Message msg)
		{
			switch (msg.what)
			{
			case MLStartupActivity.MESSAGE_READ:
				mReadBuffer = (byte[]) msg.obj;
				Command command = new Command();
				command.splitCommand(mReadBuffer);
				switch (command.cla)
				{
				case CLASS.CLA_CHANNEL:
					Log.d(TAG, "Data incoming:\nClass: " + command.cla + "\nMode: " + command.mode + "\nValue: " + command.value + "\nChannel: " + command.addressInteger);
					ChannelCommandHandler(command);
					break;
				case CLASS.CLA_GLOBAL:
					GlobalCommandHandler(command);
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
