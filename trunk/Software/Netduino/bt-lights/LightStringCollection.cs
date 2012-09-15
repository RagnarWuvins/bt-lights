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
                case (int)Constants.MODE.FUNC:
                {
                    // block from other threads
                    lock (new object())
                    {
                        SetChannelFunction(channel, (int)value);
                    }
                    break;
                }
                case (int)Constants.MODE.CMD_GET_VAL:
                {
                    if (value == 0)
                    {
                        GetChannelValue(channel);
                    }
                    else
                    {
                        GetChannelMode(channel);
                    }
                    break;
                }

                case (int)Constants.MODE.CMD_SET_VAL:
                {
                    SetChannelValue(channel, (int)value, true);
                    break;
                }
                case (int)Constants.MODE.CMD_SET_MIN:
                {
                    SetChannelLimits(channel, (int)value, true);
                    break;
                }
                case (int)Constants.MODE.CMD_SET_MAX:
                {
                    SetChannelLimits(channel, (int)value, false);
                    break;
                }
                case (int)Constants.MODE.CMD_SET_DELAY:
                {
                    SetChannelDelay(channel, (int)value);
                    break;
                }
                case (int)Constants.MODE.CMD_SET_PERIOD:
                {
                    SetChannelPeriod(channel, (int)value);
                    break;
                }
                case (int)Constants.MODE.CMD_SET_RISE:
                case (int)Constants.MODE.CMD_SET_OFFSET:
                {
                    SetChannelCurve(channel, (int)value, (int)mode);
                    break;             
                }
                case (int)Constants.MODE.CMD_RESTART:
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
                    SetChannelMode(channel, (int)mode);
                    SetChannelValue(channel, (int)value); 
                    break;
                }
            }
        }

        public void SetChannelFunction(int channel, int value)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].functionIndex = value;
                    channels[i].mode = (int)Constants.MODE.FUNC;
                }
            }
        }

        public void Invoke()
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                channels[i].Clear();
            }
        }

        public void GetChannelValue(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.MODE.CMD_GET_VAL;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)(((uint)Constants.CLASS.CC_CMD << 4) | Constants.MODE.CMD_GET_VAL) + channels[i].Value;
                    e.CommandValue = channels[i].Value;
                    SendChannelData(this, e);
                }
            }
        }

        public void GetChannelMode(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    BTEventArgs e = new BTEventArgs();
                    e.CommandAddress = (int)(0x1 << i);
                    e.CommandMode = (int)Constants.MODE.CMD_GET_VAL;
                    e.CommandClass = (int)Constants.CLASS.CC_CMD;
                    e.CommandChecksum = (int)(((uint)Constants.CLASS.CC_CMD << 4) | Constants.MODE.CMD_GET_VAL) + channels[i].mode;
                    e.CommandValue = channels[i].mode;
                    SendChannelData(this, e);
                }
            }
        }

        public void SetChannelCurve(int channel, int value, int mode)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    if (mode == (int)Constants.MODE.CMD_SET_RISE)
                    {
                        channels[i].rise = value;
                    }
                    else if (mode == (int)Constants.MODE.CMD_SET_OFFSET)
                    {
                        channels[i].offset = value;
                    }
                }
            }
        }

        public void SetChannelValue(int channel, int value, bool setDimState = false)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].Value = value;
                    if (setDimState)
                    {
                        channels[i].dimState = value;
                    }
                }
            }
        }

        public void SetChannelMode(int channel, int mode)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].mode = mode;
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
        public void AllOff()
        {
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch].mode = 2;
            }
        }

        public void AllOn()
        {
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch].mode = 1;
            }
        }

        public void AllFade()
        {
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch].mode = 3;
            }
        }
    }
}
