using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Prototype1
{
    public enum Activity
    {
        Jumping,
        Running,
        Idle,
        None
    }

    public class CompositeCharacter : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        private float centerOffset;

        public Activity activity;
        protected Activity oldActivity;

        private Vector2 jumpForce;
        private float jumpDelayTime;

        private const float nextJumpDelayTime = 0f;
        private const float runSpeed = 20;
        private const float jumpImpulse = -1.5f;
        private const float maxAirVelocity = 6f;
        private const float inAirImpluse = 0.06f;

        public CompositeCharacter(World world, Vector2 position, float width, float height, float mass, Texture2D texture)
            : base(world, position, width, height, mass, texture)
        {
            if (width > height)
            {
                throw new Exception("Error width > height: can't make character because wheel would stick out of body");
            }

            activity = Activity.None;

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
            if (activity == Activity.Jumping && oldActivity == Activity.Jumping)
            {
                activity = Activity.None;
            }
            return true;
        }

        protected override void HandleInput(GameTime gameTime)
        {
            oldActivity = activity;
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(0);

            //Console.WriteLine("CURRENT LINEAR VELOCITY: X= " + body.LinearVelocity.X + "   Y= " + body.LinearVelocity.Y);
                    

            HandleJumping(keyState, oldState, padState, oldPadState, gameTime);

            if (activity != Activity.Jumping)
            {
                HandleRunning(keyState, oldState, padState, oldPadState, gameTime);
            }           

            if (activity != Activity.Jumping && activity != Activity.Running)
            {
                HandleIdle(keyState, oldState, padState, oldPadState, gameTime);
            }

            oldState = keyState;
            oldPadState = padState;
        }

        private void HandleJumping(KeyboardState state, KeyboardState oldState, GamePadState padState, GamePadState oldPadState, GameTime gameTime)
        {
            if (jumpDelayTime < 0)
            {
                jumpDelayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if ((state.IsKeyUp(Keys.Space) && oldState.IsKeyDown(Keys.Space)) || (padState.Buttons.LeftShoulder == ButtonState.Pressed && oldPadState.Buttons.LeftShoulder == ButtonState.Released) && activity != Activity.Jumping)
            {
                if (jumpDelayTime >= 0)
                {
                    motor.MotorSpeed = 0;
                    jumpForce.Y = jumpImpulse;
                    body.ApplyLinearImpulse(jumpForce, body.Position);
                    jumpDelayTime = -nextJumpDelayTime;
                    activity = Activity.Jumping;
                }
            }

            if (activity == Activity.Jumping)
            {
                if (keyState.IsKeyDown(Keys.Right) || padState.ThumbSticks.Left.X > 0.1)
                {
                    Console.WriteLine("result impulse: " + inAirImpluse * Math.Abs(padState.ThumbSticks.Left.X));
                    if (body.LinearVelocity.X < maxAirVelocity)
                    {
                        body.ApplyLinearImpulse(new Vector2(inAirImpluse * Math.Abs(padState.ThumbSticks.Left.X * padState.ThumbSticks.Left.X), 0f), body.Position);
                    }                
                }
                else if (keyState.IsKeyDown(Keys.Left) || padState.ThumbSticks.Left.X < -0.1)
                {
                    if (body.LinearVelocity.X > -maxAirVelocity)
                    {
                        body.ApplyLinearImpulse(new Vector2(-inAirImpluse * Math.Abs(padState.ThumbSticks.Left.X * padState.ThumbSticks.Left.X), 0f), body.Position);
                    }
                }
            }            
        }

        private void HandleRunning(KeyboardState state, KeyboardState oldState, GamePadState padState, GamePadState oldPadState, GameTime gameTime)
        {
            //keyboard
            if (keyState.IsKeyDown(Keys.Right))
            {
                motor.MotorSpeed = runSpeed;
                activity = Activity.Running;
            }
            else if (keyState.IsKeyDown(Keys.Left))
            {                
                motor.MotorSpeed = -runSpeed;
                activity = Activity.Running;
            }

            //joypad
            if (padState.ThumbSticks.Left.X > 0.1f)           //the speed gets exponentially slower the less the analog stick is tilted
            {
                motor.MotorSpeed = runSpeed * (padState.ThumbSticks.Left.X * padState.ThumbSticks.Left.X);
                activity = Activity.Running;
            }
            else if (padState.ThumbSticks.Left.X < -0.1f)
            {
                motor.MotorSpeed = -runSpeed * (padState.ThumbSticks.Left.X * padState.ThumbSticks.Left.X);
                activity = Activity.Running;
            }

            if (keyState.IsKeyUp(Keys.Left) && keyState.IsKeyUp(Keys.Right) && padState.ThumbSticks.Left.X <= 0.3f && padState.ThumbSticks.Left.X >= -0.3f)
            {
                motor.MotorSpeed = 0;
                activity = Activity.None;
            }           
        }

        private void HandleIdle(KeyboardState state, KeyboardState oldState, GamePadState padState, GamePadState oldPadState, GameTime gameTime)
        {
            if (activity == Activity.None)
            {
                activity = Activity.Idle;
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

        public override Vector2 Position
        {
            get
            {
                return (ConvertUnits.ToDisplayUnits(body.Position) + Vector2.UnitY * centerOffset);
            }
        }
    }
}

            