using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace tso.world.model
{
    [XmlRoot("pet")]
    public class XmlPet
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("objID")]
        public string ObjID { get; set; }

        [XmlElement("gender")]
        public string Gender { get; set; }

        [XmlElement("body")]
        public string Body { get; set; }


        public static XmlPet Parse(string xmlFilePath)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlPet));

            using (var reader = File.OpenRead(xmlFilePath))
            {
                return (XmlPet)serialize.Deserialize(reader);
            }
        }


        public static void Save(string xmlFilePath, XmlPet data)
        {
            XmlSerializer serialize = new XmlSerializer(typeof(XmlPet));

            using (var writer = new StreamWriter(xmlFilePath))
            {
                serialize.Serialize(writer, data);
            }
        }
    }
}
