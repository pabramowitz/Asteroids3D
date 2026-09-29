// Inside your Asteroid class/struct
using System.Numerics;

namespace Asteroids
{
    public class Bullet: Polygon
    {

        private const double BulletSpeed = 7.0;
        private const double MaxDistance = 300;
        public int Distance { get; set; }


        public Bullet(Vector2 position, int heading) : base(0)
        {
            Position = position;
            Heading = heading;
            Distance = 0;
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

        // MonoGame will handle drawing bullets as simple sprites using Position.
        public void UpdateShape()
        {
            // No-op placeholder.
        }
    }
}
