using System;
using System.IO.Ports;
using System.Globalization;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Text;
using System.Diagnostics;

namespace BTLights
{
    class Program
    {
        public const int HW_VERSION = 1;
        public const int HW_BUILD = 1;
        public static SPI.Configuration _MAX;
        public static SPI _SPIBus;
        public static PWM _Ext_PWM = new PWM(Constants.PWM); // Init the external PWM for SPI
        public static uint _PWMRate = Constants.PWM_INIT;
        public static int commandCounter = 0;
        public static LightStringCollection channels;
        public static BTModule _BT;
        public static bool _ledState = false;
        public struct Errorlog
        {
            public int[] log;
            public int logIndex;
        }
        public static Errorlog errorlog;    // error log for storing firmware errors
        public static Stopwatch sysClock = Stopwatch.StartNew();   // start a new system clock
        private static OutputPort _LED = new OutputPort(Pins.ONBOARD_LED, false);
        private static bool ditherEnable = false;
        private static TimerCallback ditherCallback;
        private static Timer ditherTimer;
        private static CommandHandler mCommandHandler;

        public static void Main()
        {
            // Create a new error log. Since the Netduino has no EEPROM this is a RAM solution :/
            errorlog = new Errorlog();
            errorlog.logIndex = 0;
            errorlog.log = new int[Constants.ERROR_LOG_LENGTH];
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

            if (ditherEnable)
            {
                ditherCallback = new TimerCallback(ditherPWM);
                ditherTimer = new Timer(ditherCallback, null, 0, 1);
            }
            else
            {
                _Ext_PWM.SetPulse(_PWMRate, (_PWMRate / 2));
            }

            // Create a new SPI Bus
            _SPIBus = new SPI(_MAX);
            // define the channels and the handler.
            channels = new LightStringCollection(Constants.G_SET_ADDRESS, _SPIBus);
            // setting up the serial port for the communication to the BT module
            _BT = new BTModule("COM1", 38400, Parity.None, 8, StopBits.One);
            // create a new command Handler
            mCommandHandler = new CommandHandler();

            // Attaching handlers for events
            channels.SendChannelData += new BTEvent(SendChannelData);
            _BT.CommandReceived += new BTEvent(mCommandHandler.SplitCommand);
            // Set up the command handler
            mCommandHandler.ChannelRequest += new BTEvent(channels.ChannelCommandHandler);
            mCommandHandler.GlobalRequest += new BTEvent(GlobalCommandHandler);

            // Starting initilization
            // Only for first initialization. Damn got no EEProm to save that state?!
            _BT.dump(Constants.BT_INIT_SLOW);
            _BT.Reset(); // make a reset so we can detect the board from all devices

            Thread.Sleep(-1);
        }

        private static void ditherPWM(object intObj)
        {
            if (_PWMRate < Constants.PWM_MAX)
            {
                _PWMRate++;
            }
            else
            {
                _PWMRate = Constants.PWM_INIT;
            }
            _Ext_PWM.SetPulse(_PWMRate, (_PWMRate / 2));
        }

        public static void SendChannelData(object sender, BTEventArgs e)
        {
            byte[] data = new byte[5];
            data[0] = (byte)((e.CommandClass << 4) | e.CommandMode);
            data[1] = (byte)((e.CommandAddress & 0xFF00) >> 8);
            data[2] = (byte)(e.CommandAddress & 0x00FF);
            data[3] = (byte)(e.CommandValue);
            data[4] = (byte)(e.CommandChecksum);
            _BT.send2BT(data);
        }

        public static void GlobalCommandHandler(object sender, BTEventArgs e)
        {
            int channel = e.CommandAddress;
            int mode = e.CommandMode;
            switch (mode)
            {
                case (int)Constants.COMMANDS.CMD_GC_GET_CC:
                    _BT.send2BT(CommandHandler.CommandCounter);
                    break;
                case (int)Constants.COMMANDS.CMD_GC_RESET_CC:
                    CommandHandler.CommandCounter = 0;
                    break;
                case (int)Constants.COMMANDS.CMD_GC_CPU:
                    _BT.send2BT((int)sysClock.TotalTime);
                    break;
                case (int)Constants.COMMANDS.CMD_RESET_ALL:
                    channels.Invoke();
                    break;
                case (int)Constants.COMMANDS.CMD_RESET_SYSTEM:
                    lock (new object())
                    {
                        PowerState.RebootDevice(false);
                    }
                    break;
                case (int)Constants.COMMANDS.CMD_ERROR:
                    for (int i = 0; i < errorlog.logIndex; i++)
                    {
                        _BT.send2BT(errorlog.log[i]);
                    }
                    break;
                case (int)Constants.COMMANDS.CMD_GET_SYS_TIME:
                    _BT.send2BT((long)sysClock.TotalTime);                    
                    break;
                case(int)Constants.COMMANDS.CMD_GET_VERSION:
                    byte[] data = new byte[Constants.C_LENGTH - 2];
                    data[0] = (((int)Constants.CLASS.GC_CMD << 4) | (int)Constants.COMMANDS.CMD_GET_VERSION);
                    data[3] = (HW_VERSION << 4) | (HW_BUILD);
                    data[4] = (byte)(data[0] + data[3]);
                    _BT.send2BT(data);
                    break;
                default:
                    break;
            }
        }

        public static void THROW_ERROR(object error, String addOutput = "")
        {

            int temp = ((int)error << 24) | (((int)sysClock.TotalTime) & 0x00FFFFFF);            
            errorlog.log[errorlog.logIndex] = temp;
            if (errorlog.logIndex < Constants.ERROR_LOG_LENGTH - 1)
            {
                errorlog.logIndex++;
            }
            else
            {
                errorlog.logIndex = 0;
            }
            string errorString = "THROW_ERROR: FW ERROR OCCURRED: " + (int)error + addOutput + " Time: " + sysClock.TotalTime + "Uint Time: " + (((int)sysClock.TotalTime) & 0x00FFFFFF);
            Debug.Print(errorString);
            _ledState = !_ledState;
            _LED.Write(_ledState);
            //_BT.send2BT(errorString);
        }


    }
}

