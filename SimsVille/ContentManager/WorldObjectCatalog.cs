using FSO.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace FSO.Content
{
    public class WorldObjectCatalog
    {
        private static List<ObjectCatalogItem>[] ItemsByCategory;
        private static Dictionary<uint, ObjectCatalogItem> ItemsByGUID;

        public void Init(Content content)
        {
            //load and build catalog
            ItemsByGUID = new Dictionary<uint, ObjectCatalogItem>();
            ItemsByCategory = new List<ObjectCatalogItem>[30];
            for (int i = 0; i < 30; i++) ItemsByCategory[i] = new List<ObjectCatalogItem>();

            var packingslip = new XmlDocument();

            packingslip.Load("Content/catalog.xml");
            var objectInfos = packingslip.GetElementsByTagName("P");

            foreach (XmlNode objectInfo in objectInfos)
            {
                sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                if (Category < 0) continue;
                var item = new ObjectCatalogItem()
                {
                    GUID = guid,
                    Category = Category,
                    Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                    Name = objectInfo.Attributes["n"].Value
                };
                ItemsByCategory[Category].Add(item);
                ItemsByGUID[guid] = item;
            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack"))
            {

                var ep1packingslip = new XmlDocument();

                ep1packingslip.Load("Content/ep1.xml");
                var ep1objectInfos = ep1packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep1objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack2"))
            {

                var ep2packingslip = new XmlDocument();

                ep2packingslip.Load("Content/ep2.xml");
                var ep2objectInfos = ep2packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep2objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack3"))
            {

                var ep3packingslip = new XmlDocument();

                ep3packingslip.Load("Content/ep3.xml");
                var ep3objectInfos = ep3packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep3objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack4"))
            {

                var ep4packingslip = new XmlDocument();

                ep4packingslip.Load("Content/ep4.xml");
                var ep4objectInfos = ep4packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep4objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                     var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                     ItemsByCategory[Category].Add(item);
                     ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack5"))
            {

                var ep5packingslip = new XmlDocument();

                ep5packingslip.Load("Content/ep5.xml");
                var ep5objectInfos = ep5packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep5objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack6"))
            {

                var ep6packingslip = new XmlDocument();

                ep6packingslip.Load("Content/ep6.xml");
                var ep6objectInfos = ep6packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep6objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack7"))
            {

                var ep7packingslip = new XmlDocument();

                ep7packingslip.Load("Content/ep7.xml");
                var ep7objectInfos = ep7packingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep7objectInfos)
                {
                    sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint guid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (Category < 0) continue;
                    var item = new ObjectCatalogItem()
                    {
                        GUID = guid,
                        Category = Category,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };
                    ItemsByCategory[Category].Add(item);
                    ItemsByGUID[guid] = item;
                }

            }

            //load and build Content Objects into catalog
            if (File.Exists(Path.Combine(FSOEnvironment.ContentDir, "Objects/catalog_downloads.xml")))
            {
                var dpackingslip = new XmlDocument();

                dpackingslip.Load(Path.Combine(FSOEnvironment.ContentDir, "Objects/catalog_downloads.xml"));
                var downloadInfos = dpackingslip.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in downloadInfos)
                {
                    sbyte dCategory = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                    uint dguid = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    if (dCategory < 0) continue;
                    var ditem = new ObjectCatalogItem()
                    {
                        GUID = dguid,
                        Category = dCategory,
                        Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                        Name = objectInfo.Attributes["n"].Value
                    };

                    ItemsByCategory[dCategory].Add(ditem);
                    ItemsByGUID[dguid] = ditem;

                }
            }
        }

        public ObjectCatalogItem GetItemByGUID(uint guid)
        {
            ObjectCatalogItem item = null;
            ItemsByGUID.TryGetValue(guid, out item);
            return item;
        }

        public class ObjectCatalogItem
        {
            public uint GUID;
            public sbyte Category;
            public uint Price;
            public string Name;
            public byte DisableLevel; //1 = only shopping, 2 = rare (unsellable?)
        }
    }
}
