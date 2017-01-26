using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;
using System.Xml;
using System.IO;
using TSO.Content;
using Microsoft.Xna.Framework.Graphics;
using TSO.Files.formats.iff.chunks;
using TSOVille.LUI;
using TSOVille.Code.UI.Panels.LotControls;
using SimsHomeMaker;
using SimsHomeMaker.ContentManager;
using TSO.Files.formats.iff;

namespace TSOVille.Code.UI.Controls.Catalog
{
    public class UICatalog : UIContainer
    {
        private int Page;
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

                    if (Directory.Exists("GameData"))
                    {
                        var packingslip = new XmlDocument();

                        packingslip.Load("Content\\objects.xml");
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

                    }


                    if (Directory.Exists("ExpansionPack"))
                    {

                        var ep1packingslip = new XmlDocument();

                        ep1packingslip.Load("Content\\ep1.xml");
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

                    if (Directory.Exists("ExpansionPack2"))
                    {

                        var ep2packingslip = new XmlDocument();

                        ep2packingslip.Load("Content\\ep2.xml");
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

                    if (Directory.Exists("ExpansionPack3"))
                    {

                        var ep3packingslip = new XmlDocument();

                        ep3packingslip.Load("Content\\ep3.xml");
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

                    if (Directory.Exists("ExpansionPack4"))
                    {

                        var ep4packingslip = new XmlDocument();

                        ep4packingslip.Load("Content\\ep4.xml");
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

                    DirectoryInfo dir = new DirectoryInfo("Downloads");
                    foreach (FileInfo file in dir.GetFiles())
                        if (file.Extension == ".iff")
                        {
                            var iff = new Iff(file.FullName);
                            sbyte Category = 18;
                            if (Category < 0) continue;
                            _Catalog[Category].Add(new UICatalogElement()
                            {
                                GUID = iff.List<OBJD>()[0].GUID,
                                Category = Category,
                                Price = iff.List<OBJD>()[0].Price,
                                Name = iff.List<CTSS>()[0].Strings[0].Value
                            });


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

            var walls = Content.Get().WorldWalls.List();

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

            var floors = Content.Get().WorldFloors.List();

            sbyte category = 9;


            for (int i = 0; i < floors.Count - 1; i++)
            {
                var floor = (FloorReference)floors[i];
                _Catalog[9].Insert(0, new UICatalogElement
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

            var pool = (FloorReference)floors[floors.Count - 1];
            _Catalog[5].Insert(0, new UICatalogElement
            {
                Name = pool.Name,
                Category = 5,
                Price = (uint)pool.Price,
                Special = new UISpecialCatalogElement
                {
                    Control = typeof(UIFloorPainter),
                    ResID = pool.ID,
                    Res = res,
                    Parameters = new List<int> { (int)pool.ID } //pattern
                }
            });

        }

        private static void AddWallStyles()
        {
            var res = new UICatalogWallResProvider();

            for (int i = 0; i < WallStyleIDs.Length; i++)
            {
                var walls = Content.Get().WorldWalls;
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

        private static void AddRoofs()
         {
            var res = new UICatalogRoofResProvider();

            var roofs = Content.Get().WorldRoofs.List();

            sbyte category = 6;


            for (int i = 0; i < roofs.Count - 1; i++)
            {
                var roof = (RoofReference)roofs[i];
                _Catalog[6].Insert(0, new UICatalogElement
                {
                    Name = roof.Name,
                    Category = category,
                    Price = (uint)roof.Price,
                    Special = new UISpecialCatalogElement
                    {
                        Control = typeof(UIRoofPainter),
                        ResID = roof.ID,
                        Res = res,
                        Parameters = new List<int> { (int)roof.ID } //pattern
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
                var obj = Content.Get().WorldObjects.Get(GUID);
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
        public uint GUID;
        public sbyte Category;
        public uint Price;
        public string Name;
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
