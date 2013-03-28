using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using BTLights;

namespace BluetoothLights
{
    public partial class Form1 : Form
    {
        SerialPort _srl = new SerialPort();
        controller _ctrl;   

        public Form1()
        {
            InitializeComponent();
            string[] portNames = SerialPort.GetPortNames();
            statusBar.Text = "disconnected";
            
            foreach (string port in portNames)
            {
                ports.Items.Add(port);
            }
            try
            {
                ports.SelectedItem = Properties.Settings.Default.lastPort;
            }
            catch
            {
                Console.WriteLine(Properties.Settings.Default.lastPort +"  couldn't be found");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_srl.IsOpen)
                {
                    _srl.BaudRate = Constants.BAUDRATE;
                    _srl.PortName = ports.SelectedItem.ToString();
                    _srl.Open();
                    Properties.Settings.Default.lastPort = _srl.PortName;
                    Properties.Settings.Default.Save();
                    _ctrl = new controller();
                    _ctrl.Show();
                    statusBar.Text = "connected";
                }
                else
                {
                    _srl.Close();
                    _ctrl.Close();
                    statusBar.Text = "disconnected";
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }
    }
}
