// Inside your Asteroid class/struct
using System.Numerics;
using System.Drawing;

namespace Asteroids
{

    public abstract class Polygon
    {
        public int VertexCount;
        public Vector2 Position;
        public Vector2 Velocity;
        public int Heading;
        public double Speed;

        public PointF[] ShapeVertices; // Shape centered at (0,0)

        public PointF[] PositionVertices; // Shape in world space

        public Polygon(int vertexCount)
        {
            VertexCount = vertexCount;
            ShapeVertices = new PointF[vertexCount];
            PositionVertices = new PointF[vertexCount];
        }

        public double  HeadingRadians
        {
            get
            {
                return Heading * (Math.PI / 180.0);
            }
        }

        public Vector2 ClipPosition(Vector2 position, int width, int height)
        {
            if (position.X < 0)
                position.X += width;
            else if (position.X > width)
                position.X -= width;
            if (position.Y < 0)
                position.Y += height;
            else if (position.Y > height)
                position.Y -= height;

            return position;
        }

        public bool CheckCollision(Vector2 localPoint)
        {
            bool inside = false;
            int j = PositionVertices.Length - 1;

            for (int i = 0; i < PositionVertices.Length; i++)
            {
                if ((PositionVertices[i].Y > localPoint.Y) != (PositionVertices[j].Y > localPoint.Y) &&
                    (localPoint.X < (PositionVertices[j].X - PositionVertices[i].X) * (localPoint.Y - PositionVertices[i].Y) / (PositionVertices[j].Y - PositionVertices[i].Y) + PositionVertices[i].X))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }
    }
}
