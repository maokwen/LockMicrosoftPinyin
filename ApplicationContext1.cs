using Microsoft.Win32;
using System;
using System.Windows.Forms;
using System.Reflection;

public class ApplicationContext1 : ApplicationContext
{
  private readonly NotifyIcon notifyIcon;
  private readonly ContextMenuStrip contextMenu;
  private readonly System.Threading.Timer timer;
  private readonly uint WM_IME_CONTROL = 0x283;
  private readonly IntPtr IMC_SETCONVERSIONMODE = new IntPtr(0x002);
  private readonly IntPtr IME_CHINESE = new IntPtr(0x401);

  [System.Runtime.InteropServices.DllImport("user32.dll")]
  public static extern IntPtr GetForegroundWindow();

  [System.Runtime.InteropServices.DllImport("imm32.dll")]
  public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

  [System.Runtime.InteropServices.DllImport("user32.dll")]
  public static extern Int16 GetKeyboardLayout(Int32 hWnd);

  [System.Runtime.InteropServices.DllImport("user32.dll")]
  public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

  public ApplicationContext1()
  {
    contextMenu = new ContextMenuStrip();
    contextMenu.ShowImageMargin = false;
    contextMenu.ShowCheckMargin = true;

    var enable = new ToolStripMenuItem("Enable", null, OnCheck);
    enable.Checked = true;
    enable.CheckOnClick = true;
    contextMenu.Items.Add(enable);

    contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, OnExit));

    notifyIcon = new NotifyIcon();
    notifyIcon.Icon = SystemIcons.Application;
    notifyIcon.ContextMenuStrip = contextMenu;
    notifyIcon.Visible = true;

    var tsInterval = new TimeSpan(0, 0, 1);
    timer = new System.Threading.Timer(
      new System.Threading.TimerCallback(LockIME), null, tsInterval, tsInterval);
  }

  private void LockIME(object state)
  {
    try
    {
      var hWnd = GetForegroundWindow();
      if (hWnd == IntPtr.Zero) return;
      var id = ImmGetDefaultIMEWnd(hWnd);
      if (id == IntPtr.Zero) return;

      if (InChsStatus())
      {
        SetChsStatus(id);
      }
    }
    catch { }
  }

  private bool InChsStatus()
  {
    var langId = GetKeyboardLayout(0);
    var lang = (langId & 0xffff);
    return lang == 0x804;
  }

  private void SetChsStatus(IntPtr hWnd)
  {
    SendMessage(hWnd, WM_IME_CONTROL, IMC_SETCONVERSIONMODE, IME_CHINESE);
  }

  private void OnExit(object sender, EventArgs e)
  {
    timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    timer.Dispose();

    notifyIcon.Dispose();
    Application.Exit();
  }

  private void OnCheck(object sender, EventArgs e)
  {
    if ((sender as ToolStripMenuItem).Checked)
    {
      timer.Change(1, 1); ;
    }
    else
    {
      timer.Change(Timeout.Infinite, Timeout.Infinite); ;
    }
  }
}
