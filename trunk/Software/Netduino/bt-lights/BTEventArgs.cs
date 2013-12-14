using System;
using Microsoft.SPOT;
using bt_light_framework;

namespace BTLights
{
    /// <summary>
    /// CommandReceived delegate
    /// </summary>
    /// <param name="sender">Sender of the event</param>
    /// <param name="e">BTEventArgs that come with the event</param>
    public delegate void BTEvent(object sender, BTEventArgs e);
    /// <summary>
    /// Event arguments that can be send to other objects
    /// </summary>
    public class BTEventArgs : EventArgs
    {
        public Package Data = null;
        /// <summary> Raw byte array that contains an command </summary>
        public byte[] CommandRaw = {0};
        /// <summary>Holds the class of the command</summary>
        public int CommandClass = 0;
        /// <summary>Holds the mode of the command</summary>
        public int CommandMode = 0;
        /// <summary>Holds the address of the command</summary>
        public int CommandAddress = 0;
        /// <summary>Value of the command</summary>
        public int CommandValue = 0;
        /// <summary>Checksum of the command</summary>
        public int CommandChecksum = 0;
    }
}
