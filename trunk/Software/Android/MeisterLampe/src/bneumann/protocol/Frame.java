package bneumann.protocol;


public class Frame
{
	public final static int LENGTH = 4;
	public final static int FUNCTION_CODE_INDEX = 0;
	public final static int ERROR_CODE_INDEX = 1;	
		
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
		return (int)(this.mBuffer[0] & 0xff);
	}
	
	public int getPayload()
	{
		int payload = 0;
		for(int i = 0; i < Frame.LENGTH; i++)
		{
			payload |= this.mBuffer[i] << (Frame.LENGTH - (i + 1)) * 8;
		}
		return payload;
	}
	
	public byte[] getByteData()
	{
		return this.mBuffer;
	}
}
