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
        private bool _internalCmd = false;

        public SplitCommand(byte[] command)
        {
            if (command.Length == Constants.C_LENGTH)
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
                if (_internalCmd)
                {
                    Debug.Print("IN: Address: " + (command[1] << 8 | command[2]) + " Value: " + command[3]);
                }
            }
            else if (!_internalCmd && command.Length > 0)
            {
#if RELEASE
                Program.THROW_ERROR(Constants.FW_ERRORS.CMD_CORRUPT);
#else
                Debug.Print("IN: Stringcommand: " + new string(Encoding.UTF8.GetChars(command)));
#endif
            }
            else
            {
#if RELEASE
                Program.THROW_ERROR(Constants.FW_ERRORS.CMD_ASSERT_FAIL);
#else
                Debug.Print("IN: Command Assert failed!");
#endif

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
                    GlobalRequest(((uint)this._mode << Constants.G_MAX_ADDRESS | (uint)this._address), (uint)this._value, DateTime.Now);
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
