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
        public OutputPort _atPin;
        public OutputPort _resetPin;
        public event NativeEventHandler CommandReceived = null;
        public static byte[] _readBuffer = new Byte[bufferMax];

        const int bufferMax = 512;
        const int numOfBuffer = 2;

        private static int _writeIndex = 0;
        private static byte[] _writeBuffer;

        public BTModule(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            _atPin = new OutputPort(Constants.ATPIN, false);
            _resetPin = new OutputPort(Constants.RESET, true);
            Open();
            this.DataReceived += new SerialDataReceivedEventHandler(receiveBT);
        }

        public void Acknowledge()
        {
            _writeBuffer = Encoding.UTF8.GetBytes("ACK\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        public void Reset()
        {
            _resetPin.Write(false);
            Thread.Sleep(10);
            _resetPin.Write(true);
        }

        public void send2BT(string command)
        {
            Debug.Print("OUT: Stringcommand: " + command);
            _writeBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        public void send2BT(int command)
        {
            _writeBuffer = new byte[Constants.C_LENGTH];
            Debug.Print("OUT: Integer: " + command);
            // max length of int is 4 byte :/
            for (int i = 0; i < 4 ; ++i)
            {
                byte temp = (byte)((command >> i * 8) & 0xFF);
                _writeBuffer[Constants.C_LENGTH - i - 3] = temp;
            }
            _writeBuffer[Constants.C_LENGTH - 2] = 0xD;
            _writeBuffer[Constants.C_LENGTH - 1] = 0xA;
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        public void send2BT(uint command)
        {
            _writeBuffer = new byte[Constants.C_LENGTH];
            Debug.Print("OUT: Integer: " + command);
            // max length of int is 4 byte :/
            for (int i = 0; i < 4; ++i)
            {
                byte temp = (byte)((command >> i * 8) & 0xFF);
                _writeBuffer[Constants.C_LENGTH - i - 3] = temp;
            }
            _writeBuffer[Constants.C_LENGTH - 2] = 0xD;
            _writeBuffer[Constants.C_LENGTH - 1] = 0xA;
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }


        public void send2BT(byte[] command)
        {
            byte[] s_command = new byte[command.Length + 2];
            Array.Copy(command, s_command, command.Length);
            s_command[command.Length] = 0xD;
            s_command[command.Length + 1] = 0xA;
            Debug.Print("OUT: Address: " + (command[1] << 8 | command[2]) + " Value: " + command[3]);
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

        // This is the IRQ handler that signals the main routine that a command is there
        private void receiveBT(object sender, SerialDataReceivedEventArgs e)
        {
            int curBufferLength = BytesToRead;
            Debug.Print("buffer length: " + curBufferLength);
            Read(_readBuffer, _writeIndex, curBufferLength);
            _writeIndex += curBufferLength;
            if (_writeIndex >= Constants.C_LENGTH)
            {
                CommandReceived((uint)_writeIndex, 0, DateTime.Now);                
            }            
        }

        // The main routine is interested in the command so we'll pass it over
        public byte[] GetCommand(uint _none)
        {
            byte[] command = new Byte[Constants.C_LENGTH];
            Array.Copy(_readBuffer, command, Constants.C_LENGTH);
            _writeIndex = 0;
            flushBuffer();
            return command;
        }

        public void flushBuffer(bool readBufOnly = false)
        {
            _readBuffer = new byte[bufferMax];
            if (!readBufOnly)
            {
                _writeBuffer = new byte[Constants.C_LENGTH];
                _writeIndex = 0;
            }
        }
    }
}
