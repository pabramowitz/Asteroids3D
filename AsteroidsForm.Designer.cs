namespace DirectXAsteroids
{
  partial class AsteroidsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; 
    /// otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }

      m_renderTimer.Stop();
      m_renderTimer.Dispose();
      if (m_device != null)
      {
        m_device.Dispose();
      }

      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AsteroidsForm));
      this.SuspendLayout();
      // 
      // AsteroidsForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "AsteroidsForm";
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AsteroidsKeyUp);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AsteroidsKeyDown);
      this.ResumeLayout(false);

    }

    #endregion
  }
}