using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Framework.Parser;
using FSO.Client.UI.Model;
using FSO.Client.UI.Screens;
using FSO.Client.Utils;
using FSO.Common;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.HIT;
using FSO.SimAntics;
using FSO.SimAntics.Entities;
using FSO.SimAntics.Model;
using FSO.SimAntics.Model.TSOPlatform;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.Vitaboy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.HIT;

namespace FSO.Client.UI.Panels
{
    /// <summary>
    /// House Panel. Works very similarly to Options Panel in that it uses sub-panels chosen by which button you press in this one.
    /// Requires a LotController for obvious reasons, like build/buy.
    /// </summary>
    public class UIHouseMode : UIDestroyablePanel
    {
        public Texture2D DividerImage { get; set; }

        public UIButton HouseInfoButton { get; set; }
        public UIButton StatisticsButton { get; set; }
        public UIButton RoommatesButton { get; set; }
        public UIButton LogButton { get; set; }
        public UIButton AdmitBanButton { get; set; }
        public UIButton EnvironmentButton { get; set; }
        public UIButton BillsButton { get; set; }
        public UIButton LotResizeButton { get; set; }

        private Dictionary<UIButton, int> BtnToMode;

        private UIContainer Panel;
        private int CurrentPanel;

        public UIImage Background;
        public UIImage Divider;

        public UILotControl LotControl;
        public UIHouseMode(UILotControl lotController)
        {
            var useSmall = true;  //(FSOEnvironment.UIZoomFactor > 1f || GlobalSettings.Default.GraphicsWidth < 1024);
            var script = this.RenderScript("housepanel.uis");

            Background = new UIImage(GetTexture(useSmall ? (ulong)0x000000D800000002 : (ulong)0x0000018300000002));
            Background.Y = 9;
            Background.BlockInput();
            this.AddAt(0, Background);

            Size = Background.Size.ToVector2()+new Vector2(0, 9);

            Divider = script.Create<UIImage>("Divider");
            Divider.Texture = DividerImage;
            this.Add(Divider);

            BtnToMode = new Dictionary<UIButton, int>()
            {
                { HouseInfoButton, 0 },
                { StatisticsButton, 1 },
                { RoommatesButton, 2 },
                { LogButton, 3 },
                { AdmitBanButton, 4 },
                { EnvironmentButton, 5 },
                { BillsButton, 6 },
                { LotResizeButton, 7 }
            };

            StatisticsButton.Disabled = true;
            BillsButton.Disabled = true;
            LogButton.Disabled = true;

            foreach (var btn in BtnToMode.Keys)
                btn.OnButtonClick += SetMode;

            CurrentPanel = -1;
            LotControl = lotController;
        }

        private void SetMode(Framework.UIElement button)
        {
            if (button == HouseInfoButton)
            {
                
                
            }

            var btn = (UIButton)button;
            int newPanel = -1;
            BtnToMode.TryGetValue(btn, out newPanel);

            foreach (var ui in BtnToMode.Keys)
                ui.Selected = false;

            if (CurrentPanel != -1)
            {
                if (Panel is IDisposable)
                    ((IDisposable)Panel)?.Dispose();
                if (Panel is UIDialog)
                    UIScreen.RemoveDialog(Panel);
                this.Remove(Panel);
            }
            if (newPanel != CurrentPanel)
            {
                btn.Selected = true;
                switch (newPanel)
                {
                    case 1:
                        Panel = new UIStatsPanel(LotControl);
                        break;
                    case 2:
                        //Panel = new Panels.Neighborhoods.UIManageDonatorDialog(LotControl);
                        break;
                    case 3:
                        Panel = new UILogPanel(LotControl);
                        break;
                    case 4:
                        Panel = new UIAdmitBanPanel(LotControl);
                        //var ctr = ControllerUtils.BindController<LotAdmitController>(Panel);
                        break;
                    case 5:
                        Panel = new UIStatsPanel(LotControl);
                        Panel.X = 232;
                        Panel.Y = 0;
                        break;
                    case 7:
                        Panel = new UIBuildableAreaPanel(LotControl);
                        break;
                    default:
                        btn.Selected = false;
                        break;
                }
                if (Panel != null)
                {
                    if (Panel is UIDialog)
                    {
                        UIScreen.ShowDialog(Panel, false);
                    }
                    else
                    {
                        if (newPanel != 5)
                        {
                            Panel.X = 225; //TODO: use uiscript positions
                            Panel.Y = 9;
                        }
                        this.Add(Panel);
                    }
                    CurrentPanel = newPanel;
                }
            }
            else
            {
                CurrentPanel = -1;
            }
        }

        public override void Destroy()
        {
            if (Panel is UIBuildableAreaPanel)
                ((UIBuildableAreaPanel)Panel)?.Dispose();
            base.Remove(this);
        }
    }

    // NOTE: See UIEnvPanel.cs for the EnvPanel and its subpanels.

    /// <summary>
    /// Same as TS1 house stats. TODO: make freeso calculate these values.
    /// </summary>
    public class UIStatsPanel : UIContainer
    {
        public UILabel TitleLabel { get; set; }
        public UILabel AreaLabel { get; set; }
        public UILabel BedroomsLabel { get; set; }
        public UILabel BathroomsLabel { get; set; }
        public UILabel FloorsLabel { get; set; }
        public UILabel LotSizeLabel { get; set; }
        public UILabel AreaValue { get; set; }
        public UILabel BedroomsValue { get; set; }
        public UILabel BathroomsValue { get; set; }
        public UILabel FloorsValue { get; set; }
        public UILabel LotSizeValue { get; set; }
        public UILabel SizeLabel { get; set; }
        public UILabel FurnishingsLabel { get; set; }
        public UILabel YardLabel { get; set; }
        public UILabel UpkeepLabel { get; set; }
        public UILabel LayoutLabel { get; set; }
        public UIProgressBar SizeProgress { get; set; }
        public UIProgressBar FurnishingsProgress { get; set; }
        public UIProgressBar YardProgress { get; set; }
        public UIProgressBar UpkeepProgress { get; set; }
        public UIProgressBar LayoutProgress { get; set; }

        private UILotControl LotControl;
        private UIScript Script;
        private int RefreshTicks;
        private UIStatsBar[] Bars;

        public UIStatsPanel(UILotControl lotController)
        {
            LotControl = lotController;
            Script = this.RenderScript("statisticspanel.uis");

            //the uis alignments (1 left, 5 right) are not mapped by the parser - set directly
            foreach (var lbl in new[] { AreaLabel, BedroomsLabel, BathroomsLabel, FloorsLabel, LotSizeLabel,
                SizeLabel, FurnishingsLabel, YardLabel, UpkeepLabel, LayoutLabel })
                lbl.Alignment = TextAlignment.Right | TextAlignment.Middle;
            foreach (var val in new[] { AreaValue, BedroomsValue, BathroomsValue, FloorsValue, LotSizeValue })
                val.Alignment = TextAlignment.Left | TextAlignment.Middle;
            TitleLabel.Alignment = TextAlignment.Left | TextAlignment.Middle;

            //the script-made progress bars nine-slice EA's tiny bar art into a mess - swap in
            //plain clipped-fill bars using the same textures
            Bars = new UIStatsBar[5];
            var scriptBars = new[] { SizeProgress, FurnishingsProgress, YardProgress, UpkeepProgress, LayoutProgress };
            for (int i = 0; i < 5; i++)
            {
                scriptBars[i].Visible = false;
                Bars[i] = new UIStatsBar(scriptBars[i].Background, scriptBars[i].Bar);
                Bars[i].Position = scriptBars[i].Position;
                Add(Bars[i]);
            }

            Refresh();
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (++RefreshTicks >= FSOEnvironment.RefreshRate * 5)
            {
                RefreshTicks = 0;
                Refresh();
            }
        }

        private void Refresh()
        {
            var vm = LotControl.vm;
            if (vm?.Context?.RoomInfo == null) return;
            var arch = vm.Context.Architecture;

            int interiorArea = 0, insideRooms = 0, bedrooms = 0, bathrooms = 0, reasonableRooms = 0;
            var seenRooms = new HashSet<ushort>();
            foreach (var info in vm.Context.RoomInfo)
            {
                var room = info.Room;
                if (room.IsOutside || room.IsPool || room.Area == 0 || !seenRooms.Add(room.RoomID)) continue;
                insideRooms++;
                interiorArea += room.Area;
                if (room.Area >= 9 && room.Area <= 120) reasonableRooms++;
                if (info.Entities != null)
                {
                    if (info.Entities.Any(e => e.SemiGlobal?.Iff?.Filename == "bedsemiglobal.iff")) bedrooms++;
                    if (info.Entities.Any(e => e.SemiGlobal?.Iff?.Filename == "toiletsemiglobal.iff")) bathrooms++;
                }
            }

            var lotSize = vm.TSOState.Size & 255;
            var lotFloors = ((vm.TSOState.Size >> 8) & 255) + 2; //stored as extra floors above the base 2
            var buildableTiles = Math.Max(1, arch.BuildableArea.Width * arch.BuildableArea.Height);

            //user object groups: value totals for furnishings/yard/upkeep
            var groups = new HashSet<VMMultitileGroup>();
            foreach (var ent in vm.Entities)
            {
                if (ent is VMGameObject && ent.PersistID != 0 && ent.MultitileGroup != null) groups.Add(ent.MultitileGroup);
            }
            long objValue = 0, outdoorValue = 0, newValue = 0;
            foreach (var group in groups)
            {
                var baseObj = group.BaseObject;
                var basePrice = (baseObj == null) ? 0 : BasePrice(group, baseObj);
                if (basePrice <= 0) continue;
                var wear = Math.Min(400, (int)((baseObj as VMGameObject)?.TSOState?.Budget.Value ?? (20 * 4)));
                var price = (basePrice * (400 - wear)) / 400;
                newValue += basePrice;
                objValue += price;
                var room = vm.Context.GetObjectRoom(baseObj);
                if (room < vm.Context.RoomInfo.Length && vm.Context.RoomInfo[room].Room.IsOutside) outdoorValue += price;
            }

            AreaValue.Caption = interiorArea.ToString();
            BedroomsValue.Caption = bedrooms.ToString();
            BathroomsValue.Caption = bathrooms.ToString();
            FloorsValue.Caption = lotFloors.ToString();
            LotSizeValue.Caption = (string)Script[(lotSize <= 1) ? "SmallLotSizeText" : ((lotSize <= 3) ? "MediumLotSizeText" : "LargeLotSizeText")];

            //DiscoSO evaluators, 0-10, absolute scales so small properties read low
            Bars[0].Value = Math.Min(10f, interiorArea / 40f); //Size: 400 interior tiles = max
            Bars[1].Value = Math.Min(10f, objValue / (interiorArea > 0 ? interiorArea * 50f : 5000f)); //Furnishings
            Bars[2].Value = Math.Min(10f, outdoorValue / (buildableTiles * 5f)); //Yard
            Bars[3].Value = (newValue > 0) ? (objValue * 10f) / newValue : 10f; //Upkeep (wear)
            Bars[4].Value = Math.Min(10f, reasonableRooms * 2f); //Layout: five well-sized rooms = max
        }

        /// <summary>
        /// What an object is worth before wear. Donating one zeroes its sellback price outright,
        /// and everything on a community lot is donated - fall back to the catalog so the stats
        /// read the furniture that's there rather than what it would fetch resold.
        /// </summary>
        private static int BasePrice(VMMultitileGroup group, VMEntity baseObj)
        {
            if (group.Price > 0) return group.Price;
            var def = baseObj.MasterDefinition ?? baseObj.Object.OBJ;
            return (int)(Content.Content.Get().WorldCatalog.GetItemByGUID(def.GUID)?.Price ?? def.Price);
        }
    }

    public class UIStatsBar : UIElement
    {
        private Texture2D Background;
        private Texture2D Fill;
        public float Value; //0-10

        public UIStatsBar(Texture2D background, Texture2D fill)
        {
            Background = background;
            Fill = fill;
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            if (Background != null)
                DrawLocalTexture(batch, Background, Vector2.Zero);
            if (Fill != null && Value > 0)
            {
                var frac = Math.Min(1f, Value / 10f);
                var src = new Rectangle(0, 0, (int)(Fill.Width * frac), Fill.Height);
                if (src.Width > 0) DrawLocalTexture(batch, Fill, src, Vector2.Zero);
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(0, 0, Background?.Width ?? 0, Background?.Height ?? 0);
        }
    }

    /// <summary>
    /// Set roommate build permissions. Check buttons disabled as anything but owner.
    /// </summary>
    public class UIRoommatesPanel : UIContainer
    {
        public UILabel TitleLabel { get; set; }
        public UIRoommateCheckList RoommateList;
        public Texture2D BuildIconImage { get; set; }

        public HashSet<uint> OldRoommates;
        public HashSet<uint> OldBuilders;
        private UILotControl LotControl;

        public UIRoommatesPanel(UILotControl lotController)
        {
            this.LotControl = lotController;
            var script = this.RenderScript("roommatespanel.uis");
            RoommateList = script.Create<UIRoommateCheckList>("RoommateList");
            RoommateList.OnCheckChange += RoommateList_OnCheckChange;
            Add(RoommateList);
            TitleLabel.CaptionStyle = TitleLabel.CaptionStyle.Clone();
            TitleLabel.CaptionStyle.Shadow = true;
            TitleLabel.Alignment = TextAlignment.Left;
            TitleLabel.Y -= 8;

            var buildico = new UIImage(BuildIconImage);
            buildico.Position = new Vector2(30-18, 30+34+8); //to the left of all the checkboxes
            buildico.Tooltip = GameFacade.Strings.GetString("178", "2");
            UIUtils.GiveTooltip(buildico);
            Add(buildico);
            UpdateList();
        }

        private void RoommateList_OnCheckChange(int index)
        {
            var id = OldRoommates.ElementAt(index);
            var builder = OldBuilders.Contains(id);
            LotControl.vm.SendCommand(new VMChangePermissionsCmd
            {
                TargetUID = id,
                Level = (builder) ? VMTSOAvatarPermissions.Roommate : VMTSOAvatarPermissions.BuildBuyRoommate,
            });
        }

        private void UpdateList()
        {
            var state = LotControl.vm.TSOState;
            RoommateList.UpdateList(state.Roommates, state.BuildRoommates);
            OldRoommates = new HashSet<uint>(state.Roommates);
            OldBuilders = new HashSet<uint>(state.BuildRoommates);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            var lotState = LotControl.vm.TSOState;
            if (!OldRoommates.SetEquals(lotState.Roommates) || !OldBuilders.SetEquals(lotState.BuildRoommates))
            {
                UpdateList();
            }
        }
    }

    public class UIRoommateCheckList : UIContainer
    {
        private List<UIPersonButton> RoommateButtons = new List<UIPersonButton>();
        private List<UIButton> CheckButtons = new List<UIButton>();
        public event Callback<int> OnCheckChange; 
        public bool Disabled = false;
        public UIRoommateCheckList() : base()
        {
        }

        public void UpdateList(HashSet<uint> roommates, HashSet<uint> buildRoommates)
        {
            while (RoommateButtons.Count > roommates.Count)
            {
                Remove(RoommateButtons[RoommateButtons.Count - 1]);
                Remove(CheckButtons[CheckButtons.Count - 1]);
                RoommateButtons.RemoveAt(RoommateButtons.Count - 1);
                CheckButtons.RemoveAt(CheckButtons.Count - 1);
            }
            while (roommates.Count > RoommateButtons.Count)
            {
                var btn = new UIPersonButton();
                btn.FrameSize = UIPersonButtonSize.LARGE;
                btn.X = RoommateButtons.Count * (34 + 6); //6 is gutter size
                Add(btn);
                RoommateButtons.Add(btn);

                var cbt = new UIButton();
                cbt.ImageStates = 6;
                cbt.Texture = GetTexture(0x0000049400000001);
                cbt.X = CheckButtons.Count * (34 + 6) + 9; //6 is gutter size, 10 is margin for checkbutton
                cbt.Y = 34 + 8;
                cbt.OnButtonClick += (e) => { OnCheckChange?.Invoke(CheckButtons.IndexOf((UIButton)e)); };
                Add(cbt);
                CheckButtons.Add(cbt);
            }
            for (int i = 0; i < RoommateButtons.Count; i++)
            {
                var id = roommates.ElementAt(i);
                if (RoommateButtons[i].AvatarId != id)
                    RoommateButtons[i].AvatarId = id;
                var builder = buildRoommates.Contains(id);
             
                CheckButtons[i].Tooltip = GameFacade.Strings.GetString("178", builder?"3":"4");
            }
        }
    }

    /// <summary>
    /// We already have the property log chat window. Maybe show that instead?
    /// </summary>
    public class UILogPanel : UIContainer
    {
        public UILogPanel(UILotControl lotController)
        {
            this.RenderScript("logpanel.uis");
        }
    }

    /// <summary>
    /// Fine-grained control of who can or can't enter the lot. TODO: list component
    /// </summary>
    public class UIAdmitBanPanel : UIContainer, IDisposable
    {
        public uint LotID
        {
            get
            {
                return 0;
            }
        }

        private byte _Mode;
        public byte Mode
        {
            set
            {
                _Mode = value;
                SetMode(false);
            }
            get
            {
                return _Mode;
            }
        }

        public UIButton AdmitAllButton { get; set; }
        public UIButton AdmitListButton { get; set; }
        public UIButton BanListButtton { get; set; }
        public UIButton BanAllButton { get; set; }

        public UIButton PreviousPageButton { get; set; }
        public UIButton NextPageButton { get; set; }

        private UIImage Background;
        private UIAdmitList AdmitList;
        public UIAdmitBanPanel(UILotControl lotController)
        {
            var script = this.RenderScript("admitbanpanel.uis");
            foreach (var child in Children)
            {
                if (child is UILabel)
                {
                    var label = ((UILabel)child);
                    label.CaptionStyle = label.CaptionStyle.Clone();
                    label.CaptionStyle.Shadow = true;
                    label.Alignment = TextAlignment.Right;
                }
            }
            Background = script.Create<UIImage>("Background");
            this.AddAt(0, Background);
            AdmitList = script.Create<UIAdmitList>("AdmitInfoListSetup");
            this.Add(AdmitList);

            AdmitAllButton.OnButtonClick += (btn) => { _Mode = 0; SetMode(true); };
            AdmitListButton.OnButtonClick += (btn) => { _Mode = 1; SetMode(true); };
            BanListButtton.OnButtonClick += (btn) => { _Mode = 2; SetMode(true); };
            BanAllButton.OnButtonClick += (btn) => { _Mode = 3; SetMode(true); };

            PreviousPageButton.OnButtonClick += (btn) => ChangePage(-1);
            NextPageButton.OnButtonClick += (btn) => ChangePage(1);
            AdmitList.OnAvatarClick += (id) =>
            {
                if (UIScreen.Current is CoreGameScreen)
                {
                    var cg = (CoreGameScreen)UIScreen.Current;
                
                }
            };
            
            if (lotController.vm.TSOState.OwnerID != lotController.vm.MyUID)
            {
                AdmitAllButton.Disabled = true;
                AdmitListButton.Disabled = true;
                BanListButtton.Disabled = true;
                BanAllButton.Disabled = true;
            }
        }

        public void ChangePage(int delta)
        {
            if (delta != 0) AdmitList.SetPage(Math.Max(0, Math.Min(AdmitList.Page + delta, AdmitList.TotalPages-1)));

            PreviousPageButton.Disabled = (AdmitList.Page == 0);
            NextPageButton.Disabled = (AdmitList.Page == AdmitList.TotalPages-1);
        }

        public void SetResults(List<Avatar> avas)
        {
            AdmitList.UpdateList(avas);
        }

        public void SetMode(bool write)
        {
            AdmitAllButton.Selected = false;
            AdmitListButton.Selected = false;
            BanListButtton.Selected = false;
            BanAllButton.Selected = false;

            switch (_Mode)
            {
                case 0: //admit all
                    AdmitAllButton.Selected = true;
                    AdmitList.Visible = false;
                    break;
                case 1: //admit list
                    AdmitListButton.Selected = true;
                    //FindController<LotAdmitController>()?.SetBanMode(false);
                    AdmitList.Visible = true;
                    break;
                case 2: //ban list
                    BanListButtton.Selected = true;
                    //FindController<LotAdmitController>()?.SetBanMode(true);
                    AdmitList.Visible = true;
                    break;
                case 3: //ban all
                    BanAllButton.Selected = true;
                    AdmitList.Visible = false;
                    break;
            }
            ChangePage(0);
            NextPageButton.Visible = AdmitList.Visible;
            PreviousPageButton.Visible = AdmitList.Visible;
            //if (write) FindController<LotAdmitController>().UpdateMode(_Mode);
        }

        public void Dispose()
        {
           // FindController<LotAdmitController>()?.Dispose();
        }
    }

    public class UIAdmitList : UIContainer
    {
        private UIListBox List1;
        private UIListBox List2;
        public int Page;
        private List<Avatar> Data = new List<Avatar>();
        public int TotalPages;
        public event Callback<uint> OnAvatarClick;

        public UIAdmitList()
        {
            List1 = new UIListBox();
            List1.X = -15;
            List1.SetSize(100, 20 * 4);
            List1.VisibleRows = 4;
            List1.RowHeight = 20;

            List1.TextStyle = new UIListBoxTextStyle()
            {
                Normal = TextStyle.DefaultLabel.Clone(),
                NormalColor = new Color(247, 232, 145),
                Selected = TextStyle.DefaultLabel.Clone(),
                SelectedColor = Color.Black,
                Highlighted = TextStyle.DefaultLabel.Clone(),
                HighlightedColor = Color.White,
                Disabled = TextStyle.DefaultLabel.Clone(),
                DisabledColor = Color.Gray
            };
            List1.Columns = new UIListBoxColumnCollection();
            List1.Columns.Add(new UIListBoxColumn() { Width = 100 });
            List1.SelectionFillColor = new Color(250, 200, 140);
            Add(List1);
            List2 = new UIListBox();
            List2.X = 85;
            List2.SetSize(100, 20 * 4);
            List2.VisibleRows = 4;
            List2.RowHeight = 20;
            List2.TextStyle = List1.TextStyle;
            List2.Columns = List1.Columns;
            List2.SelectionFillColor = new Color(250, 200, 140);
            Add(List2);

            List1.OnDoubleClick += (btn) => { if (List1.SelectedIndex != -1) OnAvatarClick?.Invoke(Convert.ToUInt32((List1.SelectedItem.Data as Avatar).ID)); };
            List2.OnDoubleClick += (btn) => { if (List2.SelectedIndex != -1) OnAvatarClick?.Invoke(Convert.ToUInt32((List2.SelectedItem.Data as Avatar).ID)); };
            List1.OnChange += (elem) => { if (List1.SelectedIndex != -1 && List2.SelectedIndex != -1) List2.SelectedIndex = -1; };
            List2.OnChange += (elem) => { if (List2.SelectedIndex != -1 && List1.SelectedIndex != -1) List1.SelectedIndex = -1; };
        }

        public void UpdateList(List<Avatar> list)
        {
            Data = list;
            TotalPages = Math.Max(1, (list.Count + 7) / 8);
            SetPage(0);
        }

        public void SetPage(int page)
        {
            if (page >= TotalPages || page < 0) return;
            List1.SelectedIndex = -1;
            List2.SelectedIndex = -1;
            var sublist1 = Data.GetRange(page*8, Math.Min(4,Data.Count-page*8));
            List1.Items = sublist1.ConvertAll(x => new UIListBoxItem(x, new ValuePointer(x, "Avatar_Name")));

            if (page * 8 + 4 < Data.Count)
            {
                var sublist2 = Data.GetRange(page * 8 + 4, Math.Min(4, Data.Count - (page * 8+4)));
                List2.Items = sublist2.ConvertAll(x => new UIListBoxItem(x, new ValuePointer(x, "Avatar_Name")));
            }
            else List2.Items = new List<UIListBoxItem>();

            Page = page;
        }
    }

    /// <summary>
    /// Lets the owner upgrade or downgrade the property size. Modded a little for FreeSO to support floors.
    /// (TODO: move mods to UIScript mod instead of hardcoding them)
    /// </summary>
    public class UIBuildableAreaPanel : UIContainer, IDisposable
    {
        private UIImage BuildableAreaBackground;

        public UILabel LotSizeLabel { get; set; }
        public UILabel TilesLabel { get; set; }
        public UILabel UpgradeCostLabel { get; set; }
        public UILabel lackofRoomateLabel { get; set; }
        public UILabel TotalCostLabel { get; set; }
        public UILabel RoommatesLabel { get; set; }
        public UILabel SizeLevelLabel { get; set; }

        public UIButton LargerButton { get; set; }
        public UIButton SmallerButton { get; set; }
        public UIButton AcceptButton { get; set; }

        public UIButton FloorsLargerButton { get; set; }
        public UIButton FloorsSmallerButton { get; set; }

        private List<UILabel> Labels;
        private List<string> SavedInitialText = new List<string>();

        private UILotControl LotControl;
        private int UpdateSizeTarget = 0;
        private int UpdateFloorsTarget = 0;

        private RenderTarget2D PreviewTarget;
        private SpriteBatch Batch;
        private UIImage PreviewImage;

        private int OldLotSize;

        private Rectangle TargetSize;

        public UIBuildableAreaPanel(UILotControl lotController)
        {
            var script = this.RenderScript("buildableareapanel.uis");
            BuildableAreaBackground = script.Create<UIImage>("BuildableAreaBackground");
            this.AddAt(0, BuildableAreaBackground);

            Labels = new List<UILabel>()
            {
                LotSizeLabel,
                TilesLabel,
                UpgradeCostLabel,
                lackofRoomateLabel,
                TotalCostLabel,
                RoommatesLabel,
                SizeLevelLabel
            };

            foreach (var label in Labels)
            {
                label.Alignment = TextAlignment.Left;
                label.CaptionStyle = label.CaptionStyle.Clone();
                label.CaptionStyle.Shadow = true;
                label.Y -= 6;
                label.X += 3;
                SavedInitialText.Add(label.Caption.Replace("%d", "%s"));
            }
            LotControl = lotController;

            LargerButton.OnButtonClick += (btn) => { UpdateSizeTarget++; UpdateCost(); };
            SmallerButton.OnButtonClick += (btn) => { UpdateSizeTarget--; UpdateCost(); };
            AcceptButton.OnButtonClick += PurchaseLotSize;

            FloorsLargerButton = script.Create<UIButton>("LargerButton");
            FloorsLargerButton.Y += 39;
            FloorsLargerButton.OnButtonClick += (btn) => { UpdateFloorsTarget++; UpdateCost(); };
            Add(FloorsLargerButton);
            FloorsSmallerButton = script.Create<UIButton>("SmallerButton");
            FloorsSmallerButton.Y += 39;
            FloorsSmallerButton.OnButtonClick += (btn) => { UpdateFloorsTarget--; UpdateCost(); };
            Add(FloorsSmallerButton);

            var sizeLabel = new UILabel();
            sizeLabel.CaptionStyle = sizeLabel.CaptionStyle.Clone();
            sizeLabel.CaptionStyle.Shadow = true;
            sizeLabel.CaptionStyle.Size = 6;
            sizeLabel.Position = new Vector2(LargerButton.X + 3, LargerButton.Y + 11);
            sizeLabel.Size = new Vector2(45, 0);
            sizeLabel.Alignment = TextAlignment.Center;
            sizeLabel.Caption = "Size";
            Add(sizeLabel);

            var floorsLabel = new UILabel();
            floorsLabel.CaptionStyle = sizeLabel.CaptionStyle.Clone();
            floorsLabel.CaptionStyle.Shadow = true;
            floorsLabel.CaptionStyle.Size = 6;
            floorsLabel.Position = new Vector2(FloorsLargerButton.X + 3, FloorsLargerButton.Y + 11);
            floorsLabel.Size = new Vector2(45, 0);
            floorsLabel.Alignment = TextAlignment.Center;
            floorsLabel.Caption = "Floors";
            Add(floorsLabel);

            SizeLevelLabel.X -= 19;
            SizeLevelLabel.Alignment = TextAlignment.Center;
            SizeLevelLabel.Size = new Vector2(41, 1);

            PreviewTarget = new RenderTarget2D(GameFacade.GraphicsDevice, 72, 72, false, SurfaceFormat.Color, DepthFormat.None,
                (GlobalSettings.Default.AntiAlias == true) ? 4 : 0, RenderTargetUsage.PreserveContents);
            Batch = new SpriteBatch(GameFacade.GraphicsDevice);

            PreviewImage = new UIImage(PreviewTarget);
            PreviewImage.Position = BuildableAreaBackground.Position + new Vector2(2);
            Add(PreviewImage);

            UpdateCost();

            if (lotController.vm.TSOState.OwnerID != lotController.vm.MyUID)
            {
                LargerButton.Disabled = true;
                SmallerButton.Disabled = true;
                FloorsLargerButton.Disabled = true;
                FloorsSmallerButton.Disabled = true;
                AcceptButton.Disabled = true;
            }
        }

        private void PurchaseLotSize(UIElement button)
        {
            LotControl.vm.SendCommand(new VMNetChangeLotSizeCmd
            {
                LotSize = (byte)UpdateSizeTarget,
                LotStories = (byte)UpdateFloorsTarget
            });
            HITVM.Get().PlaySoundEvent(UISounds.ObjectPlace);
            AcceptButton.Disabled = true;
        }

        public void UpdateCost()
        {
            var lotInfo = LotControl.vm.TSOState;
            var lotSize = lotInfo.Size & 255;
            var lotFloors = (lotInfo.Size >> 8) & 255;
            var lotDir = lotInfo.Size >> 16;

            OldLotSize = lotInfo.Size;

            UpdateSizeTarget = Math.Min(Math.Max(lotSize, UpdateSizeTarget), VMBuildableAreaInfo.BuildableSizes.Length-1);
            UpdateFloorsTarget = Math.Min(Math.Max(lotFloors, UpdateFloorsTarget), 3);
            var totalTarget = UpdateFloorsTarget + UpdateSizeTarget;
            var totalOld = lotSize + lotFloors;

            AcceptButton.Disabled = (totalTarget == totalOld); //no upgrade selected

            var baseCost = VMBuildableAreaInfo.CalculateBaseCost(totalOld, totalTarget);
            var roomieCost = VMBuildableAreaInfo.CalculateRoomieCost(lotInfo.Roommates.Count, totalOld, totalTarget);

            if (baseCost + roomieCost > (LotControl.ActiveEntity?.TSOState.Budget.Value ?? 0)) AcceptButton.Disabled = true; //can't afford

            //TODO: read from uiscript
            TotalCostLabel.CaptionStyle.Color = (AcceptButton.Disabled)?new Color(255, 125, 125):TextStyle.DefaultLabel.Color;

            var targetTiles = VMBuildableAreaInfo.BuildableSizes[UpdateSizeTarget];

            string[][] applyText = new string[][]
            {
                new string[] { },
                new string[] { targetTiles.ToString(), targetTiles.ToString()+"x"+(UpdateFloorsTarget+2) },
                new string[] { baseCost.ToString() },
                new string[] { roomieCost.ToString() },
                new string[] { (baseCost+roomieCost).ToString() },
                new string[] { Math.Min(8, totalTarget+1).ToString() },
                new string[] { (UpdateSizeTarget+1) + "+" + UpdateFloorsTarget }
            };

            for (int i=0; i<Labels.Count; i++)
            {
                Labels[i].Caption = GetArgsString(SavedInitialText[i], applyText[i]);
            }
            TargetSize = LotControl.vm.Context.GetTSOBuildableArea(UpdateSizeTarget | (UpdateFloorsTarget << 8) | (lotDir << 16));
            RenderPreview(VMBuildableAreaInfo.BuildableSizes[lotSize], lotFloors + 2, targetTiles, UpdateFloorsTarget + 2);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (LotControl.vm.TSOState.Size != OldLotSize) UpdateCost();

            var blueprint = LotControl.vm.Context.Blueprint;
            if (blueprint != null && blueprint.TargetBuildableArea != TargetSize)
            {
                blueprint.TargetBuildableArea = TargetSize;
               
                LotControl.World.InvalidateZoom();
            }
        }

        public void RenderPreview(int oldSize, int oldFloors, int newSize, int newFloors)
        {
            var gd = GameFacade.GraphicsDevice;
            gd.SetRenderTarget(PreviewTarget);
            gd.Clear(Color.TransparentBlack);
            Batch.Begin();

            //64x32 base lot with a 4px border from bottom, left and right edges.
            var baseCol = new Color(0x61, 0x80, 0x9F);
            var newCol = new Color(0xFF, 0xF7, 0x99);
            var oldCol = new Color(0xCC, 0xE8, 0xE2);
            var oldT = 2;
            var newT = 2;

            var shadO = new Vector2(1, 1); //shadow offset
            var left = new Vector2(4, (72 - 4) - 16);
            var bottom = new Vector2(72/2, (72 - 4));
            var right = new Vector2(72 - 4, (72 - 4) - 16);
            var top = new Vector2(72/2, (72 - 4) - 32);

            var ctrBase = (left + bottom) / 2;
            var ltc = (bottom - left)/2;

            DrawPath(Color.Black, Batch, 2, true, left + shadO, bottom + shadO, right + shadO, top + shadO);
            DrawPath(baseCol, Batch, 2, true, left, bottom, right, top);

            //draw new lot.
            float sizePct = newSize / 64f;
            var nLeft = ctrBase - (ltc) * sizePct;
            var nBtm = ctrBase + (ltc) * sizePct;
            var nRight = nBtm + (right - bottom) * sizePct;
            var nTop = nLeft + (top - left) * sizePct;

            if (newFloors > oldFloors || newSize > oldSize)
            {
                DrawLineStack(Color.Black, nTop+shadO, nRight+shadO, 5, newFloors, Batch, newT);
                DrawLineStack(newCol, nTop, nRight, 5, newFloors, Batch, newT);
            }

            if (newSize > oldSize)
            {
                DrawPath(Color.Black, Batch, newT, true, nLeft + shadO, nBtm + shadO, nRight + shadO, nTop + shadO);
                DrawPath(newCol, Batch, newT, true, nLeft, nBtm, nRight, nTop);
            }

            //draw old lot.
            sizePct = oldSize / 64f;
            var oLeft = ctrBase - (ltc) * sizePct;
            var oBtm = ctrBase + (ltc) * sizePct;
            var oRight = oBtm + (right - bottom) * sizePct;
            var oTop = oLeft + (top - left) * sizePct;

            DrawPath(Color.Black, Batch, oldT, true, oLeft + shadO, oBtm + shadO, oRight + shadO, oTop + shadO);
            DrawLineStack(Color.Black, oTop + shadO, oLeft + shadO, 5, oldFloors, Batch, oldT);
            DrawLineStack(oldCol, oTop, oLeft, 5, oldFloors, Batch, oldT);
            DrawPath(oldCol, Batch, oldT, true, oLeft, oBtm, oRight, oTop);

            //end
            Batch.End();
            gd.SetRenderTarget(null);
            PreviewImage.Texture = PreviewTarget;
        }

        public void Dispose()
        {
            PreviewTarget.Dispose();
            Batch.Dispose();
            var blueprint = LotControl.vm.Context.Blueprint;
            if (blueprint != null)
            {
                blueprint.TargetBuildableArea = new Rectangle();
                
            }
            LotControl.World.InvalidateZoom();
        }

        public override void Draw(UISpriteBatch batch)
        {
            DrawLocalTexture(batch, BuildableAreaBackground.Texture, new Rectangle(138, 0, 46, 51), 
                BuildableAreaBackground.Position + new Vector2(138, 39), new Vector2(1));
            base.Draw(batch);
        }

        //TODO: move these to drawing utils?

        private void DrawPath(Color tint, SpriteBatch batch, int lineWidth, bool complete, params Vector2[] path)
        {
            for (int i=0; i<path.Length; i++)
            {
                if (i == path.Length - 1 && !complete) return;
                DrawLine(tint, path[i], path[(i + 1) % path.Length], batch, lineWidth);
            }
        }

        private void DrawLineStack(Color tint, Vector2 pt1, Vector2 pt2, float height, int levels, SpriteBatch spriteBatch, int lineWidth)
        {
            DrawLine(tint, pt1, pt1 - new Vector2(0, height * levels), spriteBatch, lineWidth);
            DrawLine(tint, pt2, pt2 - new Vector2(0, height * levels), spriteBatch, lineWidth);
            for (int i=1; i<=levels; i++)
            {
                DrawLine(tint, pt1 - new Vector2(0, height * (i)), pt2 - new Vector2(0, height * (i)), spriteBatch, lineWidth);
            }
        }

        private void DrawLine(Color tint, Vector2 Start, Vector2 End, SpriteBatch spriteBatch, int lineWidth) //draws a line from Start to End.
        {
            double length = Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));
            float direction = (float)Math.Atan2(End.Y - Start.Y, End.X - Start.X);
            spriteBatch.Draw(TextureGenerator.GetPxWhite(spriteBatch.GraphicsDevice), new Rectangle((int)Start.X, (int)Start.Y - (int)(lineWidth / 2), (int)length, lineWidth), 
                null, tint, direction, new Vector2(0, 0.5f), SpriteEffects.None, 0); 
        }

        private string GetArgsString(string argsStr, string[] args)
        {
            StringBuilder SBuilder = new StringBuilder();
            int ArgsCounter = 0;

            for (int i = 0; i < argsStr.Length; i++)
            {
                string CurrentArg = argsStr.Substring(i, 1);

                if (CurrentArg.Contains("%"))
                {
                    if (ArgsCounter < args.Length)
                    {
                        SBuilder.Append(CurrentArg.Replace("%", args[ArgsCounter]));
                        ArgsCounter++;
                        i++; //Next, CurrentArg will be either s or d - skip it!
                    }
                }
                else
                    SBuilder.Append(CurrentArg);
            }

            return SBuilder.ToString();
        }
    }
}
