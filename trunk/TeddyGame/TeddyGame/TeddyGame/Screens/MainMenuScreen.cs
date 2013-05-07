#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
#endregion

namespace Prototype2
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization

        public static GameplayScreen gamePlayScreen;
        public static GameplayScreen2 gamePlayScreen2;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("")
        {
            gamePlayScreen = new GameplayScreen();
            front = true;           

            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Play");
            MenuEntry controlsMenuEntry = new MenuEntry("Controls");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            controlsMenuEntry.Selected += controlsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(controlsMenuEntry);
            MenuEntries.Add(exitMenuEntry);           
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuplay.Play();
            
            GameStateManagementGame.gameSongsCue.Stop(new AudioStopOptions());

            GameStateManagementGame.music = 0;
            
            //LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, gamePlayScreen);
            LoadingScreen.Load(ScreenManager, false, null, new GameCompleteScreen());
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void controlsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuselect.Play();
            
            ScreenManager.AddScreen(new ControlsScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            GameStateManagementGame.menucancel.Play();
            
            const string message = "So the teddy bears got to you!?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            GameStateManagementGame.menuselect.Play();
            
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
