package bneumann.protocol;

public class Header extends Frame
{	
	public Header(int length)
	{
		super();
		setLength(length);
	}
	
	public Header(byte[] data) throws Exception
	{
		super(data);
	}
	
	public void setProtocolVersion(int version)
	{
		this.mBuffer[Frame.LENGTH - 2] = (byte)version;
	}
	
	public int getProtocolVersion(int version)
	{
		return (int)(this.mBuffer[Frame.LENGTH - 2] & 0xff);
	}
	
	public void setLength(int length)
	{
		this.mBuffer[Frame.LENGTH - 1] = (byte)length;
	}

	public int getPackageLength()
	{
		return (int)(this.mBuffer[Frame.LENGTH - 1] & 0xff);
	}

}
