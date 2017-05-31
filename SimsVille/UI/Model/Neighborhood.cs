using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TSO.Common.rendering.framework;
using TSO.Common.rendering.framework.model;
using TSOVille.Code;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.Utils;
using TSOVille.Code.UI.Screens;
using TSOVille.Code.UI.Controls;





namespace SimsVille.UI.Model
{
    public class Neighborhood : _3DAbstract
    {

        public float m_ZoomProgress = 0; 
        public bool m_Zoomed = false;
        public Texture2D TerrainImage;
        private Effect Shader2D;
        private GraphicsDevice m_GraphicsDevice;
        private Vector3 m_LightPosition;
        private Vector2 m_MouseStart;
        private Color m_TintColor;
        private float m_SpotOsc = 0;
        private float m_ShadowMult = 1;
        private bool RegenData, m_HandleMouse;
        private bool m_MouseMove = false;
        private Texture2D m_WhiteLine;
        private Matrix m_MovMatrix;
        private MouseState m_MouseState, m_LastMouseState;
        private int m_ScrHeight, m_ScrWidth;
        private int m_LotCost = 0;
        private byte[] m_ElevationData;
        private int[] m_SelTile = new int[] { -1, -1 };
        private int[] m_SelTileTmp = new int[] { -1, -1 };
        private static LotTileEntry m_CurrentLot;
        private ArrayList m_2DVerts;

        private Dictionary<int, Texture2D> m_HouseGraphics;

        private float m_ViewOffX, m_ViewOffY, m_TargVOffX, m_TargVOffY, m_ScrollSpeed;

        private HouseDataRetriever m_HousesData;

        private Dictionary<Vector2, LotTileEntry> m_HousesLookup;

        private Color[] m_TimeColors = new Color[] 
        {
            new Color(50, 70, 122),
            new Color(60, 80, 132),
            new Color(60, 80, 132),
            new Color(217, 109, 0),
            new Color(235, 235, 235),
            new Color(255, 255, 255),
            new Color(235, 235, 235),
            new Color(217, 109, 0),
            new Color(60, 80, 80),
            new Color(60, 80, 132),
            new Color(50, 70, 122)
        };

        public Vector2 transformSpr(float iScale, Vector3 pos)
        { //transform 3d position to view.
            Vector3 temp = Vector3.Transform(pos, m_MovMatrix);
            int width = m_ScrWidth;
            int height = m_ScrHeight;
            return new Vector2((temp.X - m_ViewOffX) * iScale + width / 2, (-(temp.Y - m_ViewOffY) * iScale) + height / 2);
        }

        private Vector2 CalculateR(Vector2 m) //get approx 3d position of 2d screen position in model/tile space.
        {
            Vector2 ReturnM = new Vector2(m.X, m.Y);
            ReturnM.Y = 2.0f * m.Y;
            float temp = ReturnM.X;
            double cos = Math.Cos((-45.0 / 180.0) * Math.PI);
            double sin = Math.Sin((-45.0 / 180.0) * Math.PI);
            ReturnM.X = (float)(cos * ReturnM.X + sin * ReturnM.Y);
            ReturnM.Y = (float)(cos * ReturnM.Y - sin * temp);
            ReturnM.X += 254.55844122715712f;
            ReturnM.Y += 254.55844122715712f;
            return ReturnM;
        }

        private bool IsInsidePoly(double[] Poly, double[] Pos)
        {
            if (Poly.Length % 2 != 0) return false; //invalid polygon
            int n = Poly.Length / 2;
            bool result = false;

            for (int i = 0; i < n; i++)
            {
                double x1 = Poly[i * 2];
                double y1 = Poly[i * 2 + 1];
                double x2 = Poly[((i + 1) * 2) % Poly.Length];
                double y2 = Poly[((i + 1) * 2 + 1) % Poly.Length];
                double slope = (y2 - y1) / (x2 - x1);
                double c = y1 - (slope * x1);
                if ((Pos[1] < (slope * Pos[0]) + c) && (Pos[0] >= Math.Min(x1, x2)) && (Pos[0] < Math.Max(x1, x2)))
                    result = !(result);
            }

            return result;
        }

        private byte[] ConvertToBinaryArray(Color[] ColorArray)
        {
            byte[] BinArray = new byte[ColorArray.Length * 4];

            for (int i = 0; i < ColorArray.Length; i++)
            {
                BinArray[i * 4] = ColorArray[i].R;
                BinArray[i * 4 + 1] = ColorArray[i].G;
                BinArray[i * 4 + 2] = ColorArray[i].B;
                BinArray[i * 4 + 3] = ColorArray[i].A;
            }

            return BinArray;
        }


        private int[] GetHoverSquare()
        {
            double ResScale = 768.0 / m_ScrHeight;
            double fisoScale = (Math.Sqrt(0.5 * 0.5 * 2) / 5.10) * ResScale; // is 5.10 on far zoom
            double zisoScale = Math.Sqrt(0.5 * 0.5 * 2) / 144.0; // currently set 144 to near zoom
            double isoScale = (1 - m_ZoomProgress) * fisoScale + (m_ZoomProgress) * zisoScale;
            double width = m_ScrWidth;
            float iScale = (float)(width / (width * isoScale * 2));

            Vector2 mid = CalculateR(new Vector2(m_ViewOffX, -m_ViewOffY));
            mid.X -= 6;
            mid.Y += 6;
            double[] bounds = new double[] { Math.Round(mid.X - 19), Math.Round(mid.Y - 19), Math.Round(mid.X + 19), Math.Round(mid.Y + 19) };
            double[] pos = new double[] { m_MouseState.X, m_MouseState.Y };

            for (int y = (int)bounds[1]; y < bounds[3]; y++)
            {
                if (y < 0 || y > 511) continue;
                for (int x = (int)bounds[0]; x < bounds[2]; x++)
                {
                    if (x < 0 || x > 511) continue;
                    //get the 4 points of this tile, and check if the mouse cursor is inside them.
                    var xy = transformSpr(iScale, new Vector3(x + 0, m_ElevationData[(y * 512 + x) * 4] / 12.0f, y + 0));
                    var xy2 = transformSpr(iScale, new Vector3(x + 1, m_ElevationData[(y * 512 + Math.Min(x + 1, 511)) * 4] / 12.0f, y + 0));
                    var xy3 = transformSpr(iScale, new Vector3(x + 1, m_ElevationData[(Math.Min(y + 1, 511) * 512 + Math.Min(x + 1, 511)) * 4] / 12.0f, y + 1));
                    var xy4 = transformSpr(iScale, new Vector3(x + 0, m_ElevationData[(Math.Min(y + 1, 511) * 512 + x) * 4] / 12.0f, y + 1));
                    if (IsInsidePoly(new double[] { xy.X, xy.Y, xy2.X, xy2.Y, xy3.X, xy3.Y, xy4.X, xy4.Y }, pos)) return new int[] { x, y }; //we have a match
                }
            }
            return new int[] { -1, -1 }; //no match, return invalid mouse selection (-1, -1)
        }


        public Neighborhood(GraphicsDevice Device)
            : base(Device)
        {
            m_GraphicsDevice = Device;
        }

        public override List<_3DComponent> GetElements()
        {
            return new List<_3DComponent>();
        }
        public override void Add(_3DComponent item) 
        {

        }


        public void LoadContent()
        {

            Stream stream = File.Open("Content/nhood.bmp", FileMode.Open);
            TerrainImage = Texture2D.FromStream(m_GraphicsDevice, stream);

            Shader2D = GameFacade.Game.Content.Load<Effect>("Effects\\colorpoly2d");

            m_WhiteLine = new Texture2D(m_GraphicsDevice, 1, 1);
            m_WhiteLine.SetData<Color>(new Color[] { Color.White });

        }

         public void Initialize(HouseDataRetriever housesData)
        {

            m_HousesData = housesData;

            LotTileEntry[] data = m_HousesData.LotTileData.ToArray();
            m_HousesLookup = new Dictionary<Vector2, LotTileEntry>();
            for (int i = 0; i < data.Length; i++)
            {
                m_HousesLookup[new Vector2(data[i].x, data[i].y)] = data[i];
            }

            m_ScrHeight = GameFacade.GraphicsDevice.Viewport.Height;
            m_ScrWidth = GameFacade.GraphicsDevice.Viewport.Width;

            m_2DVerts = new ArrayList();

            Color[] ColorData = new Color[m_ScrWidth * m_ScrHeight];

            m_ElevationData = ConvertToBinaryArray(ColorData);

            m_HouseGraphics = new Dictionary<int, Texture2D>();

            m_HandleMouse = false;

        }

         public override void DeviceReset(GraphicsDevice Device)
         {
 
             LoadContent();
             RegenData = true;
         }

         private void FixedTimeUpdate()
         {
             m_SpotOsc = (m_SpotOsc + 0.01f) % 1; //spotlight oscillation. Cycles fully every 100 frames.
             if (m_Zoomed)
             {
                 m_ZoomProgress += (1.0f - m_ZoomProgress) / 5.0f;
                 bool Triggered = false;

                 if (m_MouseMove)
                 {
                     m_TargVOffX += (m_MouseState.X - m_MouseStart.X) / 1000; //move by fraction of distance between the mouse and where it started in both axis
                     m_TargVOffY -= (m_MouseState.Y - m_MouseStart.Y) / 1000;

                     //it's your duty to deal with the mouse cursor stuff when moving into PD!

                     /*var dir = Math.Round((Math.Atan2(m_MouseStart.X - m_MouseState.Y,
                         m_MouseState.X - m_MouseStart.X) / Math.PI) * 4) + 4;
                     ChangeCursor(dir);*/
                 }
                 else //edge scroll check - do this even if mouse events are blocked
                 {
                     if (m_MouseState.X > m_ScrWidth - 32)
                     {
                         Triggered = true;
                         m_TargVOffX += m_ScrollSpeed;
                         CursorManager.INSTANCE.SetCursor(CursorType.ArrowRight);
                         //changeCursor("right.cur")
                     }
                     if (m_MouseState.X < 32)
                     {
                         Triggered = true;
                         m_TargVOffX -= m_ScrollSpeed;
                         CursorManager.INSTANCE.SetCursor(CursorType.ArrowLeft);
                         //changeCursor("left.cur");
                     }
                     if (m_MouseState.Y > m_ScrHeight - 32)
                     {
                         Triggered = true;
                         m_TargVOffY -= m_ScrollSpeed;
                         CursorManager.INSTANCE.SetCursor(CursorType.ArrowDown);
                         //changeCursor("down.cur");
                     }
                     if (m_MouseState.Y < 32)
                     {
                         Triggered = true;
                         m_TargVOffY += m_ScrollSpeed;
                         CursorManager.INSTANCE.SetCursor(CursorType.ArrowUp);
                         //changeCursor("up.cur");
                     }

                     if (!Triggered)
                     {
                         m_ScrollSpeed = 0.1f; //not scrolling. Reset speed, set default cursor.
                         CursorManager.INSTANCE.SetCursor(CursorType.Normal);
                         //changeCursor("auto", true); AKA the default cursor.
                     }
                     else
                         m_ScrollSpeed += 0.005f; //if edge scrolling make the speed increase the longer the mouse is at the edge.
                 }

                 m_TargVOffX = Math.Max(-135, Math.Min(m_TargVOffX, 138)); //maximum offsets for zoomed camera. Need adjusting for other screen sizes...
                 m_TargVOffY = Math.Max(-100, Math.Min(m_TargVOffY, 103));
             }
             else
                 m_ZoomProgress += (0 - m_ZoomProgress) / 5.0f; //zoom progress interpolation. Isn't very fixed but it's a nice gradiation.
         }

         public void SetTimeOfDay(double time)
         {
             Color col1 = m_TimeColors[(int)Math.Floor(time * (m_TimeColors.Length - 1))]; //first colour
             Color col2 = m_TimeColors[(int)Math.Floor(time * (m_TimeColors.Length - 1)) + 1]; //second colour
             double Progress = (time * (m_TimeColors.Length - 1)) % 1; //interpolation progress (mod 1)

             m_TintColor = Color.Lerp(col1, col2, (float)Progress); //linearly interpolate between the two colours for this specific time.

             m_LightPosition = new Vector3(0, 0, -263);
             Matrix Transform = Matrix.Identity;

             Transform *= Matrix.CreateRotationY((float)((((time + 0.25) % 0.5) + 0.5) * Math.PI * 2.0)); //Controls the rotation of the sun/moon around the city. 
             Transform *= Matrix.CreateRotationZ((float)(Math.PI * (45.0 / 180.0))); //Sun is at an angle of 45 degrees to horizon at it's peak. idk why, it's winter maybe? looks nice either way
             Transform *= Matrix.CreateRotationY((float)(Math.PI * 0.3)); //Offset from front-back a little. This might need some adjusting for the nicest sunset/sunrise locations.
             Transform *= Matrix.CreateTranslation(new Vector3(256, 0, 256)); //Move pivot center to center of mesh.

             m_LightPosition = Vector3.Transform(m_LightPosition, Transform);

             if (Math.Abs((time % 0.5) - 0.25) < 0.05) //Near the horizon, shadows should gracefully fade out into the opposite shadows (moonlight/sunlight)
             {
                 m_ShadowMult = (float)(1 - (Math.Abs((time % 0.5) - 0.25) * 20)) * 0.35f + 0.65f;
             }
             else
             {
                 m_ShadowMult = 0.65f; //Shadow strength. Remember to change the above if you alter this.
             }
         }

        public override void Update(UpdateState state)
         {

             if (Visible)
             { //if we're not visible, do not update CityRenderer state...

                 CoreGameScreen CurrentUIScr = (CoreGameScreen)GameFacade.Screens.CurrentUIScreen;

                 m_LastMouseState = m_MouseState;
                 m_MouseState = Mouse.GetState();

                 m_MouseMove = (m_MouseState.RightButton == ButtonState.Pressed);

                 if (m_HandleMouse)
                 {
                     if (m_Zoomed)
                     {
                         m_SelTile = GetHoverSquare();


                     }

                     if (m_MouseState.RightButton == ButtonState.Pressed && m_LastMouseState.RightButton == ButtonState.Released)
                     {
                         m_MouseStart = new Vector2(m_MouseState.X, m_MouseState.Y); //if middle mouse button activated, record where we started pressing it (to use for panning)
                     }

                     else if (m_MouseState.LeftButton == ButtonState.Released && m_LastMouseState.LeftButton == ButtonState.Pressed) //if clicked...
                     {
                         if (!m_Zoomed)
                         {
                             m_Zoomed = true;
                             double ResScale = 768.0 / m_ScrHeight;
                             double isoScale = (Math.Sqrt(0.5 * 0.5 * 2) / 5.10) * ResScale;
                             double hb = m_ScrWidth * isoScale;
                             double vb = m_ScrHeight * isoScale;

                             m_TargVOffX = (float)(-hb + m_MouseState.X * isoScale * 2);
                             m_TargVOffY = (float)(vb - m_MouseState.Y * isoScale * 2); //zoom into approximate location of mouse cursor if not zoomed already
                         }
                         else
                         {
                             if (m_SelTile[0] != -1 && m_SelTile[1] != -1)
                             {
                                 m_SelTileTmp[0] = m_SelTile[0];
                                 m_SelTileTmp[1] = m_SelTile[1];

                                 UIAlertOptions AlertCoords = new UIAlertOptions();
                                 AlertCoords.Title = GameFacade.Strings.GetString("246", "1");
                                 //AlertOptions.Message = GameFacade.Strings.GetString("215", "23", new string[] 
                                 //{ m_LotCost.ToString(), CurrentUIScr.ucp.MoneyText.Caption });

                                 AlertCoords.Message = m_SelTile[0].ToString() + " " + m_SelTile[1].ToString();
                                 AlertCoords.Buttons = UIAlertButtons.YesNo;

                                 foreach (LotTileEntry Lot in m_HousesData.LotTileData)
                                     if (Lot.x == m_SelTile[0] && Lot.y == m_SelTileTmp[1])
                                         m_CurrentLot = Lot;


                                 if (m_CurrentLot != null)
                                 {
                                     UIAlertOptions AlertOptions = new UIAlertOptions();
                                     AlertOptions.Title = GameFacade.Strings.GetString("246", "1");
                                     //AlertOptions.Message = GameFacade.Strings.GetString("215", "23", new string[] 
                                     //{ m_LotCost.ToString(), CurrentUIScr.ucp.MoneyText.Caption });

                                     AlertOptions.Message = m_CurrentLot.x.ToString() + " " + m_CurrentLot.y.ToString();
                                     AlertOptions.Buttons = UIAlertButtons.YesNo;

                                     

                                 }

                             }
                         }

                         CurrentUIScr.ucp.UpdateZoomButton();
                     }
                 }
                 else
                 {
                     m_SelTile = new int[] { -1, -1 };
                 }

                 //m_SecondsBehind += time.ElapsedGameTime.TotalSeconds;
                 //m_SecondsBehind -= 1 / 60;
                 FixedTimeUpdate();
                 //SetTimeOfDay(m_DayNightCycle % 1); //calculates sun/moon light colour and position
                 //m_DayNightCycle += 0.001; //adjust the cycle speed here. When ingame, set m_DayNightCycle to to the percentage of time passed through the day. (0 to 1)

                 m_ViewOffX = (m_TargVOffX) * m_ZoomProgress;
                 m_ViewOffY = (m_TargVOffY) * m_ZoomProgress;
             }
        }

        private void DrawLine(Texture2D Fill, Vector2 Start, Vector2 End, SpriteBatch spriteBatch, int lineWidth, float opacity) //draws a line from Start to End.
        {
            double length = Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));
            float direction = (float)Math.Atan2(End.Y - Start.Y, End.X - Start.X);
            Color tint = new Color(1f, 1f, 1f, 1f) * opacity;
            spriteBatch.Draw(Fill, new Rectangle((int)Start.X, (int)Start.Y - (int)(lineWidth / 2), (int)length, lineWidth), null, tint, direction, new Vector2(0, 0.5f), SpriteEffects.None, 0); //
        }


        /// <summary>
        /// Draws a tooltip at the specified coordinates.
        /// </summary>
        /// <param name="batch">A SpriteBatch instance.</param>
        /// <param name="tooltip">String to be drawn.</param>
        /// <param name="position">Position of tooltip.</param>
        /// <param name="opacity">Tooltip's opacity.</param>
        public void DrawTooltip(SpriteBatch batch, string tooltip, Vector2 position, float opacity)
        {
            TextStyle style = TextStyle.DefaultLabel.Clone();
            style.Color = Color.Black;
            style.Size = 8;

            var scale = new Vector2(1, 1);
            if (style.Scale != 1.0f)
            {
                scale = new Vector2(scale.X * style.Scale, scale.Y * style.Scale);
            }

            var wrapped = UIUtils.WordWrap(tooltip, 290, style, scale); //tooltip max width should be 300. There is a 5px margin on each side.

            int width = wrapped.MaxWidth + 10;
            int height = 13 * wrapped.Lines.Count + 4; //13 per line + 4.

            position.X = Math.Min(position.X, GameFacade.GraphicsDevice.Viewport.Width - width);
            position.Y = Math.Max(position.Y, height);

            var whiteRectangle = new Texture2D(batch.GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            batch.Draw(whiteRectangle, new Rectangle((int)position.X, (int)position.Y - height, width, height), Color.White * opacity); //note: in XNA4 colours need to be premultiplied

            //border
            batch.Draw(whiteRectangle, new Rectangle((int)position.X, (int)position.Y - height, 1, height), new Color(0, 0, 0, opacity));
            batch.Draw(whiteRectangle, new Rectangle((int)position.X, (int)position.Y - height, width, 1), new Color(0, 0, 0, opacity));
            batch.Draw(whiteRectangle, new Rectangle((int)position.X + width, (int)position.Y - height, 1, height), new Color(0, 0, 0, opacity));
            batch.Draw(whiteRectangle, new Rectangle((int)position.X, (int)position.Y, width, 1), new Color(0, 0, 0, opacity));

            position.Y -= height;

            for (int i = 0; i < wrapped.Lines.Count; i++)
            {
                int thisWidth = (int)(style.SpriteFont.MeasureString(wrapped.Lines[i]).X * scale.X);
                batch.DrawString(style.SpriteFont, wrapped.Lines[i], position + new Vector2((width - thisWidth) / 2, 0), new Color(0, 0, 0, opacity), 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                position.Y += 13;
            }
        }

        private void PathTile(int x, int y, float iScale, float opacity)
        { //quick and dirty function to fill a tile with white using the 2DVerts system. Used in near view for online houses.
            Vector2 xy = transformSpr(iScale, new Vector3(x + 0, m_ElevationData[(y * 512 + x) * 4] / 12.0f, y + 0));
            Vector2 xy2 = transformSpr(iScale, new Vector3(x + 1, m_ElevationData[(y * 512 + Math.Min(x + 1, 511)) * 4] / 12.0f, y + 0));
            Vector2 xy3 = transformSpr(iScale, new Vector3(x + 1, m_ElevationData[(Math.Min(y + 1, 511) * 512 + Math.Min(x + 1, 511)) * 4] / 12.0f, y + 1));
            Vector2 xy4 = transformSpr(iScale, new Vector3(x + 0, m_ElevationData[(Math.Min(y + 1, 511) * 512 + x) * 4] / 12.0f, y + 1));

            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));
            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy2, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));
            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy3, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));

            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));
            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy3, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));
            m_2DVerts.Add(new VertexPositionColor(new Vector3(xy4, 1), new Color(1.0f, 1.0f, 1.0f, opacity)));
        }

        public void Draw2DPoly()
        {
            if (m_2DVerts.Count == 0) return;
            m_GraphicsDevice.DepthStencilState = DepthStencilState.None;

            VertexPositionColor[] Vert2D = new VertexPositionColor[m_2DVerts.Count];
            m_2DVerts.CopyTo(Vert2D);

            Matrix View = new Matrix(1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);

            Matrix Projection = Matrix.CreateOrthographicOffCenter(0, (float)m_ScrWidth, -(float)m_ScrHeight, 0, 0, 1);

            Shader2D.CurrentTechnique = Shader2D.Techniques[0];
            Shader2D.Parameters["Projection"].SetValue(Projection);
            Shader2D.Parameters["View"].SetValue(View);

            Shader2D.CurrentTechnique.Passes[0].Apply();

            m_GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, Vert2D, 0, Vert2D.Length / 3); //draw 2d coloured triangle array (for spotlights etc)

            m_GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawSprites(float HB, float VB)
        {
            SpriteBatch spriteBatch = new SpriteBatch(m_GraphicsDevice);
            spriteBatch.Begin();

            if (!m_Zoomed && m_HandleMouse)
            {
                //draw rectangle to indicate zoom position
                DrawLine(m_WhiteLine, new Vector2(m_MouseState.X - 15, m_MouseState.Y - 11), new Vector2(m_MouseState.X - 15, m_MouseState.Y + 11), spriteBatch, 2, 1);
                DrawLine(m_WhiteLine, new Vector2(m_MouseState.X - 16, m_MouseState.Y + 10), new Vector2(m_MouseState.X + 16, m_MouseState.Y + 10), spriteBatch, 2, 1);
                DrawLine(m_WhiteLine, new Vector2(m_MouseState.X + 15, m_MouseState.Y + 11), new Vector2(m_MouseState.X + 15, m_MouseState.Y - 11), spriteBatch, 2, 1);
                DrawLine(m_WhiteLine, new Vector2(m_MouseState.X + 16, m_MouseState.Y - 10), new Vector2(m_MouseState.X - 16, m_MouseState.Y - 10), spriteBatch, 2, 1);
            }
            else if (m_Zoomed && m_HandleMouse)
            {
                if (m_LotCost != 0)
                {
                    float X = GetHoverSquare()[0];
                    float Y = GetHoverSquare()[1];
                    //TODO: Should this have opacity? Might have to change this to render only when hovering over a lot.
                    DrawTooltip(spriteBatch, m_LotCost.ToString() + "§", new Vector2(X, Y), 0.5f);
                }
                else
                {
                    if (m_CurrentLot != null)
                    {
                        float X = GetHoverSquare()[0];
                        float Y = GetHoverSquare()[1];

                    }
                }
            }

            if (m_ZoomProgress < 0.5)
            {
                spriteBatch.End();
                spriteBatch.Dispose();
                return;
            }

            float iScale = (float)m_ScrWidth / (HB * 2);

            float treeWidth = (float)(Math.Sqrt(2) * (128.0 / 144.0));
            float treeHeight = treeWidth * (80 / 128);

            Vector2 mid = CalculateR(new Vector2(m_ViewOffX, -m_ViewOffY)); //determine approximate tile position at center of screen
            mid.X -= 6;
            mid.Y += 6;
            float[] bounds = new float[] { (float)Math.Round(mid.X - 19), (float)Math.Round(mid.Y - 19), (float)Math.Round(mid.X + 19), (float)Math.Round(mid.Y + 19) };


            for (short y = (short)bounds[1]; y < bounds[3]; y++) //iterate over tiles close to the approximate tile position at the center of the screen and draw any trees/houses on them
            {
                if (y < 0 || y > 511) continue;
                for (short x = (short)bounds[0]; x < bounds[2]; x++)
                {
                    if (x < 0 || x > 511) continue;

                    float elev = (m_ElevationData[(y * 512 + x) * 4] + m_ElevationData[(y * 512 + Math.Min(x + 1, 511)) * 4] +
                        m_ElevationData[(Math.Min(y + 1, 511) * 512 + Math.Min(x + 1, 511)) * 4] +
                        m_ElevationData[(Math.Min(y + 1, 511) * 512 + x) * 4]) / 4; //elevation of sprite is the average elevation of the 4 vertices of the tile

                    var xy = transformSpr(iScale, new Vector3((float)(x + 0.5), elev / 12.0f, (float)(y + 0.5)));

                    if (xy.X > -64 && xy.X < m_ScrWidth + 64 && xy.Y > -40 && xy.Y < m_ScrHeight + 40) //is inside screen
                    {

                        Vector2 loc = new Vector2(x, y);
                        LotTileEntry house;

                        if (m_HousesLookup.ContainsKey(loc))
                        {
                            house = m_HousesLookup[loc];
                        }
                        else
                        {
                            house = null;
                        }
                        if (house != null) //if there is a house here, draw it
                        {
                            if ((house.flags & 1) > 0)
                            {
                                PathTile(x, y, iScale, (float)(0.3 + Math.Sin(4 * Math.PI * (m_SpotOsc % 1)) * 0.15));
                            }

                            double scale = treeWidth * iScale / 128.0;
                            if (!m_HouseGraphics.ContainsKey(house.id))
                            {
                               

                                Texture2D HouseGraphic = m_HousesData.RetrieveHouseGFX(house.id, house.name);

                                if (HouseGraphic != null)
                                    m_HouseGraphics[house.id] = HouseGraphic;
                            }
                            Texture2D lotImg = m_HouseGraphics[house.id];
                            spriteBatch.Draw(lotImg, new Rectangle((int)(xy.X - 64.0 * scale), (int)(xy.Y - 32.0 * scale), (int)(scale * 128), (int)(scale * 64)), m_TintColor);
                        }
                        
                    }
                }
            }

            Draw2DPoly(); //fill the tiles below online houses BEFORE actually drawing the houses and trees!
            
        }

        public override void Draw(GraphicsDevice GraphicsDevice)
        {
            float ResScale = 768.0f / m_ScrHeight; //scales up the vertical height to match that of the target resolution (for the far view)

            float FisoScale = (float)(Math.Sqrt(0.5 * 0.5 * 2) / 5.10f) * ResScale; // is 5.10 on far zoom
            float ZisoScale = (float)Math.Sqrt(0.5 * 0.5 * 2) / 144f;  // currently set 144 to near zoom

            float IsoScale = (1 - m_ZoomProgress) * FisoScale + (m_ZoomProgress) * ZisoScale;

            float HB = m_ScrWidth * IsoScale;
            float VB = m_ScrHeight * IsoScale;

            m_GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = new SpriteBatch(m_GraphicsDevice);
            spriteBatch.Begin();


            spriteBatch.Draw(TerrainImage, new Rectangle(0, 0, m_GraphicsDevice.Viewport.Width, m_GraphicsDevice.Viewport.Height), Color.White);

            //DrawSprites(HB, VB);


            spriteBatch.End();
            spriteBatch.Dispose();
        }


    }
}
