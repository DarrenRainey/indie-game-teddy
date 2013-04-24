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

namespace Prototype2
{
    public enum EnemyActivity
    {
        enemyJumping,
        enemyRunning,
        enemyIdle,
        enemyKnife,
        enemyDead,
        enemyNone
    }

    public class EnemyCompositeCharacter : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        private float centerOffset;

        public Animation animation;
        private BackgroundWorker bw;
        private bool threadCompleted = false;

        public EnemyActivity activity;
        protected EnemyActivity oldActivity;

        public AudioEmitter audioEmitter;
        private SoundEffectInstance soundEffectInstance;

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
            this.audioEmitter = new AudioEmitter();

            activity = EnemyActivity.enemyIdle;

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
            if (activity == EnemyActivity.enemyJumping && oldActivity == EnemyActivity.enemyJumping && activity != EnemyActivity.enemyKnife)
            {
                activity = EnemyActivity.enemyIdle;                
            }
            return true;
        }

        protected override void HandleInput(GameTime gameTime)
        {            
            //handle bear knife attacks
            if (Math.Abs(Position.X - GameplayScreen.box.Position.X) < 50 && Math.Abs(Position.Y - GameplayScreen.box.Position.Y) < 90 && GameplayScreen.box.activity != Activity.Dead && activity != EnemyActivity.enemyDead)
            {
                stopScript();                               
                
                activity = EnemyActivity.enemyKnife;                
            }
            
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

        private void handleAnimation(GameTime gameTime)
        {
            
            //control bear animation
            if (activity == EnemyActivity.enemyIdle && oldActivity != EnemyActivity.enemyIdle)
            {                
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bearIdle, Vector2.Zero, 93, 183, 40, 100, Color.White, 1f, true, new Vector2(0, 0));
            }
            else if (activity == EnemyActivity.enemyJumping && oldActivity != EnemyActivity.enemyJumping)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bearJumping, Vector2.Zero, 138, 184, 23, 40, Color.White, 1f, false, new Vector2(0, 0));
            }
            else if (activity == EnemyActivity.enemyRunning && oldActivity != EnemyActivity.enemyRunning)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bearRunning, Vector2.Zero, 116, 184, 29, 60, Color.White, 1f, true, new Vector2(0, 0));
            }
            else if (activity == EnemyActivity.enemyDead && oldActivity != EnemyActivity.enemyDead)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bearDead, Vector2.Zero, 183, 203, 21, 70, Color.White, 1f, false, new Vector2(0, 0));

                playRandomDeadSound();
            }
            else if (activity == EnemyActivity.enemyKnife && oldActivity != EnemyActivity.enemyKnife)
            {
                Vector2 temp = animation.Position;
                animation.Initialize(GameStateManagementGame.bearKnife, Vector2.Zero, 134, 183, 14, 50, Color.White, 1f, false, new Vector2(0, 0));

                GameplayScreen.knifeSound.Play();

                GameplayScreen.box.activity = Activity.Dead;
            }
  
            //if the knife animation is finished execute the post attack script
            if (activity == EnemyActivity.enemyKnife && animation.currentFrame == 13)
            {                
                postAttackScript();
            }

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

            soundEffectInstance = GameplayScreen.bearDeadSound.CreateInstance();  
            soundEffectInstance.Apply3D(GameplayScreen.audioListener, audioEmitter);
            soundEffectInstance.Play();            
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
            activity = EnemyActivity.enemyIdle; 
        }

        public void idle(int millis)
        {            
            activity = EnemyActivity.enemyIdle;

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
            activity = EnemyActivity.enemyJumping;
            motor.MotorSpeed = 0;
            jumpForce.Y = jumpImpulse;
            body.ApplyLinearImpulse(jumpForce, body.Position);

            while (activity != EnemyActivity.enemyIdle && bw.CancellationPending == false)
            {
                Thread.Sleep(10); 
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
                    
                    idle(3000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                    
                    moveLeft(2000, 3);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }
                    
                    idle(3000);

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

        public void runScript2()    //jump forward and walk back
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while(true)
                {
                    faceRight();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(4000);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveRight(1000, 7);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    jump();

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveRight(500, 7);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    idle(500);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveLeft(7000, 2);

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

        public void runScript3()  //go mental
        {
            bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)       //do this stuff in the background
            {
                while (true)
                {
                    moveRight(500, 10);

                    if (bw.CancellationPending == true)
                    {
                        break;
                    }

                    moveLeft(500, 10);

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

                    activity = EnemyActivity.enemyIdle;
                }
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

        public void drawAnimation(GameTime gameTime, SpriteBatch spriteBatch, bool active)
        {
            if (active)
            {
                animation.Position = Position;
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

            