namespace DirectXAsteroids
{
  internal class Bullet : DisplayObject
  {
    private int m_distanceTravelled;

    internal int DistanceTravelled
    {
      get
      {
        return m_distanceTravelled;
      }
      set
      {
        m_distanceTravelled = value;
      }
    }
  }
}
