using System;
using Microsoft.SPOT;
using System.Text;

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

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ByteArrayToString(byte[] arr)
        {
            String output = "";
            try
            {
                output = new String(Encoding.UTF8.GetChars(arr));
                if (output != null)
                {
                    output = output.Trim();
                }
            }
            catch (UnknownTypeException e)
            {
                MainProgram.RegisterError(MainProgram.ErrorCodes.CommandAssertionFail);
            }
            return output;
        }

        public static char[] ByteToChar(byte[] bytes)
        {
            char[] chars2 = new char[bytes.Length];
            for (int i = 0; i < chars2.Length; i++)
            {
                chars2[i] = (char)(bytes[i]);
            }
            return chars2;
        }

        public static string HexStr(byte[] p)
        {
            char[] c = new char[p.Length * 2 + 2];
            byte b;
            c[0] = '0'; c[1] = 'x';
            for (int y = 0, x = 2; y < p.Length; ++y, ++x)
            {
                b = ((byte)(p[y] >> 4));
                c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(p[y] & 0xF));
                c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }

        public static string HexChar(byte p)
        {
            char[] c = new char[2];
            byte b;
            b = ((byte)(p >> 4));
            c[0] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            b = ((byte)(p & 0xF));
            c[1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            return new string(c);
        }
    }
}
