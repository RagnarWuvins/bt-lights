using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Text;
using System.Threading;
using SecretLabs.NETMF.Hardware.Netduino;

namespace BTLights
{
    /// <summary>
    /// This is an extended class of a serialport object. It contains buffer control and new line parsing
    /// </summary>
    public class BTModule : SerialPort
    {
        private OutputPort mATPin;
        private OutputPort mResetPin;
        private static byte[] mReadBuffer = new Byte[bufferMax];
        private static int _writeIndex = 0, _readIndex = 0;
        private static byte[] _writeBuffer;
        private static int mCommandExtractCounter = 0; // must be 0 at all times
        /// <summary>Event that is called when a command is received (Termination is \r\n or 0xA, 0xD)</summary>
        public event BTEvent CommandReceived;
        //public event NativeEventHandler CommandReceived = null;
        
        


        const int bufferMax = 256;
        const int numOfBuffer = 2;        

        /// <summary>
        /// Constructor for the Bluetooth Module class
        /// </summary>
        /// <param name="portName">Serial port name</param>
        /// <param name="baudRate">Baud rate</param>
        /// <param name="parity">Parity</param>
        /// <param name="dataBits">Number of data bits</param>
        /// <param name="stopBits">Stop bit</param>
        public BTModule(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            mATPin = new OutputPort(Constants.ATPIN, false);
            mResetPin = new OutputPort(Constants.RESET, true);
            Open();
            this.DataReceived += new SerialDataReceivedEventHandler(receiveBT);
        }

        /// <summary>
        /// Send ACK\r\n over bluetooth
        /// </summary>
        public void Acknowledge()
        {
            _writeBuffer = Encoding.UTF8.GetBytes("ACK\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        /// <summary>
        /// Hard reset of the bluetooth module
        /// </summary>
        public void Reset()
        {
            mResetPin.Write(false);
            Thread.Sleep(10);
            mResetPin.Write(true);
        }

        /// <summary>
        /// Send a string over the module
        /// </summary>
        /// <param name="command">String to send (\r\n will be added automatically)</param>
        public void send2BT(string command)
        {
            Debug.Print("send2BT: OUT: Stringcommand: " + command);
            _writeBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        /// <summary>
        /// Send a long to the module
        /// </summary>
        /// <param name="command">Command as integer</param>
        public void send2BT(long command)
        {
            _writeBuffer = new byte[Constants.C_LENGTH];
            Debug.Print("send2BT: OUT: Integer: " + command);
            // max length of long is 8
            for (int i = 0; i < Constants.C_LENGTH - 2; ++i)
            {
                byte temp = (byte)((command >> i * 8) & 0xFF);
                _writeBuffer[Constants.C_LENGTH - i - 3] = temp;
            }
            _writeBuffer[Constants.C_LENGTH - 2] = 0xD;
            _writeBuffer[Constants.C_LENGTH - 1] = 0xA;
            Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        /// <summary>
        /// Send a integer over the module (int is 4 byte long!!!)
        /// </summary>
        /// <param name="command">Command as integer</param>
        public void send2BT(int command)
        {
            _writeBuffer = new byte[Constants.C_LENGTH];
            Debug.Print("send2BT: OUT: Integer: " + command);
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

        /// <summary>
        /// Send a unsigned integer over the module (uint is 4 byte long!!!)
        /// </summary>
        /// <param name="command">Command as unsigned integer</param>
        public void send2BT(uint command)
        {
            _writeBuffer = new byte[Constants.C_LENGTH];
            Debug.Print("send2BT: OUT: Integer: " + command);
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

        /// <summary>
        /// Send a byte array over the module
        /// </summary>
        /// <param name="command">Byte array that has to be send</param>
        public void send2BT(byte[] command)
        {
            byte[] s_command = new byte[command.Length + 2];
            Array.Copy(command, s_command, command.Length);
            s_command[command.Length] = 0xD;
            s_command[command.Length + 1] = 0xA;
            Debug.Print("send2BT: OUT: Address: " + (command[1] << 8 | command[2]) + " Value: " + command[3]);
            Write(s_command, 0, s_command.Length);
        }

        /// <summary>
        /// Send a complete array of string to the module
        /// </summary>
        /// <param name="commands">String array of commands</param>
        public void dump(string[] commands)
        {
            mATPin.Write(true);
            Thread.Sleep(200);
            foreach (string command in commands)
            {
                send2BT(command);
                Thread.Sleep(100);
            }
            mATPin.Write(false);
        }

        // This is the IRQ handler that signals the main routine that a command is there
        private void receiveBT(object sender, SerialDataReceivedEventArgs e)
        {
            int curBufferLength = BytesToRead;
            //Debug.Print("RI:" + _readIndex + " | WI: " + _writeIndex);
            byte[] tempBuffer = new Byte[curBufferLength];
            if (_writeIndex + curBufferLength > mReadBuffer.Length)
            {
                // do the wrap around
                Read(tempBuffer, 0, curBufferLength);
                int splitPoint = mReadBuffer.Length - _writeIndex;
                try
                {
                    Array.Copy(tempBuffer, 0, mReadBuffer, _writeIndex, splitPoint);
                    Array.Copy(tempBuffer, splitPoint, mReadBuffer, 0, curBufferLength - splitPoint);
                }
                catch (Exception)
                {                    
                    String errorString = "\nTempbuffer length: " + tempBuffer.Length +
                                         "\n_writeIndex: " + _writeIndex + 
                                         "\nsplitPoint: " + splitPoint +
                                         "\ncurBufferLength: " + curBufferLength;
                    Program.THROW_ERROR(Constants.FW_ERRORS.BUFFER_INDEX_OUT_RANGE, errorString);
                    flushBuffer();
                    return;
                }
                _writeIndex = curBufferLength - splitPoint;
            }
            else
            {
                Read(mReadBuffer, _writeIndex, curBufferLength);
                _writeIndex += curBufferLength;
            }            
            startCommandReceive();         
        }

        /// <summary>
        /// Retrieve the command from the internal buffer
        /// </summary>
        /// <param name="dataInBufferLength">Length of command in buffer</param>
        /// <param name="newReadIndex">Start index of the command in buffer</param>
        /// <returns></returns>
        private byte[] GetCommand(int dataInBufferLength, int newReadIndex)
        {
            byte[] command = new Byte[dataInBufferLength];
            if (_readIndex + (int)dataInBufferLength > mReadBuffer.Length)
            {
                int splitLength = mReadBuffer.Length - _readIndex;
                Array.Copy(mReadBuffer, _readIndex, command, 0, splitLength);
                Array.Copy(mReadBuffer, 0, command, splitLength, newReadIndex);
            }
            else
            {
                Array.Copy(mReadBuffer, _readIndex, command, 0, (int)dataInBufferLength);
            }
            _readIndex = newReadIndex;
            return command;
        }

        /// <summary>
        /// Erase the buffers
        /// </summary>
        /// <param name="readBufOnly">Define if the read buffer only should be erased</param>
        public void flushBuffer(bool readBufOnly = false)
        {
            mReadBuffer = new byte[bufferMax];
            _readIndex = 0;
            if (!readBufOnly)
            {
                _writeBuffer = new byte[Constants.C_LENGTH];
                _writeIndex = 0;
            }
        }

        /// <summary>
        /// Start a new Thread that checks the buffer for new commands
        /// </summary>
        public void startCommandReceive()
        {
           Thread getCommand = new Thread(ExtractCommand);
           getCommand.Start();
        }

        /// <summary>
        /// See if buffer contains a new line and throw an event if new command is available
        /// </summary>
        private void ExtractCommand()
        {
            mCommandExtractCounter++;
            // lock to prevent overwrite of the indices
            int readIndex = 0, writeIndex = 0;
            lock(new object())
            {
                readIndex = _readIndex;
                writeIndex = _writeIndex;
                Debug.Print("ExtractCommand: Read index: " + readIndex + " Write index: " + writeIndex);
            }
            int dataInBufferLength = 0;
            BTEventArgs e = new BTEventArgs();
            if (writeIndex < readIndex)
            {                
                for (int index = 0; index < writeIndex; index++)
                {
                    if (mReadBuffer[index] == 0x0A)
                    {
                        dataInBufferLength = (mReadBuffer.Length - readIndex) + index + 1;
                        Debug.Print("ExtractCommand: Roundtrip: 0x0A found with at: " + index + " with length: " + dataInBufferLength + " From: " + readIndex);
                        e.CommandRaw = GetCommand(dataInBufferLength, index + 1);
                        CommandReceived(this, e);
                        if (index + dataInBufferLength < writeIndex)
                        {
                            startCommandReceive();
                        }
                        break;
                    }
                }
            }
            else
            {
                for (int index = readIndex; index < writeIndex; index++)
                {
                    if (mReadBuffer[index] == 0x0A)
                    {
                        dataInBufferLength = (index + 1) - readIndex;
                        Debug.Print("ExtractCommand: Regular: 0x0A found with length: " + dataInBufferLength + " From: " + readIndex);
                        e.CommandRaw = GetCommand(dataInBufferLength, index + 1);
                        CommandReceived(this, e);
                        //TODO: dataInBufferLenght is not the right way to test?!
                        if (dataInBufferLength < _writeIndex)
                        {
                            startCommandReceive();
                        }
                        break;
                    }
                }
            }
            mCommandExtractCounter--;
            if (mCommandExtractCounter != 0)
            {
                Program.THROW_ERROR(Constants.FW_ERRORS.EXTRACT_RACE_CONDITION);
            }
        }
    }
}
