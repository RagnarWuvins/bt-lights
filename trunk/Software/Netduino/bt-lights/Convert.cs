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
            catch(UnknownTypeException e)
            {
                Program.THROW_ERROR(Constants.FW_ERRORS.CMD_ASSERT_FAIL);
            }            
            return output;
        }
    }
}
