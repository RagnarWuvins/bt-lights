using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace BTLights
{
    public class LightStringCollection
    {
        public LightString[] channels;
        public event NativeEventHandler SendChannelData;

        private static Timer[] channelTimers;
        private static TimerCallback[] channelTimerDelegates; 
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
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = new LightString(_SPIBus, ch);
                channelTimerDelegates[ch] = new TimerCallback(channels[ch].ModeSelector);
                channelTimers[ch] = new Timer(channelTimerDelegates[ch], null, channels[ch].timerDelay, channels[ch].timerPeriod);
            }
            _WriteBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(_WriteBuffer);
        }

        public void ChannelCommandHandler(uint lsCmd, uint value, DateTime time)
        {
            int channel = (int)(lsCmd & Constants.G_CHANNEL_ADR_MASK);
            int mode = ((int)lsCmd >> Constants.G_MAX_ADDRESS);
            Debug.Print("Channel " + channel + ", Mode " + mode + ", Value " + value);
            switch(mode)
            {
                case (int)Constants.MODE.FUNC:
                {
                    SetChannelFunction(channel, (int)value);
                    break;
                }
                case (int)Constants.MODE.CMD_GET_VAL:
                {
                    GetChannelValue(channel);
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
                    RestartChannelTimer(channel);
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

        public void GetChannelValue(int channel)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    uint s_command = 0;
                    uint s_address = (uint)(0x1 << i);
                    uint curValue = (uint)channels[i].Value;
                    s_command |= ((uint)Constants.CLASS.CC_CMD << 28);
                    s_command |= ((uint)Constants.MODE.CMD_GET_VAL << 24);
                    s_command |= ((uint)s_address << 8);
                    s_command |= curValue;
                    uint s_crc = (uint)(((uint)Constants.CLASS.CC_CMD << 4) | (uint)Constants.MODE.CMD_GET_VAL) + curValue;
                    SendChannelData(s_command, s_crc, DateTime.Now);
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

        public void SetChannelValue(int channel, int value)
        {
            for (int i = 0; i < _numberOfChannels; i++)
            {
                if (((channel >> i) & 0x1) == 1)
                {
                    channels[i].Value = value;
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
                    //channelTimers[i].Dispose();
                    //channelTimers[i] = new Timer(channelTimerDelegates[i], null, channels[i].timerDelay, channels[i].timerPeriod);
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
