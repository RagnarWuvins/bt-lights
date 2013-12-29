using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace BTLights
{
    public class LightStringCollection
    {
        public LightString[] channels;
        public int Length  = 0;

        private static Timer[] channelTimers = null;
        private static TimerCallback[] channelTimerDelegates = null;
        private MAX6966 mMAX6966;

        public LightStringCollection(int numberOfChannels)
        {
            this.Length = numberOfChannels;
            this.mMAX6966 = new MAX6966(SPI_Devices.SPI1, Pins.GPIO_PIN_D10);
            this.mMAX6966.Init(false);
            this.mMAX6966.Flush();
            this.mMAX6966.SetPortOn(MAX6966.PWM_PortAll);
            
            channels = new LightString[numberOfChannels];
            channelTimerDelegates = new TimerCallback[numberOfChannels];
            channelTimers = new Timer[numberOfChannels];
            // block from other threads
            lock (new object())
            {
                for (int ch = 0; ch < channels.Length; ch++)
                {
                    channels[ch] = new LightString(mMAX6966, ch);
                    channelTimerDelegates[ch] = new TimerCallback(channels[ch].ModeSelector);
                    channelTimers[ch] = new Timer(channelTimerDelegates[ch], null, channels[ch].timerDelay, channels[ch].timerPeriod);
                }
            }
        }

        /// <summary>
        /// Reset the mChannelID to it's default settings
        /// </summary>
        public void Invoke()
        {
            for (int i = 0; i < this.Length; i++)
            {
                channels[i].Clear();
            }
        }

        /// <summary>
        /// Set the channels value. If the setDimState flag is given, it will influnce the "realtime" behaviour.
        /// Otherwise it is good for setting the function number for example
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        /// <param name="value">Value to be set</param>
        /// <param name="setDimState">Activate the value directly</param>
        public void SetChannelValue(uint channel, int value)
        {
            if (channels[channel].mode == (int)LightString.Modes.Function)
            {
                channels[channel].dimValue = value;
            }
            else
            {
                channels[channel].Value = value;
            }      
        }

        /// <summary>
        /// Get the cahnnels value. Will be send out by bluetooth
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        public int GetChannelValue(uint channel)
        {
            return channels[channel].Value;
        }

        /// <summary>
        /// Sets the rise and fall behaviour of the mChannelID
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        /// <param name="value">Value to be set</param>
        /// <param name="mode">Mode of set indicator. can be CommandHandler.Command.SetRise or CommandHandler.Command.SetOffset</param>
        public void SetChannelCurve(uint channel, int value, int mode)
        {

            if (mode == (int)CommandHandler.Command.SetRise)
            {
                channels[channel].rise = value;
            }
            else if (mode == (int)CommandHandler.Command.SetOffset)
            {
                channels[channel].offset = value;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mChannelID"></param>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public int GetChannelRise(uint channel)
        {
            return (int)channels[channel].rise;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mChannelID"></param>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public int GetChannelOffset(uint channel)
        {
            return (int)channels[channel].offset;

        }
        /// <summary>
        /// Sets the channels current mode from the Constants.MODE struct
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        /// <param name="mode">Mode from the Constants.MODE struct</param>
        public void SetChannelMode(uint channel, int mode)
        {            
            channels[channel].mode = mode;
        }

        /// <summary>
        /// Gets the current mode the mChannelID is in
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        public int GetChannelMode(uint channel)
        {
            return channels[channel].mode;
        }

        /// <summary>
        /// Sets the channels function
        /// </summary>
        /// <param name="mChannelID">Channel number</param>
        /// <param name="function">Channel function</param>
        public void SetChannelFunction(uint channel, int function)
        {
            channels[channel].function = function;
        }

        /// <summary>
        /// Gets the channels function
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <returns>The value of the current function</returns>
        public int GetChannelFunction(uint channel)
        {
            return channels[channel].function;
        }

        public void SetChannelLimits(uint channel, int limit, bool lower)
        {
            if (lower)
            {
                channels[channel].lowerLimit = (byte)limit;
            }
            else
            {
                channels[channel].upperLimit = (byte)limit;
            }
        }

        public int GetChannelLimits(uint channel, bool lower)
        {
            if (lower)
            {
                return channels[channel].lowerLimit;
            }
            else
            {
                return channels[channel].upperLimit;
            }
        }

        public void SetChannelDelay(uint channel, int delay)
        {

            channels[channel].timerDelay = delay;
            channelTimers[channel].Change(channels[channel].timerDelay * 10, channels[channel].timerPeriod);
        }

        public int GetChannelDelay(uint channel)
        {
            return channels[channel].timerDelay;

        }

        public void SetChannelPeriod(uint channel, int period)
        {
                channels[channel].timerPeriod = period;
                channelTimers[channel].Change(channels[channel].timerDelay, channels[channel].timerPeriod);
        }

        public int GetChannelPeriod(uint channel)
        {
            return channels[channel].timerPeriod;                    
        }

        public void RestartChannelTimer(uint channel)
        {            
            channels[channel].dimValue = 0;
            channelTimers[channel].Dispose();
            channelTimers[channel].Change(channels[channel].timerDelay * 10, channels[channel].timerPeriod);            
        }

        public void RestartAllTimer()
        {
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch].dimValue = MAX6966.PortLimitLow;
            }
        }
    }
}
