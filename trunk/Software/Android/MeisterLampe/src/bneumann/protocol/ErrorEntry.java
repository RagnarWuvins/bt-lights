package bneumann.protocol;

public class ErrorEntry extends Frame
{	
	public ErrorEntry()
	{
		// TODO Auto-generated constructor stub
	}

	public ErrorEntry(byte[] data) throws Exception
	{
		super(data);
		// TODO Auto-generated constructor stub
	}
	
	public int getError()
	{
		return (int)(this.mBuffer[Frame.ERROR_CODE_INDEX] & 0xff);
	}
	
	public int getTimeStamp()
	{
		return (int)(this.getPayload() & ~(0xff << (Frame.ERROR_CODE_INDEX - 1) * 8));
	}

}
