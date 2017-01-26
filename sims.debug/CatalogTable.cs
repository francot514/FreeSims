using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace sims.debug
{
    [XmlRoot("Catalog")]
    public class CatalogTable
    {

        [XmlArrayItem("P")]
        public List<CatalogItem> Items { get; set; }

        public static void Save(string xmlFilePath, CatalogTable table)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(CatalogTable));



            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, table);
            }
        }

    }

    public class CatalogItem
    {
        [XmlAttribute("g")]
        public string GUID;

        [XmlAttribute("n")]
        public string Name;

        [XmlAttribute("s")]
        public sbyte Category;


        [XmlAttribute("p")]
        public uint Price;

    }

}
