using System;
using System.Text;

namespace bt_light_framework
{
    public class Header : Frame
    {
        public int ProtocolVersion 
        {
            get
            {
                return this.ContentByte[0];
            }
            set
            {
                this.ContentByte[0] = (byte)value;
            }
        }

        public int PackageLength
        {
            get
            {
                return this.ContentByte[Frame.Length - 1];
            }
            set
            {
                this.ContentByte[Frame.Length - 1] = (byte)value;
            }
        }

        public Header(int protocolVersion = 0)
        {
            this.ProtocolVersion = protocolVersion;
        }
    }
}
