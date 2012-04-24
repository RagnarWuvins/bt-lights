using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace WindowsFormsApplication1
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
                ports.SelectedItem = "COM22";
            }
            catch
            {
                Console.WriteLine("COM22 couldn't be found");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_srl.IsOpen)
                {
                    _srl.BaudRate = 38400;
                    _srl.PortName = ports.SelectedItem.ToString();
                    _srl.Open();
                    _ctrl = new controller(_srl);
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
