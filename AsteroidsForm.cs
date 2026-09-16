using COLG = System.Collections.Generic;
using DRW = System.Drawing;
using GBZ = System.Globalization;
using SWF = System.Windows.Forms;

using D3X = SlimDX;
using D3D = SlimDX.Direct3D9;

namespace DirectXAsteroids
{
  /// <prologue>
  /// <file name="AsteroidsForm.cs"/>
  /// <remarks>
  /// This class is the dialog for the asteroids window
  /// </remarks>
  /// <classification level="Unclassified"/>
  ///
  /// <author name="P. Abramowitz� date=�7/10/07"/>
  ///
  /// </prologue>  
  /// <summary>
  /// This is the Asteroids main window
  /// </summary>
  /// <remarks>Asteroids uses DirectX.</remarks>
  public partial class AsteroidsForm : SWF.Form
  {
    #region constants

    /// <summary>
    /// Maximum velocity player can reach
    /// </summary>
    private const float MaxSpeed = 0.4f;

    /// <summary>
    /// Speed of asteroids
    /// </summary>
    private const float AsteroidSpeed = 0.04f;

    /// <summary>
    /// Number of intervals shield stays up before automatically closing
    /// </summary>
    private const int ShieldMax = 30;

    /// <summary>
    /// Delay, in time intervals, before UFO shows up
    /// </summary>
    private const int UfoDelay = 200;

    /// <summary>
    /// Number of pixels ufo moves every interval
    /// </summary>
    private const float UfoSpeed = 0.1f;

    /// <summary>
    /// Delay between ufo shots
    /// </summary>
    private const int UfoShootDelay = 10;

    #endregion

    #region Class Variables

    /// <summary>
    /// Timer that controls refresh rate
    /// </summary>
    private SWF.Timer m_renderTimer;

    /// <summary>
    /// Font used for most of the display text
    /// </summary>
    private DRW.Font m_textFont;
  
    /// <summary>
    /// Matrix that represents the view window
    /// </summary>
    private D3X.Matrix m_View;

    /// <summary>
    /// Matrix that represents the view projection
    /// </summary>
    private D3X.Matrix m_Projection;

    /// <summary>
    /// The DirectX device to which all drawing is done
    /// </summary>
    private D3D.Device m_device;

    /// <summary>
    /// Flag set to true if a hardware device was ever created
    /// </summary>
    private bool m_hardwareSupportExists;

    /// <summary>
    /// DirectX font for the simple text on the display
    /// </summary>
    private D3D.Font m_drawFont;

    /// <summary>
    /// Parameters used to setup the DirectX device
    /// </summary>
    private D3D.PresentParameters m_presentParams;

    /// <summary>
    /// Player object
    /// </summary>
    private Plane m_player;

    /// <summary>
    /// Player shield
    /// </summary>
    private DisplayObject m_shield;

    /// <summary>
    /// Grid line used for perspective view
    /// </summary>
    private DisplayObject m_gridLine;

    /// <summary>
    /// UFO
    /// </summary>
    private DisplayObject m_ufo;

    /// <summary>
    /// Random number generator
    /// </summary>
    private Random m_random;

    /// <summary>
    /// Set of asteroids
    /// </summary>
    private COLG.Dictionary<Asteroid, Asteroid> m_asteroids;

    /// <summary>
    /// Set of player bullets
    /// </summary>
    private COLG.Dictionary<Bullet, Bullet> m_playerBullets;

    /// <summary>
    /// Set of ufo bullets
    /// </summary>
    private COLG.Dictionary<Bullet, Bullet> m_ufoBullets;

    /// <summary>
    /// Set  of resources for an asteroid
    /// </summary>
    private ResourceSet m_asteroidResources;

    /// <summary>
    /// Set of resources for player's plane
    /// </summary>
    private ResourceSet m_planeResources;

    /// <summary>
    /// Set of resources for player's shield
    /// </summary>
    private ResourceSet m_shieldResources;

    /// <summary>
    /// Set of resources for ufo
    /// </summary>
    private ResourceSet m_ufoResources;

    /// <summary>
    /// User score
    /// </summary>
    private int m_score;

    /// <summary>
    /// Current asteroid level
    /// </summary>
    private int m_level;

    /// <summary>
    /// Counts interval between ufos
    /// </summary>
    private int m_ufoCount;

    /// <summary>
    /// Counts interval between ufo shots
    /// </summary>
    private int m_ufoShootCount;

    /// <summary>
    /// If true, draw in first person
    /// </summary>
    private bool m_firstPerson;

    #endregion

    #region Constructor

    /// <summary>
    /// Default Constructor
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility",
     "CA1601", Justification="Asteroids requires a high refresh rate.")]
    public AsteroidsForm()
    {
      //Initialize DirectX variables
      m_presentParams = new D3D.PresentParameters();

      //Start timer.  This is used to control the interval between repaints
      //of the game.
      m_renderTimer = new SWF.Timer();
      m_renderTimer.Interval = 50;
      m_renderTimer.Tick += new EventHandler(RenderTimerTick);

      //Initialize data
      InitializeComponent();

      //Set location to center of window after initialize component
      Left = (SWF.SystemInformation.PrimaryMonitorSize.Width - Width) / 2;
      Top = (SWF.SystemInformation.PrimaryMonitorSize.Height - Height) / 2;
    }

    #endregion

    #region Overriden methods

    /// <summary>
    /// Overrides the default Form painting
    /// </summary>
    /// <param name="e">Drawing event arguments</param>
    /// <remarks>Repaint should cause entire DirectX scene to be 
    /// re-rendered.</remarks>
    protected override void OnPaint(SWF.PaintEventArgs e)
    {
      // Render on painting
      Render(); 
    }

    #endregion



    #region Private Methods

    /// <summary>
    /// Event Handler that catches event fired by the timer
    /// </summary>
    /// <param name="sender">Argument that generated event</param>
    /// <param name="e">Event argument</param>
    /// <remarks>When timer fires, re-render scene</remarks>
    private void RenderTimerTick(object sender, EventArgs e)
    {
      Render();
    }
    
    /// <summary>
    /// This method creates the DirectX device that will be used for all
    /// drawing operations
    /// </summary>
    /// <remarks>Device needs to be created in two situtations. The first
    /// is when the dialog is first initialized.  The second is whenever the
    /// active device becomes invalid.  This can happen if user locks and then
    /// unlocks the screen.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
      "CA1031")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility",
     "CA1601", Justification = "Asteroids requires a high refresh rate.")]
    private void CreateDirectXDevice()
    {
      //Dispose existing device
      if (m_device != null)
      {
        m_device.Dispose();
      }

      // We don't want to run fullscreen
      m_presentParams.Windowed = true;

      // Discard the frames 
      m_presentParams.SwapEffect = D3D.SwapEffect.Discard;

      // Turn on a Depth stencil
      m_presentParams.EnableAutoDepthStencil = true;

      // And the stencil format
      m_presentParams.AutoDepthStencilFormat = D3D.Format.D16;

      //Setup flags
      D3D.CreateFlags flags = D3D.CreateFlags.SoftwareVertexProcessing;

      // 1. Initialize Direct3D
      using (D3D.Direct3D d3d = new D3D.Direct3D())
      {
        //Catch error in hardware rendering not suppored
        try
        {
            //Check if vertex shading supported in hardware
            D3D.Capabilities hardware = d3d.GetDeviceCaps(0,  D3D.DeviceType.Hardware);
            if (hardware.VertexShaderVersion > new Version(1, 1))
            {
              if ((hardware.DeviceCaps & D3D.DeviceCaps.HWTransformAndLight) != 0)
              {
                flags = D3D.CreateFlags.HardwareVertexProcessing;
              }

              //Use pure if availible
              if ((hardware.DeviceCaps & D3D.DeviceCaps.PureDevice) != 0)
              {
                flags |= D3D.CreateFlags.PureDevice;
              }

              //Create a device
              m_device = new D3D.Device(d3d, 0, D3D.DeviceType.Hardware, this.Handle,
                flags, m_presentParams);
              m_hardwareSupportExists = true;
            }
        }
        catch (System.Exception)
        {
          //Software-only processing.  This will be slower than if hardware
          //is present.
          if (!m_hardwareSupportExists)
          {
            try
            {
              m_device = new D3D.Device(d3d, 0, D3D.DeviceType.Reference, this.Handle,
                flags, m_presentParams);

              //Slow down timer to allocate less time to game
              m_renderTimer.Interval = 1000;
            }
            catch (System.Exception)
            {
              //No direct X at all.  Stop timer.
              m_renderTimer.Stop();
            }
          }
          else
          {
            //Throw error because device not ready.  Most likely cause is that
            //the screen is locked.
            // 1. Get the HRESULT code for D3DERR_INVALIDCALL (-2005530516)
            int invalidCallHResult = D3D.ResultCode.InvalidCall.Code;

            throw new D3D.Direct3D9Exception("DirectX device not ready.  Most likely cause is that the screen is locked." );
          }
        }
      }
      

      if (m_device != null)
      {
        //Setup event handlers for device events.
        //  m_device. += new System.EventHandler(OnResetDevice);
        //  OnResetDevice(m_device, null);

        //Create device-dependent fonts and objects
        m_textFont = new DRW.Font("Arial", 10.0f,
          DRW.FontStyle.Regular);
        m_drawFont = new D3D.Font(m_device, m_textFont);
      }
    }

    /// <summary>
    /// Moves players plane.
    /// </summary>
    private void MovePlayer()
    {
      //Player turn left
      if (m_player.TurnLeft)
      {
        m_player.Heading *=
          D3X.Matrix.RotationAxis(new D3X.Vector3(0, 0, 1), 0.1f);
      }

      //Player turn right
      else if (m_player.TurnRight)
      {
        m_player.Heading *=
          D3X.Matrix.RotationAxis(new D3X.Vector3(0, 0, 1), -0.1f);
      }

      //Player accelerate
      if (m_player.MoveForward == true)
      {
        //Update velocity vector pased on current heading
        m_player.Velocity += new D3X.Vector3(
          0.01f * m_player.Heading.M12,
          -0.01f * m_player.Heading.M11, 
          0.01f * m_player.Heading.M13);

        //Cap velocity
        float speed = m_player.Velocity.Length();
        if (speed > MaxSpeed)
        {
          m_player.Velocity *= (MaxSpeed / speed);
        }
      }

      //Move plane
      D3X.Vector4 oldLoc = new D3X.Vector4(m_player.Location.X,
        m_player.Location.Y, m_player.Location.Z, 1f);
      oldLoc = D3X.Vector4.Transform(oldLoc, D3X.Matrix.Translation(m_player.Velocity));  
      m_player.Location = new D3X.Vector3(oldLoc.X, oldLoc.Y, oldLoc.Z);

      //Check if shield on and in asteroid...asteroid will push player
      foreach (Asteroid asteroid in m_asteroids.Keys)
      {
        //Only check for asteroid-player collisions if shield up
        if (m_player.ShieldOn)
        {
          float distance = 0;
          D3X.Vector3 distanceVector;

          //Check if player and asteroids collide
          //Check if distance between two is less than either radius.
          distanceVector = m_player.Location - asteroid.Location;
          distance = distanceVector.Length();
          float maxRadius = asteroid.BoundingRadius;
          if (m_shield.BoundingRadius > asteroid.BoundingRadius)
          {
            maxRadius = m_shield.BoundingRadius;
          }
          if (distance < maxRadius)
          {
            //Move player
            distanceVector = distanceVector * (maxRadius / distance);
            m_player.Location = asteroid.Location + distanceVector;
          }
        }
      }

    }

    /// <summary>
    /// Moves UFO
    /// </summary>
    private void MoveUfo()
    {
      if (m_ufo != null)
      {
        D3X.Vector4 oldLoc = new D3X.Vector4(m_ufo.Location.X,
          m_ufo.Location.Y, m_ufo.Location.Z, 1f);
        oldLoc = D3X.Vector4.Transform(oldLoc, D3X.Matrix.Translation(m_ufo.Velocity));     
        m_ufo.Location = new D3X.Vector3(oldLoc.X, oldLoc.Y, oldLoc.Z);

        //Destroy UFO when hits map edge
        if (m_ufo.HasBeenClipped)
        {
          DestroyUfo();
        }
      }
    }

    /// <summary>
    /// Destroys the Ufo
    /// </summary>
    private void DestroyUfo()
    {
      m_ufo = null;
      m_ufoCount = 0;
    }

    /// <summary>
    /// Moves all asteroids
    /// </summary>
    private void MoveAsteroids()
    {
      foreach (Asteroid asteroid in m_asteroids.Keys)
      {
        //Move asteroid
        D3X.Vector4 oldLoc = new D3X.Vector4(asteroid.Location.X,
          asteroid.Location.Y, asteroid.Location.Z, 1f);
        oldLoc = D3X.Vector4.Transform(oldLoc, D3X.Matrix.Translation(asteroid.Velocity));             
        asteroid.Location = new D3X.Vector3(oldLoc.X, oldLoc.Y, oldLoc.Z);
      }
    }

    /// <summary>
    /// Moves player bullets.
    /// </summary>
    private void MoveBullets()
    {
      COLG.List<Bullet> removedBullets = new COLG.List<Bullet>();

      foreach (Bullet bullet in m_playerBullets.Keys)
      {
        bullet.DistanceTravelled++;

        if (bullet.DistanceTravelled > 20)
        {
          removedBullets.Add(bullet);
        }
        else
        {
          //Move bullet
          D3X.Vector4 oldLoc = new D3X.Vector4(bullet.Location.X,
            bullet.Location.Y, bullet.Location.Z, 1f);
          oldLoc = D3X.Vector4.Transform(oldLoc, D3X.Matrix.Translation(bullet.Velocity));               
          bullet.Location = new D3X.Vector3(oldLoc.X, oldLoc.Y, oldLoc.Z);
        }
      }

      //Remove expired bullets
      foreach (Bullet bullet in removedBullets)
      {
        m_playerBullets.Remove(bullet);
      }

      //Draw ufo bullets
      removedBullets = new COLG.List<Bullet>();
      foreach (Bullet bullet in m_ufoBullets.Keys)
      {
        //Move bullet
        D3X.Vector4 oldLoc = new D3X.Vector4(bullet.Location.X,
          bullet.Location.Y, bullet.Location.Z, 1f);
        oldLoc = D3X.Vector4.Transform(oldLoc, D3X.Matrix.Translation(bullet.Velocity));                     
        bullet.Location = new D3X.Vector3(oldLoc.X, oldLoc.Y, oldLoc.Z);
        if (bullet.HasBeenClipped)
        {
          removedBullets.Add(bullet);
        }
      }

      //Remove expired bullets
      foreach (Bullet bullet in removedBullets)
      {
        m_ufoBullets.Remove(bullet);
      }
    }

    /// <summary>
    /// Check if player shield should expire.
    /// </summary>
    private void UpdateShield()
    {
      if (m_player.ShieldOn == true)
      {
        m_player.ShieldCount++;
        if (m_player.ShieldCount > ShieldMax)
        {
          m_player.ShieldOn = false;
          m_player.ShieldCount = -1;
        }
      }
    }

    /// <summary>
    /// Draws a grid
    /// </summary>
    private void DrawGrid()
    {
      if (m_firstPerson)
      {
        for (int i = 0; i < 40; i++)
        {
          //setup player matrix
          D3X.Matrix worldPlane =
            D3X.Matrix.RotationY((float)(Math.PI / 2.0f)) *
            D3X.Matrix.Translation(-0f, (float)(i / 3f) - 6.5f, 1f);

          m_device.SetTransform(D3D.TransformState.World, worldPlane);

          DrawDisplayObject(m_gridLine);
        }
        for (int i = 0; i < 40; i++)
        {
          //setup player matrix
          D3X.Matrix worldPlane =
            D3X.Matrix.RotationX((float)(Math.PI / 2.0f)) *
            D3X.Matrix.Translation((float)(i /3f) - 6.5f, 0f, 1f);

          m_device.SetTransform(D3D.TransformState.World, worldPlane);

          DrawDisplayObject(m_gridLine);
        }
      }
    }

    /// <summary>
    /// Draws the player's plane
    /// </summary>
    /// <remarks>Draws plane</remarks>
    private void DrawPlayer()
    {
      //setup player matrix
      D3X.Matrix worldPlane = D3X.Matrix.Scaling(m_player.ScaleFactor,
            m_player.ScaleFactor, m_player.ScaleFactor) *
        m_player.Heading *
        D3X.Matrix.Translation(m_player.Location);

      m_device.SetTransform(D3D.TransformState.World, worldPlane);

      DrawDisplayObject(m_player);

      //Draw shield
      if (m_player.ShieldOn == true)
      {
        worldPlane = 
            D3X.Matrix.Translation(new D3X.Vector3(0.0f, 0.0f, -0.1f)) *
            m_player.Heading *
            D3X.Matrix.Translation(m_player.Location);
        m_device.SetTransform(D3D.TransformState.World, worldPlane);

        m_device.SetRenderState(D3D.RenderState.SourceBlend, D3D.Blend.SourceColor);
        m_device.SetRenderState(D3D.RenderState.DestinationBlend, D3D.Blend.DestinationColor);
        m_device.SetRenderState(D3D.RenderState.AlphaBlendEnable, true);
        DrawDisplayObject(m_shield);
        m_device.SetRenderState(D3D.RenderState.AlphaBlendEnable, false);
      }
    }

   /// <summary>
    /// Draws the Ufo
    /// </summary>
    /// <remarks>Draws plane</remarks>
    private void DrawUfo()
    {
      if (m_ufo != null)
      {
        //setup player matrix
        D3X.Matrix worldPlane = 
          D3X.Matrix.Scaling(m_ufo.ScaleFactor,
            m_ufo.ScaleFactor, m_ufo.ScaleFactor) *
          D3X.Matrix.RotationAxis(new D3X.Vector3(0.0f, 1.0f, 0.0f),
            (float)-(Math.PI/2)) *
          D3X.Matrix.RotationAxis(new D3X.Vector3(1.0f, 0.0f, 0.0f),
            (float)(Math.PI/2)) *
            m_ufo.Heading *
          D3X.Matrix.Translation(m_ufo.Location);

        m_device.SetTransform(D3D.TransformState.World, worldPlane);

        DrawDisplayObject(m_ufo);
      }
    }

    /// <summary>
    /// Draws asteroids
    /// </summary>
    /// <remarks>Draws asteroids</remarks>
    private void DrawAsteroids()
    {
      foreach (Asteroid asteroid in m_asteroids.Keys)
      {
        //setup matrix
        D3X.Matrix worldPlane = D3X.Matrix.Scaling(asteroid.ScaleFactor, 
          asteroid.ScaleFactor, asteroid.ScaleFactor) *
          D3X.Matrix.Translation(new D3X.Vector3(0.0f, 
          -0.4f/asteroid.Size, 0.0f)) *
          D3X.Matrix.RotationAxis(asteroid.Rotation,
            -Environment.TickCount / 500.0f) *
          D3X.Matrix.RotationAxis(new D3X.Vector3(0.0f, 0.0f, 0.5f),
            -Environment.TickCount / 500.0f) *
          asteroid.Heading *
          D3X.Matrix.Translation(asteroid.Location);

        m_device.SetTransform(D3D.TransformState.World, worldPlane);

        DrawDisplayObject(asteroid);
      }
    }

    /// <summary>
    /// Draws all bullets
    /// </summary>
    /// <remarks>Draws player and ufo bullets</remarks>
    private void DrawBullets()
    {
      foreach (Bullet bullet in m_playerBullets.Keys)
      {
        //setup matrix
        D3X.Matrix worldPlane = D3X.Matrix.Translation(bullet.Location);

        m_device.SetTransform(D3D.TransformState.World, worldPlane);

        DrawDisplayObject(bullet);
      }

      foreach (Bullet bullet in m_ufoBullets.Keys)
      {
        //setup matrix
        D3X.Matrix worldPlane = D3X.Matrix.Translation(bullet.Location);

        m_device.SetTransform(D3D.TransformState.World, worldPlane);

        DrawDisplayObject(bullet);
      }
    }

    /// <summary>
    /// Checks for object collisions. Game will end if player collides with
    /// anything.
    /// </summary>
    private void CheckCollisions()
    {
      float distance = 0;
      D3X.Vector3 distanceVector;

      bool isGameOver = false;
      COLG.List<Asteroid> removedAsteroids = new COLG.List<Asteroid>();

      //Check if player shot by ufo
      COLG.List<Bullet> removedBullets = new COLG.List<Bullet>();
      foreach (Bullet bullet in m_ufoBullets.Keys)
      {
        //Check if distance between two is less than either radius.
        distanceVector = m_player.Location - bullet.Location;
        distance = distanceVector.Length();
        if (distance < m_player.BoundingRadius)
        {
          //Check if shield is up
          if (m_player.ShieldOn)
          {
            removedBullets.Add(bullet);
          }
          else
          {
            isGameOver = true;
          }
        }
      }
      foreach (Bullet bullet in removedBullets)
      {
        m_ufoBullets.Remove(bullet);
      }

      //Loop over asteroids
      foreach (Asteroid asteroid in m_asteroids.Keys)
      {
        //Only check for asteroid-player collisions if shield down
        if (!m_player.ShieldOn)
        {
          //Check if player and asteroids collide
          //Check if distance between two is less than either radius.
          distanceVector = m_player.Location - asteroid.Location;
          distance = distanceVector.Length();
          if (distance < asteroid.BoundingRadius ||
            distance < m_player.BoundingRadius)
          {
            isGameOver = true;
          }
        }

        //Check for Ufo-asteroid collision
        if (m_ufo != null)
        {
          //Check if player and asteroids collide
          //Check if distance between two is less than either radius.
          distanceVector = m_ufo.Location - asteroid.Location;
          distance = distanceVector.Length();
          if (distance < m_ufo.BoundingRadius ||
            distance < asteroid.BoundingRadius)
          {
            DestroyUfo();
          }
        }

        //Check if asteroid shot by player
        removedBullets = new COLG.List<Bullet>();
        foreach (Bullet bullet in m_playerBullets.Keys)
        {
          distanceVector = bullet.Location - asteroid.Location;
          distance = distanceVector.Length();
          if (distance < asteroid.BoundingRadius)
          {
            //Split asteroids
            removedAsteroids.Add(asteroid);
            removedBullets.Add(bullet);
          }
        }

        foreach (Bullet bullet in removedBullets)
        {
          m_playerBullets.Remove(bullet);
        }

        //Check if asteroid shot by ufo
        removedBullets = new COLG.List<Bullet>();
        foreach (Bullet bullet in m_ufoBullets.Keys)
        {
          distanceVector = bullet.Location - asteroid.Location;
          distance = distanceVector.Length();
          if (distance < asteroid.BoundingRadius)
          {
            //Just remove bullet
            removedBullets.Add(bullet);
          }
        }

        foreach (Bullet bullet in removedBullets)
        {
          m_ufoBullets.Remove(bullet);
        }
      }

      //Check for Ufo-player collision
      if (m_ufo != null)
      {
        //Check if distance between two is less than either radius.
        distanceVector = m_ufo.Location - m_player.Location;
        distance = distanceVector.Length();
        if (distance < m_ufo.BoundingRadius ||
          distance < m_player.BoundingRadius)
        {
          isGameOver = true;
        }
      }

      //Check if UFO shot
      if (m_ufo != null)
      {
        removedBullets = new COLG.List<Bullet>();
        foreach (Bullet bullet in m_playerBullets.Keys)
        {
          distanceVector = bullet.Location - m_ufo.Location;
          distance = distanceVector.Length();
          if (distance < m_ufo.BoundingRadius)
          {
            //Destroy Ufo
            DestroyUfo();
            removedBullets.Add(bullet);
            m_score += 25;
            break;
          }
        }

        foreach (Bullet bullet in removedBullets)
        {
          m_playerBullets.Remove(bullet);
        }
      }

      //remove shot asteroids
      foreach (Asteroid asteroid in removedAsteroids)
      {
        m_asteroids.Remove(asteroid);
        m_score += asteroid.Size;

        //Add new asteroids
        if (asteroid.Size < 4)
        {
          //Create new asteroids
          CreateAsteroid(asteroid.Size * 2, asteroid.Location, 
            asteroid.AngleHeading + 90);
          CreateAsteroid(asteroid.Size * 2, asteroid.Location, 
            asteroid.AngleHeading - 90);
        }
      }

      //Check if level complete
      if (m_asteroids.Count == 0)
      {
        NextLevel();
      }

      //Check if game over
      if (isGameOver)
      {
        m_renderTimer.Stop();
      }
    }

    /// <summary>
    /// Draws a display object
    /// </summary>
    /// <param name="displayObj">display object</param>
    /// <remarks>transformation matricies must already be applied.</remarks>
    private void DrawDisplayObject(DisplayObject displayObj)
    {
      for (int i = 0; i < displayObj.Resources.DrawMaterials.Length; i++)
      {
        // Set the material and texture for this subset
        m_device.Material = displayObj.Resources.DrawMaterials[i];
        if (displayObj.Resources.DrawTextures != null)
        {
          m_device.SetTexture(0, displayObj.Resources.DrawTextures[i]);
        }
        else
        {
          m_device.SetTexture(0, null);
        }

        // Draw the mesh subset
        displayObj.Resources.DrawMesh.DrawSubset(i);
      }
    }

    /// <summary>
    /// Creates a new asteroid
    /// </summary>
    /// <param name="size">Size of asteroid.  1 is largest, 4 is smallest.
    /// </param>
    /// <param name="location">Initial asteroid location.</param>
    /// <param name="angle">Heading of asteroid (0-360)</param>
    private void CreateAsteroid(int size, D3X.Vector3 location, int angle)
    {
      if (angle > 360)
      {
        angle -= 360;
      }
      else if (angle < -360)
      {
        angle += 360;
      }

      Asteroid newAsteroid = new Asteroid();

      float heading = (float)(angle * Math.PI / 180.0f);
      newAsteroid.Location = location;
      newAsteroid.Velocity = new D3X.Vector3((float)Math.Cos(heading),
        (float)Math.Sin(heading), 0);
      newAsteroid.Rotation = new D3X.Vector3((float)Math.Cos(heading),
        (float)Math.Sin(heading), (float)-Math.Sin(heading));

      //Cap velocity
       float speed = newAsteroid.Velocity.Length();
      if (speed > AsteroidSpeed)
      {
        newAsteroid.Velocity *= (AsteroidSpeed / speed);
      }

      //Use level to increas speed
      float scaleFactor = 1.0f + (m_level - 1) / 10.0f;
      newAsteroid.Velocity *= scaleFactor;

      newAsteroid.Heading = D3X.Matrix.Identity;
      newAsteroid.AngleHeading = angle;

      newAsteroid.Resources = m_asteroidResources;
      newAsteroid.Size = size;
      newAsteroid.ComputeBoundingRadius((float)0.5 / size);

      m_asteroids.Add(newAsteroid, newAsteroid);

    }

    /// <summary>
    /// Draws the player score
    /// </summary>
    /// <remarks>These labels drawn as simple 2-D text</remarks>
    private void DrawScore()
    {
      string scoreLabel = "Score: " + m_score.ToString(
        GBZ.CultureInfo.CurrentCulture);

      m_drawFont.DrawString(null, scoreLabel,
        20, 20, DRW.Color.White);

      //Show game over if timer is stopped
      if (!m_renderTimer.Enabled)
      {
        m_drawFont.DrawString(null, "Game Over\n" +
          "'F5' - Restart\n" +
          "'ESC' - Exit\n" + 
          "Left Arrow - Rotate Left\n" +
          "Right Arrow - Rotate Right\n" +
          "Up Arrow - Accelerate\n" +
          "Down Arrow - Activate Shield\n" + 
          "'V' - Switch between top down and first-person views.\n",
          20, 50, DRW.Color.Red);
      }
    }

    /// <summary>
    /// Sets up the lights used to illuminate the scene
    /// </summary>
    /// <remarks>Three spotlights are used for this scene</remarks>
    private void SetupLights()
    {
      D3D.Light light1 = new D3D.Light();
      light1.Type = D3D.LightType.Directional;
      light1.Ambient = DRW.Color.White;
      light1.Diffuse = DRW.Color.White;
      light1.Specular = DRW.Color.White;
      light1.Position = new D3X.Vector3(0f, 10f, -5f);
      light1.Direction = new D3X.Vector3(0f, -20f, 30f);
      m_device.SetLight(0, light1);

      D3D.Light light2 = new D3D.Light();
      light2.Type = D3D.LightType.Directional;
      light2.Ambient = DRW.Color.White;
      light2.Diffuse = DRW.Color.White;
      light2.Specular = DRW.Color.White;
      light2.Position = new D3X.Vector3(-5f, 10f, 5f);
      light2.Direction = new D3X.Vector3(30f, -20f, -30f);
      m_device.SetLight(1, light2);

      D3D.Light light3 = new D3D.Light();
      light3.Type = D3D.LightType.Directional;
      light3.Ambient = DRW.Color.White;
      light3.Diffuse = DRW.Color.White;
      light3.Specular = DRW.Color.White;
      light3.Position = new D3X.Vector3(-5f, 10f, -5f);
      light3.Direction = new D3X.Vector3(30f, -20f, 30f);
      m_device.SetLight(2, light3);
    }

    /// <summary>
    /// Draws entire scene to device
    /// </summary>
    /// <remarks>This method is called whenever the animation timer fires
    /// </remarks>
    private void Render()
    {
      //Create device when needed
      if (m_device == null)
      {
        try
        {
          CreateDirectXDevice();
        }
        catch (D3D.Direct3D9Exception)
        {
          //If we have no device just exit.  Eventually we should be able
          // to create one.
          return;
        }
      }

      //Clear the backbuffer to a solid color
      m_device.Clear(D3D.ClearFlags.Target | D3D.ClearFlags.ZBuffer,
        DRW.Color.Black, 1.0f, 0);

      //Begin the scene
      m_device.BeginScene();
      // Setup the world, view, and projection matrices
      SetupLights();

      //Create UFO, if needed
      GenerateUfo();

      //Move All objects
      MovePlayer();
      MoveUfo();
      MoveAsteroids();
      MoveBullets();

      //Perform Tests
      UpdateShield();

       //Draw objects
      DrawGrid();
      DrawAsteroids();
      DrawUfo();
      DrawBullets();
      DrawPlayer();

      SetView();

      //Check for any object collisions
      CheckCollisions();

      //Draw Text
      DrawScore();

      //End the scene
      m_device.EndScene();

      try
      {
        // Update the screen
        m_device.Present();
      }
      catch (D3D.Direct3D9Exception)
      {
        //If there was an error presenting the device, the device is most
        //likely invalid.  This could happen if user locked then unlocked the
        //screen. If this happens need to clear device so it will be created
        //for next draw.
        m_device.Dispose();
      }
    }

    /// <summary>
    /// Generate UFO at random location and heading
    /// </summary>
    private void GenerateUfo()
    {
      if (m_ufo == null)
      {
        m_ufoCount++;

        //Generate UFO when timer expires
        if (m_ufoCount == UfoDelay)
        {
          int mapSize = 6;

          //Reset timer
          m_ufoCount = 0;

          //Create UFO
          m_ufo = new DisplayObject();
          m_ufo.Resources = m_ufoResources;

          //Generate random edge that UFO starts on and heading
          int dir;
          dir = m_random.Next(4);
          int heading = m_random.Next(80) - 40;

          float startPosition = (float)(m_random.Next(mapSize) - mapSize / 2);
          if (dir == 0)
          {
            m_ufo.Location = new D3X.Vector3(-mapSize, startPosition, 0.0f);
          }
          else if (dir == 1)
          {
            m_ufo.Location = new D3X.Vector3(mapSize, startPosition, 0.0f);
            heading += 180;
          }
          else if (dir == 2)
          {
            m_ufo.Location = new D3X.Vector3(startPosition, -mapSize, 0.0f);
            heading += 90;
          }
          else if (dir == 3)
          {
            m_ufo.Location = new D3X.Vector3(startPosition, mapSize, 0.0f);
            heading += 270;
          }

          //Compute heading
          float radHeading = (float)(heading * Math.PI / 180.0f);
          m_ufo.Velocity = new D3X.Vector3((float)Math.Cos(radHeading),
            (float)Math.Sin(radHeading), 0);

          //Cap velocity
          float speed = m_ufo.Velocity.Length();
          m_ufo.Velocity *= (UfoSpeed / speed);

          //set heading
          m_ufo.Heading = D3X.Matrix.RotationAxis(new D3X.Vector3(0, 0, 1),
            radHeading);

          m_ufo.ComputeBoundingRadius(0.05f);
          m_ufoShootCount = 0;
        }
      }
      else
      {
        //test if time for UFO to shoot, and 
        // check if have max bullets already
        if (m_ufoShootCount >= UfoShootDelay &&
          m_ufoBullets.Count < 10)
        {
          m_ufoShootCount = 0;

          //Check if have max bullets already
          Bullet bullet = new Bullet();
          bullet.Location = new D3X.Vector3(m_ufo.Location.X,
            m_ufo.Location.Y, m_ufo.Location.Z);
          bullet.Resources = new ResourceSet();
          bullet.Resources.DrawMesh = D3D.Mesh.CreateSphere(m_device, 0.05f, 30, 30);
          bullet.Resources.DrawMaterials = new D3D.Material[1];
          bullet.Resources.DrawMaterials[0] = new D3D.Material();
          bullet.Resources.DrawMaterials[0].Ambient = DRW.Color.DarkGreen;
          bullet.Resources.DrawMaterials[0].Diffuse = DRW.Color.Lime;

          //Shoot in direction of the player
          int angle = (int)(180.0f / Math.PI * Math.Atan(
            (m_ufo.Location.Y - m_player.Location.Y) /
            (m_ufo.Location.X - m_player.Location.X)));
          if (m_ufo.Location.X > m_player.Location.X)
          {
            angle += 180;
          }

          //Add random factor of plus/minus 20 degrees
          angle += (m_random.Next(40) - 20);

          float heading = (float)(angle * Math.PI / 180.0f);
          bullet.Velocity = new D3X.Vector3((float)Math.Cos(heading),
            (float)Math.Sin(heading), 0);

          //Scale up speed.  
          float speed = bullet.Velocity.Length();
          bullet.Velocity *= (0.5f / speed);

          m_ufoBullets.Add(bullet, bullet);
        }
        else
        {
          m_ufoShootCount++;
        }
      }
    }

    /// <summary>
    /// Sets the view matrix
    /// </summary>
    /// <remarks>Sets up camera to draw from either an overhead, or 
    /// first-person view.</remarks>
    private void SetView()
    {
      if (m_firstPerson)
      {
        //Render from player's perpspective
        D3X.Vector4 tempEye = new D3X.Vector4(0f, 2f, 0f, 1f);
        tempEye = D3X.Vector4.Transform(tempEye, m_player.Heading);
        D3X.Vector3 eye = new D3X.Vector3(tempEye.X, tempEye.Y, 0f);
        float dist = eye.Length();
        eye *= (3.0f / dist);
        eye += m_player.Location;
        eye.Z = -0.6f;
        D3X.Vector3 target = new D3X.Vector3(m_player.Location.X,
          m_player.Location.Y, 0.3f);

        m_View = D3X.Matrix.LookAtLH(eye, target,
          new D3X.Vector3(0.0f, 0.0f, -1.0f));
      }
      else
      {
        // Set up our view matrix. A view matrix can be defined given an eye,
        // point a point to look at, and a direction for which way is up. Here, 
        // we set the eye eight units back along the z-axis and left two units, 
        // look at the left of the origin, and define "up" to be in the 
        // y-direction.  This is the overhead view
        m_View = D3X.Matrix.LookAtLH(
          new D3X.Vector3(0.0f, 0f, -15.0f),
          new D3X.Vector3(0.0f, 0.0f, 0.0f),
          new D3X.Vector3(0.0f, 1.0f, 0.0f));
      }

      m_device.SetTransform(D3D.TransformState.View, m_View);

    }

    /// <summary>
    /// Event that fires when when a DirectX device is reset
    /// </summary>
    /// <remarks>When the device is reset, need to reset device parameters and
    /// regenerate items dependent on device state</remarks>
    /// <param name="sender">Object that generated the event</param>
    /// <param name="e">Event argument</param>
    private void OnResetDevice(object sender, EventArgs e)
    {
      D3D.Device dev = (D3D.Device)sender;

      //Reset device parameters
      // Turn off culling, so we see the front and back of the triangle
      dev.SetRenderState(D3D.RenderState.CullMode, D3D.Cull.None);
      dev.SetRenderState(D3D.RenderState.Lighting, true);

      // Turn on the ZBuffer
      dev.SetRenderState(D3D.RenderState.ZEnable, D3D.ZBufferType.UseZBuffer);
      dev.SetRenderState(D3D.RenderState.AlphaBlendEnable, true);

      // For the projection matrix, we set up a perspective transform (which
      // transforms geometry from 3D view space to 2D viewport space, with
      // a perspective divide making objects smaller in the distance). To build
      // a perpsective transform, we need the field of view (1/4 pi is common),
      // the aspect ratio, and the near and far clipping planes (which define 
      // at what distances geometry should be no longer be rendered).
      m_Projection = D3X.Matrix.PerspectiveFovLH(
        (float)Math.PI / 4.0f, 1.0f, 1.0f, 100.0f);

      m_device.SetTransform(D3D.TransformState.View, m_View);
      m_device.SetTransform(D3D.TransformState.Projection, m_Projection);

      //start new game
      StartGame();
    }

    /// <summary>
    /// Loads all DirectX resources
    /// </summary>
    private void LoadResources()
    {
      // Now create our textures
      // get a reference to the current assembly
      System.Reflection.Assembly currAss =
        System.Reflection.Assembly.GetExecutingAssembly();

      //Create player
      if (m_planeResources == null)
      {
        m_planeResources = new ResourceSet();
      }
      System.IO.Stream bitmapStream = currAss.GetManifestResourceStream(
          "DirectXAsteroids.Meshes.airplane 2.x");
      m_planeResources.DrawMesh = D3D.Mesh.FromStream(m_device, bitmapStream, D3D.MeshFlags.Managed);
      D3D.ExtendedMaterial[] materials = m_planeResources.DrawMesh.GetMaterials();

      if (m_planeResources.DrawTextures == null)
      {
        // We need to extract the material properties and texture names 
        m_planeResources.DrawTextures = new D3D.Texture[materials.Length];
        m_planeResources.DrawMaterials = new D3D.Material[materials.Length];
        for (int i = 0; i < m_planeResources.DrawMaterials.Length; i++)
        {
          m_planeResources.DrawMaterials[i] = materials[i].MaterialD3D;

          // Set the ambient color for the material (D3DX does not do this)
          m_planeResources.DrawMaterials[i].Ambient =
            m_planeResources.DrawMaterials[i].Diffuse;

          // Create the texture
          if (materials[i].TextureFileName != null)
          {
            bitmapStream = currAss.GetManifestResourceStream(
                "DirectXAsteroids.Images." + materials[i].TextureFileName);
            m_planeResources.DrawTextures[i] = D3D.Texture.FromStream(m_device,
              bitmapStream, 0, D3D.Pool.Managed);
          }
        }
      }

      //Create UFO resources
      if (m_ufoResources == null)
      {
        m_ufoResources = new ResourceSet();
      }
      bitmapStream = currAss.GetManifestResourceStream(
          "DirectXAsteroids.Meshes.bigship1.x");
      m_ufoResources.DrawMesh = D3D.Mesh.FromStream(m_device, bitmapStream, D3D.MeshFlags.Managed);
      materials = m_ufoResources.DrawMesh.GetMaterials();

      if (m_ufoResources.DrawMaterials == null)
      {
        // We need to extract the material properties and texture names 
        m_ufoResources.DrawMaterials = new D3D.Material[materials.Length];
        m_ufoResources.DrawTextures = null;
        for (int i = 0; i < m_ufoResources.DrawMaterials.Length; i++)
        {
          m_ufoResources.DrawMaterials[i] = new D3D.Material();
          m_ufoResources.DrawMaterials[i].Ambient = DRW.Color.DarkGreen;
          m_ufoResources.DrawMaterials[i].Diffuse = DRW.Color.Lime;
        }
      }

      //Create shield
      m_shield = new DisplayObject();
      m_shield.BoundingRadius = 0.7f;
      if (m_shieldResources == null)
      {
        m_shieldResources = new ResourceSet();
      }
      m_shield.Resources = m_shieldResources;
      m_shield.Resources.DrawMesh = D3D.Mesh.CreateSphere(m_device,
        m_shield.BoundingRadius, 100, 100);
      m_shield.Resources.DrawMaterials = new D3D.Material[1];
      m_shield.Resources.DrawMaterials[0] = new D3D.Material();
      DRW.Color shieldColor = DRW.Color.FromArgb(150, 100, 100, 200);
      m_shield.Resources.DrawMaterials[0].Diffuse = shieldColor;
      DRW.Color shieldAmbientColor = DRW.Color.FromArgb(50, 80, 50);
      m_shield.Resources.DrawMaterials[0].Ambient = shieldAmbientColor;

      //Create cylander used for grid
      m_gridLine = new DisplayObject();
      m_gridLine.Resources = new ResourceSet();
      m_gridLine.Resources.DrawMesh = D3D.Mesh.CreateCylinder(m_device, 0.02f, 0.02f, 13f, 300, 2);
      m_gridLine.Resources.DrawMaterials = new D3D.Material[1];
      m_gridLine.Resources.DrawMaterials[0] = new D3D.Material();
      m_gridLine.Resources.DrawMaterials[0].Diffuse = DRW.Color.White;

      //Create asteroids
      if (m_asteroidResources == null)
      {
        m_asteroidResources = new ResourceSet();
      }
      bitmapStream = currAss.GetManifestResourceStream("DirectXAsteroids.Meshes.rock.x");
      m_asteroidResources.DrawMesh = D3D.Mesh.FromStream(m_device, bitmapStream,
       D3D.MeshFlags.Managed);
      materials = m_asteroidResources.DrawMesh.GetMaterials();

      if (m_asteroidResources.DrawTextures == null)
      {
        // We need to extract the material properties and texture names 
        m_asteroidResources.DrawTextures = new D3D.Texture[materials.Length];
        m_asteroidResources.DrawMaterials = new D3D.Material[materials.Length];
        for (int i = 0; i < m_asteroidResources.DrawMaterials.Length; i++)
        {
          m_asteroidResources.DrawMaterials[i] = materials[i].MaterialD3D;

          // Set the ambient color for the material (D3DX does not do this)
          m_asteroidResources.DrawMaterials[i].Ambient =
            m_asteroidResources.DrawMaterials[i].Diffuse;

          // Create the texture
          materials[i].TextureFileName = "Rock01.jpg";
          if (materials[i].TextureFileName != null)
          {
            bitmapStream = currAss.GetManifestResourceStream(
                "DirectXAsteroids.Images." + materials[i].TextureFileName);
            m_asteroidResources.DrawTextures[i] = D3D.Texture.FromStream(m_device,
              bitmapStream, 0, D3D.Pool.Managed);
          }
        }
      }
    }

    #endregion

    /// <summary>
    /// Create initial objects, and starts game.
    /// </summary>
    private void StartGame()
    {
      m_random = new Random();
      m_score = 0;
      m_level = 0;
      m_ufoCount = 0;
      m_ufo = null;

      LoadResources();

      NextLevel();

      //Start game
      if (!m_renderTimer.Enabled)
      {
        m_renderTimer.Start();
      }
    }

    /// <summary>
    /// Advances game by creating fresh level of asteroids
    /// </summary>
    private void NextLevel()
    {
      m_level++;

      //Create player
      m_player = new Plane();
      m_player.Location = new D3X.Vector3(0, 0, 0);
      m_player.Velocity = new D3X.Vector3(0, 0, 0);
      m_player.Heading = D3X.Matrix.RotationAxis(new D3X.Vector3(1, 0, 0),
        -1.57f);
      m_player.Resources = m_planeResources;
      m_player.ComputeBoundingRadius(0.1f);

      //Create initial Asteroids
      m_asteroids = new COLG.Dictionary<Asteroid, Asteroid>();
      for (int i = 0; i < 4; i++)
      {
        int heading = m_random.Next(0, 360);
        CreateAsteroid(1, new D3X.Vector3(8, 6, 0), heading);
      }

      //Create bullet collections
      m_playerBullets = new COLG.Dictionary<Bullet, Bullet>();
      m_ufoBullets = new COLG.Dictionary<Bullet, Bullet>();
    }

    /// <summary>
    /// Event handler for pressing a key
    /// </summary>
    /// <param name="sender">Object that generated event</param>
    /// <param name="e">Event arguments</param>
    private void AsteroidsKeyDown(object sender, SWF.KeyEventArgs e)
    {
      if (e.KeyCode == SWF.Keys.Left)
      {
        m_player.TurnLeft = true;
        m_player.TurnRight = false;
      }
      else if (e.KeyCode == SWF.Keys.Right)
      {
        m_player.TurnLeft = false;
        m_player.TurnRight = true;
      }
      else if (e.KeyCode == SWF.Keys.Up)
      {
        m_player.MoveForward = true;
      }
      else if (e.KeyCode == SWF.Keys.Down && m_player.ShieldCount >= 0)
      {
        m_player.ShieldOn = true;
      }

      //Player shoots
      else if (e.KeyCode == SWF.Keys.Space && m_player.ShieldOn == false)
      {
        //Check if have max bullets already
         if (m_playerBullets.Count < 10)
        {
          Bullet bullet = new Bullet();
          bullet.Location = new D3X.Vector3(m_player.Location.X,
            m_player.Location.Y, m_player.Location.Z);
          bullet.Resources = new ResourceSet();
          bullet.Resources.DrawMesh = D3D.Mesh.CreateSphere(m_device, 0.05f, 30, 30);
          bullet.Resources.DrawMaterials = new D3D.Material[1];
          bullet.Resources.DrawMaterials[0] = new D3D.Material();
          bullet.Resources.DrawMaterials[0].Ambient = DRW.Color.Silver;
          bullet.Resources.DrawMaterials[0].Diffuse = DRW.Color.Gray;
          
          //Scale up speed.  Get bullet heading from player facing.
          bullet.Velocity = new D3X.Vector3(m_player.Heading.M12,
            -m_player.Heading.M11, m_player.Heading.M13);
          float speed = bullet.Velocity.Length();
          bullet.Velocity *= (0.5f / speed);

          m_playerBullets.Add(bullet, bullet);
        }
      }

      //Quit game
      else if (e.KeyCode == SWF.Keys.Escape)
      {
        Close();
      }

      //Restart game
      else if (e.KeyCode == SWF.Keys.F5)
      {
        StartGame();
      }

      //Switch view
      else if (e.KeyCode == SWF.Keys.V)
      {
        m_firstPerson = !m_firstPerson;
      }
    }

    /// <summary>
    /// Event handler for releasing a key
    /// </summary>
    /// <param name="sender">Object that generated event</param>
    /// <param name="e">Event arguments</param>
    private void AsteroidsKeyUp(object sender, SWF.KeyEventArgs e)
    {
      if (e.KeyCode == SWF.Keys.Left)
      {
        m_player.TurnLeft = false;
      }
      else if (e.KeyCode == SWF.Keys.Right)
      {
        m_player.TurnRight = false;
      }
      else if (e.KeyCode == SWF.Keys.Up)
      {
        m_player.MoveForward = false;
      }
      else if (e.KeyCode == SWF.Keys.Down)
      {
        m_player.ShieldOn = false;
        m_player.ShieldCount = 0;
      }
    }
  }

}