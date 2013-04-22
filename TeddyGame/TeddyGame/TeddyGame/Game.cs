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

        public static Texture2D A_0_0;
        public static Texture2D A_0_720;
        public static Texture2D A_1280_0;
        public static Texture2D A_1280_720;
        public static Texture2D A_2560_1440;
        public static Texture2D A_2560_720;
        public static Texture2D A_3840_1440;
        public static Texture2D A_3840_720;
        public static Texture2D A_5120_1440;
        public static Texture2D A_5120_720;
        public static Texture2D A_M1280_0;
        public static Texture2D A_M1280_720;
                
        /*public static SoundEffect jumpSound;
        public static SoundEffect pistolSound;
        public static SoundEffect dieSound;
        public static SoundEffect hammerSound;
        public static SoundEffect knifeSound;
        public static SoundEffect bulletHit;
        public static SoundEffect bearDeadSound;
        public static SoundEffect bearDeadSound2;
        public static SoundEffect bearDeadSound3;
        public static SoundEffect bearDeadSound4;
        public static SoundEffect bearDeadSound5;
        public static SoundEffect bearShoot;*/



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

            A_0_0 = Content.Load<Texture2D>("level tiles/A_0_0");
            A_0_720 = Content.Load<Texture2D>("level tiles/A_0_720");
            A_1280_0 = Content.Load<Texture2D>("level tiles/A_1280_0");
            A_1280_720 = Content.Load<Texture2D>("level tiles/A_1280_720");
            A_2560_1440 = Content.Load<Texture2D>("level tiles/A_2560_1440");
            A_2560_720 = Content.Load<Texture2D>("level tiles/A_2560_720");
            A_3840_1440 = Content.Load<Texture2D>("level tiles/A_3840_1440");
            A_3840_720 = Content.Load<Texture2D>("level tiles/A_3840_720");
            A_5120_1440 = Content.Load<Texture2D>("level tiles/A_5120_1440");
            A_5120_720 = Content.Load<Texture2D>("level tiles/A_5120_720");
            A_M1280_0 = Content.Load<Texture2D>("level tiles/A_M1280_0");
            A_M1280_720 = Content.Load<Texture2D>("level tiles/A_M1280_720");

            /*jumpSound = Content.Load<SoundEffect>("jumpSound");
            pistolSound = Content.Load<SoundEffect>("pistolSound");
            dieSound = Content.Load<SoundEffect>("dieSound");
            knifeSound = Content.Load<SoundEffect>("knifeSound");
            hammerSound = Content.Load<SoundEffect>("hammerSound");
            bulletHit = Content.Load<SoundEffect>("bulletHit");
            bearDeadSound = Content.Load<SoundEffect>("bearDeadSound");
            bearDeadSound2 = Content.Load<SoundEffect>("bearDeadSound2");
            bearDeadSound3 = Content.Load<SoundEffect>("bearDeadSound3");
            bearDeadSound4 = Content.Load<SoundEffect>("bearDeadSound4");
            bearDeadSound5 = Content.Load<SoundEffect>("bearDeadSound5");
            bearShoot = Content.Load<SoundEffect>("bearShoot"); */      
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
