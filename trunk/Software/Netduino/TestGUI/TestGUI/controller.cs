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

namespace WindowsFormsApplication1
{
    public partial class controller : Form
    {
        private CheckBox[] _checkBoxes = new CheckBox[Constants.G_MAX_CHANNELS];
        private Button[] _cmdButtons = new Button[(int)Constants.MODE.CMD_NUM];
        private Button[] _gCmdButtons = new Button[(int)Constants.COMMANDS.CMD_NUM];
        private TrackBar _valueBar = new TrackBar();

        private static int _margin = 10;
        private static int _cbWidth = 80;
        private static int _bWidth = 120;
        private static int _bHeight = 20;
        private static int _elementNum = 0;

        private static int[] _allElements = {Constants.G_MAX_CHANNELS, (int)Constants.MODE.CMD_NUM, (int)Constants.COMMANDS.CMD_NUM};
        private static int _sliderYPos = functions.GetMax(_allElements);
        private static int _sliderWidth = 2 * _margin + 2 * _bWidth + _cbWidth;
        private static int _width = _sliderWidth + 4 * _margin;
        private static int _height = _sliderYPos * _bHeight + _bHeight*4 + _margin;

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

            this.Size = new Size(_width, _height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private static void _cmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            Console.WriteLine(bt.Text);
            _mode = _string2enum(bt.Text);
            _cla = 0;
            Console.WriteLine(_mode);
            _sendData();
        }

        private static void _gCmdButton_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            Console.WriteLine(bt.Text);
            _mode = _string2enum(bt.Text);
            _cla = 1;
            Console.WriteLine(_mode);
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
            _sendData();
        }

        private static void _valueBar_Change(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            _value = tb.Value;
            _sendData();
        }

        private static void _sendData()
        {
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
    }
}
