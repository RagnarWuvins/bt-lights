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
        private string[] mATreply = new string[5];  // store the last 5 incoming commands
        private static byte[] _writeBuffer;
        private Stopwatch btClock = Stopwatch.StartNew();
        private long elapsedCommandTime = 0;
        private bool clocklock = false;
        private static byte CR = 0xA; //This is the ASCII code for a carriage return
        private static byte LF = 0xD; //This is the ASCII code for a line feed
        /// <summary>
        /// Connection status of the module
        /// </summary>
        public static STATES State = STATES.UNKNOWN;

        /// <summary>Event that is called when a command is received (Termination is \r\n or 0xA, 0xD)</summary>
        public event BTEvent CommandReceived;

        public enum STATES
        {
            INITIALIZED,    // initialized status
            ERROR,          // Error status
            CONNECTED,      // connected status
            DISCONNECTED,   // disconnected
            UNKNOWN,        // no reply or something
        }
        const int bufferMax = 2048;
        const int numOfBuffer = 2;
        private int readBufferPosition;

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
            _writeBuffer = Encoding.UTF8.GetBytes(command + "\r");
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
            Debug.Print("send2BT: OUT: Address: " + (command[2] << 8 | command[3]) + " Value: " + command[4]);
            Write(s_command, 0, s_command.Length);
        }

        /// <summary>
        /// Send a complete array of string to the module
        /// </summary>
        /// <param name="commands">String array of commands</param>
        public void dump(string[] commands)
        {
            dump(commands, true);
        }

        /// <summary>
        /// Send a complete array of string to the module
        /// </summary>
        /// <param name="commands">String array of commands</param>
        /// <param name="fastMode">If the commands should be send as fast as possible</param>
        public void dump(string[] commands, bool fastMode)
        {
            mATPin.Write(true);
            if (!fastMode)
            {
                Thread.Sleep(200);
            }
            foreach (string command in commands)
            {
                send2BT(command);
                if (!fastMode)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            mATPin.Write(false);
        }

        public void OnModuleAnswer(object sender, BTEventArgs e)
        {
            string restoredText = Convert.ByteArrayToString(e.CommandRaw);
            if (restoredText.IndexOf("CONNECT", 0) >= 0)
            {
                State = STATES.CONNECTED;
            }
            if (restoredText.IndexOf("OK", 0) >= 0)
            {
                State = STATES.INITIALIZED;
            }
            if (restoredText.IndexOf("DISCONNECT", 0) >= 0)
            {
                State = STATES.DISCONNECTED;
            }
            if (restoredText.IndexOf("ERROR", 0) >= 0)
            {
                State = STATES.ERROR;
            }
            Debug.Print("State: " + State + " Answer: " + restoredText);
        }

        public long GetCommandProcessingTime()
        {
            return elapsedCommandTime;
        }

        // This is the IRQ handler that signals the main routine that a command is there
        private void receiveBT(object sender, SerialDataReceivedEventArgs e)
        {
            Thread getCommand = new Thread(ExtractCommand);
            getCommand.Start();            

            // restart the 5 minute timer
            MainProgram.RestartBluetoothTimeout();
        }

        /// <summary>
        /// Retrieve the command from the internal buffer
        /// </summary>
        /// <param name="dataInBufferLength">Length of command in buffer</param>
        /// <param name="newReadIndex">Start index of the command in buffer</param>
        /// <returns></returns>
        /*private byte[] GetCommand(int dataInBufferLength, int newReadIndex)
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
        }*/

        /// <summary>
        /// Erase the buffers
        /// </summary>
        /// <param name="readBufOnly">Define if the read buffer only should be erased</param>
        /*public void flushBuffer(bool readBufOnly = false)
        {
            mReadBuffer = new byte[bufferMax];
            mATreply = new string[mATreply.Length];
            mATreplyIndex = 0;
            _readIndex = 0;
            if (!readBufOnly)
            {
                _writeBuffer = new byte[Constants.C_LENGTH];
                _writeIndex = 0;
            }
        }*/

        /// <summary>
        /// See if buffer contains a new line and throw an event if new command is available
        /// </summary>
        private void ExtractCommand()
        {
            int bytesAvailable = BytesToRead;
            if (bytesAvailable > 0)
            {
                byte[] packetBytes = new byte[bytesAvailable];
                Read(packetBytes, 0, bytesAvailable);
                for (int i = 0; i < bytesAvailable; i++)
                {
                    byte b = packetBytes[i];
                    byte b4 = i == 0 ? (byte)0 : packetBytes[i - 1];
                    if ((b == CR) && (b4 == LF))
                    {
                        mReadBuffer[readBufferPosition++] = b;
                        byte[] encodedBytes = new byte[packetBytes.Length]; // + 1 because the splitter demands cr/lf flags
                        Array.Copy(mReadBuffer, 0, encodedBytes, 0, encodedBytes.Length);
                        readBufferPosition = 0;                        
                        elapsedCommandTime = btClock.ElapsedMilliseconds;
                        btClock.Restart();
                        BTEventArgs evt = new BTEventArgs();
                        evt.CommandRaw = encodedBytes;
                        CommandReceived(this, evt);                        
                    }
                    else
                    {
                        mReadBuffer[readBufferPosition++] = b;
                    }
                }
            }
        }
    }
}
