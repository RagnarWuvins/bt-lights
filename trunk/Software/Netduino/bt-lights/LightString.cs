using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightString
    {
        public byte channel;
        public int mode = 0, timerPeriod = 50, timerDelay = 0;
        public byte upperLimit
        {
            get { return _upperLimit; }
            set
            {
                if (value <= _lowerLimit)
                {
                    _upperLimit = _lowerLimit;
                }
                if (value >= 0xFF)
                {
                    _upperLimit = 0xFE;
                }
                else
                {
                    _upperLimit = value;
                }
            }
        }
        public byte lowerLimit
        {
            get { return _lowerLimit; }
            set
            {
                if (value >= _upperLimit)
                {
                    _lowerLimit = _upperLimit;
                }
                if (value <= 0x00)
                {
                    _lowerLimit = 0x01;
                }
                else
                {
                    _lowerLimit = value;
                }
            }
        }

        private int _dimState = 0x03, _fadeStep = 0x05;
        private byte _lowerLimit = 0x03, _upperLimit = 0x80;
        private double _m = 1.0/6.0, _b = 5.0/6.0;
        private bool _dimDir = true;
        private byte[] writeBuffer;
        private byte[] readBuffer;
        private SPI _SPIBus;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SPIBus">SPI Bus to transmit to</param>
        /// <param name="channel">Number of channel to send</param>
        public LightString(SPI SPIBus, int channel)
        {
            this._SPIBus = SPIBus;
            this.channel = (byte)channel;

            Off();
            writeBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(writeBuffer);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SPIBus">SPI Bus to transmit to</param>
        /// <param name="channel">Number of channel to send</param>
        /// <param name="timerDelay">Delay of timer in ms</param>
        public LightString(SPI SPIBus, int channel, int timerDelay)
        {
            this._SPIBus = SPIBus;
            this.channel = (byte)channel;
            this.timerDelay = timerDelay;

            Off();
            writeBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(writeBuffer);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SPIBus">SPI Bus to transmit to</param>
        /// <param name="channel">Number of channel to send</param>
        /// <param name="timerDelay">Delay of timer in ms</param>
        /// <param name="timerPeriod">Timer period in ms</param>
        public LightString(SPI SPIBus, int channel, int timerDelay, int timerPeriod)
        {
            this._SPIBus = SPIBus;
            this.channel = (byte)channel;
            this.timerDelay = timerDelay;
            this.timerPeriod = timerPeriod;

            Off();
            writeBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(writeBuffer);
        }

        // Delegate for timer
        public void ModeSelector(object modeInfo)
        {
            // Set the public mode by bluetooth and this delegate here will call the correct function
            switch (mode)
            {
                case 0:
                    //default, do nothing
                    break;
                case 1:
                    On();
                    break;
                case 2:
                    Off();
                    break;
                case 3:
                    Fade();
                    break;
                default:
                    break;
            }
        }

        // set to on
        public void On()
        {
            writeBuffer = new byte[] { Constants.Write((byte)channel), Constants.LIGHT_ON };
            _SPIBus.Write(writeBuffer);

        }

        // set to off
        public void Off()
        {
            writeBuffer = new byte[] { Constants.Write((byte)channel), Constants.LIGHT_OFF };
            _SPIBus.Write(writeBuffer);

        }
    
        // set to any value
        public int Value
        {
            get
            {
                writeBuffer = new byte[] { Constants.Read((byte)channel), 0 };
                _SPIBus.WriteRead(writeBuffer, readBuffer);
                return readBuffer[1];
            }
            set
            {
                writeBuffer = new byte[] { Constants.Write((byte)channel), (byte)value };
                _SPIBus.Write(writeBuffer);
            }

        }

        // fade in and out
        public void Fade()
        {
            if (_dimDir)
            {
                _dimState += DimCurve(_dimState);
            }
            else
            {
                _dimState -= DimCurve(_dimState);
            }

            if (_dimState <= _lowerLimit)
            {
                _dimState = _lowerLimit;
                _dimDir = !_dimDir;
            }
            else if (_dimState >= _upperLimit)
            {
                _dimState = _upperLimit;
                _dimDir = !_dimDir;
            }
            writeBuffer = new byte[] { Constants.Write((byte)channel), (byte)_dimState };
            _SPIBus.Write(writeBuffer);
        }

        private int DimCurve(int x)
        {
            double curve = _m * x + _b;
            return (int)curve;
        }
    }
}
