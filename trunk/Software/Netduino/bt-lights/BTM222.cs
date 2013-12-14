using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Text;
using System.Threading;
using SecretLabs.NETMF.Hardware.Netduino;
using bt_light_framework;

namespace BTLights
{
    public class BTM222 : SerialPort
    {
        private OutputPort mATPin;
        private OutputPort mResetPin;
        private const int bufferMax = 2048;
        private static byte[] mReadBuffer = new Byte[bufferMax];
        private static byte[] mwriteBuffer;
        private int mBufferPosition;

        /// <summary>
        /// Connection status of the module
        /// </summary>
        public STATES State = STATES.DISCONNECTED;

        /// <summary>
        /// Gives the maximum  time that the BT module stays connected before resetting it.
        /// This prevents hangs in connection when the controlling device crashes without disconnecting.
        /// </summary>
        public const int ConnectionTimeout = 300000;    // time before bluetooth module resets (5 minutes)

        /// <summary>Event that is called when a command is received (Termination is \r\n or 0xA, 0xD)</summary>
        public event BTEvent CommandReceived;

        public enum STATES
        {
            CONNECTED,      // connected status
            DISCONNECTED,   // disconnected
        }


        public BTM222(string portName, int baudRate, Cpu.Pin ATPin, Cpu.Pin ResetPin)
            : base(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            mATPin = new OutputPort(ATPin, false);
            mResetPin = new OutputPort(Constants.RESET, true);
            this.Open();
            this.DataReceived += new SerialDataReceivedEventHandler(OnReceiveData);
            this.Init();
        }

        public void Init()
        {
            mATPin.Write(true);
            Thread.Sleep(200);
            string[] commands ={ "ATN=Meister Lampe", "ATL5", "ATE0" };
            foreach (string command in commands)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(command + "\r");
                this.Write(buffer, 0, buffer.Length);
                Thread.Sleep(100);
            }
            mATPin.Write(false);
        }


        private void OnReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            State = STATES.CONNECTED;
            lock (mReadBuffer)
            {
                while (this.BytesToRead > 0)
                {
                    this.Read(mReadBuffer, mBufferPosition, 1);
                    mBufferPosition++;
                }
                if (mReadBuffer[0] < 0x0A && mBufferPosition > 0)
                {
                    Header h = new Header();
                    h.ContentByte = Convert.SubArray(mReadBuffer, 0, Frame.Length);
                    if (mBufferPosition >= h.PackageLength * Frame.Length)
                    {
                        Package p = new Package(Convert.SubArray(mReadBuffer, 0, h.PackageLength * Frame.Length));
                        BTEventArgs evt = new BTEventArgs();
                        evt.Data = p;
                        CommandReceived(this, evt);
                        ClearBuffer();
                    }
                }
                else
                {
                    MainProgram.RegisterError(MainProgram.ErrorCodes.InterpretationError);
                    ClearBuffer();
                }
                if (mBufferPosition > BTM222.bufferMax)
                {
                    mBufferPosition = 0;
                }
            }
        }

        public void ClearBuffer()
        {
            Array.Clear(mReadBuffer, 0, mReadBuffer.Length);
            mBufferPosition = 0;
        }

        /// <summary>
        /// Hard reset of the bluetooth module
        /// </summary>
        public void Reset()
        {
            mResetPin.Write(false);
            Thread.Sleep(10);
            mResetPin.Write(true);
            State = STATES.DISCONNECTED;
        }
    }
}
