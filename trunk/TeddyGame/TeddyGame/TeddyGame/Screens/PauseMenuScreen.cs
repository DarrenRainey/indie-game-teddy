#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Prototype2
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        #region Initialization

        MenuEntry resumeGameMenuEntry;
        MenuEntry retryGameMenuEntry;
        MenuEntry musicGameMenuEntry;
        MenuEntry quitGameMenuEntry;


        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen()
            : base("-Paused-")
        {
            // Create our menu entries.
            resumeGameMenuEntry = new MenuEntry("Continue");
            retryGameMenuEntry = new MenuEntry("Restart");

            if (GameStateManagementGame.gameSongsCue.IsPaused)
            {
                musicGameMenuEntry = new MenuEntry("Music: Off");                
            }
            else if (GameStateManagementGame.gameSongsCue.IsPlaying)
            {
                musicGameMenuEntry = new MenuEntry("Music: On");              
            } 
                       
            quitGameMenuEntry = new MenuEntry("Quit");
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnResume;
            retryGameMenuEntry.Selected += OnRetry;
            musicGameMenuEntry.Selected += OnMusicOptionsSelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(retryGameMenuEntry);
            MenuEntries.Add(musicGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);

            GameStateManagementGame.menupause.Play();

            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }


        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void OnResume(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuselect.Play();
            
            ExitScreen();
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void OnRetry(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuselect.Play();
            
            MainMenuScreen.gamePlayScreen.killAllEnemyThreads();

            MainMenuScreen.gamePlayScreen = new GameplayScreen();

            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, MainMenuScreen.gamePlayScreen);            
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void OnMusicOptionsSelected(object sender, PlayerIndexEventArgs e)
        {
           GameStateManagementGame.menuselect.Play();
            
           if (GameStateManagementGame.gameSongsCue.IsPaused)
           {
               musicGameMenuEntry.Text = "Music: On";
               
               GameStateManagementGame.gameSongsCue.Resume();
           }
           else if (GameStateManagementGame.gameSongsCue.IsPlaying)
           {
               musicGameMenuEntry.Text = "Music: Off";
               
               GameStateManagementGame.gameSongsCue.Pause();
           }      
        }

        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuback.Play();
            
            MainMenuScreen.gamePlayScreen.killAllEnemyThreads();
            
            GameStateManagementGame.gameSongsCue.Stop(new AudioStopOptions());
               
            GameStateManagementGame.music = 1;
          
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        }

        #endregion
    }
}
