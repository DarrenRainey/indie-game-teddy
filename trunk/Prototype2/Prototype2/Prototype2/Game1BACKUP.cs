using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Prototype1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1BACKUP : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int screenWidth = 1280;
        const int screenHeight = 720;
        Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

        Texture2D background1;
        Texture2D player1;

        Vector2 playerPosition;

        GamePadState oldGamePadState;

        public Game1BACKUP()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = screenWidth;               //xbox safe resolution settings 
            graphics.PreferredBackBufferHeight = screenHeight;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {        
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background1 = Content.Load<Texture2D>("background1");
            player1 = Content.Load<Texture2D>("player1"); 



            playerPosition = screenCenter - new Vector2(player1.Width / 2, player1.Height / 2);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            GamePadState newGamePadState = GamePad.GetState(PlayerIndex.One);        

            playerPosition.X += (int)(newGamePadState.ThumbSticks.Left.X * 10);

            Console.WriteLine(newGamePadState.ThumbSticks.Left.X);

            


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(background1, new Vector2(0, 0), Color.White);

            spriteBatch.Draw(player1, playerPosition, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
