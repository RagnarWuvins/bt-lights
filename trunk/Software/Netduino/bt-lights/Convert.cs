using System;
using Microsoft.SPOT;

namespace BTLights
{
    public static class Convert
    {
        public static byte[] ToByteArray(uint value)
        {
            return ToByteArray((int)value);
        }

        public static byte[] ToByteArray(int value)
        {
            byte[] output = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                output[i] = (byte)(value & 0xFF);
                value >>= 8;
            }
            return output;
        }
    }
}
