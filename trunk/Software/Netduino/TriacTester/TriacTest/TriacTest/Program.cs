using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Netduino.TestInterrupt
{
    public class Program
    {
        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        static OutputPort TriacA = new OutputPort(Pins.GPIO_PIN_D2, false);
        static OutputPort TriacB = new OutputPort(Pins.GPIO_PIN_D3, false);
        static OutputPort TriacC = new OutputPort(Pins.GPIO_PIN_D4, false);
        static OutputPort TriacD = new OutputPort(Pins.GPIO_PIN_D5, false);
        static OutputPort TriacE = new OutputPort(Pins.GPIO_PIN_D6, false);
        static OutputPort TriacF = new OutputPort(Pins.GPIO_PIN_D7, false);
        static OutputPort TriacG = new OutputPort(Pins.GPIO_PIN_D8, false);
        static OutputPort TriacH = new OutputPort(Pins.GPIO_PIN_D9, false);
        static OutputPort EscudoState = new OutputPort(Pins.GPIO_PIN_D10, true);

        public static OutputPort[] AllTriacs = { TriacA, TriacB, TriacC, TriacD, TriacE, TriacF, TriacG, TriacH };
        public static int TriacCounter = 0;


        public static void Main()
        {
            // write your code here
            InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

            Thread.Sleep(-1);
        }

        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            foreach (OutputPort Triac in AllTriacs)
            {
                Triac.Write(false);
            }
            AllTriacs[TriacCounter].Write(true); Debug.Print("Setting Triac: " + (TriacCounter + 1));
            if (TriacCounter == AllTriacs.Length - 1)
            {
                TriacCounter = 0; 
            }
            else
            {
                TriacCounter++;
            }
        }
    }
}

