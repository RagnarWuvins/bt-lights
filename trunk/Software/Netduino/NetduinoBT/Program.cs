using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Text;

namespace NetduinoBT
{
    public class Program
    {
#if BT_TEST
        static SerialPort serialPort;
        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        static OutputPort resetPin = new OutputPort(Pins.GPIO_PIN_D5, true);
        static OutputPort atPin = new OutputPort(Pins.GPIO_PIN_D4, false);
        static bool tl = false;
#endif
#if LT_TEST
        ELEscudo channel1 = new ELEscudo(Pins.GPIO_PIN_D2, false, 10);
        ELEscudo channel2 = new ELEscudo(Pins.GPIO_PIN_D3, false, 10);
        ELEscudo channel3 = new ELEscudo(Pins.GPIO_PIN_D4, false, 10);
        ELEscudo channel4 = new ELEscudo(Pins.GPIO_PIN_D5, false, 10);
        ELEscudo channel5 = new ELEscudo(Pins.GPIO_PIN_D6, false, 10);
        ELEscudo channel6 = new ELEscudo(Pins.GPIO_PIN_D7, false, 10);
        ELEscudo channel7 = new ELEscudo(Pins.GPIO_PIN_D8, false, 10);
        ELEscudo channel8 = new ELEscudo(Pins.GPIO_PIN_D9, false, 10);
        ELEscudo status = new ELEscudo(Pins.GPIO_PIN_D10, true, 10);
#endif

        public static void Main()
        {  
#if BT_TEST
            testBT();
#endif
#if LT_TEST
            new Program(); 
            Thread.Sleep(-1);
#endif
        }

#if BT_TEST
        public static void testBT()
        {
            InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);
            //Thread.Sleep(-1);

            // write your code here
            serialPort = new SerialPort("COM1", 38400, Parity.None, 8, StopBits.One);
            serialPort.Open();
            // Get the inqury commands
            dump(Constants.attest);

            while (true)
            {
                string readStr = receiveBT();
                if (readStr != "")
                {
                    led.Write(tl);
                    tl = !tl;
                    Debug.Print(readStr);
                    //send2BT(readStr);
                }
                switch (readStr)
                {
                    case "a":
                        send2BT("Bluetooth says hello!\n\r");
                        break;
                    case "n":
                        send2BT("Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!Nyan!\n\r");
                        break;
                    case "t":
                        send2BT("All your base are belong to us!\n\r");
                        break;
                    case "r":
                        send2BT("Resetting, device will be disco....\n\r");
                        Thread.Sleep(100);
                        resetBTModul();
                        break;
                    case "o":
                        send2BT("Resetting and going into inquiry = 0\n\r");
                        Thread.Sleep(100);
                        //resetBTModul();
                        dump(Constants.reset);
                        break;
                    case "i":
                        send2BT("Resetting and going into inquiry = 1\n\r");
                        Thread.Sleep(100);
                        //resetBTModul();
                        dump(Constants.inquiry);
                        break;
                    default:
                        break;
                }
                // flood stopper
                Thread.Sleep(100);
            }
        }//testBT()

        public static void send2BT(string sendString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(sendString);
            serialPort.Write(bytes, 0, bytes.Length);
            //checkOK();
        }//send2BT(string sendString)

        public static void dump(string[] commands)
        {
            atPin.Write(true);
            Thread.Sleep(100);
            for (int i = 0; i < commands.Length; i++)
            {
                if (i == commands.Length - 1)
                {
                    Thread.Sleep(200);
                    led.Write(true);
                    Thread.Sleep(1800);
                    led.Write(false);
                }
                send2BT(commands[i]);
                string readStr = receiveBT();
                Debug.Print(readStr.Trim());
                Thread.Sleep(100);
            }
            atPin.Write(false);
            resetBTModul();
        }//dump(string[] commands)

        public static string receiveBT()
        {
            string output = "";
            long ticks = Utility.GetMachineTime().Ticks;
            long test = Utility.GetMachineTime().Ticks - ticks;
            while ((Utility.GetMachineTime().Ticks - ticks) < 5000)
            {
                int bytesToRead = serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    // get the waiting data
                    byte[] buffer = new byte[bytesToRead];
                    serialPort.Read(buffer, 0, buffer.Length);
                    // print out our received data
                    try
                    {
                        output = new String(System.Text.Encoding.UTF8.GetChars(buffer));
                    }
                    catch
                    {
                        output = "Fehler";
                    }
                }
            }
            return output;
        }//string receiveBT()

        public static void debug(string command)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            send2BT(command);
            string testStr = "null";
            while (testStr != "")
            {
                Thread.Sleep(100);
                testStr = receiveBT();
                Debug.Print(testStr);
            }
        }//debug(string command)

        public static void resetBTModul()
        {
            resetPin.Write(false);
            Thread.Sleep(5);
            resetPin.Write(true);
            Thread.Sleep(2000);
        }//resetBTModul()

        public static void inqBTModul()
        {
            resetPin.Write(false);
            Thread.Sleep(5);
            resetPin.Write(true);
            Thread.Sleep(2000);
            send2BT("\r\n+CONN=90,21,55,59,23,e5\r\n");
            string readStr = receiveBT();
            Debug.Print(readStr);
            send2BT("\r\n+INQ=1\r\n");
            readStr = receiveBT();
            Debug.Print(readStr);
        }//inqBTModul()

        public static int millis()
        {
            return (int)DateTime.Now.Millisecond;
        }//millis()

        public static void checkOK()
        {
            char a, b;
            string test;
            byte[] buffer;

            while (true)
            {
                if (serialPort.BytesToRead > 0)
                {
                    test = receiveBT();
                    /*a = System.Text.Encoding.UTF8.GetChars(buffer)[0];
                    if ('O' == a)
                    {
                        buffer = new byte[serialPort.BytesToRead];
                        serialPort.Read(buffer, 0, buffer.Length);
                        b = System.Text.Encoding.UTF8.GetChars(buffer)[0];
                        if ('K' == b)
                        {
                            break;
                        }
                    }*/
                }
            }

            while (buffer[0] != 0)
            {
                buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);
                a = System.Text.Encoding.UTF8.GetChars(buffer)[0];
                //Wait until all response chars are received
            }
        }//checkOK()

        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            //resetBTModul();
            dump(Constants.reset);
            //inqBTModul();
        }//button_OnInterrupt()
#endif
#if LT_TEST
        public Program()
        {
            int[] testArray = { 1, 2, 3, 5, 10, 15, 20, 25, 30, 35, 40 };
            while (true)
            {
                channel1._debug = false;
                foreach (int t in testArray)
                {
                    channel1.pulse();
                    channel4.pulse();
                    channel1._pulseDuration = t;
                    channel4._pulseDuration = t;
                    Debug.Print("Now at channel 1 with pulsewidth " + t);
                }
                //channel2.pulse(); Debug.Print("Now at channel 2");
                //channel3.pulse(); Debug.Print("Now at channel 3");
                //channel4.pulse(); Debug.Print("Now at channel 4");
                //channel5.pulse(); Debug.Print("Now at channel 5");
                //channel6.pulse(); Debug.Print("Now at channel 6");
            }
        }
#endif
    }
}
