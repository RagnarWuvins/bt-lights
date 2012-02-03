using System;
using Microsoft.SPOT;

namespace SPITest
{
    class Constants
    {
        private static byte READ        = 0x80; // Read modifier
        private static byte WRITE       = 0x00; // write modifier
        public static byte OUTPUT_P0    = 0x00;
        public static byte OUTPUT_P1    = 0x01;
        public static byte OUTPUT_P2    = 0x02;
        public static byte OUTPUT_P3    = 0x03;
        public static byte OUTPUT_P4    = 0x04;
        public static byte OUTPUT_P5    = 0x05;
        public static byte OUTPUT_P6    = 0x06;
        public static byte OUTPUT_P7    = 0x07;
        public static byte OUTPUT_P8    = 0x08;
        public static byte OUTPUT_P9    = 0x09;
        public static byte OUTPUT_ALL   = 0x0A; // All outputs
        public static byte RAMP_DOWN    = 0x11;
        public static byte RAMP_UP      = 0x12;

        public static byte CONF_RUN     = 0x01; // run current config
        public static byte CONF_CSRUN   = 0x02; // run current config on CS
        public static byte CONF_STAGGER = 0x20; // Ports out of phase
        public static byte CONF_PHASE   = 0x00; // Ports in phase

        public static byte CONFIGURATION = 0x10;

        public static byte Read(byte Parameter)
        {
            return (byte)(Parameter | Constants.READ);
        }

        public static byte Write(byte Parameter)
        {
            return (byte)(Parameter | Constants.WRITE);
        }

        public static byte[] RampDown(int hold, int fade)
        {
            return new byte[] {Constants.RAMP_DOWN, (byte)(hold << 0x03 | fade)};
        }

    }
}
