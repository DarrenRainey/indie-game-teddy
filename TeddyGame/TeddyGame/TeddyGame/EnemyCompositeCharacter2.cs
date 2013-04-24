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
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Prototype2;

namespace Prototype2
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
        private bool threadCompleted = false;

        private Queue<Bullet> bulletQueue;
        public Array tempBulletArray;
        private Boolean bulletAdded;

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

            this.bulletQueue = new Queue<Bullet>();
            this.tempBulletArray = bulletQueue.ToArray();
            this.bulletAdded = false;

            activity = EnemyActivity2.enemyIdle;

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
                activity = EnemyActivity2.enemyIdle;                
            }
            return true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            handleBullets();
            
            handleAnimation(gameTime);

            //update 3d sound 
            audioEmitter.Position = new Vector3(Position.X, Position.Y, 1f) / GameplayScreen.soundDistanceFactor;       

            if (soundEffectInstance != null)
            {
                if (soundEffectInstance.State == SoundState.Playing)
                {
                    soundEffectInstance.Apply3D(GameplayScreen.audioListener, audioEmitter);
                }
            }

            oldActivity = activity;    
        }

        private void handleBullets()
        {
            //bullet stuff
            if (bulletQueue.Count > 0)
            {
                tempBulletArray = bulletQueue.ToArray();
                bulletQueue.Clear();                                  //convert to array, clear the queue and fill it up after changes

                for (int i = 0; i < tempBulletArray.Length; i++)
                {
                    ((Bullet)tempBulletArray.GetValue(i)).CurrentPos += ((Bullet)tempBulletArray.GetValue(i)).DirectionIncrement;

                    bulletQueue.Enqueue((Bullet)tempBulletArray.GetValue(i));
                }

                if (((Bullet)bulletQueue.Peek()).isDead())        //remove dead bullets from the queue
                {
                    bulletQueue.Dequeue();
                }

                tempBulletArray = bulletQueue.ToArray();
            }

            //this is the frame where the bullet is created
            if (animation.currentFrame == 18 && bulletAdded == false && (activity == EnemyActivity2.enemyShootDown || activity == EnemyActivity2.enemyShootDiagDown || activity == EnemyActivity2.enemyShootAhead || activity == EnemyActivity2.enemyShootDiagUp || activity == EnemyActivity2.enemyShootUp))  
            {
                Bullet bullet = new Bullet();
                bullet.Texture = GameplayScreen.bulletTex2;
                
                if(animation.myEffect == SpriteEffects.FlipHorizontally)  //to the right
                {
                    if (activity == EnemyActivity2.enemyShootDown)
                    {
                        bullet.DirectionIncrement = new Vector2(0f, 1f);
                        bullet.Origin = Position + new Vector2(21f, 70f);
                    }
                    else if (activity == EnemyActivity2.enemyShootDiagDown)
                    {
                        bullet.DirectionIncrement = new Vector2(0.75f, 0.75f);
                        bullet.Origin = Position + new Vector2(60f, 50f);
                    }
                    else if (activity == EnemyActivity2.enemyShootAhead)
                    {
                        bullet.DirectionIncrement = new Vector2(1f, 0f);
                        bullet.Origin = Position + new Vector2(90f, -5f);
                    }
                    else if (activity == EnemyActivity2.enemyShootDiagUp)
                    {
                        bullet.DirectionIncrement = new Vector2(0.75f, -0.75f);
                        bullet.Origin = Position + new Vector2(65f, -73f);
                    }
                    else if (activity == EnemyActivity2.enemyShootUp)
                    {
                        bullet.DirectionIncrement = new Vector2(0f, -1f);
                        bullet.Origin = Position + new Vector2(2f, -100f);
                    }
                }
                else   //to the left
                {
                    if (activity == EnemyActivity2.enemyShootDown)
                    {
                        bullet.DirectionIncrement = new Vector2(0f, 1f);
                        bullet.Origin = Position + new Vector2(-35f, 70f);
                    }
                    else if (activity == EnemyActivity2.enemyShootDiagDown)
                    {
                        bullet.DirectionIncrement = new Vector2(-0.75f, 0.75f);
                        bullet.Origin = Position + new Vector2(-87f, 58f);
                    }
                    else if (activity == EnemyActivity2.enemyShootAhead)
                    {
                        bullet.DirectionIncrement = new Vector2(-1f, 0f);
                        bullet.Origin = Position + new Vector2(-90f, -5f);
                    }
                    else if (activity == EnemyActivity2.enemyShootDiagUp)
                    {
                        bullet.DirectionIncrement = new Vector2(-0.75f, -0.75f);
                        bullet.Origin = Position + new Vector2(-77f, -75f);
                    }
                    else if (activity == EnemyActivity2.enemyShootUp)
                    {
                        bullet.DirectionIncrement = new Vector2(0f, -1f);
                        bullet.Origin = Position + new Vector2(-16f, -100f);
                    }
                }

                bullet.MaxDistanceFromOrigin = 1500;
                bullet.Speed = 6;
                bullet.CurrentPos = bullet.Origin;
                bullet.DirectionIncrement *= bullet.Speed;

                bulletQueue.Enqueue(bullet);    //add the bullet to the queue of bullets

                bulletAdded = true;
                
                soundEffectInstance = GameplayScreen.bearShoot.CreateInstance();        //play bullet sound quitely if far away
                soundEffectInstance.Apply3D(GameplayScreen.audioListener, audioEmitter);
                soundEffectInstance.Play();                
            }

            if (animation.currentFrame > 18 && (activity == EnemyActivity2.enemyShootDown || activity == EnemyActivity2.enemyShootDiagDown || activity == EnemyActivity2.enemyShootAhead || activity == EnemyActivity2.enemyShootDiagUp || activity == EnemyActivity2.enemyShootUp))
            {
                bulletAdded = false;
            }            
        }

        private void handleAnimation(GameTime gameTime)
        {            
            //control bear animation
            if (activity == EnemyActivity2.enemyIdle && oldActivity != EnemyActivity2.enemyIdle)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2Idle, Vector2.Zero, 143, 184, 28, 120, Color.White, 1f, true, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyJumping && oldActivity != EnemyActivity2.enemyJumping)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2Jumping, Vector2.Zero, 153, 183, 23, 40, Color.White, 1f, false, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyRunning && oldActivity != EnemyActivity2.enemyRunning)
            {                
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2Running, Vector2.Zero, 156, 184, 23, 60, Color.White, 1f, true, new Vector2(0, 0));

                drawOffset = new Vector2(0f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootDown && oldActivity != EnemyActivity2.enemyShootDown)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2ShootDown, Vector2.Zero, 138, 189, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, -3f);
            }
            else if (activity == EnemyActivity2.enemyShootDiagDown && oldActivity != EnemyActivity2.enemyShootDiagDown)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2ShootDiagDown, Vector2.Zero, 143, 187, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, -2f);
            }
            else if (activity == EnemyActivity2.enemyShootAhead && oldActivity != EnemyActivity2.enemyShootAhead)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2ShootAhead, Vector2.Zero, 162, 184, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-18f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootDiagUp && oldActivity != EnemyActivity2.enemyShootDiagUp)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2ShootDiagUp, Vector2.Zero, 139, 185, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(-7f, 0f);
            }
            else if (activity == EnemyActivity2.enemyShootUp && oldActivity != EnemyActivity2.enemyShootUp)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2ShootUp, Vector2.Zero, 146, 196, 25, 50, Color.White, 1f, false, new Vector2(0, 0));
                animation.currentFrame = shootStartFrame;

                drawOffset = new Vector2(5f, -6f);
            }
            else if (activity == EnemyActivity2.enemyDead && oldActivity != EnemyActivity2.enemyDead)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bear2Dead, Vector2.Zero, 184, 203, 22, 70, Color.White, 1f, false, new Vector2(0, 0));

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
                soundEffectInstance = GameplayScreen.bearDeadSound.CreateInstance();
            }
            else if (rand <= 75)
            {
                soundEffectInstance = GameplayScreen.bearDeadSound2.CreateInstance();
            }
            else if (rand <= 89)
            {
                soundEffectInstance = GameplayScreen.bearDeadSound3.CreateInstance();
            }
            else if (rand <= 97)
            {
                soundEffectInstance = GameplayScreen.bearDeadSound4.CreateInstance();
            }
            else
            {
                soundEffectInstance = GameplayScreen.bearDeadSound5.CreateInstance();
            }

            soundEffectInstance.Apply3D(GameplayScreen.audioListener, audioEmitter);
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

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

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

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

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

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                i += 10;
            }
        }

        public void jump()
        {
            activity = EnemyActivity2.enemyJumping;
            motor.MotorSpeed = 0;
            jumpForce.Y = jumpImpulse;
            body.ApplyLinearImpulse(jumpForce, body.Position);

            while (activity != EnemyActivity2.enemyIdle && bw.CancellationPending == false)
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

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

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

        public void shootLeft(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.None;
            activity = EnemyActivity2.enemyShootAhead;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootTopLeft(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.None;
            activity = EnemyActivity2.enemyShootDiagUp;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootUp(int bullets)
        {
            motor.MotorSpeed = 0;
                        
            activity = EnemyActivity2.enemyShootUp;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootTopRight(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.FlipHorizontally;
            activity = EnemyActivity2.enemyShootDiagUp;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootRight(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.FlipHorizontally;
            activity = EnemyActivity2.enemyShootAhead;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootBottomRight(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.FlipHorizontally;
            activity = EnemyActivity2.enemyShootDiagDown;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootDown(int bullets)
        {
            motor.MotorSpeed = 0;
                        
            activity = EnemyActivity2.enemyShootDown;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

                    animation.currentFrame = 15;          //this is the frame where the bear starts shooting, so loop the shooting until out of bullets

                    shootStartFrame = 15;            //covers the case if the animation type changes mid shooting       
                }
            }

            shootStartFrame = 0;
            activity = EnemyActivity2.enemyIdle;
        }

        public void shootBottomLeft(int bullets)
        {
            motor.MotorSpeed = 0;

            animation.myEffect = SpriteEffects.None;
            activity = EnemyActivity2.enemyShootDiagDown;

            while (bullets != 0 && animation.currentFrame != animation.frameCount - 1 && bw.CancellationPending == false)
            {
                Thread.Sleep(10);

                while (!MainMenuScreen.gamePlayScreen.IsActive && bw.CancellationPending == false)
                {
                    Thread.Sleep(5);
                }

                if (animation.currentFrame == animation.frameCount - 1)
                {
                    bullets--;

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
            double enemyToPlayerAngle = Math.Atan2(GameplayScreen.box.Position.Y - Position.Y, GameplayScreen.box.Position.X - Position.X) * 180 / Math.PI;

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

                    faceLeft();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(3000);                    

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootDown(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootBottomLeft(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootLeft(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootTopLeft(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootUp(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);
                    faceRight();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootUp(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootTopRight(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootRight(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootBottomRight(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootDown(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootBottomLeft(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

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

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate(object o, RunWorkerCompletedEventArgs args)      //event for when finished
                    {                      
                        threadCompleted = true;
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

                    shootAtPlayer(3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    shootAtPlayer(3);

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

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate(object o, RunWorkerCompletedEventArgs args)      //event for when finished
                    {                       
                        threadCompleted = true;
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

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate(object o, RunWorkerCompletedEventArgs args)      //event for when finished
                    {                     
                        threadCompleted = true;
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

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate(object o, RunWorkerCompletedEventArgs args)      //event for when finished
                    {                      
                        threadCompleted = true;
                    });

            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }
        
        public void stopScript()
        {
            if (bw != null)
            {
                if (bw.IsBusy)
                {                    
                    bw.CancelAsync();
                                       
                    while (threadCompleted == false)
                    {
                        Thread.Sleep(1);
                    }                    

                    threadCompleted = false;

                    activity = EnemyActivity2.enemyIdle;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {                        
            //These first two draw calls draw the upper and lower body independently
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(body.Position.X), (int)ConvertUnits.ToDisplayUnits(body.Position.Y), (int)width, (int)(height - (width / 2))), null, Color.White, body.Rotation, origin, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(wheel.Position.X), (int)ConvertUnits.ToDisplayUnits(wheel.Position.Y), (int)width, (int)width), null, Color.White, wheel.Rotation, origin, SpriteEffects.None, 0f);
                        
            spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, (int)width, (int)height), null, new Color(1, 1, 1, 0.5f), body.Rotation, origin, SpriteEffects.None, 0f);           
        }

        public void drawAnimation(GameTime gameTime, SpriteBatch spriteBatch, bool active)
        {
            if (active)
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
            }            
            
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

            