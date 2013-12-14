using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bt_light_framework
{
    class Package
    {
        public Header PackageHeader
        {
            get
            {
                return (Header)PackageFrames[0];
            }
            set
            {
                PackageFrames[0] = (Header)value;
            }
        }
        public Frame[] PackageFrames;
        public Package(int length)
        {
            PackageFrames = new Frame[length + 1];
            for (int i = 0; i < PackageFrames.Length; i++ )
            {
                PackageFrames[i] = new Frame();
            }
            PackageHeader = new Header();
            PackageHeader.PackageLength = length + 1;
            PackageFrames[0] = PackageHeader;
        }

        public Package(byte[] package)
        {
            PackageFrames = new Frame[package.Length / Frame.Length];
            PackageFrames[0] = new Header();
            Array.Copy(package, 0 * Frame.Length, PackageFrames[0].ContentByte, 0, Frame.Length);
            for (int i = 1; i < PackageFrames.Length; i++)
            {
                PackageFrames[i] = new Frame();
                Array.Copy(package, i * Frame.Length, PackageFrames[i].ContentByte, 0, Frame.Length);
            }
        }

        public byte[] GetPackageBytes()
        {
            byte[] outBytes = new byte[PackageFrames.Length * 4];
            int bCounter = 0;
            for (int fCount = 0; fCount < PackageFrames.Length; fCount++)
            {
                for (int bCount = 0; bCount < Frame.Length; bCount++)
                {
                    outBytes[bCounter] = PackageFrames[fCount].ContentByte[bCount];
                    bCounter++;
                }
            }
            return outBytes;
        }
    }
}
