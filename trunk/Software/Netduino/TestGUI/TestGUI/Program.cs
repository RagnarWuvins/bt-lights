﻿using System;
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

            Constants constClass = new Constants();
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Constants));

            System.IO.StreamWriter file = new System.IO.StreamWriter(
                @"c:\temp\SerializationOverview.xml");
            writer.Serialize(file, constClass);
            file.Close();

            if (false)
            {
                SerialPort srl = new SerialPort("COM31", Constants.BAUDRATE);
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