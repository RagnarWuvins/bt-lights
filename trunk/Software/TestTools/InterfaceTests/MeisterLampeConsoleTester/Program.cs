using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using bt_light_framework;
using System.Threading;
using System.Windows.Forms;
using BTLights;

namespace MeisterLampeConsoleTester
{
    class Program
    {
        static SerialPort srl;
        static string[] SerialPorts = SerialPort.GetPortNames();
        static int counter = 0;
        private static byte[] mReadBuffer = new byte[2048];
        private static int mBufferPosition = 0;
        private static bool drawGraph = false;
        private static Plot plot;

        delegate void SerialReceived(Package p);

        static void Main(string[] args)
        {
            string readPort = PrintPorts();

            while (!SerialPorts.Contains(readPort))
            {
                PrintPorts();
            }

            bool tryAgain = true;
            while (tryAgain)
            {
                tryAgain = !OpenPort(readPort);
                if (tryAgain)
                {
                    Console.WriteLine("Couldn't open port. Press ESC to quit any other to start again");

                    if (Console.ReadKey(true).Key != ConsoleKey.Escape)
                    {
                        return;
                    }
                }
            }            

            ChooseFunctions();
            Console.WriteLine("Test done. Press ESC to quit any other to start again");
            while (Console.ReadKey(false).Key != ConsoleKey.Escape)
            {
                ChooseFunctions();
            }
        }

        public static void ShowSingleValues(SerialPort srl)
        {
            drawGraph = true;
            srl.DataReceived += new SerialDataReceivedEventHandler(OnDataReceive);
            
            Package p = new Package(1);
            p.PackageFrames[1].Command = (byte)CommandHandler.Command.ChannelTracer; // Command for getting a value. Enum can't simply be imported

            plot = new Plot(800, 600);

            p.PackageFrames[1].Payload = 0;
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

            srl.DiscardInBuffer();
            Thread.Sleep(100);

            p.PackageFrames[1].Payload = 1;
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);        
        }

        public static void ShowPlot(Package p)
        {            
            
            int[] test = new int[p.PackageHeader.PackageLength - 1];
            int i = 0;
            foreach (Frame f in p.PackageFrames)
            {
                if(f is Header)
                {
                    continue;
                }
                test[i] = (int)f.ContentByte[3];
                i++;
            }
            if (test.Length > 2)
            {
                plot.AddLine(test);
            }
  
        }

        public static void TriggerSingleFunction(SerialPort srl)
        {
            do
            {
                Console.WriteLine("Type a function number. Press ESC to quit and Enter to send");
                string funString = Console.ReadLine();
                if (funString == "")
                {
                    continue;
                }

                Console.WriteLine("Type a function argument. Press ESC to quit and Enter to send");
                string argString = Console.ReadLine();
                if (argString == "")
                {
                    continue;
                }


                Package p = new Package(1);
                uint chanCounter = 0;
                foreach (Frame f in p.PackageFrames)
                {
                    if (f is Header)
                    {
                        continue;
                    }
                    f.Command = Byte.Parse(funString); // Command for getting a value. Enum can't simply be imported
                    f.Payload = UInt32.Parse(argString);
                    chanCounter++;
                }
                srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

                ReadSerial(srl, p);
            } while (Console.ReadKey(false).Key != ConsoleKey.Escape);
        }

        public static void SetChannelMaximum(SerialPort srl)
        {
            Package p = new Package(10);
            uint chanCounter = 0;
            foreach (Frame f in p.PackageFrames)
            {
                if (f is Header)
                {
                    continue;
                }
                f.Command = 0x4; // Command for getting a value. Enum can't simply be imported
                f.ContentByte[1] = (byte)chanCounter;
                f.ContentByte[3] = 0xAF;
                chanCounter++;
            }
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

            ReadSerial(srl, p);
        }

        public static void QueryChannelMaximum(SerialPort srl)
        {
            Package p = new Package(10);
            uint chanCounter = 0;
            foreach (Frame f in p.PackageFrames)
            {
                if (f is Header)
                {
                    continue;
                }
                f.Command = 0x5; // Command for getting a value. Enum can't simply be imported
                f.ContentByte[1] = (byte)chanCounter;
                chanCounter++;
            }
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

            ReadSerial(srl, p);
        }

        public static void QueryChannelValues(SerialPort srl)
        {
            Package p = new Package(10);
            uint chanCounter = 0;
            foreach (Frame f in p.PackageFrames)
            {
                if (f is Header)
                {
                    continue;
                }
                f.Command = 0x3; // Command for getting a value. Enum can't simply be imported
                f.ContentByte[1] = (byte)chanCounter;
                chanCounter++;
            }
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

            ReadSerial(srl, p);
        }

        public static void StressTest(SerialPort srl)
        {
            Package p = new Package(254);
            uint c = 0;
            foreach (Frame f in p.PackageFrames)
            {
                if (f is Header)
                {
                    continue;
                }
                f.Command = 3;
                f.ContentByte[1] = (byte)(c % 10);
                c++;
            }
            srl.Write(p.GetPackageBytes(), 0, p.GetPackageBytes().Length);

            ReadSerial(srl, p);
        }

        public static void OnDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort srl = (SerialPort)sender;
            //lock (mReadBuffer)
            //{
                srl.Read(mReadBuffer, mBufferPosition, 4);
                mBufferPosition += 4;
                if (mReadBuffer[0] < 0x0A && mBufferPosition > 0)
                {
                    Header h = new Header();
                    Array.Copy(mReadBuffer, 0, h.ContentByte, 0, Frame.Length);
                    if (h.PackageLength == 0)
                    {
                        ClearBuffer();
                        return;
                    }
                    if (mBufferPosition >= h.PackageLength * Frame.Length)
                    {
                        byte[] byteContent = new byte[h.PackageLength * Frame.Length];
                        Array.Copy(mReadBuffer, 0,byteContent, 0, h.PackageLength * Frame.Length);
                        Package p = new Package(byteContent);
                        PrintPackage(p);
                        if (drawGraph)
                        {
                            ShowPlot(p);
                        }                        
                        ClearBuffer();
                    }
                }
                else
                {
                    Console.WriteLine("Wrong pattern detected: throwing data away");
                    ClearBuffer();
                }
                if (mBufferPosition >= mReadBuffer.Length)
                {
                    mBufferPosition = 0;
                }
            //}
        }

        public static void ClearBuffer()
        {
            Array.Clear(mReadBuffer, 0, mReadBuffer.Length);
            mBufferPosition = 0;
        }

        public static void ReadSerial(SerialPort srl, Package p)
        {
            bool dataReady = false;
            int i = 0;
            for (i = 0; i < 1000; i++)
            {
                if (srl.BytesToRead > 0)
                {
                    dataReady = true;
                    break;
                }
                Thread.Sleep(1);
            }

            if (!dataReady)
            {
                Console.WriteLine("No data returned in 1000 ms! press key to return");
                Console.ReadLine();
                return;
            }

            Console.Write("first data returned after: ");
            Console.Write(i);
            Console.WriteLine(" ms");

            int retCounter = 0;
            TimeSpan interval = DateTime.Now - DateTime.Now.Date;
            while (srl.BytesToRead > 0)
            {
                if ((srl.BytesToRead < 4))
                {
                    continue;
                }
                Console.Write("Return frame ");
                Console.Write(retCounter++);
                Console.Write(" : ");

                byte[] readBuffer = new byte[4];
                srl.Read(readBuffer, 0, 4);
                
                string hex = BitConverter.ToString(readBuffer).Replace("-", " 0x");
                Console.Write(" 0x");
                Console.Write(hex);
                for (i = 0; i < 1000; i++)
                {
                    if (srl.BytesToRead > 0 || retCounter == p.PackageHeader.PackageLength - 1)
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }
                Console.Write(" Time: ");
                Console.WriteLine(i);
            }
        }

        public static void PrintPackage(Package p)
        {
            foreach (Frame f in p.PackageFrames)
            {
                if (f is Header)
                {
                    Console.Write("Header: ");
                }
                else
                {
                    Console.Write("Frame: ");                    
                }
                string hex = BitConverter.ToString(f.ContentByte).Replace("-", " 0x");
                Console.Write(" 0x");
                Console.WriteLine(hex);
            }
        }

        // Inner functions

        public static void ChooseFunctions()
        {
            string[] functions = { "Stresstest", "Query channel values", "Set channel maximum", "Query channel maximum", "Trigger function manually", "Show channel values" };
            Console.WriteLine("\nAvailable tests:\n");
            counter = 0;
            foreach (string function in functions)
            {
                Console.Write(counter++);
                Console.Write("  ");
                Console.WriteLine(function);
            }
            Console.Write("Choose function: ");
            string repString = Console.ReadLine();
            if (repString == "")
            {
                return;
            }
            switch (Int16.Parse(repString))
            {
                case 0:
                    StressTest(srl);
                    break;
                case 1:
                    QueryChannelValues(srl);
                    break;
                case 2:
                    SetChannelMaximum(srl);
                    break;
                case 3:
                    QueryChannelMaximum(srl);
                    break;
                case 4:
                    TriggerSingleFunction(srl);
                    break;
                case 5:
                    ShowSingleValues(srl);
                    break;
                default:
                    return;
            }
        }

        public static string PrintPorts()
        {
            Console.WriteLine("Available SerialPorts:\n");
            counter = 0;
            foreach (string port in SerialPorts)
            {
                Console.Write(counter++);
                Console.Write(" : ");
                Console.WriteLine(port);
            }
            Console.Write("\nChoose port: ");
            string str = Console.ReadLine();
            if (str == "")
            {
                return str;
            }
            if (Int16.Parse(str) + 1 > SerialPorts.Length)
            {
                return "";
            }
            string readPort = SerialPorts[Int16.Parse(str)];
            return readPort;
        }

        public static bool OpenPort(string readPort)
        {
            srl = new SerialPort(readPort, 115200);
            try
            {
                srl.Open();
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
                Console.ReadLine();
                return false;
            }
        }
    }
}
