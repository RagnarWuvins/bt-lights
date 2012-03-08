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

        const int bufferMax = 2048;
        const int numOfBuffer = 2;
        static int curStartIndex = 0;
        static bool receiveLocked = false;

        private static int bufferIndex = 0;
        private static byte[] writeBuffer = new Byte[bufferMax];
        private static byte[] buffer = new Byte[bufferMax];
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

        public void send2BT(string command)
        {
            Debug.Print("-> " + command);
            writeBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
            Write(writeBuffer, 0, writeBuffer.Length);
        }

        public void send2BT(byte[] command)
        {
            byte[] s_command = new byte[command.Length + 2];
            Array.Copy(command, s_command, command.Length);
            s_command[command.Length] = 0xD;
            s_command[command.Length + 1] = 0xA;
            Debug.Print("-> " + Encoding.UTF8.GetChars(command));
            Write(s_command, 0, s_command.Length);
        }

        public void dump(string[] commands)
        {
            _atPin.Write(true);
            Thread.Sleep(200);
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
            while(BytesToRead > 0)
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
                    if (buffer[i] == '\n' && buffer[i-1] == '\r')
                    {
                        Debug.Assert(_readBuffer[bufferIndex][0] == 0, "Buffer overwrite", "Warning, buffer to write not empty");
                        Array.Copy(buffer, index2Copy, _readBuffer[bufferIndex], 0, i - index2Copy - 1);
                        index2Copy = i + 1;
                        uint saveBuffer = (uint)bufferIndex;
                        bufferIndex++;
                        if (bufferIndex >= numOfBuffer)
                        {
                            bufferIndex = 0;
                        }
                        CommandReceived(saveBuffer, 0, DateTime.Now);
                    }
                }
                Array.Copy(buffer, index2Copy, buffer, 0, curStartIndex - index2Copy);
                // if no command was found, clearing the buffer won't do any good
                if (index2Copy != 0)
                {
                    Array.Clear(buffer, curStartIndex - index2Copy, curStartIndex);
                }
                curStartIndex -= index2Copy;
            }
            receiveLocked = false;
        }

        public byte[] GetCommand(uint bufferIndex)
        {
            byte[] command = new byte[bufferMax];
            Array.Copy(_readBuffer[bufferIndex],command,bufferMax);
            Array.Clear(_readBuffer[bufferIndex], 0, bufferMax);
            return command;
        }
    }
}
