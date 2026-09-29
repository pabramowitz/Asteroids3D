// Inside your Asteroid class/struct
using System.Numerics;
using System.Drawing;

namespace Asteroids
{
    public class Ufo: Polygon
    {
        private const int UfoPoints = 8;
        private const int UfoSpeed = 4;
        SolidBrush UfoBrush;

        // Call this when spawning the UFO
        public Ufo(): base(UfoPoints)
        {
            UfoBrush = new SolidBrush(Color.LightGreen);

            ShapeVertices = new PointF[VertexCount];
            PositionVertices = new PointF[VertexCount];

            ShapeVertices[0] = new Point(14, 0);
            ShapeVertices[1] = new Point(6, 0);
            ShapeVertices[2] = new Point(5, -7);
            ShapeVertices[3] = new Point(-4, -7);
            ShapeVertices[4] = new Point(-6, 0);
            ShapeVertices[5] = new Point(-14, 0);
            ShapeVertices[6] = new Point(-11, 8);
            ShapeVertices[7] = new Point(11, 8);
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

