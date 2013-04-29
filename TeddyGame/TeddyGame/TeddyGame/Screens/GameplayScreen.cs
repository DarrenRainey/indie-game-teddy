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

        private bool finLevel = false;
        private int postWinDelay = 4000;         //in milliseconds

        private Body _circleBody;
        private Body _groundBody;
        private Body playerBody;
        private Body backgroundBody;

        private List<PassableEdge> passableEdges;

        public static CompositeCharacter box;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;
        private Texture2D player;
        private Texture2D playerTexture;
        private Texture2D playerJump;
        private Texture2D playerShoot;
        private Texture2D playerDead;
        private Texture2D playerKnife;
        private Texture2D armgun;
        private Texture2D head;

        /*private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;
        private Cue gameSongsCue;*/

        private Texture2D tile1;
        private Texture2D tile2;

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


        public static Texture2D bearIdle;
        public static Texture2D bearRunning;
        public static Texture2D bearJumping;
        public static Texture2D bearKnife;
        public static Texture2D bearDead;
        private EnemyCompositeCharacter bearBox;

        public static Texture2D bear2Idle;
        public static Texture2D bear2Running;
        public static Texture2D bear2Jumping;
        public static Texture2D bear2ShootDown;
        public static Texture2D bear2ShootDiagDown;
        public static Texture2D bear2ShootAhead;
        public static Texture2D bear2ShootDiagUp;
        public static Texture2D bear2ShootUp;
        public static Texture2D bear2Dead;
        private EnemyCompositeCharacter2 bear2Box;

        public static AudioListener audioListener;
        public const float soundDistanceFactor = 300f;     //the higher this is the further sounds can be heard        

        private List<EnemyCompositeCharacter> enemies;
        private List<EnemyCompositeCharacter2> enemies2;

        int armgunXOffset;
        Vector2 armgunOrigin;

        private Activity oldActivity;

        private int vibrationTime = -1000; //stores vibration time in millis [default = -1000]

        private Texture2D squareTex;

        private Vector2 playerPos;

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
        //private ArrayList markers;
        
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
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
                        
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
            _circleSprite = content.Load<Texture2D>("circleSprite"); //  96px x 96px => 0.96m x 0.96m
            _groundSprite = content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   5.12m x 0.64m            
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
            
            //flashing bullet colors
            playerBulletColors = new List<Color> { Color.Red, Color.WhiteSmoke };
            enemyBulletColors = new List<Color> { Color.Red, Color.Black };

            //armgun stuff init
            armgunXOffset = 0;
            armgunOrigin = Vector2.Zero;                                    

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

            enemies2 = new List<EnemyCompositeCharacter2>();

            //shootbear 1 - just after the first stump
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(3925f, 1010f), 93, 183, 0.15f, squareTex);
            bearBox.forcePower = 100;
            enemies2.Add(bear2Box);

            //shootbear 2 - in front of the large tree
            bear2Box = new EnemyCompositeCharacter2(_world, new Vector2(5460f, 1800f), 93, 183, 0.15f, squareTex);
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
            FixtureFactory.AttachEdge(new Vector2(58.96f, 19.21f), new Vector2(63.11f, 19.28f), impassableEdge);
            //To the ground just past the big tree 

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

                    initEnemy2Script(0);
                    initEnemy2Script(1);  

                    firstGameUpdate = false;
                }

                //check for level finish
                if (box.Position.X > 5650)
                {
                    finLevel = true;
                }

                if (finLevel)
                {
                    postWinDelay -= gameTime.ElapsedGameTime.Milliseconds;
                }

                //change to level 2
                if (postWinDelay <= 0)
                {
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                    
                    MainMenuScreen.gamePlayScreen.killAllEnemyThreads();

                    MainMenuScreen.gamePlayScreen2 = new GameplayScreen2();

                    LoadingScreen.Load(ScreenManager, true, 0, MainMenuScreen.gamePlayScreen2); 
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
                    GameStateManagementGame.pistolSound.Play();

                    //vibration
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


            /*if (state.IsKeyDown(Keys.Left))
            {
                playerAnimation.myEffect = SpriteEffects.FlipHorizontally;
                shootAnimation.myEffect = SpriteEffects.FlipHorizontally;
                armgunEffects = SpriteEffects.FlipHorizontally;
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                playerAnimation.myEffect = SpriteEffects.None;
                shootAnimation.myEffect = SpriteEffects.None;
                armgunEffects = SpriteEffects.None;
            }*/

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

                    //markers.Add(new Vector2(worldPos.X - 4, worldPos.Y - 4));

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
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
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

            if (showBox == true)
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

            _batch.End();

            //draw items attached to screen as aposed to world
            _batch.Begin();

            //Text = "FPS: " + fpsCounter.frameRate + "    player effect: " + playerAnimation.myEffect;

            // Display instructions
            //_batch.DrawString(_font, Text, new Vector2(34f, 34f), Color.Black);
            //_batch.DrawString(_font, Text, new Vector2(32f, 32f), Color.White);

            //_batch.DrawString(_font, box.activity.ToString(), new Vector2(90, 105), Color.Red);

            if (box.activity == Activity.Dead && playerAnimation.currentFrame == 36)   //if on last frame of dead animation
            {
                _batch.DrawString(bigFont, "YOU DIED!", new Vector2(500, 300), Color.Red);

                _batch.DrawString(_font, "Press BACK to retry", new Vector2(490, 370), Color.White);
            }

            if (finLevel)
            {
                _batch.DrawString(bigFont, "LEVEL 1 COMPLETE!", new Vector2(250, 300), Color.Green);
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
