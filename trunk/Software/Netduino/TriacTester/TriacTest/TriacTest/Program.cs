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
        static OutputPort EscudoState = new OutputPort(Pins.GPIO_PIN_D10, true);

        public static OutputPort[] AllTriacs = { TriacA, TriacB, TriacC };
        public static int TriacCounter = 0;

        public static PWM PwmA = new PWM(Pins.GPIO_PIN_D5);
        public static PWM PwmB = new PWM(Pins.GPIO_PIN_D6);
        public static PWM PwmC = new PWM(Pins.GPIO_PIN_D9);
       

        // set PWM initial periods
        static public uint period = 16 * 1000;    // 10 ms
        static public uint duration = 1 * 1000;  // 1 ms      
        static public uint dimValue = 100;
        static public bool direction = false;

        public static void Main()
        {
            // write your code here
            InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

             // Timer for dimming
            TimerCallback WriteDelegate = new TimerCallback(Dimmer);
            Timer Write_ValueTimer = new Timer(WriteDelegate, null, 0, 1);

            PwmA.SetPulse(period, duration);
            PwmB.SetPulse(period, duration);
            PwmC.SetPulse(period, duration);

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
            duration = 1000;
            dimValue = 100 * ((uint)TriacCounter + 1);
        }

        static void Dimmer(object state)
        {
            Debug.Print("Timer wird ausgeführt " + duration);

            if (direction)
            {
                 duration += dimValue;
            }
            else
            {
                duration -= dimValue;
            }

            if (duration <= 0 || duration >= period)
            {
                direction = !direction;
            }

            PwmA.SetPulse(period, duration);
            PwmB.SetPulse(period, duration);
            PwmC.SetPulse(period, duration);
        }
    }
}

