// Inside your Asteroid class/struct
using System.Numerics;
using System.Drawing;

namespace Asteroids
{
    public class Bullet: Polygon
    {
        private SolidBrush BulletBrush;

        private const double BulletSpeed = 7.0;
        private const double MaxDistance = 300;
        public int Distance { get; set; }


        public Bullet(Vector2 position, int heading, SolidBrush brush) : base(0)
        {
            Position = position;
            Heading = heading;
            Distance = 0;
            BulletBrush = brush;
        }

        public bool MoveBullet(int width, int height)
        {
            Position.X += (float)(BulletSpeed * Math.Sin(HeadingRadians));
            Position.Y -= (float)(BulletSpeed * Math.Cos(HeadingRadians));
            Position = ClipPosition(Position, width, height);

            // Check if maximum distance flown
            Distance += (int)BulletSpeed;
            return Distance > MaxDistance;
        }

        public void Draw(Graphics gc)
        {
            // Draw a small drop shadow for the bullet
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                gc.FillRectangle(shadowBrush, this.Position.X - 1 + 2, this.Position.Y - 1 + 2, 3, 3);
            }

            // Draw the main bullet body
            gc.FillRectangle(this.BulletBrush, this.Position.X - 1, this.Position.Y - 1, 3, 3);

            // Draw a black outline/border around the bullet
            using (Pen outlinePen = new Pen(Color.Black, 1))
            {
                gc.DrawRectangle(outlinePen, this.Position.X - 1, this.Position.Y - 1, 3, 3);
            }
        }
    }
}
