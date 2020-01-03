using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FSO.Common.Rendering.Framework;
using FSO.Common.Rendering.Framework.Model;
using FSO.Client;
using FSO.Client.UI.Screens;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.Utils;
using FSO.Client.Rendering.City;





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
        private int[] m_SelTile = new int[] { -1, -1 };
        private int[] m_SelTileTmp = new int[] { -1, -1 };
        private static LotTileEntry m_CurrentLot;
        private ArrayList m_2DVerts;
        private double m_SecondsBehind = 0, m_DayNightCycle = 24.0;

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
             m_HouseGraphics = new Dictionary<int, Texture2D>();

            m_HousesLookup = new Dictionary<Vector2, LotTileEntry>();
            for (int i = 0; i < data.Length; i++)
            {
                m_HousesLookup[new Vector2(data[i].x, data[i].y)] = data[i];
                  m_HouseGraphics[i] = m_HousesData.HousesImages[i];
            }

            m_ScrHeight = GameFacade.GraphicsDevice.Viewport.Height;
            m_ScrWidth = GameFacade.GraphicsDevice.Viewport.Width;

            m_HandleMouse = false;

        }

         public override void DeviceReset(GraphicsDevice Device)
         {
 
             LoadContent();
             RegenData = true;
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
                            

                                 foreach (LotTileEntry Lot in m_HousesData.LotTileData)
                                     if (Lot.x == m_SelTile[0] && Lot.y == m_SelTile[1])
                                         m_CurrentLot = Lot;


                                 if (m_CurrentLot != null)
                                 {
                                     UIAlertOptions AlertOptions = new UIAlertOptions();
                                     AlertOptions.Title = GameFacade.Strings.GetString("246", "1");
                                     //AlertOptions.Message = GameFacade.Strings.GetString("215", "23", new string[] 
                                     //{ m_LotCost.ToString(), CurrentUIScr.ucp.MoneyText.Caption });

                                     AlertOptions.Message = m_CurrentLot.x.ToString() + " " + m_CurrentLot.y.ToString();
                                     //AlertOptions.Buttons = UIAlertButtons.YesNo;

                                     

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

                 GameTime time = new GameTime();

                 m_SecondsBehind += time.ElapsedGameTime.TotalSeconds;
                 m_SecondsBehind -= 1 / 60;
                 SetTimeOfDay(m_DayNightCycle % 1); //calculates sun/moon light colour and position
                 m_DayNightCycle += 0.001; //adjust the cycle speed here. When ingame, set m_DayNightCycle to to the percentage of time passed through the day. (0 to 1)

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

        private void DrawSprites(SpriteBatch sbatch)
        {

            for (int i = 0; i < m_HousesData.LotTileData.Count; i++)
            {

                LotTileEntry lot = m_HousesData.LotTileData[i];

                if (m_HousesData.HousesImages[i] != null)
                    sbatch.Draw(m_HousesData.HousesImages[i], new Rectangle(lot.x, lot.y, 256, 256), Color.White);       


            }

            //Draw2DPoly();

        }

        public override void Draw(GraphicsDevice GraphicsDevice)
        {


            m_GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = new SpriteBatch(m_GraphicsDevice);
            spriteBatch.Begin();


            spriteBatch.Draw(TerrainImage, new Rectangle(0, 0, m_GraphicsDevice.Viewport.Width, m_GraphicsDevice.Viewport.Height), Color.White);

            DrawSprites(spriteBatch);


            spriteBatch.End();
            spriteBatch.Dispose();
        }


        public override void Dispose()
        {
         

        }

    }
}
