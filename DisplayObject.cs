using D3X = SlimDX;
using D3D = SlimDX.Direct3D9;
using System.Runtime.InteropServices;

namespace DirectXAsteroids
{
  internal class DisplayObject
  {
    #region Private Members

    /// <summary>
    /// Location of object
    /// </summary>
    private D3X.Vector3 m_location;

    #endregion

    /// <summary>
    /// Velocity vector
    /// </summary>
    internal D3X.Vector3 Velocity;

    /// <summary>
    /// Heading Vector
    /// </summary>
    internal D3X.Matrix Heading;

    internal ResourceSet Resources;

    internal float BoundingRadius;

    internal float ScaleFactor;

    internal bool HasBeenClipped;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public D3X.Vector3 Position;
        public D3X.Vector4 Color;
    }

    /// <summary>
    /// When location is set, objects are clipped to screen edges and 
    /// "wrapped around" edges.
    /// </summary>
    internal D3X.Vector3 Location
    {
      get
      {
        return m_location;
      }
      set
      {
        //When location is set, do clipping
        m_location = value;
        ClipLocation();
      }
    }

    internal void ComputeBoundingRadius(float scaleFactor)
    {
      //Check if player and asteroids collide
      using (D3D.VertexBuffer vb = Resources.DrawMesh.VertexBuffer)
      {
        //Computer asteroid center and radius
        D3X.Vector3 boundingCenter;
        D3X.DataStream objectData = vb.Lock(0, 0, D3D.LockFlags.None);

        int vertexCount = (int)(objectData.Length / Marshal.SizeOf(typeof(D3X.Vector3)));
        D3X.Vector3[] vertices = new D3X.Vector3[vertexCount];
        objectData.ReadRange(vertices, 0, vertexCount);

        BoundingRadius = D3X.BoundingSphere.FromPoints(vertices).Radius * scaleFactor;
        ScaleFactor = scaleFactor;

        vb.Unlock();
      }
    }

    /// <summary>
    /// Clips object location to screen edges
    /// </summary>
    private void ClipLocation()
    {
      if (m_location.Y < -6)
      {
        m_location.Y = 6;
        HasBeenClipped = true;
      }
      else if (m_location.Y > 6)
      {
        m_location.Y = -6;
        HasBeenClipped = true;
      }
      if (m_location.X < -6)
      {
        m_location.X = 6f;
        HasBeenClipped = true;
      }
      else if (m_location.X > 6)
      {
        m_location.X = -6f;
        HasBeenClipped = true;
      }
    }
  }
}
