using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightString
    {
        //TODO: constants for default values
        public byte mChannelID;
        public int timerPeriod = 50, timerDelay = 0, dimState = MAX6966.PortLimitLow, functionIndex = (int)Constants.FUNCTIONS.FUNC_FADE;
        public byte upperLimit
        {
            get { return _upperLimit; }
            set
            {
                if (value <= _lowerLimit)
                {
                    _upperLimit = _lowerLimit;
                }
                if (value >= MAX6966.PortLimitHigh)
                {
                    _upperLimit = MAX6966.PortLimitHigh;
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
                if (value <= MAX6966.PortLimitLow)
                {
                    _lowerLimit = MAX6966.PortLimitLow;
                }
                else
                {
                    _lowerLimit = value;
                }
            }
        }
        public double rise = 6.0;
        public double offset = 6.0;

        private int _mode = (int)Constants.MODE.FUNC, _lastMode = (int)Constants.MODE.NOOP,
            mValue = MAX6966.PortLimitLow, _lastValue = MAX6966.PortLimitLow;
        private byte _lowerLimit = MAX6966.PortLimitLow, _upperLimit = MAX6966.PortLimitHigh;
        private bool _dimDir = true;
        private byte[] writeBuffer;
        private byte[] readBuffer = new byte[2];
        private MAX6966 mMax6966;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SPIBus">SPI Bus to transmit to</param>
        /// <param name="mChannelID">Number of mChannelID to send</param>
        public LightString(MAX6966 SPIBus, int channel)
        {
            this.mMax6966 = SPIBus;
            this.mChannelID = (byte)channel;
        } 

        // reset mChannelID
        public void Clear()
        {
            this.timerDelay = 0;
            this.timerPeriod = 50;
            this.dimState = MAX6966.PortLimitLow;
            this.functionIndex = (int)Constants.FUNCTIONS.FUNC_FADE;       
            this.rise = 6.0;
            this.offset = 6.0;
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
                    switch (functionIndex)
                    {
                        case (int)Constants.FUNCTIONS.FUNC_FADE:
                            Fade(_lowerLimit, _upperLimit);
                            break;
                        case (int)Constants.FUNCTIONS.FUNC_SAW:
                            Saw(_lowerLimit, _upperLimit);
                            break;
                        case (int)Constants.FUNCTIONS.FUNC_SAW_REV:
                            Saw_rev(_lowerLimit, _upperLimit);
                            break;
                        default:
                            MainProgram.RegisterError(MainProgram.ErrorCodes.WrongFunction);
                            functionIndex = (int)Constants.FUNCTIONS.FUNC_FADE;             
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public void NoOp()
        {
            return;
        }

        public void SetDirect()
        {
            mMax6966.SetPortValue(this.mChannelID, this.mValue);
        }

        // set to on (cannot be set by Value variable, because out of range!)
        public void On()
        {
            mMax6966.SetPortOn(this.mChannelID);
        }

        // set to off  (cannot be set by Value variable, because out of range!)
        public void Off()
        {
            mMax6966.SetPortOff(this.mChannelID);
        }
    
        // set to any value
        public int Value
        {
            get { return mValue; }
            set
            {
                _lastValue = mValue;
                mValue = value > upperLimit ? upperLimit : value;
                mValue = value < lowerLimit ? lowerLimit : value;
                int channelDissapation = (_lastValue - Value) >= 0 ? (_lastValue - Value) : -(_lastValue - Value);
                if (channelDissapation > Constants.MAX_CHANNEL_DISSAPATION)
                {
                    MainProgram.RegisterError(MainProgram.ErrorCodes.CHANNEL_VALUE_ASSERT);
                }
                mMax6966.SetPortValue(this.mChannelID, this.mValue);
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
            Value = dimState;
        }

        // fade in and go to lowerlimit
        public void Saw(int ll, int ul)
        {
            dimState += DimCurve(dimState);
            if (dimState >= ul)
            {
                dimState = ll;
            }
            Value = dimState;
        }

        // fade out and go to upperlimit
        public void Saw_rev(int ll, int ul)
        {
            dimState -= DimCurve(dimState);
            if (dimState <= ll)
            {
                dimState = ul;
            }
            Value = dimState;
        }

        private int DimCurve(int x)
        {
            //private double _m = 1.0 / this.rise, _b = 5.0 / this.offset;
            this.rise = this.rise == 0 ? 1 : this.rise;
            this.offset = this.offset == 0 ? 1 : this.offset;
            double curve = (1.0 / this.rise) * x + (5.0 / this.offset);
            return (int)curve;
        }
    }
}
