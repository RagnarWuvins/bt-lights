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
        public static byte[] WriteBuffer;
        public static byte[] ReadBuffer;
        
        public static int commandCounter = 0;

        public static BTModule _BT;

        public static LightString channelP9;
        public static LightString channelP8;
        public static LightString channelP7;
        public static LightString channelP6;
        public static LightString channelP5;
        public static LightString channelP4;
        public static LightString channelP3;
        public static LightString channelP2;

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

            WriteBuffer = new byte[2];
            ReadBuffer = new byte[2];
            _SPIBus = new SPI(_MAX);

            // setting up the serial port for the communication to the BT module
            _BT = new BTModule("COM1", 38400, Parity.None, 8, StopBits.One, Pins.GPIO_PIN_D2);
            _BT.CommandReceived += new NativeEventHandler(setMode);
            // Only for first initialization. Damn got no EEProm to save that state?!
            _BT.dump(Constants.BT_INIT);
            _BT.send2BT("at+reset");

            channelP9 = new LightString(_SPIBus, 9, 0);
            channelP8 = new LightString(_SPIBus, 8, 300);
            channelP7 = new LightString(_SPIBus, 7, 600);
            channelP6 = new LightString(_SPIBus, 6, 900);
            channelP5 = new LightString(_SPIBus, 5, 1200);
            channelP4 = new LightString(_SPIBus, 4, 1500);
            channelP3 = new LightString(_SPIBus, 3, 1800);
            channelP2 = new LightString(_SPIBus, 2, 2100);

            channelP9.mode = 1;
            channelP8.mode = 2;

            TimerCallback TimerDelegate_P9 = new TimerCallback(channelP9.ModeSelector);
            Timer Timer_P9 = new Timer(TimerDelegate_P9, null, channelP9.timerDelay, channelP9.timerPeriod);
            
            TimerCallback TimerDelegate_P8 = new TimerCallback(channelP8.ModeSelector);
            Timer Timer_P8 = new Timer(TimerDelegate_P8, null, channelP8.timerDelay, channelP8.timerPeriod);

            TimerCallback TimerDelegate_P7 = new TimerCallback(channelP7.ModeSelector);
            Timer Timer_P7 = new Timer(TimerDelegate_P7, null, channelP7.timerDelay, channelP7.timerPeriod);

            TimerCallback TimerDelegate_P6 = new TimerCallback(channelP6.ModeSelector);
            Timer Timer_P6 = new Timer(TimerDelegate_P6, null, channelP6.timerDelay, channelP6.timerPeriod);

            TimerCallback TimerDelegate_P5 = new TimerCallback(channelP5.ModeSelector);
            Timer Timer_P5 = new Timer(TimerDelegate_P5, null, channelP5.timerDelay, channelP5.timerPeriod);

            TimerCallback TimerDelegate_P4 = new TimerCallback(channelP4.ModeSelector);
            Timer Timer_P4 = new Timer(TimerDelegate_P4, null, channelP4.timerDelay, channelP4.timerPeriod);

            TimerCallback TimerDelegate_P3 = new TimerCallback(channelP3.ModeSelector);
            Timer Timer_P3 = new Timer(TimerDelegate_P3, null, channelP3.timerDelay, channelP3.timerPeriod);

            TimerCallback TimerDelegate_P2 = new TimerCallback(channelP2.ModeSelector);
            Timer Timer_P2 = new Timer(TimerDelegate_P2, null, channelP2.timerDelay, channelP2.timerPeriod);
            
            WriteBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(WriteBuffer);

            Thread.Sleep(-1);
        }

        public static void setMode(uint bufferIndex, uint j, DateTime time)
        {
            string _command = _BT.GetCommand(bufferIndex);
            if (_command == "" || _command == null)
            {
                return;
            }
            commandCounter++;
            switch (_command.ToLower())
            {
                case "a":
                    channelP9.mode = 0;
                    channelP8.mode = 0;
                    channelP7.mode = 0;
                    channelP6.mode = 0;
                    channelP5.mode = 0;
                    channelP4.mode = 0;
                    channelP3.mode = 0;
                    channelP2.mode = 0;
                    _BT.Acknowledge();
                    break;
                case "b":
                    channelP9.mode = 1;
                    channelP8.mode = 1;
                    channelP7.mode = 1;
                    channelP6.mode = 1;
                    channelP5.mode = 1;
                    channelP4.mode = 1;
                    channelP3.mode = 1;
                    channelP2.mode = 1;
                    _BT.Acknowledge();
                    break;
                case "c":
                    channelP9.mode = 2;
                    channelP8.mode = 2;
                    channelP7.mode = 2;
                    channelP6.mode = 2;
                    channelP5.mode = 2;
                    channelP4.mode = 2;
                    channelP3.mode = 2;
                    channelP2.mode = 2;
                    _BT.Acknowledge();
                    break;
                case "d":
                    channelP9.mode = 1;
                    channelP8.mode = 2;
                    channelP7.mode = 1;
                    channelP6.mode = 2;
                    channelP5.mode = 1;
                    channelP4.mode = 2;
                    channelP3.mode = 1;
                    channelP2.mode = 2;
                    _BT.Acknowledge();
                    break;
                case "f":
                    _BT.dump(Constants.BT_INIT);
                    _BT.Acknowledge();
                    break;
                case "gc+clearcommandcounter":
                    commandCounter = 0;
                    break;
                case "gc+commandsreceived?":
                    _BT.send2BT(commandCounter.ToString());
                    break;
                default:
                    Debug.Print(_command);
                    break;
            }            
        }      
        
    }
}
