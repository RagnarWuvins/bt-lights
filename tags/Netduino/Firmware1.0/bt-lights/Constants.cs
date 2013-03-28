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
        public static int C_LENGTH = 0x08; // Command length incl \n\r
        public static int BAUDRATE = 115200; // Default baudrate MCU -> BT (default: 19200)
        public static int ERROR_LOG_LENGTH = 256; // Length of error log
        public static int MAX_CHANNEL_DISSAPATION = 128; // value changes should not exceed this limit
        public static int BT_TIMEOUT = 300000;    // time before bluetooth module resets (5 minutes)
#if TARGET
        public static Microsoft.SPOT.Hardware.Cpu.Pin RXPIN = Pins.GPIO_PIN_D0;     // RX pin for receiving from UART
        public static Microsoft.SPOT.Hardware.Cpu.Pin TXPIN = Pins.GPIO_PIN_D1;     // TX pin for transmitting to UART
        public static Microsoft.SPOT.Hardware.Cpu.Pin ATPIN = Pins.GPIO_PIN_D2;     // AT pin to switch to bluetooth service commands
        public static Microsoft.SPOT.Hardware.Cpu.Pin RESERVED1 = Pins.GPIO_PIN_D6; // Reserved for future use
        public static Microsoft.SPOT.Hardware.Cpu.Pin RESERVED2 = Pins.GPIO_PIN_D7; // Reserved for future use
        public static Microsoft.SPOT.Hardware.Cpu.Pin RESET = Pins.GPIO_PIN_D8;     // Reset pin (low activ) to reset the BT module
        public static Microsoft.SPOT.Hardware.Cpu.Pin PWM = Pins.GPIO_PIN_D9;       // PWM pin to control the PWM clock of the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin CS = Pins.GPIO_PIN_D10;       // Slave select of the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin MOSI = Pins.GPIO_PIN_D11;     // MOISof the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin MISO = Pins.GPIO_PIN_D12;     // MISO of the SPI module
        public static Microsoft.SPOT.Hardware.Cpu.Pin SCLK = Pins.GPIO_PIN_D13;     // Serial clock of the SPI module

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
            WRONG_MODE_POINTER,     // 0x09: Wrong mode set. Will be set to NOOP instead
            CHANNEL_CMD_UNKNOWN,    // 0x0A: This is an unhandled channel or global command
        };

        public enum CLASS
        {
            CC_CMD,     // Channel command
            GC_CMD,     // global command
            AT_CMD,     // at command
            DP_CMD,     // direct port command
            CMD_NUM,    // total number of commands
        };
        
        public enum COMMAND
        {
            CMD_SET_MODE,   // 0x00: Set the channel mode
            CMD_GET_MODE,   // 0x01: Get the channel mode
            CMD_SET_VAL,    // 0x02: Set current value
            CMD_GET_VAL,    // 0x03: Get current value            
            CMD_SET_MAX,    // 0x04: Set the maximum value
            CMD_GET_MAX,    // 0x05: Get the maximum value
            CMD_SET_MIN,    // 0x06: Set the minimum value
            CMD_GET_MIN,    // 0x07: Get the minimum value
            CMD_SET_DELAY,  // 0x08: Set the timer delay
            CMD_GET_DELAY,  // 0x09: Get the timer delay
            CMD_SET_PERIOD, // 0x0A: Set the timer period
            CMD_GET_PERIOD, // 0x0B: Get the timer period
            CMD_SET_RISE,   // 0x0C: Set rise modifier
            CMD_GET_RISE,   // 0x0D: Get rise modifier
            CMD_SET_OFFSET, // 0x0E: Set offset modifier
            CMD_GET_OFFSET, // 0x0F: Set offset modifier
            CMD_RESTART,    // 0x10: Reset the channel timer     
            CMD_NUM,        // Number of commands must be end of enum
        }

        public enum GLOBAL_COMMAND
        {
            CMD_GET_CC,         // 0x00: Get the command counter
            CMD_RESET_CC,       // 0x01: Reset the command counter
            CMD_CPU,            // 0x02: get the current cpu usage
            CMD_ERROR,          // 0x03: trace out the error log
            CMD_RESET_ALL,      // 0x04: reset all channels
            CMD_RESET_SYSTEM,   // 0x05: Do a hardware reset
            CMD_GET_SYS_TIME,   // 0x06: Get the time on the board
            CMD_GET_VERSION,    // 0x07: Get the hardware version
            CMD_RESET_BT,       // 0x08: Reset the BT module only
            CMD_GET_CMD_TIME,   // 0x09: Get Command processing time
            CMD_NUM,            // Number of commands must be end of enum
        };

        public enum MODE
        {
            NOOP, 	        // no change of current mode
            DIRECT,	        // Use channel value
            ON,             // On value
            OFF,            // Off value
            FUNC,	        // set Function
            NUM_OF_MODES,   // Number of modes
        }

        public enum FUNCTIONS
        {
            FUNC_FADE = LIM_LOW,    // lowest possible function value (will be send with value :)
            FUNC_SAW,                // fade in no out
            FUNC_SAW_REV,           // fade out  no in
            NUM_OF_FUNC,             // Max number of functions
        }
        #endregion

        #region BTM222 Constants
        public static string[] BT_INIT_BTM222 = {
            "ATN=Meister Lampe",
            "ATL?",
            "ATL5",
            "ATE0"
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
