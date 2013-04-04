package bneumann.meisterlampe;

public class Command
{
	public int cla;
	public int mode;
	public int address;
	public int addressInteger;
	public int value;
	public int errorNum;
	public int errorTime;
	public long sysTime;
	public long payload;
	private int checksum;
	private byte[] rawCommand;
	
	private static final int BYTE_CLA = 0;
	private static final int BYTE_MOD = 1;
	private static final int BYTE_ADD_HIGH = 2;
	private static final int BYTE_ADD_LOW = 3;
	private static final int BYTE_VAL = 4;
	private static final int BYTE_CRC = 5;
	private static final int HIGH_LIM = 0xFE; // highest possible limit in HW
	private static final int LOW_LIM = 0x03; // lowest possible limit in HW
	private static final int LIGHT_ON = 0x02; // Maximum allowed value
	private static final int LIGHT_OFF = 0x01; // Minimum allowed value
	
	/** Length of commands */
	public static final int COMMAND_LENGTH = 0x08;
	/** Maximum number of channels */
	public static final int MAX_CHANNELS = 0x10;

	public void splitCommand(byte[] command)
	{
		rawCommand = command;
		cla = rawCommand[BYTE_CLA];
		mode = rawCommand[BYTE_MOD];
		address = ((rawCommand[BYTE_ADD_HIGH] << 8) + rawCommand[BYTE_ADD_LOW]) & 0xFFFF;
		for (int i = 0; i < MAX_CHANNELS; i++)
		{
			if (((address >> i) & 0x1) == 1)
			{
				addressInteger = i;
				break;
			}
		}
		value = rawCommand[BYTE_VAL] & 0xFF;
		checksum = rawCommand[BYTE_CRC];
		// in case of error we use other values:
		errorNum = rawCommand[BYTE_ADD_HIGH] & 0xFF;
		errorTime = ((rawCommand[BYTE_ADD_LOW] & 0xFF) << 16) | ((rawCommand[BYTE_VAL] & 0xFF) << 8) | (rawCommand[BYTE_CRC] & 0xFF);
		sysTime = ((((long)rawCommand[BYTE_ADD_HIGH] & 0xFF) << 24) | (rawCommand[BYTE_ADD_LOW] & 0xFF) << 16) | ((rawCommand[BYTE_VAL] & 0xFF) << 8) | (rawCommand[BYTE_CRC] & 0xFF);
		payload = sysTime;
	}
}
