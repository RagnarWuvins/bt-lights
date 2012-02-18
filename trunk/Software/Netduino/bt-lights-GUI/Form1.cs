using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace bt_lights_GUI
{
    public partial class Form1 : Form
    {
        // TODO: Load settings from file
        public SerialPort _serial = new SerialPort("COM22", 38400, Parity.None, 8, StopBits.One);
        private string[] _modes = { "No Op", "On", "Off", "Fade" };


        public Form1()
        {
            //_serial.Open();
            _serial.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            InitializeComponent();
            var c = gui_channelSlider.Controls.OfType<ComboBox>();
            int channelIndex = 0;
            foreach (ComboBox cb in c)
            {
                foreach (string mode in _modes)
                {
                    cb.Items.Add(mode);
                }
                cb.SelectedIndex = 0;
                cb.Tag = channelIndex;
                cb.SelectedIndexChanged += new EventHandler(channelMode_Changed);
                channelIndex++;
            }
        }

        private void channelMode_Changed(object sender, EventArgs args)
        {
            ComboBox cb = (ComboBox)sender;
            sendChannelMode((int)cb.Tag, cb.SelectedIndex);
        }

        private void tool_connect_Click(object sender, EventArgs e)
        {
            string conState;
            if(tool_connect.Checked)
            {
                _serial.Open();
                conState = "connected";
            }
            else
            {
                _serial.Close();
                conState = "disconnected";
            }
            status_connection.Text = String.Format("Connection: {0}", conState);
        }

        private void menu_options_connection_Click(object sender, EventArgs e)
        {
            Form connSettingsForm = new Form();
            PropertyGrid connSettings = new PropertyGrid();
            connSettings.Size = new Size(300, 250);
            connSettings.Tag = "serialSettings";
            connSettingsForm.Controls.Add(connSettings);
            this.Text = "Optionen-Dialogfeld";
            connSettings.SelectedObject = _serial;
            connSettings.Visible = true;
            connSettingsForm.FormClosed += new FormClosedEventHandler(setupSerial);
            connSettingsForm.Show();            
        }

        private void setupSerial(object sender, FormClosedEventArgs args)
        {
            //TODO: save to file
        }

        private void sendChannelValue(int channelNum, int value)
        {
            string command = "cc" + channelNum.ToString("00") + "+value=" + value.ToString("000") + "\r\n";
            Console.Write(command);
            _serial.Write(command);
        }

        private void sendChannelMode(int channelNum, int value)
        {
            string command = "cc" + channelNum.ToString("00") + "+mode=" + value.ToString("000") + "\r\n";
            Console.Write(command);
            _serial.Write(command);
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            
            //byte[] read = new byte[100];
            //_serial.Read(read,0,_serial.BytesToRead);
            //string test = new string(Encoding.UTF8.GetChars(read));
            //Console.WriteLine(test);
            Console.WriteLine(_serial.ReadLine());
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(0, tb.Value);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(1, tb.Value);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(2, tb.Value);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(3, tb.Value);
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(4, tb.Value);
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(5, tb.Value);
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(6, tb.Value);
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(7, tb.Value);
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(8, tb.Value);
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            sendChannelValue(9, tb.Value);
        }
    }
}
