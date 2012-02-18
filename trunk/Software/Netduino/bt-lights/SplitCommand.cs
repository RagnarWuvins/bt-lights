using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;

namespace BTLights
{
    public class SplitCommand
    {
        public event NativeEventHandler BTModuleRequest, LightStringRequest;

        private int _cmdClass;
        private string _command;
        private static int _commandCounter = 0;

        public SplitCommand(int cmdClass, string command)
        {
            this._cmdClass = cmdClass;
            this._command = command.Substring(3);
        }

        public void ThreadProc()
        {
            _commandCounter++;

            switch(_cmdClass)
            {
                case 1:
                    int channel = Convert.ToInt32(_command.Substring(2, 2));
                    this._command = this._command.Substring(2);
                    try
                    {
                        int valInd = _command.IndexOf("value");
                        int modInd = _command.IndexOf("mode");
                        bool ccQuery = (_command.IndexOf("?") > 0) ? true : false;
                        int value = 0;                        
                        if (valInd == 0 & !ccQuery)
                        {
                            value = Convert.ToInt32(_command.Substring(6, 3));
                            uint channelValue = (uint)channel << 8 | (uint)value;
                            LightStringRequest((uint)Constants.LS_COMMANDS.CMD_SET_VALUE, channelValue, DateTime.Now);
                        }
                        if (modInd >= 0 & !ccQuery)
                        {
                            value = Convert.ToInt32(_command.Substring(5, 3));
                            uint channelMode = (uint)channel << 8 | (uint)value;
                            LightStringRequest((uint)Constants.LS_COMMANDS.CMD_MODE, channelMode, DateTime.Now);
                        }
                    }
                    catch (Exception exp)
                    {
                        Debug.Print("FW_ERROR(" + Constants.FW_ERRORS.CMD_CORRUPT + "), " + exp.Message);
                    }
                    break;
                case 2:
                    if (_command == "gc+clearcommandcounter")
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
                    }
                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    break;
            
        }
        }
    }
}
