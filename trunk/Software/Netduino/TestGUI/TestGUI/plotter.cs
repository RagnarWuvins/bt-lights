using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO.Ports;
using BTLights;
using System.Drawing;
using GraphLib;

namespace BluetoothLights
{
    class plotter : Form
    {
        private static int _width = 800;
        private static int _height = 600;
        private static int _margin = 10;
        private static int _bHeight = 20;
        private static int _rbWidth = 120;
        private static int _elementNum = 0;
        private static int _plotWidth = _width - 3 * _margin - _rbWidth;
        private static int _plotHeight = _height - 2 * _margin;
        private static int _numOfGraphs = 1;
        private static int[] _readIndex = new int[_numOfGraphs];
        private static int _address = 1;
        private static DateTime lastTimerTick = DateTime.Now;

        private static DataSource _StackSrc = null;
        private int _StackIdx = 0;
        private int _queryTime = 60;
        private int _queryBuffer = 256;

        private SerialPort _srl;
        private RadioButton[] _checkBoxes = new RadioButton[Constants.G_MAX_CHANNELS];
        private PlotterDisplayEx display = null;
        private PrecisionTimer.Timer mTimer;


        public plotter(SerialPort srl)
        {
            this._srl = srl;
            if (!_srl.IsOpen)
            {
                MessageBox.Show("Connection closed! Please open connection first!");
                return;
            }

            this._srl.DataReceived += new SerialDataReceivedEventHandler(_srlDataReceived);

            for (int channelCounter = 0; channelCounter < _checkBoxes.Length; channelCounter++)
            {
                RadioButton rb = _checkBoxes[channelCounter];
                rb = new RadioButton();
                if (channelCounter == 0)
                {
                    rb.Checked = true;
                }                
                rb.Location = new Point(_margin, (channelCounter * _bHeight) + _margin);
                rb.Name = String.Format("_channel{0}", channelCounter);
                rb.Size = new Size(_rbWidth, _bHeight);
                rb.TabIndex = _elementNum;
                rb.Tag = channelCounter;
                rb.Text = String.Format("Channel: {0:X1}", channelCounter);
                rb.UseVisualStyleBackColor = true;
                rb.Click += new EventHandler(_rb_Click);
                this.Controls.Add(rb);
                _elementNum++;
            }

            display = new PlotterDisplayEx();
            display.Smoothing = System.Drawing.Drawing2D.SmoothingMode.None;
            display.DataSources.Clear();
            display.DataSources.Add(new DataSource());
            display.Location = new Point(_rbWidth + _margin, _margin);
            display.Size = new Size(_plotWidth, _plotHeight);
            display.SetDisplayRangeX(0, _queryBuffer);
            display.DataSources[0].Length = _queryBuffer;
            display.PanelLayout = PlotterGraphPaneEx.LayoutMode.STACKED;
            display.DataSources[0].AutoScaleY = false;
            display.DataSources[0].AutoScaleX = false;
            display.DataSources[0].SetDisplayRangeY(Constants.LIM_LOW, Constants.LIM_HIGH);
            display.DataSources[0].SetGridDistanceY(100);

            display.DataSources[0].OnRenderXAxisLabel = RenderXLabel;
            ReadData(display.DataSources[0], 0);
            display.Refresh();
            this.Controls.Add(display);
            
            mTimer = new PrecisionTimer.Timer();
            mTimer.Period = _queryTime;                         // 20 fps
            mTimer.Tick += new EventHandler(OnTimerTick);
            lastTimerTick = DateTime.Now;
            mTimer.Start();

            this.ClientSize = new System.Drawing.Size(_width, _height);
            this.Name = "hp";
            this.Text = "Harry Plotter";
            this.Icon = (System.Drawing.Icon)SharedRessources.meister_lampe;
            
        }

        protected void ReadData(DataSource src, int idx)
        {
            _StackSrc = src;
            _StackIdx = idx;
            this._sendData();
        }

        private void _srlReadLine(object sender, EventArgs e)
        {
            string buffer = _srl.ReadLine();
        }

        private void _srlDataReceived(object sender, EventArgs e)
        {
            SerialPort srl = (SerialPort)sender;
            if (srl.BytesToRead < Constants.C_LENGTH)
            {
                return;
            }
            byte[] reply = new Byte[Constants.C_LENGTH];
            srl.Read(reply, 0, reply.Length);
            
            if (_readIndex[_StackIdx] < _StackSrc.Length - 1)
            {
                _readIndex[_StackIdx]++;
            }
            else
            {
                _readIndex[_StackIdx] = 0;
                for (int i = 0; i < _StackSrc.Length - 1; i++)
                {
                    _StackSrc.Samples[i].y = 0;
                }
            }
            int crc = reply[0] + reply[4];
            if (crc == reply[5] | (crc - 0x100) == reply[5])
            {
                _StackSrc.Samples[_readIndex[_StackIdx]].x = _readIndex[_StackIdx];
                _StackSrc.Samples[_readIndex[_StackIdx]].y = reply[4];
                
            }
            else if ((reply[2] << 8 | reply[3]) != _address)
            {
                Console.WriteLine("Wrong channel returned");
            }
            else
            {
                Console.WriteLine("CRC Error!");
            }
        }

        private void _rb_Click(object sender, EventArgs e)
        {
            Console.WriteLine(String.Format("Former address was: {0:X}", _address));
            RadioButton rb = (RadioButton)sender;
            _address = (1 << (int)rb.Tag);
            Console.WriteLine(String.Format("Now on Channel: {0}, address is: {1:X}", (int)rb.Tag, _address));
        }

        private void _sendData()
        {
            byte _mod = (byte)Constants.COMMAND.CMD_GET_VAL;
            byte _cla = (byte)Constants.CLASS.CC_CMD;
            byte _address_higher = (byte)((_address & 0xFF00) >> 8);
            byte _adress_lower = (byte)(_address & 0x00FF);
            byte _crc = (byte)(_cla + 0x00);

            byte[] command = { _cla, _mod, _address_higher, _adress_lower, 0x00, _crc, 0xD, 0xA };
            this._srl.Write(command, 0, command.Length);
        }

        private String RenderXLabel(DataSource s, int idx)
        {
            if (s.AutoScaleX)
            {
                //if (idx % 2 == 0)
                {
                    int Value = (int)(s.Samples[idx].x);
                    return "" + Value;
                }
                return "";
            }
            else
            {
                int Value = (int)(s.Samples[idx].x / 200);
                String Label = "" + Value + "\"";
                return Label;
            }
        }

        private String RenderYLabel(DataSource s, float value)
        {
            return String.Format("{0:0.0}", value);
        }

        protected override void OnClosed(EventArgs e)
        {
            mTimer.Stop();
            mTimer.Dispose();
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            display.Dispose();
            base.OnClosing(e);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            try
            {
                for (int j = 0; j < _numOfGraphs; j++)
                {
                    ReadData(display.DataSources[j], j);
                }
                this.Invoke(new MethodInvoker(RefreshGraph));
            }
            catch
            {
                Console.WriteLine("Form closed before timer executed.");
            }
        }

        private void RefreshGraph()
        {
            display.Refresh();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(plotter));
            this.SuspendLayout();
            // 
            // plotter
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "plotter";
            this.ResumeLayout(false);

        }
    }
}
