package bneumann.protocol;


public class Frame
{
	public final static int LENGTH = 4;
	public final static int LENGTH_PAYLOAD = 3;
	public final static int FUNCTION_CODE_INDEX = 0;
	public final static int ERROR_CODE_INDEX = 1;	
	public final static int CHANNEL_CODE_INDEX = 1;
	public final static int VALUE_CODE_INDEX = 3;
		
	protected byte[] mBuffer;
	
	public Frame()
	{
		this.mBuffer = new byte[Frame.LENGTH];
	}
	
	public Frame(byte[] data) throws Exception
	{
		if (data.length != Frame.LENGTH)
		{
			throw new Exception("Length of data array does not match protocol frame length");
		}
		this.mBuffer = data;
	}
	
	public int getFunction()
	{
		return (int)(this.mBuffer[FUNCTION_CODE_INDEX] & 0xff);
	}	

	public void setFunction(int function)
	{
		this.mBuffer[FUNCTION_CODE_INDEX] = (byte)function;
	}
	
	public int getChannel()
	{
		return this.mBuffer[CHANNEL_CODE_INDEX];
	}
	
	public void setChannel(int channel)
	{
		this.mBuffer[CHANNEL_CODE_INDEX] = (byte)channel;
	}
	
	public int getValue()
	{
		return this.mBuffer[VALUE_CODE_INDEX];
	}
	
	public void setValue(int value)
	{
		this.mBuffer[VALUE_CODE_INDEX] = (byte)value;
	}
	
	public int getPayload()
	{
		int payload = 0;
		int startIndex = LENGTH - LENGTH_PAYLOAD;
		for(int i = 0; i < LENGTH_PAYLOAD; i++)
		{
			payload = payload << 8;
			payload += (this.mBuffer[startIndex + i] & 0xff);			
		}
		return payload;
	}
	
	public void setPayload(int payload)
	{
		int startIndex = LENGTH - LENGTH_PAYLOAD;
		int stopIndex = LENGTH;
		for(int i = startIndex; i < stopIndex; i++)
		{
			this.mBuffer[i] = (byte)((payload >> (stopIndex - (i + 1)) * 8) & 0xff);
		}
	}
	
	public byte getByte(int index)
	{
		return this.mBuffer[index];
	}
	
	public void setByte(int index, byte data)
	{
		this.mBuffer[index] = data;
	}
	
	public byte[] getByteData()
	{
		return this.mBuffer;
	}
	
	public void setByteData(byte[] data)
	{
		this.mBuffer = data;
	}

}
