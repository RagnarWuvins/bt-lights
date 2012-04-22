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
        

        public Form1()
        {
            //_serial.Open();
            _serial.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            CheckForIllegalCrossThreadCalls = false; 
            InitializeComponent();
            gui_channelSlider.Enabled = false;
            var cbs = gui_channelSlider.Controls.OfType<ComboBox>();
            var tbs = gui_channelSlider.Controls.OfType<TrackBar>();
            int channelIndex = 0;
            
            foreach (ComboBox cb in cbs)
            {
                foreach (string mode in Enum.GetNames(typeof(BTLights.Constants.MODE)))
                {
                    cb.Items.Add(mode);
                }
                cb.SelectedIndex = 0;
                cb.Tag = channelIndex;
                cb.SelectedIndexChanged += new EventHandler(channelMode_Changed);
                channelIndex++; 
            }

            foreach (string mode in Enum.GetNames(typeof(BTLights.Constants.MODE)))
            {
                master_mode.Items.Add(mode);
                master_mode.SelectedIndex = 0;
                master_mode.Tag = 0xFFFF;
                master_mode.SelectedIndexChanged += new EventHandler(channelMode_Changed);
            }

            channelIndex = 0;
            foreach (TrackBar tb in tbs)
            {
                tb.Tag = channelIndex;
                tb.Scroll += new EventHandler(channelValue_Changed);
                channelIndex++;
            }

            master_value.Tag = 0xFFFF;
            master_value.Scroll += new EventHandler(channelValue_Changed);
        }

        private void channelMode_Changed(object sender, EventArgs args)
        {
            ComboBox cb = (ComboBox)sender;
            bool master = (cb.Name.IndexOf("master") >= 0) ? true : false;
            sendChannelMode((int)cb.Tag, cb.SelectedIndex, 0, master);
        }

        private void channelValue_Changed(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            bool master = (tb.Name.IndexOf("master") >= 0) ? true : false;
            sendChannelValue((int)tb.Tag, 0,tb.Value, master);
        }

        private void tool_connect_Click(object sender, EventArgs e)
        {
            string conState;
            if(tool_connect.Checked)
            {
                try
                {
                    _serial.Open();
                    sendChannelValue(0xFFFF, (int)BTLights.Constants.MODE.CMD_GET_VAL, 0, true);
                    conState = "connected";
                    gui_channelSlider.Enabled = true;
                }
                catch(Exception exp)
                {
                    conState = "failed (" + exp.Message + ")";
                }
            }
            else
            {
                _serial.Close();
                conState = "disconnected";
                gui_channelSlider.Enabled = false;
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

        private void sendChannelValue(int channelNum, int mode, int value, bool master = false)
        {
            byte _mode = (byte)mode, _class = 1;
            byte _modcla = (byte)(_class << 4 | _mode);
            int _address = (master) ? channelNum : (0x1 << channelNum);
            byte _address_higher = (byte)((_address & 0xFF00) >> 8);
            byte _adress_lower = (byte)(_address & 0x00FF);
            byte _crc = (byte)(_modcla + value);

            byte[] command = { _modcla, _address_higher, _adress_lower, (byte)value, _crc, 0xD, 0xA };

            string output = "Channel " + channelNum + ", Mode " + _mode + ", Value " + value + "\n";
            Console.Write(output);
            _serial.Write(command,0,command.Length);
        }

        private void sendChannelMode(int channelNum, int mode, int value = 0, bool master = false)
        {
            byte _mode = (byte)mode, _class = 1;
            byte _modcla = (byte)(_class << 4 | _mode);
            int _address = (master) ? channelNum : (0x1 << channelNum);
            byte _address_higher = (byte)((_address & 0xFF00) >> 8);
            byte _adress_lower = (byte)(_address & 0x00FF);
            byte _crc = (byte)(_modcla + value);

            byte[] command = { _modcla, _address_higher, _adress_lower, (byte)value, _crc, 0xD, 0xA };

            string output = "Channel " + channelNum + ", Mode " + _mode + ", Value " + value + "\n";
            Console.Write(output);
            _serial.Write(command, 0, command.Length);
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            string command = _serial.ReadLine();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] receivedCommand = enc.GetBytes(command);
            Console.WriteLine(command);           
        }

        private void outputBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
