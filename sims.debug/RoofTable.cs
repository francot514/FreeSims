using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace sims.debug
{

     [XmlRoot("Roofs")]
    public class RoofTable
    {

        [XmlArrayItem("R")]
         public List<RoofItem> Items { get; set; }

        public static void Save(string xmlFilePath, RoofTable table)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(RoofTable));



            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, table);
            }
        }


    }


     public class RoofItem
     {
         [XmlAttribute("id")]
         public uint ID;

         [XmlAttribute("n")]
         public string Name;

         [XmlAttribute("s")]
         public sbyte Category;


         [XmlAttribute("p")]
         public uint Price;

     }
}
