using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Prototype2
{
    public class FrameRateCounter
    {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        public int frameRate = 0;
        public int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        
        public FrameRateCounter()  
        {            
        }

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }
    }
}