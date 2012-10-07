// use these assemblies for NETFM target only
#if TARGET
using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;
#endif

#if !TARGET
using System.Xml.Serialization;
using System.ComponentModel;
using System;
using System.Runtime.Serialization;
#endif

namespace BTLights
{
    [Serializable]
    public class Constants
    {
        #region Global const definition block
        public static int G_MAX_ADDRESS = 16;           // number of maximum addresses
        public static int G_SET_ADDRESS     = 10;           // number of given addresses = number of channels
        public static uint PWM_INIT = 10;
        public static uint PWM_MAX = 20;
        public static int G_MAX_CHANNELS = G_SET_ADDRESS;   // number of channels
        public static int G_CHANNEL_ADR_MASK = 0xFFFF;   // mask for the address part
        public static int C_LENGTH = 0x07; // Command length incl \n\r
        public static int BAUDRATE = 38400; // Global baudrate (default: 38400)
        public static int ERROR_LOG_LENGTH = 256; // Length of error log
        public static int MAX_CHANNEL_DISSAPATION = 128; // value changes should not exceed this limit
        public static int BT_TIMEOUT = 300000;    // time before bluetooth module resets (5 minutes)
#if TARGET
        public static Microsoft.SPOT.Hardware.Cpu.Pin ATPIN = Pins.GPIO_PIN_D2; // AT pin to switch to bluetooth service commands
        public static Microsoft.SPOT.Hardware.Cpu.Pin PWM = Pins.GPIO_PIN_D9;   // PWM pin to control the PWM clock of the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin RESET = Pins.GPIO_PIN_D8; // Reset pin (low activ) to reset the BT module
#endif

        public enum FW_ERRORS
        {
            CMD_UNKNOWN,            // 0x00: just unknown command (maybe came through checksum by incident)
            CMD_CORRUPT,            // 0x01: not a channel (internal) command
            CMD_ASSERT_FAIL,        // 0x02: it was neither internal nor external (maybe length 0)
            INTERPRETATION_ERROR,   // 0x03: to many commands comming in
            WRONG_FUNCTION_POINTER, // 0x04: 0x00: this functioin is not declared for channels
            CHANNEL_VALUE_ASSERT,   // 0x05: the channel changed its value very fast, this should not happen accidently
            EXTRACT_RACE_CONDITION, // 0x06: a race condition occured while extracting the command
            BUFFER_INDEX_OUT_RANGE, // 0x07: The buffer write or read pointer are out of range
            TYPE_CAST_FAILED,       // 0x08: While casting from byte to another type the system encountered an error
        };

        public enum CLASS
        {
            CC_CMD,     // Channel command
            GC_CMD,     // global command
            AT_CMD,     // at command
            DP_CMD,     // direct port command
            CMD_NUM,    // total number of commands
        };

        public enum MODE
        {
            NOOP, 	// no change of current mode
            DIRECT,	// Use channel value
            ON,     // On value
            OFF,    // Off value
            FUNC,	// set Function
            // now the commands start
            CMD_GET_VAL,    // get current value
            CMD_SET_VAL,    // set current value
            CMD_SET_MAX,    // set the maximum value
            CMD_SET_MIN,    // set the minimum value
            CMD_SET_DELAY,    // set the timer delay
            CMD_SET_PERIOD, // set the timer period
            CMD_SET_RISE, // set rise modifier
            CMD_SET_OFFSET, // set offset modifier
            CMD_RESTART,    // restart the timer
            CMD_NUM,    // Number of commands must be end of enum
        }

        public enum COMMANDS
        {
            CMD_GC_GET_CC,      // 0x00: Get the command counter
            CMD_GC_RESET_CC,    // 0x01: Reset the command counter
            CMD_GC_CPU,         // 0x02: get the current cpu usage
            CMD_ERROR,          // 0x03: trace out the error log
            CMD_RESET_ALL,      // 0x04: reset all channels
            CMD_RESET_SYSTEM,   // 0x05: Do a hardware reset
            CMD_GET_SYS_TIME,   // 0x06: Get the time on the board
            CMD_GET_VERSION,    // 0x07: Get the hardware version
            CMD_RESET_BT,       // 0x08: Reset the BT module only
            CMD_NUM,            // Number of commands must be end of enum
        };

        public enum FUNCTIONS
        {
            FUNC_FADE = LIM_LOW,    // lowest possible function value (will be send with value :)
            FUNC_SAW,   // fade in no out
            FUNC_SAW_REV,   // fade out  no in
        }
        #endregion

        /// <summary>
        /// Bluetooth module definition block. Contains all needed constants and support functions
        /// </summary>
        #region Bluetooth module definition block
        public static string[] BT_INIT_SLOW = {
            "at+class=820118", 
            //"at+class=240404",
            "at+role=0",
            "at+name=Meister Lampe",
            "at+uart=38400,0,0",
            "at+inqm=1,9,48",
            "at+rmaad",
            "at+adcn?",
            "at+state?"    // current state of BT module
                                        };
        public static string[] BT_INIT_FAST = {
            "at+version?",  // get version         
            "at+class=820118", //"at+class=240404",
            "at+role=0",
            "at+name=Meister Lampe",
            "at+uart=" + Constants.BAUDRATE + ",0,0",
            "at+inqm=1,9,48",
            "at+uart?",
            "at+state?"    // current state of BT module
                                        };

        public static string[] BT_INIT_BTM222 = {
            "ATN=Meister Lampe",
            "ATE0"
                                                };


        public static string[] BT_RESET = {
            "at+orgl",
            };
        public static string[] BT_BAUD1 = {
            "at+uart=9600,0,0",
            "at+uart?"
            };
        public static string[] BT_BAUD2 = {
            "at+uart=38400,0,0",
            "at+uart?"
            };
        public static string[] BT_BAUD3 = {
            "at+uart=115200,0,0",
            "at+uart?"
            };
        public static string[] BT_TEST = {
            "at+uart?"
            };
        #endregion
        /// <summary>
        /// MAX6966 definition block. Contains all needed constants and support functions
        /// </summary>
        #region MAX6966 definition block
        private const byte READ = 0x80; // Read modifier
        private const byte WRITE = 0x00; // write modifier
        public const byte OUTPUT_P0 = 0x00;
        public const byte OUTPUT_P1 = 0x01;
        public const byte OUTPUT_P2 = 0x02;
        public const byte OUTPUT_P3 = 0x03;
        public const byte OUTPUT_P4 = 0x04;
        public const byte OUTPUT_P5 = 0x05;
        public const byte OUTPUT_P6 = 0x06;
        public const byte OUTPUT_P7 = 0x07;
        public const byte OUTPUT_P8 = 0x08;
        public const byte OUTPUT_P9 = 0x09;
        public const byte OUTPUT_ALL = 0x0A; // All outputs
        public const byte RAMP_DOWN = 0x11;
        public const byte RAMP_UP = 0x12;
        public const byte LIM_HIGH = 0xFE; // Maximum allowed value
        public const byte LIM_LOW = 0x03; // Minimum allowed value
        public const byte LIGHT_ON = 0x02; // Maximum allowed value
        public const byte LIGHT_OFF = 0x01; // Minimum allowed value

        public const byte CONF_RUN = 0x01; // run current config
        public const byte CONF_CSRUN = 0x02; // run current config on CS
        public const byte CONF_STAGGER = 0x20; // Ports out of phase
        public const byte CONF_PHASE = 0x00; // Ports in phase
        public const byte CONF_OSC = 0x80;  // MISO is used as PWM input

        public const byte CONFIGURATION = 0x10;

        public static byte Read(byte Parameter)
        {
            return (byte)(Parameter | Constants.READ);
        }

        public static byte Write(byte Parameter)
        {
            return (byte)(Parameter | Constants.WRITE);
        }
        #endregion
    }
}
