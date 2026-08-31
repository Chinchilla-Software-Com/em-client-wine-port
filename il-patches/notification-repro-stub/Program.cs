// Minimal standalone repro of eM Client's FormGenericNotification/LayeredBaseForm mechanism,
// built to test hypotheses about the "empty box until fade" bug (see
// reports/notification-empty-until-fade-findings.md) fast -- normal C# source, no Cecil IL
// patching, no MailClient.dll involved at all. Build/run against a live bottle exactly like
// font-systemlink-writer/ (not itself deployed into the app; a dev-time tool run manually):
//   dotnet publish -c Release
//   wine <path-to>\bin\Release\net8.0-windows\win-x86\publish\notif-repro.exe [args]
//
// Five iterations (each toggled by a positional arg below, all on by default) were tried against
// the real bug and NONE reproduced it -- real, useful negative evidence ruling out the WinForms
// layered-window mechanism itself as sufficient cause. See the findings report for the full
// writeup; summary of what each arg adds and why:
//   fullOpacity  (default 1.0)  the Opacity value used for "fully visible". Wine's
//                X11DRV_SetLayeredWindowAttributes deletes the _NET_WM_WINDOW_OPACITY X11
//                property (XDeleteProperty) when opacity rounds to exactly 0xffffffff/alpha=255
//                -- a different code path than any other value (XChangeProperty). Pass e.g. 0.996
//                to test staying off the delete path. Tested: no difference.
//   holdMs       (default 4000) how long to hold at full opacity before fading out.
//   useShadow    (default on)   separate drop-shadow WS_EX_LAYERED companion window driven by
//                UpdateLayeredWindow, matching LayeredBaseForm.layeredWindow.
//   useRawShow   (default on)   raw ShowWindow/SetWindowPos(HWND_TOPMOST) show sequence,
//                bypassing WinForms' own Show(), matching FormGenericNotification.Show()'s real
//                implementation.
//   useAvatar    (default on)   draws an avatar/icon (filled circle + initials) in the content
//                area, matching FormMailNotification's Image. Also spawns a background Thread
//                that delivers the avatar ~180ms later via cross-thread Invoke() + Invalidate(),
//                matching the real AvatarManager_AvatarUpdated -> SafeInvoke -> Invalidate()
//                pattern, rather than drawing it synchronously on the same thread.
//   useMainWindow (default off) shows a large mock "main window" first (Application.Run's actual
//                argument) and creates/shows the notification ~1.5s later as an independent
//                Form.Show() on the same UI thread -- closer to the real scenario where the
//                notification is never the process's only/first window.
//
// Every run's animation events (ctor, Load, each OnPaint with its state/opacity, state
// transitions, avatar delivery) are logged to Z:\tmp\notif-repro.log (== /tmp/notif-repro.log on
// the Linux side) with Environment.TickCount timestamps -- confirmed to share the same clock as
// Wine's own CX_DEBUGMSG trace timestamps (both raw system-uptime milliseconds), so a repro run's
// log lines up directly against a simultaneous Wine trace with no unit conversion needed.
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

double fullOpacity = args.Length > 0 ? double.Parse(args[0]) : 1.0;
int holdMs = args.Length > 1 ? int.Parse(args[1]) : 4000;
bool useShadow = args.Length > 2 ? args[2] == "1" : true;
bool useRawShow = args.Length > 3 ? args[3] == "1" : true;
bool useAvatar = args.Length > 4 ? args[4] == "1" : true;
bool useMainWindow = args.Length > 5 ? args[5] == "1" : false;

ApplicationConfiguration.Initialize();
if (useMainWindow)
{
    Application.Run(new MainForm(fullOpacity, holdMs, useShadow, useRawShow, useAvatar));
}
else
{
    Application.Run(new ReproForm(fullOpacity, holdMs, useShadow, useRawShow, useAvatar));
}

// Stand-in for em Client's own big main window -- shown first and stays open the whole time,
// mimicking the real scenario where the notification appears as a secondary window while the
// app's main window is already active, rather than being the process's only/first-ever window.
class MainForm : Form
{
    public MainForm(double fullOpacity, int holdMs, bool useShadow, bool useRawShow, bool useAvatar)
    {
        Text = "Mock main window";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Gainsboro;

        var startTimer = new System.Windows.Forms.Timer { Interval = 1500 };
        startTimer.Tick += (_, _) =>
        {
            startTimer.Stop();
            var notif = new ReproForm(fullOpacity, holdMs, useShadow, useRawShow, useAvatar);
            notif.FormClosed += (_, _) => notif.Dispose();
            notif.Show();
        };

        Load += (_, _) => startTimer.Start();
    }
}

static class Win32
{
    public const int SW_SHOWNOACTIVATE = 4;
    public const uint SWP_NOACTIVATE = 0x10;
    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public const int WS_EX_LAYERED = 0x80000;
    public const int WS_EX_TOOLWINDOW = 0x80;
    public const int ULW_ALPHA = 2;

    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] public static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X; public int Y; public POINT(int x, int y) { X = x; Y = y; } }
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE { public int cx; public int cy; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION { public byte BlendOp; public byte BlendFlags; public byte SourceConstantAlpha; public byte AlphaFormat; }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
        IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
}

// Separate drop-shadow window, matching LayeredBaseForm's layeredWindow field: its own hwnd,
// WS_EX_LAYERED via CreateParams, painted entirely through UpdateLayeredWindow (not WM_PAINT).
class ShadowForm : Form
{
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= Win32.WS_EX_LAYERED;
            cp.ExStyle |= Win32.WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    public ShadowForm()
    {
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        MinimizeBox = false;
        MaximizeBox = false;
        ControlBox = false;
    }

    public void UpdateWindow(Bitmap image, byte opacity, int width, int height, Point pos)
    {
        IntPtr windowDC = Win32.GetWindowDC(Handle);
        IntPtr memDC = Win32.CreateCompatibleDC(windowDC);
        IntPtr hBitmap = image.GetHbitmap(Color.FromArgb(0));
        IntPtr oldObj = Win32.SelectObject(memDC, hBitmap);
        var size = new Win32.SIZE { cx = Math.Min(image.Width, width), cy = Math.Min(image.Height, height) };
        var srcPt = new Win32.POINT(0, 0);
        var dstPt = new Win32.POINT(pos.X, pos.Y);
        var blend = new Win32.BLENDFUNCTION { BlendOp = 0, SourceConstantAlpha = opacity, AlphaFormat = 1, BlendFlags = 0 };
        Win32.UpdateLayeredWindow(Handle, windowDC, ref dstPt, ref size, memDC, ref srcPt, 0, ref blend, Win32.ULW_ALPHA);
        Win32.SelectObject(memDC, oldObj);
        Win32.DeleteObject(hBitmap);
        Win32.DeleteDC(memDC);
        Win32.ReleaseDC(Handle, windowDC);
    }
}

class ReproForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new();
    private readonly double fullOpacity;
    private readonly int holdMs;
    private readonly bool useShadow;
    private readonly bool useRawShow;
    private readonly bool useAvatar;
    private bool avatarReady;
    private int state; // 0=Appearing, 1=Visible(holding), 2=Disappearing
    private DateTime visibleSince;
    private ShadowForm? shadow;
    private Bitmap? shadowBitmap;
    private const int ShadowSize = 9;
    private static readonly string LogPath = @"Z:\tmp\notif-repro.log";

    public ReproForm(double fullOpacity, int holdMs, bool useShadow, bool useRawShow, bool useAvatar)
    {
        this.fullOpacity = fullOpacity;
        this.holdMs = holdMs;
        this.useShadow = useShadow;
        this.useRawShow = useRawShow;
        this.useAvatar = useAvatar;

        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        Width = 310;
        Height = 125;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(40, 40, 40);
        TransparencyKey = Color.Lime;
        AllowTransparency = true;

        var wa = Screen.PrimaryScreen!.WorkingArea;
        Location = new Point(wa.Right - Width - 20, wa.Bottom - Height - 20);

        Log($"ctor: fullOpacity={fullOpacity} holdMs={holdMs} useShadow={useShadow} useRawShow={useRawShow} useAvatar={useAvatar}");
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (useShadow)
        {
            shadow = new ShadowForm();
            UpdateShadow();
        }
        Opacity = 0.05;

        if (useAvatar)
        {
            // Mimics the real AvatarManager_AvatarUpdated pattern: avatar image arrives
            // asynchronously (a background download in the real app), completion marshaled back
            // to the UI thread via Invoke, which then calls Invalidate() -- not a same-thread
            // synchronous draw like the rest of this repro.
            var thread = new Thread(() =>
            {
                Thread.Sleep(180);
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                    {
                        Invoke(() =>
                        {
                            avatarReady = true;
                            Log("avatar ready (cross-thread Invoke), Invalidate()");
                            Invalidate();
                        });
                    }
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }

    private void UpdateShadow()
    {
        if (shadow == null || shadow.IsDisposed) return;
        int w = Width + 2 * ShadowSize, h = Height + 2 * ShadowSize;
        if (shadowBitmap == null || shadowBitmap.Width != w || shadowBitmap.Height != h)
        {
            shadowBitmap?.Dispose();
            shadowBitmap = new Bitmap(w, h);
            using var g = Graphics.FromImage(shadowBitmap);
            g.Clear(Color.FromArgb(120, 0, 0, 0)); // flat semi-transparent grey, stand-in for a real drop-shadow gradient
        }
        byte opacity = (byte)(Opacity * 255.0);
        var pos = new Point(Left - ShadowSize, Top - ShadowSize);
        shadow.Location = pos;
        shadow.UpdateWindow(shadowBitmap, opacity, w, h, pos);
    }

    public new void Show()
    {
        if (useRawShow)
        {
            if (shadow != null && !shadow.IsDisposed)
            {
                Win32.ShowWindow(shadow.Handle, Win32.SW_SHOWNOACTIVATE);
                Win32.SetWindowPos(shadow.Handle, Win32.HWND_TOPMOST, shadow.Left, shadow.Top, shadow.Width, shadow.Height, Win32.SWP_NOACTIVATE);
            }
            Win32.ShowWindow(Handle, Win32.SW_SHOWNOACTIVATE);
            Win32.SetWindowPos(Handle, Win32.HWND_TOPMOST, Left, Top, Width, Height, Win32.SWP_NOACTIVATE);
            Log("raw Show() sequence issued");
        }
        else
        {
            base.Show();
            Log("normal WinForms Show()");
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Show();
        TopMost = true;
        timer.Interval = 25;
        timer.Tick += Timer_Tick;
        timer.Start();
        Log("Load: timer started");
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e)
    {
        Log($"OnPaint state={state} opacity={Opacity:F3}");
        e.Graphics.Clear(BackColor);
        e.Graphics.FillRectangle(Brushes.DarkSlateGray, 0, 0, Width, 24);
        if (useAvatar)
        {
            if (avatarReady)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var avatarBrush = new SolidBrush(Color.FromArgb(80, 140, 200));
                e.Graphics.FillEllipse(avatarBrush, 10, 32, 32, 32);
                TextRenderer.DrawText(e.Graphics, "GA", new Font("Tahoma", 10, FontStyle.Bold), new Rectangle(10, 32, 32, 32), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            TextRenderer.DrawText(e.Graphics, "Test Sender", new Font("Tahoma", 11), new Point(52, 4), Color.White);
            TextRenderer.DrawText(e.Graphics, "Re: Testing notifications repro " + DateTime.Now.ToString("HH:mm:ss.fff"),
                new Font("Tahoma", 10), new Point(52, 40), Color.White);
        }
        else
        {
            TextRenderer.DrawText(e.Graphics, "Test Sender", new Font("Tahoma", 11), new Point(10, 4), Color.White);
            TextRenderer.DrawText(e.Graphics, "Re: Testing notifications repro " + DateTime.Now.ToString("HH:mm:ss.fff"),
                new Font("Tahoma", 10), new Point(10, 40), Color.White);
        }
        base.OnPaint(e);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        switch (state)
        {
            case 0: // Appearing
                Opacity = Math.Min(fullOpacity, Opacity + 0.05);
                if (useShadow) UpdateShadow();
                Invalidate();
                if (Opacity >= fullOpacity)
                {
                    state = 1;
                    visibleSince = DateTime.Now;
                    Log($"-> Visible (opacity={Opacity:F3}), holding {holdMs}ms");
                }
                break;

            case 1: // Visible, holding
                if ((DateTime.Now - visibleSince).TotalMilliseconds >= holdMs)
                {
                    state = 2;
                    Log("-> Disappearing");
                }
                break;

            case 2: // Disappearing
                Opacity = Math.Max(0.0, Opacity - 0.05);
                if (useShadow) UpdateShadow();
                Invalidate();
                if (Opacity <= 0.0)
                {
                    timer.Stop();
                    Log("done, closing");
                    Close();
                }
                break;
        }
    }

    private static void Log(string msg)
    {
        try { File.AppendAllText(LogPath, $"{Environment.TickCount} {msg}\n"); } catch { }
    }
}
