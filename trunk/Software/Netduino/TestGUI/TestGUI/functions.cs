using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using BTLights;
using System.Reflection;
using System.Xml;

namespace BluetoothLights
{
    public class functions
    {
        public static double GetMax(double[] DoubleCollection)
        {
            double max = double.MinValue;
            foreach (double i in DoubleCollection)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            return max;
        }

        public static int GetMax(int[] DoubleCollection)
        {
            int max = int.MinValue;
            foreach (int i in DoubleCollection)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            return max;
        }

        public static string Const2XML()
        {
            string outpath = "../../../../../Android/res/xml/constants.xml";
            string ConstPath = "Constants.cs";
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode, subNode, enumNode, typeNode;
            rootNode = doc.CreateElement("root");
            enumNode = doc.CreateElement("enum");
            doc.AppendChild(rootNode);            
            int counter = 0;
            string line;
            int type = 0;
            int enumCounter = 0;
            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(ConstPath);
            while ((line = file.ReadLine()) != null)
            {     
                
                if (line.Contains("public enum"))
                {
                    type = 1; 
                    enumNode = doc.CreateElement("enum");
                    enumCounter = 0;                    
                    string[] temp = line.Split(' ');
                    typeNode = doc.CreateAttribute("type");
                    typeNode.Value = temp[temp.Length - 1];
                    enumNode.Attributes.SetNamedItem(typeNode);
                    rootNode.AppendChild(enumNode);
                    line = file.ReadLine(); // skip {
                    line = file.ReadLine(); // first of enum                    
                    
                }
                else if (line.Contains(" = "))
                {
                    type = 2;
                }
                else if (line.Contains("}"))
                {
                    type = 0;
                }
                if (line.Contains(" = {"))
                {
                    type = 0;
                }
                switch (type)
                {
                    case 1:
                        if (line.Trim().StartsWith("//"))
                        {
                            break;
                        }
                        string[] temp = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        subNode = doc.CreateElement("parameter");
                        XmlNode nameNode = doc.CreateAttribute("name");
                        nameNode.Value = temp[0].Trim();
                        XmlNode valueNode = doc.CreateAttribute("value");
                        valueNode.Value = string.Format("0x{0:X}", enumCounter);
                        subNode.Attributes.SetNamedItem(nameNode);
                        subNode.Attributes.SetNamedItem(valueNode);
                        enumNode.AppendChild(subNode);
                        enumCounter++;
                        break;
                    case 2:
                        string[] tempEq = line.Split(new string[] {" "},StringSplitOptions.RemoveEmptyEntries);
                        int index = Array.IndexOf(tempEq, "=");
                        subNode = doc.CreateElement("parameter");
                        XmlNode nameNodeEq = doc.CreateAttribute("name");
                        nameNodeEq.Value = tempEq[index-1].Trim();
                        XmlNode valueNodeEq = doc.CreateAttribute("value");
                        valueNodeEq.Value = tempEq[index + 1].TrimEnd(';');
                        subNode.Attributes.SetNamedItem(nameNodeEq);
                        subNode.Attributes.SetNamedItem(valueNodeEq);
                        rootNode.AppendChild(subNode);
                        type = 0;
                        break;
                    default:
                        break;
                }
                
                counter++;
            }

            file.Close();
            doc.Save(outpath);
            return outpath;
        }
    }



    public static class StreamReaderExtension
    {
        public static IEnumerable<string> Lines(this StreamReader source)
        {
            String line;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            while ((line = source.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
