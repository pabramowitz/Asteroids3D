// Inside your Asteroid class/struct
using System.Numerics;

namespace Asteroids
{
    public class Plane: Polygon
    {

        private const int PlanePoints = 12;
        private const int ThrustPoints = 3;

        private Vector2[]  ThrustShapeVertices;
        private Vector2[]  ThrustPositionVertices;

        public double SpeedX;
        public double SpeedY;

        // Call this when spawning the plane
        public Plane(): base(PlanePoints)
        {
            ShapeVertices = new Vector2[VertexCount];
            PositionVertices = new Vector2[VertexCount];
            
            ThrustShapeVertices = new Vector2[ThrustPoints];
            ThrustPositionVertices = new Vector2[ThrustPoints];
                    
            ShapeVertices[0] = new Vector2(0, 4);
            ShapeVertices[1] = new Vector2(2, 2);
            ShapeVertices[2] = new Vector2(3, 4);
            ShapeVertices[3] = new Vector2(5, 2);
            ShapeVertices[4] = new Vector2(10, 5);
            ShapeVertices[5] = new Vector2(13, 2);
            ShapeVertices[6] = new Vector2(0, -12);
            ShapeVertices[7] = new Vector2(-13, 2);
            ShapeVertices[8] = new Vector2(-10, 5);
            ShapeVertices[9] = new Vector2(-5, 2);
            ShapeVertices[10] = new Vector2(-3, 4);
            ShapeVertices[11] = new Vector2(-2, 2);

            ThrustShapeVertices[0] = new Vector2(0, 25);
            ThrustShapeVertices[1] = new Vector2(5, 10);
            ThrustShapeVertices[2] = new Vector2(-5, 10);
        }

        public void SetLocation(Vector2 position, int heading, double speedX, double speedY, int width, int height)
        {
            Position = position;
            Heading = heading;
            SpeedX = speedX;
            SpeedY = speedY;

            // Compute new position based on heading and speed
            Position.X += (float) speedX;
            Position.Y += (float) speedY;
            Position = ClipPosition(Position, width, height);

            // Compute drawing points
            for (int i = 0; i < PlanePoints; i++)
            {
                PositionVertices[i].X = (float)(Position.X + ShapeVertices[i].X * Math.Cos(HeadingRadians) - 
                        ShapeVertices[i].Y * Math.Sin(HeadingRadians));
                PositionVertices[i].Y = (float)(Position.Y + ShapeVertices[i].X * Math.Sin(HeadingRadians) + 
                        ShapeVertices[i].Y * Math.Cos(HeadingRadians));
            }

            // Calculate thrust points
            for (int i = 0; i < ThrustPoints; i++)
            {
                ThrustPositionVertices[i].X = (float)(Position.X + ThrustShapeVertices[i].X * Math.Cos(HeadingRadians) - 
                        ThrustShapeVertices[i].Y * Math.Sin(HeadingRadians));
                ThrustPositionVertices[i].Y = (float)(Position.Y + ThrustShapeVertices[i].X * Math.Sin(HeadingRadians) + 
                        ThrustShapeVertices[i].Y * Math.Cos(HeadingRadians));
            }
        }

        // MonoGame will handle drawing based on PositionVertices and ThrustPositionVertices.
        public void UpdateShape()
        {
            // No-op placeholder for future per-frame shape updates if needed.
        }
    }
}

