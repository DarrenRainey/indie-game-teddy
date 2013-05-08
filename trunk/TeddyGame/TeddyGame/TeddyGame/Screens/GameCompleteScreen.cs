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
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
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

        Vector2 titlePos = new Vector2(0f, 45f);

        Vector2 gradeBoxPos = new Vector2(-550f, 165f);
        Vector2 finalGradeBoxPos;
        Vector2 creditsBoxPos = new Vector2(-550f, 165f);
        Vector2 finalCreditsBoxPos;

        Vector2 level1TotOffset = new Vector2(73f, 70f);
        Vector2 level2TotOffset = new Vector2(73f, 90f);
        Vector2 totalTimeLabelOffset = new Vector2(300f, 47f);
        Vector2 totalTimeOffset = new Vector2(370f, 73f);
        Vector2 gradeLabelOffset = new Vector2(0f, 150f);     //this will be centered
        Vector2 gradeOffset = new Vector2(0f, 230f);         //this will be centered
        Vector2 commentOffset = new Vector2(0f, 380f);          //this will be centered

        Vector2 creditsLabelOffset = new Vector2(0f, 30f);       //this will be centered
        Vector2 creditStartOffset = new Vector2(0f, 365f);          //this will be centered
        Vector2 creditFinishOffset = new Vector2(0f, 20f);         //this will be centered
        Vector2 creditFadeOffset = new Vector2(0f, 100f);         //this will be centered
        float creditsCentreYOffset = 210;
        
        float gradeScale = 0.0f;
        bool startScale = false;

        int totalTime = 0;
        String comment = "Next Grade: less than 30 seconds";

        int preSwoopDelay = 1700;  //in millis

        int totGap = 30;    //time between each tot
        int timeSinceLastTot;

        float gradeSlideTransition = 1;     //1.0 = start of trans, 0.0 = end
        float creditsSlideTransition = 1;

        double time;
        float pulsate;
        float pulsate2;        
        float titleScale = 1.0f;

        float gradeRotation;
        Vector2 rotateOrigin;
        float gradeStartAngle = 0.075f;

        List<Texture2D> grades;
        int awardedGradeIndex = 0;

        float commentAlpha = 0.0f;

        String buttonPrompt = "Press any button to continue...";
        Vector2 promptPos = new Vector2(0, 635f);
        bool showPrompt = false;
        int timeSinceLastFlash = 0;  //in millis
        int flashTimeGap = 500;  //in millis

        List<CreditsLine> creditsLines;
        float creditLineGap = 20;  //in pixals      
        int currentLine = 0;

        bool stopCredits = false;

        int skipStage = 0;

        bool wipeSoundPlayed = false;

        #endregion

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameCompleteScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(2.5);
            TransitionOffTime = TimeSpan.FromSeconds(1.8);

            grades = new List<Texture2D>();

            grades.Add(GameStateManagementGame.gradeA);
            grades.Add(GameStateManagementGame.gradeB);
            grades.Add(GameStateManagementGame.gradeC);
            grades.Add(GameStateManagementGame.gradeD);
            grades.Add(GameStateManagementGame.gradeF);

            creditsLines = new List<CreditsLine>();

            creditsLines.Add(new CreditsLine("Pretty Much Everything", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Hugh Desmond"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Testers", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Mateusz Przybylek"));
            creditsLines.Add(new CreditsLine("Hugh Desmond"));
            creditsLines.Add(new CreditsLine("Gavin Desmond"));
            creditsLines.Add(new CreditsLine("Robert Fehily"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));           
            creditsLines.Add(new CreditsLine("Project Manager", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Jim O'Dwyer"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));           
            creditsLines.Add(new CreditsLine("Sound Effects", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Ableton"));
            creditsLines.Add(new CreditsLine("bfxr"));            
            creditsLines.Add(new CreditsLine("audiomicro"));
            creditsLines.Add(new CreditsLine("freesound.org"));
            creditsLines.Add(new CreditsLine("freesfx.co.uk"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Music", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Ableton"));
            creditsLines.Add(new CreditsLine("FL Studio"));
            creditsLines.Add(new CreditsLine("Audacity"));
            creditsLines.Add(new CreditsLine("Woolyss Soundfonts"));
            creditsLines.Add(new CreditsLine("XACT3"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Track 1 - Retro Attack by Hugh Desmond"));            
            creditsLines.Add(new CreditsLine("Track 2 - Dont Cheese Me by Hugh/Gavin Desmond"));
            creditsLines.Add(new CreditsLine("Track 3 - Please Hold by Hugh/Gavin Desmond"));
            creditsLines.Add(new CreditsLine("Title Music - Cuddle Boy by Hugh Desmond"));
            creditsLines.Add(new CreditsLine("Credits Music - A.C.I.D. by Hugh Desmond"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Game Art & Animation", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));           
            creditsLines.Add(new CreditsLine("Adobe Photoshop"));
            creditsLines.Add(new CreditsLine("Adobe Illustrator"));
            creditsLines.Add(new CreditsLine("Adobe Flash"));
            creditsLines.Add(new CreditsLine("Thanks to Mark Fenny for ideas"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Third Party Software", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));            
            creditsLines.Add(new CreditsLine("Farseer Physics"));            
            creditsLines.Add(new CreditsLine("Tortoise SVN"));
            creditsLines.Add(new CreditsLine("Google Code"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));           
            creditsLines.Add(new CreditsLine("Special Thanks", 0.6f, 0.6f, 0.6f));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("My Xbox for not breaking.."));
            creditsLines.Add(new CreditsLine("RB Whitaker's XNA Tutorials"));           
            creditsLines.Add(new CreditsLine("Helpful folk @ gamedev.net"));
            creditsLines.Add(new CreditsLine("Helpful folk @ stackoverflow.com"));
            creditsLines.Add(new CreditsLine("PHL @ generic2d.blogspot.ie"));
            creditsLines.Add(new CreditsLine("Bryan Dismas @ amazingretardo.simiansoftwerks.com"));
            creditsLines.Add(new CreditsLine("All other XNA Bloggers"));
            creditsLines.Add(new CreditsLine("Benji the Dog")); 
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Wow you actually got this"));
            creditsLines.Add(new CreditsLine("far down the credits.."));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Good for you!"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("Heres your reward:"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("<( ''  <)"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("<(  ''  )>"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(" (>  '' )>"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("<(  ''  )>"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("<( ''  <)"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine("<(  ''  )>"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(" (>  '' )>"));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));
            creditsLines.Add(new CreditsLine(""));           
            creditsLines.Add(new CreditsLine("Thanks For Playing!"));
                    

            for (int i = 0; i < creditsLines.Count; i++)
            {
                creditsLines[i].position = creditStartOffset;
            }            
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
            
            int timeScore = GameStateManagementGame.level1Time + GameStateManagementGame.level2Time;

            if (timeScore == 10)    //a
            {
                awardedGradeIndex = 0;

                comment = "Congratulations, you officially have no life!";
            }
            else if (timeScore < 15)         //b
            {
                awardedGradeIndex = 1;

                comment = "Great, but can you do it in less then 10 seconds?!";
            }
            else if (timeScore < 35)          //c
            {
                awardedGradeIndex = 2;

                comment = "Next Grade: less than 15 seconds";
            }
            else if (timeScore < 60)        //d
            {
                awardedGradeIndex = 3;

                comment = "Next Grade: less than 35 seconds";
            }
            else                            //f
            {
                awardedGradeIndex = 4;

                comment = "You're rubbish! try again..";
            }
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
            if ((input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.IsMenuSelect(ControllingPlayer, out playerIndex)) && preSwoopDelay <= 0)
            {
                if (skipStage == 0)
                {
                    gradeSlideTransition = 0.0f;
                    gradeBoxPos = finalGradeBoxPos;
                    gradeScale = 1f;
                    commentAlpha = 1f;                    
                    totalTime += GameStateManagementGame.level1Time + GameStateManagementGame.level2Time;
                    GameStateManagementGame.level1Time = 0;
                    GameStateManagementGame.level2Time = 0;

                    GameStateManagementGame.menuselect.Play();

                    skipStage = 1;
                }
                else if (skipStage == 1)
                {
                    GameStateManagementGame.menuselect.Play();
                    
                    skipStage = 2;
                }
                else if (skipStage == 2)
                {
                    gradeSlideTransition = 1.0f;
                    gradeBoxPos = new Vector2(-550f, 165f);

                    creditsSlideTransition = 0.0f;
                    creditsBoxPos = finalCreditsBoxPos;
                    
                    GameStateManagementGame.menuselect.Play();

                    skipStage = 3;
                }
                else if (skipStage == 3)
                {
                    GameStateManagementGame.gameSongsCue.Stop(AudioStopOptions.AsAuthored);
                    
                    GameStateManagementGame.menuselect.Play();
                    
                    skipStage = 4;
                }                
            }
        }


        #region Update and Draw


        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
                                    
            
            //initial gradebox swoop
            if (preSwoopDelay <= 0 && skipStage == 0)
            {
                if (!wipeSoundPlayed)
                {
                    GameStateManagementGame.wipeSound.Play();

                    wipeSoundPlayed = true;
                }
                
                if (gradeSlideTransition <= 0)
                {                    
                    if (timeSinceLastTot >= totGap)
                    {
                        if (GameStateManagementGame.level1Time > 0)
                        {
                            GameStateManagementGame.level1Time--;
                            totalTime++;

                            GameStateManagementGame.gradeTotSound.Play();
                        }
                        else if (GameStateManagementGame.level2Time > 0)
                        {
                            GameStateManagementGame.level2Time--;
                            totalTime++;

                            GameStateManagementGame.gradeTotSound.Play();
                        }
                        else
                        {
                            if (gradeScale == 0.0f)
                            {
                                gradeScale = 10f;

                                startScale = true;
                            }                            
                        }

                        timeSinceLastTot = 0;
                    }
                    else
                    {
                        timeSinceLastTot += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else
                {                    
                    gradeSlideTransition -= 0.02f;

                    gradeBoxPos = finalGradeBoxPos;
                    gradeBoxPos.X -= ((float)Math.Pow(gradeSlideTransition, 2)) * 1000;
                }

                if (startScale)
                {
                    if (gradeScale > 0.0f && gradeScale <= 1.0f)
                    {
                        if (commentAlpha < 1.0f)
                        {
                            commentAlpha += 0.05f;
                        }
                        else
                        {
                            skipStage = 1;

                            wipeSoundPlayed = false;
                        }
                    }
                    else
                    {
                        gradeScale -= 0.25f;
                    }
                }
            }
            else
            {
                preSwoopDelay -= gameTime.ElapsedGameTime.Milliseconds;
            }

            if (skipStage == 2)
            {                
                if (gradeSlideTransition >= 1f)
                {
                    if (!wipeSoundPlayed)
                    {
                        GameStateManagementGame.wipeSound.Play();

                        wipeSoundPlayed = true;
                    }
                    
                    if (creditsSlideTransition <= 0)
                    {
                        wipeSoundPlayed = false;
                        
                        skipStage = 3;
                    }
                    else
                    {
                        creditsSlideTransition -= 0.02f;    //trans on

                        creditsBoxPos = finalCreditsBoxPos;
                        creditsBoxPos.X -= ((float)Math.Pow(creditsSlideTransition, 2)) * 1000;
                    }
                }
                else
                {
                    gradeSlideTransition += 0.02f;   //trans off

                    gradeBoxPos = finalGradeBoxPos;
                    gradeBoxPos.X += ((float)Math.Pow(gradeSlideTransition, 2)) * 1000;
                }
            }

            if (skipStage == 3)
            {
                //play credits music            
                if (GameStateManagementGame.music == 0)
                {
                    GameStateManagementGame.gameSongsCue = GameStateManagementGame.soundBank.GetCue("creditsMusic");
                    GameStateManagementGame.gameSongsCue.Play();

                    GameStateManagementGame.music = 3;
                }

                if (currentLine == 0)
                {
                    creditsLines[currentLine].active = true;
                    currentLine++;
                }
                else
                {
                    //rolling credits
                    if (creditsLines[currentLine].position.Y - creditsLines[currentLine - 1].position.Y >= creditLineGap)
                    {
                        if (currentLine < creditsLines.Count - 1)
                        {
                            creditsLines[currentLine].active = true;
                            currentLine++;
                        }
                        else
                        {
                            if(!stopCredits)
                            {
                                creditsLines[currentLine].active = true;

                                stopCredits = true;
                            }
                        }
                    }
                }

                for (int i = 0; i < creditsLines.Count; i++)
                {
                    if (creditsLines[i].active && creditsLines[i].Alpha < 1f && creditsLines[i].position.Y > creditFadeOffset.Y)
                    {
                        creditsLines[i].Alpha += 0.02f;
                    }

                    if (creditsLines[i].active)
                    {
                        creditsLines[i].position.Y -= 0.3f;
                    }

                    if (creditsLines[i].active && creditsLines[i].Alpha > 0f && creditsLines[i].position.Y <= creditFadeOffset.Y)
                    {
                        creditsLines[i].Alpha -= 0.02f;
                    }

                    if (creditsLines[i].position.Y <= creditFinishOffset.Y)
                    {
                        creditsLines[i].active = false;
                    }

                    if (i == creditsLines.Count - 1 && creditsLines[i].position.Y <= creditsCentreYOffset)
                    {                        
                        creditsLines[i].active = false;
                    }
                }
            }

            if (skipStage == 4)
            {                
                if (creditsSlideTransition >= 1)
                {                    
                    skipStage = 5;                    

                    ExitScreen();

                    GameStateManagementGame.gameSongsCue.Stop(new AudioStopOptions());

                    GameStateManagementGame.music = 1;

                    LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());                    
                }
                else
                {
                    creditsSlideTransition += 0.02f;    //trans off

                    creditsBoxPos = finalCreditsBoxPos;
                    creditsBoxPos.X += ((float)Math.Pow(creditsSlideTransition, 2)) * 1000;
                }                
            }

            if (skipStage < 4)
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
                showPrompt = false;
            }        

            //play grade woosh sound
            if (gradeScale == 7.5)
            {
                GameStateManagementGame.wooshSound.Play();
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

            finalGradeBoxPos = new Vector2((viewportSize.X - (GameStateManagementGame.gradeBox.Width)) / 2, gradeBoxPos.Y);
            finalCreditsBoxPos = finalGradeBoxPos;             
            
            //grade wiggling
            time = gameTime.TotalGameTime.TotalSeconds;                           
            pulsate = (float)Math.Sin(time * 5) + 1;  
            gradeRotation = gradeStartAngle - ((Single)pulsate * 0.09f);
            rotateOrigin = new Vector2(grades[awardedGradeIndex].Width / 2, grades[awardedGradeIndex].Height / 2);                      
            
            
            //draw pulsating title 
            pulsate2 = (float)Math.Sin(time * 3) + 1;
            titleScale = 1 - (pulsate2 * 0.04f);


            spriteBatch.Begin();
            
            spriteBatch.Draw(GameStateManagementGame.finScreen, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            titlePos.X = (viewportSize.X - (GameStateManagementGame.gameCompleteBanner.Width * titleScale)) / 2;
            spriteBatch.Draw(GameStateManagementGame.gameCompleteBanner, titlePos, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0);

            // Draw grade box.            
            spriteBatch.Draw(GameStateManagementGame.gradeBox, gradeBoxPos, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.DrawString(GameStateManagementGame.smallFont, "Level 1: " + GameStateManagementGame.level1Time + " secs", gradeBoxPos + level1TotOffset, Color.Black);
            spriteBatch.DrawString(GameStateManagementGame.smallFont, "Level 2: " + GameStateManagementGame.level2Time + " secs", gradeBoxPos + level2TotOffset, Color.Black);

            spriteBatch.DrawString(GameStateManagementGame.font, "Total Time", gradeBoxPos + totalTimeLabelOffset, new Color(0.6f, 0.0f, 1.0f));
            
            totalTimeOffset.X = totalTimeLabelOffset.X + ((GameStateManagementGame.font.MeasureString("Total Time".ToString()).X - GameStateManagementGame.bigFont.MeasureString(totalTime.ToString()).X) / 2);
            spriteBatch.DrawString(GameStateManagementGame.bigFont, totalTime.ToString(), gradeBoxPos + totalTimeOffset, Color.Black);

            gradeLabelOffset.X = (GameStateManagementGame.gradeBox.Width - GameStateManagementGame.bigFont.MeasureString("GRADE".ToString()).X) / 2;
            spriteBatch.DrawString(GameStateManagementGame.bigFont, "GRADE", gradeBoxPos + gradeLabelOffset, new Color(0.6f, 0.0f, 1.0f));

            //draw awarded grade with animations
            gradeOffset.X = (GameStateManagementGame.gradeBox.Width - (grades[awardedGradeIndex].Width) * gradeScale) / 2;
            float gradeY = gradeOffset.Y + ((grades[awardedGradeIndex].Height - (grades[awardedGradeIndex].Height * gradeScale)) / 2);

            //to undo centre origin's effects
            Vector2 gradeDrawOffset = new Vector2(gradeOffset.X + ((grades[awardedGradeIndex].Width * gradeScale) / 2), gradeY + ((grades[awardedGradeIndex].Height * gradeScale) / 2));
            spriteBatch.Draw(grades[awardedGradeIndex], gradeBoxPos + gradeDrawOffset, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), gradeRotation, rotateOrigin, gradeScale, SpriteEffects.None, 0f);

            commentOffset.X = (GameStateManagementGame.gradeBox.Width - GameStateManagementGame.smallFont.MeasureString(comment.ToString()).X) / 2;
            spriteBatch.DrawString(GameStateManagementGame.smallFont, comment, gradeBoxPos + commentOffset, new Color(0f, 0f, 0f, commentAlpha));

            if (showPrompt)
            {
                promptPos.X = (viewportSize.X - GameStateManagementGame.smallFont.MeasureString(buttonPrompt.ToString()).X) / 2;

                spriteBatch.DrawString(GameStateManagementGame.smallFont, buttonPrompt, promptPos, Color.White);
            }

            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //credits stuff-------------------------------------------                       
            spriteBatch.Draw(GameStateManagementGame.creditsBox, creditsBoxPos, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            creditsLabelOffset.X = (GameStateManagementGame.creditsBox.Width - GameStateManagementGame.font.MeasureString("CREDITS".ToString()).X) / 2;
            spriteBatch.DrawString(GameStateManagementGame.font, "CREDITS", creditsBoxPos + creditsLabelOffset, Color.White);

            for (int i = 0; i < creditsLines.Count; i++)
            {
                creditsLines[i].position.X = (GameStateManagementGame.creditsBox.Width - GameStateManagementGame.smallFont.MeasureString(creditsLines[i].Text.ToString()).X) / 2;
                spriteBatch.DrawString(GameStateManagementGame.smallFont, creditsLines[i].Text, creditsBoxPos + creditsLines[i].position, new Color(creditsLines[i].R, creditsLines[i].G, creditsLines[i].B, creditsLines[i].Alpha));
            }

            spriteBatch.End();
        }


        #endregion
    }
}
