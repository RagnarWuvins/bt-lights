using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bt_light_framework
{
    class Program
    {
        static void Main(string[] args)
        {
            Package p = new Package(4);
            byte[] r = p.GetPackageBytes();
            byte[] b = { 10, 25, 30, 40, 50, 60, 70, 80};
            p = new Package(b);
            p.PackageFrames[1] = new Frame();
        }
    }
}
