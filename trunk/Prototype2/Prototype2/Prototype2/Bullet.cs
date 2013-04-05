using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Prototype2
{
    class Bullet
    {
        private Texture2D texture;
        private Vector2 origin;       
        private Vector2 currentPos;
        private Vector2 directionIncrement;       //the XY increment for the bullet
        private int speed;
        private int maxDistanceFromOrigin;         //the lifespan of the bullet in pixels

        public Bullet()
        {
            this.texture = null;
            this.origin = Vector2.Zero;
            this.currentPos = Vector2.Zero;
            this.directionIncrement = Vector2.Zero;
            this.speed = 15;
            this.maxDistanceFromOrigin = 2560;    //bullet disappears once outside the screen (1280 pixels wide)
        }

        public void Initialise(Texture2D texture, Vector2 origin, Vector2 currentPos, Vector2 directionIncrement, int speed, int maxDistanceFromOrigin)
        {
            this.texture = texture;
            this.origin = origin;
            this.currentPos = currentPos;
            this.directionIncrement = directionIncrement;
            this.speed = speed;
            this.maxDistanceFromOrigin = maxDistanceFromOrigin;
        }
        
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }              

        public Vector2 CurrentPos
        {
            get { return currentPos; }
            set { currentPos = value; }
        }        

        public Vector2 DirectionIncrement
        {
            get { return directionIncrement; }
            set { directionIncrement = value; }
        }

        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public int MaxDistanceFromOrigin
        {
            get { return maxDistanceFromOrigin; }
            set { maxDistanceFromOrigin = value; }
        }

        //operations...........

        public Boolean isDead()
        {
            if (Math.Abs(origin.X - currentPos.X) > maxDistanceFromOrigin || Math.Abs(origin.Y - currentPos.Y) > maxDistanceFromOrigin)
            {
                return true;
            }

            return false;
        }
    }
}
