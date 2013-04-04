package bneumann.meisterlampe;

import java.io.IOException;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.xpath.XPathExpressionException;
import org.xml.sax.SAXException;
import android.util.Log;
import bneumann.meisterlampe.CommandHandler.COMMANDS;
import bneumann.meisterlampe.CommandHandler.ChannelCommandReceivedListener;
import bneumann.meisterlampe.CommandHandler.Channels;
import bneumann.meisterlampe.CommandHandler.GLOBAL_COMMANDS;
import bneumann.meisterlampe.CommandHandler.GlobalCommandReceivedListener;
import bneumann.meisterlampe.CommandHandler.MODE;
import bneumann.meisterlampe.Lamp.Channel;

public class Lamp implements ChannelCommandReceivedListener, GlobalCommandReceivedListener, Serializable
{
	
	public interface AcknowledgeReceivedListener
	{
		public abstract void OnAcknowledgeReceived();
	}
	
	public interface ChannelPropertyChangeListener
	{
		public abstract void ChannelPropertyChangeReceived(Channel channel, int channelProperty, int value);
	}
	
	public static final int channelOff = 3;	
	public int HWVersion, HWBuild;
	public int CommandCounter;
	public long SysTime;
	public Channel[] channels;
	public Channel AllChannels;
	public ArrayList<int[]> errors;
	private String TAG = "LampClass";
	private ArrayList<AcknowledgeReceivedListener> mAckListener;
	private ArrayList<ChannelPropertyChangeListener> mChChangeListener;
	public int mNumberOfChannels;
	
	public Lamp()
	{
		SetNumberOfChannels(10);
		this.HWBuild = 0;
		this.HWVersion = 0;
		this.CommandCounter = 0;
		this.SysTime = 0;
		this.errors = new ArrayList<int[]>();
				
		this.mAckListener = new ArrayList<AcknowledgeReceivedListener>();
		this.mChChangeListener = new ArrayList<ChannelPropertyChangeListener>();
	}

	public void SetNumberOfChannels(int numberOfChannels)
	{
		this.mNumberOfChannels = numberOfChannels;
		if(this.channels == null)
		{
			this.channels = new Channel[this.mNumberOfChannels];
			for (int i = 0; i < numberOfChannels; i++)
			{
				this.channels[i] = new Channel();
				this.channels[i].ID = 0x01 << i;
			}
			// this is a dummy channel to send data to all channels at once
			this.AllChannels = new Channel();
			this.AllChannels.ID = Channels.CHANNEL_MAX;
		}
		else if(this.channels.length > numberOfChannels)
		{
			this.channels = Arrays.copyOfRange(this.channels, 0, numberOfChannels);
		}
		else
		{
			Channel[] tmpChannels = new Channel[numberOfChannels];
			for (int i = 0; i < numberOfChannels; i++)
			{
				tmpChannels[i] = new Channel();
				tmpChannels[i].ID = 0x01 << i;
			}
			System.arraycopy(this.channels, 0, tmpChannels, 0, this.channels.length);
			this.channels = tmpChannels;
		}
	}
	
	public int GetNumberOfChannels()
	{
		return this.mNumberOfChannels;
	}
	
	public void SetAllProperty(int command, int value)
	{
		// Send the property change to all
		for(ChannelPropertyChangeListener listener : mChChangeListener)
		{
			listener.ChannelPropertyChangeReceived(this.AllChannels, command, value);
		}
		// save the property change to class
		for(int i = 0; i < this.channels.length; i++)
		{
			switch (command)
			{
			case COMMANDS.CMD_SET_VAL:
				this.channels[i].mValue = value;
				break;
			case COMMANDS.CMD_SET_DELAY:
				this.channels[i].mDelay = value;
				break;
			case COMMANDS.CMD_SET_MAX:
				this.channels[i].mMax = value;
				break;
			case COMMANDS.CMD_SET_MIN:
				this.channels[i].mMin = value;
				break;
			case COMMANDS.CMD_SET_MODE:
				this.channels[i].mMode = value;
				break;
			case COMMANDS.CMD_SET_OFFSET:
				this.channels[i].mOffset = value;
				break;
			case COMMANDS.CMD_SET_PERIOD:
				this.channels[i].mPeriod = value;
				break;
			case COMMANDS.CMD_SET_RISE:
				this.channels[i].mRise = value;
				break;
			default:
				break;
			}
		}
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
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_VAL, value);
			}
		}
		
		public int getValue()
		{
			return this.mValue;		
		}
		
		public void setMode(int value)
		{
			this.lastMode = mMode;
			this.mMode = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_MODE, value);
			}
		}
		
		public int getMode()
		{
			return mMode;
		}
		
		public void setDelay(int value)
		{
			this.mDelay = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_DELAY, value);
			}
		}
		
		public int getDelay()
		{
			return mDelay;
		}
		
		public void setPeriod(int value)
		{
			this.mPeriod = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_PERIOD, value);
			}
		}
		
		public int getPeriod()
		{
			return mPeriod;
		}
		
		public void setMin(int value)
		{
			this.mMin = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_MIN, value);
			}
		}
		
		public int getMin()
		{
			return mMin;
		}
		
		public void setMax(int value)
		{
			this.mMax = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_MAX, value);
			}
		}
		
		public int getMax()
		{
			return mMax;
		}
		
		public void setRise(int value)
		{
			this.mRise = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_RISE, value);
			}
		}
		
		public int getRise()
		{
			return mRise;
		}
		
		public void setOffset(int value)
		{
			this.mOffset = value;
			for(ChannelPropertyChangeListener listener : mChChangeListener)
			{
				listener.ChannelPropertyChangeReceived(this, COMMANDS.CMD_SET_OFFSET, value);
			}
		}
		
		public int getOffset()
		{
			return mOffset;
		}
	}	
	
	public void AddAcknowledgeReceivedListener(AcknowledgeReceivedListener listener)
	{
		mAckListener.add(listener);
	}
	
	public void AddChannelPropertyChangeListener(ChannelPropertyChangeListener listener)
	{
		mChChangeListener.add(listener);
	}

	
	public void OnCommandReceive(int command, int address, int payload)
	{
		if (!(address < this.mNumberOfChannels && address >= 0))
		{
			Log.w(TAG, "Address " + address + " out of scope!");
			return;
		}
		switch (command)
		{
		case COMMANDS.CMD_GET_VAL:
			this.channels[address].mValue = payload;
			break;
		case COMMANDS.CMD_GET_DELAY:
			this.channels[address].mDelay = payload;
			break;
		case COMMANDS.CMD_GET_MAX:
			this.channels[address].mMax = payload;
			break;
		case COMMANDS.CMD_GET_MIN:
			this.channels[address].mMin = payload;
			break;
		case COMMANDS.CMD_GET_MODE:
			this.channels[address].mMode = payload;
			break;
		case COMMANDS.CMD_GET_OFFSET:
			this.channels[address].mOffset = payload;
			break;
		case COMMANDS.CMD_GET_PERIOD:
			this.channels[address].mPeriod = payload;
			break;
		case COMMANDS.CMD_GET_RISE:
			this.channels[address].mRise = payload;
			break;
		default:
			break;
		}
	}

	
	public void OnCommandReceive(Command command)
	{
		switch (command.mode)
		{
		case GLOBAL_COMMANDS.ERROR:
			errors.add(new int[]{command.errorNum, command.errorTime});
			break;
		case GLOBAL_COMMANDS.GET_CC:
			this.CommandCounter = (int) command.value;
			for(AcknowledgeReceivedListener listener : mAckListener)
			{
				listener.OnAcknowledgeReceived();
			}
			break;
		case GLOBAL_COMMANDS.GET_VERSION:
			this.HWVersion = command.value >> 4;
			this.HWBuild = command.value & 0x0F;
			break;
		case GLOBAL_COMMANDS.GET_SYS_TIME:
			this.SysTime = command.sysTime;
			break;
		default:
			break;
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
