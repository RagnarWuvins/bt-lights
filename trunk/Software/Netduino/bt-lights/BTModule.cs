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

        const int bufferMax = 32;
        const int numOfBuffer = 2;

        private static int _writeIndex = 0, _readIndex = 0;
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
            //Debug.Print("RI:" + _readIndex + " | WI: " + _writeIndex);
            byte[] tempBuffer = new Byte[curBufferLength];
            if (_writeIndex + curBufferLength > _readBuffer.Length)
            {
                // do the wrap around
                Read(tempBuffer, 0, curBufferLength);
                int splitPoint = _readBuffer.Length - _writeIndex;
                Array.Copy(tempBuffer, 0, _readBuffer, _writeIndex, splitPoint);
                Array.Copy(tempBuffer, splitPoint, _readBuffer, 0, curBufferLength - splitPoint);
                _writeIndex = curBufferLength - splitPoint;
            }
            else
            {
                Read(_readBuffer, _writeIndex, curBufferLength);
                _writeIndex += curBufferLength;
            }            
            startCommandReceive();         
        }

        // The main routine is interested in the command so we'll pass it over
        public byte[] GetCommand(uint _dataInBufferLength, int newReadIndex)
        {
            byte[] command = new Byte[_dataInBufferLength];
            if (_readIndex + (int)_dataInBufferLength > _readBuffer.Length)
            {
                int splitLength = _readBuffer.Length - _readIndex;
                Array.Copy(_readBuffer, _readIndex, command, 0, splitLength);
                Array.Copy(_readBuffer, 0, command, splitLength, newReadIndex);
            }
            else
            {
                Array.Copy(_readBuffer, _readIndex, command, 0, (int)_dataInBufferLength);
            }
            _readIndex = newReadIndex;
            return command;
        }

        public void flushBuffer(bool readBufOnly = false)
        {
            _readBuffer = new byte[bufferMax];
            _readIndex = 0;
            if (!readBufOnly)
            {
                _writeBuffer = new byte[Constants.C_LENGTH];
                _writeIndex = 0;
            }
        }

        public void startCommandReceive()
        {
            Thread getCommand = new Thread(CommandReceivedTest);
            getCommand.Start();
        }

        public void CommandReceivedTest()
        {
            // lock to prevent overwrite of the indices
            int readIndex = 0, writeIndex = 0;
            lock(new object())
            {
                readIndex = _readIndex;
                writeIndex = _writeIndex;
            }
            int dataInBufferLength = 0;
            if (writeIndex < readIndex)
            {
                dataInBufferLength = (_readBuffer.Length - readIndex) + writeIndex;
                for (int index = 0; index < writeIndex; index++)
                {
                    if (_readBuffer[index] == 0x0A)
                    {
                        CommandReceived((uint)dataInBufferLength, (uint)index + 1, DateTime.Now);
                        break;
                    }
                }
            }
            else
            {
                dataInBufferLength = writeIndex - readIndex;
                for (int index = readIndex; index < writeIndex; index++)
                {
                    if (_readBuffer[index] == 0x0A)
                    {
                        CommandReceived((uint)dataInBufferLength, (uint)index + 1, DateTime.Now);
                        break;
                    }
                }
            }                  
        }
    }
}
