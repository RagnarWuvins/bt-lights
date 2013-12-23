using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightString
    {
        public enum Functions
        {
            Sine,               // Sine wave emulation
            Sawtooth,           // Sawtooth wave emulation
            SawtoothInverse,    // Inverse sawtooth wave emulation
            NumberOfFunctions,  // Max number of functions
        }

        public enum Modes
        {
            NoOperation, 	// no change of current mode
            Direct,	        // Use mChannelID value
            On,             // On value
            Off,            // Off value
            Function,	    // set Function
            NumberOfModes,  // Number of modes
        }

        public byte mChannelID;
        public int timerPeriod = 50, timerDelay = 0, dimState = MAX6966.PortLimitLow;
        public byte upperLimit
        {
            get { return mUpperLimit; }
            set
            {
                if (value <= mLowerLimit)
                {
                    mUpperLimit = mLowerLimit;
                }
                if (value >= MAX6966.PortLimitHigh)
                {
                    mUpperLimit = MAX6966.PortLimitHigh;
                }
                else
                {
                    mUpperLimit = value;
                }
            }
        }
        public byte lowerLimit
        {
            get { return mLowerLimit; }
            set
            {
                if (value >= mUpperLimit)
                {
                    mLowerLimit = mUpperLimit;
                }
                if (value <= MAX6966.PortLimitLow)
                {
                    mLowerLimit = MAX6966.PortLimitLow;
                }
                else
                {
                    mLowerLimit = value;
                }
            }
        }
        public double rise = 6.0;
        public double offset = 6.0;

        private int mMode = (int)Modes.Function;
        private int mLastMode = (int)Modes.NoOperation;
        private int mValue = MAX6966.PortLimitLow; 
        private int mLastValue = MAX6966.PortLimitLow;
        private int mFunction = (int)Functions.Sine;
        private byte mLowerLimit = MAX6966.PortLimitLow;
        private byte mUpperLimit = MAX6966.PortLimitHigh;
        private bool mDimmingDirection = true;
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
            this.rise = 6.0;
            this.offset = 6.0;
        }

        // Delegate for timer
        public void ModeSelector(object modeInfo)
        {
            // Set the public mode by bluetooth and this delegate here will call the correct function
            switch (mode)
            {
                case (int)Modes.NoOperation:
                    NoOp();
                    break;
                case (int)Modes.Direct:
                    SetDirect();
                    break;
                case (int)Modes.On:
                    On();
                    break;
                case (int)Modes.Off:
                    Off();
                    break;
                case (int)Modes.Function:
                    switch (this.mFunction)
                    {
                        case (int)Functions.Sine:
                            Fade(mLowerLimit, mUpperLimit);
                            break;
                        case (int)Functions.Sawtooth:
                            Saw(mLowerLimit, mUpperLimit);
                            break;
                        case (int)Functions.SawtoothInverse:
                            Saw_rev(mLowerLimit, mUpperLimit);
                            break;
                        default:          
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
            get { return mMax6966.GetPortValue(this.mChannelID); }
            set
            {
                mLastValue = mValue;
                mValue = value > upperLimit ? upperLimit : value;
                mValue = value < lowerLimit ? lowerLimit : value;
                int channelDissapation = (mLastValue - Value) >= 0 ? (mLastValue - Value) : -(mLastValue - Value);
                if (channelDissapation > Constants.MAX_CHANNEL_DISSAPATION && this.mFunction == (int)Functions.Sine)
                {
                    MainProgram.RegisterError(MainProgram.ErrorCodes.ChannelValueAssertion);
                }
                mMax6966.SetPortValue(this.mChannelID, this.mValue);
            }

        }

        // mode setter and getter
        public int mode
        {
            get { return mMode; }
            set
            {
                if (mode >= (int)Modes.NumberOfModes)
                {
                    MainProgram.RegisterError(MainProgram.ErrorCodes.WrongMode);
                    return;
                }
                mLastMode = mMode;
                mMode = value; 
            }
        }

        // mode setter and getter
        public int function
        {
            get { return this.mFunction; }
            set
            {
                if (value >= (int)Functions.NumberOfFunctions)
                {
                    MainProgram.RegisterError(MainProgram.ErrorCodes.WrongFunction);
                    return;
                }
                this.mFunction = value;
            }
        }

        // fade in and out
        private void Fade(int ll, int ul)
        {
            if (mDimmingDirection)
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
                mDimmingDirection = !mDimmingDirection;
            }
            else if (dimState >= ul)
            {
                dimState = ul;
                mDimmingDirection = !mDimmingDirection;
            }
            Value = dimState;
        }

        // fade in and go to lowerlimit
        private void Saw(int ll, int ul)
        {
            dimState += DimCurve(dimState);
            if (dimState >= ul)
            {
                dimState = ll;
            }
            Value = dimState;
        }

        // fade out and go to upperlimit
        private void Saw_rev(int ll, int ul)
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
