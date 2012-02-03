using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace TriacMultiplexed
{
    public class Program
    {
        public static OutputPort S0, S1, S2, EN, LED;
        public static int brightness = 0, testBr = 0;

        public static void Main()
        {
            // write your code here
            

            S0 = new OutputPort(Pins.GPIO_PIN_D2, false);
            S1 = new OutputPort(Pins.GPIO_PIN_D3, false);
            S2 = new OutputPort(Pins.GPIO_PIN_D4, false);
            EN = new OutputPort(Pins.GPIO_PIN_D13, true);
            LED = new OutputPort(Pins.ONBOARD_LED, true);
            InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

            while (true)
            {
                
                for (int channel = 0; channel < 8; channel++)
                {
                    if (channel == 0)
                    {
                        if (testBr <= brightness)
                        {
                            testBr++;
                            continue;
                        }
                        testBr = 0;                        
                    }
                    channelOn(channel);
                }
            }
        }

        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (brightness == 10)
            {
                brightness = 0;
            }
            else
            {
                brightness++;
            }
            //Debug.Print("Brightness is now at: " + brightness);
        }

        public static void channelOn(int channel)
        {
            EN.Write(true);
            S0.Write(byte2bool(channel, 0));
            S1.Write(byte2bool(channel, 1));
            S2.Write(byte2bool(channel, 2));
            EN.Write(false);
        }

        public static bool byte2bool(int byetIn, int position)
        {
            return (byetIn & (int)System.Math.Pow(2, position)) >> position == 1;
        }
    }
}
