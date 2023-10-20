using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace sims.debug
{

    [XmlRoot("ObjectInfoTable")]
    public class InfoTable
    {

        [XmlArrayItem("I")]
        public List<TableItem> Items { get; set; }

        public static void Save(string xmlFilePath, InfoTable table)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(InfoTable));



            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, table);
            }
        }
    }


    public class TableItem
    {
        [XmlAttribute("g")]
        public string GUID;

        [XmlAttribute("n")]
        public string FileName;

        [XmlAttribute("o")]
        public string Name;

        [XmlAttribute("m")]
        public string Group;

        [XmlAttribute("i")]
        public string SubIndex;

        [XmlAttribute("l")]
        public string Layer;

    }

    

}
