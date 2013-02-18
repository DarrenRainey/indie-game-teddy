///////////////////////////////////////
 /* Hugh Desmond - Final year project
  * 
  * Keyboard Controls:
  * 
  * move                    -  arrow left/right
  * jump                    -  space
  * camera                  -  WASD
  * show physics box        -  B
  * code gen mode toggle    -  F
  * generate tarrain code   -  LMB
  * 
  *  
  */
//////////////////////////////////////


using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using Prototype2;
using System.Collections.Generic;

namespace Prototype1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;
        Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

        //private Camera2D camera;
        //private List<Scene2DNode> nodeList;
        
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private MouseState _oldMouseState;
        public SpriteFont _font;

        private World _world;

        private Body _circleBody;
        private Body _groundBody;
        private Body playerBody;
        private Body backgroundBody;

        private List<PassableEdge> passableEdges;

        private CompositeCharacter box; 

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;        
        private Texture2D player;
        private Texture2D playerTexture;
        private Texture2D playerJump;
        private Texture2D playerShoot;
        private Texture2D armgun;
        private Texture2D head;

        private Texture2D tile1;
        private Texture2D tile2;

        private Queue bulletQueue;
        private Array tempBulletArray;
        private Texture2D bulletTex;

        //private Vector2 bulletPos;
        //private Boolean isBullet;
        //private Vector2 bulletDirection;       //the XY increment for the bullet
        //private int bulletSpeed = 2;
        //private Vector2 bulletOrigin;

        private SpriteEffects armgunEffects;
        private float armgunAngle;
        private double armgunAngleInDegrees;

        private float headAngle;      
        

        private Animation playerAnimation;
        
        private Activity oldActivity;

        private Texture2D squareTex;        

        private Vector2 playerPos;     

        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private bool freeViewOn = false;            //controllable camera   
        
        string Text = "BASIC HUD";

        FrameRateCounter fpsCounter;

        private bool showBox = false;
                
        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1 meters equals 100 pixels here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPixels = 100f;

        private Texture2D crosshair;
        private Texture2D marker;

        private Vector2 previousClickWorldPos = Vector2.Zero;
        private ArrayList markers;
        

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;

            //_graphics.IsFullScreen = true;

            //camera = new Camera2D(_batch);

            Content.RootDirectory = "Content";

            fpsCounter = new FrameRateCounter();

            _world = new World(new Vector2(0, 30f));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = new Vector2(0f,0f);

            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);

            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("circleSprite"); //  96px x 96px => 0.96m x 0.96m
            _groundSprite = Content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   5.12m x 0.64m            
            player = Content.Load<Texture2D>("player1"); // 95px x 80px =>   1m x 1.25m
            armgun = Content.Load<Texture2D>("armgun"); // 28px x 67px =>   1m x 1.25m
            head = Content.Load<Texture2D>("head"); // 41px x 37px   
            bulletTex = Content.Load<Texture2D>("bullet");

            squareTex = Content.Load<Texture2D>("square");

            playerJump = Content.Load<Texture2D>("jumping");

            //editing textures
            crosshair = Content.Load<Texture2D>("crosshair");
            marker = Content.Load<Texture2D>("marker");

            //load level tiles
            tile1 = Content.Load<Texture2D>("background1"); //  1280px x 720px => 12.8m x 7.2m
            tile2 = Content.Load<Texture2D>("prototype_Tile2"); //  1280px x 720px => 12.8m x 7.2m

            //testings
            playerTexture = Content.Load<Texture2D>("run");
            playerAnimation = new Animation();
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 86, 119, 25, 300, Color.White, 1f, true, new Vector2(0, 0));
         
            box = new CompositeCharacter(_world, new Vector2(100f, screenCenter.Y / MeterInPixels), 64, 128, 0.3f, squareTex);
            box.forcePower = 100;

            //init bullet stuff
            //bulletDirection = new Vector2(0, 0);
            bulletQueue = new Queue();
            tempBulletArray = bulletQueue.ToArray();

            markers = new ArrayList();

            /*
            //player
            Vector2 playerPos1 = new Vector2(1f, screenCenter.Y / MeterInPixels);

            // Create the player fixture
            //playerBody = BodyFactory.CreateRectangle(_world, player.Width / MeterInPixels, player.Height / MeterInPixels, 0.7f, playerPos1 + (new Vector2(player.Width / 2, player.Height / 2) / MeterInPixels));
            playerBody = BodyFactory.CreateCircle(_world, player.Height / (2f * MeterInPixels), 1f, playerPos1 + (new Vector2(player.Width / 2, player.Height / 2) / MeterInPixels));
            playerBody.BodyType = BodyType.Dynamic;
            playerBody.Restitution = 0.0f;
            playerBody.Friction = 0.1f; 


            //Circle 
            Vector2 circlePosition = new Vector2((1f + (_groundSprite.Width / MeterInPixels) / 2) - ((_circleSprite.Width / MeterInPixels) / 2), 0f) + (new Vector2(_circleSprite.Width / MeterInPixels, _circleSprite.Height / MeterInPixels) / 2);

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, 96f / (2f * MeterInPixels), 1f, circlePosition);
            _circleBody.BodyType = BodyType.Dynamic;
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;

            //Ground
            Vector2 groundPosition = new Vector2(1f, 6.5f) + (new Vector2(_groundSprite.Width / MeterInPixels, _groundSprite.Height / MeterInPixels) / 2);

            // Create the ground fixture
            _groundBody = BodyFactory.CreateRectangle(_world, 512f / MeterInPixels, 64f / MeterInPixels, 1f, groundPosition);
            _groundBody.IsStatic = true;
            _groundBody.Restitution = 0.3f;
            _groundBody.Friction = 0.5f;
            
            */

            //edge

            // Create the ground fixture
            Body impassableEdge = BodyFactory.CreateEdge(_world, new Vector2(0f, 3f), new Vector2(0.25f, 3f));
            impassableEdge.IsStatic = true;
            impassableEdge.Restitution = 0.1f;
            impassableEdge.Friction = 0.7f;

            //TILE1...........
            //bottom left piece
            FixtureFactory.AttachEdge(new Vector2(0.25f, 3f), new Vector2(0.25f, 6f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(0.25f, 6f), new Vector2(5f, 6f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(5f, 6f), new Vector2(5f, 7.2f), impassableEdge);

            //bottom right piece
            FixtureFactory.AttachEdge(new Vector2(7f, 7.2f), new Vector2(7f, 5f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(7f, 5f), new Vector2(12.8f, 5f), impassableEdge);

            //raised platform right
            //FixtureFactory.AttachEdge(new Vector2(12.8f, 3f), new Vector2(10f, 3f), impassableEdge);
            //FixtureFactory.AttachEdge(new Vector2(10f, 3f), new Vector2(10f, 2.5f), impassableEdge);
            //FixtureFactory.AttachEdge(new Vector2(10f, 2.5f), new Vector2(12.8f, 2.5f), impassableEdge);

            //raised platform middle
            //FixtureFactory.AttachEdge(new Vector2(4f, 1.5f), new Vector2(8f, 1.5f), impassableEdge);
           // FixtureFactory.AttachEdge(new Vector2(8f, 1.5f), new Vector2(8f, 2f), impassableEdge);
           // FixtureFactory.AttachEdge(new Vector2(8f, 2f), new Vector2(4f, 2f), impassableEdge);
           // FixtureFactory.AttachEdge(new Vector2(4f, 2f), new Vector2(4f, 1.5f), impassableEdge);   
            
 
            //code gen test            
            FixtureFactory.AttachEdge(new Vector2(12.79f, 4.995f), new Vector2(13.89f, 5.105f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(13.89f, 5.105f), new Vector2(15.14f, 5.365f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(15.14f, 5.365f), new Vector2(16.23f, 5.745f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(16.23f, 5.745f), new Vector2(17.3f, 6.095f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(17.3f, 6.095f), new Vector2(18.65f, 6.345f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(18.65f, 6.345f), new Vector2(19.85f, 6.495f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(19.85f, 6.495f), new Vector2(21.78f, 6.515f), impassableEdge);
            FixtureFactory.AttachEdge(new Vector2(21.78f, 6.515f), new Vector2(25.6f, 5.185f), impassableEdge);


            //passable edges
            passableEdges = new List<PassableEdge>();

            passableEdges.Add(new PassableEdge(_world, new Vector2(10f, 2.5f), new Vector2(12.8f, 2.5f)));    //right rased platform
            passableEdges.Add(new PassableEdge(_world, new Vector2(4f, 1.5f), new Vector2(8f, 1.5f)));    //centred raised platform
            
               

            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {            
            HandleGamePad();
            HandleKeyboard();
            HandleMouse();

            fpsCounter.Update(gameTime);

            if (box.activity == Activity.Idle && oldActivity != Activity.Idle)
            {
                Vector2 temp = playerAnimation.Position;
                playerAnimation.Initialize(player, temp, 100, 123, 1, 0, Color.White, 1f, true, new Vector2(0, 0));                
            }
            else if (box.activity == Activity.Jumping && oldActivity != Activity.Jumping)
            {
                Vector2 temp = playerAnimation.Position;
                playerAnimation.Initialize(playerJump, temp, 120, 125, 23, 10, Color.White, 1f, false, new Vector2(0, 0));
            }
            else if (box.activity == Activity.Running && oldActivity != Activity.Running)
            {
                Vector2 temp = playerAnimation.Position;
                playerAnimation.Initialize(playerTexture, temp, 86, 121, 26, 30, Color.White, 1f, true, new Vector2(0, 0));
            }            

            oldActivity = box.activity;              

            box.Update(gameTime);
            
            playerAnimation.Update(gameTime);

            //update all passable edges
            for (int i = 0; i < passableEdges.Count; i++) 
            {
                passableEdges[i].Update(box);              
            }

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

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

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();
                                

                if (padState.Buttons.B == ButtonState.Pressed && _oldPadState.Buttons.B == ButtonState.Released)
                {
                    if (showBox == true)
                    {
                        showBox = false;
                    }
                    else
                    {
                        showBox = true;
                    }
                }

                if (padState.Triggers.Right > 0.5 && _oldPadState.Triggers.Right < 0.5)
                {
                    Bullet bullet = new Bullet();
                    bullet.Texture = bulletTex;                    

                    if (padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                    {
                        bullet.DirectionIncrement = (padState.ThumbSticks.Right * new Vector2(1, -1));
                    }
                    else
                    {
                        if (armgunEffects == SpriteEffects.None)
                        {
                            bullet.DirectionIncrement = new Vector2(1, 0);
                        }
                        else if (armgunEffects == SpriteEffects.FlipHorizontally)
                        {
                            bullet.DirectionIncrement = new Vector2(-1, 0);
                        }
                    }

                    Vector2 perpToDirection;

                    //arm pivot point + direction multiplyed by guess number to bring the bullet origin to the tip of the pistol. 
                    //also moved a few pixels 90 degrees perpendicular to direction to match the tip of the gun  
                    if (armgunEffects == SpriteEffects.None)
                    {
                        perpToDirection = new Vector2(bullet.DirectionIncrement.Y, bullet.DirectionIncrement.X * -1) * 12;

                        bullet.Origin = new Vector2(box.Position.X + 10, box.Position.Y - 20) + (bullet.DirectionIncrement * 55) + perpToDirection;
                    }
                    else if (armgunEffects == SpriteEffects.FlipHorizontally)
                    {
                        perpToDirection = new Vector2(bullet.DirectionIncrement.Y * -1, bullet.DirectionIncrement.X) * 12;

                        bullet.Origin = new Vector2(box.Position.X - 20, box.Position.Y - 20) + (bullet.DirectionIncrement * 55) + perpToDirection;
                    }

                    bullet.CurrentPos = bullet.Origin;
                    bullet.DirectionIncrement *= bullet.Speed; 

                    bulletQueue.Enqueue(bullet);    //add the bullet to the queue of bullets
                }

                if (padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                {
                    armgunAngle = (float)Math.Atan2(padState.ThumbSticks.Right.Y, padState.ThumbSticks.Right.X) * -1;

                    armgunAngleInDegrees = (armgunAngle * 180) / Math.PI;

                    if (armgunAngleInDegrees > 90 || armgunAngleInDegrees < -90)
                    {
                        playerAnimation.myEffect = SpriteEffects.FlipHorizontally;
                        armgunEffects = SpriteEffects.FlipHorizontally;
                    }
                    else
                    {
                        playerAnimation.myEffect = SpriteEffects.None;
                        armgunEffects = SpriteEffects.None;
                    }
                }
                else
                {                   
                    armgunAngle = 0.0f;
                    headAngle = 0.0f;                   
                }

                //camera follow player  
                if (freeViewOn == false)
                {
                    _cameraPosition = (box.Position - _screenCenter) * -1;
                }

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();

            
                // Move camera
                if (state.IsKeyDown(Keys.A))
                    _cameraPosition.X += 5f;

                if (state.IsKeyDown(Keys.D))
                    _cameraPosition.X -= 5f;

                if (state.IsKeyDown(Keys.W))
                    _cameraPosition.Y += 5f;

                if (state.IsKeyDown(Keys.S))
                    _cameraPosition.Y -= 5f;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                        Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));
            
            
            /*if (state.IsKeyDown(Keys.Left))
            {
                playerAnimation.myEffect = SpriteEffects.FlipHorizontally;
                shootAnimation.myEffect = SpriteEffects.FlipHorizontally;
                armgunEffects = SpriteEffects.FlipHorizontally;
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                playerAnimation.myEffect = SpriteEffects.None;
                shootAnimation.myEffect = SpriteEffects.None;
                armgunEffects = SpriteEffects.None;
            }*/

            if (state.IsKeyDown(Keys.B) && _oldKeyState.IsKeyDown(Keys.B) == false)
            {
                if (showBox == true)
                {
                    showBox = false;
                }
                else
                {
                    showBox = true;
                }
            }
            
            if (state.IsKeyDown(Keys.F) && _oldKeyState.IsKeyDown(Keys.F) == false)       //f to toggle freeview on/off
            {
                if (freeViewOn == true)
                {
                    freeViewOn = false;
                }
                else
                {
                    freeViewOn = true;
                }
            }            

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        private void HandleMouse()
        {
            MouseState state = Mouse.GetState();

            if (freeViewOn)
            {
                //generate edge code from click coordinates
                if (state.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
                {
                    Vector2 worldPos = (_cameraPosition * -1) + new Vector2(state.X, state.Y);
                    worldPos = new Vector2((float)Math.Round(worldPos.X, 1), (float)Math.Round(worldPos.Y, 1));

                    Console.WriteLine("FixtureFactory.AttachEdge(new Vector2(" + previousClickWorldPos.X / MeterInPixels + "f, " + previousClickWorldPos.Y / MeterInPixels + "f), new Vector2(" + worldPos.X / MeterInPixels + "f, " + worldPos.Y / MeterInPixels + "f), impassableEdge);");

                    markers.Add(new Vector2(worldPos.X - 4, worldPos.Y - 4));

                    previousClickWorldPos = worldPos;
                }
            } 

            _oldMouseState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            fpsCounter.frameCounter++;

            /* Circle position and rotation */
            // Convert physics position (meters) to screen coordinates (pixels)
            //Vector2 circlePos = _circleBody.Position * MeterInPixels;
            //float circleRotation = _circleBody.Rotation;                       

            /* Ground position and origin */
            //Vector2 groundPos = _groundBody.Position * MeterInPixels;
            //Vector2 groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);

            // Align sprite center to body position
            //Vector2 circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            //_batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);

            //_batch.Draw(tile1, new Vector2(0, 0), Color.White);

            //player pos calc
            //playerPos = (playerBody.Position * MeterInPixels) - new Vector2(player.Width / 2, player.Height / 2);

            //Console.WriteLine("PLAYER BODY POS: x=" + playerBody.Position.X + "  y: " + playerBody.Position.Y);

            //draw player
            //_batch.Draw(player, playerPos, Color.White);
            
            //Draw circle
            //_batch.Draw(_circleSprite, circlePos, null, Color.White, circleRotation, circleOrigin, 1f, SpriteEffects.None, 0f);

            //Draw ground
            //_batch.Draw(_groundSprite, groundPos, null, Color.White, 0f, groundOrigin, 1f, SpriteEffects.None, 0f);

            //_batch.End();

            //_batch.Begin();

            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);

            _batch.Draw(tile1, new Vector2(0, 0), Color.White);
            _batch.Draw(tile2, new Vector2(1280, 0), Color.White); 

            if (showBox == true)
            {
                box.Draw(_batch);
            }

            int armgunXOffset = 0;            
            Vector2 armgunOrigin = Vector2.Zero;            

            if (armgunEffects == SpriteEffects.None)
            {
                armgunXOffset = 17;
                armgunOrigin = new Vector2(6, 17);                
            }
            else if (armgunEffects == SpriteEffects.FlipHorizontally)
            {
                armgunXOffset = -18;
                armgunOrigin = new Vector2(60, 17);                

                GamePadState padState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

                if(padState.ThumbSticks.Right.X > 0.50 || padState.ThumbSticks.Right.X < -0.50 || padState.ThumbSticks.Right.Y > 0.50 || padState.ThumbSticks.Right.Y < -0.50)
                {
                    if (armgunAngleInDegrees < -180 || armgunAngleInDegrees > -90)
                    {
                        armgunAngleInDegrees = -180 + armgunAngleInDegrees;

                        armgunAngle = (float)(armgunAngleInDegrees * Math.PI) / 180;
                    }
                    else if (armgunAngleInDegrees < 180 || armgunAngleInDegrees > 90)
                    {
                        armgunAngleInDegrees = (armgunAngleInDegrees + 180);                       

                        armgunAngle = (float)(armgunAngleInDegrees * Math.PI) / 180;                        
                    }                    
                }
            }

            headAngle = armgunAngle / 3;             //head rotates with analog stick but 3 times slower

            //Console.WriteLine("drawangle: " + armgunAngle * 180 / Math.PI);

            _batch.Draw(armgun, new Rectangle((int)box.Position.X + armgunXOffset, (int)box.Position.Y - 15, armgun.Width, armgun.Height), new Rectangle(0, 0, armgun.Width, armgun.Height), Color.White, armgunAngle, armgunOrigin, armgunEffects, 0.0f);


            //testings 
            box.Update(gameTime);
            playerAnimation.Position = box.Position;  
            playerAnimation.Update(gameTime);
            playerAnimation.Draw(_batch);



            _batch.Draw(head, new Rectangle((int)playerAnimation.Position.X + 0, (int)playerAnimation.Position.Y - 35, head.Width, head.Height), new Rectangle(0, 0, head.Width, head.Height), Color.White, headAngle, new Vector2(20, 18), armgunEffects, 0.0f);
                           

            for (int i = 0; i < tempBulletArray.Length; i++)
            {
                _batch.Draw(((Bullet)tempBulletArray.GetValue(i)).Texture, ((Bullet)tempBulletArray.GetValue(i)).CurrentPos, Color.White);
            }

            //draw markers for the points clicked in edit mode (points which make up a collision edge)
            if (freeViewOn)
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    _batch.Draw(marker, (Vector2)markers[i], Color.White);
                }
            }
                        
            _batch.End();


            //draw items attached to screen as aposed to world
            _batch.Begin();

            Text = "FPS: " + fpsCounter.frameRate + "    player effect: " + playerAnimation.myEffect;

            // Display instructions
            _batch.DrawString(_font, Text, new Vector2(34f, 34f), Color.Black);
            _batch.DrawString(_font, Text, new Vector2(32f, 32f), Color.White);

            _batch.DrawString(_font, box.activity.ToString(), new Vector2(90, 105), Color.Red);

            //draw crosshair at mouse
            if (freeViewOn)
            {          
                _batch.Draw(crosshair, new Vector2(Mouse.GetState().X - 4, Mouse.GetState().Y - 4), Color.White);             
            }

            _batch.End();


            base.Draw(gameTime);
        }

        private float DegreeToRadian(float angle)
        {
            float PI = (float)Math.PI;

            return PI * angle / 180.0f;
        }
    }
}