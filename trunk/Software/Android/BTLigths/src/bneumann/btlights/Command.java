package bneumann.btlights;

public class Command {
	
	public byte[] assembleCommand(int cla, int mode, int address, int value)
	{
		byte _modcla = (byte)(cla << 4 | mode);
        byte _address_higher = (byte)((address & 0xFF00) >> 8);
        byte _adress_lower = (byte)(address & 0x00FF);
        byte _crc = (byte)(_modcla + value);

        byte[] command = { _modcla, _address_higher, _adress_lower, (byte)value, _crc, 0xD, 0xA };
        return command;
	}
}
