using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bt_light_framework
{
    class Frame
    {
        public byte[] ContentByte;
        public const int Length = Frame.Length;
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
