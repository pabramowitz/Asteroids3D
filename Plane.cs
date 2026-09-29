// Inside your Asteroid class/struct
using System.Numerics;
using System.Drawing;

namespace Asteroids
{
    public class Plane: Polygon
    {
        private SolidBrush PlaneBrush;
        private SolidBrush ThrustBrush;

        private const int PlanePoints = 12;
        private const int ThrustPoints = 3;

        private PointF[]  ThrustShapeVertices;
        private PointF[]  ThrustPositionVertices;

        public double SpeedX;
        public double SpeedY;

        // Call this when spawning the plane
        public Plane(): base(PlanePoints)
        {
            PlaneBrush = new SolidBrush(Color.Gray);
            ThrustBrush = new SolidBrush(Color.Orange);

            ShapeVertices = new PointF[VertexCount];
            PositionVertices = new PointF[VertexCount];
            
            ThrustShapeVertices = new PointF[ThrustPoints];
            ThrustPositionVertices = new PointF[ThrustPoints];
                    
            ShapeVertices[0] = new Point(0, 4);
            ShapeVertices[1] = new Point(2, 2);
            ShapeVertices[2] = new Point(3, 4);
            ShapeVertices[3] = new Point(5, 2);
            ShapeVertices[4] = new Point(10, 5);
            ShapeVertices[5] = new Point(13, 2);
            ShapeVertices[6] = new Point(0, -12);
            ShapeVertices[7] = new Point(-13, 2);
            ShapeVertices[8] = new Point(-10, 5);
            ShapeVertices[9] = new Point(-5, 2);
            ShapeVertices[10] = new Point(-3, 4);
            ShapeVertices[11] = new Point(-2, 2);

            ThrustShapeVertices[0] = new Point(0, 25);
            ThrustShapeVertices[1] = new Point(5, 10);
            ThrustShapeVertices[2] = new Point(-5, 10);
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

        public void Draw(Graphics gc)
        {
            // Draw hard-edged shadow/highlight for cel-shading effect
            PointF[] shadowVertices = new PointF[PositionVertices.Length];
            for (int i = 0; i < PositionVertices.Length; i++)
            {
                shadowVertices[i] = new PointF(PositionVertices[i].X + 4, PositionVertices[i].Y + 4);
            }

            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                gc.FillPolygon(shadowBrush, shadowVertices);
            }

            // Draw the main plane body
            gc.FillPolygon(this.PlaneBrush, this.PositionVertices);

            // Draw a thick, cartoon-style black outline
            using (Pen outlinePen = new Pen(Color.Black, 3))
            {
                gc.DrawPolygon(outlinePen, this.PositionVertices);
            }
        }

        public void Draw(Graphics gc, bool isAccelerating)
        {
            // Draw plane
            Draw(gc);

            // Draw flame if accelerating
            if (isAccelerating)
            {
                // Draw thrust if accelerating (assuming you draw thrust when moving)
                PointF[] thrustShadowVertices = new PointF[ThrustPositionVertices.Length];
                for (int i = 0; i < ThrustPositionVertices.Length; i++)
                {
                    thrustShadowVertices[i] = new PointF(ThrustPositionVertices[i].X + 4, ThrustPositionVertices[i].Y + 4);
                }

                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    gc.FillPolygon(shadowBrush, thrustShadowVertices);
                }

                gc.FillPolygon(this.ThrustBrush, this.ThrustPositionVertices);

                using (Pen outlinePen = new Pen(Color.Black, 3))
                {
                    gc.DrawPolygon(outlinePen, this.ThrustPositionVertices);
                }
            }
        }
    }
}

