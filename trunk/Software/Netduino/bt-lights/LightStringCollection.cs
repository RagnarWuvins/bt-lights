using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightStringCollection
    {       
        public LightString[] channels;
        public event BTEvent SendChannelData;

        private static Timer[] channelTimers = null;
        private static TimerCallback[] channelTimerDelegates = null; 
        private SPI _SPIBus;
        private int _numberOfChannels = 0;
        private byte[] _WriteBuffer;
        private byte[] _ReadBuffer;

        public LightStringCollection(int numberOfChannels, SPI SPIBus)
        {
            this._numberOfChannels = numberOfChannels;
            this._SPIBus = SPIBus;
            this._WriteBuffer = new byte[2];
            this._ReadBuffer = new byte[2];
            channels = new LightString[numberOfChannels];
            channelTimerDelegates = new TimerCallback[numberOfChannels];
            channelTimers = new Timer[numberOfChannels];
            // block from other threads
            lock (new object())
            {
                for (int ch = 0; ch < channels.Length; ch++)
                {
                    channels[ch] = new LightString(_SPIBus, ch);
                    channelTimerDelegates[ch] = new TimerCallback(channels[ch].ModeSelector);
                    channelTimers[ch] = new Timer(channelTimerDelegates[ch], null, channels[ch].timerDelay, channels[ch].timerPeriod);
                }
            }
            _WriteBuffer = new byte[] { Constants.Read(Constants.CONFIGURATION), 0x00 };
            _SPIBus.WriteRead(_WriteBuffer, _ReadBuffer);
            // run this configuration & apply the external input clock
            _WriteBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN | Constants.CONF_OSC };
            _SPIBus.Write(_WriteBuffer);
            _WriteBuffer = new byte[] { Constants.Read(Constants.CONFIGURATION), 0x00 };
            _SPIBus.WriteRead(_WriteBuffer, _ReadBuffer);
        }

        public void ChannelCommandHandler(object sender, BTEventArgs e)
        {
            int channel = e.CommandAddress;
            int mode = e.CommandMode;
            int value = e.CommandValue;
            switch(mode)
            {
                case (int)Constants.COMMAND.CMD_SET_MODE:
                {
                    // block from other threads
                    lock (new object())
                    {
                        SetChannelMode(channel, (int)value);
                    }
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_MODE:
                {
                    GetChannelMode(channel);
                    break;
                }
                case (int)Constants.COMMAND.CMD_SET_VAL:
                {
                    SetChannelValue(channel, (int)value);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_VAL:
                {
                    GetChannelValue(channel);     
                    break;
                }                
                case (int)Constants.COMMAND.CMD_SET_MIN:
                {
                    SetChannelLimits(channel, (int)value, true);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_MIN:
                {
                    GetChannelLimits(channel, true);
                    break;
                }
                case (int)Constants.COMMAND.CMD_SET_MAX:
                {
                    SetChannelLimits(channel, (int)value, false);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_MAX:
                {
                    GetChannelLimits(channel, false);
                    break;
                }
                case (int)Constants.COMMAND.CMD_SET_DELAY:
                {
                    SetChannelDelay(channel, (int)value);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_DELAY:
                {
                    GetChannelDelay(channel);
                    break;
                }
                case (int)Constants.COMMAND.CMD_SET_PERIOD:
                {
                    SetChannelPeriod(channel, (int)value);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_PERIOD:
                {
                    GetChannelPeriod(channel);
                    break;
                }
                case (int)Constants.COMMAND.CMD_SET_RISE:
                case (int)Constants.COMMAND.CMD_SET_OFFSET:
                {
                    SetChannelCurve(channel, (int)value, (int)mode);
                    break;             
                }
                case (int)Constants.COMMAND.CMD_GET_RISE:
                {
                    GetChannelRise(channel, (int)value, (int)mode);
                    break;
                }
                case (int)Constants.COMMAND.CMD_GET_OFFSET:
                {
                    GetChannelOffset(channel, (int)value, (int)mode);
                    break;
                }
                case (int)Constants.COMMAND.CMD_RESTART:
                {
                    // block from other threads
                    lock(new object())
                    {
                        RestartChannelTimer(channel);
                    }
                    break;
                }
                default:
                {
                    Program.THROW_ERROR(Constants.FW_ERRORS.CHANNEL_CMD_UNKNOWN, "Channel command unknown");
                    break;
                }
            }
        }

        /// <summary>
        /// Reset the channel to it's default settings
        /// </summary>
        public void Invoke()
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                channels[i].Clear();
            }
        }

        /// <summary>
        /// Set the channels value. If the setDimState flag is given, it will influnce the "realtime" behaviour.
        /// Otherwise it is good for setting the function number for example
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <param name="value">Value to be set</param>
        /// <param name="setDimState">Activate the value directly</param>
        public void SetChannelValue(int channel, int value)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    //
                    if (channels[i].mode == (int)Constants.MODE.FUNC)
                    {
                        channels[i].dimState = value;
                    }
                    else
                    {
                        channels[i].Value = value;
                    }
                }
            }
        }

        /// <summary>
        /// Get the cahnnels value. Will be send out by bluetooth
        /// </summary>
        /// <param name="channel">Channel number</param>
        public void GetChannelValue(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_VAL;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)Constants.CLASS.CC_CMD  + channels[i].Value;
                    e.CommandValue = channels[i].Value;
                    SendChannelData(this, e);
                }
            }
        }

        /// <summary>
        /// Sets the rise and fall behaviour of the channel
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <param name="value">Value to be set</param>
        /// <param name="mode">Mode of set indicator. can be Constants.COMMAND.CMD_SET_RISE or Constants.COMMAND.CMD_SET_OFFSET</param>
        public void SetChannelCurve(int channel, int value, int mode)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    if (mode == (int)Constants.COMMAND.CMD_SET_RISE)
                    {
                        channels[i].rise = value;
                    }
                    else if (mode == (int)Constants.COMMAND.CMD_SET_OFFSET)
                    {
                        channels[i].offset = value;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public void GetChannelRise(int channel, int value, int mode)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_RISE;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)(uint)Constants.CLASS.CC_CMD + (int)channels[i].rise;
                    e.CommandValue = (int)channels[i].rise;
                    SendChannelData(this, e);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        public void GetChannelOffset(int channel, int value, int mode)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_OFFSET;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)(uint)Constants.CLASS.CC_CMD + (int)channels[i].offset;
                    e.CommandValue = (int)channels[i].offset;
                    SendChannelData(this, e);
                }
            }
        }
        /// <summary>
        /// Sets the channels current mode from the Constants.MODE struct
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <param name="mode">Mode from the Constants.MODE struct</param>
        public void SetChannelMode(int channel, int mode)
        {
            if (mode >= (int)Constants.MODE.NUM_OF_MODES)
            {
                Program.THROW_ERROR(Constants.FW_ERRORS.WRONG_MODE_POINTER);
                mode = (int)Constants.MODE.NOOP;
            }
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].mode = mode;
                }
            }            
        }

        /// <summary>
        /// Gets the current mode the channel is in
        /// </summary>
        /// <param name="channel">Channel number</param>
        public void GetChannelMode(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_MODE;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)(uint)Constants.CLASS.CC_CMD + channels[i].mode;
                    e.CommandValue = channels[i].mode;
                    SendChannelData(this, e);
                }
            }
        }

        public void SetChannelLimits(int channel, int limit, bool lower)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    if (lower)
                    {
                        channels[i].lowerLimit = (byte)limit;
                    }
                    else
                    {
                        channels[i].upperLimit = (byte)limit;
                    }
                }
            }
        }

        public void GetChannelLimits(int channel, bool lower)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                int mode = (int)Constants.COMMAND.CMD_GET_MAX;
                int modeValue = channels[i].upperLimit;
                if (lower)
                {
                    mode = (int)Constants.COMMAND.CMD_GET_MIN;
                    modeValue = channels[i].lowerLimit;
                }
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = mode;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)Constants.CLASS.CC_CMD + modeValue;
                    e.CommandValue = modeValue;
                    SendChannelData(this, e);
                }
            }
        }

        public void SetChannelDelay(int channel, int delay)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].timerDelay = delay;
                    channelTimers[i].Change(channels[i].timerDelay, channels[i].timerPeriod);
                }
            }
        }

        public void GetChannelDelay(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_DELAY;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)Constants.CLASS.CC_CMD + channels[i].timerDelay;
                    e.CommandValue = channels[i].timerDelay;
                    SendChannelData(this, e);
                }
            }
        }

        public void SetChannelPeriod(int channel, int period)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].timerPeriod = period;
                    channelTimers[i].Change(channels[i].timerDelay, channels[i].timerPeriod);
                }
            }
        }

        public void GetChannelPeriod(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.COMMAND.CMD_GET_PERIOD;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)Constants.CLASS.CC_CMD + channels[i].timerPeriod;
                    e.CommandValue = channels[i].timerPeriod;
                    SendChannelData(this, e);
                }
            }
        }

        public void RestartChannelTimer(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].dimState = 0;
                }
            }
        }
    }
}
