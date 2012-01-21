using System;
using Microsoft.SPOT;

namespace NetduinoBT
{
    static class Constants
    {
        public static string[] attest = {
            "at+version?\r\n",
            "at+rmaad\r\n",
            "at+class=240404\r\n",
            "at+role=0\r\n",
            "at+name=MeisterLampe\r\n",
            "at+inqm=1,9,48\r\n"
                                        };
        public static string[] inquiry = {
            "\r\n+STWMOD=0\r\n",
            "\r\n+STBD=38400\r\n",
            "\r\n+STNA=TestModul\r\n",
            "\r\n+STAUTO=0\r\n",
            "\r\n+STOAUT=1\r\n",
            "\r\n+STPIN=0000\r\n",
            "\r\n+RTADDR\r\n",
            "\r\n+INQ=1\r\n"
            };
        public static string[] reset = {
            "at+orgl\r\n",
            "at+class=240404\r\n"
            };
        public static string[] master = {
            "\r\n+STWMOD=1\r\n",
            "\r\n+STBD=38400\r\n",                                    
            "\r\n+STNA=TestModul\r\n",
            "\r\n+STAUTO=0\r\n",
            "\r\n+STOAUT=1\r\n",
            "\r\n+STPIN=0000\r\n",
            "\r\n+RTINQ=90,21,55,59,23,e5"
        };
    }
}
