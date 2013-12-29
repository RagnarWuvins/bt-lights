using System;
using System.IO.Ports;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Microsoft.SPOT.Hardware;
using PWM_Port = System.Byte;

namespace BTLights
{
    public class MAX6966
    {
        private const byte READ = 0x80; // Read modifier
        private const byte RAMP_DOWN = 0x11;
        private const byte RAMP_UP = 0x12;
        private const byte LIGHT_ON = 0x02; // Maximum allowed value
        private const byte LIGHT_OFF = 0x01; // Minimum allowed value
        private const byte DUMMY = 0xAF; // Dummy data for read

        private const byte CONFIGURATION = 0x10;
        private const byte CONF_RUN = 0x01; // run current config
        private const byte CONF_CSRUN = 0x02; // run current config on CS
        private const byte CONF_STAGGER = 0x20; // Ports out of phase
        private const byte CONF_PHASE = 0x00; // Ports in phase
        private const byte CONF_OSC = 0x80;  // MISO is used as PWM input        

        public const byte NumberOfPorts = 0x0A; // Number of ports
        public const byte PortLimitHigh = 0xFE; // Maximum allowed value
        public const byte PortLimitLow = 0x03; // Minimum allowed value
 
        public const PWM_Port PWM_Port0 = 0x00;
        public const PWM_Port PWM_Port1 = 0x01;
        public const PWM_Port PWM_Port2 = 0x02;
        public const PWM_Port PWM_Port3 = 0x03;
        public const PWM_Port PWM_Port4 = 0x04;
        public const PWM_Port PWM_Port5 = 0x05;
        public const PWM_Port PWM_Port6 = 0x06;
        public const PWM_Port PWM_Port7 = 0x07;
        public const PWM_Port PWM_Port8 = 0x08;
        public const PWM_Port PWM_Port9 = 0x09;
        public const PWM_Port PWM_PortAll = 0x0A; // All outputs

        private SPI.Configuration mSPIConfig;
        private SPI mSPI;

        private byte[] mPortValues;

        /// <summary>
        /// The driver class for the MAX6966 PWM signal generator. It controls up to 10 channels at different PWM values and speeds
        /// </summary>
        /// <param name="module">SPI.SPI_module connector on the Netduino/FEZ board</param>
        /// <param name="CSPin">Pin where the CS is connected</param>
        public MAX6966(SPI.SPI_module module, Cpu.Pin CSPin)
        {
            mSPIConfig = new SPI.Configuration(
            CSPin,              // CS-pin
            false,              // CS-pin active state
            0,                  // The setup time for the SS port
            0,                  // The hold time for the SS port
            false,              // The idle state of the clock
            true,               // The sampling clock edge
            10000,              // The SPI clock rate in KHz
            module              // The used SPI bus (refers to a MOSI MISO and SCLK pinset)
            );

            mSPI = new SPI(mSPIConfig);
            
            mPortValues = new byte[NumberOfPorts];
            for (int i = 0; i < NumberOfPorts; i++)
            {
                mPortValues[i] = LIGHT_OFF;
            }
        }

        /// <summary>
        /// Initializes the PWM MAX6966 module. It can be driven by an external clock.
        /// </summary>
        /// <param name="extPWMEnable">Use a extrenal clock for the MAX. Has to be connected to MISO (now return mChannelID anymore!)</param>
        public void Init(bool extPWMEnable = false)
        {
            byte configByte = (byte)(CONF_RUN | CONF_STAGGER | (extPWMEnable ? (CONF_OSC) : 0x00));
            byte[] init_data = new byte[] { 
                CONFIGURATION, 
                configByte
            };

            mSPI.Write(init_data);
        }

        /// <summary>
        /// Write the values of the ports to the MAX via SPI.
        /// </summary>
        public void Flush()
        {
            for (PWM_Port i = 0; i < NumberOfPorts; i++)
            {
                mSPI.Write(new byte[]{i, mPortValues[i]});
            }
        }

        /// <summary>
        /// Set the value of a port at the PWM module.
        /// </summary>
        /// <param name="port">Byte that defines the port (0-9)</param>
        /// <param name="value">Value of the PWM signal (3-254)</param>
        /// <param name="flushEnable">Write the values directly (default: true)</param>
        public void SetPortValue(PWM_Port port, int value, bool flushEnable = true)
        {
            if (value > PortLimitHigh || value < PortLimitLow)
            {
                throw new ArgumentException("Value out of range", "value");
            }
            if (port < 0 || port > (NumberOfPorts - 1))
            {
                throw new ArgumentException("Port out of range", "port");
            }
            mPortValues[port] = (byte)value;
            if (flushEnable)
            {
                mSPI.Write(new byte[]{port, mPortValues[port]});
            }
        }

        /// <summary>
        /// Set the PWM signal to HIGH
        /// </summary>
        /// <param name="port">Byte that defines the port (0-9)</param>
        public void SetPortOn(PWM_Port port)
        {
            byte[] writeData = new byte[] { port, LIGHT_ON };
            mSPI.Write(writeData);
        }

        /// <summary>
        /// Set the PWM signal to LOW
        /// </summary>
        /// <param name="port">Byte that defines the port (0-9)</param>
        public void SetPortOff(PWM_Port port)
        {
            byte[] writeData = new byte[] { port, LIGHT_OFF };
            mSPI.Write(writeData);
        }

        public byte GetPortValue(PWM_Port port)
        {
            byte[] writeData = new byte[] { (byte)(READ | port), DUMMY };
            byte[] readData = new byte[2];
            mSPI.WriteRead(writeData,readData);
            // From the documentation:
            // Issue another read or write command, and examine
            // the bit stream at DOUT; the second 8 bits are the
            // contents of the register addressed by bits D14
            // through D8 in step 3).
            mSPI.WriteRead(writeData, readData);
            return readData[1];
        }
    }
}
