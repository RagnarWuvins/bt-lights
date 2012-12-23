package bneumann.meisterlampe;

public class Lamp
{
	public int NumberOfChannels;
	public int HWVersion, HWBuild;
	public int CommandCounter;
	public long SysTime;
	public Channel[] channels;
	
	public Lamp(int numberOfChannels)
	{
		this.NumberOfChannels = numberOfChannels;
		this.HWBuild = 0;
		this.HWVersion = 0;
		this.CommandCounter = 0;
		this.SysTime = 0;
		this.channels = new Channel[numberOfChannels];
		for(int i = 0; i < numberOfChannels; i++)
		{
			this.channels[i] = new Channel();
			this.channels[i].ID = i;
		}
	}
	
	public class Channel
	{
		public int ID, mode, function, value, min, max, delay, period, rise, offset;
		
		public Channel()
		{
			this.ID = 0;
			this.mode = 0;
			this.delay = 0;
			this.function = 0;
			this.value = 0;
			this.min = 0;
			this.max = 0;
			this.delay = 0;
			this.period = 0;
			this.rise = 0;
			this.offset = 0;
		}
	}
}
