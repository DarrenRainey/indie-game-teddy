#region File Description
///////////////////////////////////////
/* Hugh Desmond - Final year project
 * 
 * version: 1.0
 * release: 7
 * 
 * 
 * Keyboard Controls:
 * 
 * move                    -  arrow left/right
 * jump                    -  space
 * camera                  -  WASD
 * show physics box        -  B
 * code gen mode toggle    -  F
 * generate tarrain code   -  LMB
 * 
 *  
 */
///////////////////////////////////////
#endregion

#region Imports
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Prototype2;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Prototype2
{
    /// <summary>
    /// This screen implements the actual game logic. 
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        const int screenWidth = 1280;
        const int screenHeight = 720;
        Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

        //private Camera2D camera;
        //private List<Scene2DNode> nodeList;

        //private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private MouseState _oldMouseState;
        public SpriteFont _font;
        public SpriteFont bigFont;

        private Boolean firstGameUpdate = true;

        private World _world;
        
        private int gameClock = 0;
        private bool levelFinished = false;

        Vector2 ammoHudPos = new Vector2(128, 72);
        Vector2 clockPos;
        Vector2 enemyHudPos = new Vector2(1047, 72);
          
        private List<PassableEdge> passableEdges;
        private List<Rectangle> deadZones;

        private float cloudIncrement = 0;

        public static CompositeCharacter box;
        
        private Texture2D player;
        private Texture2D playerTexture;
        private Texture2D playerJump;       
        private Texture2D playerDead;
        private Texture2D playerKnife;
        private Texture2D armgun;
        private Texture2D head;

        private Texture2D bg;
        private Texture2D mg;
        float mgYOffset = 0;
        float cloudYOffset = 0;

        private Vector2 bgPos = new Vector2(0, -50);
        private Vector2 bgPosB;
        private Vector2 mgPos;
        private Vector2 mgPosB;
        private Vector2 cloudsPos = new Vector2(1350, 0); 
 
        private Texture2D A_0_0;
        private Texture2D A_0_720;
        private Texture2D A_1280_0;
        private Texture2D A_1280_720;
        private Texture2D A_2560_1440;
        private Texture2D A_2560_720;
        private Texture2D A_3840_1440;
        private Texture2D A_3840_720;
        private Texture2D A_5120_1440;
        private Texture2D A_5120_720;
        private Texture2D A_5120_2160;
        private Texture2D A_6400_1440;
        private Texture2D A_6400_2160;
        private Texture2D A_7680_1440;
        private Texture2D A_7680_2160;
        private Texture2D A_M1280_0;
        private Texture2D A_M1280_720;
        

        private Queue<Bullet> bulletQueue;
        private Array tempBulletArray;
        private Texture2D bulletTex;
        public static Texture2D bulletTex2;

        private SpriteEffects armgunEffects;
        private float armgunAngle;
        private double armgunAngleInDegrees;

        private float headAngle;

        private Animation playerAnimation;
      
        private EnemyCompositeCharacter bearBox;
        private EnemyCompositeCharacter2 bear2Box;

        public static AudioListener audioListener;
        public const float soundDistanceFactor = 300f;     //the higher this is the further sounds can be heard        
            
        private int ammo = 4;
        private Color ammoHudColor = Color.White;

        private int totalEnemies = 0;
        private Color enemyHudColor = Color.White;
        private List<EnemyCompositeCharacter> enemies;
        private List<EnemyCompositeCharacter2> enemies2;

        int armgunXOffset;
        Vector2 armgunOrigin;

        private Activity oldActivity;

        private int vibrationTime = -1000; //stores vibration time in millis [default = -1000]

        private Texture2D squareTex;

        //flashing colour test
        List<Color> playerBulletColors;
        List<Color> enemyBulletColors;

        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private bool freeViewOn = false;            //controllable camera   

        string Text = "BASIC HUD";             

        private bool showBox = false;

        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1 meters equals 100 pixels here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPixels = 100f;

        private Texture2D crosshair;
        private Texture2D marker;

        private Vector2 previousClickWorldPos = Vector2.Zero;
        private List<Vector2> markers;

        private Viewport viewport;
        private Vector2 viewportSize;
        private Vector2 oldCameraPos;
        
        //--------------------------------------------screenmgmt vars
        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);        
        
        Random random = new Random();
        float pauseAlpha;
        //-----------------------------------------------------

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            MainMenuScreen.currentGameScreen = 1;
            
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            GameStateManagementGame.enemiesKilled = 0;
                        
            _world = new World(new Vector2(0, 30f));
        }

        #endregion

         
        #region LoadContent

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {           
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = new Vector2(0f, 0f);

            _screenCenter = new Vector2(GameStateManagementGame.graphics.GraphicsDevice.Viewport.Width / 2f,
                                                GameStateManagementGame.graphics.GraphicsDevice.Viewport.Height / 2f);
            
            _batch = new SpriteBatch(GameStateManagementGame.graphics.GraphicsDevice);
            _font = content.Load<SpriteFont>("font");
            bigFont = content.Load<SpriteFont>("bigFont");

            // Load sprites                     
            player = content.Load<Texture2D>("player1"); // 95px x 80px =>   1m x 1.25m
            armgun = content.Load<Texture2D>("armgun"); // 28px x 67px =>   1m x 1.25m
            head = content.Load<Texture2D>("head"); // 41px x 37px   
            bulletTex = content.Load<Texture2D>("bullet");
            bulletTex2 = content.Load<Texture2D>("bullet2"); 
            squareTex = content.Load<Texture2D>("square");

            playerJump = content.Load<Texture2D>("jumping");
            playerDead = content.Load<Texture2D>("dead");
            playerKnife = content.Load<Texture2D>("knife");         

            //editing textures
            crosshair = content.Load<Texture2D>("crosshair");
            marker = content.Load<Texture2D>("marker");
            
            //background
            bg = content.Load<Texture2D>("level1bg");
            mg = content.Load<Texture2D>("level1mg");
            bgPosB = bgPos + new Vector2(bg.Width, 0f);
            mgPos = Vector2.Zero;
            mgPosB = Vector2.Zero;        //to be changed

            //level tiles
            A_0_0 = content.Load<Texture2D>("level tiles/A_0_0");            
            A_0_720 = content.Load<Texture2D>("level tiles/A_0_720");
            A_1280_0 = content.Load<Texture2D>("level tiles/A_1280_0");
            A_1280_720 = content.Load<Texture2D>("level tiles/A_1280_720");
            A_2560_1440 = content.Load<Texture2D>("level tiles/A_2560_1440");
            A_2560_720 = content.Load<Texture2D>("level tiles/A_2560_720");
            A_3840_1440 = content.Load<Texture2D>("level tiles/A_3840_1440");
            A_3840_720 = content.Load<Texture2D>("level tiles/A_3840_720");
            A_5120_1440 = content.Load<Texture2D>("level tiles/A_5120_1440");
            A_5120_720 = content.Load<Texture2D>("level tiles/A_5120_720");
            A_M1280_0 = content.Load<Texture2D>("level tiles/A_M1280_0");
            A_M1280_720 = content.Load<Texture2D>("level tiles/A_M1280_720");
            A_5120_2160 = content.Load<Texture2D>("level tiles/A_5120_2160");
            A_6400_1440 = content.Load<Texture2D>("level tiles/A_6400_1440");
            A_6400_2160 = content.Load<Texture2D>("level tiles/A_6400_2160");
            A_7680_1440 = content.Load<Texture2D>("level tiles/A_7680_1440");
            A_7680_2160 = content.Load<Texture2D>("level tiles/A_7680_2160");
            
            //flashing bullet colors
            playerBulletColors = new List<Color> { Color.Red, Color.WhiteSmoke };
            enemyBulletColors = new List<Color> { Color.Red, Color.Black };

            //armgun stuff init
            armgunXOffset = 0;
            armgunOrigin = Vector2.Zero;   
            
            //markers
            markers = new List<Vector2>();

            //setup main guy
            playerTexture = content.Load<Texture2D>("run");
            playerAnimation = new Animation();
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 86, 119, 25, 30, Color.White, 1f, true, new Vector2(0, 0));

            box = new CompositeCharacter(_world, new Vector2(300f, 600f), 64, 128, 0.3f, squareTex);
            box.forcePower = 100;

            //create enemies
            enemies = new List<EnemyCompositeCharacter>();

            //knifebear 1 - at start
            bearBox = new EnemyCompositeCharacter(_world, new Vector2(600f, 600f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies.Add(bearBox);

            //knifebear 2 - on top of first hill
            bearBox = new EnemyCompositeCharacter(_world, new Vector2(1330f, 550f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies.Add(bearBox);

            //knifebear 3 - just after second rock
            bearBox = new EnemyCompositeCharacter(_world, new Vector2(3470f, 980f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies.Add(bearBox);

            //knifebear 4 - first sea platform
            bearBox = new EnemyCompositeCharacter(_world, new Vector2(6920f, 1780f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies.Add(bearBox);

            enemies2 = new List<EnemyCompositeCharacter2>();

            //shootbear 1 - just after the first stump
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(3925f, 1010f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies2.Add(bear2Box);

            //shootbear 2 - in front of the large tree
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(5460f, 1800f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies2.Add(bear2Box);

            //shootbear 3 - 2d sea platform
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(7520f, 1580f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies2.Add(bear2Box);

            //shootbear 3 - 3rd sea platform
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(8170f, 1880f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies2.Add(bear2Box);

            //player ignore collisions with enemies
            box.body.CollisionGroup = -1;
            box.wheel.CollisionGroup = -1;

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].body.CollisionGroup = -1;
                enemies[i].wheel.CollisionGroup = -1;
            }

            for (int i = 0; i < enemies2.Count; i++)
            {
                enemies2[i].body.CollisionGroup = -1;
                enemies2[i].wheel.CollisionGroup = -1;
            }


            //setup 3d sound           
            audioListener = new AudioListener();

            //init bullet stuff
            //bulletDirection = new Vector2(0, 0);
            bulletQueue = new Queue<Bullet>();
            tempBulletArray = bulletQueue.ToArray();

            //setup deadzones
            deadZones = new List<Rectangle>();

            deadZones.Add(new Rectangle(2552, 1040, 402, 115));      //first pit  
            deadZones.Add(new Rectangle(6497, 1989, 402, 115));      //2nd  
            deadZones.Add(new Rectangle(7013, 1992, 502, 115));      //3rd pit   
            deadZones.Add(new Rectangle(7654, 2196, 702, 115));      //last pit   


            // Create the ground fixture
            Body impassableEdge = BodyFactory.CreateEdge(_world, new Vector2(0f, 3f), new Vector2(0.25f, 3f));
            impassableEdge.IsStatic = true;
            impassableEdge.Restitution = 0.1f;
            impassableEdge.Friction = 0.7f;


            //From top of first cliff---------------------------------------------------------------------------------------
            FixtureFactory.AttachEdge(new Vector2(-2.06f, 1.215f), new Vector2(-0.32f, 1.185f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(-0.32f, 1.185f), new Vector2(1.29f, 1.285f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.29f, 1.285f), new Vector2(2.37f, 1.315f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.37f, 1.315f), new Vector2(2.2f, 1.585f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.2f, 1.585f), new Vector2(2.08f, 1.645f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.08f, 1.645f), new Vector2(2.03f, 1.885f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.03f, 1.885f), new Vector2(1.69f, 2.005f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.69f, 2.005f), new Vector2(1.47f, 2.225f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.47f, 2.225f), new Vector2(1.44f, 2.425f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.44f, 2.425f), new Vector2(1.34f, 2.735f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.34f, 2.735f), new Vector2(1.41f, 2.965f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.41f, 2.965f), new Vector2(1.38f, 3.225f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.38f, 3.225f), new Vector2(1.35f, 3.585f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.35f, 3.585f), new Vector2(1.36f, 3.885f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.36f, 3.885f), new Vector2(1.42f, 4.155f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.42f, 4.155f), new Vector2(1.56f, 4.405f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.56f, 4.405f), new Vector2(1.68f, 4.585f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.68f, 4.585f), new Vector2(1.56f, 4.825f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.56f, 4.825f), new Vector2(1.71f, 5.275f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.71f, 5.275f), new Vector2(1.81f, 5.405f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.81f, 5.405f), new Vector2(1.86f, 5.575f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.86f, 5.575f), new Vector2(1.98f, 5.755f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(1.98f, 5.755f), new Vector2(2.16f, 6.015f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.16f, 6.015f), new Vector2(2.26f, 6.085f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.26f, 6.085f), new Vector2(2.28f, 6.185f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.28f, 6.185f), new Vector2(2.35f, 6.325f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.35f, 6.325f), new Vector2(2.43f, 6.465f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(2.43f, 6.465f), new Vector2(16.38f, 6.465f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(16.38f, 6.465f), new Vector2(22.68f, 8.915f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(22.68f, 8.915f), new Vector2(23.85f, 8.125f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(23.85f, 8.125f), new Vector2(26.29f, 8.065f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.29f, 8.065f), new Vector2(26.48f, 8.165f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.48f, 8.165f), new Vector2(26.43f, 8.825f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.43f, 8.825f), new Vector2(26.24f, 9.395f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.24f, 9.395f), new Vector2(26.38f, 9.885f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.38f, 9.885f), new Vector2(26.15f, 10.215f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.15f, 10.215f), new Vector2(25.67f, 10.425f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.67f, 10.425f), new Vector2(25.95f, 10.945f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.95f, 10.945f), new Vector2(26.03f, 11.505f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(26.03f, 11.505f), new Vector2(25.8f, 12.015f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.8f, 12.015f), new Vector2(25.65f, 12.775f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.65f, 12.775f), new Vector2(25.66f, 13.225f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.66f, 13.225f), new Vector2(25.65f, 13.845f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.65f, 13.845f), new Vector2(25.62f, 14.405f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.62f, 14.405f), new Vector2(25.74f, 14.115f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(25.74f, 14.115f), new Vector2(21.72f, 16.385f), impassableEdge);
            //To bottom of rock jump on the left---------------------------------------------------------------------------------------

            //From bottom of rock jump on right----------------------------------------------------------------------------------
            FixtureFactory.AttachEdge(new Vector2(29.19f, 14.08f), new Vector2(29.19f, 13.36f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.19f, 13.36f), new Vector2(29.01f, 12.87f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.01f, 12.87f), new Vector2(28.92f, 12.63f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(28.92f, 12.63f), new Vector2(29.11f, 11.9f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.11f, 11.9f), new Vector2(29.18f, 11.23f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.18f, 11.23f), new Vector2(29.39f, 11.11f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.39f, 11.11f), new Vector2(29.28f, 10.72f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.28f, 10.72f), new Vector2(29.4f, 10.14f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.4f, 10.14f), new Vector2(29.55f, 9.99f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(29.55f, 9.99f), new Vector2(30.07f, 9.96f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(30.07f, 9.96f), new Vector2(30.61f, 10.03f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(30.61f, 10.03f), new Vector2(31.33f, 10.06f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(31.33f, 10.06f), new Vector2(31.9f, 10.05f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(31.9f, 10.05f), new Vector2(32.32f, 10.12f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(32.32f, 10.12f), new Vector2(32.67f, 10.29f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(32.67f, 10.29f), new Vector2(32.94f, 10.46f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(32.94f, 10.46f), new Vector2(33.18f, 10.68f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(33.18f, 10.68f), new Vector2(33.63f, 10.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(33.63f, 10.69f), new Vector2(34.57f, 10.7f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(34.57f, 10.7f), new Vector2(35.5f, 10.63f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(35.5f, 10.63f), new Vector2(36.26f, 10.67f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(36.26f, 10.67f), new Vector2(36.83f, 10.62f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(36.83f, 10.62f), new Vector2(36.96f, 10.42f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(36.96f, 10.42f), new Vector2(37.04f, 10.33f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(37.04f, 10.33f), new Vector2(37.07f, 10.21f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(37.07f, 10.21f), new Vector2(37.34f, 10.19f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(37.34f, 10.19f), new Vector2(37.77f, 10.23f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(37.77f, 10.23f), new Vector2(38.19f, 10.27f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(38.19f, 10.27f), new Vector2(38.36f, 10.29f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(38.36f, 10.29f), new Vector2(38.41f, 10.59f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(38.41f, 10.59f), new Vector2(38.61f, 10.77f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(38.61f, 10.77f), new Vector2(39f, 10.87f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(39f, 10.87f), new Vector2(42.73f, 11f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(42.73f, 11f), new Vector2(43.95f, 11f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.95f, 11f), new Vector2(43.79f, 11.63f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.79f, 11.63f), new Vector2(43.72f, 12.17f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.72f, 12.17f), new Vector2(43.68f, 12.78f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.68f, 12.78f), new Vector2(43.66f, 13.58f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.66f, 13.58f), new Vector2(43.7f, 13.87f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(43.7f, 13.87f), new Vector2(43.51f, 14.35f), impassableEdge);
            //To just above the cave, on the cliff vertical edge

            //from tip of big tree branch platform          
            FixtureFactory.AttachEdge(new Vector2(45.928f, 13.036f), new Vector2(46.718f, 13.016f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(46.718f, 13.016f), new Vector2(47.618f, 12.976f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(47.618f, 12.976f), new Vector2(48.728f, 12.796f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(48.728f, 12.796f), new Vector2(48.988f, 12.616f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(48.988f, 12.616f), new Vector2(49.368f, 12.676f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(49.368f, 12.676f), new Vector2(49.348f, 12.306f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(49.348f, 12.306f), new Vector2(49.228f, 11.796f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(49.228f, 11.796f), new Vector2(49.238f, 11.116f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(49.238f, 11.116f), new Vector2(49.078f, 10.146f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(49.078f, 10.146f), new Vector2(48.808f, 9.256f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(48.808f, 9.256f), new Vector2(48.388f, 8.296f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(48.388f, 8.296f), new Vector2(47.718f, 7.056f), impassableEdge);
            //to above big tree

            //from top of cave            
            FixtureFactory.AttachEdge(new Vector2(42.92f, 14.67f), new Vector2(42.47f, 14.98f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(42.47f, 14.98f), new Vector2(42.34f, 15.52f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(42.34f, 15.52f), new Vector2(42.26f, 16.11f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(42.26f, 16.11f), new Vector2(42.28f, 16.56f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(42.28f, 16.56f), new Vector2(44.93f, 16.56f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(44.93f, 16.56f), new Vector2(45.98f, 16.8f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(45.98f, 16.8f), new Vector2(50.18f, 18.92f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(50.18f, 18.92f), new Vector2(57.04f, 19.23f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(57.04f, 19.23f), new Vector2(57f, 18.9f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(57f, 18.9f), new Vector2(57.13f, 18.35f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(57.13f, 18.35f), new Vector2(57.47f, 18.06f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(57.47f, 18.06f), new Vector2(58.06f, 18f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.06f, 18f), new Vector2(58.51f, 18.28f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.51f, 18.28f), new Vector2(58.69f, 18.39f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.69f, 18.39f), new Vector2(58.84f, 18.61f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.84f, 18.61f), new Vector2(58.99f, 18.91f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.99f, 18.91f), new Vector2(58.96f, 19.21f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(58.96f, 19.21f), new Vector2(63.11f, 19.00f), impassableEdge);
            //To the ground just past the big tree 

            //from log to end of the nearest cliff and all ending collisions
            FixtureFactory.AttachEdge(new Vector2(63.11f, 19.00f), new Vector2(65.26f, 18.87f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.26f, 18.87f), new Vector2(65.21f, 19.2f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.21f, 19.2f), new Vector2(65.23f, 19.5f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.23f, 19.5f), new Vector2(65.11f, 19.83f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.11f, 19.83f), new Vector2(65.05f, 20.19f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.05f, 20.19f), new Vector2(65.03f, 20.48f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(65.03f, 20.48f), new Vector2(64.9f, 20.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.9f, 20.69f), new Vector2(64.78f, 21.03f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.78f, 21.03f), new Vector2(64.79f, 21.18f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.79f, 21.18f), new Vector2(64.71f, 21.4f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.71f, 21.4f), new Vector2(64.47f, 22.44f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.47f, 22.44f), new Vector2(64.17f, 23.53f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.17f, 23.53f), new Vector2(64.09f, 23.98f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.09f, 23.98f), new Vector2(64.19f, 24.48f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.19f, 24.48f), new Vector2(64.11f, 24.92f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(64.11f, 24.92f), new Vector2(67.52f, 24.04f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.52f, 24.04f), new Vector2(67.55f, 23.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.55f, 23.69f), new Vector2(67.54f, 23.17f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.54f, 23.17f), new Vector2(67.63f, 22.94f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.63f, 22.94f), new Vector2(67.59f, 22.65f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.59f, 22.65f), new Vector2(67.62f, 22.1f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.62f, 22.1f), new Vector2(67.57f, 21.86f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.57f, 21.86f), new Vector2(67.64f, 21.22f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.64f, 21.22f), new Vector2(67.74f, 20.8f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.74f, 20.8f), new Vector2(67.69f, 20.47f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.69f, 20.47f), new Vector2(67.76f, 19.96f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.76f, 19.96f), new Vector2(67.74f, 19.49f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.74f, 19.49f), new Vector2(67.72f, 19.35f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.72f, 19.35f), new Vector2(67.69f, 19.16f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(67.69f, 19.16f), new Vector2(68.65f, 19.19f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(68.65f, 19.19f), new Vector2(69.49f, 19.17f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(69.49f, 19.17f), new Vector2(70.38f, 19.14f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.38f, 19.14f), new Vector2(70.29f, 19.4f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.29f, 19.4f), new Vector2(70.22f, 19.93f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.22f, 19.93f), new Vector2(70.27f, 20.3f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.27f, 20.3f), new Vector2(70.24f, 20.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.24f, 20.69f), new Vector2(70.13f, 21.25f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.13f, 21.25f), new Vector2(70.09f, 21.88f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.09f, 21.88f), new Vector2(70.15f, 22.33f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.15f, 22.33f), new Vector2(70.09f, 22.64f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.09f, 22.64f), new Vector2(70.13f, 23.02f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.13f, 23.02f), new Vector2(70.03f, 23.39f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.03f, 23.39f), new Vector2(70.06f, 23.95f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.06f, 23.95f), new Vector2(70.05f, 24.34f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(70.05f, 24.34f), new Vector2(72.79f, 25.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.79f, 25.69f), new Vector2(72.87f, 25.48f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.87f, 25.48f), new Vector2(72.86f, 25.18f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.86f, 25.18f), new Vector2(72.87f, 24.99f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.87f, 24.99f), new Vector2(72.86f, 24.46f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.86f, 24.46f), new Vector2(72.85f, 24.25f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.85f, 24.25f), new Vector2(73.14f, 23.35f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.14f, 23.35f), new Vector2(73.04f, 22.64f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.04f, 22.64f), new Vector2(72.92f, 22.17f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.92f, 22.17f), new Vector2(73.01f, 21.91f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.01f, 21.91f), new Vector2(72.95f, 21.7f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(72.95f, 21.7f), new Vector2(73.08f, 21.36f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.08f, 21.36f), new Vector2(73.09f, 20.29f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.09f, 20.29f), new Vector2(73.07f, 20.08f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.07f, 20.08f), new Vector2(73.19f, 19.13f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.19f, 19.13f), new Vector2(73.23f, 18.92f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.23f, 18.92f), new Vector2(73.16f, 18.67f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.16f, 18.67f), new Vector2(73.12f, 18.3f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(73.12f, 18.3f), new Vector2(76.87f, 18.34f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.87f, 18.34f), new Vector2(76.76f, 18.79f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.76f, 18.79f), new Vector2(76.69f, 18.9f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.69f, 18.9f), new Vector2(76.75f, 19.8f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.75f, 19.8f), new Vector2(76.82f, 20.19f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.82f, 20.19f), new Vector2(76.77f, 20.72f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.77f, 20.72f), new Vector2(76.74f, 21.35f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.74f, 21.35f), new Vector2(76.62f, 21.86f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.62f, 21.86f), new Vector2(76.53f, 22.37f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.53f, 22.37f), new Vector2(76.5f, 22.8f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.5f, 22.8f), new Vector2(76.56f, 23f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.56f, 23f), new Vector2(76.59f, 23.27f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.59f, 23.27f), new Vector2(76.63f, 23.41f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.63f, 23.41f), new Vector2(76.59f, 23.58f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.59f, 23.58f), new Vector2(76.6f, 23.86f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.6f, 23.86f), new Vector2(76.62f, 24.36f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.62f, 24.36f), new Vector2(76.58f, 24.48f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.58f, 24.48f), new Vector2(76.56f, 24.78f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.56f, 24.78f), new Vector2(76.53f, 25.29f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.53f, 25.29f), new Vector2(76.52f, 25.69f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.52f, 25.69f), new Vector2(76.47f, 26.12f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.47f, 26.12f), new Vector2(76.47f, 26.45f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.47f, 26.45f), new Vector2(76.41f, 26.79f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.41f, 26.79f), new Vector2(76.47f, 27.1f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.47f, 27.1f), new Vector2(76.46f, 27.38f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(76.46f, 27.38f), new Vector2(80.6f, 26.87f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.6f, 26.87f), new Vector2(80.55f, 26.71f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.55f, 26.71f), new Vector2(80.59f, 26.55f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.59f, 26.55f), new Vector2(80.6f, 26.21f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.6f, 26.21f), new Vector2(80.71f, 25.79f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.71f, 25.79f), new Vector2(80.67f, 25.39f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.67f, 25.39f), new Vector2(80.71f, 24.99f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.71f, 24.99f), new Vector2(80.69f, 24.55f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.69f, 24.55f), new Vector2(80.76f, 24.1f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.76f, 24.1f), new Vector2(80.7f, 23.62f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.7f, 23.62f), new Vector2(80.72f, 23.26f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.72f, 23.26f), new Vector2(80.76f, 22.79f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.76f, 22.79f), new Vector2(80.79f, 22.25f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.79f, 22.25f), new Vector2(80.78f, 21.79f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.78f, 21.79f), new Vector2(80.72f, 21.47f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.72f, 21.47f), new Vector2(80.67f, 21.13f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(80.67f, 21.13f), new Vector2(81.8f, 21.16f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(81.8f, 21.16f), new Vector2(82.78f, 21.22f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(82.78f, 21.22f), new Vector2(83.65f, 21.21f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(83.65f, 21.21f), new Vector2(84.7f, 21.23f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(84.7f, 21.23f), new Vector2(84.57f, 21.98f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(84.57f, 21.98f), new Vector2(84.6f, 22.76f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(84.6f, 22.76f), new Vector2(84.45f, 23.58f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(84.45f, 23.58f), new Vector2(84.42f, 24.41f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(84.42f, 24.41f), new Vector2(84.46f, 25.37f), impassableEdge);

            //play game music            
            if (GameStateManagementGame.music == 0)
            {
                GameStateManagementGame.gameSongsCue = GameStateManagementGame.soundBank.GetCue("gamesongs");
                GameStateManagementGame.gameSongsCue.Play();

                GameStateManagementGame.music = 2;
            }

            firstGameUpdate = true;      

            ScreenManager.Game.ResetElapsedTime();
        }

        #endregion

        #region UnloadContent

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion
        

        private void initEnemyScript(int index)
        {
            switch(index)
            {
                case 0:
                {
                    enemies[index].runScript1();                    
                }
                break;

                case 1:
                {                    
                    enemies[index].runScript2();
                }
                break;

                case 2:
                {                    
                    enemies[index].runScript3();
                }
                break;

                case 3:
                {
                    enemies[index].runScript4();
                }
                break;
            }
        }

        private void initEnemy2Script(int index)
        {
            switch(index)
            {
                case 0:
                {
                    enemies2[index].runScript1();                    
                }
                break;

                case 1:
                {                    
                    enemies2[index].runScript2();
                }
                break;

                case 2:
                {
                    enemies2[index].runScript3();
                }
                break;

                case 3:
                {
                    enemies2[index].runScript4();
                }
                break;
            }
        }        

        #region Update

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {            
            //pause screen fade
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                HandleGamePad();
                HandleKeyboard();
                HandleMouse();                

                handleAttacks();
                handleDeadZones();

                //manage vibration
                if (vibrationTime <= 0 && vibrationTime != -1000)
                {
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);

                    vibrationTime = -1000;
                }
                else if (vibrationTime != -1000)
                {
                    vibrationTime -= gameTime.ElapsedGameTime.Milliseconds;
                }

                //handle main player animations
                if (box.activity == Activity.Idle && oldActivity != Activity.Idle)
                {
                    Vector2 temp = playerAnimation.Position;
                    playerAnimation.Initialize(player, temp, 100, 123, 1, 0, Color.White, 1f, true, new Vector2(0, 0));
                }
                else if (box.activity == Activity.Jumping && oldActivity != Activity.Jumping)
                {
                    Vector2 temp = playerAnimation.Position;
                    playerAnimation.Initialize(playerJump, temp, 120, 125, 23, 10, Color.White, 1f, false, new Vector2(0, 0));
                }
                else if (box.activity == Activity.Running && oldActivity != Activity.Running)
                {
                    Vector2 temp = playerAnimation.Position;
                    playerAnimation.Initialize(playerTexture, temp, 86, 121, 26, 30, Color.White, 1f, true, new Vector2(0, 0));
                }
                else if (box.activity == Activity.Dead && oldActivity != Activity.Dead)
                {
                    Vector2 temp = playerAnimation.Position;
                    playerAnimation.Initialize(playerDead, temp, 103, 140, 37, 80, Color.White, 1f, false, new Vector2(0, 0));

                    GameStateManagementGame.dieSound.Play();

                    GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                    
                    vibrationTime = 600;
                }
                else if (box.activity == Activity.Knife && oldActivity != Activity.Knife)
                {
                    Vector2 temp = playerAnimation.Position;
                    playerAnimation.Initialize(playerKnife, temp, 175, 160, 14, 40, Color.White, 1f, false, new Vector2(0, 0));

                    GameStateManagementGame.hammerSound.Play();

                    GamePad.SetVibration(PlayerIndex.One, 0.6f, 0.6f);

                    vibrationTime = 150;
                }

                //if the knife animation is finished 
                if (box.activity == Activity.Knife && playerAnimation.currentFrame == 13)
                {
                    box.activity = Activity.None;
                }

                //control bear actions
                if (firstGameUpdate)
                {
                    initEnemyScript(0);
                    initEnemyScript(1);
                    initEnemyScript(2);
                    initEnemyScript(3);

                    initEnemy2Script(0);
                    initEnemy2Script(1);
                    initEnemy2Script(2);
                    initEnemy2Script(3);

                    totalEnemies = enemies.Count + enemies2.Count;

                    firstGameUpdate = false;
                }
                
                //change hud colors on certain events
                if (ammo == 3)
                {
                    ammoHudColor = Color.Red;
                }

                if (GameStateManagementGame.enemiesKilled == totalEnemies)
                {
                    enemyHudColor = new Color(0f, 1f, 0f);    //green
                }

                //check for level finish
                if (box.Position.X > 8300)
                {                    
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                    
                    ScreenManager.AddScreen(new LevelCompleteScreen(false, ammo, totalEnemies, ammoHudColor, enemyHudColor, gameClock, ammoHudPos, clockPos, enemyHudPos), ControllingPlayer);

                    levelFinished = true;
                }
                else
                {
                    //update game clock
                    if (gameTime.TotalGameTime.Milliseconds % 1000 == 0)
                    {
                        gameClock++;
                    }
                }              

                oldActivity = box.activity;

                box.Update(gameTime, IsActive);
                playerAnimation.Update(gameTime);

                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].Update(gameTime, IsActive);
                }

                for (int i = 0; i < enemies2.Count; i++)
                {
                    enemies2[i].Update(gameTime, IsActive);
                }

                //update 3d sound listener
                audioListener.Position = new Vector3(box.Position.X, box.Position.Y, 1f) / soundDistanceFactor;

                //We update the world
                _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);
            }
                      
            base.Update(gameTime, otherScreenHasFocus, false);
        }
        
        #endregion

        private void handleDeadZones()
        {
            for (int i = 0; i < deadZones.Count; i++)
            {
                if(box.Position.X + 30 > deadZones[i].X && box.Position.X - 30 < deadZones[i].X + deadZones[i].Width && box.Position.Y + 65 > deadZones[i].Y && box.Position.Y - 65 < deadZones[i].Y + deadZones[i].Height)
                {
                    box.activity = Activity.Dead;
                }
            }
        }

        #region handleAttacks

        private void handleAttacks()
        {
            //handle enemy player hammer attacks - can only knife melee bears from behind
            for (int i = 0; i < enemies.Count; i++)
            {
                if (Math.Abs(enemies[i].Position.X - box.Position.X) < 100 && Math.Abs(enemies[i].Position.Y - box.Position.Y) < 100 && box.activity == Activity.Knife && playerAnimation.currentFrame > 8 && playerAnimation.myEffect.ToString() != enemies[i].animation.myEffect.ToString())
                {                   
                    enemies[i].stopScript();

                    enemies[i].activity = EnemyActivity.enemyDead;

                    break;
                }
            }

            //handle enemy2 player hammer attacks - can knife shoot bears from the front and behind
            for (int i = 0; i < enemies2.Count; i++)
            {                
                if (Math.Abs(enemies2[i].Position.X - box.Position.X) < 100 && Math.Abs(enemies2[i].Position.Y - box.Position.Y) < 100 && box.activity == Activity.Knife && playerAnimation.currentFrame > 8)
                {
                    enemies2[i].stopScript();

                    enemies2[i].activity = EnemyActivity2.enemyDead;

                    break;
                }
            }

            //handle enemy player pistol attacks
            for (int i = 0; i < enemies.Count; i++)
            {                
                for (int j = 0; j < tempBulletArray.Length; j++)
                {
                    if (Math.Abs(((Bullet)tempBulletArray.GetValue(j)).CurrentPos.X - enemies[i].Position.X) < 50 && Math.Abs(((Bullet)tempBulletArray.GetValue(j)).CurrentPos.Y - enemies[i].Position.Y) < 95 && enemies[i].activity != EnemyActivity.enemyDead)
                    {
                        enemies[i].stopScript();

                        enemies[i].activity = EnemyActivity.enemyDead;                                            

                        break;
                    }
                }
            }

            //handle enemy2 player pistol attacks
            for (int i = 0; i < enemies2.Count; i++)
            {                
                for (int j = 0; j < tempBulletArray.Length; j++)
                {
                    if (Math.Abs(((Bullet)tempBulletArray.GetValue(j)).CurrentPos.X - enemies2[i].Position.X) < 50 && Math.Abs(((Bullet)tempBulletArray.GetValue(j)).CurrentPos.Y - enemies2[i].Position.Y) < 95 && enemies2[i].activity != EnemyActivity2.enemyDead)
                    {
                        enemies2[i].stopScript();

                        enemies2[i].activity = EnemyActivity2.enemyDead;

                        break;
                    }
                }
            }

            Boolean breaker = false;

            //handle enemy2 pistol attacks at player
            for (int i = 0; i < enemies2.Count; i++)
            {
                for (int j = 0; j < enemies2[i].tempBulletArray.Length; j++)
                {
                    if (Math.Abs(((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos.X - box.Position.X) < 42 && ((((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos.Y - box.Position.Y > -68 && ((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos.Y - box.Position.Y < 0) || ((((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos.Y - box.Position.Y < 58 && ((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos.Y - box.Position.Y >= 0))) && box.activity != Activity.Dead)
                    {
                        box.activity = Activity.Dead;

                        GameStateManagementGame.bulletHit.Play();

                        breaker = true;
                        
                        break;
                    }
                }

                if (breaker)
                {
                    break;
                }
            }   
        }

        #endregion

        #region handleGamePad

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);                    
                        
            //bullet stuff
            if (bulletQueue.Count > 0)
            {
                tempBulletArray = bulletQueue.ToArray();
                bulletQueue.Clear();                                  //convert to array, clear the queue and fill it up after changes

                for (int i = 0; i < tempBulletArray.Length; i++)
                {
                    ((Bullet)tempBulletArray.GetValue(i)).CurrentPos += ((Bullet)tempBulletArray.GetValue(i)).DirectionIncrement;

                    bulletQueue.Enqueue((Bullet)tempBulletArray.GetValue(i));
                }

                if (((Bullet)bulletQueue.Peek()).isDead())        //remove dead bullets from the queue
                {
                    bulletQueue.Dequeue();
                }

                tempBulletArray = bulletQueue.ToArray();
            }

            //back button resets the player
            if (box.activity == Activity.Dead && padState.Buttons.Back == ButtonState.Pressed && _oldPadState.Buttons.Back == ButtonState.Released && padState.IsConnected)   
            {
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                
                MainMenuScreen.gamePlayScreen.killAllEnemyThreads();
                
                MainMenuScreen.gamePlayScreen = new GameplayScreen();

                LoadingScreen.Load(ScreenManager, true, 0, MainMenuScreen.gamePlayScreen);                
            }

            if (padState.IsConnected && box.activity != Activity.Dead)       
            {               

                if (padState.Buttons.B == ButtonState.Pressed && _oldPadState.Buttons.B == ButtonState.Released)
                {
                    /*if (showBox == true)
                    {
                        showBox = false;
                    }
                    else
                    {
                        showBox = true;
                    }   */                 
                }

                if (padState.Triggers.Right > 0.5 && _oldPadState.Triggers.Right < 0.5)
                {
                    if (ammo > 0)
                    {
                        GameStateManagementGame.pistolSound.Play();                        

                        //vibration to the side of shooting
                        if (padState.ThumbSticks.Right.X < 0)
                        {
                            GamePad.SetVibration(PlayerIndex.One, Math.Abs(padState.ThumbSticks.Right.X / 2), 0.0f);
                        }
                        else
                        {
                            GamePad.SetVibration(PlayerIndex.One, 0.0f, (padState.ThumbSticks.Right.X / 2));
                        }

                        vibrationTime = 300;

                        Bullet bullet = new Bullet();
                        bullet.Texture = bulletTex;

                        if (padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                        {
                            bullet.DirectionIncrement = (padState.ThumbSticks.Right * new Vector2(1, -1));
                        }
                        else
                        {
                            if (armgunEffects == SpriteEffects.None)
                            {
                                bullet.DirectionIncrement = new Vector2(1, 0);
                            }
                            else if (armgunEffects == SpriteEffects.FlipHorizontally)
                            {
                                bullet.DirectionIncrement = new Vector2(-1, 0);
                            }
                        }

                        Vector2 perpToDirection;

                        //arm pivot point + direction multiplyed by guess number to bring the bullet origin to the tip of the pistol. 
                        //also moved a few pixels 90 degrees perpendicular to direction to match the tip of the gun  
                        if (armgunEffects == SpriteEffects.None)
                        {
                            perpToDirection = new Vector2(bullet.DirectionIncrement.Y, bullet.DirectionIncrement.X * -1) * 12;

                            bullet.Origin = new Vector2(box.Position.X + 10, box.Position.Y - 20) + (bullet.DirectionIncrement * 55) + perpToDirection;
                        }
                        else if (armgunEffects == SpriteEffects.FlipHorizontally)
                        {
                            perpToDirection = new Vector2(bullet.DirectionIncrement.Y * -1, bullet.DirectionIncrement.X) * 12;

                            bullet.Origin = new Vector2(box.Position.X - 20, box.Position.Y - 20) + (bullet.DirectionIncrement * 55) + perpToDirection;
                        }

                        bullet.CurrentPos = bullet.Origin;
                        bullet.DirectionIncrement *= bullet.Speed;

                        bulletQueue.Enqueue(bullet);    //add the bullet to the queue of bullets

                        ammo--;
                    }
                    else
                    {
                        GameStateManagementGame.clickSound.Play();
                    }
                }

                if (padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                {
                    armgunAngle = (float)Math.Atan2(padState.ThumbSticks.Right.Y, padState.ThumbSticks.Right.X) * -1;

                    armgunAngleInDegrees = (armgunAngle * 180) / Math.PI;

                    if (armgunAngleInDegrees > 90 || armgunAngleInDegrees < -90)
                    {
                        playerAnimation.myEffect = SpriteEffects.FlipHorizontally;
                        armgunEffects = SpriteEffects.FlipHorizontally;
                    }
                    else
                    {
                        playerAnimation.myEffect = SpriteEffects.None;
                        armgunEffects = SpriteEffects.None;
                    }
                }
                else
                {                   
                    armgunAngle = 0.0f;
                    headAngle = 0.0f;                   
                }                

                //run animation speed is determined by the degree to which the analog stick is tilted
                if(box.activity == Activity.Running)
                {                    
                    if (padState.ThumbSticks.Left.X < -0.1f || padState.ThumbSticks.Left.X > 0.1f)
                    {
                        playerAnimation.frameTime = (int)(playerAnimation.minFrameTime * (1 / Math.Abs(padState.ThumbSticks.Left.X)));
                    }                    
                } 

                //camera follow player  
                if (freeViewOn == false)
                {
                    _cameraPosition = (box.Position - _screenCenter) * -1;
                    _cameraPosition += new Vector2(-130,130);
                    
                    //BG stuff..
                    bgPos.X += (_cameraPosition.X - oldCameraPos.X) / 9;           //move the bg slower then the foreground

                    if (bgPos.X > 1f)
                    {
                        bgPosB = bgPos - new Vector2(bg.Width, 0f);
                    }
                    else
                    {
                        bgPosB = bgPos + new Vector2(bg.Width , 0f);
                    }

                    if (bgPosB.X > 1f)
                    {
                        bgPos = bgPosB - new Vector2(bg.Width, 0f);
                    }
                    else
                    {
                        bgPos = bgPosB + new Vector2(bg.Width, 0f);
                    }

                    //Middle Ground Stuff...
                    mgPos.X += (_cameraPosition.X - oldCameraPos.X) / 4;           

                    if (mgPos.X > 1f)
                    {
                        mgPosB = mgPos - new Vector2(mg.Width, 0f);
                    }
                    else
                    {
                        mgPosB = mgPos + new Vector2(mg.Width, 0f);
                    }

                    if (mgPosB.X > 1f)
                    {
                        mgPos = mgPosB - new Vector2(mg.Width, 0f);
                    }
                    else
                    {
                        mgPos = mgPosB + new Vector2(mg.Width, 0f);
                    }
                    
                    bgPos.Y += (_cameraPosition.Y - oldCameraPos.Y) / 5;
                    bgPosB.Y = bgPos.Y;

                    mgYOffset += (_cameraPosition.Y - oldCameraPos.Y) / 10;
                    mgPos.Y = bgPos.Y + 920 + mgYOffset;
                    mgPosB.Y = mgPos.Y;

                    //move clouds                    
                    cloudsPos.X = (bgPos.X + 1280) + cloudIncrement;
                    cloudYOffset -= (_cameraPosition.Y - oldCameraPos.Y) / 30;
                    cloudsPos.Y = bgPos.Y + 200 + cloudYOffset;

                    cloudIncrement -= 0.4f;

                    if (cloudsPos.X < (-GameStateManagementGame.clouds.Width + -200))           //rest clouds once all shown
                    {
                        cloudIncrement = 300;
                    }

                    oldCameraPos = _cameraPosition;
                }

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        #endregion  

        #region handleKeyboard

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();


            // Move camera
            if (state.IsKeyDown(Keys.A))
                _cameraPosition.X += 5f;

            if (state.IsKeyDown(Keys.D))
                _cameraPosition.X -= 5f;

            if (state.IsKeyDown(Keys.W))
                _cameraPosition.Y += 5f;

            if (state.IsKeyDown(Keys.S))
                _cameraPosition.Y -= 5f;

            _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                    Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));


            if (state.IsKeyDown(Keys.B) && _oldKeyState.IsKeyDown(Keys.B) == false)
            {
                if (showBox == true)
                {
                    showBox = false;
                }
                else
                {
                    showBox = true;
                }
            }

            if (state.IsKeyDown(Keys.F) && _oldKeyState.IsKeyDown(Keys.F) == false)       //f to toggle freeview on/off
            {
                if (freeViewOn == true)
                {
                    freeViewOn = false;
                }
                else
                {
                    freeViewOn = true;
                }
            }

            /*if (state.IsKeyDown(Keys.Escape))
                Exit();*/

            _oldKeyState = state;
        }

        #endregion

        #region handleMouse

        private void HandleMouse()
        {
            MouseState state = Mouse.GetState();

            if (freeViewOn)
            {
                //generate edge code from click coordinates
                if (state.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
                {
                    Vector2 worldPos = (_cameraPosition * -1) + new Vector2(state.X, state.Y);
                    worldPos = new Vector2((float)Math.Round(worldPos.X, 1), (float)Math.Round(worldPos.Y, 1));                    
                    
                    Console.WriteLine("FixtureFactory.AttachEdge(new Vector2(" + previousClickWorldPos.X / MeterInPixels + "f, " + previousClickWorldPos.Y / MeterInPixels + "f), new Vector2(" + worldPos.X / MeterInPixels + "f, " + worldPos.Y / MeterInPixels + "f), impassableEdge);");                    
                    
                    markers.Add(new Vector2(worldPos.X - 4, worldPos.Y - 4));                    
                    previousClickWorldPos = worldPos;
                }
            }

            _oldMouseState = state;
        }

        #endregion

        #region HandleInput

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if they unplug the active gamepad. 
            bool gamePadDisconnected = !gamePadState.IsConnected && input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(false), ControllingPlayer);
            }            
        }

        #endregion  


        #region Draw
 
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {            
            GameStateManagementGame.graphics.GraphicsDevice.Clear(Color.SkyBlue);

            viewport = GameStateManagementGame.graphics.GraphicsDevice.Viewport;
            viewportSize = new Vector2(viewport.Width, viewport.Height);

            //draw parralax bg
            _batch.Begin();

            _batch.Draw(bg, bgPos, Color.White);
            _batch.Draw(bg, bgPosB, Color.White);

            _batch.Draw(mg, mgPos, Color.White);
            _batch.Draw(mg, mgPosB, Color.White);
            
            //draw clouds
            _batch.Draw(GameStateManagementGame.clouds, cloudsPos, Color.White);

            _batch.End();


            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);
            
            _batch.Draw(A_0_0, new Vector2(0, 0), Color.White);
            _batch.Draw(A_0_720, new Vector2(0, 720), Color.White);
            _batch.Draw(A_1280_0, new Vector2(1280, 0), Color.White);
            _batch.Draw(A_1280_720, new Vector2(1280, 720), Color.White);
            _batch.Draw(A_2560_1440, new Vector2(2560, 1440), Color.White);
            _batch.Draw(A_2560_720, new Vector2(2560, 720), Color.White);
            _batch.Draw(A_3840_1440, new Vector2(3840, 1440), Color.White);
            _batch.Draw(A_3840_720, new Vector2(3840, 720), Color.White);
            _batch.Draw(A_5120_1440, new Vector2(5120, 1440), Color.White);
            _batch.Draw(A_5120_720, new Vector2(5120, 720), Color.White);
            _batch.Draw(A_M1280_0, new Vector2(-1280, 0), Color.White);
            _batch.Draw(A_M1280_720, new Vector2(-1280, 720), Color.White);
            _batch.Draw(A_5120_2160, new Vector2(5120, 2160), Color.White);
            _batch.Draw(A_6400_1440, new Vector2(6400, 1440), Color.White);
            _batch.Draw(A_6400_2160, new Vector2(6400, 2160), Color.White);
            _batch.Draw(A_7680_1440, new Vector2(7680, 1440), Color.White);
            _batch.Draw(A_7680_2160, new Vector2(7680, 2160), Color.White);

            if (freeViewOn)
            {
                box.Draw(_batch);

                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].Draw(_batch);
                }

                for (int i = 0; i < enemies2.Count; i++)
                {
                    enemies2[i].Draw(_batch);
                }

                //test deadzones
                for (int i = 0; i < deadZones.Count; i++)
                {
                    _batch.Draw(squareTex, deadZones[i], Color.DarkRed);
                }
            }            

            //draw enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].drawAnimation(gameTime, _batch, IsActive);
            }

            for (int i = 0; i < enemies2.Count; i++)
            {
                enemies2[i].drawAnimation(gameTime, _batch, IsActive);
            }

            if (IsActive)
            {
                if (armgunEffects == SpriteEffects.None)
                {
                    armgunXOffset = 17;
                    armgunOrigin = new Vector2(6, 17);
                }
                else if (armgunEffects == SpriteEffects.FlipHorizontally)
                {
                    armgunXOffset = -18;
                    armgunOrigin = new Vector2(60, 17);

                    GamePadState padState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

                    if (padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                    {
                        if (armgunAngleInDegrees < -180 || armgunAngleInDegrees > -90)
                        {
                            armgunAngleInDegrees = -180 + armgunAngleInDegrees;

                            armgunAngle = (float)(armgunAngleInDegrees * Math.PI) / 180;
                        }
                        else if (armgunAngleInDegrees < 180 || armgunAngleInDegrees > 90)
                        {
                            armgunAngleInDegrees = (armgunAngleInDegrees + 180);

                            armgunAngle = (float)(armgunAngleInDegrees * Math.PI) / 180;
                        }
                    }
                }

                headAngle = armgunAngle / 3;             //head rotates with analog stick but 3 times slower
            }           

            if (box.activity != Activity.Dead)
            {
                _batch.Draw(armgun, new Rectangle((int)box.Position.X + armgunXOffset, (int)box.Position.Y - 15, armgun.Width, armgun.Height), new Rectangle(0, 0, armgun.Width, armgun.Height), Color.White, armgunAngle, armgunOrigin, armgunEffects, 0.0f);
            }

            if (IsActive)
            {
                box.Update(gameTime, IsActive);
                playerAnimation.Position = box.Position;
                playerAnimation.Update(gameTime);
            }

            playerAnimation.Draw(_batch);

            if (box.activity != Activity.Dead && box.activity != Activity.Knife)
            {
                _batch.Draw(head, new Rectangle((int)playerAnimation.Position.X + 0, (int)playerAnimation.Position.Y - 35, head.Width, head.Height), new Rectangle(0, 0, head.Width, head.Height), Color.White, headAngle, new Vector2(20, 18), armgunEffects, 0.0f);
            }


            //TESTING POSITIONS
            //_batch.Draw(crosshair, box.Position, Color.White);
            //_batch.Draw(crosshair, enemies[0].Position, Color.White); 


            for (int i = 0; i < tempBulletArray.Length; i++)
            {
                _batch.Draw(((Bullet)tempBulletArray.GetValue(i)).Texture, ((Bullet)tempBulletArray.GetValue(i)).CurrentPos, getRandomColor(playerBulletColors));
            }

            //draw enemy bullets
            for (int i = 0; i < enemies2.Count; i++)
            {
                for (int j = 0; j < enemies2[i].tempBulletArray.Length; j++)
                {
                    _batch.Draw(((Bullet)enemies2[i].tempBulletArray.GetValue(j)).Texture, ((Bullet)enemies2[i].tempBulletArray.GetValue(j)).CurrentPos, getRandomColor(enemyBulletColors));
                }
            }
                        
            if (freeViewOn)
            {                
                //draw markers
                for (int i = 0; i < markers.Count; i++)
                {
                    _batch.Draw(marker, (Vector2)markers[i], Color.White);
                }
            }   

            _batch.End();

            //draw items attached to screen as aposed to world
            _batch.Begin();

            if (!levelFinished)
            {
                //draw hud
                _batch.Draw(GameStateManagementGame.ammoHUD, ammoHudPos, Color.White);
                _batch.DrawString(_font, "" + ammo, ammoHudPos + new Vector2(62 + 1, 6 + 1), Color.Black);
                _batch.DrawString(_font, "" + ammo, ammoHudPos + new Vector2(62, 6), ammoHudColor);

                clockPos = new Vector2(((viewportSize.X - bigFont.MeasureString(gameClock.ToString()).X) / 2), 50f);

                _batch.DrawString(bigFont, gameClock.ToString(), clockPos + new Vector2(1, 1), Color.Black);
                _batch.DrawString(bigFont, gameClock.ToString(), clockPos, Color.White);

                _batch.Draw(GameStateManagementGame.bearHUD, enemyHudPos, Color.White);
                _batch.DrawString(_font, "" + GameStateManagementGame.enemiesKilled + "/" + totalEnemies, enemyHudPos + new Vector2(63 + 1, 10 + 1), Color.Black);
                _batch.DrawString(_font, "" + GameStateManagementGame.enemiesKilled + "/" + totalEnemies, enemyHudPos + new Vector2(63, 10), enemyHudColor);
            }

            if (box.activity == Activity.Dead && playerAnimation.currentFrame == 36)   //if on last frame of dead animation
            {
                _batch.DrawString(bigFont, "YOU DIED!", new Vector2(501, 301), Color.Black);
                _batch.DrawString(bigFont, "YOU DIED!", new Vector2(500, 300), Color.Red);

                _batch.DrawString(_font, "Press BACK to retry", new Vector2(491, 371), Color.Black);
                _batch.DrawString(_font, "Press BACK to retry", new Vector2(490, 370), Color.White);
            }

            //draw crosshair at mouse
            if (freeViewOn)
            {
                _batch.Draw(crosshair, new Vector2(Mouse.GetState().X - 4, Mouse.GetState().Y - 4), Color.White);
            }            

            _batch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

            base.Draw(gameTime);
        }

        #endregion


        #region useful methods

        private Color getRandomColor(List<Color> colors)
        {
            if (colors.Count == 0)
            {
                return Color.White;
            }

            return colors[new Random().Next(0, colors.Count)];
        }

        private float DegreeToRadian(float angle)
        {
            float PI = (float)Math.PI;

            return PI * angle / 180.0f;
        }

        public void killAllEnemyThreads()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].stopScript();
            }

            for (int i = 0; i < enemies2.Count; i++)
            {
                enemies2[i].stopScript();
            }
        }

        #endregion
    }
}
