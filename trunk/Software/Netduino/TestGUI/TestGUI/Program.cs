using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using BTLights;

namespace BluetoothLights
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (true)
            {
                SerialPort srl = new SerialPort("COM22", Constants.BAUDRATE);
                srl.NewLine = "\r\n";
                Application.Run(new controller(srl));
            }
            else
            {
                Application.Run(new Form1());
            }

        }
    }
}
