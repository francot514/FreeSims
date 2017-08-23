using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace tso.world.Model
{
    [XmlRoot("character")]
    public class XmlCharacter
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("objID")]
        public string ObjID { get; set; }

        [XmlElement("gender")]
        public string Gender { get; set; }

        [XmlElement("head")]
        public string Head { get; set; }

        [XmlElement("body")]
        public string Body { get; set; }

        [XmlElement("appearance")]
        public string Appearance { get; set; }

        public static XmlCharacter Parse(string xmlFilePath)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlCharacter));

            using (var reader = File.OpenRead(xmlFilePath))
            {
                return (XmlCharacter)serialize.Deserialize(reader);
            }
        }


        public static void Save(string xmlFilePath, XmlCharacter data)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlCharacter));

            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, data);
            }
        }
    }
}
