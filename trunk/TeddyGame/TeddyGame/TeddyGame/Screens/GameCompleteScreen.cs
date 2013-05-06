#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Prototype2
{    
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class GameCompleteScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        Vector2 titlePos = new Vector2(0, 80f);
        Vector2 gradeBoxPos = new Vector2(300, 250f);

        Vector2 level1TotOffset = new Vector2(30, 70f);
        Vector2 level2TotOffset = new Vector2(30, 100f);
        Vector2 totalTimeLabelOffset = new Vector2(150, 40f);
        Vector2 totalTimeOffset = new Vector2(180, 80f);

        int totalTime = 0;
        
       

        #endregion

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameCompleteScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(2);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");                        
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if ((input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.IsMenuSelect(ControllingPlayer, out playerIndex)))
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, new PlayerIndexEventArgs(playerIndex));

                GameStateManagementGame.menuselect.Play();

                ExitScreen();               

                GameStateManagementGame.gameSongsCue.Stop(new AudioStopOptions());

                GameStateManagementGame.music = 1;

                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
            }
        }


        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

           
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            
             
            /*
            //subtitle wiggling
            pulsate2 = (float)Math.Sin(time * 2.5) + 1;
            subTitleRotation = subtitleStartAngle - ((Single)pulsate2 * 0.03f);
            rotateOrigin = new Vector2(GameStateManagementGame.menusubtitle.Width / 2, GameStateManagementGame.menusubtitle.Height / 2);

            //man pulsating
            pulsate3 = (float)Math.Sin(time * 0.3) + 1;
            manScale = 1 - ((Single)pulsate3 * 0.035f); 
            */


            //draw pulsating title            
            double time = gameTime.TotalGameTime.TotalSeconds;
            float pulsate = (float)Math.Sin(time * 3) + 1;
            float titleScale = 1 - (pulsate * 0.04f);

            spriteBatch.Begin();
            
            spriteBatch.Draw(GameStateManagementGame.finScreen, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            titlePos.X = (viewportSize.X - (GameStateManagementGame.gameCompleteBanner.Width * titleScale)) / 2;
            spriteBatch.Draw(GameStateManagementGame.gameCompleteBanner, titlePos, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0);

            // Draw the background rectangle.
            spriteBatch.Draw(GameStateManagementGame.levelCompleteBox, gradeBoxPos, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha / 2f));

            spriteBatch.DrawString(GameStateManagementGame.smallFont, "Level 1: " + GameStateManagementGame.level1Time + "secs", gradeBoxPos + level1TotOffset, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.DrawString(GameStateManagementGame.smallFont, "Level 2: " + GameStateManagementGame.level2Time + "secs", gradeBoxPos + level2TotOffset, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.DrawString(GameStateManagementGame.font, "Total Time: ", gradeBoxPos + totalTimeLabelOffset, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.DrawString(GameStateManagementGame.bigFont, totalTime.ToString(), gradeBoxPos + totalTimeOffset, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            /*
            manPos.X = (viewportSize.X - (GameStateManagementGame.menuman.Width * manScale)) / 2;
            float manY = manPos.Y + ((GameStateManagementGame.menuman.Height - (GameStateManagementGame.menuman.Height * manScale)) / 2);
            spriteBatch.Draw(GameStateManagementGame.menuman, new Vector2(manPos.X, manY), null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, manScale, SpriteEffects.None, 0);    //draw man                     

            titlePos.X = (viewportSize.X - (GameStateManagementGame.menutitle.Width * titleScale)) / 2;
            spriteBatch.Draw(GameStateManagementGame.menutitle, titlePos, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0);

            spriteBatch.Draw(GameStateManagementGame.menusubtitle, subtitlePos + new Vector2(GameStateManagementGame.menusubtitle.Width / 2, GameStateManagementGame.menusubtitle.Height / 2), null,
                             new Color(subtitleAlpha, subtitleAlpha, subtitleAlpha, subtitleAlpha), subTitleRotation, rotateOrigin, 1f, SpriteEffects.None, 0);    //draw subtitle
            */
            spriteBatch.End();
        }


        #endregion
    }
}
