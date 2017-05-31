using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace tso.world.Model
{
    [XmlRoot("city")]
    public class XmlCity
    {

        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("lots")]
        [XmlArrayItem("lot")]
        public List<XmlLot> Lots { get; set; }

        public static XmlCity Parse(string xmlFilePath)
        {
            using (var reader = File.OpenRead(xmlFilePath))
            {
                return Parse(reader);
            }
        }

        public static XmlCity Parse(Stream reader)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlCity));
            return (XmlCity)serialize.Deserialize(reader);
        }

        public static void Save(string xmlFilePath, XmlCity data)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlCity));

            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, data);
            }
        }

    }

    public class XmlLot
    {

        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("cost")]
        public int Cost { get; set; }

        [XmlElement("x")]
        public int X { get; set; }

        [XmlElement("y")]
        public int Y { get; set; }

        [XmlElement("flags")]
        public int Flags { get; set; }

    }

}
