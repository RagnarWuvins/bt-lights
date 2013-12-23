package bneumann.meisterlampe;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.Locale;
import android.util.Log;
import bneumann.protocol.ErrorEntry;
import bneumann.protocol.Frame;
import bneumann.protocol.Package;

public class Lamp implements Serializable
{
	public final static int SET_MODE = 0x00; 				// Set the mChannelID mode
	public final static int GET_MODE = 0x01; 				// Get the mChannelID mode
	public final static int SET_VALUE = 0x02; 				// Set current value
	public final static int GET_VALUE = 0x03; 				// Get current value
	public final static int SET_FUNCTION = 0x04; 			// Set current value
	public final static int GET_FUNCTION = 0x05; 			// Get current value
	public final static int SET_MAXIMUM = 0x06; 			// Set the maximum value
	public final static int GET_MAXIMUM = 0x07; 			// Get the maximum value
	public final static int SET_MINIMUM = 0x08; 			// Set the minimum value
	public final static int GET_MINIMUM = 0x09; 			// Get the minimum value
	public final static int SET_DELAY = 0x0A; 				// Set the timer delay
	public final static int GET_DELAY = 0x0B; 				// Get the timer delay
	public final static int SET_PERIOD = 0x0C; 				// Set the timer period
	public final static int GET_PERIOD = 0x0D; 				// Get the timer period
	public final static int SET_RISE = 0x0E; 				// Set rise modifier
	public final static int GET_RISE = 0x0F; 				// Get rise modifier
	public final static int SET_OFFSET = 0x10; 				// Set offset modifier
	public final static int GET_OFFSET = 0x11; 				// Set offset modifier
	/**	Reset timer of given channel */
	public final static int RESET_CHANNEL = 0x12;
	public final static int GET_COMMAND_COUNTER = 0x13; 	// Get the command counter
	public final static int RESET_COMMAND_COUNTER = 0x14; 	// Reset the command counter
	public final static int GET_ERRORLOG = 0x15;			// trace out the error log
	public final static int RESET_SYSTEM = 0x16; 			// Do a hardware reset
	public final static int GET_SYSTEM_TIME = 0x17; 		// Get the time on the board
	public final static int GET_SYSTEM_VERSION = 0x18; 		// Get the hardware version
	public final static int RESET_BLUETOOTH = 0x19; 		// Reset the BT module only
	public final static int ACKNOWLEDGE = 0x1A; 			// Acknowledge received command
	/**	 Activate or Deactivate channel value tracer */	
	public final static int CHANNEL_TRACER = 0x1B; 			
	/**	 Resets all the channel timers at once */
	public final static int RESET_ALL_TIMER = 0x1C; 		
	public final static int NUMBER_OF_COMMANDS = 0x1D; 		// Number of commands

	public final static int MODE_NOOP = 0x00; 	        // no change of current mode
	public final static int MODE_DIRECT = 0x01;	        // Use mChannelID value
	public final static int MODE_ON = 0x02;             // On value
	public final static int MODE_OFF = 0x03;            // Off value
	public final static int MODE_FUNC = 0x04;	        // set Function

	public final static int FUNC_SINE = 0x00; 		// lowest possible function value (will be send with value :)
	public final static int FUNC_SAW = 0x01;        // fade in no out
	public final static int FUNC_SAW_REV = 0x02;    // fade out no in

	public final static int NUMBER_OF_CHANNELS = 10;

	private static final long serialVersionUID = 1L;
	private static final String TAG = "LampClass";

	public Channel[] mChannel;
	private ErrorList mErrorList;
	private int mBuild = -1, mVersion = -1, mSystime = -1, mCommandCounter = 0;

	public Lamp()
	{
		this.mErrorList = new ErrorList();
		this.mChannel = new Channel[NUMBER_OF_CHANNELS];
		for (int i = 0; i < NUMBER_OF_CHANNELS; i++)
		{
			this.mChannel[i] = new Channel();
		}
	}

	public Package getUpdatePackage()
	{
		byte[] channelGetter = { GET_DELAY, GET_MAXIMUM, GET_MINIMUM, GET_PERIOD, GET_VALUE, GET_MODE, GET_VALUE, GET_FUNCTION };
		byte[] globalGetter = { GET_COMMAND_COUNTER, GET_SYSTEM_TIME, GET_SYSTEM_VERSION };
		Package p = new Package();
		for (int i = 0; i < channelGetter.length; i++)
		{
			for (int j = 0; j < NUMBER_OF_CHANNELS; j++)
			{
				Frame f = new Frame();
				f.setFunction(channelGetter[i]);
				f.setByte(Frame.CHANNEL_CODE_INDEX, (byte) j);
				p.add(f);
			}
		}
		for (int i = 0; i < globalGetter.length; i++)
		{
			Frame f = new Frame();
			f.setFunction(globalGetter[i]);
			p.add(f);
		}
		return p;
	}

	public Package getErrorLogRequestPackage()
	{
		Package p = new Package();
		Frame f = new Frame();
		f.setFunction((byte) GET_ERRORLOG);
		p.add(f);
		return p;
	}

	public void update(Package p)
	{
		Frame[] frames = p.getFrames();
		for (int i = 0; i < frames.length; i++)
		{
			int channel = frames[i].getByte(Frame.CHANNEL_CODE_INDEX) & 0xff;
			int payload = frames[i].getPayload();
			int value = frames[i].getByte(Frame.VALUE_CODE_INDEX) & 0xff;
			switch (frames[i].getFunction())
			{
			case GET_SYSTEM_VERSION:
				this.setSystemVersion(payload);
				break;
			case GET_SYSTEM_TIME:
				this.mSystime = frames[i].getPayload();
			case GET_COMMAND_COUNTER:
				this.mCommandCounter = payload;
				break;
			case GET_ERRORLOG:
				this.mErrorList.add(frames[i]);
				break;
			case GET_DELAY:
				this.mChannel[channel].setDelay(value);
			case GET_FUNCTION:
				this.mChannel[channel].setFunction(value);
				break;
			case GET_MAXIMUM:
				this.mChannel[channel].setMax(value);
				break;
			case GET_MINIMUM:
				this.mChannel[channel].setMin(value);
				break;
			case GET_VALUE:
				this.mChannel[channel].setValue(value);
				break;
			case GET_MODE:
				this.mChannel[channel].setMode(value);
				break;
			case GET_PERIOD:
				this.mChannel[channel].setPeriod(value);
				break;
			case GET_OFFSET:
				this.mChannel[channel].setOffset(value);
				break;
			case ACKNOWLEDGE:
				Log.d(TAG, "Acknowledged command: " + frames[i].getValue());
				break;
			case 0:
				// let's ignore 0 at the moment
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

	public int[] getSystemTime()
	{
		return getTimeArray(this.mSystime);
	}

	public int getCommandCounter()
	{
		return this.mCommandCounter;
	}

	public ArrayList<String> getErrorLog()
	{
		return this.mErrorList.getErrorLog();
	}

	public int[] getChannelValues()
	{
		int[] output = new int[this.mChannel.length];
		int count = 0;
		for (Channel c : this.mChannel)
		{
			output[count] = c.getValue();
			count++;
		}
		return output;
	}

	public int[] getTimeArray(int timeStamp)
	{
		int hours = timeStamp / 3600;
		int remainder = timeStamp - hours * 3600;
		int mins = remainder / 60;
		remainder = remainder - mins * 60;
		int secs = remainder;

		int[] ints = { hours, mins, secs };
		return ints;
	}

	public class Channel
	{
		public int ID, lastMode;
		private int mValue, mMode, mFunction, mDelay, mPeriod, mOffset, mRise, mMin, mMax;

		public Channel()
		{
			this.ID = 0;
			this.mMode = 0;
			this.mFunction = 0;
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

		public void setFunction(int value)
		{
			this.mFunction = value;
		}
		
		public int getFunction()
		{
			return this.mFunction;
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
			mErrorLog.add(0x03, "Header not found, clearing memory");
			mErrorLog.add(0x04, "This functioin is not declared");
			mErrorLog.add(0x05, "the mChannelID changed its value very fast, this should not happen accidently");
			mErrorLog.add(0x06, "A race condition occured while extracting the command");
			mErrorLog.add(0x07, "The buffer write or read pointer are out of range");
			mErrorLog.add(0x08, "While casting from byte to another type the system encountered an error");
			mErrorLog.add(0x09, "Wrong mode set. Will be set to NOOP instead");
			mErrorLog.add(0x0A, "This is an unhandled mChannelID or global command");
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
					String entry = mErrorLog.get(curFrame.getError());
					int[] ts = getTimeArray(curFrame.getTimeStamp());
					String timeStamp = String.format(Locale.getDefault(), "[%d:%d:%d]", ts[0], ts[1], ts[2]);
					map.add(entry + " " + timeStamp);
				}
				catch (Exception e)
				{
					Log.e(TAG, "Some entries have not been added to error list");
					e.printStackTrace();
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
