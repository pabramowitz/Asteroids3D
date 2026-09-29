// Inside your Asteroid class/struct
using System.Numerics;

namespace Asteroids
{
    public class Asteroid: Polygon
    {
        private const int AsteroidPoints = 8;

        public static Random RandomNumberGenerator = new Random();

        public double Radius; // Used for broad-phase collision / bounding circle
        public Microsoft.Xna.Framework.Color Color { get; }

        private static Microsoft.Xna.Framework.Color MakeRandomColor()
        {
            return new Microsoft.Xna.Framework.Color(
                RandomNumberGenerator.Next(80, 256),
                RandomNumberGenerator.Next(80, 256),
                RandomNumberGenerator.Next(80, 256));
        }

        // Call this when spawning the asteroid
        public Asteroid(double radius, Vector2 position, int heading, double speed, int width, int height): base(AsteroidPoints)
        {
            // Define initial shape
            ShapeVertices = new Vector2[VertexCount];
            PositionVertices = new Vector2[VertexCount];
            float angleStep = MathF.Tau / VertexCount;
            Radius = radius;
            Heading = heading;
            Speed = speed;
            Position = position;

            Color = MakeRandomColor();

            for (int i = 0; i < VertexCount; i++)
            {
                double angle = i * angleStep;
                // Vary the radius slightly for an irregular jagged look
                double variance = 0.7f + 0.6f * RandomNumberGenerator.NextDouble();
                double r = Radius * variance;

                ShapeVertices[i] = new Vector2((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
            }

        }
        public void UpdateShape()
        {
            // No-op for now: drawing will be handled by MonoGame in AsteroidsGame.
        }

        public void MoveAsteroid(int width, int height)
        {
            // Compute new position
            Position.X += (float)(Speed * Math.Sin(Heading));
            Position.Y += (float)(Speed * Math.Cos(Heading));

            Position = ClipPosition(Position, width, height);

            for (int i = 0; i < VertexCount; i++)
            {
                PositionVertices[i].X = ShapeVertices[i].X + Position.X;
                PositionVertices[i].Y = ShapeVertices[i].Y + Position.Y;
            }
        }
    }
}

