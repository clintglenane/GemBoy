using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GemBoy
{
    class GlidingPlatform
    {
        #region Fields

        // This platform belongs to this level.      
        Level level;

        // Graphics.      
        Texture2D texture;
        Vector2 origin;

        // Place in the level.      
        Vector2 position;
        Rectangle localBounds;

        // Used to swap direction.      
        public FaceDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        FaceDirection direction;

        float moveSpeed;
        float waitTime;
        float maxWaitTime;
        TileMovement glidermovement;
        string name;

        #endregion

        #region Properties

        public Level Level
        {
            get { return level; }
        }

        /// <summary>      
        /// Position in world space of the bottom center of this enemy.      
        /// </summary>      
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float MoveSpeed
        {
            get { return moveSpeed; }
        }

        public float Velocity
        {
            get { return moveSpeed * (int)direction; }
        }

        /// <summary>      
        /// Gets a rectangle which bounds this platform in world space.      
        /// </summary>      
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>      
        /// Gets a rectangle which bounds the surface of this platform.      
        /// </summary>      
        public Rectangle Surface
        {
            get
            {
                int left = (int)Math.Round(Position.X - origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, 1); // this rectangle is 1 pixel tall.      
            }
        }

        #endregion

        #region Initialize

        /// <summary>      
        /// Constructor      
        /// </summary>      
        /// <param name="level">The level this platform belongs to.</param>      
        /// <param name="position">Where the platform is in the level.</param>      
        /// <param name="faceDirection">The direction it is moving.</param>      
        /// <param name="moveSpeed">How fast it is moving.</param>      
        /// <param name="maxWaitTime">How long it will wait before turning around.</param>      
        public GlidingPlatform(Level level, Vector2 position, FaceDirection faceDirection,
            float moveSpeed, float maxWaitTime, TileMovement glidermovement, string name)
        {

            this.level = level;                 //We need to know what level the platform is in.      
            Position = position;                //We need to know its position in that level.      
            this.direction = faceDirection;     //We need to know what direction it will be moving.      
            this.moveSpeed = moveSpeed;         //We need to know how fast it moves.      
            this.maxWaitTime = maxWaitTime;     //We need to know how long it waits before changing directions.    
            this.glidermovement = glidermovement;
            this.name = name;
            //Equiped with these different fields, we can create a variety of glidingPlatforms with minimal effort.      
            //We can create slow platforms, fast platforms, and platforms moving different directions. 

            LoadContent();
        }

        #endregion

        #region Loading

        /// <summary>      
        /// Loads the glidingPlatform's content.      
        /// </summary>      
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Tiles/" + name);
            // The origin of a gliding platform is the middle of its top surface.      
            origin = new Vector2(texture.Width / 2.0f, texture.Height);
            localBounds = new Rectangle(0, 0, texture.Width, texture.Height);
            // Because localBounds is based on the texture      
            // you can design platforms of different sizes in your favorite paint program.      
        }

        #endregion

        #region Update

        /// <summary>      
        /// Moves the platform along a path, waiting at either end.      
        /// </summary>      
        public void Update(GameTime gameTime)
        {
            bool mustmove = true;

            // Get the bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;
            
            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                            direction = (FaceDirection)(-(int)direction);
                            if (glidermovement == TileMovement.Vertical)
                            {
                                Position = new Vector2(Position.X, Position.Y + depth.Y);
                            }
                            else if (glidermovement == TileMovement.Horizontal)
                            {
                                Position = new Vector2(Position.X + depth.X, Position.Y);
                            }

                            mustmove = false;
                            break;
                        }
                    }
                }
                if (!mustmove)
                {
                    break;
                }
            }

            if (mustmove)
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (position.Y < 0.0f - bounds.Height)
                {
                    position = new Vector2(position.X, (level.Height * Tile.Height)+bounds.Height);
                }
                else if (position.Y > (level.Height * Tile.Height) + bounds.Height)
                {
                    position = new Vector2(position.X, 0.0f);
                }
                else
                {
                    if (glidermovement == TileMovement.Vertical)
                    {
                        Vector2 velocity = new Vector2(0.0f, (int)direction * moveSpeed * elapsed);
                        position = position + velocity;
                    }
                    else if (glidermovement == TileMovement.Horizontal)
                    {
                        Vector2 velocity = new Vector2((int)direction * moveSpeed * elapsed, 0.0f);
                        position = position + velocity;
                    }
                }
            }
        }

        #endregion

        #region Draw

        /// <summary>      
        /// Draws the gliding platform.      
        /// </summary>      
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        #endregion      

    }
}
