using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Text;
using System.Threading;
using SecretLabs.NETMF.Hardware.Netduino;

namespace BTLights
{
    public class BTModule : SerialPort
    {
        private OutputPort _atPin;
        public string commandBuffer = "";
        public event NativeEventHandler CommandReceived;

        const int bufferMax = 1024;
        const int numOfBuffer = 4;
        static int buffersInUse = 0;
        static int curStartIndex = 0;
        static bool receiveLocked = false;

        private int bufferIndex = 0;
        private byte[] writeBuffer = new Byte[bufferMax];
        private byte[] buffer = new Byte[bufferMax];
        private static byte[][] _readBuffer = new Byte[numOfBuffer][];

        public BTModule(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Cpu.Pin atPin)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            _atPin = new OutputPort(atPin, false);
            for (int i = 0; i < numOfBuffer; i++)
            {
                _readBuffer[i] = new Byte[bufferMax];
            }
            Open();
            this.DataReceived += new SerialDataReceivedEventHandler(receiveBT);
        }

        public void Acknowledge()
        {
            writeBuffer = Encoding.UTF8.GetBytes("ACK\r\n");
            Write(writeBuffer, 0, writeBuffer.Length);
        }

        public void send2BT(string sendString)
        {
            writeBuffer = Encoding.UTF8.GetBytes(sendString + "\r\n");
            Write(writeBuffer, 0, writeBuffer.Length);
        }

        public void dump(string[] commands)
        {
            _atPin.Write(true);
            Thread.Sleep(100);
            foreach (string command in commands)
            {
                send2BT(command);
                Thread.Sleep(100);
            }
            _atPin.Write(false);
        }


        private void receiveBT(object sender, SerialDataReceivedEventArgs e)
        {
            if (receiveLocked)
            {
                return;
            }
            while (BytesToRead > 0)
            {
                receiveLocked = true;
                int curBufferLength = BytesToRead;
                byte[] temp = new byte[curBufferLength];
                Read(temp, 0, curBufferLength);
                Array.Copy(temp, 0, buffer, curStartIndex, curBufferLength);
                curStartIndex = curBufferLength + curStartIndex;
                int index2Copy = 0;
                for (int i = 0; i < curStartIndex; i++)
                {
                    if (buffer[i] == '\n')
                    {
                        Array.Copy(buffer, index2Copy, _readBuffer[bufferIndex], 0, i - index2Copy - 1);
                        index2Copy = i + 1;
                        uint saveBuffer = (uint)bufferIndex;
                        bufferIndex++;
                        buffersInUse++;
                        //Debug.Print("Buffers in use: " + buffersInUse + ", Current temporary buffer length: " + curStartIndex);
                        if (bufferIndex >= numOfBuffer)
                        {
                            bufferIndex = 0;
                        }
                        CommandReceived(saveBuffer, 0, DateTime.Now);
                    }
                }
                Array.Copy(buffer, index2Copy, buffer, 0, curStartIndex - index2Copy);
                if (index2Copy == 0 && curStartIndex > 0)
                {
                    // in case of doubt, clear whole buffer!
                    //Array.Clear(buffer, 0, bufferMax);
                    Debug.Print("WARNING, BUFFER INTERRUPT!");
                }
                else
                {
                    Array.Clear(buffer, curStartIndex - index2Copy, curStartIndex);
                }
                curStartIndex -= index2Copy;                
            }
            receiveLocked = false;
        }

        public string GetCommand(uint bufferIndex)
        {
            string command = new string(Encoding.UTF8.GetChars(_readBuffer[bufferIndex]));
            Array.Clear(_readBuffer[bufferIndex], 0, bufferMax);
            buffersInUse--;
            //Debug.Print("Buffers in use: " + buffersInUse + ", Current temporary buffer length: " + curStartIndex);
            return command;
        }
    }
}
