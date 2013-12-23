package bneumann.protocol;

import java.util.ArrayList;
import java.util.Arrays;

public class Package extends ArrayList<Frame>
{
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	private Header mHeader;		
	
	public Package()
	{
		this.mHeader = new Header(1);
	}
	
	public Package(byte[] bytes) throws Exception
	{
		this.mHeader = new Header(Arrays.copyOfRange(bytes, 0, Frame.LENGTH));
		int packageLength = bytes.length / Frame.LENGTH;
		for(int i = 1; i < packageLength; i++)
		{
			Frame f = new Frame(Arrays.copyOfRange(bytes, i * Frame.LENGTH, (i + 1) * (Frame.LENGTH)));
			this.add(f);
		}
	}
	
	@Override
	public boolean add(Frame object)
	{
		this.mHeader.setPackageLength(this.mHeader.getPackageLength() + 1);
		return super.add(object);
	}
	
	@Override
	public Frame remove(int index)
	{
		this.mHeader.setPackageLength(this.mHeader.getPackageLength() - 1);
		return super.remove(index);
	}
		
	public byte[] getBytes()
	{
		byte[] output = new byte[this.getLength() * Frame.LENGTH];
		System.arraycopy(this.mHeader.getByteData(), 0, output, 0, Frame.LENGTH);
		for(int i = 0; i < this.getFrames().length; i++)
		{
			System.arraycopy(this.getFrames()[i].getByteData(), 0, output, (i + 1) * Frame.LENGTH, Frame.LENGTH);
		}
		return output;
	}
	
	public Frame[] getFrames()
	{
		Frame[] output = new Frame[this.size()];
		return this.toArray(output);
	}
	
	public int getLength()
	{
		return this.mHeader.getPackageLength();
	}	
}
