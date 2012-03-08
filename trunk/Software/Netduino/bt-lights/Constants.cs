// use these assemblies for NETFM target only
#if TARGET
using System;
using Microsoft.SPOT;
#endif

namespace BTLights
{
    class Constants
    {
        #region Global const definition block
        public static int G_MAX_ADDRESS     = 16;           // number of maximum addresses
        public static int G_SET_ADDRESS     = 10;           // number of given addresses
        public static int G_CHANNEL_ADR_MASK    = 0xFFFF;   // mask for the address part

        public enum FW_ERRORS
        {
            CMD_UNKNOWN,
            CMD_CORRUPT,            
        };

        public enum CLASS
        {
            CC_CMD = 1, // Channel command
            GC_CMD,     // global command
            AT_CMD,     // at command
            DP_CMD,     // direct port command
            NUM_CMD     // total number of commands
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
            CMD_RESTART,    // restart the timer
        }

        public enum COMMANDS
        {
            CMD_CC_SET_VALUE,      // Set channel value
            CMD_CC_GET_VALUE,      // Get channel value
            CMD_CC_MODE,           // Set channel mode
            CMD_CC_RETRIGGER,      // Retrigger channel
            CMD_CC_RETRIGGER_ALL,  // Retrigger all channels
            CMD_CC_SET_DELAY,      // Set delay of channel timer
            CMD_CC_GET_DELAY,      // Get delay of channel timer
            CMD_CC_SET_PERIOD,     // Set period of channel timer
            CMD_CC_GET_PERIOD,     // Get period of channel timer 
            CMD_GC_GET_CC,         // Get the command counter
            CMD_GC_RESET_CC,       // Reset the command counter
            CMD_GC_CPU,         
            CMD_ERROR,
            CMD_ACK,
        };
        #endregion

        /// <summary>
        /// Bluetooth module definition block. Contains all needed constants and support functions
        /// </summary>
        #region Bluetooth module definition block
        public static string[] BT_INIT = {
            "at+version?",  // get version         
            "at+class=240404",
            "at+role=0",
            "at+name=Meister Lampe",
            "at+uart=38400,0,0",
            "at+inqm=1,9,48",
            "at+uart?",
            "at+state?"    // current state of BT module
                                        };
        public static string[] BT_RESET = {
            "at+orgl",
            "at+class=240404"
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
        private static byte READ = 0x80; // Read modifier
        private static byte WRITE = 0x00; // write modifier
        public static byte OUTPUT_P0 = 0x00;
        public static byte OUTPUT_P1 = 0x01;
        public static byte OUTPUT_P2 = 0x02;
        public static byte OUTPUT_P3 = 0x03;
        public static byte OUTPUT_P4 = 0x04;
        public static byte OUTPUT_P5 = 0x05;
        public static byte OUTPUT_P6 = 0x06;
        public static byte OUTPUT_P7 = 0x07;
        public static byte OUTPUT_P8 = 0x08;
        public static byte OUTPUT_P9 = 0x09;
        public static byte OUTPUT_ALL = 0x0A; // All outputs
        public static byte RAMP_DOWN = 0x11;
        public static byte RAMP_UP = 0x12;
        public static byte LIM_HIGH = 0xFE; // Maximum allowed value
        public static byte LIM_LOW = 0x03; // Minimum allowed value
        public static byte LIGHT_ON = 0x02; // Maximum allowed value
        public static byte LIGHT_OFF = 0x01; // Minimum allowed value

        public static byte CONF_RUN = 0x01; // run current config
        public static byte CONF_CSRUN = 0x02; // run current config on CS
        public static byte CONF_STAGGER = 0x20; // Ports out of phase
        public static byte CONF_PHASE = 0x00; // Ports in phase

        public static byte CONFIGURATION = 0x10;

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
