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
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Prototype2
{   
    class LevelCompleteScreen : GameScreen
    {
        #region Fields

        Texture2D gradientTexture;
        Viewport viewport;
        Vector2 viewportSize;
        Vector2 popupPos;
        bool lastLevel;

        //------stats vars
        int ammo;
        int enemiesKilled;
        int totalEnemies;
        Color ammoHudColor;
        Color enemyHudColor;
        int gameClock;

        Vector2 ammoHudPos;
        Vector2 clockPos;
        Vector2 enemyHudPos;

        float ammoHudFinalYPos = 365f;
        float clockFinalYPos = 270f;
        float enemyHudFinalYPos = 365f;

        String buttonPrompt = "Press any button to continue...";
        Vector2 promptPos = new Vector2(0, 485f);
        bool showPrompt = false;
        int timeSinceLastFlash = 0;  //in millis
        int flashTimeGap = 500;  //in millis

        int preSwoopDelay = 700;  //small delay before swooping animation starts
        int preTotDelay = 2200;    //delay before totting animation starts
        int preSkipDelay = 2700;    //delay before able to skip
        
        int totGap = 400;    //time between each tot
        int timeSinceLastTot;

        int ammoBonus = 1;
        int bearBonus = 2;
        Vector2 ammoBonusStartPos;
        Vector2 bearBonusStartPos;
        Vector2 bonusPos1;
        Vector2 bonusPos2;
        int bonusValue1;
        int bonusValue2;
        bool totSwitch = true;

        Color bonusColor = new Color(0f, 1f, 0f);
        float bonusAlpha1 = 0f;
        float bonusAlpha2 = 0f;

        Vector2 titlePos = new Vector2(0, 125f);
        Vector2 subTitlePos = new Vector2(0, 240f);
        String subTitle = "Time:";

        #endregion

        #region Events

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #endregion

        #region Initialization


        public LevelCompleteScreen(bool lastLevel, int ammo, int totalEnemies, Color ammoHudColor, Color enemyHudColor, int gameClock, Vector2 ammoHudPos, Vector2 clockPos, Vector2 enemyHudPos)
        {            
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.4);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            this.lastLevel = lastLevel;
            this.ammo = ammo;            
            this.totalEnemies = totalEnemies;
            this.ammoHudColor = ammoHudColor;
            this.enemyHudColor = enemyHudColor;
            this.gameClock = gameClock;
            this.ammoHudPos = ammoHudPos;
            this.clockPos = clockPos;
            this.enemyHudPos = enemyHudPos;

            this.enemiesKilled = GameStateManagementGame.enemiesKilled;

            timeSinceLastTot = totGap;           
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

            GameStateManagementGame.finLevelSound.Play();
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
            if ((input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.IsMenuSelect(ControllingPlayer, out playerIndex)) && preSkipDelay <= 0)
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, new PlayerIndexEventArgs(playerIndex));

                GameStateManagementGame.menuselect.Play();

                MainMenuScreen.gamePlayScreen.killAllEnemyThreads();                

                ExitScreen();

                if (!lastLevel)
                {
                    GameStateManagementGame.level1Time = gameClock;
                    
                    MainMenuScreen.gamePlayScreen2 = new GameplayScreen2();
                    
                    LoadingScreen.Load(ScreenManager, true, 0, MainMenuScreen.gamePlayScreen2); 
                }
                else
                {
                    GameStateManagementGame.level2Time = gameClock;

                    LoadingScreen.Load(ScreenManager, false, null, new GameCompleteScreen());
                }
            }
        }


        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
             
            //stats swoop animation
            if (preSwoopDelay <= 0)
            {
                if (clockPos.Y < clockFinalYPos)
                {
                    clockPos.Y += 5;
                }

                if (ammoHudPos.Y < ammoHudFinalYPos)
                {
                    ammoHudPos.X += 7;
                    ammoHudPos.Y += 7;
                }

                if (enemyHudPos.Y < enemyHudFinalYPos)
                {
                    enemyHudPos.X -= 7;
                    enemyHudPos.Y += 7;
                }
            }
            else
            {
                preSwoopDelay -= gameTime.ElapsedGameTime.Milliseconds;
            }

            //stats tot animation
            if (preTotDelay <= 0)
            {
                bearBonusStartPos = enemyHudPos + new Vector2(0, -30);
                ammoBonusStartPos = ammoHudPos + new Vector2(-15, -30);
                
                if (timeSinceLastTot >= totGap)
                {
                    if (enemiesKilled > 0)
                    {
                        enemiesKilled--;
                        
                        gameClock -= bearBonus;

                        if (totSwitch)
                        {
                            bonusPos1 = bearBonusStartPos;
                            bonusAlpha1 = 1f;
                            bonusValue1 = bearBonus;

                            totSwitch = false;
                        }
                        else
                        {
                            bonusPos2 = bearBonusStartPos;
                            bonusAlpha2 = 1f;
                            bonusValue2 = bearBonus;

                            totSwitch = true;
                        }

                        GameStateManagementGame.totSound.Play();                                              
                    }
                    else if (ammo > 0)
                    {
                        ammo--;

                        gameClock -= ammoBonus;

                        if (totSwitch)
                        {
                            bonusPos1 = ammoBonusStartPos;
                            bonusAlpha1 = 1f;
                            bonusValue1 = ammoBonus;

                            totSwitch = false;
                        }
                        else
                        {
                            bonusPos2 = ammoBonusStartPos;
                            bonusAlpha2 = 1f;
                            bonusValue2 = ammoBonus;

                            totSwitch = true;
                        }
                                                
                        GameStateManagementGame.totSound.Play();
                    }

                    if (gameClock < 0)
                    {
                        gameClock = 0;
                    }

                    timeSinceLastTot = 0;                    
                }
                else
                {
                    timeSinceLastTot += gameTime.ElapsedGameTime.Milliseconds;

                    if (bonusPos1.Y < ammoBonusStartPos.Y + 400)   //to make sure its stops animating the invisible string
                    {
                        bonusPos1.Y -= 2;
                    }

                    if (bonusPos2.Y < ammoBonusStartPos.Y + 400)   //to make sure its stops animating the invisible string
                    {
                        bonusPos2.Y -= 2;
                    }

                    if (bonusAlpha1 != 0f)
                    {
                        bonusAlpha1 -= 0.02f;
                    }

                    if (bonusAlpha2 != 0f)
                    {
                        bonusAlpha2 -= 0.02f;
                    }
                }
            }
            else
            {
                preTotDelay -= gameTime.ElapsedGameTime.Milliseconds;
            }
                       
            if (preSkipDelay <= 0)
            {
                //flashing prompt
                if (timeSinceLastFlash >= flashTimeGap)
                {
                    if (showPrompt)
                    {
                        showPrompt = false;
                    }
                    else
                    {
                        showPrompt = true;
                    }

                    timeSinceLastFlash = 0;
                }
                else
                {
                    timeSinceLastFlash += gameTime.ElapsedGameTime.Milliseconds;
                }  
            }
            else
            {
                preSkipDelay -= gameTime.ElapsedGameTime.Milliseconds;
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
            //ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            viewport = ScreenManager.GraphicsDevice.Viewport;
            viewportSize = new Vector2(viewport.Width, viewport.Height);
            popupPos = (viewportSize - new Vector2(GameStateManagementGame.levelCompleteBox.Width, GameStateManagementGame.levelCompleteBox.Height)) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)popupPos.X - hPad,
                                                          (int)popupPos.Y - vPad,
                                                          (int)GameStateManagementGame.levelCompleteBox.Width + hPad * 2,
                                                          (int)GameStateManagementGame.levelCompleteBox.Height + vPad * 2);

            // Fade the popup alpha during transitions.
            /*Color color = new Color(0.6f, 0.0f, 1.0f) * TransitionAlpha;
            Color color2 = new Color(0.25f, 0.25f, 0.25f) * TransitionAlpha;*/

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(GameStateManagementGame.levelCompleteBox, backgroundRectangle, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha / 2f));

            //draw pulsating title            
            double time = gameTime.TotalGameTime.TotalSeconds;
            float pulsate = (float)Math.Sin(time * 3) + 1;
            float titleScale = 1 - (pulsate * 0.04f);

            titlePos.X = (viewportSize.X - (GameStateManagementGame.levelCompleteBanner.Width * titleScale)) / 2;
            spriteBatch.Draw(GameStateManagementGame.levelCompleteBanner, titlePos, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0);

            spriteBatch.Draw(GameStateManagementGame.ammoHUD, ammoHudPos, Color.White);
            spriteBatch.DrawString(GameStateManagementGame.font, "" + ammo, ammoHudPos + new Vector2(62 + 1, 6 + 1), Color.Black);
            spriteBatch.DrawString(GameStateManagementGame.font, "" + ammo, ammoHudPos + new Vector2(62, 6), ammoHudColor);

            subTitlePos.X = (viewportSize.X - GameStateManagementGame.font.MeasureString(subTitle.ToString()).X) / 2;
            spriteBatch.DrawString(GameStateManagementGame.font, subTitle, subTitlePos, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            clockPos.X = (viewportSize.X - GameStateManagementGame.bigFont.MeasureString(gameClock.ToString()).X) / 2;
            spriteBatch.DrawString(GameStateManagementGame.bigFont, gameClock.ToString(), clockPos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(GameStateManagementGame.bigFont, gameClock.ToString(), clockPos, Color.White);

            spriteBatch.Draw(GameStateManagementGame.bearHUD, enemyHudPos, Color.White);
            spriteBatch.DrawString(GameStateManagementGame.font, "" + enemiesKilled + "/" + totalEnemies, enemyHudPos + new Vector2(63 + 1, 10 + 1), Color.Black);
            spriteBatch.DrawString(GameStateManagementGame.font, "" + enemiesKilled + "/" + totalEnemies, enemyHudPos + new Vector2(63, 10), enemyHudColor);
            
            if(showPrompt)
            {
                promptPos.X = (viewportSize.X - GameStateManagementGame.smallFont.MeasureString(buttonPrompt.ToString()).X) / 2;

                spriteBatch.DrawString(GameStateManagementGame.smallFont, buttonPrompt,  promptPos, Color.White);
            }

            spriteBatch.End();

            SpriteBatch _batch = new SpriteBatch(GameStateManagementGame.graphics.GraphicsDevice);
            _batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //draw the tot animated string
            
            _batch.DrawString(GameStateManagementGame.font, "-" + bonusValue1 + " secs", bonusPos1, new Color(0f, 1f, 0f, bonusAlpha1));
           
            _batch.DrawString(GameStateManagementGame.font, "-" + bonusValue2 + " secs", bonusPos2, new Color(0f, 1f, 0f, bonusAlpha2));
            
            _batch.End();

        }


        #endregion
    }
}
