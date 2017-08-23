using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TSO.Common.rendering.framework;
using TSOVille;
using TSOVille.Code;
using tso.world;
using System.Threading;
using System.IO;


namespace SimsHomeMaker
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SimsGame : TSO.Common.rendering.framework.Game
    {
        public UILayer uiLayer;
        public _3DLayer SceneMgr;

        public SimsGame()
        {
            GameFacade.Game = this;
            Content.RootDirectory = "Content";
            Graphics.SynchronizeWithVerticalRetrace = true; //why was this disabled

            Graphics.PreferredBackBufferWidth = GlobalSettings.GraphicsWidth;
            Graphics.PreferredBackBufferHeight = GlobalSettings.GraphicsHeight;

            Graphics.ApplyChanges();

            Window.Position = new Point(GlobalSettings.GraphicsWidth / 8, GlobalSettings.GraphicsHeight / 8);
            //Log.UseSensibleDefaults();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            TSO.Content.Content.Init(AppDomain.CurrentDomain.BaseDirectory, GraphicsDevice);
            base.Initialize();

            GameFacade.GameThread = Thread.CurrentThread;

            SceneMgr = new _3DLayer();
            SceneMgr.Initialize(GraphicsDevice);

            GameFacade.Controller = new GameController();
            GameFacade.Scenes = SceneMgr;
            GameFacade.Screens = uiLayer;
            GameFacade.GraphicsDevice = GraphicsDevice;
            GameFacade.GraphicsDeviceManager = Graphics;
            GameFacade.Cursor = new CursorManager(this.Window);
            GameFacade.Cursor.Init(TSO.Content.Content.Get().GetPath(""));

            /** Init any computed values **/
            GameFacade.Init();

            GameFacade.Strings = new ContentStrings();
            

            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None }; //no culling until i find a good way to do this in xna4 (apparently recreating state obj is bad?)

            this.IsMouseVisible = true;

            this.IsFixedTimeStep = true;

            WorldContent.Init(this.Services, Content.RootDirectory);


            GlobalSettings.DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Sims Ville";

            if (!Directory.Exists(GlobalSettings.DocumentsPath))
                {
                Directory.CreateDirectory(GlobalSettings.DocumentsPath);
                Directory.CreateDirectory(GlobalSettings.DocumentsPath + "\\Characters");
                Directory.CreateDirectory(GlobalSettings.DocumentsPath + "\\Houses");

                }

            base.Screen.Layers.Add(SceneMgr);
            base.Screen.Layers.Add(uiLayer);
            GameFacade.LastUpdateState = base.Screen.State;
            if (!GlobalSettings.Windowed) Graphics.ToggleFullScreen();
        }

        void RegainFocus(object sender, EventArgs e)
        {
            GameFacade.Focus = true;
        }

        void LostFocus(object sender, EventArgs e)
        {
            GameFacade.Focus = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            try
            {
                GameFacade.MainFont = new TSOVille.Code.UI.Framework.Font();
                GameFacade.MainFont.AddSize(12, Content.Load<SpriteFont>("SimsFont"));
                GameFacade.MainFont.AddSize(16, Content.Load<SpriteFont>("SimsFontBig"));

                uiLayer = new UILayer(this, Content.Load<SpriteFont>("SimsFont"), Content.Load<SpriteFont>("SimsFontBig"));
            }
            catch (Exception)
            {
                //System.Windows.Forms.MessageBox.Show("Content could not be loaded. Make sure that the SimsHomeMaker content has been compiled! (ContentSrc/TSOVilleContent.mgcb)");
                Exit();
            }
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private float m_FPS = 0;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            m_FPS = (float)(1 / gameTime.ElapsedGameTime.TotalSeconds);

            //NetworkFacade.Client.ProcessPackets();
            //GameFacade.SoundManager.MusicUpdate();
            //if (HITVM.Get() != null) HITVM.Get().Tick();

            base.Update(gameTime);
        }
    }
}
