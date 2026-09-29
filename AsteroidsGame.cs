using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    public class AsteroidsGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;
        private Texture2D _pixel = null!;

        // Game state
        private Plane _player = null!;
        private readonly List<Asteroid> _asteroids = new();
        private readonly List<Bullet> _playerBullets = new();
        private readonly List<Bullet> _ufoBullets = new();
        private Ufo? _enemyUfo;

        private bool _turningLeft;
        private bool _turningRight;
        private bool _accelerating;
        private bool _shieldOn;
        private bool _gameOver;

        private int _score;
        private int _lives;
        private int _shieldRemaining;
        private int _ufoCount;
        private int _ufoBulletCount;
        private int _ufoExplosionX;
        private int _ufoExplosionY;
        private int _ufoExplosionRange;
        private int _asteroidScore;
        private double _asteroidSpeed;

        private const int StartingAsteroidNumber = 4;
        private const double AsteroidRadiusLarge = 40.0;
        private const double AsteroidRadiusMedium = AsteroidRadiusLarge / 2.0;
        private const double AsteroidRadiusSmall = AsteroidRadiusMedium / 2.0;
        private const double MaxBullets = 10;
        private const int UfoDelay = 300;
        private const int UfoShootDelay = 16;

        private readonly Random _rng = new();

        public AsteroidsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set a reasonable default backbuffer size similar to the old WinForms client size
            _graphics.PreferredBackBufferWidth = 492;
            _graphics.PreferredBackBufferHeight = 473;
        }

        protected override void Initialize()
        {
            _player = new Plane();
            StartGame();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 1x1 white pixel texture for line/rect drawing
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        private void StartGame()
        {
            _score = 0;
            _lives = 3;
            _shieldRemaining = 100;
            _ufoCount = 0;
            _ufoBulletCount = 0;
            _ufoExplosionRange = -1;
            _gameOver = false;
            _asteroidSpeed = 1.5;
            _asteroidScore = 0;
            _enemyUfo = null;

            NextLevel();
        }

        private void NextLevel()
        {
            _asteroidSpeed += 0.5;
            _asteroidScore++;

            _asteroids.Clear();
            _playerBullets.Clear();
            _ufoBullets.Clear();

            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            for (int i = 0; i < StartingAsteroidNumber; i++)
            {
                _asteroids.Add(new Asteroid(AsteroidRadiusLarge, new System.Numerics.Vector2(0, 0), _rng.Next(0, 360), _asteroidSpeed, w, h));
            }

            ResetPlayer();
        }

        private void ResetPlayer()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            bool collision;
            Vector2 spawnPosition = new(w / 2f, h / 2f);

            do
            {
                collision = false;

                _player.SetLocation(new System.Numerics.Vector2(spawnPosition.X, spawnPosition.Y), 0, 0, 0, w, h);

                foreach (var asteroid in _asteroids)
                {
                    foreach (var vertex in _player.PositionVertices)
                    {
                        if (asteroid.CheckCollision(vertex))
                        {
                            collision = true;
                            break;
                        }
                    }
                    if (collision) break;
                }

                spawnPosition = new Vector2(
                    _rng.Next(50, w - 50),
                    _rng.Next(50, h - 50));

            } while (collision);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (!_gameOver)
            {
                HandleInput(keyboard);
                UpdateShield();
                MoveAll();
                TestUfoShoot();
                CheckHit();

                if (_asteroids.Count == 0)
                {
                    NextLevel();
                }
            }

            base.Update(gameTime);
        }

        private void HandleInput(KeyboardState keyboard)
        {
            _turningLeft = keyboard.IsKeyDown(Keys.Left);
            _turningRight = keyboard.IsKeyDown(Keys.Right);
            _accelerating = keyboard.IsKeyDown(Keys.Up);

            // Shield
            if (keyboard.IsKeyDown(Keys.Down) && _shieldRemaining > 0)
                _shieldOn = true;
            if (keyboard.IsKeyUp(Keys.Down))
                _shieldOn = false;

            // Shoot
            if (keyboard.IsKeyDown(Keys.Space) && !_shieldOn && _playerBullets.Count < MaxBullets)
            {
                Vector2 bulletPos = new(
                    (float)(_player.Position.X + 10.0 * Math.Sin(_player.HeadingRadians)),
                    (float)(_player.Position.Y - 10.0 * Math.Cos(_player.HeadingRadians)));

                _playerBullets.Add(new Bullet(new System.Numerics.Vector2(bulletPos.X, bulletPos.Y), _player.Heading));
            }

            // Restart
            if (keyboard.IsKeyDown(Keys.F5))
            {
                StartGame();
            }
        }

        private void UpdateShield()
        {
            if (_shieldOn)
            {
                _shieldRemaining -= 1;
                if (_shieldRemaining <= 0)
                {
                    _shieldOn = false;
                    _shieldRemaining = 0;
                }
            }
        }

        private void MoveAll()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            int newHeading = _player.Heading;
            double speedX = _player.SpeedX;
            double speedY = _player.SpeedY;

            if (_turningLeft)
            {
                newHeading -= 7;
                if (newHeading < 0) newHeading += 360;
            }

            if (_turningRight)
            {
                newHeading += 7;
                if (newHeading >= 360) newHeading -= 360;
            }

            if (_accelerating)
            {
                speedX += 0.2 * Math.Sin(_player.HeadingRadians);
                speedY -= 0.2 * Math.Cos(_player.HeadingRadians);

                double newSpeed = Math.Sqrt(speedX * speedX + speedY * speedY);
                if (newSpeed > 5)
                {
                    double scaleFactor = 5.0 / newSpeed;
                    speedX *= scaleFactor;
                    speedY *= scaleFactor;
                }
            }

            _player.SetLocation(_player.Position, newHeading, speedX, speedY, w, h);

            // Player bullets
            int index = 0;
            while (index < _playerBullets.Count)
            {
                Bullet bullet = _playerBullets[index];
                bool exited = bullet.MoveBullet(w, h);
                if (exited)
                {
                    _playerBullets.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            // Asteroids
            foreach (var asteroid in _asteroids)
            {
                asteroid.MoveAsteroid(w, h);
            }

            // UFO bullets
            index = 0;
            while (index < _ufoBullets.Count)
            {
                Bullet bullet = _ufoBullets[index];
                bool exited = bullet.MoveBullet(w, h);
                if (exited)
                {
                    _ufoBullets.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            // UFO
            if (_enemyUfo != null)
            {
                bool exited = _enemyUfo.MoveUfo(w, h);
                if (exited)
                {
                    _enemyUfo = null;
                }
            }
        }

        private void TestUfoShoot()
        {
            if (_enemyUfo != null)
            {
                _ufoBulletCount++;
                if (_ufoBulletCount == UfoShootDelay)
                {
                    _ufoBulletCount = 0;
                    UfoShoot();
                }
            }
        }

        private void UfoShoot()
        {
            if (_enemyUfo != null && _ufoBullets.Count < MaxBullets)
            {
                _ufoBullets.Add(new Bullet(_enemyUfo.Position, _rng.Next(360)));
            }
        }

        private void DecreaseLives()
        {
            _lives--;
            if (_lives <= 0)
            {
                _lives = 0;
                _gameOver = true;
            }
            else
            {
                _shieldRemaining = 100;
                ResetPlayer();
            }
        }

        private void CheckHit()
        {
            // Player bullets vs asteroids
            int i = 0;
            while (i < _playerBullets.Count)
            {
                Bullet bullet = _playerBullets[i];
                bool hit = false;

                for (int j = 0; j < _asteroids.Count && !hit; j++)
                {
                    Asteroid asteroid = _asteroids[j];
                    if (asteroid.CheckCollision(bullet.Position))
                    {
                        _playerBullets.RemoveAt(i);
                        _asteroids.RemoveAt(j);
                        hit = true;

                        if (asteroid.Radius == AsteroidRadiusLarge)
                        {
                            int newHeading = asteroid.Heading - 90;
                            if (newHeading < 0) newHeading += 360;
                            _asteroids.Add(new Asteroid(AsteroidRadiusMedium, asteroid.Position, newHeading,
                                _asteroidSpeed, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                            newHeading = asteroid.Heading + 90;
                            if (newHeading >= 360) newHeading -= 360;
                            _asteroids.Add(new Asteroid(AsteroidRadiusMedium, asteroid.Position, newHeading,
                                _asteroidSpeed, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                            _score += _asteroidScore;
                        }
                        else if (asteroid.Radius == AsteroidRadiusMedium)
                        {
                            int newHeading = asteroid.Heading - 90;
                            if (newHeading < 0) newHeading += 360;
                            _asteroids.Add(new Asteroid(AsteroidRadiusSmall, asteroid.Position, newHeading,
                                _asteroidSpeed, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                            newHeading = asteroid.Heading + 90;
                            if (newHeading >= 360) newHeading -= 360;
                            _asteroids.Add(new Asteroid(AsteroidRadiusSmall, asteroid.Position, newHeading,
                                _asteroidSpeed, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                            _score += (_asteroidScore + 1);
                        }
                        else
                        {
                            _score += (_asteroidScore + 2);
                        }
                    }
                }

                if (!hit)
                {
                    i++;
                }
            }

            // Player vs asteroids
            if (!_shieldOn)
            {
                foreach (var vertex in _player.PositionVertices)
                {
                    foreach (var asteroid in _asteroids)
                    {
                        if (asteroid.CheckCollision(vertex))
                        {
                            DecreaseLives();
                            return;
                        }
                    }
                }
            }

            // TODO: For brevity, UFO-related collision (explosion radius, bullets vs player) can be added similarly.
        }

        private void GenerateUfo()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            _enemyUfo = new Ufo();

            int dir = _rng.Next(4);
            int head = _rng.Next(80) - 39;

            if (dir == 0)
            {
                _enemyUfo.SetLocation(new System.Numerics.Vector2(0, _rng.Next(h) / 2 + h / 4), 90 + head, w, h);
            }
            else if (dir == 1)
            {
                _enemyUfo.SetLocation(new System.Numerics.Vector2(w, _rng.Next(h) / 2 + h / 4), 270 + head, w, h);
            }
            else if (dir == 2)
            {
                _enemyUfo.SetLocation(new System.Numerics.Vector2(w / 2 + h / 4, 0), 180 + head, w, h);
            }
            else
            {
                if (head < 0) head += 360;
                _enemyUfo.SetLocation(new System.Numerics.Vector2(w / 2 + h / 4, h), head, w, h);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            // Background
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, w, h), new Color(20, 20, 40));

            // Asteroids
            foreach (var asteroid in _asteroids)
            {
                DrawPolygon(asteroid.PositionVertices, asteroid.Color);
            }

            // UFO
            if (_enemyUfo != null)
            {
                DrawPolygon(_enemyUfo.PositionVertices, Color.LightGreen);
            }

            // Player
            DrawPolygon(_player.PositionVertices, Color.Gray);

            // Thrust (simple orange triangle when accelerating)
            if (_accelerating)
            {
                DrawPolygon(_playerThrustVertices(), Color.Orange);
            }

            // Player bullets
            foreach (var bullet in _playerBullets)
            {
                DrawCircle(bullet.Position, 2, Color.White);
            }

            // UFO bullets
            foreach (var bullet in _ufoBullets)
            {
                DrawCircle(bullet.Position, 2, Color.LightGreen);
            }

            // Shield
            if (_shieldOn)
            {
                DrawCircle(_player.Position, 20, new Color(200, 200, 255));
            }

            // HUD text (simple white text; requires a SpriteFont if you want real text)
            // For now we skip text rendering to avoid needing content pipeline setup.

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private System.Numerics.Vector2[] _playerThrustVertices()
        {
            // Approximate thrust as a small triangle behind the ship.
            return new[]
            {
                new System.Numerics.Vector2(_player.Position.X, _player.Position.Y + 25),
                new System.Numerics.Vector2(_player.Position.X + 5, _player.Position.Y + 10),
                new System.Numerics.Vector2(_player.Position.X - 5, _player.Position.Y + 10),
            };
        }

        private void DrawPolygon(System.Numerics.Vector2[] vertices, Color fillColor)
        {
            if (vertices.Length < 2) return;

            // Fill: naive triangle fan (center + edges)
            System.Numerics.Vector2 center = System.Numerics.Vector2.Zero;
            foreach (var v in vertices) center += v;
            center /= vertices.Length;

            for (int i = 0; i < vertices.Length; i++)
            {
                System.Numerics.Vector2 v0 = vertices[i];
                System.Numerics.Vector2 v1 = vertices[(i + 1) % vertices.Length];
                DrawTriangle(v0, v1, center, fillColor);

                // Outline
                DrawLine(v0, v1, Color.Black, 3f);
            }
        }

        private void DrawTriangle(System.Numerics.Vector2 v0, System.Numerics.Vector2 v1, System.Numerics.Vector2 v2, Color color)
        {
            // Very simple approximation: draw three lines; for a real filled triangle you’d use a custom vertex buffer.
            DrawLine(v0, v1, color, 1f);
            DrawLine(v1, v2, color, 1f);
            DrawLine(v2, v0, color, 1f);
        }

        private void DrawLine(System.Numerics.Vector2 from, System.Numerics.Vector2 to, Color color, float thickness)
        {
            System.Numerics.Vector2 edge = to - from;
            float length = edge.Length();
            float rotation = (float)Math.Atan2(edge.Y, edge.X);

            _spriteBatch.Draw(_pixel,
                destinationRectangle: new Rectangle((int)from.X, (int)from.Y, (int)length, (int)thickness),
                sourceRectangle: null,
                color: color,
                rotation: rotation,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: 0f);
        }

        private void DrawCircle(System.Numerics.Vector2 center, float radius, Color color)
        {
            // Approximate circle as a 16-gon.
            int segments = 16;
            System.Numerics.Vector2[] vertices = new System.Numerics.Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * MathF.Tau / segments;
                vertices[i] = center + new System.Numerics.Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
            }

            DrawPolygon(vertices, color);
        }
    }
}
