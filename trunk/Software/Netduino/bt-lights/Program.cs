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
        public static PWM _Ext_PWM = new PWM(Constants.PWM); // Init the external PWM for SPI
        public static uint _PWMRate = Constants.DEFAULT_PWM;
        public static int commandCounter = 0;
        public static LightStringCollection channels;
        public static BTModule _BT;
        public static bool _ledState = false;
        public struct Errorlog
        {
            public int[] log;
            public int logIndex;
        }
        public static Errorlog errorlog;
        private static OutputPort _LED = new OutputPort(Pins.ONBOARD_LED, false);

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

            _Ext_PWM.SetPulse(_PWMRate, (_PWMRate / 2));

            _SPIBus = new SPI(_MAX);

            // define the channels and the handler.
            channels = new LightStringCollection(Constants.G_SET_ADDRESS, _SPIBus);
            channels.SendChannelData += new NativeEventHandler(SendChannelData);

            // setting up the serial port for the communication to the BT module
            _BT = new BTModule("COM1", 38400, Parity.None, 8, StopBits.One);
            _BT.CommandReceived += new NativeEventHandler(CommandHandler);
            // Only for first initialization. Damn got no EEProm to save that state?!
            _BT.dump(Constants.BT_INIT_FAST);
            _BT.Reset(); // make a reset so we can detect the board from all devices

            _BT.flushBuffer();  //flush the buffers after init to interact with user hassle free


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
            _BT.send2BT(data);
        }

        public static void GlobalCommandHandler(uint btCmd, uint value, DateTime time)
        {
            int channel = (int)(btCmd & Constants.G_CHANNEL_ADR_MASK);
            int mode = ((int)btCmd >> Constants.G_MAX_ADDRESS);
            switch (mode)
            {
                case (int)Constants.COMMANDS.CMD_GC_GET_CC:
                    _BT.send2BT(SplitCommand._commandCounter);
                    break;
                case (int)Constants.COMMANDS.CMD_GC_RESET_CC:
                    SplitCommand._commandCounter = 0;
                    break;
                case (int)Constants.COMMANDS.CMD_GC_CPU:
                    _BT.send2BT(Cpu.SystemClock);
                    break;
                case (int)Constants.COMMANDS.CMD_INC_PWM:
                    if (_PWMRate < 200)
                    {
                        _PWMRate += 10;
                    }
                    else
                    {
                        _PWMRate = 10;
                    }
                    _Ext_PWM.SetPulse(_PWMRate, (_PWMRate / 2));
                    break;
                case (int)Constants.COMMANDS.CMD_RESET_ALL:
                    channels.Invoke();
                    break;
                case (int) Constants.COMMANDS.CMD_ERROR:
                    for(int i = 0; i < errorlog.logIndex; i++)
                    {
                        _BT.send2BT(errorlog.log[i]);
                    }
                    break;
                default:
                    break;
            }
        }

        public static void THROW_ERROR(object error)
        {
            errorlog.log[errorlog.logIndex] = (int)error << 16 | System.DateTime.Now.Second;
            if (errorlog.logIndex < Constants.ERROR_LOG_LENGTH - 1)
            {
                errorlog.logIndex++;
            }
            else
            {
                errorlog.logIndex = 0;
            }
            string errorString = "FW ERROR OCCURRED: " + (int)error;
            Debug.Print(errorString);
            _ledState = !_ledState;
            _LED.Write(_ledState);
            //_BT.send2BT(errorString);
        }

        public static void CommandHandler(uint writeIndex, uint j, DateTime time)
        {
            
            byte[] _command = _BT.GetCommand(writeIndex);
            if (_command == null)
            {
                return;
            }
            if (writeIndex > Constants.C_LENGTH)
            {
                THROW_ERROR(Constants.FW_ERRORS.BUFFER_OFERFLOW);
            }
            SplitCommand sc = new SplitCommand(_command);
            sc.ChannelRequest += new NativeEventHandler(channels.ChannelCommandHandler);
            sc.GlobalRequest += new NativeEventHandler(GlobalCommandHandler);
            Thread splitter = new Thread(new ThreadStart(sc.ThreadProc));
            splitter.Start();
            //_BT.Acknowledge();
        }        
    }   
}

