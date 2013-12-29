using System;
using System.Text;

namespace bt_light_framework
{
    public class Frame
    {
        public byte[] ContentByte;
        public const int Length = 4;
        public const int ValueIndex = 3;
        public const int CommandIndex = 0;
        public const int ChannelIndex = 1;

        public uint ContentInteger
        {
            get { return (uint)(ContentByte[0] << 24 | ContentByte[1] << 16 | ContentByte[2] << 8 | ContentByte[3]); }
            set 
            { 
                ContentByte[0] = (byte)((value & 0xFF000000) >> 24);
                ContentByte[1] = (byte)((value & 0x00FF0000) >> 16);
                ContentByte[2] = (byte)((value & 0x0000FF00) >> 8);
                ContentByte[3] = (byte)((value & 0x000000FF));
            }
        }

        public byte Command
        {
            get { return ContentByte[0]; }
            set { ContentByte[0] = value; }
        }

        public byte Value
        {
            get { return (ContentByte[ValueIndex]); }
            set { ContentByte[ValueIndex] = value; }
        }

        public uint Payload
        {
            get { return (ContentInteger & 0x00FFFFFF); }
            set { ContentInteger = value & 0x00FFFFFF; }
        }

        public Frame()
        {
            ContentByte = new byte[Frame.Length];
        }

        public Frame(UInt32 input)
        {
            ContentByte = new byte[Frame.Length];
            ContentInteger = input;
        }
    }
}
