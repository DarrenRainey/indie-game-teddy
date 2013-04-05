using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.ComponentModel;

namespace Prototype1
{
    public enum EnemyActivity
    {
        enemyJumping,
        enemyRunning,
        enemyIdle,
        enemyNone
    }

    public class EnemyCompositeCharacter : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        private float centerOffset;

        private Animation animation;

        private BackgroundWorker bw;

        public EnemyActivity activity;
        protected EnemyActivity oldActivity;

        private Vector2 jumpForce;
        private float jumpDelayTime;

        private const float nextJumpDelayTime = 0f;
        private const float runSpeed = 5;
        private const float jumpImpulse = -1.7f;
        private const float maxAirVelocity = 6f;
        private const float inAirImpluse = 0.06f;

        public EnemyCompositeCharacter(World world, Vector2 position, float width, float height, float mass, Texture2D texture)
            : base(world, position, width, height, mass, texture)
        {
            if (width > height)
            {
                throw new Exception("Error width > height: can't make character because wheel would stick out of body");
            }

            this.animation = new Animation();

            activity = EnemyActivity.enemyNone;

            wheel.OnCollision += new OnCollisionEventHandler(OnCollision);
        }

        protected override void SetUpPhysics(World world, Vector2 position, float width, float height, float mass)
        {
            //Create a fixture with a body almost the size of the entire object
            //but with the bottom part cut off.
            float upperBodyHeight = height - (width / 2);

            //fixture = FixtureFactory.AttachRectangle((float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), mass / 2, Vector2.Zero, body); //CreateRectangle(world, (float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), mass / 2);
            body = BodyFactory.CreateRectangle(world, (float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), mass / 2); //fixture.Body;
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.0f;
            body.Friction = 0.5f;
            //also shift it up a tiny bit to keey the new object's center correct
            body.Position = ConvertUnits.ToSimUnits(position - (Vector2.UnitY * (width / 4)));
            centerOffset = position.Y - (float)ConvertUnits.ToDisplayUnits(body.Position.Y); //remember the offset from the center for drawing
            //fixture = FixtureFactory.AttachRectangle((float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), mass / 2, Vector2.Zero, body);

            //Now let's make sure our upperbody is always facing up.
            fixedAngleJoint = JointFactory.CreateFixedAngleJoint(world, body);

            //Create a wheel as wide as the whole object
            //wheel = FixtureFactory.AttachCircle((float)ConvertUnits.ToSimUnits(width / 2), mass / 2, body); //CreateCircle(world, (float)ConvertUnits.ToSimUnits(width / 2), mass / 2);
            wheel = BodyFactory.CreateCircle(world, (float)ConvertUnits.ToSimUnits(width / 2), mass / 2);

            //And position its center at the bottom of the upper body
            wheel.Position = body.Position + ConvertUnits.ToSimUnits(Vector2.UnitY * (upperBodyHeight / 2));
            wheel.BodyType = BodyType.Dynamic;
            wheel.Restitution = 0.0f;
            wheel.Friction = 0.5f;

            //These two bodies together are width wide and height high :)
            //So lets connect them together
            motor = JointFactory.CreateRevoluteJoint(world, body, wheel, Vector2.Zero);
            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 4f; //set this higher for some more juice
            motor.MotorSpeed = 0;

            //Make sure the two fixtures don't collide with each other
            wheel.IgnoreCollisionWith(body);
            body.IgnoreCollisionWith(wheel);

            //Set the friction of the wheel to float.MaxValue for fast stopping/starting
            //or set it higher to make the character slip.
            //wheel.Friction = float.MaxValue;
            wheel.Friction = 9f;
        }

        //Fired when we collide with another object. Use this to stop jumping
        //and resume normal movement
        public bool OnCollision(Fixture fix1, Fixture fix2, Contact contact)
        {                        
            //Check if we are both jumping this frame and last frame
            //so that we ignore the initial collision from jumping away from 
            //the ground
            if (activity == EnemyActivity.enemyJumping && oldActivity == EnemyActivity.enemyJumping)
            {
                activity = EnemyActivity.enemyNone;                
            }
            return true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            handleAnimation(gameTime);

            oldActivity = activity;    
        }

        private void handleAnimation(GameTime gameTime)
        {
            //control bear animation
            if (activity == EnemyActivity.enemyIdle && oldActivity != EnemyActivity.enemyIdle)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bearIdle, Vector2.Zero, 93, 183, 40, 100, Color.White, 1f, true, new Vector2(0, 0));
            }
            else if (activity == EnemyActivity.enemyJumping && oldActivity != EnemyActivity.enemyJumping)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bearJumping, Vector2.Zero, 138, 184, 23, 40, Color.White, 1f, false, new Vector2(0, 0));
            }
            else if (activity == EnemyActivity.enemyRunning && oldActivity != EnemyActivity.enemyRunning)
            {                
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bearRunning, Vector2.Zero, 116, 184, 29, 60, Color.White, 1f, true, new Vector2(0, 0));
            }

            animation.Update(gameTime);
        }

        public void moveRight(int millis, int speed)
        {                                            
            motor.MotorSpeed = speed;
            activity = EnemyActivity.enemyRunning;
            animation.myEffect = SpriteEffects.FlipHorizontally;
            Thread.Sleep(20);
            if (speed < 5)
            {
                animation.frameTime = 60;
            }
            else
            {
                animation.frameTime = 30;
            }

            Thread.Sleep(millis);

            motor.MotorSpeed = 0;
            activity = EnemyActivity.enemyIdle;  
        }

        public void moveLeft(int millis, int speed)
        {            
            motor.MotorSpeed = -speed;
            activity = EnemyActivity.enemyRunning;
            animation.myEffect = SpriteEffects.None;
            Thread.Sleep(20);
            if (speed < 5)
            {
                animation.frameTime = 60;
            }
            else
            {
                animation.frameTime = 30;
            }

            Thread.Sleep(millis);

            motor.MotorSpeed = 0;
            activity = EnemyActivity.enemyIdle; 
        }

        public void idle(int millis)
        {            
            activity = EnemyActivity.enemyIdle;
            Thread.Sleep(millis); 
        }

        public void jump()
        {
            activity = EnemyActivity.enemyJumping;
            motor.MotorSpeed = 0;
            jumpForce.Y = jumpImpulse;
            body.ApplyLinearImpulse(jumpForce, body.Position);

            while (activity != EnemyActivity.enemyNone)
            {
                Thread.Sleep(30); 
            }
            
            activity = EnemyActivity.enemyIdle;
        }

        public void faceLeft()
        {
            animation.myEffect = SpriteEffects.None; 
        }

        public void faceRight()
        {
            animation.myEffect = SpriteEffects.FlipHorizontally;
        }

        public void runScript1()  //pace back and forth
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while(true)
                {
                    moveRight(5000, 3);
                    idle(2000);
                    faceLeft();
                    idle(1000);
                    moveLeft(3000, 3);
                    idle(2000);
                    faceRight();
                    idle(3000);
                    moveLeft(2000, 3);
                    idle(3000);                                       
                }
            });

            bw.RunWorkerAsync();
        }

        public void runScript2()    //jump forward and walk back
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while(true)
                {
                    faceRight();
                    idle(4000);
                    moveRight(1000, 7);
                    jump();
                    moveRight(500, 7);
                    idle(500);
                    moveLeft(7000, 2);
                    idle(2000);                                                          
                } 
            });

            bw.RunWorkerAsync();
        }

        public void runScript3()  //go mental
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while (true)
                {
                    moveRight(500, 10);
                    moveLeft(500, 10);
                }
            });

            bw.RunWorkerAsync();
        }

        public void stopScript()
        {
            if (bw != null)
            {
                bw.CancelAsync();
                activity = EnemyActivity.enemyIdle;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //These first two draw calls draw the upper and lower body independently
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(body.Position.X), (int)ConvertUnits.ToDisplayUnits(body.Position.Y), (int)width, (int)(height - (width / 2))), null, Color.White, body.Rotation, origin, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(wheel.Position.X), (int)ConvertUnits.ToDisplayUnits(wheel.Position.Y), (int)width, (int)width), null, Color.White, wheel.Rotation, origin, SpriteEffects.None, 0f);

            //This last draw call shows how to draw these two bodies with one texture (drawn semi-transparent here so you can see the inner workings)            
            spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)(Position.Y), (int)width, (int)height), null, new Color(1, 1, 1, 0.5f), body.Rotation, origin, SpriteEffects.None, 0f);
        }

        public void drawAnimation(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animation.Position = Position;
            animation.Update(gameTime);
            animation.Draw(spriteBatch);
        }

        public override Vector2 Position
        {
            get
            {
                return (ConvertUnits.ToDisplayUnits(body.Position) + Vector2.UnitY * centerOffset);
            }
        }
    }
}

            