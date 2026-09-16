using System;
using D3X = SlimDX;

namespace DirectXAsteroids
{
  internal class Asteroid: DisplayObject
  {
    /// <summary>
    /// Custom Rotation
    /// </summary>
    internal D3X.Vector3 Rotation;

    /// <summary>
    /// Size of asteroid
    /// </summary>
    internal int Size;

    /// <summary>
    /// Heading of asteroid (0-360)
    /// </summary>
    internal int AngleHeading;
  }
}
