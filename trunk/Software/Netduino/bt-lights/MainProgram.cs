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
using bt_light_framework;

namespace BTLights
{
    class MainProgram
    {
        public const int Version = 3;
        public const int Build = 0;

        public static LightStringCollection channels;
        public static BTM222 mBluetoothModule;
        public static bool _ledState = false;


        public enum ErrorCodes
        {
            CommandUnknown,         // 0x00: just unknown command (maybe came through checksum by incident)
            CommandCorrupt,         // 0x01: not a mChannelID (internal) command
            CommandAssertionFail,   // 0x02: it was neither internal nor external (maybe length 0)
            InterpretationError,    // 0x03: to many commands comming in
            WrongFunction,          // 0x04: This functioin is not declared
            CHANNEL_VALUE_ASSERT,   // 0x05: the mChannelID changed its value very fast, this should not happen accidently
            EXTRACT_RACE_CONDITION, // 0x06: a race condition occured while extracting the command
            BUFFER_INDEX_OUT_RANGE, // 0x07: The buffer write or read pointer are out of range
            TYPE_CAST_FAILED,       // 0x08: While casting from byte to another type the system encountered an error
            WRONG_MODE_POINTER,     // 0x09: Wrong mode set. Will be set to NOOP instead
            CHANNEL_CMD_UNKNOWN,    // 0x0A: This is an unhandled mChannelID or global command
        };
        public struct Errorlog
        {
            public Package log;
            public int logIndex;
            public static int Length = 254; // Length of error log
        }
        public static Errorlog errorlog;    // error log for storing firmware errors

        public static Stopwatch SystemClock = Stopwatch.StartNew();   // start a new system clock
        private static OutputPort _LED = new OutputPort(Pins.ONBOARD_LED, false);

        private static CommandHandler mCommandHandler;
        private static TimerCallback mTimerCallback;
        private static Timer mTimer;

        private static TimerCallback mTraceTimerCallback;
        private static Timer mTraceTimer;

        private static int[] values = new int[MAX6966.NumberOfPorts];
        private static bool[] dirs = new bool[MAX6966.NumberOfPorts];

        public static void Main()
        {
            // Create a new error log. Since the Netduino has no EEPROM this is a RAM solution :/
            errorlog = new Errorlog();
            errorlog.logIndex = 1;
            errorlog.log = new Package(Errorlog.Length);

            // Setup BTM222:
            mBluetoothModule = new BTM222("COM1", Constants.BAUDRATE, Constants.ATPIN, Constants.RESET);

            // Setup LightStrings (includes MAX6966 communication):
            channels = new LightStringCollection(10);

            // create a new command Handler
            mCommandHandler = new CommandHandler(channels);

            // Create a timer delegate callback for the trace channel command            
            mTraceTimerCallback = new TimerCallback(MainProgram.ChannelTraceDelegate);

            // Attaching handlers for events
            mBluetoothModule.CommandReceived += new BTEvent(mCommandHandler.SplitCommand);            

            // after 5 minutes the BT module will be reset in order to prevent blocking
            mTimerCallback = new TimerCallback(ResetBluetoothCallback);
            mTimer = new Timer(mTimerCallback, null, BTM222.ConnectionTimeout, 0);

            RegisterError(ErrorCodes.CHANNEL_CMD_UNKNOWN);

            Thread.Sleep(-1);
        }

        public static void DebugOut(byte[] input)
        {
            mBluetoothModule.Write(input, 0, input.Length);
        }

        private static void ResetBluetoothCallback(object sender)
        {
            mBluetoothModule.Reset();
            mTimer.Dispose();
        }

        public static void RestartBluetoothTimeout()
        {
            if (mTimer != null)
            {
                mTimer.Change(BTM222.ConnectionTimeout, 0);
            }
        }

        public static void ResetBluetooth()
        {
            mBluetoothModule.Reset();
            mTimer.Dispose();
        }

        public static void ResetSystem()
        {
            lock (new object())
            {
                PowerState.RebootDevice(false);
            }
        }

        public static void ChannelTracer(uint state)
        {
            if (state == 0)
            {
                if (mTraceTimer != null)
                {
                    mTraceTimer.Dispose();
                }
            }
            else
            {
                mTraceTimer = new Timer(mTraceTimerCallback, null, 0, 100);
                
            }
        }

        private static void ChannelTraceDelegate(object eventArgs)
        {
            Package p = new Package(channels.Length);
            for(uint i = 1; i < p.PackageFrames.Length; i++)
            {
                p.PackageFrames[i].ContentByte[0]  = (byte)CommandHandler.Command.GetValue;
                p.PackageFrames[i].ContentByte[1] = (byte)(i-1);
                p.PackageFrames[i].ContentByte[3] = (byte)(channels.GetChannelValue(i - 1));
            }
            DebugOut(p.GetPackageBytes());
        }

        public static void SendErrorLog()
        {
            byte[] writeBuffer = errorlog.log.GetPackageBytes();
            mBluetoothModule.Write(writeBuffer, 0, writeBuffer.Length);
        }

        public static void RegisterError(ErrorCodes error, String addOutput = "")
        {
            Frame ErrorLogEntry = new Frame();
            ErrorLogEntry.Command = (byte)CommandHandler.Command.GetErrorLog;
            ErrorLogEntry.Payload = (uint)SystemClock.TotalTime;
            ErrorLogEntry.ContentByte[1] = (byte)error;

            errorlog.log.PackageFrames[errorlog.logIndex] = ErrorLogEntry;
            if (errorlog.logIndex < Errorlog.Length - 1)
            {
                errorlog.logIndex++;
            }
            else
            {
                errorlog.logIndex = 1;
            }
            string errorString = "RegisterError: FW ERROR OCCURRED: " + (int)error + addOutput + " Time: " + SystemClock.TotalTime;
            Debug.Print(errorString);
            _ledState = !_ledState;
            _LED.Write(_ledState);
            //mBluetoothModule.send2BT(errorString);
        }


    }
}

