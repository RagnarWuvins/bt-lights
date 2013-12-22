package bneumann.meisterlampe;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import android.util.Log;
import bneumann.protocol.ErrorEntry;
import bneumann.protocol.Frame;
import bneumann.protocol.Package;

public class Lamp implements Serializable
{
	public final static int SET_MODE = 0x00; // 0x00: Set the mChannelID mode
	public final static int GET_MODE = 0x01; // 0x01: Get the mChannelID mode
	public final static int SET_VALUE = 0x02; // 0x02: Set current value
	public final static int GET_VALUE = 0x03; // 0x03: Get current value
	public final static int SET_MAXIMUM = 0x04; // 0x04: Set the maximum value
	public final static int GET_MAXIMUM = 0x05; // 0x05: Get the maximum value
	public final static int SET_MINIMUM = 0x06; // 0x06: Set the minimum value
	public final static int GET_MINIMUM = 0x07; // 0x07: Get the minimum value
	public final static int SET_DELAY = 0x08; // 0x08: Set the timer delay
	public final static int GET_DELAY = 0x09; // 0x09: Get the timer delay
	public final static int SET_PERIOD = 0x0A; // 0x0A: Set the timer period
	public final static int GET_PERIOD = 0x0B; // 0x0B: Get the timer period
	public final static int SET_RISE = 0x0C; // 0x0C: Set rise modifier
	public final static int GET_RISE = 0x0D; // 0x0D: Get rise modifier
	public final static int SET_OFFSET = 0x0E; // 0x0E: Set offset modifier
	public final static int GET_OFFSET = 0x0F; // 0x0F: Set offset modifier
	public final static int RESET_CHANNEL = 0x10; // 0x10: Reset the mChannelID timer
	public final static int GET_COMMAND_COUNTER = 0x11; // 0x11: Get the command counter
	public final static int RESET_COMMAND_COUNTER = 0x12; // 0x12: Reset the command counter
	public final static int GET_ERRORLOG = 0x13; // 0x13: trace out the error log
	public final static int RESET_SYSTEM = 0x14; // 0x14: Do a hardware reset
	public final static int GET_SYSTEM_TIME = 0x15; // 0x15: Get the time on the board
	public final static int GET_SYSTEM_VERSION = 0x16; // 0x16: Get the hardware version
	public final static int RESET_BLUETOOTH = 0x17; // 0x17: Reset the BT module only
	public final static int ACKNOWLEDGE = 0x18; // 0x18: Acknowdlege received command
	public final static int CHANNEL_TRACER = 0x19; // 0x19: Activate or Deactivate channel value tracer
	public final static int NUMBER_OF_COMMANDS = 0x1A; // Number of commands

	public final static int NUMBER_OF_CHANNELS = 10;

	private static final long serialVersionUID = 1L;
	private static final String TAG = "LampClass";

	public Channel[] channels;
	private ErrorList mErrorList;
	private int mBuild = -1, mVersion = -1, mSystime = -1, mCommandCounter = 0;

	public Lamp()
	{
		this.mErrorList = new ErrorList();
	}

	public void Update(Package p)
	{
		Frame[] frames = p.getFrames();
		for (int i = 0; i < frames.length; i++)
		{
			switch (frames[i].getFunction())
			{
			case GET_SYSTEM_VERSION:
				setSystemVersion(frames[i].getPayload());
				break;
			case GET_SYSTEM_TIME:
				this.mSystime = frames[i].getPayload();
			case GET_COMMAND_COUNTER:
				this.mCommandCounter = frames[i].getPayload();
				break;
			case GET_ERRORLOG:
				this.mErrorList.add(frames[i]);
				break;
			default:
				Log.d(TAG, "Function: " + frames[i].getFunction() + " Payload: " + frames[i].getPayload());
			}
		}
	}

	private void setSystemVersion(int payload)
	{
		this.mBuild = payload & 0x00ff;
		this.mVersion = (payload & 0xff00) >> 8;
	}

	public int getBuild()
	{
		return this.mBuild;
	}

	public int getVersion()
	{
		return this.mVersion;
	}
	
	public int getSystemTime()
	{
		return this.mSystime;
	}
	
	public int getCommandCounter()
	{
		return this.mCommandCounter;
	}
	
	public ArrayList<String> getErrorLog()
	{
		return this.mErrorList.getErrorLog();
	}

	public class Channel
	{
		public int ID, lastMode;
		private int mValue, mMode, mDelay, mPeriod, mOffset, mRise, mMin, mMax;

		public Channel()
		{
			this.ID = 0;
			this.mMode = 0;
			this.lastMode = 0;
			this.mDelay = 0;
			this.mValue = 0;
			this.mMin = 0;
			this.mMax = 0;
			this.mDelay = 0;
			this.mPeriod = 0;
			this.mRise = 0;
			this.mOffset = 0;
		}

		public void setValue(int value)
		{
			mValue = value;
		}

		public int getValue()
		{
			return this.mValue;
		}

		public void setMode(int value)
		{
			this.lastMode = mMode;
			this.mMode = value;
		}

		public int getMode()
		{
			return mMode;
		}

		public void setDelay(int value)
		{
			this.mDelay = value;
		}

		public int getDelay()
		{
			return mDelay;
		}

		public void setPeriod(int value)
		{
			this.mPeriod = value;
		}

		public int getPeriod()
		{
			return mPeriod;
		}

		public void setMin(int value)
		{
			this.mMin = value;
		}

		public int getMin()
		{
			return mMin;
		}

		public void setMax(int value)
		{
			this.mMax = value;
		}

		public int getMax()
		{
			return mMax;
		}

		public void setRise(int value)
		{
			this.mRise = value;
		}

		public int getRise()
		{
			return mRise;
		}

		public void setOffset(int value)
		{
			this.mOffset = value;
		}

		public int getOffset()
		{
			return mOffset;
		}
	}

	public class ErrorList extends ArrayList<Frame>
	{
		/**
		 * 
		 */
		private static final long serialVersionUID = -4759978213464865007L;
		private ArrayList<String> mErrorLog = new ArrayList<String>();

		public ErrorList()
		{
			super();
			mErrorLog.add(0x00, "Just unknown command (maybe came through checksum by incident)");
			mErrorLog.add(0x01, "Not a mChannelID (internal) command");
			mErrorLog.add(0x02, "It was neither internal nor external (maybe length 0)");
			mErrorLog.add(0x03, "Too many commands comming in");
			mErrorLog.add(0x04, "This functioin is not declared");
			mErrorLog.add(0x05, "the mChannelID changed its value very fast, this should not happen accidently");
			mErrorLog.add(0x06, "A race condition occured while extracting the command");
			mErrorLog.add(0x07, "The buffer write or read pointer are out of range");
			mErrorLog.add(0x08, "While casting from byte to another type the system encountered an error");
			mErrorLog.add(0x09, "Wrong mode set. Will be set to NOOP instead");
			mErrorLog.add(0x0A, "This is an unhandled mChannelID or global command");
		}

		@Override
		public boolean add(Frame object)
		{
			// we do not add objects that are not in the error log
			if (object.getFunction() == 0)
			{
				return false;
			}
			return super.add(object);
		}

		public ArrayList<String> getErrorLog()
		{
			ArrayList<String> map = new ArrayList<String>();
			Iterator<Frame> it = iterator();
			while (it.hasNext())
			{				
				try
				{
					ErrorEntry curFrame;
					curFrame = new ErrorEntry(it.next().getByteData());
					map.add(mErrorLog.get(curFrame.getError()));
				}
				catch (Exception e)
				{
					Log.e(TAG,"Some entries have not been added to error list");
				}
				
			}
			return map;

		}
	}

	public void SaveData()
	{
		try
		{
			XMLHandler xh = new XMLHandler(this);
			String test = xh.GetHeader().get(0);
			xh.EditCurrentSetting(test);
		}
		catch (Exception e)
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
