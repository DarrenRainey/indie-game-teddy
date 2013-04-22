using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Prototype2
{
    class PassableEdge
    {
        private Body body;
        private Vector2 startPoint;
        private Vector2 endPoint;
        private bool playerAbove = false;

        public PassableEdge(World _world, Vector2 start, Vector2 end)
        {                        
            this.body = BodyFactory.CreateEdge(_world, start, end);
            this.body.IsStatic = true;
            this.body.Restitution = 0.1f;
            this.body.Friction = 0.7f;

            this.startPoint = start;
            this.endPoint = end;
        }

        //operations...........

        public void Update(CompositeCharacter player)
        {        
            if ((player.Position.Y + 50) < (startPoint.Y * 100) && playerAbove == false)         //if player is above the passable platform
            {
                body.CollisionCategories = Category.Cat1;                
                playerAbove = true;                       
            }
            else if ((player.Position.Y + 40) > (startPoint.Y * 100) && playerAbove == true)
            {
                body.CollisionCategories = Category.None;              
                playerAbove = false;
            }
        }

        public bool isPlayerOn(CompositeCharacter player)
        {
            //if player pos is similar to the pos of the edge
            if ((player.Position.Y + 50) < ((startPoint.Y * 100) - 10) && (player.Position.Y + 50) < ((startPoint.Y * 100) + 10) && (player.Position.X + 60) > (startPoint.X * 100) && (player.Position.X - 60) < (endPoint.X * 100))        
            {
                return true;
            }

            return false;
        }
    }
}
