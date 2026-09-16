using IOP = System.Runtime.InteropServices;

namespace DirectXAsteroids
{
  /// <summary>
  /// Legacy Win32 methods
  /// </summary>
  public static class NativeMethods
  {
    #region Constants

    /// <summary>
    /// VB6 specific message used to send it private messages via SendMessage
    /// and PostMessage to force it to execute internal/private methods.
    /// </summary>
    public const int VisualBasicMethodValue = 0x1011;

    /// <summary>
    /// VB6 specific message that forces it to Validate the target control.
    /// </summary>
    public const int VisualBasicValidatingEvent = 0x162;

    #endregion

    #region Enumerations

    #region WindowsMessages

    /// <summary>
    /// Win32 Windows Message Definitions
    /// </summary>
    /// <remarks>Use these message definitions for handling and invoking
    /// WindowsProcedures.  They may also be required when posting or sending
    /// a message to a window.</remarks>
    /// <history>
    ///   <entry date="11/02/05" author="R. Jacobs" scr= "SCR005429"
    ///          desc="Several Route Preferences will not save"/> 
    /// </history>       
    public static class WindowsMessages
    {
      /// <summary>
      /// This message is sent to a window immediately before it loses the 
      /// keyboard focus.
      /// </summary>
      public const int WM_KILLFOCUS = 0x0008;

      /// <summary>
      /// The WM_LBUTTONDOWN message is posted when the user presses the left 
      /// mouse button while the cursor is in the client area of a window. If 
      /// the mouse is not captured, the message is posted to the window 
      /// beneath the cursor. Otherwise, the message is posted to the window 
      /// that has captured the mouse.
      /// </summary>
      public const int WM_LBUTTONDOWN = 0x0201;
    }

    #endregion

    #region PeekMessageOption

    /// <summary>
    /// Peek Message Options
    /// </summary>
    /// <remarks>Pass one of these values as the last parameter of 
    /// the Win32 API method PeekMessage</remarks>
    public static class PeekMessageOptions
    {
      /// <summary>
      /// Messages are not removed from the queue after processing by 
      /// PeekMessage.
      /// </summary>
      public const int PM_NOREMOVE = 0x0000;

      /// <summary>
      /// Messages are removed from the queue after processing by PeekMessage.
      /// </summary>
      public const int PM_REMOVE = 0x0001;
    }

    #endregion

    /// <summary>
    /// The Get/Set Windows Long indices
    /// </summary>
    public enum WindowsLongIndicies
    {
      /// <summary>
      /// User data to send to a window
      /// </summary>
      GWL_USERDATA = (-21),

      /// <summary>
      /// Extended style parameter for a window
      /// </summary>
      GWL_EXSTYLE = (-20),

      /// <summary>
      /// Style parameter for a window
      /// </summary>
      GWL_STYLE = (-16),

      /// <summary>
      /// Identifier for a window
      /// </summary>
      GWL_ID = (-12),

      /// <summary>
      /// Parent for a window
      /// </summary>
      GWL_HWNDPARENT = (-8),

      /// <summary>
      /// Handle for a window
      /// </summary>
      GWL_HINSTANCE = (-6),

      /// <summary>
      /// Window Proc for a window
      /// </summary>
      GWL_WNDPROC = (-4),
    }

    #endregion

    #region Win32 Method Signatures

    /// <summary>
    /// Sets a property represented by a "long" integer on a window
    /// </summary>
    /// <param name="hWnd">The window that is modified</param>
    /// <param name="nIndex">The property to modify</param>
    /// <param name="dwNewLong">The new value of the property</param>
    /// <returns>The old value of the property</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability",
      "CA1901:PInvokeDeclarationsShouldBePortable",
      Justification = "This code will never run on 64-bit machines")]
    [IOP.DllImport("user32.dll")]
    public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex,
      IntPtr dwNewLong);

    /// <summary>
    /// Windows API Send Message
    /// </summary>
    /// <remarks>Sends a synchronous message to the target window.</remarks>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="message">Specifies the message to be sent.</param>
    /// <param name="firstParameter">Specifies additional message-specific 
    /// information.</param>
    /// <param name="secondParameter">Specifies additional message-specific 
    /// information.</param>
    /// <returns></returns>
    [IOP.DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr windowHandle,
      int message, IntPtr firstParameter, IntPtr secondParameter);


    /// <summary>
    /// Win32 SDK IsChild
    /// </summary>
    /// <remarks>N/A</remarks>
    /// <param name="hwndParent">The parent window.</param>
    /// <param name="hwndChild">The window tested to see if it is a child of the
    /// parent.</param>
    /// <returns>An int value that indicates if the window is a child window.
    /// Non-Zero means the window is a child.</returns>
    [IOP.DllImport("user32.dll")]
    public static extern int IsChild(IntPtr hwndParent, IntPtr hwndChild);

    #endregion
  }
}
