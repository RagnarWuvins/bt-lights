// use these assemblies for NETFM target only
#if TARGET
using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;
#endif

namespace BTLights
{
    class Constants
    {
        #region Global const definition block
        public static int G_MAX_ADDRESS     = 16;           // number of maximum addresses
        public static int G_SET_ADDRESS     = 10;           // number of given addresses = number of channels
        public static int G_MAX_CHANNELS = G_SET_ADDRESS;   // number of channels
        public static int G_CHANNEL_ADR_MASK    = 0xFFFF;   // mask for the address part
        public static int C_LENGTH = 0x07; // Command length incl \n\r
        public static int BAUDRATE = 38400; // Global baudrate
        public static int ERROR_LOG_LENGTH = 256; // Length of error log
#if TARGET
        public static Microsoft.SPOT.Hardware.Cpu.Pin ATPIN = Pins.GPIO_PIN_D2; // AT pin to switch to bluetooth service commands
        public static Microsoft.SPOT.Hardware.Cpu.Pin PWM = Pins.GPIO_PIN_D9;   // PWM pin to control the PWM clock of the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin RESET = Pins.GPIO_PIN_D8; // Reset pin (low activ) to reset the BT module
#endif

        public enum FW_ERRORS
        {
            CMD_UNKNOWN,
            CMD_CORRUPT,
            BUFFER_OFERFLOW,
            WRONG_FUNCTION_POINTER,
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
            CMD_GC_GET_CC,         // Get the command counter
            CMD_GC_RESET_CC,       // Reset the command counter
            CMD_GC_CPU, // get the current cpu usage
            CMD_ERROR, // trace out the error log
            CMD_INC_PWM,    // increase PWM by 10 until 200 then go back to 10
            CMD_RESET_ALL,  // reset all channels
            CMD_ACK,
            CMD_NUM,    // Number of commands must be end of enum
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
            "at+version?",  // get version         
            "at+class=240404",
            "at+role=0",
            "at+name=Meister Lampe",
            "at+uart=38400,0,0",
            "at+inqm=1,9,48",
            "at+uart?",
            "at+state?"    // current state of BT module
                                        };
        public static string[] BT_INIT_FAST = {
            "at+version?",  // get version         
            "at+class=240404",
            "at+role=0",
            "at+name=Meister Lampe",
            "at+uart=" + Constants.BAUDRATE + ",0,0",
            "at+inqm=1,9,48",
            "at+uart?",
            "at+state?"    // current state of BT module
                                        };
        public static string[] BT_RESET = {
            "at+orgl",
            "at+class=240404",
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
