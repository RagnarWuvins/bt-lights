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
        public event NativeEventHandler CommandReceived, BufferOverflow;
        public static byte[] _readBuffer = new Byte[bufferMax];

        const int bufferMax = 512;
        const int numOfBuffer = 2;
        static int curStartIndex = 0;
        static bool receiveLocked = false;
        
        private static int _readIndex = 0;
        private static int _writeIndex = 0;
        private static byte[] _writeBuffer;

        public BTModule(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Cpu.Pin atPin)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            _atPin = new OutputPort(atPin, false);
            Open();
            this.DataReceived += new SerialDataReceivedEventHandler(receiveBT);
        }

        public void Acknowledge()
        {
            _writeBuffer = Encoding.UTF8.GetBytes("ACK\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        public void send2BT(string command)
        {
            Debug.Print("-> " + command);
            _writeBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
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
            int curBufferLength = BytesToRead;
            Read(_readBuffer, _writeIndex, curBufferLength);
            _writeIndex += curBufferLength;
            if (_writeIndex >= Constants.C_LENGTH)
            {
                CommandReceived(0, 0, DateTime.Now);
            }
        }

        public byte[] GetCommand(uint _none)
        {
            byte[] command = new Byte[Constants.C_LENGTH];
            Array.Copy(_readBuffer, command, Constants.C_LENGTH);
            //Array.Clear(_readBuffer[bufferIndex], 0, bufferMax);
            _writeIndex = 0;
            return command;
        }
    }
}
