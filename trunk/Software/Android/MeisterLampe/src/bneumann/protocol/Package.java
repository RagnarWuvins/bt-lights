package bneumann.protocol;

import java.util.Arrays;

public class Package
{
	private int mLength;
	private Frame[] mFrames;
	private Header mHeader;		
	private boolean mIsValid = false;
	
	public Package(int length)
	{
		this.mHeader = new Header(length);
		// Create package without header
		this.mFrames = new Frame[length - 1];
		validate();
	}
	
	public Package(byte[] bytes) throws Exception
	{
		this(bytes.length / Frame.LENGTH);
		this.mHeader = new Header(Arrays.copyOfRange(bytes, 0, Frame.LENGTH));
		int packageLength = bytes.length / Frame.LENGTH;
		for(int i = 1; i < packageLength; i++)
		{
			this.mFrames[i - 1] = new Frame(Arrays.copyOfRange(bytes, i * Frame.LENGTH, (i + 1) * (Frame.LENGTH)));
		}
		validate();
	}
		
	public byte[] getBytes()
	{
		byte[] output = new byte[this.getLength() * Frame.LENGTH];
		for(int i = Frame.LENGTH; i < this.getLength() * Frame.LENGTH; i += Frame.LENGTH)
		{
			System.arraycopy(this.mFrames[i].getByteData(), 0, output, i, Frame.LENGTH);
		}
		return output;
	}
	
	public Frame[] getFrames()
	{
		return this.mFrames;
	}
	
	public int getLength()
	{
		return this.mHeader.getPackageLength();
	}
	
	private boolean validate()
	{
		if((this.mFrames.length == this.mLength) && this.mHeader.getPackageLength() == this.mLength)
		{
			this.mIsValid = true;
		}
		return this.mIsValid;
	}
}
