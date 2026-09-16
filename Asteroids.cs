namespace DirectXAsteroids
{
  /// <prologue>
  /// <file name="Asteroids.cs"/>
  /// <remarks>
  /// This class starts Asteroids
  /// </remarks>
  /// </prologue>  
  /// <summary>
  /// This displays Asteroids.
  /// </summary>
  /// <remarks>Asteroids screen uses DirectX.</remarks>
  public class Asteroids
  {
    #region Class Variables
    
    /// <summary>
    /// The Asteroids dialog
    /// </summary>
    private static AsteroidsForm m_asteroids;
  
    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor.  
    /// </summary>
    /// <remarks>A zero argument constructor is required for
    /// any class running in own thread.</remarks>
    internal Asteroids()
    {
    }

    #endregion

    /// <summary>
    /// Main method
    /// </summary>
     [STAThread]
    static void Main()
    {
      try
      {
        m_asteroids = new AsteroidsForm();
        System.Windows.Forms.Application.Run(m_asteroids);
      }
      catch (System.Exception ex)
      {
        //Suppress any error so that thread doesn't crash
        System.Windows.Forms.MessageBox.Show("Fatal Error: " + ex.Message);
      }
    }

  }
}
