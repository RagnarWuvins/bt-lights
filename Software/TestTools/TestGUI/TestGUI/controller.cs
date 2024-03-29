﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BTLights;
using System.IO.Ports;
using System.Threading;
using GraphLib;
using System.IO;
using System.Diagnostics;

namespace BluetoothLights
{
    public partial class controller : Form
    {
        private static string[] _toolButtonDesc = { "Plot channel values", "Generate XML for Android", "Clear output", "Stress Test" };

        private CheckBox[] _checkBoxes = new CheckBox[Constants.G_MAX_CHANNELS];
        private Button[] _cmdButtons = new Button[(int)Constants.COMMAND.CMD_NUM];
        private Button[] _gCmdButtons = new Button[(int)Constants.GLOBAL_COMMAND.CMD_NUM];
        private TrackBar _valueBar = new TrackBar();
        private NumericUpDown _textBox = new NumericUpDown();
        private static TextBox _output = new TextBox();
        private Button[] _toolButtons = new Button[_toolButtonDesc.Length];
        private static bool _childrenRunning = false;

        private static int _offset = 30;
        private static int _margin = 10;
        private static int _cbWidth = 80;
        private static int _bWidth = 120;
        private static int _bHeight = 20;
        private static int _elementNum = 0;
        private static int _outputHeight = 100;

        private static int[] _allElements = { Constants.G_MAX_CHANNELS, (int)Constants.COMMAND.CMD_NUM, (int)Constants.GLOBAL_COMMAND.CMD_NUM };
        private static int _sliderYPos = functions.GetMax(_allElements);
        private static int _sliderWidth = 2 * _margin + 2 * _bWidth + _cbWidth;
        private static int _width = _sliderWidth + 4 * _margin;
        private static int _height = 850;

        private static int _cla = 0;
        private static int _mode = 0;
        private static int _address = 0;
        private static int _value = 0;
        private static SerialPort _srl;
        static int entryCounter = 0;
        private volatile bool GetCCReceived = false;
        private Stopwatch mStopWatch;
        private int mValue;

        public controller()
        {
            InitializeComponent();

            connectComBox.Items.AddRange(SerialPort.GetPortNames());
            connectComBox.SelectedItem = Properties.Settings.Default.lastPort;

            statusBar.Text = "Disconnected";

            _srl = new SerialPort();
            _srl.PortName = (string)connectComBox.SelectedItem;
            _srl.NewLine = "\r\n";
            _srl.DataReceived += new SerialDataReceivedEventHandler(_dataReceived);

            for (int channelCounter = 0; channelCounter < _checkBoxes.Length; channelCounter++)
            {
                CheckBox cb = _checkBoxes[channelCounter];
                cb = new CheckBox();
                cb.Location = new Point(_margin, (channelCounter * _bHeight) + _margin + _offset);
                cb.Name = String.Format("_channel{0}", channelCounter);
                cb.Size = new Size(_cbWidth, _bHeight);
                cb.TabIndex = _elementNum;
                cb.Tag = channelCounter;
                cb.Text = String.Format("Channel: {0:X1}", channelCounter);
                cb.UseVisualStyleBackColor = true;
                cb.Click += new EventHandler(_cb_Click);
                this.Controls.Add(cb);
                _elementNum++;
            }

            string[] CommandTexts = Enum.GetNames(typeof(Constants.COMMAND));
            for (int buttonCounter = 0; buttonCounter < _cmdButtons.Length; buttonCounter++)
            {

                Button bt = _cmdButtons[buttonCounter];
                bt = new Button();
                bt.Location = new Point((_margin * 2 + _cbWidth), (buttonCounter * _bHeight + _margin + _offset));
                bt.Name = String.Format("_cmdButton{0}", buttonCounter);
                bt.Size = new Size(_bWidth, _bHeight);
                bt.TabIndex = _elementNum;
                bt.Text = CommandTexts[buttonCounter];
                bt.UseVisualStyleBackColor = true;
                bt.Click += new EventHandler(_cmdButton_Click);
                this.Controls.Add(bt);
                _elementNum++;
            }

            string[] gCommandTexts = Enum.GetNames(typeof(Constants.GLOBAL_COMMAND));
            for (int buttonCounter = 0; buttonCounter < _gCmdButtons.Length; buttonCounter++)
            {

                Button bt = _gCmdButtons[buttonCounter];
                bt = new Button();
                bt.Location = new Point((_margin * 3 + _cbWidth + _bWidth), (buttonCounter * _bHeight + _margin + _offset));
                bt.Name = String.Format("_gCmdButton{0}", buttonCounter);
                bt.Size = new Size(_bWidth, _bHeight);
                bt.TabIndex = _elementNum;
                bt.Text = gCommandTexts[buttonCounter];
                bt.UseVisualStyleBackColor = true;
                bt.Click += new EventHandler(_gCmdButton_Click);
                this.Controls.Add(bt);
                _elementNum++;
            }

            _valueBar.Orientation = Orientation.Horizontal;
            _valueBar.Location = new Point(_margin, _sliderYPos * _bHeight + _margin * 2 + _offset);
            _valueBar.Name = "_valueBar";
            _valueBar.Size = new Size(_sliderWidth, _bHeight);
            _valueBar.Maximum = 255;
            _valueBar.Minimum = 0;
            _valueBar.ValueChanged += new EventHandler(_valueBar_Change);
            this.Controls.Add(_valueBar);
            _elementNum++;

            _textBox.Location = new Point(_margin, _valueBar.Location.Y + _valueBar.Size.Height + _margin + _offset);
            _textBox.Name = "_textBox";
            _textBox.ValueChanged += new EventHandler(_textBox_Change);
            _textBox.Maximum = 255;
            _textBox.Minimum = 0;
            this.Controls.Add(_textBox);

            _output.Location = new Point(_margin, _textBox.Location.Y + _textBox.Size.Height + _margin + _offset);
            _output.Name = "_output";
            _output.Multiline = true;
            System.Drawing.Font tbFont = new Font("Courier New", 8, FontStyle.Bold);
            _output.Font = tbFont;
            _output.ScrollBars = ScrollBars.Vertical;
            _output.Size = new Size(_sliderWidth, _outputHeight);
            this.Controls.Add(_output);
            _elementNum++;

            for (int toolButtonCounter = 0; toolButtonCounter < _toolButtons.Length; toolButtonCounter++)
            {
                Button tb = _toolButtons[toolButtonCounter];
                tb = new Button();
                tb.Location = new Point(_margin, _output.Location.Y + _output.Size.Height + _margin + (toolButtonCounter * _bHeight) + _offset);
                tb.Name = String.Format("tb{0}", toolButtonCounter);
                tb.Size = new Size(_sliderWidth, _bHeight);
                tb.TabIndex = _elementNum;
                tb.Tag = String.Format("tb{0}", toolButtonCounter);
                tb.Text = _toolButtonDesc[toolButtonCounter];
                tb.UseVisualStyleBackColor = true;
                tb.Click += new EventHandler(_tb_Click);
                this.Controls.Add(tb);
                _elementNum++;
            }

            this.Size = new Size(_width, _height);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private static void _childClosed(object sender, EventArgs e)
        {
            _childrenRunning = false;
        }

        private void _tb_Click(object sender, EventArgs e)
        {
            Button tb = (Button)sender;
            debugOut(tb.Text);
            switch ((string)tb.Tag)
            {
                case "tb0":
                    _childrenRunning = true;
                    plotter pl = new plotter(_srl);
                    pl.FormClosed += new FormClosedEventHandler(_childClosed);
                    pl.Show();
                    break;
                case "tb1":
                    string oupath = functions.Const2XML();
                    string path = Path.Combine(Environment.CurrentDirectory, oupath);
                    path = Path.GetFullPath(path);
                    debugOut(path);
                    break;
                case "tb2":
                    _output.Clear();
                    break;
                case "tb3":
                    this.GetCCReceived = false;
                    mStopWatch = new Stopwatch();
                    mStopWatch.Start();
                    // Clear counter
                    _mode = (int)Constants.GLOBAL_COMMAND.CMD_RESET_CC;
                    _cla = (int)Constants.CLASS.GC_CMD;
                    _sendData();
                    Thread.Sleep(100);
                    // Send 100 commands
                    ThreadStart ts = new ThreadStart(stressSender);
                    Thread t = new Thread(ts);
                    t.Start();
                    break;
                default:
                    break;
            }
        }

        private void stressSender()
        {
            Stopwatch totalWatch = new Stopwatch();
            totalWatch.Start();
            bool success = true;
            for (int i = 0; i < 100; i++)
            {
                mStopWatch.Restart();
                _srl.DiscardInBuffer();
                _mode = (int)Constants.GLOBAL_COMMAND.CMD_GET_CC;
                _cla = (int)Constants.CLASS.GC_CMD;
                byte[] cmd = _sendData();
                while (!this.GetCCReceived)
                {
                    if (mStopWatch.ElapsedMilliseconds > 1000)
                    {
                        debugOut(String.Format("No answer received after: {0}\n", mStopWatch.ElapsedMilliseconds), true);
                        return;
                    }
                }
                success &= (this.mValue == i + 1);
                this.GetCCReceived = false;
                debugOut(String.Format("Command: {0} at {1}\n", BitConverter.ToString(cmd), mStopWatch.ElapsedMilliseconds), true);
            }
            mStopWatch.Stop();
            totalWatch.Stop();
            long endTime = totalWatch.ElapsedMilliseconds;
            debugOut(String.Format("Sending 100 commands took: {0} ms\nAverage time: {1}ms\n", endTime, endTime / 100), true);
            debugOut(String.Format("Test success: {0}\n", success), true);
            _srl.DiscardInBuffer();
        }

        private static void _cmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            debugOut(bt.Text);
            _mode = _string2enum(bt.Text);
            _cla = (int)Constants.CLASS.CC_CMD;
            debugOut(_mode);
            _sendData();
        }

        private static void _gCmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            _mode = _string2enum(bt.Text);
            _cla = (int)Constants.CLASS.GC_CMD;
            entryCounter = 0;
            _sendData();
        }

        private static void _cb_Click(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                _address |= (1 << (int)cb.Tag);
            }
            else
            {
                _address &= ~(1 << (int)cb.Tag);
            }
        }

        private void _valueBar_Change(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            _value = tb.Value;
            _textBox.Value = _value;
            _sendData();
        }

        private void _textBox_Change(object sender, EventArgs e)
        {
            NumericUpDown tb = (NumericUpDown)sender;
            _value = (int)tb.Value;
            _valueBar.Value = _value;
            _sendData();
        }
        private void _dataReceived(object sender, EventArgs e)
        {
            if (_childrenRunning)
            {
                return;
            }
            SerialPort srl = (SerialPort)sender;
            while (srl.BytesToRead > 0)
            {
                string input = srl.ReadLine();
                if (input == "ACK")
                {
                    Debug.Print("ACK Received");
                }
                else
                {
                    byte[] currentBuffer = ASCIIEncoding.UTF8.GetBytes(input);
                    string debugString = "";
                    switch (_cla)
                    {
                        // these are the channel commands
                        case (int)Constants.CLASS.CC_CMD:
                            int cla = currentBuffer[0];
                            int mode = currentBuffer[1];
                            int address = currentBuffer[2] << 8 | currentBuffer[3];
                            int value = currentBuffer[4];
                            int crc = currentBuffer[0] + currentBuffer[4];
                            if (crc == currentBuffer[5] | (crc - 0x100) == currentBuffer[5])
                            {
                                debugString = String.Format("Class:\t\t{0}\nMode:\t\t{1}\nAddress:\t0x{2:X4}\nValue:\t\t{3}\n\n", cla, mode, address, value);
                            }
                            else
                            {
                                debugString = "CRC Error!";
                            }
                            debugOut(debugString);
                            break;
                        // these are the global commands
                        case (int)Constants.CLASS.GC_CMD:
                                cla = currentBuffer[0];
                                mode = currentBuffer[1];
                                address = currentBuffer[2] << 8 | currentBuffer[3];
                                value = currentBuffer[4];
                                crc = currentBuffer[0] + currentBuffer[4];
                                if (!(crc == currentBuffer[5] | (crc - 0x100) == currentBuffer[5]))
                                {
                                    debugString = "CRC Error!";
                                }
                                // if the command was firmware error tracing:
                                if (_mode == (int)Constants.GLOBAL_COMMAND.CMD_ERROR)
                                {
                                    uint sysTime = 0;
                                    uint fwError = 0;
                                    // stupig BitConverter uses revers endianism :/
                                    uint errorEntry = BitConverter.ToUInt32(new byte[] { currentBuffer[5], currentBuffer[4], currentBuffer[3], currentBuffer[2] }, 0);
                                    sysTime = (errorEntry & 0x00FFFFFF);
                                    fwError = (errorEntry & 0xFF000000) >> 24;
                                    int min = (int)sysTime / 60;
                                    int hours = (int)min / 60;
                                    int sec = (int)sysTime - (hours * 3600 + min * 60);
                                    String erDesc = _int2enum((int)fwError, typeof(Constants.FW_ERRORS));
                                    String formatString = "{0}\t\t{1}:{2}:{3}\t{4}\n";
                                    if (erDesc.Length > 12)
                                    {
                                        formatString = "{0}\t{1}:{2}:{3}\t{4}\n";
                                    }
                                    debugOut(String.Format(formatString, erDesc, hours, min, sec, entryCounter), true);
                                    entryCounter++;
                                }
                                else if (_mode == (int)Constants.GLOBAL_COMMAND.CMD_GET_CC)
                                {
                                    this.GetCCReceived = true;
                                    this.mValue = value;
                                    debugOut(String.Format("Command Counter: {0}\n", value), true);
                                }
                                else if (_mode == (int)Constants.GLOBAL_COMMAND.CMD_GET_CMD_TIME)
                                {
                                    debugOut(String.Format("Command Time: {0}", (currentBuffer[3] << 16) | (currentBuffer[4] << 8) | currentBuffer[5]), false);
                                }
                                else if (_mode == (int)Constants.GLOBAL_COMMAND.CMD_GET_VERSION)
                                {
                                    debugOut(String.Format("Version: {0}.{1}", value >> 4, value & 0xF), false);
                                }
                                else
                                {
                                    int pos = 32;
                                    int result = 0;
                                    foreach (byte by in currentBuffer)
                                    {
                                        result |= (by << pos);
                                        pos -= 8;
                                    }
                                    debugOut(result.ToString() + "\n", true);
                                }                            
                            break;
                        default:
                            debugOut(srl.ReadLine(), true);
                            break;
                    }
                }
            }
        }
        

        private static byte[] _sendData()
        {
            byte _class = (byte)_cla;
            byte _mod = (byte)_mode;
            byte _address_higher = (byte)((_address & 0xFF00) >> 8);
            byte _adress_lower = (byte)(_address & 0x00FF);
            byte _crc = (byte)(_cla + _value);

            byte[] command = { _class, _mod, _address_higher, _adress_lower, (byte)_value, _crc, 0xD, 0xA };
            _srl.Write(command, 0, command.Length);
            return command;
        }

        private static int _string2enum(string text)
        {
            int output = 0;
            if (Enum.IsDefined(typeof(Constants.COMMAND), text))
            {
                output = (int)Enum.Parse(typeof(Constants.COMMAND), text, true);
            }
            else if (Enum.IsDefined(typeof(Constants.GLOBAL_COMMAND), text))
            {
                output = (int)Enum.Parse(typeof(Constants.GLOBAL_COMMAND), text, true);
            }
            else
            {
                Console.Out.WriteLine(String.Format("{0} is not part of any command enum {1}", text));
            }
            return output;
        }

        private static string _int2enum(int modeNum, Type enumType)
        {
            string output = "";
            if (Enum.IsDefined(enumType, modeNum))
            {
                string[] modeEnums = Enum.GetNames(enumType);
                output = modeEnums[modeNum];
            }
            else
            {
                Console.Out.WriteLine(String.Format("{0} is not part of this enum {1}", modeNum, enumType.ToString()));
            }
            return output;
        }

        private static void debugOut(int number, bool append = false)
        {
            debugOut(String.Format("{0}", number), append);
        }

        private static void debugOut(string text, bool append = false)
        {
            string debugText = _output.Text;
            text = text.Replace("\n", Environment.NewLine);
            if (append)
            {
                debugText += text;
            }
            else
            {
                debugText = text;
            }
            if (_output.InvokeRequired)
            {
                _output.Invoke((MethodInvoker)delegate()
                    {
                        _output.Text = debugText;
                        _output.SelectionStart = _output.Text.Length;
                        _output.ScrollToCaret();

                    }
                );
            }
            else
            {
                _output.Text = debugText;
                _output.SelectionStart = _output.Text.Length;
                _output.ScrollToCaret();

            }
        }

        private void connectCom_Click(object sender, EventArgs e)
        {
            _srl.PortName = (string)connectComBox.SelectedItem;
            Properties.Settings.Default.lastPort = _srl.PortName;
            Properties.Settings.Default.Save();
            statusBar.Text = "Disconnected";
            if (!_srl.IsOpen)
            {
                try
                {
                    _srl.Close();
                    _srl.Open();
                    statusBar.Text = "Connected";
                }
                catch
                {
                    MessageBox.Show("Error while opening COM interface.", "Critical Warning");
                }
            }
        }

        private void disconnectCom_Click(object sender, EventArgs e)
        {
            if (_srl.IsOpen)
            {
                statusBar.Text = "Connected";
                try
                {
                    _srl.Close();
                    statusBar.Text = "Disconnected";
                }
                catch
                {
                    MessageBox.Show("Error while disconnecting COM interface.", "Critical Warning");
                }
            }
        }
    }
}
