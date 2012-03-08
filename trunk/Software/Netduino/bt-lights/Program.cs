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
            _BT.CommandReceived += new NativeEventHandler(CommandHandler);
            // Only for first initialization. Damn got no EEProm to save that state?!
            _BT.dump(Constants.BT_INIT);
            //_BT.send2BT("at+reset");

            channels = new LightStringCollection(Constants.G_SET_ADDRESS, _SPIBus);
            channels.SendChannelData += new NativeEventHandler(SendChannelData);
            Thread.Sleep(-1);
        }

        public static void SendChannelData(uint value, uint crc, DateTime time)
        {
            byte[] data = new byte[5];
            for (int i = 3; i >= 0; i--)
            {                
                data[i] = (byte)(value & 0xFF);
                value >>= 8;
            }
            data[4] = (byte)crc;
            _BT.send2BT(data); //Test: 12 00 01 00 03
        }

        public static void GlobalCommandHandler(uint btCmd, uint value, DateTime time)
        {
            switch (btCmd)
            {
                case (uint)Constants.COMMANDS.CMD_GC_GET_CC:
                    _BT.send2BT(value.ToString());
                    break;
                case (uint)Constants.COMMANDS.CMD_GC_RESET_CC:
                    SplitCommand._commandCounter = 0;
                    break;
                case (uint)Constants.COMMANDS.CMD_GC_CPU:
                    _BT.send2BT(Cpu.SystemClock.ToString());
                    break;
                default:
                    break;
            }
            _BT.send2BT("ACK");
        }

        public static void CommandHandler(uint bufferIndex, uint j, DateTime time)
        {
            byte[] _command = _BT.GetCommand(bufferIndex);
            if (_command == null)
            {
                return;
            }
            SplitCommand sc = new SplitCommand(_command);
            sc.ChannelRequest += new NativeEventHandler(channels.ChannelCommandHandler);
            sc.GlobalRequest += new NativeEventHandler(GlobalCommandHandler);
            Thread splitter = new Thread(new ThreadStart(sc.ThreadProc));
            splitter.Start();
        }        
    }   
}

