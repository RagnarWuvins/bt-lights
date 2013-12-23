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
        public static int G_SET_ADDRESS = 10;           // number of given addresses = number of channels
        public static uint PWM_INIT = 10;
        public static uint PWM_MAX = 20;
        public static int G_MAX_CHANNELS = G_SET_ADDRESS;   // number of channels
        public static int G_CHANNEL_ADR_MASK = 0xFFFF;   // mask for the address part
        public static int C_LENGTH = 0x08; // Command length incl \n\r
        public static int BAUDRATE = 115200; // Default baudrate MCU -> BT (default: 19200)        
        public static int MAX_CHANNEL_DISSAPATION = 128; // value changes should not exceed this limit        
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
        #endregion

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


    }
}
