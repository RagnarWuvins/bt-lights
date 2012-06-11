using System;
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

namespace BluetoothLights
{
    public partial class controller : Form
    {
        private static string[] _toolButtonDesc = {"Plot channel values", "Trace data", "Clear output"};

        private CheckBox[] _checkBoxes = new CheckBox[Constants.G_MAX_CHANNELS];
        private Button[] _cmdButtons = new Button[(int)Constants.MODE.CMD_NUM];
        private Button[] _gCmdButtons = new Button[(int)Constants.COMMANDS.CMD_NUM];
        private TrackBar _valueBar = new TrackBar();
        private static TextBox _output = new TextBox();        
        private Button[] _toolButtons = new Button[_toolButtonDesc.Length];
        private static bool _childrenRunning = false;

        private static int _margin = 10;
        private static int _cbWidth = 80;
        private static int _bWidth = 120;
        private static int _bHeight = 20;
        private static int _elementNum = 0;
        private static int _outputHeight = 100;

        private static int[] _allElements = {Constants.G_MAX_CHANNELS, (int)Constants.MODE.CMD_NUM, (int)Constants.COMMANDS.CMD_NUM};
        private static int _sliderYPos = functions.GetMax(_allElements);
        private static int _sliderWidth = 2 * _margin + 2 * _bWidth + _cbWidth;        
        private static int _width = _sliderWidth + 4 * _margin;
        private static int _height = 600;

        private static int _cla = 0;
        private static int _mode = 0;        
        private static int _address = 0;
        private static int _value = 0;
        private static SerialPort _srl;

        public controller(SerialPort srl)
        {
            InitializeComponent();
            _srl = srl;
            if (!_srl.IsOpen)
            {
                try
                {
                    _srl.Open();
                }
                catch
                {
                    MessageBox.Show("Error while opening COM interface.", "Critical Warning");
                }
            }

            _srl.DataReceived += new SerialDataReceivedEventHandler(_dataReceived);

            for (int channelCounter = 0; channelCounter < _checkBoxes.Length; channelCounter++ )
            {
                CheckBox cb = _checkBoxes[channelCounter];
                cb = new CheckBox();
                cb.Location = new Point(_margin, (channelCounter * _bHeight) + _margin);
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

            string[] CommandTexts = Enum.GetNames(typeof(Constants.MODE));
            for (int buttonCounter = 0; buttonCounter < _cmdButtons.Length; buttonCounter++)
            {                

                Button bt = _cmdButtons[buttonCounter];
                bt = new Button();
                bt.Location = new Point((_margin * 2 + _cbWidth), (buttonCounter * _bHeight + _margin));
                bt.Name = String.Format("_cmdButton{0}", buttonCounter);
                bt.Size = new Size(_bWidth, _bHeight);
                bt.TabIndex = _elementNum;
                bt.Text = CommandTexts[buttonCounter];
                bt.UseVisualStyleBackColor = true;
                bt.Click += new EventHandler(_cmdButton_Click);
                this.Controls.Add(bt);
                _elementNum++;
            }

            string[] gCommandTexts = Enum.GetNames(typeof(Constants.COMMANDS));
            for (int buttonCounter = 0; buttonCounter < _gCmdButtons.Length; buttonCounter++)
            {

                Button bt = _gCmdButtons[buttonCounter];
                bt = new Button();
                bt.Location = new Point((_margin * 3 + _cbWidth + _bWidth), (buttonCounter * _bHeight + _margin));
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
            _valueBar.Location = new Point(_margin, _sliderYPos * _bHeight+ _margin * 2);
            _valueBar.Name = "_valueBar";
            _valueBar.Size = new Size(_sliderWidth, _bHeight);
            _valueBar.Maximum = Constants.LIM_HIGH;
            _valueBar.Minimum = Constants.LIM_LOW;            
            _valueBar.ValueChanged += new EventHandler(_valueBar_Change);
            this.Controls.Add(_valueBar);
            _elementNum++;

            _output.Location = new Point(_margin, _valueBar.Location.Y + _valueBar.Size.Height + _margin);
            _output.Name = "_output";
            _output.Multiline = true;
            _output.ScrollBars = ScrollBars.Vertical;
            _output.Size = new Size(_sliderWidth, _outputHeight);
            this.Controls.Add(_output);
            _elementNum++;

            for (int toolButtonCounter = 0; toolButtonCounter < _toolButtons.Length; toolButtonCounter++)
            {
                Button tb = _toolButtons[toolButtonCounter];
                tb = new Button();
                tb.Location = new Point(_margin, _output.Location.Y + _output.Size.Height + _margin + (toolButtonCounter * _bHeight));
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

        private static void _tb_Click(object sender, EventArgs e)
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

                    break;
                case "tb2":
                    _output.Clear();
                    break;
                default:
                    break;
            }
        }

        private static void _cmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            debugOut(bt.Text);
            _mode = _string2enum(bt.Text);
            _cla = 0;
            debugOut(_mode);
            _sendData();
        }

        private static void _gCmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            _mode = _string2enum(bt.Text);
            _cla = 1;
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

        private static void _valueBar_Change(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            _value = tb.Value;
            Console.WriteLine(_value);
            _sendData();
        }

        private static void _dataReceived(object sender, EventArgs e)
        {
            if (_childrenRunning)
            {
                return;
            }
            SerialPort srl = (SerialPort)sender;
            if (srl.BytesToRead < 7)
            {
                return;
            }
            string debugString = "";
            switch (_cla)
            {
                case 0:
                    byte[] currentBuffer = new Byte[7];
                    srl.Read(currentBuffer, 0, 7);
                    int cla = currentBuffer[0] >> 4;
                    int mode = currentBuffer[0] & 0x00FF;
                    int address = currentBuffer[1] << 8 | currentBuffer[2];
                    int value = currentBuffer[3];
                    int crc = currentBuffer[0] + currentBuffer[3];
                    if (crc == currentBuffer[4] | (crc - 0x100) == currentBuffer[4])
                    {
                        debugString = String.Format("Class:\t{0}\nMode:\t{1}\nAddress:\t0x{2:X4}\nValue:\t{3}\n\n", cla, mode, address, value);
                    }
                    else
                    {
                        debugString = "CRC Error!";
                    }
                    debugOut(debugString);
                    break;
                case 1:
                    while (srl.BytesToRead > 0)
                    {
                        byte[] test = ASCIIEncoding.UTF8.GetBytes(srl.ReadLine());
                        if (_mode == 3)
                        {
                            int pos = 32;
                            int sysTime = 0;
                            int fwError = 0;
                            foreach (byte by in test)
                            {
                                if (pos > 8)
                                {
                                    fwError |= (by << pos);
                                }
                                else
                                {
                                    sysTime |= (by << pos);
                                }
                                pos -= 8;
                            }
                            debugOut(String.Format("ERROR Num: {0} Time: {1}\n",fwError >> 16, sysTime), true); 
                        }
                        else
                        {
                            int pos = 32;
                            int result = 0;
                            foreach (byte by in test)
                            {
                                result |= (by << pos);
                                pos -= 8;
                            }
                            debugOut(result.ToString() + "\n", true); 
                        }
                    }
                    break;
                default:
                    break;
            }
            
        }

        private static void _sendData()
        {
            //if (_mode == (int)Constants.MODE.FUNC & _value > 0)
            //{
            //    _value = _value - Constants.LIM_LOW;
            //}
            byte _modcla = (byte)(_cla << 4 | _mode);
            byte _address_higher = (byte)((_address & 0xFF00) >> 8);
            byte _adress_lower = (byte)(_address & 0x00FF);
            byte _crc = (byte)(_modcla + _value);

            byte[] command = { _modcla, _address_higher, _adress_lower, (byte)_value, _crc, 0xD, 0xA };
            _srl.Write(command, 0, command.Length);
        }

        private static int _string2enum(string text)
        {
            int output = 0;
            if (Enum.IsDefined(typeof(Constants.MODE), text))
            {
                output = (int)Enum.Parse(typeof(Constants.MODE), text, true);
            }
            else if(Enum.IsDefined(typeof(Constants.COMMANDS), text))
            {
                output = (int)Enum.Parse(typeof(Constants.COMMANDS), text, true);
            }
            else
            {
                MessageBox.Show("Not an enum!!!");
            }
            return output;
        }

        private static string _int2enum(int modeNum)
        {
            string output = "";
            if (Enum.IsDefined(typeof(Constants.MODE), modeNum))
            {
                string[] modeEnums = Enum.GetNames(typeof(Constants.MODE));
                output = modeEnums[modeNum];
            }
            else if (Enum.IsDefined(typeof(Constants.COMMANDS), modeNum))
            {
                string[] modeEnums = Enum.GetNames(typeof(Constants.COMMANDS));
                output = modeEnums[modeNum];
            }
            else
            {
                MessageBox.Show("Not an enum!!!");
            }
            return output;
        }

        private static void debugOut(int number, bool append = false)
        {
            debugOut(String.Format("{0}",number), append);
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
    }
}
