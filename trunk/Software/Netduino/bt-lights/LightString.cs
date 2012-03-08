using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightString
    {
        public byte channel;
        public int timerPeriod = 50, timerDelay = 0, dimState = Constants.LIM_LOW;
        public byte upperLimit
        {
            get { return _upperLimit; }
            set
            {
                if (value <= _lowerLimit)
                {
                    _upperLimit = _lowerLimit;
                }
                if (value >= Constants.LIM_HIGH)
                {
                    _upperLimit = Constants.LIM_HIGH;
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
                if (value <= Constants.LIM_LOW)
                {
                    _lowerLimit = Constants.LIM_LOW;
                }
                else
                {
                    _lowerLimit = value;
                }
            }
        }
        public event NativeEventHandler SendData;

        private int _mode = (int)Constants.MODE.FUNC, _lastMode = (int)Constants.MODE.NOOP,
            _Value = Constants.LIM_LOW, _lastValue = Constants.LIM_LOW;        
        private byte _lowerLimit = Constants.LIM_LOW, _upperLimit = Constants.LIM_HIGH;
        private double _m = 1.0/6.0, _b = 5.0/6.0;
        private bool _dimDir = true;
        private byte[] writeBuffer;
        private byte[] readBuffer = new byte[2];
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

            writeBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), (byte)(Constants.CONF_RUN | Constants.CONF_STAGGER) };
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

            writeBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(writeBuffer);
        }

        // Delegate for timer
        public void ModeSelector(object modeInfo)
        {
            // Set the public mode by bluetooth and this delegate here will call the correct function
            switch (mode)
            {
                case (int)Constants.MODE.NOOP:
                    NoOp();
                    break;
                case (int)Constants.MODE.DIRECT:
                    SetDirect();
                    break;
                case (int)Constants.MODE.ON:
                    On();
                    break;
                case (int)Constants.MODE.OFF:
                    Off();
                    break;
                case (int)Constants.MODE.FUNC:
                    Fade(_lowerLimit, _upperLimit);
                    break;
                default:
                    break;
            }
        }

        public void Command()
        {
            _Value = _lastValue;
            mode = _lastMode;
            Debug.Print("Now in last mode" + mode);
            return;
        }

        public void NoOp()
        {
            return;
        }

        public void SetDirect()
        {
            writeBuffer = new byte[] { Constants.Write((byte)channel), (byte)_Value};
            _SPIBus.Write(writeBuffer);
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
                _lastValue = _Value;
                _Value = value > upperLimit ? upperLimit : value;
                _Value = value < lowerLimit ? lowerLimit : value;
                writeBuffer = new byte[] { Constants.Write((byte)channel), (byte)_Value };
                _SPIBus.Write(writeBuffer);
            }

        }

        // mode setter and getter
        public int mode
        {
            get { return _mode; }
            set
            {
                _lastMode = _mode;
                _mode = value; 
            }
        }

        // fade in and out
        public void Fade(int ll, int ul)
        {
            if (_dimDir)
            {
                dimState += DimCurve(dimState);
            }
            else
            {
                dimState -= DimCurve(dimState);
            }

            if (dimState <= ll)
            {
                dimState = ll;
                _dimDir = !_dimDir;
            }
            else if (dimState >= ul)
            {
                dimState = ul;
                _dimDir = !_dimDir;
            }
            writeBuffer = new byte[] { Constants.Write((byte)channel), (byte)dimState };
            _SPIBus.Write(writeBuffer);
        }

        private int DimCurve(int x)
        {
            double curve = _m * x + _b;
            return (int)curve;
        }
    }
}
