using System;
using System.IO.Ports;
using System.Globalization;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Text;

namespace BTLights
{
    public class Program
    {
        public static SPI.Configuration _MAX;
        public static SPI _SPIBus;
        public static int commandCounter = 0;
        public static LightStringCollection channels;
        public static BTModule _BT;

        public static void Main()
        {
            // Setup the SPI interface. I guttenberged these settings from some tutorial and got the correct values by try and error.
            // Afterwards I found out they were documented in the MAX6966 documentation :/
            _MAX = new SPI.Configuration(
            Pins.GPIO_PIN_D10,  // CS-pin
            false,              // CS-pin active state
            0,                  // The setup time for the SS port
            0,                  // The hold time for the SS port
            false,              // The idle state of the clock
            true,               // The sampling clock edge
            10000,              // The SPI clock rate in KHz
            SPI_Devices.SPI1    // The used SPI bus (refers to a MOSI MISO and SCLK pinset)
            );

            _SPIBus = new SPI(_MAX);

            // setting up the serial port for the communication to the BT module
            _BT = new BTModule("COM1", 38400, Parity.None, 8, StopBits.One, Pins.GPIO_PIN_D2);
            _BT.CommandReceived += new NativeEventHandler(setMode);
            // Only for first initialization. Damn got no EEProm to save that state?!
            _BT.dump(Constants.BT_INIT);
            //_BT.send2BT("at+reset");

            channels = new LightStringCollection(10, _SPIBus);

            Thread.Sleep(-1);
        }

        public static void BTCommandHandler(uint btCmd, uint value, DateTime time)
        {
            switch (btCmd)
            {
                case (uint)Constants.BT_COMMANDS.CMD_COMMANDCOUNTER:
                    _BT.send2BT(value.ToString());
                    break;
                case (uint)Constants.BT_COMMANDS.CMD_CPU:
                    _BT.send2BT(Cpu.SystemClock.ToString());
                    break;
                default:
                    break;
            }
        }

        public static void LSCommandHandler(uint lsCmd, uint value, DateTime time)
        {          
            int channel;

            switch (lsCmd)
            {
                case (uint)Constants.LS_COMMANDS.CMD_SET_VALUE:
                    channel = (int)value >> 8;
                    Debug.Print("Channel " + channel + ", Value " + (value & 0xFF));
                    channels.SetChannelValue(channel, (int)value & 0xFF);
                    break;
                case (uint)Constants.LS_COMMANDS.CMD_MODE:
                    channel = (int)value >> 8;
                    Debug.Print("Channel " + channel + ", Mode " + (value & 0xFF));
                    channels.SetChannelMode(channel, (int)value & 0xFF);
                    break;
                default:
                    break;
            }
        }

        public static void setMode(uint bufferIndex, uint j, DateTime time)
        {
            string _command = _BT.GetCommand(bufferIndex);
            if (_command == null || _command.Length < 2)
            {
                return;
            }
            Debug.Print("<- " + _command);
            _command = _command.ToLower();
            int cmdClass = 0;            
            string cmdType = _command.Substring(0, 2);
            switch (cmdType)
            {
                case "cc":
                    cmdClass = 1;
                    break;
                case "gc":
                    cmdClass = 2;
                    break;
                case "dc":
                    cmdClass = 3;
                    break;
                case "at":
                    cmdClass = 4;
                    break;
                default:                    
                    break;
            }
            if (cmdClass > 0)
            {
                SplitCommand sc = new SplitCommand(cmdClass, _command);
                sc.BTModuleRequest += new NativeEventHandler(BTCommandHandler);
                sc.LightStringRequest += new NativeEventHandler(LSCommandHandler);
                Thread splitter = new Thread(new ThreadStart(sc.ThreadProc));
                splitter.Start();
            }
        }        
    }   
}

