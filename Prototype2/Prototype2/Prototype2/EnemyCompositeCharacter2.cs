﻿using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.ComponentModel;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Prototype2;

namespace Prototype1
{
    public enum EnemyActivity2
    {
        enemyJumping,
        enemyRunning,
        enemyIdle,
        enemyShootAhead,
        enemyShootUp,
        enemyShootDown,
        enemyShootDiagUp,
        enemyShootDiagDown,
        enemyDead,
        enemyNone
    }

    public class EnemyCompositeCharacter2 : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        private float centerOffset;

        private Vector2 drawOffset;
        private int shootStartFrame;

        public Animation animation;
        private BackgroundWorker bw;

        private Queue<Bullet> bulletQueue;
        private Array tempBulletArray;

        public EnemyActivity2 activity;
        protected EnemyActivity2 oldActivity;

        public AudioEmitter audioEmitter;
        private SoundEffectInstance soundEffectInstance;

        private Vector2 jumpForce;
        private float jumpDelayTime;

        private const float nextJumpDelayTime = 0f;
        private const float runSpeed = 5;
        private const float jumpImpulse = -1.7f;
        private const float maxAirVelocity = 6f;
        private const float inAirImpluse = 0.06f;

        public EnemyCompositeCharacter2(World world, Vector2 position, float width, float height, float mass, Texture2D texture)
            : base(world, position, width, height, mass, texture)
        {
            if (width > height)
            {
                throw new Exception("Error width > height: can't make character because wheel would stick out of body");
            }

            this.animation = new Animation();
            this.audioEmitter = new AudioEmitter();
            this.drawOffset = new Vector2(0f, 0f);
            this.shootStartFrame = 0;

            activity = EnemyActivity2.enemyNone;

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
            if (activity == EnemyActivity2.enemyJumping && oldActivity == EnemyActivity2.enemyJumping)
            {
                activity = EnemyActivity2.enemyNone;                
            }
            return true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            handleShoot();
            
            handleAnimation(gameTime);

            //update 3d sound 
            audioEmitter.Position = new Vector3(Position.X, Position.Y, 1f) / Game1.soundDistanceFactor;

            if (soundEffectInstance != null)
            {
                if (soundEffectInstance.State == SoundState.Playing)
                {
                    soundEffectInstance.Apply3D(Game1.audioListener, audioEmitter);
                }
            }

            oldActivity = activity;    
        }

        private void handleShoot()
        {
            //handle bear shoot attacks
            /*if (Math.Abs(Position.X - Game1.box.Position.X) < 50 && Math.Abs(Position.Y - Game1.box.Position.Y) < 50 && Game1.box.activity != Activity.Dead && activity != EnemyActivity2.enemyDead)
            {
                stopScript();

                activity = EnemyActivity2.enemyKnife;
            }*/
        }

        private void handleAnimation(GameTime gameTime)
        {            
            //control bear animation
            if (activity == EnemyActivity2.enemyIdle && oldActivity != EnemyActivity2.enemyIdle)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2Idle, Vector2.Zero, 143, 184, 28, 120, Color.White, 1f, true, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyJumping && oldActivity != EnemyActivity2.enemyJumping)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2Jumping, Vector2.Zero, 153, 183, 23, 40, Color.White, 1f, false, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyRunning && oldActivity != EnemyActivity2.enemyRunning)
            {                
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2Running, Vector2.Zero, 156, 184, 23, 60, Color.White, 1f, true, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootDown && oldActivity != EnemyActivity2.enemyShootDown)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2ShootDown, Vector2.Zero, 138, 189, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, -3f);
            }
            else if (activity == EnemyActivity2.enemyShootDiagDown && oldActivity != EnemyActivity2.enemyShootDiagDown)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2ShootDiagDown, Vector2.Zero, 143, 187, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, -2f);
            }
            else if (activity == EnemyActivity2.enemyShootAhead && oldActivity != EnemyActivity2.enemyShootAhead)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2ShootAhead, Vector2.Zero, 162, 184, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-18f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootDiagUp && oldActivity != EnemyActivity2.enemyShootDiagUp)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2ShootDiagUp, Vector2.Zero, 139, 185, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootUp && oldActivity != EnemyActivity2.enemyShootUp)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2ShootUp, Vector2.Zero, 146, 196, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(5f, -6f);
            }
            else if (activity == EnemyActivity2.enemyDead && oldActivity != EnemyActivity2.enemyDead)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(Game1.bear2Dead, Vector2.Zero, 184, 203, 22, 70, Color.White, 1f, false, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);

                playRandomDeadSound();                                                
            }             

            //if the knife animation is finished execute the post attack script
            /*if (activity == EnemyActivity2.enemyShoot && animation.currentFrame == 13)
            {                
                postAttackScript();
            }*/                        

            animation.Update(gameTime);
        }

        private void playRandomDeadSound()
        {
            int rand = new Random().Next(1, 101);  //num between 1 and 100 - play die sounds from most common to rarest

            if (rand <= 50)
            {
                soundEffectInstance = Game1.bearDeadSound.CreateInstance();
            }
            else if (rand <= 75)
            {
                soundEffectInstance = Game1.bearDeadSound2.CreateInstance();
            }
            else if (rand <= 89)
            {
                soundEffectInstance = Game1.bearDeadSound3.CreateInstance();
            }
            else if (rand <= 97)
            {
                soundEffectInstance = Game1.bearDeadSound4.CreateInstance();
            }
            else
            {
                soundEffectInstance = Game1.bearDeadSound5.CreateInstance();
            }

            soundEffectInstance.Apply3D(Game1.audioListener, audioEmitter);
            soundEffectInstance.Play();
        }

        public void moveRight(int millis, int speed)
        {                                            
            motor.MotorSpeed = speed;
            activity = EnemyActivity2.enemyRunning;
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

            int i = 0;

            while (bw.CancellationPending == false && i < millis)
            {
                Thread.Sleep(10);

                i += 10;
            }

            motor.MotorSpeed = 0;
            activity = EnemyActivity2.enemyIdle;  
        }

        public void moveLeft(int millis, int speed)
        {            
            motor.MotorSpeed = -speed;
            activity = EnemyActivity2.enemyRunning;
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

            int i = 0;

            while (bw.CancellationPending == false && i < millis)
            {
                Thread.Sleep(10);

                i += 10;
            }

            motor.MotorSpeed = 0;
            activity = EnemyActivity2.enemyIdle; 
        }

        public void idle(int millis)
        {            
            activity = EnemyActivity2.enemyIdle;

            int i = 0;

            while (bw.CancellationPending == false && i < millis)
            {
                Thread.Sleep(10);

                i += 10;
            }
        }

        public void jump()
        {
            activity = EnemyActivity2.enemyJumping;
            motor.MotorSpeed = 0;
            jumpForce.Y = jumpImpulse;
            body.ApplyLinearImpulse(jumpForce, body.Position);

            while (activity != EnemyActivity2.enemyNone && bw.CancellationPending == false)
            {
                Thread.Sleep(10); 
            }
            
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootAtPlayer(int bullets)
        {
            motor.MotorSpeed = 0;

            activity = getActivityToShootPlayer();

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    activity = getActivityToShootPlayer();

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void faceLeft()
        {
            animation.myEffect = SpriteEffects.None; 
        }

        public void faceRight()
        {
            animation.myEffect = SpriteEffects.FlipHorizontally;
        }

        public EnemyActivity2 getActivityToShootPlayer()
        {
            double enemyToPlayerAngle = Math.Atan2(Game1.box.Position.Y - Position.Y, Game1.box.Position.X - Position.X) * 180 / Math.PI;

            if (enemyToPlayerAngle < 22.5 && enemyToPlayerAngle >= -22.5)  //to the right
            {     
                animation.myEffect = SpriteEffects.FlipHorizontally;

                return EnemyActivity2.enemyShootAhead; 
            }
            else if (enemyToPlayerAngle < 67.5 && enemyToPlayerAngle >= 22.5)  //to the bottom right
            {
                animation.myEffect = SpriteEffects.FlipHorizontally;

                return EnemyActivity2.enemyShootDiagDown; 
            }
            else if (enemyToPlayerAngle < 112.5 && enemyToPlayerAngle >= 67.5)  //to the bottom
            {
                return EnemyActivity2.enemyShootDown; 
            }
            else if (enemyToPlayerAngle < 157.5 && enemyToPlayerAngle >= 112.5)  //to the bottom left
            {                
                animation.myEffect = SpriteEffects.None;

                return EnemyActivity2.enemyShootDiagDown; 
            }
            else if (enemyToPlayerAngle < -157.5 || enemyToPlayerAngle >= 157.5)  //to the left
            {
                animation.myEffect = SpriteEffects.None;

                return EnemyActivity2.enemyShootAhead; 
            }
            else if (enemyToPlayerAngle < -112.5 && enemyToPlayerAngle >= -157.5)   //to the top left
            {    
                animation.myEffect = SpriteEffects.None;

                return EnemyActivity2.enemyShootDiagUp; 
            }
            else if (enemyToPlayerAngle < -67.5 && enemyToPlayerAngle >= -112.5)  //to the top
            {
                return EnemyActivity2.enemyShootUp; 
            }
            else if (enemyToPlayerAngle < -22.5 && enemyToPlayerAngle >= -67.5)  //to the top right
            {               
                animation.myEffect = SpriteEffects.FlipHorizontally;

                return EnemyActivity2.enemyShootDiagUp; 
            }

            return EnemyActivity2.enemyShootAhead;     //default - should never happen       
        }            

        public void runScript1()  //pace back and forth tightly and shoot
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while(true)
                {
                    moveRight(2000, 3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(2000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceLeft();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(1000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceRight();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(3000);                    

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootAtPlayer(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(1000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                    
                    moveLeft(2000, 3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                    
                    idle(2000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                }
            });

            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        public void runScript2()    //skip left, run right, look left and right
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while(true)
                {   
                    moveLeft(1500, 6);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    jump();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveLeft(1000, 6);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    jump();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveLeft(1000, 6);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceRight();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(1500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveRight(3000, 10);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(2000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceLeft();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootAtPlayer(8);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceRight();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    faceLeft();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(2500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                } 
            });

            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        public void runScript3()  //go even more mental
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while (true)
                {
                    moveRight(1000, 20);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveLeft(1000, 20);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                }
            });

            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        public void postAttackScript()  //executed after the enemy has attacked
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //randomly pace from left to right
            {
                int rand;
                
                while (true)
                {
                    rand = (new Random()).Next(0, 2);                    

                    if (rand == 1)
                    {
                        moveRight(3000, 3);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        idle(2000);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        faceLeft();

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        idle(1000);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }
                    }
                    else
                    {
                        moveLeft(3000, 3);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        idle(2000);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        faceRight();

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }

                        idle(1000);

                        if (bw.CancellationPending == true)
                        {
                            break;
                        }
                    }                                
                }
            });

            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        public void stopScript()
        {
            if (bw != null)
            {                                    
                bw.CancelAsync();

                Thread.Sleep(12);         //wait to make sure the script is stopped

                activity = EnemyActivity2.enemyIdle;                
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {                        
            //These first two draw calls draw the upper and lower body independently
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(body.Position.X), (int)ConvertUnits.ToDisplayUnits(body.Position.Y), (int)width, (int)(height - (width / 2))), null, Color.White, body.Rotation, origin, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(wheel.Position.X), (int)ConvertUnits.ToDisplayUnits(wheel.Position.Y), (int)width, (int)width), null, Color.White, wheel.Rotation, origin, SpriteEffects.None, 0f);
                        
            spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, (int)width, (int)height), null, new Color(1, 1, 1, 0.5f), body.Rotation, origin, SpriteEffects.None, 0f);           
        }

        public void drawAnimation(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (animation.myEffect == SpriteEffects.FlipHorizontally)
            {
                animation.Position = Position + new Vector2(-drawOffset.X, drawOffset.Y);
            }
            else
            {
                animation.Position = Position + new Vector2(drawOffset.X, drawOffset.Y);
            }

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

            