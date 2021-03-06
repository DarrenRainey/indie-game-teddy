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
#endregion

namespace Prototype2
{    
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;      
        private Vector2 manPos = new Vector2(429f, 762f);
        private Vector2 manFinalPos = new Vector2(429f, 167f);

        private Vector2 titlePos = new Vector2(312f, -421f);
        private Vector2 titleFinalPos = new Vector2(312f, 29f);

        private Vector2 subtitlePos = new Vector2(490f, 132f);
        private float subtitleAlpha = 0.0f;
        private float subtitleFinalAlpha = 1.0f;

        private int subtitleAlphaDelay = 1700;   //millis

        double time;
        float pulsate;
        float pulsate2;
        float pulsate3;
        float titleScale = 1.0f;

        float subTitleRotation; 
        Vector2 rotateOrigin;
        float subtitleStartAngle = 0.025f;
                        
        float manScale = 1.0f;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
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

            if (manPos.Y > manFinalPos.Y)
            {
                manPos.Y -= 7;
            }           

            if (titlePos.Y < titleFinalPos.Y)
            {
                titlePos.Y += 5;
            }

            if (subtitleAlphaDelay < 0)
            {
                if (subtitleAlpha != subtitleFinalAlpha)
                {
                    subtitleAlpha += 0.01f;
                }
            }
            else
            {
                subtitleAlphaDelay -= gameTime.ElapsedGameTime.Milliseconds;
            }
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


            //pulsating title            
            time = gameTime.TotalGameTime.TotalSeconds;
            pulsate = (float)Math.Sin(time * 3.2) + 1;
            titleScale = 1 - ((Single)pulsate * 0.04f);  

            //subtitle wiggling
            pulsate2 = (float)Math.Sin(time * 2.5) + 1;
            subTitleRotation = subtitleStartAngle - ((Single)pulsate2 * 0.03f);
            rotateOrigin = new Vector2(GameStateManagementGame.menusubtitle.Width / 2, GameStateManagementGame.menusubtitle.Height / 2);

            //man pulsating
            pulsate3 = (float)Math.Sin(time * 0.35) + 1;
            manScale = 1 - ((Single)pulsate3 * 0.035f); 

            spriteBatch.Begin();

            spriteBatch.Draw(GameStateManagementGame.menubg, fullscreen, 
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            manPos.X = (viewportSize.X - (GameStateManagementGame.menuman.Width * manScale)) / 2;
            float manY = manPos.Y + ((GameStateManagementGame.menuman.Height - (GameStateManagementGame.menuman.Height * manScale)) / 2);
            spriteBatch.Draw(GameStateManagementGame.menuman, new Vector2(manPos.X, manY), null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, manScale, SpriteEffects.None, 0);    //draw man                     

            titlePos.X = (viewportSize.X - (GameStateManagementGame.menutitle.Width * titleScale)) / 2;
            spriteBatch.Draw(GameStateManagementGame.menutitle, titlePos, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0);

            spriteBatch.Draw(GameStateManagementGame.menusubtitle, subtitlePos + new Vector2(GameStateManagementGame.menusubtitle.Width / 2, GameStateManagementGame.menusubtitle.Height / 2), null,
                             new Color(subtitleAlpha, subtitleAlpha, subtitleAlpha, subtitleAlpha), subTitleRotation, rotateOrigin, 1f, SpriteEffects.None, 0);    //draw subtitle

            spriteBatch.End();
        }


        #endregion
    }
}
