using D3D = SlimDX.Direct3D9;

namespace DirectXAsteroids
{
  internal class ResourceSet
  {
    /// <summary>
    /// Surface materials for the display object
    /// </summary>
    internal D3D.Material[] DrawMaterials;

    /// <summary>
    /// Textures for display object
    /// </summary>
    internal D3D.Texture[] DrawTextures;

    /// <summary>
    /// Mesh used to store the user mesh
    /// </summary>
    internal D3D.Mesh DrawMesh;
  }
}
