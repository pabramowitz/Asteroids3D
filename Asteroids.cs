namespace Asteroids
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Numerics;
    using System.Windows.Forms;

    /// <summary>
    ///    Summary description for Form1.
    /// </summary>
    public partial class Asteroids : Form
    {
        private const int StartingAsteroidNumber = 4;
        private const double AsteroidRadiusLarge = 40.0;
        private const double AsteroidRadiusMedium = AsteroidRadiusLarge / 2.0;
        private const double AsteroidRadiusSmall = AsteroidRadiusMedium / 2.0;
        private const double MaxBullets = 10;
        private const int UfoDelay = 300;
        private const int UfoShootDelay = 16;

        List<Asteroid> ActiveAsteroids;
        List<Bullet> PlayerBullets;
        List<Bullet> UfoBullets;
        Plane PlayerPlane;
        Ufo? EnemyUfo;

        SolidBrush BlackBrush, TextBrush, BulletBrush, UfoBrush;
        Pen ShieldPen, UfoPen;
        Font TextFont;
        Graphics BitmapGc, WindowsGc;
        Boolean TurningLeft, TurningRight, Accelerating, GameOver, IsShieldOn;
        int Score, Lives;
        int UfoCount, UfoBulletCount;
        int UfoExplosionX, UfoExplosionY, UfoExplosionRange;
        int AsteroidScore, ShieldRemaining;
        Random RandomNumberGenerator;
        double AsteroidSpeed;
        Bitmap GameBitmap;

        private Timer? FrameTimer;

        public Asteroids()
        {
            BlackBrush = new SolidBrush(Color.FromArgb(255, 200, 200, 255));
            BulletBrush = new SolidBrush(Color.White);
            TextBrush = new SolidBrush(Color.White);
            UfoBrush = new SolidBrush(Color.LightGreen);
            UfoPen = new Pen(UfoBrush, 3);
            ShieldPen = new Pen(BulletBrush, 2);

            TextFont = new Font("Arial", 16);

            RandomNumberGenerator = new Random();

            PlayerPlane = new Plane();
            ActiveAsteroids = new List<Asteroid>();
            PlayerBullets = new List<Bullet>();
            UfoBullets = new List<Bullet>();

            GameBitmap = new Bitmap(this.Width, this.Height);
            BitmapGc = Graphics.FromImage(GameBitmap);
            WindowsGc = Graphics.FromImage(GameBitmap);

            InitializeComponent();

            this.Text = "B2 Asteroids";
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.ClientSize = new System.Drawing.Size(492, 473);
        }


        protected void Asteroids_Resize(object sender, System.EventArgs e)
        {
            GameBitmap = new Bitmap(this.Width, this.Height);
            BitmapGc = Graphics.FromImage(GameBitmap);
        }

        protected void Asteroids_Paint(object sender, PaintEventArgs e)
        {
            DrawGameboard();
        }

        protected void Asteroids_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                TurningLeft = false;
            else if (e.KeyCode == Keys.Right)
                TurningRight = false;
            else if (e.KeyCode == Keys.Up)
                Accelerating = false;
            else if (e.KeyCode == Keys.Down)
            {
                IsShieldOn = false;
            }
        }

        protected void Asteroids_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                TurningLeft = true;
            else if (e.KeyCode == Keys.Right)
                TurningRight = true;
            else if (e.KeyCode == Keys.Up)
                Accelerating = true;
            else if (e.KeyCode == Keys.Down && ShieldRemaining >= 0)
                IsShieldOn = true;

            //Player shoots
            else if (e.KeyCode == Keys.Space && IsShieldOn == false && PlayerBullets.Count < MaxBullets)
            {
                Bullet newBullet = new Bullet(
                    new Vector2(
                        (float)(PlayerPlane.Position.X + 10.0 * Math.Sin(PlayerPlane.HeadingRadians)),
                        (float)(PlayerPlane.Position.Y - 10.0 * Math.Cos(PlayerPlane.HeadingRadians))),
                    (int)PlayerPlane.Heading,
                    BulletBrush);

                PlayerBullets.Add(newBullet);
            }

            //Restart game
            else if (e.KeyCode == Keys.F5)
            {
                StartGame();
            }

            //Quit game
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        protected void Asteroids_Layout(object sender, LayoutEventArgs e)
        {
            //Init variables
            WindowsGc = Graphics.FromHwnd(this.Handle);
            GameBitmap = new Bitmap(this.Width, this.Height);
            BitmapGc = Graphics.FromImage(GameBitmap);

            TurningLeft = false;
            TurningRight = false;
            Accelerating = false;

            StartGame();
        }

        protected void TimerTick(object? sender, System.EventArgs e)
        {
            UfoCount++;
            if (UfoCount == UfoDelay)
            {
                UfoCount = 0;
                GenerateUfo();
            }

            MoveAll();
            TestUfoShoot();
            UpdateShield();
            CheckHit();
            DrawGameboard();

            if (ActiveAsteroids.Count == 0)
                NextLevel();
        }

        public void UpdateShield()
        {
            if (IsShieldOn == true)
            {
                ShieldRemaining -= 1;
                if (ShieldRemaining <= 0)
                {
                    IsShieldOn = false;
                    ShieldRemaining = 0;
                }
            }
        }

        private void StartGame()
        {
           Score = 0;
            Lives = 3;
            ShieldRemaining = 100;
            UfoCount = 0;
            UfoBulletCount = 0;
            UfoExplosionRange = -1;
            GameOver = false;
            AsteroidSpeed = 1.5;
            AsteroidScore = 0;
            NextLevel();
            EnemyUfo = null;
            FrameTimer?.Enabled = true;
        }

        private void ResetPlayer()
        {
            bool collision;

            // Default is to start the plane in the center of the map
            Vector2 spawnPosition = new Vector2(
                this.Width / 2, 
                this.Height / 2
            );

            do
            {
                collision = false;

                // Try to find a safe space to spawn the plane
                PlayerPlane.SetLocation(spawnPosition, 0, 0, 0, this);

                foreach (var asteroid in ActiveAsteroids)
                {
                    foreach (var vertex in PlayerPlane.PositionVertices)
                    {
                        if (asteroid.CheckCollision(new Vector2(vertex.X, vertex.Y)))
                        {
                            collision = true;
                            break;
                        }
                    }
                    if (collision) break;
                }

                // On future attempts, pick a random position
                spawnPosition = new Vector2(
                    RandomNumberGenerator.Next(50, this.Width - 50),
                    RandomNumberGenerator.Next(50, this.Height - 50)
                ); 

            } while (collision);
        }

        private void NextLevel()
        {
            int i;

            AsteroidSpeed += 0.5;
            AsteroidScore++;

            ActiveAsteroids = new List<Asteroid>();
            PlayerBullets = new List<Bullet>();
            UfoBullets = new List<Bullet>();

            for (i = 0; i < StartingAsteroidNumber; i++)
            {
                ActiveAsteroids.Add(new Asteroid(AsteroidRadiusLarge, new Vector2(0, 0), RandomNumberGenerator.Next(0, 360),
                    AsteroidSpeed, this));
            }

            ResetPlayer();
        }

        private void TestUfoShoot()
        {
            if (EnemyUfo != null)
            {
                UfoBulletCount++;
                if (UfoBulletCount == UfoShootDelay)
                {
                    UfoBulletCount = 0;
                    UfoShoot();
                }
            }
        }

        private void UfoShoot()
        {
            if (EnemyUfo != null && UfoBullets.Count < MaxBullets)
            {
                Bullet newBullet = new Bullet(
                new Vector2(
                    (float)(EnemyUfo.Position.X),
                    (float)(EnemyUfo.Position.Y)),
                RandomNumberGenerator.Next(360),
                UfoBrush);

                UfoBullets.Add(newBullet);
            }
       }

        private void DrawGameboard()
        {
            DrawMap();
            DrawAsteroids();
            if (EnemyUfo != null) 
             {
                DrawUfo();
            }
            DrawExplosion();
            DrawBullets();
            DrawPlayer();
            DrawUfoBullets();
            DrawScore();

            //End game if  player out of lives
            if (GameOver)
            {
                FrameTimer?.Enabled = false;

                DrawOutlinedString("Game Over - Hit 'F5' to restart", 100, 100);
                DrawOutlinedString("Left Arrow: Rotate counter-clockwise", 100, 120);
                DrawOutlinedString("Right Arrow: Rotate clockwise", 100, 140);
                DrawOutlinedString("Up Arrow: Accelerate", 100, 160);
                DrawOutlinedString("Down Arrow: Activate Shield", 100, 180);
                DrawOutlinedString("Space Bar: Shoot", 100, 200);
                DrawOutlinedString("Escape: Exit Program", 100, 220);
            }

            //Copy bitmap to screen
            WindowsGc.DrawImage(GameBitmap, 0, 0);
        }

        private void DrawOutlinedString(string text, float x, float y)
        {
            using GraphicsPath textPath = new GraphicsPath();
            textPath.AddString(text, TextFont.FontFamily, (int)TextFont.Style, TextFont.Size,
                new PointF(x, y), StringFormat.GenericDefault);

            using Pen outlinePen = new Pen(Color.Black, 4);
            BitmapGc.DrawPath(outlinePen, textPath);
            BitmapGc.FillPath(TextBrush, textPath);
        }

        private void GenerateUfo()
        {
            EnemyUfo = new Ufo();

            int dir = RandomNumberGenerator.Next(4);
            int head = RandomNumberGenerator.Next(80) - 39;
           
            if (dir == 0)
            {
                EnemyUfo.SetLocation(new Vector2(0, RandomNumberGenerator.Next(this.Height) / 2 + this.Height / 4), 90 + head, this);
            }
            else if (dir == 1)
            {
                EnemyUfo.SetLocation(new Vector2(this.Width, RandomNumberGenerator.Next(this.Height) / 2 + this.Height / 4), 270 + head, this);
            }
            else if (dir == 2)
            {
                EnemyUfo.SetLocation(new Vector2(this.Width / 2 + this.Height / 4, 0), 180 + head, this);
            }
            else if (dir == 3)
            {
                if (head < 0) 
                {
                   head += 360;
                }
                EnemyUfo.SetLocation(new Vector2(this.Width / 2 + this.Height / 4, this.Height), head, this);
            }
        }

        private void DrawUfo()
        {
            if (EnemyUfo != null) 
            {
                EnemyUfo.Draw(BitmapGc);
            }
        }

        private void DrawExplosion()
        {
            if (UfoExplosionRange > 0)
            {
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Orange))) 
                {
                    BitmapGc.FillEllipse(shadowBrush,
                                UfoExplosionX - UfoExplosionRange + 4,
                                UfoExplosionY - UfoExplosionRange + 4,
                                2 * UfoExplosionRange, 2 * UfoExplosionRange);
                }

                using (Pen outlinePen = new Pen(Color.Black, 3))
                {
                    BitmapGc.DrawEllipse(outlinePen,
                                UfoExplosionX - UfoExplosionRange,
                                UfoExplosionY - UfoExplosionRange,
                                2 * UfoExplosionRange, 2 * UfoExplosionRange);
                }

                using (Pen explosionPen = new Pen(Color.Yellow, 1))
                {
                    BitmapGc.DrawEllipse(explosionPen,
                                UfoExplosionX - UfoExplosionRange,
                                UfoExplosionY - UfoExplosionRange,
                                2 * UfoExplosionRange, 2 * UfoExplosionRange);
                }
                              
                UfoExplosionRange += 10;
                if (UfoExplosionRange >= 100)
                    UfoExplosionRange = -1;
            }
        }

        private void DrawScore()
        {
            String scoreStr = "Score: " + Score;
            Point p = new()
            {
                X = 10,
                Y = 10
            };
            DrawOutlinedString(scoreStr, p.X, p.Y);

            scoreStr = "Shield: " + ShieldRemaining + "%";
            p = new Point
            {
                X = 10,
                Y = 30
            };
            DrawOutlinedString(scoreStr, p.X, p.Y);

            scoreStr = "Lives: " + Lives;
            p = new Point
            {
                X = this.Width - 100,
                Y = 10
            };
            DrawOutlinedString(scoreStr, p.X, p.Y);
        }

        private void DrawAsteroids()
        {
            foreach (var asteroid in ActiveAsteroids)
            {
                asteroid.Draw(BitmapGc);
            }
        }

        private void DrawBullets()
        {
            foreach (var bullet in PlayerBullets)
            {
                bullet.Draw(BitmapGc);
            }
        }

        private void DrawUfoBullets()
        {
            foreach (var bullet in UfoBullets)
            {
                bullet.Draw(BitmapGc);
            }   
        }

        private void DrawMap()
        {
            BitmapGc.FillRectangle(BlackBrush, 0, 0, this.Width, this.Height);
        }

        private void DrawPlayer()
        {
            // Draw plane
            PlayerPlane.Draw(BitmapGc, Accelerating);

            // Draw shield
            if (IsShieldOn == true)
            {
                // Draw a multi-layered glowing shield effect
                for (int i = 0; i < 3; i++)
                {
                    int alpha = 255 - 60 * i; // Decreasing opacity for inner layers
                    using (Pen glowPen = new Pen(Color.FromArgb(alpha, Color.White), 2))
                    {
                        float shieldRadius = 20f; 
                        int radius = (int) shieldRadius - 2*i;
                        BitmapGc.DrawEllipse(glowPen, 
                            PlayerPlane.Position.X - radius, 
                            PlayerPlane.Position.Y - radius, 
                            radius * 2, 
                            radius * 2);
                    }
                }
            }
        }

        private void DecreaseLives()
        {
            Lives--;
            if (Lives < 0)
            {
                Lives = 0;
            }
            if (Lives == 0)
            {
                GameOver = true;
            }
            else
            {
                ShieldRemaining = 100;
                ResetPlayer();
            }
        }

        private void MoveAll()
        {
            int newHeading = PlayerPlane.Heading;
            double speedX = PlayerPlane.SpeedX;
            double speedY = PlayerPlane.SpeedY;

            //Player turn left
            if (TurningLeft)
            {
                newHeading -= 7;
                if (newHeading < 0) {
                    newHeading += 360;
                }
            }

            //Player turn right
            if (TurningRight)
            {
                newHeading += 7;
                if (newHeading >= 360)
                    newHeading -= 360;
            }

            //Player accelerate
            if (Accelerating)
            {
                speedX += 0.2 * Math.Sin(PlayerPlane.HeadingRadians);
                speedY -= 0.2 * Math.Cos(PlayerPlane.HeadingRadians);

                double newSpeed = Math.Sqrt(speedX * speedX + speedY * speedY);
                if (newSpeed > 5)
                {
                    double scaleFactor;
                    scaleFactor = 5.0 / newSpeed;
                    speedX *= scaleFactor;
                    speedY *= scaleFactor;
                }
            }

            // Move player
            PlayerPlane.SetLocation(PlayerPlane.Position, newHeading, speedX, speedY, this);

            //Move bullets
            int index = 0;
            while (index < PlayerBullets.Count)
            {
                Bullet bullet = PlayerBullets[index];
                bool exitedMap = bullet.MoveBullet(this);
                if (exitedMap)
                {
                    PlayerBullets.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }           

            // Move Asteroids
            foreach (Asteroid asteroid in ActiveAsteroids)
            {
                asteroid.MoveAsteroid(this);
            }

            //Move UFO  bullets
            index = 0;
            while (index < UfoBullets.Count)
            {
                Bullet bullet = UfoBullets[index];
                bool exitedMap = bullet.MoveBullet(this);
                if (exitedMap)
                {
                    UfoBullets.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }    

            //Move ufo
            if (EnemyUfo != null)
            {
                bool exitedMap = EnemyUfo.MoveUfo(this);
                if (exitedMap)
                {
                    EnemyUfo = null;
                }
            }

        }

        private void CheckHit()
        {
            int i, j;
            double dx, dy, dist;
            bool bFound;
            Region r, r2;
            Rectangle rr;
            GraphicsPath gp;

            //Check if player shot asteroid
            i = 0;
            while (i < PlayerBullets.Count)
            {
                Bullet bullet = PlayerBullets[i];
                bFound = false;

                //Check if an asteroid shot
                for (j = 0; j < ActiveAsteroids.Count && bFound == false; j++)
                {
                    Asteroid asteroid = ActiveAsteroids[j];
                    if (asteroid.CheckCollision(bullet.Position))
                    {
                        PlayerBullets.Remove(bullet);
                        ActiveAsteroids.Remove(asteroid);
                        bFound = true;

                        // Split asteroid if it is large
                        if (asteroid.Radius == (int)AsteroidRadiusLarge)
                        {
                            int newHeading = asteroid.Heading - 90;
                            if (newHeading < 0) 
                            {   
                                newHeading += 360;
                            }
                            ActiveAsteroids.Add(new Asteroid(AsteroidRadiusMedium, asteroid.Position, newHeading,
                                AsteroidSpeed, this));
                            newHeading = asteroid.Heading + 90;
                            if (newHeading > 360) 
                            {   
                                newHeading -= 360;
                            }
                            ActiveAsteroids.Add(new Asteroid(AsteroidRadiusMedium, asteroid.Position, newHeading,
                                AsteroidSpeed, this));                                   
                            Score += AsteroidScore;
                        }

                        // Split asteroid if it is medium
                        else if (asteroid.Radius== (int)AsteroidRadiusMedium)
                        {
                            int newHeading = asteroid.Heading - 90;
                            if (newHeading < 0) 
                            {   
                                newHeading += 360;
                            }
                            ActiveAsteroids.Add(new Asteroid(AsteroidRadiusSmall, asteroid.Position, newHeading,
                                AsteroidSpeed, this));
                            newHeading = asteroid.Heading + 90;
                            if (newHeading > 360) 
                            {   
                                newHeading -= 360;
                            }
                            ActiveAsteroids.Add(new Asteroid(AsteroidRadiusSmall, asteroid.Position, newHeading,
                                AsteroidSpeed, this));                                   
                            Score += (AsteroidScore + 1);
                        }

                        // Just remove asteroid if it is small and give points
                        else
                        {
                            Score += (AsteroidScore + 2);
                        }
                    } 
                }

                if (!bFound)
                {
                    i++;
                }
            }

            //Check if ufo and player collided
            if (EnemyUfo != null && IsShieldOn == false)
            {
                gp = new GraphicsPath();
                gp.AddPolygon(EnemyUfo.PositionVertices);
                r = new Region(gp);
                gp = new GraphicsPath();
                gp.AddPolygon(PlayerPlane.PositionVertices);
                r2 = new Region(gp);
                r.Intersect(r2);
                if (!r.IsEmpty(BitmapGc))
                {
                    DecreaseLives();
                }
            }

            //Check if player ran into ufo explosion
            if (UfoExplosionRange > 0 && IsShieldOn == false)
            {
                bool pointBelow = false;
                bool pointAbove = false;
                for (i = 0; i < PlayerPlane.VertexCount; i++)
                {
                    dx = UfoExplosionX - PlayerPlane.PositionVertices[i].X;
                    dy = UfoExplosionY - PlayerPlane.PositionVertices[i].Y;
                    dist = dx * dx + dy * dy;
                    if (dist <= UfoExplosionRange * UfoExplosionRange)
                        pointBelow = true;
                    else
                        pointAbove = true;
                }

                if (pointAbove && pointBelow)
                {
                    DecreaseLives();
                }
            }

            //Check if player ran into asteroid
            for (i = 0; i < PlayerPlane.VertexCount && IsShieldOn == false; i++)
            {
                for (j = 0; j < ActiveAsteroids.Count; j++)
                {
                    Asteroid asteroid = ActiveAsteroids[j];
                    if (asteroid.CheckCollision(new Vector2(PlayerPlane.PositionVertices[i].X, PlayerPlane.PositionVertices[i].Y)))
                    {
                        DecreaseLives();
                    }
                }
            }

            //Check if ufo ran into asteroid
            if (EnemyUfo != null)
            {
                for (i = 0; EnemyUfo != null && i < EnemyUfo.VertexCount; i++)
                {
                    for (j = 0; j < ActiveAsteroids.Count && EnemyUfo != null; j++)
                    {
                        Asteroid asteroid = ActiveAsteroids[j];
                        if (asteroid.CheckCollision(new Vector2(EnemyUfo.PositionVertices[i].X, EnemyUfo.PositionVertices[i].Y)))
                        {
                            UfoExplosionRange = 10;
                            UfoExplosionX = (int) EnemyUfo.Position.X;
                            UfoExplosionY = (int) EnemyUfo.Position.Y;
                            EnemyUfo = null;
                        }
                    }
                }
            }

            //Check if ufo shot an asteroid
            i = 0;
            while (i < UfoBullets.Count)
            {
                Bullet bullet = UfoBullets[i];
                bool removedBullet = false;

                for (j = 0; j < ActiveAsteroids.Count && !removedBullet; j++)
                {
                    Asteroid asteroid = ActiveAsteroids[j];
                    if (asteroid.CheckCollision(bullet.Position))
                    {
                        UfoBullets.Remove(bullet);
                        removedBullet = true;
                    }
                }

                if (!removedBullet)
                {
                    i++;
                }
            }

            //check if ufo shot player
            gp = new GraphicsPath();
            gp.AddPolygon(PlayerPlane.PositionVertices);
            for (i = 0; i < UfoBullets.Count && IsShieldOn == false; i++)
            {
                Bullet bullet = UfoBullets[i];
                rr = new Rectangle((int)bullet.Position.X - 1, (int)bullet.Position.Y - 1, 3, 3);
                r2 = new Region(rr);
                r = new Region(gp);
                r.Intersect(r2);
                if (!r.IsEmpty(BitmapGc))
                {
                    DecreaseLives();
                    break;
                }
            }

            //check if player shot ufo
            bool bHit = false;
            if (EnemyUfo != null)
            {
                gp = new GraphicsPath();
                gp.AddPolygon(EnemyUfo.PositionVertices);

                i = 0;
                while (i < PlayerBullets.Count && bHit == false && EnemyUfo != null)
                {
                    Bullet bullet = PlayerBullets[i];

                    r = new Region(gp);
                    rr = new Rectangle((int)bullet.Position.X - 1, (int)bullet.Position.Y - 1, 3, 3);
                    r2 = new Region(rr);
                    r.Intersect(r2);
                    if (!r.IsEmpty(BitmapGc))
                    {
                        UfoExplosionRange = 10;
                        UfoExplosionX = (int) EnemyUfo.Position.X;
                        UfoExplosionY = (int) EnemyUfo.Position.Y;
                        EnemyUfo = null;
                        PlayerBullets.RemoveAt(i);
                        Score += 25;
                        bHit = true;
                    } 
                    else
                    {
                        i++;
                    }
                }
            }

        }
    }
}