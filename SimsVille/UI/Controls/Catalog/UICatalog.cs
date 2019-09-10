/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Framework;
using System.Xml;
using System.IO;
using FSO.Content;
using Microsoft.Xna.Framework.Graphics;
using FSO.Files.Formats.IFF.Chunks;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Panels.LotControls;
using FSO.Common;
using static FSO.Content.WorldObjectCatalog;

namespace FSO.Client.UI.Controls.Catalog
{
    public class UICatalog : UIContainer
    {
        private int Page;
        private int _Budget;
        public int Budget
        {
            get { return _Budget; }
            set {
                if (value != _Budget)
                {
                    if (CatalogItems != null)
                    {
                        for (int i = 0; i < CatalogItems.Length; i++)
                        {
                            CatalogItems[i].SetDisabled(CatalogItems[i].Info.Price > value);
                        }
                    }
                    _Budget = value;
                }
            }
        }
        private static List<UICatalogElement>[] _Catalog;
        public event CatalogSelectionChangeDelegate OnSelectionChange;

        public static List<UICatalogElement>[] Catalog {
            get
            {
                if (_Catalog != null) return _Catalog;
                else
                {
                    //load and build catalog
                    _Catalog = new List<UICatalogElement>[30];
                    for (int i = 0; i < 30; i++) _Catalog[i] = new List<UICatalogElement>();

                    var packingslip = new XmlDocument();
                    
                    packingslip.Load("Content/catalog.xml");
                    var objectInfos = packingslip.GetElementsByTagName("P");

                    foreach (XmlNode objectInfo in objectInfos)
                    {
                        sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                        if (Category < 0) continue;
                        _Catalog[Category].Add(new UICatalogElement()
                        {
                            GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                            Category = Category,
                            Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                            Name = objectInfo.Attributes["n"].Value
                        });
                    }


                    if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack"))
                    {

                        var ep1packingslip = new XmlDocument();

                        ep1packingslip.Load("Content/ep1.xml");
                        var ep1objectInfos = ep1packingslip.GetElementsByTagName("P");

                        foreach (XmlNode objectInfo in ep1objectInfos)
                        {
                            sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
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
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
                        }

                    }

                    if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/Downloads"))
                    {

                        var dpackingslip = new XmlDocument();

                        dpackingslip.Load("Content/downloads.xml");
                        var dobjectInfos = dpackingslip.GetElementsByTagName("P");

                        foreach (XmlNode objectInfo in dobjectInfos)
                        {
                            sbyte Category = Convert.ToSByte(objectInfo.Attributes["s"].Value);
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16),
                                Category = Category,
                                Price = Convert.ToUInt32(objectInfo.Attributes["p"].Value),
                                Name = objectInfo.Attributes["n"].Value
                            });
                        }

                    }

                    AddWallpapers();
                    AddFloors();
                    AddRoofs();

                    for (int i = 0; i < 30; i++) _Catalog[i].Sort(new CatalogSorter());

                    AddWallStyles();

                    return _Catalog;
                }
            }
        }

        private static void AddWallpapers()
        {
            var res = new UICatalogWallpaperResProvider();

            var walls = Content.Content.Get().WorldWalls.List();

            for (int i = 0; i < walls.Count; i++)
            {
                var wall = (WallReference)walls[i];
                _Catalog[8].Insert(0, new UICatalogElement
                {
                    Name = wall.Name,
                    Category = 8,
                    Price = (uint)wall.Price,
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPainter),
                        ResID = wall.ID,
                        Res = res,
                        Parameters = new List<int> { (int)wall.ID } //pattern
                    }
                });
            }
        }

        private static void AddFloors()
        {
            var res = new UICatalogFloorResProvider();

            var floors = Content.Content.Get().WorldFloors.List();

            for (int i = 0; i < floors.Count; i++)
            {
                var floor = (FloorReference)floors[i];
                sbyte category = (sbyte)((floor.ID >= 65534)?5:9);
                _Catalog[category].Insert(0, new UICatalogElement
                {
                    Name = floor.Name,
                    Category = category,
                    Price = (uint)floor.Price,
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIFloorPainter),
                        ResID = floor.ID,
                        Res = res,
                        Parameters = new List<int> { (int)floor.ID } //pattern
                    }
                });
            }
        }

        private static void AddRoofs()
        {
            var res = new UICatalogRoofResProvider();

            var total = Content.Content.Get().WorldRoofs.Count;

            for (int i = 0; i < total; i++)
            {
                sbyte category = 6;
                _Catalog[category].Insert(0, new UICatalogElement
                {
                    Name = "",
                    Category = category,
                    Price = 0,
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIRoofer),
                        ResID = (uint)i,
                        Res = res,
                        Parameters = new List<int> { i } //pattern
                    }
                });
            }
        }

        private static void AddWallStyles()
        {
            var res = new UICatalogWallResProvider();

            for (int i = 0; i < WallStyleIDs.Length; i++)
            {
                var walls = Content.Content.Get().WorldWalls;
                var style = walls.GetWallStyle((ulong)WallStyleIDs[i]);
                _Catalog[7].Insert(0, new UICatalogElement
                {
                    Name = style.Name,
                    Category = 7,
                    Price = (uint)style.Price,
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIWallPlacer),
                        ResID = (ulong)WallStyleIDs[i],
                        Res = res,
                        Parameters = new List<int> { WallStylePatterns[i], WallStyleIDs[i] } //pattern, style
                    }
                });
            }
        }

        public static short[] WallStyleIDs =
        {
            0x1, //wall
            0x2, //picket fence
            0xD, //iron fence
            0xC, //privacy fence
            0xE //banisters
        };

        public static short[] WallStylePatterns =
        {
            0, //wall
            248, //picket fence
            250, //iron fence
            249, //privacy fence
            251, //banisters
        };

        private int PageSize;
        private List<UICatalogElement> Selected;
        private UICatalogItem[] CatalogItems;
        private Dictionary<uint, Texture2D> IconCache;

        public UICatalog(int pageSize)
        {
            IconCache = new Dictionary<uint, Texture2D>();
            PageSize = pageSize;
        }

        public void SetActive(int selection, bool active) {
            int index = selection - Page * PageSize;
            if (index >= 0 && index < CatalogItems.Length) CatalogItems[index].SetActive(active);
        }

        public void SetCategory(List<UICatalogElement> select) {
            Selected = select;
            SetPage(0);
        }

        public int TotalPages()
        {
            if (Selected == null) return 0;
            return ((Selected.Count-1) / PageSize)+1;
        }

        public int GetPage()
        {
            return Page;
        }

        public void SetPage(int page) {
            if (CatalogItems != null)
            {
                for (int i = 0; i < CatalogItems.Length; i++)
                {
                    this.Remove(CatalogItems[i]);
                }
            }

            int index = page*PageSize;
            CatalogItems = new UICatalogItem[Math.Min(PageSize, Math.Max(Selected.Count-index, 0))];
            int halfPage = PageSize / 2;
            
            for (int i=0; i<CatalogItems.Length; i++) {
                var elem = new UICatalogItem(false);
                elem.Index = index;
                elem.Info = Selected[index++];
                elem.Icon = (elem.Info.Special != null)?elem.Info.Special.Res.GetIcon(elem.Info.Special.ResID):GetObjIcon(elem.Info.GUID);
                elem.Tooltip = "$"+elem.Info.Price.ToString();
                elem.X = (i % halfPage) * 45 + 2;
                elem.Y = (i / halfPage) * 45 + 2;
                elem.OnMouseEvent += new ButtonClickDelegate(InnerSelect);
                elem.SetDisabled(elem.Info.Price > Budget);
                CatalogItems[i] = elem;
                this.Add(elem);
            }
            Page = page;
        }

        void InnerSelect(UIElement button)
        {
            if (OnSelectionChange != null) OnSelectionChange(((UICatalogItem)button).Index);
        }

        public Texture2D GetObjIcon(uint GUID)
        {
            if (!IconCache.ContainsKey(GUID)) {
                var obj = Content.Content.Get().WorldObjects.Get(GUID);
                if (obj == null)
                {
                    IconCache[GUID] = null;
                    return null;
                }
                var bmp = obj.Resource.Get<BMP>(obj.OBJ.CatalogStringsID);
                if (bmp != null) IconCache[GUID] = bmp.GetTexture(GameFacade.GraphicsDevice);
                else IconCache[GUID] = null;
            }
            return IconCache[GUID];
        }

        private class CatalogSorter : IComparer<UICatalogElement>
        {
            #region IComparer<UICatalogElement> Members

            public int Compare(UICatalogElement x, UICatalogElement y)
            {
                if (x.Price > y.Price) return 1;
                else if (x.Price < y.Price) return -1;
                else return 0;
            }

            #endregion
        }
    }

    public delegate void CatalogSelectionChangeDelegate(int selection);

    public struct UICatalogElement {
        public ObjectCatalogItem Item;
        public uint GUID;
        public sbyte Category;
        public uint Price;
        public string Name;
        public byte DisableLevel; //1 = only shopping, 2 = rare (unsellable?)
        public UISpecialCatalogElement Special;
    }

    public class UISpecialCatalogElement
    {
        public Type Control;
        public ulong ResID;
        public UICatalogResProvider Res;
        public List<int> Parameters;
    }
}
