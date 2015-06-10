using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace EnrrVa.Common
{
    public class DeserialzerXml
    {
        public static string getXml()
        {
            string st = "";

            XmlTextReader reader = new XmlTextReader("C:\\Dev\\1622.2\\output060915.xml");

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        st += "<" + reader.Name;
                        st +=  "> \n";
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        st += reader.Value + "\n";
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        st += "</" + reader.Name;
                        st += ">" + "\n";
                        break;
                }
            }

            return st;
        }
            

    }

    

}
