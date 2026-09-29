// Inside your Asteroid class/struct
using System.Numerics;

namespace Asteroids
{
    public class Ufo: Polygon
    {
        private const int UfoPoints = 8;
        private const int UfoSpeed = 4;

        // Call this when spawning the UFO
        public Ufo(): base(UfoPoints)
        {
            ShapeVertices = new Vector2[VertexCount];
            PositionVertices = new Vector2[VertexCount];

            ShapeVertices[0] = new Vector2(14, 0);
            ShapeVertices[1] = new Vector2(6, 0);
            ShapeVertices[2] = new Vector2(5, -7);
            ShapeVertices[3] = new Vector2(-4, -7);
            ShapeVertices[4] = new Vector2(-6, 0);
            ShapeVertices[5] = new Vector2(-14, 0);
            ShapeVertices[6] = new Vector2(-11, 8);
            ShapeVertices[7] = new Vector2(11, 8);
        }

        public void SetLocation(Vector2 position, int heading, int width, int height)
        {
            Position = position;
            Heading = heading;

            // Compute drawing points
            for (int i = 0; i < UfoPoints; i++)
            {
                PositionVertices[i].X = (float)(Position.X + ShapeVertices[i].X);
                PositionVertices[i].Y = (float)(Position.Y + ShapeVertices[i].Y);
            }
        }
        public bool MoveUfo(int width, int height)
        {
            Position.X += (float)(UfoSpeed * Math.Sin(HeadingRadians));
            Position.Y -= (float)(UfoSpeed * Math.Cos(HeadingRadians));

            // Compute drawing points
            for (int i = 0; i < UfoPoints; i++)
            {
                PositionVertices[i].X = (float)(Position.X + ShapeVertices[i].X);
                PositionVertices[i].Y = (float)(Position.Y + ShapeVertices[i].Y);
            }

            if (Position.X < 0 || Position.X > width ||
                    Position.Y < 0 || Position.Y > height)
            {
                return true;
            }
            
            return false;
        }

        public void UpdateShape()
        {
            // No-op for now: drawing will be handled by MonoGame in AsteroidsGame.
        }
    }
}

