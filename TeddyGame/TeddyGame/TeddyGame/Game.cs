#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Prototype2
{
    /// <summary>
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class GameStateManagementGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        public static GraphicsDeviceManager graphics;
        public static AudioEngine audioEngine;
        public static WaveBank waveBank;
        public static SoundBank soundBank;
        public static Cue gameSongsCue;

        public static int level1Time;
        public static int level2Time;

        public static int enemiesKilled;
              
        public static Texture2D bearHUD;
        public static Texture2D ammoHUD;

        public static Texture2D menubg;
        public static Texture2D menubox;
        public static Texture2D exitbox;
        public static Texture2D controls;
        public static Texture2D levelCompleteBox;
        public static Texture2D levelCompleteBanner;
        public static Texture2D pressstart;
        public static Texture2D menuman;
        public static Texture2D menutitle;
        public static Texture2D menusubtitle;
        public static Texture2D buttonA;
        public static Texture2D buttonB;

        public static Texture2D finScreen;
        public static Texture2D gameCompleteBanner;

        public static SpriteFont smallFont;
        public static SpriteFont bigFont;
        public static SpriteFont font;

        //load bear animations
        public static Texture2D bearIdle;  
        public static Texture2D bearRunning;            
        public static Texture2D bearJumping;            
        public static Texture2D bearKnife;     
        public static Texture2D bearDead;

        public static Texture2D bear2Idle;
        public static Texture2D bear2Running;
        public static Texture2D bear2Jumping;
        public static Texture2D bear2ShootDown;
        public static Texture2D bear2ShootDiagDown;
        public static Texture2D bear2ShootAhead;
        public static Texture2D bear2ShootDiagUp;
        public static Texture2D bear2ShootUp;
        public static Texture2D bear2Dead;

        public static SoundEffect menuup;
        public static SoundEffect menudown;
        public static SoundEffect menuselect;
        public static SoundEffect menustart;
        public static SoundEffect menuback;
        public static SoundEffect menucancel;
        public static SoundEffect menuplay;
        public static SoundEffect menupause;

        public static SoundEffect finLevelSound;
        public static SoundEffect totSound;

        public static SoundEffect jumpSound;
        public static SoundEffect pistolSound;
        public static SoundEffect clickSound;
        public static SoundEffect dieSound;
        public static SoundEffect hammerSound;
        public static SoundEffect knifeSound;
        public static SoundEffect bulletHit;
        public static SoundEffect bearDeadSound;
        public static SoundEffect bearDeadSound2;
        public static SoundEffect bearDeadSound3;
        public static SoundEffect bearDeadSound4;
        public static SoundEffect bearDeadSound5;
        public static SoundEffect bearShoot;



        public static int music = 1;   //0 = none, 1 = menu, 2 = game

        ScreenManager screenManager;


        // By preloading any assets used by UI rendering, we avoid framerate glitches
        // when they suddenly need to be loaded in the middle of a menu transition.
        static readonly string[] preloadAssets =
        {
            "gradient",
        };

        
        #endregion

        #region Initialization


        /// <summary>
        /// The main game constructor.
        /// </summary>
        public GameStateManagementGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
        }


        /// <summary>
        /// Loads graphics content.
        /// </summary>
        protected override void LoadContent()
        {            
            foreach (string asset in preloadAssets)
            {
                Content.Load<object>(asset);
            }

            audioEngine = new AudioEngine("Content/teddyMusic.xgs");
            waveBank = new WaveBank(audioEngine, "Content/playlist.xwb");
            soundBank = new SoundBank(audioEngine, "Content/playlist.xsb");
            gameSongsCue = soundBank.GetCue("menumusic");

            bearHUD = Content.Load<Texture2D>("bearhead");
            ammoHUD = Content.Load<Texture2D>("ammo");

            menubg = Content.Load<Texture2D>("menubg");
            menubox = Content.Load<Texture2D>("menubox");
            exitbox = Content.Load<Texture2D>("exitbox");
            controls = Content.Load<Texture2D>("controls");
            levelCompleteBox = Content.Load<Texture2D>("LevelCompleteBox");
            levelCompleteBanner = Content.Load<Texture2D>("levelCompleteBanner");
            pressstart = Content.Load<Texture2D>("pressstart");
            menuman = Content.Load<Texture2D>("menuman");
            menutitle = Content.Load<Texture2D>("menutitle");
            menusubtitle = Content.Load<Texture2D>("menusubtitle");
            buttonA = Content.Load<Texture2D>("buttona");
            buttonB = Content.Load<Texture2D>("buttonb");

            finScreen = Content.Load<Texture2D>("finscreen");
            gameCompleteBanner = Content.Load<Texture2D>("gameCompleteBanner");

            smallFont = Content.Load<SpriteFont>("smallfont");
            font = Content.Load<SpriteFont>("font");
            bigFont = Content.Load<SpriteFont>("bigFont");

            //troublesome loads
            bearIdle = Content.Load<Texture2D>("bearidle");
            bearRunning = Content.Load<Texture2D>("bearrun");
            bearJumping = Content.Load<Texture2D>("bearjump");
            bearKnife = Content.Load<Texture2D>("bearknife");
            bearDead = Content.Load<Texture2D>("beardead");

            bear2Idle = Content.Load<Texture2D>("bear2idle");       
            bear2Running = Content.Load<Texture2D>("bear2run");
            bear2Jumping = Content.Load<Texture2D>("bear2jump");
            bear2ShootDown = Content.Load<Texture2D>("bear2shootdown");       
            bear2ShootDiagDown = Content.Load<Texture2D>("bear2shootdiagdown");
            bear2ShootAhead = Content.Load<Texture2D>("bear2shootahead");
            bear2ShootDiagUp = Content.Load<Texture2D>("bear2shootdiagup");
            bear2ShootUp = Content.Load<Texture2D>("bear2shootup");
            bear2Dead = Content.Load<Texture2D>("bear2dead");

            menuup = Content.Load<SoundEffect>("menuup");
            menudown = Content.Load<SoundEffect>("menudown");
            menucancel = Content.Load<SoundEffect>("menucancel");
            menuselect = Content.Load<SoundEffect>("menuselect");
            menustart = Content.Load<SoundEffect>("menustart");
            menuback = Content.Load<SoundEffect>("menuback");
            menuplay = Content.Load<SoundEffect>("menuplay");
            menupause = Content.Load<SoundEffect>("menupause");
            
            finLevelSound = Content.Load<SoundEffect>("finLevel");
            totSound = Content.Load<SoundEffect>("tot");

            jumpSound = Content.Load<SoundEffect>("jumpSound");
            pistolSound = Content.Load<SoundEffect>("pistolSound");
            clickSound = Content.Load<SoundEffect>("click");
            dieSound = Content.Load<SoundEffect>("dieSound");
            knifeSound = Content.Load<SoundEffect>("knifeSound");
            hammerSound = Content.Load<SoundEffect>("hammerSound");
            bulletHit = Content.Load<SoundEffect>("bulletHit");
            bearDeadSound = Content.Load<SoundEffect>("bearDeadSound");
            bearDeadSound2 = Content.Load<SoundEffect>("bearDeadSound2");
            bearDeadSound3 = Content.Load<SoundEffect>("bearDeadSound3");
            bearDeadSound4 = Content.Load<SoundEffect>("bearDeadSound4");
            bearDeadSound5 = Content.Load<SoundEffect>("bearDeadSound5");
            bearShoot = Content.Load<SoundEffect>("bearShoot");       
        }


        #endregion
       

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (GameStateManagementGame game = new GameStateManagementGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
