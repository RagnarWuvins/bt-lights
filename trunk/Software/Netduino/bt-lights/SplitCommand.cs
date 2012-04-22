using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace BTLights
{
    public class SplitCommand
    {
        public event NativeEventHandler ChannelRequest, GlobalRequest;
        public static int _commandCounter = 0;

        //|CLA |MOD |       ADR         |   VAL   |   CRC   | 
        //|0001|0101|0000|0000|0000|0001|0000|0000|0000|0006|

        private byte _class;
        private int _globalcmd, _address;
        private byte _mode;
        private int _value;
        private int _crc;        
        private bool _internalCmd = true;

        public SplitCommand(byte[] command)
        {           
            this._class = (byte)(command[0] >> 4);
            this._mode = (byte)(command[0] & 0x0F);
            this._address = ((int)command[1] << 8) | (int)command[2];
            this._globalcmd = this._address;
            this._value = command[3];
            this._crc = command[0] + command[3];
            // check if the command comes from the BT board or user. Unfortunately the
            // CRC can be bigger than a byte, that's why we add 0x100 for the checksum
            _internalCmd = 
                (command[4] == this._crc || 
                (command[4] + 0x100) == this._crc) ? true : false; 
            if (!_internalCmd)
            {
                Debug.Print("<- " + new string(Encoding.UTF8.GetChars(command)));
            }
        }

        public void ThreadProc()
        {
            if (!_internalCmd)
            {               
                return;
            }
            _commandCounter++;
            switch(_class)
            {
                case (int)Constants.CLASS.CC_CMD:

                    ChannelRequest(((uint)this._mode << Constants.G_MAX_ADDRESS | (uint)this._address), (uint)this._value, DateTime.Now);
                    break;
                case (int)Constants.CLASS.GC_CMD:
                    /*if (_command == "gc+clearcommandcounter")
                    {
                        BTModuleRequest((uint)Constants.BT_COMMANDS.CMD_ACK, (uint)_commandCounter, DateTime.Now);
                        _commandCounter = 0;
                    }
                    if (_command == "gc+commandsreceived?")
                    {
                        BTModuleRequest((uint)Constants.BT_COMMANDS.CMD_COMMANDCOUNTER, (uint)_commandCounter, DateTime.Now);
                    }
                    if (_command == "gc+cpu?")
                    {
                        BTModuleRequest((uint)Constants.BT_COMMANDS.CMD_CPU, 0, DateTime.Now);
                    }*/
                    break;
                case (int)Constants.CLASS.AT_CMD:
                    break;
                case (int)Constants.CLASS.DP_CMD:
                    break;
                default:
                    break;
            
        }
        }
    }
}
