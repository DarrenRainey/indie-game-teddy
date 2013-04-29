#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
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
#endregion

namespace Prototype2
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class ControlsScreen : GameScreen
    {
        #region Fields

        string message;
        Texture2D gradientTexture;
        Viewport viewport;
        Vector2 viewportSize;
        Vector2 popupPos;
        String prefix = "HINT:";
        String tip = "";
        int timeSinceLastTip;
        int tipGap = 9500;

        #endregion

        #region Events

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #endregion

        #region Initialization
       

        public ControlsScreen()
        {            
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.35);
            TransitionOffTime = TimeSpan.FromSeconds(0.35);

            timeSinceLastTip = tipGap;
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
        }


        #endregion

        #region Handle Input


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
            if (input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, new PlayerIndexEventArgs(playerIndex));

                GameStateManagementGame.menuback.Play();

                ExitScreen();
            }
        }


        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (timeSinceLastTip >= tipGap)
            {
                int rand = new Random().Next(1, 101);  //num between 1 and 100 - display tips from most common to rarest      

                if (rand <= 35)
                {
                    tip = "Use the hammer as much as possible to reserve much needed pistol ammo.";
                }
                else if (rand <= 60)
                {
                    tip = "Those weaponless bears looks suspicious, i wouldnt trust them!";
                }
                else if (rand <= 76)
                {
                    tip = "Try to take out multiple enemies with one bullet to save ammo.";
                }
                else if (rand <= 92)
                {
                    tip = "Be careful of the knifing bears, you can only hammer attack them from behind.";
                }
                else
                {
                    tip = "Run against curb shaped objects and jump at the right time to get extra height.";
                }

                timeSinceLastTip = 0;
            }
            else
            {
                timeSinceLastTip += gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            viewport = ScreenManager.GraphicsDevice.Viewport;
            viewportSize = new Vector2(viewport.Width, viewport.Height);
            popupPos = (viewportSize - new Vector2(GameStateManagementGame.controls.Width, GameStateManagementGame.controls.Height)) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)popupPos.X - hPad,
                                                          (int)popupPos.Y - vPad,
                                                          (int)GameStateManagementGame.controls.Width + hPad * 2,
                                                          (int)GameStateManagementGame.controls.Height + vPad * 2);

            // Fade the popup alpha during transitions.
            Color color = new Color(0.6f, 0.0f, 1.0f) * TransitionAlpha;
            Color color2 = new Color(0.25f, 0.25f, 0.25f) * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(GameStateManagementGame.controls, backgroundRectangle, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            // Draw the message box text.
            spriteBatch.DrawString(GameStateManagementGame.smallFont, prefix, new Vector2(((viewportSize.X - GameStateManagementGame.smallFont.MeasureString(tip).X) / 2) - ((GameStateManagementGame.smallFont.MeasureString(prefix).X / 2) + 5), 575), color);
            spriteBatch.DrawString(GameStateManagementGame.smallFont, tip, new Vector2(((viewportSize.X - GameStateManagementGame.smallFont.MeasureString(tip).X) / 2) + (GameStateManagementGame.smallFont.MeasureString(prefix).X / 2), 575), color2);     

            spriteBatch.End();
        }


        #endregion
    }
}
