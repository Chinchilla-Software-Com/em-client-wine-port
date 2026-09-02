using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MailClient.Notifications.ButtonOverlay;

// Plain (non-layered) borderless window used to host controls that need to sit visually "inside"
// the notification popup but paint reliably under Wine. The main notification form is itself a
// layered window (AllowTransparency + Opacity, i.e. SetLayeredWindowAttributes) whose own
// WM_PAINT-driven content only actually gets composited to screen during a real message-loop
// tick under Wine -- confirmed, alongside the fix for title/content text, in this project's own
// reports/notification-empty-until-fade-findings.md. This window deliberately has NO
// AllowTransparency/layered involvement at all, so its own repainting goes through the normal
// WM_PAINT path this project's other fixes (e.g. ControlDataGrid's AllowPaintingInWmPaint) already
// show works fine under Wine without needing any tick-driven workaround.
internal sealed class OverlayWindow : Form
{
    public OverlayWindow(Color backColor)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        // Z-order history, confirmed live with a garish debug BackColor each time: TopMost,
        // Control.BringToFront() called synchronously right after Show(), the same
        // BringToFront() deferred to a later tick via BeginInvoke, and the ShowWithoutActivation/
        // WS_EX_NOACTIVATE combination all silently broke painting entirely under Wine (not
        // merely losing focus or z-order -- genuinely never composited again). A raw
        // SetWindowPos(HWND_TOP, SWP_NOACTIVATE) P/Invoke call after Show() -- RaiseViaSetWindowPos
        // below -- is the first one that kept painting working AND visibly moved the icon overlay
        // above the notification in a live test. See reports/notification-empty-until-fade-
        // findings.md for the full history and CLAUDE.md's option 1 (a genuinely layered overlay
        // using UpdateLayeredWindow) as the fallback if this turns out not to be fully reliable.
        BackColor = backColor;
    }

    // Raw P/Invoke instead of Control.BringToFront()/TopMost: those go through WinForms' own
    // wrappers, which may be doing more than a single SetWindowPos call (e.g. BringToFront() also
    // calls SetFocus internally; TopMost persistently toggles the WS_EX_TOPMOST extended style).
    // SWP_NOACTIVATE here is a one-time flag on this single call, not a persistent extended style
    // like WS_EX_NOACTIVATE was -- narrower scope, worth testing in isolation.
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private static readonly IntPtr HWND_TOP = IntPtr.Zero;

    public void RaiseViaSetWindowPos()
    {
        SetWindowPos(Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    // Child controls (the icon PictureBoxes, the reparented button panel) still weren't painting
    // even once the overlay window itself reliably did -- an immediate Refresh() right after
    // Show()+RaiseViaSetWindowPos() didn't help, matching this project's own established pattern
    // (see periodic-reblit in reports/notification-empty-until-fade-findings.md) that a paint
    // forced immediately after a state change can race Wine's compositor before it's caught up.
    // Re-assert both a short delay later via a one-shot timer, same idiom.
    private System.Windows.Forms.Timer? _delayedRepaintTimer;

    public void ScheduleDelayedRepaint()
    {
        _delayedRepaintTimer = new System.Windows.Forms.Timer { Interval = 250 };
        _delayedRepaintTimer.Tick += (_, _) =>
        {
            _delayedRepaintTimer!.Stop();
            RaiseViaSetWindowPos();
            Refresh();
            foreach (Control child in Controls) child.Refresh();
        };
        _delayedRepaintTimer.Start();
    }
}

/// <summary>
/// Single entry point deliberately kept simple to call from the IL patch: all the Rectangle math,
/// Controls.Find lookup, and screen-coordinate translation live here in ordinary, debuggable C#
/// rather than being hand-assembled as IL. The patch only needs to load a handful of existing
/// fields, build two EventHandler delegates, and make one call.
/// </summary>
public static class ButtonOverlayManager
{
    // Bottom is nullable: not every FormGenericNotification subclass has a reply/flag/delete
    // panel (the NewMailsCount case doesn't -- Controls.Find comes back empty for it), but the
    // icon overlay (close/settings) applies to every notification, so Top is always created.
    private static readonly Dictionary<Form, (Form? bottom, Form top)> Overlays = new();
    private static readonly HashSet<Form> DisposalHooked = new();

    /// <summary>
    /// Creates (once per notification form instance, reused across every notification it goes on
    /// to show) or repositions both overlays: the icon overlay always, the reply/flag/delete panel
    /// overlay only if a control named "tableLayoutPanel1" is actually found on `notification`.
    ///
    /// Both stay visible for as long as the notification itself is visible. The original code only
    /// ever drew close/settings while the mouse was over the notification; deliberately simplified
    /// away here (always-visible instead) rather than risk a second layered window interfering
    /// with the existing mouse-enter/leave tracking on the real form, which also drives an
    /// unrelated reshow-on-hover behavior. Revisit if the always-visible icons turn out to be an
    /// unwanted look, not a functional problem.
    ///
    /// closeRectLocal/settingsRectLocal are in the notification form's own client coordinates (the
    /// same coordinate space doLayout() already computes closeRect/settingsRect in).
    /// </summary>
    public static void Sync(
        Form notification,
        Rectangle closeRectLocal, Image closeImage, Image closeImageOver, EventHandler closeClick,
        Rectangle settingsRectLocal, Image settingsImage, Image settingsImageOver, EventHandler settingsClick,
        Color headerBackColor, Color contentBackColor)
    {
        Point screenOrigin = notification.PointToScreen(Point.Empty);
        Rectangle headerLocal = Rectangle.Union(closeRectLocal, settingsRectLocal);
        Rectangle headerScreen = Translate(headerLocal, screenOrigin);

        Overlays.TryGetValue(notification, out (Form? bottom, Form top) pair);

        if (pair.top is null)
        {
            pair.top = CreateIconOverlay(notification, headerLocal, headerScreen, headerBackColor,
                closeRectLocal, closeImage, closeImageOver, closeClick,
                settingsRectLocal, settingsImage, settingsImageOver, settingsClick);
        }
        else
        {
            pair.top.Location = headerScreen.Location;
            pair.top.ClientSize = headerScreen.Size;
        }
        pair.top.Visible = true;

        Control[] found = notification.Controls.Find("tableLayoutPanel1", true);
        if (found.Length > 0)
        {
            Control panel = found[0];
            Rectangle panelScreen = new Rectangle(screenOrigin.X + panel.Left, screenOrigin.Y + panel.Top, panel.Width, panel.Height);
            if (pair.bottom is null)
            {
                pair.bottom = CreateControlOverlay(notification, panel, panelScreen, contentBackColor);
            }
            else
            {
                pair.bottom.Location = panelScreen.Location;
                pair.bottom.ClientSize = panelScreen.Size;
            }
            pair.bottom.Visible = true;
        }

        Overlays[notification] = pair;
        HookDisposal(notification);
    }

    /// <summary>Hides both overlays for this notification (call from Hide()/setFormHidden()).</summary>
    public static void HideAll(Form notification)
    {
        if (Overlays.TryGetValue(notification, out (Form? bottom, Form top) pair))
        {
            if (pair.bottom is not null) pair.bottom.Visible = false;
            pair.top.Visible = false;
        }
    }

    private static void HookDisposal(Form notification)
    {
        if (!DisposalHooked.Add(notification)) return;
        notification.Disposed += (_, _) =>
        {
            if (Overlays.Remove(notification, out (Form? bottom, Form top) pair))
            {
                pair.bottom?.Dispose();
                pair.top.Dispose();
            }
            DisposalHooked.Remove(notification);
        };
    }

    private static Rectangle Translate(Rectangle r, Point screenOrigin) =>
        new Rectangle(screenOrigin.X + r.X, screenOrigin.Y + r.Y, r.Width, r.Height);

    private static Form CreateControlOverlay(Form owner, Control panel, Rectangle screenBounds, Color backColor)
    {
        var overlay = new OverlayWindow(backColor)
        {
            ClientSize = screenBounds.Size,
            Location = screenBounds.Location,
        };
        panel.Parent?.Controls.Remove(panel);
        panel.Location = Point.Empty;
        overlay.Controls.Add(panel);
        overlay.Show(owner);
        overlay.RaiseViaSetWindowPos();
        overlay.Refresh();
        overlay.ScheduleDelayedRepaint();
        return overlay;
    }

    private static Form CreateIconOverlay(
        Form owner, Rectangle localBounds, Rectangle screenBounds, Color backColor,
        Rectangle closeRectLocal, Image closeImage, Image closeImageOver, EventHandler closeClick,
        Rectangle settingsRectLocal, Image settingsImage, Image settingsImageOver, EventHandler settingsClick)
    {
        var overlay = new OverlayWindow(backColor)
        {
            ClientSize = screenBounds.Size,
            Location = screenBounds.Location,
        };
        // closeRectLocal/settingsRectLocal share the same notification-client coordinate space as
        // localBounds (all computed by the same doLayout()) -- translate both into this overlay's
        // own local coordinates by subtracting localBounds' own origin.
        Point offset = new Point(-localBounds.X, -localBounds.Y);
        overlay.Controls.Add(MakeIconButton(Offset(closeRectLocal, offset), closeImage, closeImageOver, closeClick));
        overlay.Controls.Add(MakeIconButton(Offset(settingsRectLocal, offset), settingsImage, settingsImageOver, settingsClick));
        overlay.Show(owner);
        overlay.RaiseViaSetWindowPos();
        overlay.Refresh();
        overlay.ScheduleDelayedRepaint();
        return overlay;
    }

    private static Rectangle Offset(Rectangle r, Point by) => new Rectangle(r.X + by.X, r.Y + by.Y, r.Width, r.Height);

    private static PictureBox MakeIconButton(Rectangle localBounds, Image normal, Image hover, EventHandler click)
    {
        var pb = new PictureBox
        {
            Bounds = localBounds,
            Image = normal,
            SizeMode = PictureBoxSizeMode.CenterImage,
            Cursor = Cursors.Hand,
        };
        pb.MouseEnter += (_, _) => pb.Image = hover;
        pb.MouseLeave += (_, _) => pb.Image = normal;
        pb.Click += click;
        return pb;
    }
}
