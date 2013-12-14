using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bt_light_framework
{
    class Header : Frame
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
                return this.ContentByte[3];
            }
            set
            {
                this.ContentByte[3] = (byte)value;
            }
        }

        public Header(int protocolVersion = 0)
        {
            this.ProtocolVersion = protocolVersion;
        }
    }
}
