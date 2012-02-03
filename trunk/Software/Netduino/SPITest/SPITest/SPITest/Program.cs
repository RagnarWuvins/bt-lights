using System;
using System.Globalization;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace SPITest
{
    public class Program
    {
        public static SPI.Configuration _MAX;
        public static SPI _SPIBus;
        public static byte[] WriteBuffer;
        public static byte[] ReadBuffer;
        public static int dimmingP0 = 0x00, dimmingP1 = 0x00;
        public static bool directionP0 = true, directionP1 = true;

        public static void Main()
        {
            _MAX = new SPI.Configuration(
            Pins.GPIO_PIN_D10,  // CS-pin
            false,              // CS-pin active state
            0,                  // The setup time for the SS port
            0,                  // The hold time for the SS port
            false,               // The idle state of the clock
            true,              // The sampling clock edge
            10000,               // The SPI clock rate in KHz
            SPI_Devices.SPI1    // The used SPI bus (refers to a MOSI MISO and SCLK pinset)
            );

            WriteBuffer = new byte[2];
            ReadBuffer = new byte[2];
            _SPIBus = new SPI(_MAX);

            //InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            //button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

            LightString channelP9 = new LightString(_SPIBus, 9, 0);
            LightString channelP8 = new LightString(_SPIBus, 8, 300);
            LightString channelP7 = new LightString(_SPIBus, 7, 600);
            LightString channelP6 = new LightString(_SPIBus, 6, 900);
            LightString channelP5 = new LightString(_SPIBus, 5, 1200);
            LightString channelP4 = new LightString(_SPIBus, 4, 1500);
            LightString channelP3 = new LightString(_SPIBus, 3, 1800);
            LightString channelP2 = new LightString(_SPIBus, 2, 2100);

            TimerCallback TimerDelegate_P9 = new TimerCallback(channelP9.Fade);
            Timer Timer_P9 = new Timer(TimerDelegate_P9, null, channelP9.timerDelay, channelP9.timerPeriod);
            
            TimerCallback TimerDelegate_P8 = new TimerCallback(channelP8.Fade);
            Timer Timer_P8 = new Timer(TimerDelegate_P8, null, channelP8.timerDelay, channelP8.timerPeriod);

            TimerCallback TimerDelegate_P7 = new TimerCallback(channelP7.Fade);
            Timer Timer_P7 = new Timer(TimerDelegate_P7, null, channelP7.timerDelay, channelP7.timerPeriod);

            TimerCallback TimerDelegate_P6 = new TimerCallback(channelP6.Fade);
            Timer Timer_P6 = new Timer(TimerDelegate_P6, null, channelP6.timerDelay, channelP6.timerPeriod);

            TimerCallback TimerDelegate_P5 = new TimerCallback(channelP5.Fade);
            Timer Timer_P5 = new Timer(TimerDelegate_P5, null, channelP5.timerDelay, channelP5.timerPeriod);

            TimerCallback TimerDelegate_P4 = new TimerCallback(channelP4.Fade);
            Timer Timer_P4 = new Timer(TimerDelegate_P4, null, channelP4.timerDelay, channelP4.timerPeriod);

            TimerCallback TimerDelegate_P3 = new TimerCallback(channelP3.Fade);
            Timer Timer_P3 = new Timer(TimerDelegate_P3, null, channelP3.timerDelay, channelP3.timerPeriod);

            TimerCallback TimerDelegate_P2 = new TimerCallback(channelP2.Fade);
            Timer Timer_P2 = new Timer(TimerDelegate_P2, null, channelP2.timerDelay, channelP2.timerPeriod);
            
            //TimerCallback TimerDelegate_P1 = new TimerCallback(Fade);
            //Timer Timer_P1 = new Timer(TimerDelegate_P1, Constants.OUTPUT_P8, 0, 200);

            WriteBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(WriteBuffer);

            Thread.Sleep(-1);
        }
       
        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print(">>Before:\n---------------------------------");
            WriteBuffer = new byte[] { Constants.Read(Constants.OUTPUT_ALL) };
            ReadBuffer = new byte[10];
            _SPIBus.WriteRead(WriteBuffer, ReadBuffer);
            SPIRead(ReadBuffer[0], "Reading Configuration Upper: ");
            SPIRead(ReadBuffer[1], "Reading Configuration Lower: ");

            Debug.Print("Setting the Port & Run command");
            WriteBuffer = new byte[] { Constants.Write(Constants.CONFIGURATION), Constants.CONF_RUN };
            _SPIBus.Write(WriteBuffer);
            for (int iFade = 0; iFade <= 0xFF; iFade++)
            {
                _SPIBus.Write(WriteBuffer);
                WriteBuffer = new byte[] { Constants.Write(Constants.OUTPUT_ALL), (byte)iFade };
                Thread.Sleep(10);
            }         

            Debug.Print(">>After:\n---------------------------------");
            WriteBuffer = new byte[] { Constants.Read(Constants.CONFIGURATION) };
            ReadBuffer = new byte[2];
            _SPIBus.WriteRead(WriteBuffer, ReadBuffer);
            SPIRead(ReadBuffer[0], "Reading Configuration Upper: ");
            SPIRead(ReadBuffer[1], "Reading Configuration Lower: ");

            dimmingP0 += 0x20;
            if (dimmingP0 > 0xFF)
            {
                dimmingP0 = 0x00;
            }

            ReadBuffer = new byte[2];
        }

        static void SPIRead(byte input, string info = "Output: ")
        {
            string output = "";
            for (int i = 0; i < 8; i++)
            {
                if((input & 0x80) != 0)
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                input *= 2;
            }
            Debug.Print(info + "|" + output + "|");
        }
    }
}
