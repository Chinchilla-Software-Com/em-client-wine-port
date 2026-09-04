using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

if (args.Length > 0 && args[0] == "--patch")
{
    return RunPatch(args);
}

if (args.Length > 0 && args[0] == "--patch-allpaintinginwmpaint")
{
    return RunPatchAllPaintingInWmPaint(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-force-onload")
{
    return RunPatchNotificationForceOnLoad(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-title-singleline")
{
    return RunPatchNotificationTitleSingleLine(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-toolbar-icons")
{
    return RunPatchNotificationToolbarIcons(args);
}

if (args.Length > 0 && args[0] == "--patch-settings-refresh")
{
    return RunPatchSettingsRefresh(args);
}

if (args.Length > 0 && args[0] == "--patch-default-client-notimpl")
{
    return RunPatchDefaultClientNotImpl(args);
}

if (args.Length > 0 && args[0] == "--dump-handlers")
{
    return RunDumpHandlers(args);
}

if (args.Length > 0 && args[0] == "--dump-il")
{
    return RunDumpIL(args);
}

if (args.Length > 0 && args[0] == "--find-member")
{
    return RunFindMember(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-text-backcolor")
{
    return RunPatchNotificationTextBackColor(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-text-drawstring")
{
    return RunPatchNotificationTextDrawString(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-content-padding")
{
    return RunPatchNotificationContentPadding(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-avatar-title-gap")
{
    return RunPatchNotificationAvatarTitleGap(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-title-vcenter-fix")
{
    return RunPatchNotificationTitleVCenterFix(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-icon-bitmap")
{
    return RunPatchNotificationIconBitmap(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-title-icon-clip")
{
    return RunPatchNotificationTitleIconClip(args);
}

if (args.Length > 0 && args[0] == "--patch-license-icon")
{
    return RunPatchLicenseIcon(args);
}

if (args.Length > 0 && args[0] == "--patch-splash-tip-icon")
{
    return RunPatchSplashTipIcon(args);
}

if (args.Length > 0 && args[0] == "--patch-account-manager-sync-async")
{
    return RunPatchAccountManagerSyncAsync(args);
}

if (args.Length > 0 && args[0] == "--patch-folder-sync-async")
{
    return RunPatchFolderSyncAsync(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-invalidate")
{
    return RunPatchNotificationInvalidate(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-long-burst")
{
    return RunPatchNotificationLongBurst(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-silent-wait")
{
    return RunPatchNotificationSilentWait(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-no-fading")
{
    return RunPatchNotificationNoFading(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-click-resubscribe")
{
    return RunPatchNotificationClickResubscribe(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-hover-forward")
{
    return RunPatchNotificationHoverForward(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-timer-kick")
{
    return RunPatchNotificationTimerKick(args, alsoInvalidate: false, alsoFlipState: false, kickAfterMs: 2000);
}

if (args.Length > 0 && args[0] == "--patch-notification-timer-kick-invalidate")
{
    return RunPatchNotificationTimerKick(args, alsoInvalidate: true, alsoFlipState: false, kickAfterMs: 2000);
}

if (args.Length > 0 && args[0] == "--patch-notification-timer-kick-state-flip")
{
    return RunPatchNotificationTimerKick(args, alsoInvalidate: false, alsoFlipState: true, kickAfterMs: 100);
}

if (args.Length > 0 && args[0] == "--patch-notification-real-fade-abort")
{
    return RunPatchNotificationRealFadeAbort(args, abortOpacity: 1.0);
}

if (args.Length > 0 && args[0] == "--patch-notification-real-fade-abort-0999")
{
    return RunPatchNotificationRealFadeAbort(args, abortOpacity: 0.999);
}

if (args.Length > 0 && args[0] == "--patch-notification-state-vs-alpha")
{
    return RunPatchNotificationStateVsAlpha(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive")
{
    return RunPatchNotificationKeepAlive(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive-v2")
{
    return RunPatchNotificationKeepAliveV2(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive-v3")
{
    return RunPatchNotificationKeepAliveV3(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive-v4")
{
    return RunPatchNotificationKeepAliveV4(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive-v5")
{
    return RunPatchNotificationKeepAliveV5(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-keep-alive-v6")
{
    return RunPatchNotificationKeepAliveV6(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-text-in-bitmap")
{
    return RunPatchNotificationTextInBitmap(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-suppress-self-paint")
{
    return RunPatchNotificationSuppressSelfPaint(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-refresh-on-content-change")
{
    return RunPatchNotificationRefreshOnContentChange(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-periodic-reblit")
{
    return RunPatchNotificationPeriodicReblit(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-suppress-self-text-only")
{
    return RunPatchNotificationSuppressSelfTextOnly(args);
}

if (args.Length > 0 && args[0] == "--version")
{
    return RunVersion(args);
}

if (args.Length > 0 && args[0] == "--check-patched")
{
    return RunCheckPatched(args);
}

return RunScan(args);

// --dump-handlers <dll> <type> <method>
//
// Verification tool, not a patch: dumps a method's exception-handler table in on-disk table
// order with each entry's (TryStart, TryEnd) and (HandlerStart, HandlerEnd) as instruction
// offsets, and flags any pair where one handler's range is nested inside another's but the
// nested one appears AFTER the enclosing one -- the CLR requires most-nested-first order for
// overlapping protected regions, and violating it produces a runtime InvalidProgramException
// that neither Cecil's writer nor ilspycmd's decompiler flags at patch time (hit this for real:
// --patch-default-client-notimpl's first version appended a new inner catch handler to the end
// of the handler list, landing it after the method's outer finally handler -- decompiled clean,
// crashed at runtime). Run this after any patch that adds or moves exception-handler regions.
static int RunDumpHandlers(string[] args)
{
    if (args.Length < 4)
    {
        Console.Error.WriteLine("usage: il-patcher --dump-handlers <dll> <type> <method>");
        return 2;
    }
    string dllPath = args[1], typeName = args[2], methodName = args[3];
    var resolver = new DefaultAssemblyResolver();
    resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath))!);
    using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters { AssemblyResolver = resolver });
    var type = module.GetType(typeName);
    if (type is null) { Console.Error.WriteLine($"type not found: {typeName}"); return 1; }
    var method = type.Methods.FirstOrDefault(m => m.Name == methodName && m.HasBody);
    if (method is null) { Console.Error.WriteLine($"method not found: {typeName}::{methodName}"); return 1; }

    var handlers = method.Body.ExceptionHandlers;
    string Off(Instruction? i) => i is null ? "END" : $"0x{i.Offset:x4}";
    for (int i = 0; i < handlers.Count; i++)
    {
        var h = handlers[i];
        Console.WriteLine($"[{i}] {h.HandlerType,-8} try=({Off(h.TryStart)}-{Off(h.TryEnd)}) handler=({Off(h.HandlerStart)}-{Off(h.HandlerEnd)}) catchType={h.CatchType?.FullName}");
    }

    bool ok = true;
    for (int i = 0; i < handlers.Count; i++)
    {
        for (int j = i + 1; j < handlers.Count; j++)
        {
            var a = handlers[i];
            var b = handlers[j];
            bool aInsideB = a.TryStart!.Offset >= b.TryStart!.Offset &&
                (b.TryEnd is null || (a.TryEnd is not null && a.TryEnd.Offset <= b.TryEnd.Offset)) &&
                !(a.TryStart.Offset == b.TryStart.Offset && a.TryEnd == b.TryEnd); // not siblings on the same try
            if (aInsideB)
            {
                // a is more nested than b, so a must come first (i < j) -- it does, since i<j here.
                continue;
            }
            bool bInsideA = b.TryStart!.Offset >= a.TryStart!.Offset &&
                (a.TryEnd is null || (b.TryEnd is not null && b.TryEnd.Offset <= a.TryEnd.Offset)) &&
                !(a.TryStart.Offset == b.TryStart.Offset && a.TryEnd == b.TryEnd);
            if (bInsideA)
            {
                // b is more nested than a but appears AFTER a (j > i) -- WRONG order.
                Console.Error.WriteLine($"ORDER VIOLATION: handler [{j}] ({b.HandlerType}, try {Off(b.TryStart)}-{Off(b.TryEnd)}) is more nested than [{i}] ({a.HandlerType}, try {Off(a.TryStart)}-{Off(a.TryEnd)}) but appears after it -- must be reordered before it.");
                ok = false;
            }
        }
    }
    Console.WriteLine(ok ? "OK: nesting order looks correct." : "FAIL: nesting order violation(s) found above.");
    return ok ? 0 : 1;
}

// --patch-notification-content-padding <input-dir> <output-dir>
//
// User directly measured this against the real-Windows reference screenshot (pixel-precise, not
// eyeballed) once a genuinely opaque capture was possible (after the empty-box-until-fade fix was
// restored -- see reports/notification-empty-until-fade-findings.md's twenty-first/twenty-second
// rounds): content text sits flush with the avatar's own left edge and right at the header/content
// boundary with no top breathing room, where the reference has a small (~4px, ~13% of the avatar's
// own diameter) left inset past the avatar and a visible top gap. `doLayout()`'s own IL (dumped via
// --dump-il, not guessed from decompiled C#) computes `contentRect` as:
//   new Rectangle(scaledPadding.Left, num + 5, base.Width - scaledPadding.Horizontal, base.Height - num - 10)
// where `num` is the scaled header height. Fix: add 4 to X (and subtract 4 from Width, to keep the
// right edge where it was) for the left inset, and change the Y offset from `+5` to `+11` (and the
// Height's trailing `-10` to `-16`, to keep the bottom edge where it was) for the top padding --
// four small, independently-verified edits within the same Rectangle-construction instruction
// range (bounded by the unique `getScaledPadding()` call and the `stfld contentRect` that follows
// it), not a wholesale rewrite of the expression.
static int RunPatchNotificationContentPadding(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-content-padding <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var doLayoutMethod = type.Methods.FirstOrDefault(m => m.Name == "doLayout" && m.HasBody);
        if (doLayoutMethod is null) { Console.Error.WriteLine("FAIL: doLayout not found"); return 1; }

        var body = doLayoutMethod.Body;
        // Normalize compact-form opcodes (ldc.i4.5, ldc.i4.s 10) to general ldc.i4 <n> up front --
        // both the value-based scan below and IL-patching lesson 4 (this patch inserts new
        // instructions too) need this regardless.
        body.SimplifyMacros();
        var il = body.GetILProcessor();
        var instrs = body.Instructions;

        int paddingCallIdx = -1, contentStfldIdx = -1;
        for (int i = 0; i < instrs.Count; i++)
        {
            if (paddingCallIdx < 0 && instrs[i].OpCode == OpCodes.Call && instrs[i].Operand is MethodReference mr0 && mr0.Name == "getScaledPadding")
            {
                paddingCallIdx = i;
            }
            if (instrs[i].OpCode == OpCodes.Stfld && instrs[i].Operand is FieldReference fr0 && fr0.Name == "contentRect")
            {
                contentStfldIdx = i;
                break;
            }
        }
        if (paddingCallIdx < 0 || contentStfldIdx < 0 || contentStfldIdx <= paddingCallIdx)
        {
            Console.Error.WriteLine("FAIL: couldn't bound the contentRect construction (getScaledPadding()...stfld contentRect) -- method shape changed, review needed");
            return 1;
        }

        // Within [paddingCallIdx, contentStfldIdx), find the four unique sites.
        Instruction? xSite = null, ySite = null, widthSubSite = null, heightSite = null;
        int xCount = 0, yCount = 0, widthCount = 0, heightCount = 0;
        for (int i = paddingCallIdx; i < contentStfldIdx; i++)
        {
            var instr = instrs[i];
            if (instr.OpCode == OpCodes.Call && instr.Operand is MethodReference mrX && mrX.Name == "get_Left" && mrX.DeclaringType.Name == "Padding")
            {
                xSite = instr; xCount++;
            }
            else if (instr.OpCode == OpCodes.Ldc_I4 && instr.Operand is int v5 && v5 == 5)
            {
                ySite = instr; yCount++;
            }
            else if (instr.OpCode == OpCodes.Call && instr.Operand is MethodReference mrH && mrH.Name == "get_Horizontal" && mrH.DeclaringType.Name == "Padding"
                     && i + 1 < contentStfldIdx && instrs[i + 1].OpCode == OpCodes.Sub)
            {
                widthSubSite = instrs[i + 1]; widthCount++;
            }
            else if (instr.OpCode == OpCodes.Ldc_I4 && instr.Operand is int v10 && v10 == 10)
            {
                heightSite = instr; heightCount++;
            }
        }
        if (xCount != 1 || yCount != 1 || widthCount != 1 || heightCount != 1)
        {
            Console.Error.WriteLine($"FAIL: expected exactly 1 of each site within contentRect's construction, found X={xCount} Y={yCount} Width={widthCount} Height={heightCount} -- method shape changed, review needed");
            return 1;
        }

        // Y offset +5 -> +16 (11px more top padding), Height's trailing -10 -> -21 (keep bottom edge).
        //
        // First landed on +11 (6px extra) by eyeballing the reference's proportions. Live-measured
        // afterward (per-row pixel profile, not eyeballed) against the real deployed build: content
        // ink actually starts rendering ~5px higher than the coded contentRect.Y (frame-relative
        // ink onset ~61 vs the coded target's ~66) -- the same "renders higher than its coordinate"
        // bias load-bearing in --patch-notification-title-vcenter-fix's own empirical correction,
        // apparently a broader Wine text-positioning characteristic and not specific to
        // VerticalCenter. Bumped by that measured 5px gap (11 -> 16) rather than re-guessing.
        ySite!.Operand = 16;
        heightSite!.Operand = 21;

        // X: insert `+ 9` right after get_Left(); Width: insert `- 9` right after the existing sub,
        // to keep the right edge where it was. Neither insertion point is a branch target or
        // handler boundary (contentRect's construction is straight-line code within doLayout, per
        // the raw --dump-il read this patch was designed against).
        //
        // First landed on `+4` from measuring the real-Windows reference directly. Live-measured
        // against the real deployed build afterward (per-column pixel scan, cross-checked against
        // the avatar's own logged left edge) and found the same rendering bias
        // --patch-notification-title-vcenter-fix and this patch's own Y-offset correction both hit:
        // content text's ink rendered ~5-6px LEFT of the coded contentRect.X, landing at frame-
        // relative X ~21-22 -- actually left of the avatar's own left edge (X=24), not right of it
        // as intended, despite the field itself correctly reading X=12 in the geometry-diag log.
        // Corrected using the user's own directly-verified method rather than re-deriving a bias
        // constant blind: measured D = how far left of the avatar's edge the ink currently sits
        // (~2.5px), then added 2*D (~5) to the existing `+4` insert -- once to reach the avatar's
        // edge, once more to land the same D as a real inset past it, same idea as the Y-offset
        // correction above but derived from the user's own worked example instead of assumed to
        // transfer from the vertical case unchanged.
        il.InsertAfter(xSite!, Instruction.Create(OpCodes.Add));
        il.InsertAfter(xSite!, Instruction.Create(OpCodes.Ldc_I4, 9));
        il.InsertAfter(widthSubSite!, Instruction.Create(OpCodes.Sub));
        il.InsertAfter(widthSubSite!, Instruction.Create(OpCodes.Ldc_I4, 9));

        Console.WriteLine($"OK   {fileName}: {targetType}::doLayout -- contentRect.X += 9 (left inset past the avatar's own edge; includes a live-measured correction for content text rendering ~5-6px left of its own coded X), contentRect.Y offset +5 -> +16 (top padding, includes an empirically-measured +5 correction for the same Wine text-positioning bias --patch-notification-title-vcenter-fix found), Width/Height trimmed to match so the right/bottom edges don't move");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-avatar-title-gap <input-dir> <output-dir>
//
// Same measurement round as --patch-notification-content-padding (see its own comment): the gap
// between the avatar and the sender-name title is visibly tighter than the real-Windows reference.
// `OnPaintTitle`'s IL (dumped via --dump-il) shows the gap as a single compact-form `ldc.i4.8`
// instruction feeding `ScaleUtils.Scale(this, 8)`, added to both `bounds.X` (past the avatar) and
// subtracted from `bounds.Width` -- unlike a general-form `ldc.i4 <n>`, `ldc.i4.8`'s operand is
// implicit in the opcode itself and can't be edited in place; replace the instruction outright with
// a new `ldc.i4 14` (6px more gap, matching the content-padding fix's own magnitude).
static int RunPatchNotificationAvatarTitleGap(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-avatar-title-gap <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle(PaintEventArgs) not found"); return 1; }

        var il = onPaintTitleMethod.Body.GetILProcessor();
        var instrs = onPaintTitleMethod.Body.Instructions;

        int matchCount = 0;
        Instruction? match = null;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].OpCode == OpCodes.Ldc_I4_8 &&
                instrs[i + 1].OpCode == OpCodes.Call && instrs[i + 1].Operand is MethodReference mr && mr.Name == "Scale")
            {
                match = instrs[i];
                matchCount++;
            }
        }
        if (matchCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 `ldc.i4.8` immediately before a Scale() call in OnPaintTitle, found {matchCount} -- method shape changed, review needed"); return 1; }

        il.Replace(match!, Instruction.Create(OpCodes.Ldc_I4, 14));

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle -- avatar-to-title gap constant 8 -> 14 (6px more space, matching the real-Windows reference)");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-title-vcenter-fix <input-dir> <output-dir>
//
// Same measurement round as the two patches above. Unlike those, this one IS a genuine Wine gap,
// not a design-constant tweak: `OnPaintTitle` already passes `TextFormatFlags.VerticalCenter |
// TextFormatFlags.SingleLine` (added by --patch-notification-title-singleline, MSDN-required
// together) to `TextRendererEx.DrawText`, which for non-emoji text (this title) is confirmed by
// reading `TextRendererEx.DrawTextInternal`'s own decompiled source to fall through to plain
// `System.Windows.Forms.TextRenderer.DrawText` -- so this bounces off the real native Win32
// DrawTextEx, not a custom rendering path. Measured directly against the app's own logged
// `headerRect` (--patch-notification-geometry-diag) on a genuinely opaque capture (only possible
// after the empty-box-until-fade fix was restored): the title's rendered ink sits centered
// ~9px above the header band's true vertical center, not straddling it -- Wine's DrawTextEx isn't
// vertically centering this flag combination correctly.
//
// Fix: stop relying on VerticalCenter under Wine at all. Measure the title's actual rendered
// height via `TextRenderer.MeasureText(title, headerFont, new Size(bounds.Width, int.MaxValue),
// SingleLine | NoPrefix)` (the same native call, just asked what size it would need instead of
// asking it to center), then explicitly set `bounds.Y`/`bounds.Height` so the measured text
// exactly fills bounds -- top-alignment within a rect sized to match the text is mathematically
// identical to true centering, without depending on DrawTextEx's own (here, wrong) centering math.
// Drops VerticalCenter from the final flags constant (34852 -> 34848, i.e. 0x8824 &~ 0x0004) since
// it's now a no-op by construction. All new types/methods (TextRenderer, Size, TextFormatFlags) are
// resolved from references already present in this exact method body or module -- never via
// typeof() reflection on the patching tool's own process (IL-patching lesson 5: TextRenderer lives
// in System.Windows.Forms, a WinForms-shared-framework assembly MailClient.dll references at its
// own version, not necessarily the patching tool's net10.0 one).
static int RunPatchNotificationTitleVCenterFix(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-title-vcenter-fix <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    const int oldFlags = 34852; // EndEllipsis | NoPrefix | SingleLine | VerticalCenter
    const int newFlags = 34848; // - VerticalCenter (now redundant -- bounds is sized to fit exactly)
    const int measureFlags = 2080; // SingleLine (0x20) | NoPrefix (0x800)

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle(PaintEventArgs) not found"); return 1; }
        var titleField = type.Fields.FirstOrDefault(f => f.Name == "title");
        var headerFontField = type.Fields.FirstOrDefault(f => f.Name == "headerFont");
        if (titleField is null || headerFontField is null) { Console.Error.WriteLine("FAIL: title/headerFont field(s) not found"); return 1; }

        var body = onPaintTitleMethod.Body;
        body.SimplifyMacros(); // widen this method's several .s short-form branches up front (lesson 4)
        var il = body.GetILProcessor();
        var instrs = body.Instructions;

        // Find the final `ldc.i4 34852` immediately before the DrawText call (also gives us the
        // TextRendererEx.DrawText MethodReference itself, and from its last parameter, an
        // already-correctly-versioned TextFormatFlags TypeReference -- module.GetType() only finds
        // types DEFINED in this module, not referenced ones, so this is what the file's other
        // patches use instead of typeof() reflection).
        // Captured as an Instruction OBJECT, not an index -- the Emit() insertions below happen
        // earlier in the list and would shift any captured index out from under it (hit for real:
        // the first version of this patch used an index here and ended up overwriting one of the
        // newly-inserted instructions' Operand with the int 34848 instead, an Int32-into-
        // VariableDefinition-slot corruption that only surfaced as a cast exception at write time).
        Instruction? flagsInstr = null;
        MethodReference? drawTextRef = null;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].OpCode == OpCodes.Ldc_I4 && instrs[i].Operand is int v && v == oldFlags &&
                instrs[i + 1].OpCode == OpCodes.Call && instrs[i + 1].Operand is MethodReference mr && mr.Name == "DrawText")
            {
                flagsInstr = instrs[i];
                drawTextRef = mr;
            }
        }
        if (flagsInstr is null || drawTextRef is null) { Console.Error.WriteLine($"FAIL: expected exactly 1 `ldc.i4 {oldFlags}` immediately before a DrawText call in OnPaintTitle -- method shape changed, review needed"); return 1; }
        var textFormatFlagsTypeRef = drawTextRef.Parameters[drawTextRef.Parameters.Count - 1].ParameterType;

        // Ldarg.1(e).callvirt get_Graphics() marks the start of the DrawText call's own argument
        // list -- our new bounds.Y/Height computation must run before it. Also the target of the
        // imageList-null-check branch (per IL-patching lesson 2), so any branch pointing at it must
        // be retargeted to whatever we insert first.
        Instruction? drawTextArgsStart = null;
        int drawTextArgsStartCount = 0;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            // SimplifyMacros() above already converted the original compact Ldarg_1 to the general
            // Ldarg form (operand = the `e` ParameterDefinition, Index 0 since Index excludes the
            // implicit `this`) -- match that, not the macro opcode which no longer appears.
            if (instrs[i].OpCode == OpCodes.Ldarg && instrs[i].Operand is ParameterDefinition pd && pd.Index == 0 &&
                instrs[i + 1].OpCode == OpCodes.Callvirt &&
                instrs[i + 1].Operand is MethodReference mrG && mrG.Name == "get_Graphics")
            {
                drawTextArgsStart = instrs[i];
                drawTextArgsStartCount++;
            }
        }
        if (drawTextArgsStartCount != 1 || drawTextArgsStart is null) { Console.Error.WriteLine($"FAIL: expected exactly 1 Ldarg_1+get_Graphics() pair in OnPaintTitle, found {drawTextArgsStartCount} -- method shape changed, review needed"); return 1; }

        // `bounds` is V_0 (a Rectangle local) -- confirmed via --dump-il: stloc.0 right after
        // loading headerRect at the top of the method, reused (ldloca.s V_0) throughout.
        var boundsLocal = body.Variables[0];
        if (boundsLocal.VariableType.Name != "Rectangle") { Console.Error.WriteLine($"FAIL: expected V_0 to be Rectangle, got {boundsLocal.VariableType.Name} -- method shape changed, review needed"); return 1; }
        var rectangleTypeDef = boundsLocal.VariableType.Resolve();
        if (rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle from V_0's own VariableType"); return 1; }
        var rectGetWidthRef = module.ImportReference(rectangleTypeDef.Methods.First(m => m.Name == "get_Width"));
        var rectGetHeightRef = module.ImportReference(rectangleTypeDef.Methods.First(m => m.Name == "get_Height"));
        var rectSetYRef = module.ImportReference(rectangleTypeDef.Methods.First(m => m.Name == "set_Y" && m.Parameters.Count == 1));
        var rectSetHeightRef = module.ImportReference(rectangleTypeDef.Methods.First(m => m.Name == "set_Height" && m.Parameters.Count == 1));

        // TextRenderer and Size aren't referenced by OnPaintTitle itself -- pull their
        // already-correctly-versioned TypeReferences from the module's own reference table
        // (GetTypeReferences(), a Mono.Cecil.Rocks extension already `using`d in this file) rather
        // than typeof() reflection on the patching tool's own process (lesson 5).
        var textRendererTypeRef = module.GetTypeReferences().FirstOrDefault(t => t.FullName == "System.Windows.Forms.TextRenderer");
        var sizeTypeRef = module.GetTypeReferences().FirstOrDefault(t => t.FullName == "System.Drawing.Size");
        if (textRendererTypeRef is null || sizeTypeRef is null) { Console.Error.WriteLine("FAIL: couldn't find System.Windows.Forms.TextRenderer/System.Drawing.Size in this module's own type references"); return 1; }
        var sizeTypeDef = sizeTypeRef.Resolve();
        if (sizeTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Drawing.Size"); return 1; }
        var sizeCtorRef = module.ImportReference(sizeTypeDef.Methods.First(m => m.Name == ".ctor" && m.Parameters.Count == 2));
        var sizeGetHeightRef = module.ImportReference(sizeTypeDef.Methods.First(m => m.Name == "get_Height"));

        var measureTextRef = new MethodReference("MeasureText", module.ImportReference(sizeTypeRef), module.ImportReference(textRendererTypeRef)) { HasThis = false };
        measureTextRef.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        measureTextRef.Parameters.Add(new ParameterDefinition(headerFontField.FieldType));
        measureTextRef.Parameters.Add(new ParameterDefinition(module.ImportReference(sizeTypeRef)));
        measureTextRef.Parameters.Add(new ParameterDefinition(module.ImportReference(textFormatFlagsTypeRef)));
        var measureTextRefImported = module.ImportReference(measureTextRef);

        var sizeLocal = new VariableDefinition(module.ImportReference(sizeTypeRef));
        body.Variables.Add(sizeLocal);

        var newFirst = Instruction.Create(OpCodes.Ldarg_0);
        il.InsertBefore(drawTextArgsStart!, newFirst);
        void Emit(params Instruction[] ins) { foreach (var i in ins) il.InsertBefore(drawTextArgsStart!, i); }

        // Size measured = TextRenderer.MeasureText(title, headerFont, new Size(bounds.Width, int.MaxValue), measureFlags);
        Emit(
            Instruction.Create(OpCodes.Ldfld, titleField),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, headerFontField),
            Instruction.Create(OpCodes.Ldloca_S, boundsLocal),
            Instruction.Create(OpCodes.Call, rectGetWidthRef),
            Instruction.Create(OpCodes.Ldc_I4, int.MaxValue),
            Instruction.Create(OpCodes.Newobj, sizeCtorRef),
            Instruction.Create(OpCodes.Ldc_I4, measureFlags),
            Instruction.Create(OpCodes.Call, measureTextRefImported),
            Instruction.Create(OpCodes.Stloc, sizeLocal),
            // bounds.Y = (bounds.Height - measured.Height) / 2 + 8;
            //
            // The `+ 8` is an empirically-measured correction, not part of the original plan --
            // live-measured (per-column pixel scan, not eyeballed) after deploying the plain
            // mathematically-centered version: with just the /2 centering, the title's visible ink
            // spanned frame-relative Y 24-34 (window-relative 9-19, center ~14) against the header's
            // true center at window-relative Y=20 -- entirely in the upper half, not straddling
            // center at all. TextRenderer.MeasureText's returned Size.Height is the font's full
            // line-height metric (ascent + descent + internal leading), not the visible glyph span;
            // mathematically centering a box sized to that full metric still visually favors the
            // ascent region for a typical Latin string, because the reserved descender space below
            // the baseline mostly goes unused (this title's few true descenders -- 'g', 'p' -- don't
            // reach anywhere near the metric's full reserve). So `bounds.Y = (Height-measured)/2`
            // was mathematically correct for centering the *box* and still visually wrong for
            // centering the *ink* -- confirmed, not assumed: fixing this "properly" would mean
            // computing centering from the font's actual cap-height-to-baseline span (via
            // FontFamily.GetCellAscent/GetCellDescent design-unit metrics) instead of its full
            // line-height, which risks the exact same Wine-font-metric-reporting gap that caused
            // this in the first place. Simpler and directly verified instead: shift down by the
            // measured gap (target ink-center Y=20 minus observed ink-center Y=14 = 6). A second
            // round measured the x-height letters specifically (the 'o' in "Workshop", not the
            // whole string's ink band, since ascenders like 'J'/'W' skew a whole-band-center measure
            // high relative to what actually reads as "the text" to a human eye) at frame-relative Y
            // 30-37, center ~33.5, against the same true center at frame Y=35 -- needed 1.5-2px
            // more; bumped 6 -> 8 rather than re-deriving from scratch.
            Instruction.Create(OpCodes.Ldloca_S, boundsLocal),
            Instruction.Create(OpCodes.Dup),
            Instruction.Create(OpCodes.Call, rectGetHeightRef),
            Instruction.Create(OpCodes.Ldloca_S, sizeLocal),
            Instruction.Create(OpCodes.Call, sizeGetHeightRef),
            Instruction.Create(OpCodes.Sub),
            Instruction.Create(OpCodes.Ldc_I4_2),
            Instruction.Create(OpCodes.Div),
            Instruction.Create(OpCodes.Ldc_I4_8),
            Instruction.Create(OpCodes.Add),
            Instruction.Create(OpCodes.Call, rectSetYRef),
            // bounds.Height = measured.Height;
            Instruction.Create(OpCodes.Ldloca_S, boundsLocal),
            Instruction.Create(OpCodes.Ldloca_S, sizeLocal),
            Instruction.Create(OpCodes.Call, sizeGetHeightRef),
            Instruction.Create(OpCodes.Call, rectSetHeightRef)
        );

        // Retarget the imageList-null-check branch (the only one pointing at the old first
        // instruction of the DrawText-args sequence) to our new first inserted instruction.
        int retargeted = 0;
        foreach (var instr in instrs)
        {
            if (instr != newFirst && instr.Operand == drawTextArgsStart) { instr.Operand = newFirst; retargeted++; }
        }

        flagsInstr.Operand = newFlags;

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle -- explicitly measures and centers the title text instead of relying on Wine's DrawTextEx VerticalCenter (confirmed off by ~9px on this flag combination); {retargeted} branch(es) retargeted");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-icon-bitmap <input-dir> <output-dir>
//
// Option 1 from the reply/flag/delete/close/settings invisible-until-fade investigation (see
// reports/notification-empty-until-fade-findings.md's twenty-fourth/twenty-fifth rounds): option 2
// (a plain, non-layered overlay window) hit a real, unresolved Wine paint gap for child controls
// after several rounds of otherwise-successful Z-order fixes. This patch instead reuses the SAME
// mechanism that already fixed title/content text -- bake the close/settings icons directly into
// `backgroundBitmap`, which `layeredWindow` already blits reliably via a genuine forced
// `UpdateLayeredWindow` call every time, matching this project's own core finding: reliable
// rendering under Wine means never depending on `this` form's own live, compositor-mediated
// `WM_PAINT` at all. Scoped to close/settings only for this first pass -- they already use plain
// `Image` fields (`closeImage`/`closeImageOver`/`settingsImage`/`settingsImageOver`), no
// `MultiResImageList` resolution needed, unlike reply/flag/delete's real `ControlToolStripButton`
// instances.
//
// Three coordinated edits, all designed from a raw `--dump-il` read (not decompiled C#, per this
// project's own established discipline):
//
// 1. Extend `__drawNotificationTextIntoBitmap` (added by --patch-notification-text-in-bitmap) to
//    also draw the close/settings icons, using the already-correct `mouseOverClose`/
//    `mouseOverSettings` fields to pick the hover-state image -- inserted before the method's
//    existing `graphics.Dispose()` call, which per --dump-il is not itself a branch target (the
//    method's one branch targets an *earlier*, distinct `Ldloc` of the same graphics variable).
// 2. Hook `OnMouseMove`'s two existing `Invalidate()` calls (one for close, one for settings hover
//    changes) to also call `updateLayeredBackground(refreshBitmap: true)` -- forcing the same
//    reliable rebuild-and-blit the text fix already depends on, so a hover-state change actually
//    reaches the screen instead of relying on `this`'s own unreliable repaint.
// 3. Remove the two *direct* `DrawImage(icon, rect)` calls from `OnPaint`'s `if (mouseOver)` block
//    -- the ones that draw straight onto `this`'s own (unshadowed) device context, at a 9px offset
//    from where the shadow-padded bitmap's own copy would land. Left untouched: the two
//    `DrawImage(backgroundBitmap, rect, ...)` calls immediately before them, which *refresh that
//    area from the bitmap* -- now correct and useful (they'll show the bitmap-baked icon, at the
//    right position) instead of redundant, since the bitmap now always has the correct icon baked
//    in. This is the same class of ghost-duplicate risk `--patch-notification-suppress-self-
//    text-only` fixed for text (a live, unshadowed draw landing at a different pixel offset than
//    the shadow-padded bitmap's own copy) -- removing only the offending calls, per that same
//    precedent, rather than the whole block.
static int RunPatchNotificationIconBitmap(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-icon-bitmap <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var closeRectField = type.Fields.FirstOrDefault(f => f.Name == "closeRect");
        var settingsRectField = type.Fields.FirstOrDefault(f => f.Name == "settingsRect");
        var closeImageField = type.Fields.FirstOrDefault(f => f.Name == "closeImage");
        var closeImageOverField = type.Fields.FirstOrDefault(f => f.Name == "closeImageOver");
        var settingsImageField = type.Fields.FirstOrDefault(f => f.Name == "settingsImage");
        var settingsImageOverField = type.Fields.FirstOrDefault(f => f.Name == "settingsImageOver");
        var mouseOverCloseField = type.Fields.FirstOrDefault(f => f.Name == "mouseOverClose");
        var mouseOverSettingsField = type.Fields.FirstOrDefault(f => f.Name == "mouseOverSettings");
        if (closeRectField is null || settingsRectField is null || closeImageField is null || closeImageOverField is null ||
            settingsImageField is null || settingsImageOverField is null || mouseOverCloseField is null || mouseOverSettingsField is null)
        {
            Console.Error.WriteLine("FAIL: one or more required fields not found on FormGenericNotification");
            return 1;
        }

        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground(bool) not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var updateBgMethod = type.Methods.FirstOrDefault(m => m.Name == "updateBackgroundBitmap" && m.HasBody);
        if (updateBgMethod is null) { Console.Error.WriteLine("FAIL: updateBackgroundBitmap not found"); return 1; }
        MethodReference? drawImageRef = null;
        foreach (var instr in updateBgMethod.Body.Instructions)
        {
            if (instr.OpCode == OpCodes.Callvirt && instr.Operand is MethodReference mrDraw && mrDraw.Name == "DrawImage" && mrDraw.Parameters.Count == 2)
            { drawImageRef = mrDraw; break; }
        }
        if (drawImageRef is null) { Console.Error.WriteLine("FAIL: couldn't find Graphics::DrawImage(Image,Rectangle) in updateBackgroundBitmap"); return 1; }
        drawImageRef = module.ImportReference(drawImageRef);

        // === Part 1: extend __drawNotificationTextIntoBitmap ===
        var drawTextMethod = type.Methods.FirstOrDefault(m => m.Name == "__drawNotificationTextIntoBitmap" && m.HasBody);
        if (drawTextMethod is null) { Console.Error.WriteLine("FAIL: __drawNotificationTextIntoBitmap not found (expected from --patch-notification-text-in-bitmap)"); return 1; }
        {
            var body = drawTextMethod.Body;
            body.SimplifyMacros();
            var il = body.GetILProcessor();
            var instrs = body.Instructions;

            Instruction? disposeCall = null;
            foreach (var instr in instrs)
            {
                if (instr.OpCode == OpCodes.Callvirt && instr.Operand is MethodReference mrDispose && mrDispose.Name == "Dispose" && mrDispose.DeclaringType.Name == "Graphics")
                    disposeCall = instr;
            }
            if (disposeCall is null) { Console.Error.WriteLine("FAIL: couldn't find Graphics::Dispose() call in __drawNotificationTextIntoBitmap"); return 1; }
            int disposeIdx = instrs.IndexOf(disposeCall);
            var ldlocGraphics = instrs[disposeIdx - 1];
            if (ldlocGraphics.OpCode != OpCodes.Ldloc || ldlocGraphics.Operand is not VariableDefinition graphicsVar)
            {
                Console.Error.WriteLine($"FAIL: expected Ldloc <graphics var> immediately before Dispose() call, got {ldlocGraphics.OpCode} {ldlocGraphics.Operand}");
                return 1;
            }

            void Emit(Instruction[] ins) { foreach (var i in ins) il.InsertBefore(ldlocGraphics, i); }

            // Guard both icon draws on the relevant Image field being non-null: closeImage/
            // closeImageOver/settingsImage/settingsImageOver are only populated once
            // recolorImages() (called from OnLoad()) has actually run. This method can run before
            // that -- confirmed live: the very first updateBackgroundBitmap() call for a freshly
            // constructed form crashed the whole process (no .NET exception/bug report at all,
            // consistent with an unhandled ArgumentNullException from Graphics.DrawImage(null,...)
            // inside GDI+'s own call stack rather than ordinary managed code) before this guard was
            // added. The original code never hit this because it only ever drew these icons from
            // `this`'s own OnPaint `if (mouseOver)` block, which can't fire before a user has had
            // time to move the mouse over an already-loaded, already-visible form.
            // Nop, not another Ldarg_0: this is purely a branch-target marker for the null-check
            // (both the fall-through-after-drawing path and the skip-because-null path land here),
            // and must have zero stack effect -- an Ldarg_0 here would push an orphaned `this`
            // reference nothing ever consumes, corrupting the stack for everything after it.
            var closeSkipInstr = Instruction.Create(OpCodes.Nop);
            var closeOverInstr = Instruction.Create(OpCodes.Ldarg_0);
            var closeLoadedInstr = Instruction.Create(OpCodes.Ldarg_0);
            Emit(new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, closeImageField),
                Instruction.Create(OpCodes.Brfalse, closeSkipInstr),
                Instruction.Create(OpCodes.Ldloc, graphicsVar),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, mouseOverCloseField),
                Instruction.Create(OpCodes.Brtrue, closeOverInstr),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, closeImageField),
                Instruction.Create(OpCodes.Br, closeLoadedInstr),
                closeOverInstr,
                Instruction.Create(OpCodes.Ldfld, closeImageOverField),
                closeLoadedInstr,
                Instruction.Create(OpCodes.Ldfld, closeRectField),
                Instruction.Create(OpCodes.Callvirt, drawImageRef),
                closeSkipInstr,
            });

            var settingsSkipInstr = Instruction.Create(OpCodes.Nop); // same reasoning as closeSkipInstr above
            var settingsOverInstr = Instruction.Create(OpCodes.Ldarg_0);
            var settingsLoadedInstr = Instruction.Create(OpCodes.Ldarg_0);
            Emit(new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, settingsImageField),
                Instruction.Create(OpCodes.Brfalse, settingsSkipInstr),
                Instruction.Create(OpCodes.Ldloc, graphicsVar),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, mouseOverSettingsField),
                Instruction.Create(OpCodes.Brtrue, settingsOverInstr),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, settingsImageField),
                Instruction.Create(OpCodes.Br, settingsLoadedInstr),
                settingsOverInstr,
                Instruction.Create(OpCodes.Ldfld, settingsImageOverField),
                settingsLoadedInstr,
                Instruction.Create(OpCodes.Ldfld, settingsRectField),
                Instruction.Create(OpCodes.Callvirt, drawImageRef),
                settingsSkipInstr,
            });

            Console.WriteLine($"OK   {fileName}: {targetType}::__drawNotificationTextIntoBitmap -- now also draws the close/settings icons (hover-state-aware) into backgroundBitmap");
        }

        // === Part 2: hook OnMouseMove's two Invalidate() calls ===
        var onMouseMoveMethod = type.Methods.FirstOrDefault(m => m.Name == "OnMouseMove" && m.HasBody);
        if (onMouseMoveMethod is null) { Console.Error.WriteLine("FAIL: OnMouseMove not found"); return 1; }
        {
            var body = onMouseMoveMethod.Body;
            body.SimplifyMacros();
            var il = body.GetILProcessor();
            var instrs = body.Instructions;

            var invalidateCalls = new List<Instruction>();
            foreach (var instr in instrs)
            {
                if (instr.OpCode == OpCodes.Call && instr.Operand is MethodReference mrInv && mrInv.Name == "Invalidate" && mrInv.Parameters.Count == 0)
                    invalidateCalls.Add(instr);
            }
            if (invalidateCalls.Count != 2) { Console.Error.WriteLine($"FAIL: expected exactly 2 Invalidate() calls in OnMouseMove, found {invalidateCalls.Count} -- method shape changed, review needed"); return 1; }

            foreach (var invalidateCall in invalidateCalls)
            {
                var insertionPoint = invalidateCall.Next
                    ?? throw new Exception("OnMouseMove: an Invalidate() call has no following instruction");
                il.InsertBefore(insertionPoint, Instruction.Create(OpCodes.Ldarg_0));
                il.InsertBefore(insertionPoint, Instruction.Create(OpCodes.Ldc_I4_1));
                il.InsertBefore(insertionPoint, Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
            }

            Console.WriteLine($"OK   {fileName}: {targetType}::OnMouseMove -- both hover-state Invalidate() calls now also force updateLayeredBackground(refreshBitmap: true)");
        }

        // === Part 3: remove the two direct DrawImage(icon, rect) calls from OnPaint's mouseOver block ===
        var onPaintMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaint" && m.HasBody && m.Parameters.Count == 1);
        if (onPaintMethod is null) { Console.Error.WriteLine("FAIL: OnPaint(PaintEventArgs) not found"); return 1; }
        {
            var body = onPaintMethod.Body;
            body.SimplifyMacros();
            var il = body.GetILProcessor();
            var instrs = body.Instructions;

            // Each removable block starts at `Ldarg_0; Ldfld mouseOverClose|mouseOverSettings` and
            // ends at the `Callvirt DrawImage(Image,Rectangle)` call that immediately follows the
            // corresponding `Ldfld closeRect|settingsRect`.
            (Instruction start, Instruction end)? FindBlock(FieldDefinition mouseOverField, FieldDefinition rectField)
            {
                for (int i = 1; i < instrs.Count - 1; i++)
                {
                    // Compare by field NAME, not object/reference equality -- FieldDefinition
                    // instances resolved independently (module.GetType(...).Fields.FirstOrDefault)
                    // are not guaranteed to be reference-equal to whatever FieldReference Cecil
                    // attached as an existing instruction's Operand, even for the same field in the
                    // same module. Anchor on the Ldfld itself (index i), not on the `this`-load
                    // before it (index i-1) having a specific opcode -- body.SimplifyMacros() above
                    // converts the original compact Ldarg_0 to the general Ldarg form (operand =
                    // the implicit `this` parameter), so checking OpCodes.Ldarg_0 specifically would
                    // never match post-simplification (hit for real, this exact patch's first
                    // attempt).
                    if (instrs[i].OpCode == OpCodes.Ldfld &&
                        instrs[i].Operand is FieldReference frMouseOver && frMouseOver.Name == mouseOverField.Name)
                    {
                        var blockStart = instrs[i - 1];
                        // scan forward for the matching DrawImage(Image,Rectangle) call, requiring
                        // an Ldfld of rectField to appear immediately before it (the icon-rect
                        // argument), to distinguish this from the earlier
                        // DrawImage(Image,Rectangle,Rectangle,GraphicsUnit) bitmap-refresh call.
                        for (int j = i + 2; j < instrs.Count - 1; j++)
                        {
                            if (instrs[j].OpCode == OpCodes.Ldfld && instrs[j].Operand is FieldReference frRect && frRect.Name == rectField.Name &&
                                instrs[j + 1].OpCode == OpCodes.Callvirt && instrs[j + 1].Operand is MethodReference mrD &&
                                mrD.Name == "DrawImage" && mrD.Parameters.Count == 2)
                            {
                                return (blockStart, instrs[j + 1]);
                            }
                        }
                        return null;
                    }
                }
                return null;
            }

            var closeBlock = FindBlock(mouseOverCloseField, closeRectField);
            var settingsBlock = FindBlock(mouseOverSettingsField, settingsRectField);
            if (closeBlock is null || settingsBlock is null)
            {
                Console.Error.WriteLine($"FAIL: couldn't find both removable icon-draw blocks in OnPaint (close found={closeBlock is not null}, settings found={settingsBlock is not null})");
                return 1;
            }

            void RemoveBlock((Instruction start, Instruction end) block)
            {
                var afterBlock = block.end.Next ?? throw new Exception("OnPaint: a removable block has no following instruction");
                // Per IL-patching lesson 2/3: retarget anything pointing at block.start (the
                // block's own first instruction) to afterBlock, since the block is being removed
                // wholesale. Confirmed via --dump-il that each block's start IS a branch target
                // (the preceding `if (backgroundBitmap != null)` check skips straight to it when
                // backgroundBitmap is null).
                foreach (var instr in instrs)
                {
                    if (instr.Operand == block.start) instr.Operand = afterBlock;
                }
                foreach (var eh in body.ExceptionHandlers)
                {
                    if (eh.TryStart == block.start) eh.TryStart = afterBlock;
                    if (eh.TryEnd == block.start) eh.TryEnd = afterBlock;
                    if (eh.HandlerStart == block.start) eh.HandlerStart = afterBlock;
                    if (eh.HandlerEnd == block.start) eh.HandlerEnd = afterBlock;
                    if (eh.FilterStart == block.start) eh.FilterStart = afterBlock;
                }
                // Remove the whole [start, end] range. Internal branches (this block's own
                // brtrue.s/br.s picking closeImage vs closeImageOver) target instructions strictly
                // inside the range and are removed together with their targets -- nothing outside
                // the range references them.
                var toRemove = new List<Instruction>();
                var cur = block.start;
                while (true)
                {
                    toRemove.Add(cur);
                    if (cur == block.end) break;
                    cur = cur.Next!;
                }
                foreach (var instr in toRemove) il.Remove(instr);
            }

            // Remove settings block first (later in the method) so removing it doesn't invalidate
            // the already-captured close block's own instruction references.
            RemoveBlock(settingsBlock.Value);
            RemoveBlock(closeBlock.Value);

            Console.WriteLine($"OK   {fileName}: {targetType}::OnPaint -- removed the two direct DrawImage(icon, rect) calls from the mouseOver block (the DrawImage(backgroundBitmap, ...) refresh calls immediately before them are unchanged and now correct, since the bitmap has the icon baked in)");
        }

        patched = true;
        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --dump-il <dll> <type> <method> -- generic raw-IL dumper, same rationale as --dump-handlers:
// planning a constant/operand edit against decompiled C# is guesswork about what the compiler
// actually emitted (a `new Rectangle(a, b + 5, c, d - 10)` call could tokenize its constants in any
// order); dumping the real instruction stream removes that guesswork before writing a patch.
static int RunDumpIL(string[] args)
{
    if (args.Length < 4)
    {
        Console.Error.WriteLine("usage: il-patcher --dump-il <dll> <type> <method>");
        return 2;
    }
    string dllPath = args[1], typeName = args[2], methodName = args[3];
    var resolver = new DefaultAssemblyResolver();
    resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath))!);
    using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters { AssemblyResolver = resolver });
    var type = module.GetType(typeName);
    if (type is null) { Console.Error.WriteLine($"type not found: {typeName}"); return 1; }
    var method = type.Methods.FirstOrDefault(m => m.Name == methodName && m.HasBody);
    if (method is null) { Console.Error.WriteLine($"method not found: {typeName}::{methodName}"); return 1; }

    foreach (var instr in method.Body.Instructions)
    {
        Console.WriteLine($"0x{instr.Offset:x4}  {instr.OpCode,-12} {instr.Operand}");
    }
    return 0;
}

// --find-member <dll> <substring> -- searches every type in the assembly for a method, property,
// or field whose name contains <substring> (case-insensitive), printing "DeclaringType::Member".
// For locating an unknown declaring type of a known member name (e.g. a static factory property
// found only via `strings <dll> | grep`, with no type name attached) without guessing type names
// one at a time against ilspycmd -t.
static int RunFindMember(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --find-member <dll> <substring>");
        return 2;
    }
    string dllPath = args[1], substring = args[2];
    var resolver = new DefaultAssemblyResolver();
    resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath))!);
    using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters { AssemblyResolver = resolver });

    int hits = 0;
    foreach (var type in module.GetTypes())
    {
        if (type.Name.Contains(substring, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"type     {type.FullName}" + (type.BaseType is not null ? $" : {type.BaseType.Name}" : "") + (type.Interfaces.Count > 0 ? " implements " + string.Join(",", type.Interfaces.Select(i => i.InterfaceType.Name)) : ""));
            hits++;
        }
        foreach (var m in type.Methods)
        {
            if (m.Name.Contains(substring, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"method   {type.FullName}::{m.Name}({string.Join(",", m.Parameters.Select(p => p.ParameterType.Name))}) -> {m.ReturnType.Name}");
                hits++;
            }
        }
        foreach (var f in type.Fields)
        {
            if (f.Name.Contains(substring, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"field    {type.FullName}::{f.Name} : {f.FieldType.Name}");
                hits++;
            }
        }
        foreach (var p in type.Properties)
        {
            if (p.Name.Contains(substring, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"property {type.FullName}::{p.Name} : {p.PropertyType.Name}");
                hits++;
            }
        }
    }
    Console.WriteLine($"-- {hits} match(es) for \"{substring}\" in {Path.GetFileName(dllPath)}");
    return 0;
}

static int RunScan(string[] args)
{
    string dir = args.Length > 0 ? args[0] : "/home/portagent/project/original";
    var dlls = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
    Array.Sort(dlls);

    int totalHits = 0;

    foreach (var dllPath in dlls)
    {
        ModuleDefinition module;
        try
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dir);
            module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters { AssemblyResolver = resolver });
        }
        catch
        {
            continue; // not a .NET assembly, or unreadable
        }

        List<string> hits = new();

        foreach (var type in module.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;
                var instructions = method.Body.Instructions;
                for (int i = 0; i < instructions.Count; i++)
                {
                    var insn = instructions[i];
                    if (!IsLdcI4(insn, out int value)) continue;

                    // walk forward past any nop/dup/other loads to find the setter call,
                    // matching the simple "graphics.InterpolationMode = X;" pattern:
                    // ldloc/ldarg (graphics), ldc.i4.7, callvirt set_InterpolationMode
                    for (int j = i + 1; j < instructions.Count && j <= i + 3; j++)
                    {
                        var next = instructions[j];
                        if (next.OpCode == OpCodes.Callvirt || next.OpCode == OpCodes.Call)
                        {
                            if (next.Operand is MethodReference mr &&
                                mr.Name == "set_InterpolationMode" &&
                                mr.DeclaringType.FullName == "System.Drawing.Graphics")
                            {
                                string modeName = ModeName(value);
                                hits.Add($"{type.FullName}::{method.Name}  ->  InterpolationMode.{modeName}  [il_offset 0x{insn.Offset:x4}]");
                            }
                            break;
                        }
                        // allow a couple of harmless intervening instructions (dup, ldloc, etc.) but bail on anything
                        // that looks like a different statement boundary
                        if (next.OpCode == OpCodes.Nop || next.OpCode.Name.StartsWith("ldloc") || next.OpCode.Name.StartsWith("ldarg") || next.OpCode.Name.StartsWith("dup"))
                            continue;
                        break;
                    }
                }
            }
        }

        if (hits.Count > 0)
        {
            Console.WriteLine($"=== {Path.GetFileName(dllPath)} ===");
            foreach (var h in hits) Console.WriteLine("  " + h);
            totalHits += hits.Count;
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Total InterpolationMode set-sites found: {totalHits}");
    return 0;
}

// --patch <spec.json> <input-dir> <output-dir>
//
// spec.json: [{ "Assembly": "MailClient.dll", "Type": "MailClient.UI.Forms.FormSplashScreen",
//               "Method": "OnPaintBackground", "IlOffset": "0x001c", "OldValue": 7, "NewValue": 6 }, ...]
//
// For each entry: loads <input-dir>/<Assembly>, finds the ldc.i4 instruction at the exact
// (Type, Method, IlOffset) triple, verifies it currently loads OldValue and is immediately
// followed (within a couple instructions) by a call to Graphics::set_InterpolationMode, then
// rewrites it to load NewValue instead. Aborts that entry (and exits nonzero) if anything doesn't
// match exactly what the spec claims -- this is meant to be paranoid, not clever. Assemblies not
// mentioned in the spec are copied through to <output-dir> untouched so the directory is a
// complete drop-in replacement for <input-dir>.
static int RunPatch(string[] args)
{
    if (args.Length < 4)
    {
        Console.Error.WriteLine("usage: il-patcher --patch <spec.json> <input-dir> <output-dir>");
        return 2;
    }

    string specPath = args[1];
    string inDir = args[2];
    string outDir = args[3];
    Directory.CreateDirectory(outDir);

    var spec = JsonSerializer.Deserialize<List<PatchEntry>>(File.ReadAllText(specPath))
        ?? throw new InvalidDataException("empty or invalid patch spec");

    var byAssembly = spec.GroupBy(e => e.Assembly).ToDictionary(g => g.Key, g => g.ToList());

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    int patchedTotal = 0;
    int failedTotal = 0;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (!byAssembly.TryGetValue(fileName, out var entries))
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        foreach (var entry in entries)
        {
            var type = module.GetType(entry.Type);
            if (type is null)
            {
                Console.Error.WriteLine($"FAIL {fileName}: type not found: {entry.Type}");
                failedTotal++;
                continue;
            }

            var method = type.Methods.FirstOrDefault(m => m.Name == entry.Method && m.HasBody);
            if (method is null)
            {
                Console.Error.WriteLine($"FAIL {fileName}: method not found: {entry.Type}::{entry.Method}");
                failedTotal++;
                continue;
            }

            int wantOffset = Convert.ToInt32(entry.IlOffset, 16);
            var instructions = method.Body.Instructions;
            var insn = instructions.FirstOrDefault(x => x.Offset == wantOffset);
            if (insn is null || !IsLdcI4(insn, out int currentValue))
            {
                Console.Error.WriteLine($"FAIL {fileName}: no ldc.i4 at {entry.Type}::{entry.Method} offset {entry.IlOffset}");
                failedTotal++;
                continue;
            }

            if (currentValue != entry.OldValue)
            {
                Console.Error.WriteLine($"FAIL {fileName}: {entry.Type}::{entry.Method} offset {entry.IlOffset} loads {currentValue} ({ModeName(currentValue)}), expected {entry.OldValue} ({ModeName(entry.OldValue)}) -- refusing to patch");
                failedTotal++;
                continue;
            }

            // confirm it's actually feeding set_InterpolationMode, same walk as the scanner
            bool confirmedSetter = false;
            int idx = instructions.IndexOf(insn);
            for (int j = idx + 1; j < instructions.Count && j <= idx + 3; j++)
            {
                var next = instructions[j];
                if (next.OpCode == OpCodes.Callvirt || next.OpCode == OpCodes.Call)
                {
                    confirmedSetter = next.Operand is MethodReference mr &&
                        mr.Name == "set_InterpolationMode" &&
                        mr.DeclaringType.FullName == "System.Drawing.Graphics";
                    break;
                }
                if (next.OpCode == OpCodes.Nop || next.OpCode.Name.StartsWith("ldloc") || next.OpCode.Name.StartsWith("ldarg") || next.OpCode.Name.StartsWith("dup"))
                    continue;
                break;
            }
            if (!confirmedSetter)
            {
                Console.Error.WriteLine($"FAIL {fileName}: {entry.Type}::{entry.Method} offset {entry.IlOffset} does not feed Graphics::set_InterpolationMode -- refusing to patch");
                failedTotal++;
                continue;
            }

            // rewrite in place: any ldc.i4 form -> the canonical short form for the new value if one
            // exists, else ldc.i4 with an explicit operand. Keeps IL valid without needing to touch
            // surrounding offsets (Cecil recomputes offsets/branches on write).
            insn.OpCode = ShortFormOpCode(entry.NewValue);
            insn.Operand = NeedsOperand(entry.NewValue) ? entry.NewValue : null;

            Console.WriteLine($"OK   {fileName}: {entry.Type}::{entry.Method} offset {entry.IlOffset}  {ModeName(entry.OldValue)} -> {ModeName(entry.NewValue)}");
            patchedTotal++;
        }

        module.Write(destPath);
    }

    Console.WriteLine();
    Console.WriteLine($"Patched {patchedTotal} site(s), {failedTotal} failure(s).");
    return failedTotal > 0 ? 1 : 0;
}

// --patch-allpaintinginwmpaint <input-dir> <output-dir>
//
// One-off, hardcoded experimental patch: MailClient.Common.UI.dll,
// Controls.ControlDataGrid.ControlDataGrid::initialize() sets four ControlStyles flags
// (UserPaint=2, ResizeRedraw=0x10, Selectable=0x200, StandardClick|StandardDoubleClick=0x1100)
// via four back-to-back `ldarg.0; ldc.i4 X; ldc.i4.1; call SetStyle` sequences at the very top
// of the method, but never sets AllPaintingInWmPaint (0x2000) -- the flag WinForms recommends
// for owner-drawn double-buffered controls, whose absence may be why this control's WM_PAINT
// hits a Wine BeginPaint/WM_NCPAINT region-reuse bug that collapses its client clip to empty
// (see reports/settings-panel-clip-region-findings.md). This inserts a fifth
// `ldarg.0; ldc.i4 8192; ldc.i4.1; call SetStyle` sequence immediately after the fourth one,
// reusing the exact same SetStyle MethodReference already in the method -- verifying the exact
// four-call preamble (values 2, 16, 512, 4352 in that order) before touching anything, since
// this is real instruction insertion, not just an operand rewrite, and there's no automatic
// "does this even make sense" check the way the interpolation-mode patcher has.
static int RunPatchAllPaintingInWmPaint(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-allpaintinginwmpaint <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.Common.UI.dll";
    const string targetType = "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid";
    const string targetMethod = "initialize";
    const int allPaintingInWmPaint = 0x2000;
    int[] expectedPreamble = { 2, 16, 512, 4352 };

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null)
        {
            Console.Error.WriteLine($"FAIL: type not found: {targetType}");
            return 1;
        }
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null)
        {
            Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}");
            return 1;
        }

        var il = method.Body.Instructions;
        MethodReference? setStyleRef = null;
        Instruction? insertAfter = null;
        int matched = 0;
        int i = 0;
        while (i + 3 < il.Count && matched < expectedPreamble.Length)
        {
            if (il[i].OpCode == OpCodes.Ldarg_0 &&
                IsLdcI4(il[i + 1], out int value) &&
                value == expectedPreamble[matched] &&
                IsLdcI4(il[i + 2], out int one) && one == 1 &&
                (il[i + 3].OpCode == OpCodes.Call || il[i + 3].OpCode == OpCodes.Callvirt) &&
                il[i + 3].Operand is MethodReference mr &&
                mr.Name == "SetStyle" &&
                mr.DeclaringType.FullName == "System.Windows.Forms.Control")
            {
                setStyleRef = mr;
                insertAfter = il[i + 3];
                matched++;
                i += 4;
            }
            else
            {
                break;
            }
        }

        if (matched != expectedPreamble.Length || setStyleRef is null || insertAfter is null)
        {
            Console.Error.WriteLine($"FAIL: {targetType}::{targetMethod} preamble didn't match expected four SetStyle calls " +
                $"(matched {matched}/{expectedPreamble.Length}) -- refusing to patch, method body may have changed");
            return 1;
        }

        var ilProcessor = method.Body.GetILProcessor();
        // insert in reverse so each InsertAfter(insertAfter, ...) lands right after the call, in order
        var newCall = Instruction.Create(OpCodes.Call, setStyleRef);
        var newTrue = Instruction.Create(OpCodes.Ldc_I4_1);
        var newFlag = Instruction.Create(OpCodes.Ldc_I4, allPaintingInWmPaint);
        var newLdarg = Instruction.Create(OpCodes.Ldarg_0);
        ilProcessor.InsertAfter(insertAfter, newLdarg);
        ilProcessor.InsertAfter(newLdarg, newFlag);
        ilProcessor.InsertAfter(newFlag, newTrue);
        ilProcessor.InsertAfter(newTrue, newCall);

        Console.WriteLine($"OK   {fileName}: {targetType}::{targetMethod} -- inserted SetStyle(AllPaintingInWmPaint, true) after the existing 4-call preamble");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-text-backcolor <input-dir> <output-dir>
//
// Experimental fix for the user's live report: with the theme switched to Light, notification
// title/content text renders visibly heavier/bolder than the same text under Dark theme (same
// font, same size, confirmed via decompile -- `headerFont`/`Font` selection has no theme
// dependency at all: `ScaleUtils.ScaleFont(this, FontCache.CreateFont(FontManager.UIFont.
// FontFamily, 11f))`, no FontStyle argument, so always Regular weight regardless of theme).
// Live-tested directly (via --patch-theme-switcher + --patch-test-monogram-avatar, no screen
// recording needed): Light theme's notification foreground/background pairing is RGB(40,40,40)
// text on white (near-maximum contrast); Dark theme's is white text on a mid-dark gray (lower
// contrast). Both title and content calls currently go through `TextRendererEx.DrawText(Graphics,
// string, Font, Rectangle, Color foreColor, TextFormatFlags)` -- the overload that passes
// `Color.Empty` as backColor internally, which both non-emoji code paths (confirmed by decompile:
// our test strings contain no emoji) forward straight to `System.Windows.Forms.TextRenderer.
// DrawText(g, text, font, bounds, foreColor, backColor, flags)`. Per .NET's own TextRenderer
// implementation, `backColor == Color.Empty` selects a different internal GDI code path (a
// transparent-background render via a memory-DC round-trip) than a real backColor (a direct,
// opaque `ExtTextOut` call) -- plausible root cause: Wine's GDI text-rendering quality differs
// between these two paths, and/or the transparent-background path's own antialiasing blend is
// more sensitive to extreme (near-black-on-white) contrast than the opaque path is.
//
// Fix: pass the ALREADY-KNOWN actual background color explicitly (available on the same
// `IColorTheme` already fetched for the foreground color: `NotificationWindowHeaderStart` for the
// title -- confirmed equal to `...HeaderEnd` in both DefaultColorTheme and DarkColorTheme, i.e.
// effectively solid despite being modeled as a gradient pair -- and `NotificationWindowBackgroundStart`
// for content, same equal-pair situation), forcing the opaque `ExtTextOut` path instead. Routes
// through the EXISTING `TextRendererEx.DrawText(Graphics, string, Font, Rectangle, Color, Color,
// TextFormatFlags)` 7-parameter overload (already present in MailClient.Common.UI.dll, used
// elsewhere in the app -- no new method needed, just a different existing overload and one extra
// argument at each of the two call sites).
//
// ThemeManager/IColorTheme live in MailClient.Common.UI.dll, a different assembly than the one
// this patch edits (MailClient.dll) -- resolved via a second module load + import, same
// cross-assembly pattern as --patch-theme-switcher above (not typeof() reflection -- IL-patching
// lesson 5, these are app-deployed assemblies).
static int RunPatchNotificationTextBackColor(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-text-backcolor <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string commonUiAssembly = "MailClient.Common.UI.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var commonUiPath = Path.Combine(inDir, commonUiAssembly);
        if (!File.Exists(commonUiPath)) { Console.Error.WriteLine($"FAIL: {commonUiAssembly} not found in {inDir}"); return 1; }
        using var commonUiModule = ModuleDefinition.ReadModule(commonUiPath, new ReaderParameters { AssemblyResolver = resolver, ReadWrite = false });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        var onPaintContentMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintContent" && m.HasBody);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle not found"); return 1; }
        if (onPaintContentMethod is null) { Console.Error.WriteLine("FAIL: OnPaintContent not found"); return 1; }

        var themeManagerType = commonUiModule.GetType("MailClient.Common.UI.Themes.ThemeManager");
        var colorThemeType = commonUiModule.GetType("MailClient.Common.UI.Themes.IColorTheme");
        var textRendererExType = commonUiModule.GetType("MailClient.Common.UI.TextRendererEx");
        if (themeManagerType is null || colorThemeType is null || textRendererExType is null)
        {
            Console.Error.WriteLine("FAIL: couldn't resolve ThemeManager/IColorTheme/TextRendererEx in " + commonUiAssembly);
            return 1;
        }
        var themeManagerGetInstanceDef = themeManagerType.Methods.FirstOrDefault(m => m.Name == "get_Instance");
        var getActiveThemeDef = themeManagerType.Methods.FirstOrDefault(m => m.Name == "GetActiveTheme");
        var headerStartGetterDef = colorThemeType.Methods.FirstOrDefault(m => m.Name == "get_NotificationWindowHeaderStart");
        var backgroundStartGetterDef = colorThemeType.Methods.FirstOrDefault(m => m.Name == "get_NotificationWindowBackgroundStart");
        // The 7-arg overload: (Graphics, string, Font, Rectangle, Color, Color, TextFormatFlags).
        var drawText7ArgDef = textRendererExType.Methods.FirstOrDefault(m => m.Name == "DrawText" && m.Parameters.Count == 7 &&
            m.Parameters[1].ParameterType.Name == "String" && m.Parameters[4].ParameterType.Name == "Color" && m.Parameters[5].ParameterType.Name == "Color");
        if (themeManagerGetInstanceDef is null || getActiveThemeDef is null || headerStartGetterDef is null || backgroundStartGetterDef is null || drawText7ArgDef is null)
        {
            Console.Error.WriteLine("FAIL: couldn't resolve one of get_Instance/GetActiveTheme/get_NotificationWindowHeaderStart/get_NotificationWindowBackgroundStart/DrawText(7-arg)");
            return 1;
        }
        var themeManagerGetInstanceRef = module.ImportReference(themeManagerGetInstanceDef);
        var getActiveThemeRef = module.ImportReference(getActiveThemeDef);
        var headerStartGetterRef = module.ImportReference(headerStartGetterDef);
        var backgroundStartGetterRef = module.ImportReference(backgroundStartGetterDef);
        var drawText7ArgRef = module.ImportReference(drawText7ArgDef);

        // Shared logic for both OnPaintTitle and OnPaintContent: find the single `Ldc_I4 <flags>`
        // immediately followed by `Call DrawText(6-arg)`, insert
        // `Call ThemeManager.get_Instance(); Ldarg_0; Callvirt GetActiveTheme(object); Callvirt
        // get_NotificationWindow{Header,Background}Start()` right before the flags constant (so
        // the new backColor argument lands between foreColor and flags, matching the 7-arg
        // overload's parameter order), then retarget the Call to the 7-arg overload.
        void AddBackColorArg(MethodDefinition method, MethodReference backColorGetterRef, string label)
        {
            var body = method.Body;
            var il = body.GetILProcessor();
            var instrs = body.Instructions;

            Instruction? flagsInstr = null;
            Instruction? callInstr = null;
            int matchCount = 0;
            for (int i = 0; i < instrs.Count - 1; i++)
            {
                if (instrs[i].OpCode == OpCodes.Ldc_I4 && instrs[i + 1].OpCode == OpCodes.Call &&
                    instrs[i + 1].Operand is MethodReference mr && mr.Name == "DrawText" && mr.Parameters.Count == 6)
                {
                    flagsInstr = instrs[i];
                    callInstr = instrs[i + 1];
                    matchCount++;
                }
            }
            if (matchCount != 1 || flagsInstr is null || callInstr is null)
            {
                throw new InvalidOperationException($"expected exactly 1 `Ldc_I4 <flags>` immediately before a DrawText(6-arg) call in {label}, found {matchCount} -- method shape changed, review needed");
            }

            il.InsertBefore(flagsInstr, Instruction.Create(OpCodes.Call, themeManagerGetInstanceRef));
            il.InsertBefore(flagsInstr, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(flagsInstr, Instruction.Create(OpCodes.Callvirt, getActiveThemeRef));
            il.InsertBefore(flagsInstr, Instruction.Create(OpCodes.Callvirt, backColorGetterRef));
            callInstr.Operand = drawText7ArgRef;
        }

        try
        {
            AddBackColorArg(onPaintTitleMethod, headerStartGetterRef, "OnPaintTitle");
            AddBackColorArg(onPaintContentMethod, backgroundStartGetterRef, "OnPaintContent");
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle/OnPaintContent -- now pass the theme's actual NotificationWindowHeaderStart/BackgroundStart color as TextRendererEx.DrawText's backColor argument (was Color.Empty), forcing the opaque ExtTextOut GDI path instead of the transparent memory-DC blend path");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-text-drawstring <input-dir> <output-dir>
//
// Second fix attempt for the same Light-theme-only bold-text bug --patch-notification-text-
// backcolor tried and crashed on (see that patch's own doc comment; do not retry its approach).
// Root cause theory, now better understood: `TextRendererEx.DrawText`/`TextRenderer.DrawText`'s
// no-backColor overload draws in GDI's "transparent background" mode, which needs to read back
// the REAL destination pixels to blend anti-aliased glyph edges correctly. That works fine
// against a live on-screen window (confirmed: the main mail list -- also near-black-on-white,
// same high contrast -- renders with completely normal weight in Light theme) but our
// notification text is drawn into an OFFSCREEN `Bitmap` (`Graphics.FromImage(backgroundBitmap)`,
// needed for the reliable `UpdateLayeredWindow` blit this whole project's fix chain depends on).
// Plausible Wine gap: when GDI can't do a real read-back against that kind of surface, it likely
// falls back to assuming a BLACK background for the anti-aliasing blend math regardless of what's
// actually there. Dark theme's real background is close enough to black that the wrong
// assumption barely shows; Light theme's white background is about as far from black as
// possible, so the miscalibrated edges render far darker/heavier than intended -- visually
// indistinguishable from bold. This explains every piece of observed evidence: theme-dependence,
// notification-specificity, and why the main list (a live window, not a bitmap) is unaffected.
//
// Fix: stop using GDI's `TextRenderer`/`TextRendererEx` for this specific offscreen-bitmap
// drawing entirely, in favor of GDI+'s `Graphics.DrawString` -- GDI+ does its own alpha
// compositing directly against the in-memory pixel buffer it already has, with no native
// screen-read-back trick involved at all, so this Wine gap has nothing to attach to. Deliberately
// narrow in scope: only the FINAL draw call in `OnPaintTitle`/`OnPaintContent` is replaced (both
// simple, branch-free straight-line tails, confirmed via --dump-il and a full-method branch-
// target scan before removal) -- all upstream layout/measurement logic (bounds computation,
// vertical centering, icon-width allowance) is left completely untouched, still driven by the
// same `TextRenderer.MeasureText`-based `bounds` this project's earlier padding/centering fixes
// already tuned, to avoid re-opening that already-carefully-measured work. `StringFormat` is
// configured to approximate the original `TextFormatFlags` as closely as GDI+ allows: `NoWrap`
// for the title (matching `SingleLine`) and default (wrapping allowed) for content (matching
// `WordBreak`), `Trimming = EllipsisCharacter` for both (matching `EndEllipsis`) -- GDI+'s own
// metrics differ slightly from GDI's (a long-documented historical discrepancy between
// `Graphics.DrawString` and `TextRenderer.DrawText`), so a fresh visual re-check of wrapping/
// truncation/positioning against both themes is still needed after this deploys, not just the
// boldness itself.
//
// `Graphics`/`SolidBrush`/`StringFormat` live in `System.Drawing.Common.dll`;
// `Rectangle`/`RectangleF` live in `System.Drawing.Primitives.dll` -- both app-deployed
// assemblies (IL-patching lesson 5), resolved here from types ALREADY referenced in the method
// being patched (the existing `Callvirt get_Graphics()`'s return type; the existing
// `headerRect`/`contentRect` fields' own field types) rather than typeof() reflection on
// il-patcher's own .NET 10 process.
static int RunPatchNotificationTextDrawString(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-text-drawstring <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        var onPaintContentMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintContent" && m.HasBody && m.DeclaringType == type);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle not found"); return 1; }
        if (onPaintContentMethod is null) { Console.Error.WriteLine("FAIL: OnPaintContent not found (base class one, not FormIMStatusNotification's override)"); return 1; }

        var titleField = type.Fields.FirstOrDefault(f => f.Name == "title");
        var headerFontField = type.Fields.FirstOrDefault(f => f.Name == "headerFont");
        var contentField = type.Fields.FirstOrDefault(f => f.Name == "content");
        var contentRectField = type.Fields.FirstOrDefault(f => f.Name == "contentRect");
        var headerRectField = type.Fields.FirstOrDefault(f => f.Name == "headerRect");
        if (titleField is null || headerFontField is null || contentField is null || contentRectField is null || headerRectField is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find title/headerFont/content/contentRect/headerRect field(s)");
            return 1;
        }

        var rectangleTypeDef = headerRectField.FieldType.Resolve();
        if (rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle from headerRect's own FieldType"); return 1; }
        var drawingPrimitivesModule = rectangleTypeDef.Module;
        var rectangleFTypeDef = drawingPrimitivesModule.GetType("System.Drawing.RectangleF");
        if (rectangleFTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Drawing.RectangleF"); return 1; }
        var rectToRectFOpDef = rectangleFTypeDef.Methods.FirstOrDefault(m => m.Name == "op_Implicit" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Drawing.Rectangle");
        if (rectToRectFOpDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve RectangleF.op_Implicit(Rectangle)"); return 1; }
        var rectToRectFOpRef = module.ImportReference(rectToRectFOpDef);
        // Title needs a RectangleF built with EXTRA height beyond the tightly-measured single-line
        // bounds (see the call site below for why -- GDI+'s ellipsis trimming needs headroom to
        // operate on width alone without an incidental vertical constraint interfering), so build
        // it via RectangleF's own 4-float constructor from the original Rectangle's X/Y/Width plus
        // a widened height, rather than a straight op_Implicit conversion.
        var rectGetXDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "get_X");
        var rectGetYDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Y");
        var rectGetWidthDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Width");
        var rectGetHeightDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Height");
        var rectangleFCtor4Def = rectangleFTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 4);
        if (rectGetXDef is null || rectGetYDef is null || rectGetWidthDef is null || rectGetHeightDef is null || rectangleFCtor4Def is null)
        {
            Console.Error.WriteLine("FAIL: couldn't resolve Rectangle.get_X/get_Y/get_Width/get_Height or RectangleF(float,float,float,float)");
            return 1;
        }
        var rectGetXRef = module.ImportReference(rectGetXDef);
        var rectGetYRef = module.ImportReference(rectGetYDef);
        var rectGetWidthRef = module.ImportReference(rectGetWidthDef);
        var rectGetHeightRef = module.ImportReference(rectGetHeightDef);
        var rectangleFCtor4Ref = module.ImportReference(rectangleFCtor4Def);

        // Graphics/SolidBrush/StringFormat all live in System.Drawing.Common.dll -- resolve via
        // the existing PaintEventArgs::get_Graphics() call already present in OnPaintTitle's own
        // body, whose return type IS Graphics, rather than a fresh typeof() reflection.
        var getGraphicsCallInTitle = onPaintTitleMethod.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr && mr.Name == "get_Graphics");
        if (getGraphicsCallInTitle is null) { Console.Error.WriteLine("FAIL: couldn't find PaintEventArgs::get_Graphics() call in OnPaintTitle"); return 1; }
        var getGraphicsMethodRef = (MethodReference)getGraphicsCallInTitle.Operand;
        var graphicsTypeDef = getGraphicsMethodRef.ReturnType.Resolve();
        if (graphicsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics type"); return 1; }
        var drawingCommonModule = graphicsTypeDef.Module;
        var solidBrushTypeDef = drawingCommonModule.GetType("System.Drawing.SolidBrush");
        var stringFormatTypeDef = drawingCommonModule.GetType("System.Drawing.StringFormat");
        if (solidBrushTypeDef is null || stringFormatTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve SolidBrush/StringFormat"); return 1; }
        var solidBrushCtorDef = solidBrushTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 1);
        // Match the parameter type by name explicitly -- StringFormat also has an internal
        // single-parameter constructor taking a native GpStringFormat* pointer (interop plumbing),
        // and a plain `Parameters.Count == 1` filter picked that one up instead of the public
        // StringFormat(StringFormatFlags) constructor the first time this was written, confirmed
        // via decompile showing `new StringFormat((GpStringFormat*)16384)` -- garbage that would
        // have crashed the instant it ran.
        var stringFormatCtorDef = stringFormatTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "StringFormatFlags");
        var setTrimmingDef = stringFormatTypeDef.Methods.FirstOrDefault(m => m.Name == "set_Trimming");
        if (solidBrushCtorDef is null || stringFormatCtorDef is null || setTrimmingDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve SolidBrush(Color)/StringFormat(StringFormatFlags)/set_Trimming"); return 1; }
        var solidBrushCtorRef = module.ImportReference(solidBrushCtorDef);
        var stringFormatCtorRef = module.ImportReference(stringFormatCtorDef);
        var setTrimmingRef = module.ImportReference(setTrimmingDef);

        var drawStringDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "DrawString" && m.Parameters.Count == 5 &&
            m.Parameters[0].ParameterType.Name == "String" && m.Parameters[2].ParameterType.Name == "Brush" &&
            m.Parameters[3].ParameterType.Name == "RectangleF" && m.Parameters[4].ParameterType.Name == "StringFormat");
        if (drawStringDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics.DrawString(string, Font, Brush, RectangleF, StringFormat)"); return 1; }
        var drawStringRef = module.ImportReference(drawStringDef);

        // For the title's manual ellipsis-truncation helper (see its own doc comment below):
        // TextRenderer.MeasureText and Size are both app-deployed (System.Windows.Forms.dll /
        // System.Drawing.Primitives.dll -- IL-patching lesson 5), resolved from the EXISTING
        // MeasureText call already present in OnPaintTitle's own (untouched-by-this-patch) bounds
        // computation, rather than typeof() reflection.
        var measureTextCall = onPaintTitleMethod.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr && mr.Name == "MeasureText" && mr.Parameters.Count == 4);
        if (measureTextCall is null) { Console.Error.WriteLine("FAIL: couldn't find TextRenderer.MeasureText(string,Font,Size,TextFormatFlags) call in OnPaintTitle"); return 1; }
        var measureTextRef = (MethodReference)measureTextCall.Operand;
        var sizeTypeRef = measureTextRef.ReturnType;
        var sizeTypeDef = sizeTypeRef.Resolve();
        if (sizeTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Size type from MeasureText's return type"); return 1; }
        var sizeCtorDef = sizeTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        var sizeGetWidthDef = sizeTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Width");
        if (sizeCtorDef is null || sizeGetWidthDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Size(int,int)/get_Width"); return 1; }
        var sizeCtorRef = module.ImportReference(sizeCtorDef);
        var sizeGetWidthRef = module.ImportReference(sizeGetWidthDef);
        // The TextFormatFlags combination this exact call already uses (NoPrefix | SingleLine) --
        // captured as a literal from the existing instruction, confirmed via --dump-il to be the
        // constant 2080, rather than re-deriving/guessing the combined flags value by hand.
        var measureTextFlagsInstr = measureTextCall.Previous;
        if (measureTextFlagsInstr is null || measureTextFlagsInstr.OpCode != OpCodes.Ldc_I4) { Console.Error.WriteLine("FAIL: expected Ldc_I4 (TextFormatFlags) immediately before MeasureText call"); return 1; }
        int measureTextFlagsValue = (int)measureTextFlagsInstr.Operand;

        var stringTypeDef = module.TypeSystem.String.Resolve();
        var isNullOrEmptyRef = module.ImportReference(typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) })!);
        var stringConcat2Ref = module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        var stringGetLengthRef = module.ImportReference(typeof(string).GetProperty("Length")!.GetGetMethod()!);
        var stringSubstringRef = module.ImportReference(typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) })!);

        // New private static helper: TruncateWithEllipsis(string text, Font font, int maxWidth).
        // GDI+'s own StringFormat.Trimming (EllipsisCharacter) turned out not to work at all under
        // Wine -- confirmed live: the title just cut off mid-word with no "..." shown whatsoever,
        // even with Trimming explicitly set and ample RectangleF headroom (see the Y-offset fix
        // above for the headroom experiment this was tested alongside) -- plausibly another
        // instance of this whole project's recurring theme (Wine's gdiplus not fully implementing
        // a GDI+ feature, same shape as the InterpolationMode gaps found much earlier). Worked
        // around by measuring and truncating the string manually in code, using the SAME
        // TextRenderer.MeasureText already used for layout (GDI-based measurement, unaffected by
        // the unrelated GDI-vs-GDI+ RENDERING bug this whole DrawString migration exists to avoid
        // -- measuring text doesn't render pixels, so it was never part of the boldness problem),
        // then passing the already-fits literal string to DrawString instead of relying on its own
        // trimming. Linear character-by-character shrink (not a binary search) -- simpler to get
        // right in hand-written IL, and titles are short enough that a few dozen iterations worst
        // case is irrelevant for a once-per-paint cost.
        var truncateHelper = new MethodDefinition("__truncateWithEllipsis", MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.String);
        truncateHelper.Parameters.Add(new ParameterDefinition("text", ParameterAttributes.None, module.TypeSystem.String));
        var fontTypeRef = headerFontField.FieldType;
        truncateHelper.Parameters.Add(new ParameterDefinition("font", ParameterAttributes.None, fontTypeRef));
        truncateHelper.Parameters.Add(new ParameterDefinition("maxWidth", ParameterAttributes.None, module.TypeSystem.Int32));
        type.Methods.Add(truncateHelper);
        {
            var thBody = truncateHelper.Body;
            thBody.InitLocals = true;
            var fullSizeLocal = new VariableDefinition(sizeTypeRef);
            var sLocal = new VariableDefinition(module.TypeSystem.String);
            var candidateLocal = new VariableDefinition(module.TypeSystem.String);
            var szLocal = new VariableDefinition(sizeTypeRef);
            thBody.Variables.Add(fullSizeLocal);
            thBody.Variables.Add(sLocal);
            thBody.Variables.Add(candidateLocal);
            thBody.Variables.Add(szLocal);
            var il = thBody.GetILProcessor();

            var retEllipsisOnly = Instruction.Create(OpCodes.Ldstr, "…");
            var retTextUnchanged = Instruction.Create(OpCodes.Ldarg_0);
            var loopTop = Instruction.Create(OpCodes.Ldloc, sLocal);

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Call, isNullOrEmptyRef));
            il.Append(Instruction.Create(OpCodes.Brtrue, retTextUnchanged));

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, int.MaxValue));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, int.MaxValue));
            il.Append(Instruction.Create(OpCodes.Newobj, sizeCtorRef));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, measureTextFlagsValue));
            il.Append(Instruction.Create(OpCodes.Call, measureTextRef));
            il.Append(Instruction.Create(OpCodes.Stloc, fullSizeLocal));

            il.Append(Instruction.Create(OpCodes.Ldloca, fullSizeLocal));
            il.Append(Instruction.Create(OpCodes.Call, sizeGetWidthRef));
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Ble, retTextUnchanged));

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Stloc, sLocal));

            il.Append(loopTop);
            il.Append(Instruction.Create(OpCodes.Callvirt, stringGetLengthRef));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Ble, retEllipsisOnly));

            il.Append(Instruction.Create(OpCodes.Ldloc, sLocal));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Ldloc, sLocal));
            il.Append(Instruction.Create(OpCodes.Callvirt, stringGetLengthRef));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            il.Append(Instruction.Create(OpCodes.Sub));
            il.Append(Instruction.Create(OpCodes.Callvirt, stringSubstringRef));
            il.Append(Instruction.Create(OpCodes.Stloc, sLocal));

            il.Append(Instruction.Create(OpCodes.Ldloc, sLocal));
            il.Append(Instruction.Create(OpCodes.Ldstr, "…"));
            il.Append(Instruction.Create(OpCodes.Call, stringConcat2Ref));
            il.Append(Instruction.Create(OpCodes.Stloc, candidateLocal));

            il.Append(Instruction.Create(OpCodes.Ldloc, candidateLocal));
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, int.MaxValue));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, int.MaxValue));
            il.Append(Instruction.Create(OpCodes.Newobj, sizeCtorRef));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, measureTextFlagsValue));
            il.Append(Instruction.Create(OpCodes.Call, measureTextRef));
            il.Append(Instruction.Create(OpCodes.Stloc, szLocal));

            il.Append(Instruction.Create(OpCodes.Ldloca, szLocal));
            il.Append(Instruction.Create(OpCodes.Call, sizeGetWidthRef));
            il.Append(Instruction.Create(OpCodes.Ldarg_2));
            il.Append(Instruction.Create(OpCodes.Bgt, loopTop));

            il.Append(Instruction.Create(OpCodes.Ldloc, candidateLocal));
            il.Append(Instruction.Create(OpCodes.Ret));

            il.Append(retEllipsisOnly);
            il.Append(Instruction.Create(OpCodes.Ret));

            il.Append(retTextUnchanged);
            il.Append(Instruction.Create(OpCodes.Ret));
        }
        var truncateHelperRef = truncateHelper;

        // Title's foreColor comes from ThemeManager.Instance.GetActiveTheme(this).
        // NotificationWindowHeaderForeground -- capture the exact MethodReferences the EXISTING
        // call chain in OnPaintTitle already uses, before that chain's instructions are removed.
        var titleColorChainCalls = onPaintTitleMethod.Body.Instructions
            .Where(i => i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
            .Select(i => i.Operand as MethodReference)
            .Where(mr => mr is not null && (mr.Name == "get_Instance" || mr.Name == "GetActiveTheme" || mr.Name == "get_NotificationWindowHeaderForeground"))
            .ToList();
        var themeGetInstanceRef = titleColorChainCalls.FirstOrDefault(mr => mr!.Name == "get_Instance");
        var getActiveThemeRef = titleColorChainCalls.FirstOrDefault(mr => mr!.Name == "GetActiveTheme");
        var headerForegroundGetterRef = titleColorChainCalls.FirstOrDefault(mr => mr!.Name == "get_NotificationWindowHeaderForeground");
        if (themeGetInstanceRef is null || getActiveThemeRef is null || headerForegroundGetterRef is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find the existing ThemeManager.Instance.GetActiveTheme(this).NotificationWindowHeaderForeground call chain in OnPaintTitle");
            return 1;
        }

        // Content's foreColor/font come from Control.get_ForeColor()/get_Font() -- capture from
        // OnPaintContent's own existing calls the same way.
        var contentColorFontCalls = onPaintContentMethod.Body.Instructions
            .Where(i => i.OpCode == OpCodes.Callvirt)
            .Select(i => i.Operand as MethodReference)
            .Where(mr => mr is not null && (mr.Name == "get_ForeColor" || mr.Name == "get_Font"))
            .ToList();
        var controlGetForeColorRef = contentColorFontCalls.FirstOrDefault(mr => mr!.Name == "get_ForeColor");
        var controlGetFontRef = contentColorFontCalls.FirstOrDefault(mr => mr!.Name == "get_Font");
        if (controlGetForeColorRef is null || controlGetFontRef is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find Control.get_ForeColor()/get_Font() calls in OnPaintContent");
            return 1;
        }

        // OnPaintTitle stashes its computed bounds in a local Rectangle variable (V_0, confirmed
        // via --dump-il) reused across the whole method -- capture it now, before the tail using
        // it gets removed (removing instructions doesn't remove the local itself from
        // body.Variables, but grab it up front for clarity).
        var titleBoundsLocal = onPaintTitleMethod.Body.Variables[0];

        // Verified directly against System.Drawing.Common.dll's own enum definitions (--dump -t)
        // rather than trusted from memory, after the first draft got both wrong: NoWrap is
        // 0x1000 (0x4000 is actually NoClip), and EllipsisCharacter is 3 (5 is EllipsisPath).
        const int stringFormatFlagsNoWrap = 0x1000; // System.Drawing.StringFormatFlags.NoWrap
        const int stringFormatFlagsLineLimit = 0x2000; // System.Drawing.StringFormatFlags.LineLimit
        const int stringTrimmingEllipsisCharacter = 3; // System.Drawing.StringTrimming.EllipsisCharacter
        const float titleRectExtraHeight = 40f; // generous headroom, see call site below

        // Replaces the tail of `method` -- from the `Ldarg e` immediately preceding its
        // `Callvirt PaintEventArgs::get_Graphics()` call through the final `Ret` -- with a
        // DrawString-based sequence built from the caller-supplied instruction lists for pushing
        // the text/font/color/rect arguments (the parts that differ between OnPaintTitle and
        // OnPaintContent), keeping everything before that point (bounds computation, etc)
        // untouched.
        void ReplaceDrawCall(MethodDefinition method, string label, List<Instruction> pushText, List<Instruction> pushFont,
            List<Instruction> pushColor, List<Instruction> pushRectF, int formatFlags)
        {
            var body = method.Body;
            var il = body.GetILProcessor();
            var instrs = body.Instructions;

            var getGraphicsCalls = instrs.Where(i => i.Operand is MethodReference mr && mr.Name == "get_Graphics").ToList();
            if (getGraphicsCalls.Count != 1) throw new InvalidOperationException($"{label}: expected exactly 1 get_Graphics() call, found {getGraphicsCalls.Count}");
            var getGraphicsCall = getGraphicsCalls[0];
            var graphicsGetterRef = (MethodReference)getGraphicsCall.Operand;
            var rangeStart = getGraphicsCall.Previous; // the Ldarg pushing `e`
            if (rangeStart is null) throw new InvalidOperationException($"{label}: get_Graphics() call has no preceding instruction");
            var lastRet = instrs.Last(i => i.OpCode == OpCodes.Ret);

            // Confirm nothing in the whole method (or its exception handlers, though these two
            // methods have none) branches into the range about to be removed (IL-patching lesson
            // 2) -- collect every instruction between rangeStart and lastRet inclusive, then scan
            // ALL instructions' Operand for a reference to any of them.
            var toRemove = new List<Instruction>();
            for (var i = rangeStart; i is not null; i = i.Next)
            {
                toRemove.Add(i);
                if (ReferenceEquals(i, lastRet)) break;
            }
            var toRemoveSet = new HashSet<Instruction>(toRemove);
            int externalRefs = instrs.Count(i => !toRemoveSet.Contains(i) && i.Operand is Instruction target && toRemoveSet.Contains(target));
            if (externalRefs != 0) throw new InvalidOperationException($"{label}: {externalRefs} instruction(s) outside the removed range branch into it -- not safe to remove, review needed");

            foreach (var i in toRemove) il.Remove(i);

            void Emit(IEnumerable<Instruction> group) { foreach (var i in group) il.Append(i); }

            il.Append(Instruction.Create(OpCodes.Ldarg_1)); // `e` is always parameter index 1 on these two methods
            il.Append(Instruction.Create(OpCodes.Callvirt, graphicsGetterRef));
            Emit(pushText);
            Emit(pushFont);
            Emit(pushColor);
            il.Append(Instruction.Create(OpCodes.Newobj, solidBrushCtorRef));
            Emit(pushRectF);
            il.Append(Instruction.Create(OpCodes.Ldc_I4, formatFlags));
            il.Append(Instruction.Create(OpCodes.Newobj, stringFormatCtorRef));
            il.Append(Instruction.Create(OpCodes.Dup));
            il.Append(Instruction.Create(OpCodes.Ldc_I4, stringTrimmingEllipsisCharacter));
            il.Append(Instruction.Create(OpCodes.Callvirt, setTrimmingRef));
            il.Append(Instruction.Create(OpCodes.Callvirt, drawStringRef));
            il.Append(Instruction.Create(OpCodes.Ret));
        }

        try
        {
            // Title's RectangleF is built from the ALREADY-tightly-measured `bounds` local's own
            // X/Y/Width (unchanged -- the horizontal ellipsis-clip position this project's own
            // padding/centering fixes already tuned), but with a widened, generous Height instead
            // of the tightly-fit `size.Height` the local actually holds. First deploy attempt used
            // the local's real (single-line-tight) height via a straight Rectangle->RectangleF
            // conversion and the title rendered with NO ellipsis at all -- just an abrupt mid-word
            // cutoff -- confirmed live via screenshot. Root cause theory: GDI+'s EllipsisCharacter
            // trimming logic needs vertical headroom to evaluate independently of the WIDTH-based
            // trim decision it's actually meant to make; a rect exactly one line tall apparently
            // isn't enough for it to even attempt the trim. DrawString's default LineAlignment
            // (Near, i.e. top-aligned within the rect) means adding height below the already-
            // correct Y doesn't move the rendered line at all, so this is safe.
            ReplaceDrawCall(onPaintTitleMethod, "OnPaintTitle",
                pushText: new List<Instruction> {
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, titleField),
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, headerFontField),
                    Instruction.Create(OpCodes.Ldloca, titleBoundsLocal), Instruction.Create(OpCodes.Call, rectGetWidthRef),
                    Instruction.Create(OpCodes.Call, truncateHelperRef)
                },
                pushFont: new List<Instruction> { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, headerFontField) },
                pushColor: new List<Instruction> {
                    Instruction.Create(OpCodes.Call, themeGetInstanceRef),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Callvirt, getActiveThemeRef),
                    Instruction.Create(OpCodes.Callvirt, headerForegroundGetterRef)
                },
                pushRectF: new List<Instruction> {
                    // -8: DrawString renders title text ~8px further right than TextRenderer did
                    // for the same nominal X -- confirmed by precise pixel measurement the same
                    // way as the Y-offset below (avatar-diameter-normalized scale, first-non-
                    // background-pixel detection on the same "J" glyph in both captures): title
                    // text left edge sat 149px right of the avatar's own left edge in the new
                    // DrawString capture vs 124px in the old TextRenderer reference, a
                    // 25px-at-3x-scale = ~8px-native difference. Same root cause as the Y offset:
                    // GDI+ includes its own left-side glyph bearing that GDI's TextRenderer
                    // compensates for internally by default; DrawString does not.
                    Instruction.Create(OpCodes.Ldloca, titleBoundsLocal), Instruction.Create(OpCodes.Call, rectGetXRef),
                    Instruction.Create(OpCodes.Ldc_I4, 8), Instruction.Create(OpCodes.Sub), Instruction.Create(OpCodes.Conv_R4),
                    // -9: DrawString renders ~9px lower than TextRenderer.DrawText did for the
                    // same nominal Y, confirmed by precise pixel measurement against the
                    // known-good TextRenderer screenshot (supporting/notification-dark-theme-
                    // normal-text-comparison.png) -- avatar-circle diameter matched exactly
                    // (87px at 3x scale in both, confirming identical capture scale), title text
                    // top sat at +5.5px from avatar-center in the new DrawString capture vs
                    // -21.5px in the old TextRenderer one, a 27px-at-3x-scale = 9px-native
                    // difference. Root cause: GDI+'s DrawString positions a full font line-height
                    // box (including its own internal leading above the glyph) starting at the
                    // given Y, while GDI's TextRenderer computes Y from TextRenderer.MeasureText's
                    // own (leading-excluded) metrics -- the same `rectangle.Y` value means two
                    // different things to the two APIs. `rectangle.Y` itself (the vertical-
                    // centering computation upstream) is untouched; only the value actually
                    // passed to DrawString is corrected.
                    Instruction.Create(OpCodes.Ldloca, titleBoundsLocal), Instruction.Create(OpCodes.Call, rectGetYRef),
                    Instruction.Create(OpCodes.Ldc_I4, 9), Instruction.Create(OpCodes.Sub), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Ldloca, titleBoundsLocal), Instruction.Create(OpCodes.Call, rectGetWidthRef), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Ldc_R4, titleRectExtraHeight),
                    Instruction.Create(OpCodes.Newobj, rectangleFCtor4Ref)
                },
                formatFlags: stringFormatFlagsNoWrap);

            // Content keeps the plain conversion (its rect's height is meant to constrain wrapping,
            // unlike title's) but adds LineLimit -- without it, the first live test showed content
            // word-wrapping to a 3rd line and overflowing past the notification's intended bounds
            // instead of stopping at 2 lines with an ellipsis, confirmed via screenshot (GDI+'s own
            // line-wrap metrics differ slightly from GDI's, so a wrap point TextRenderer.MeasureText
            // didn't anticipate became reachable). LineLimit disallows a partially-visible final
            // line, restoring the intended "whole lines only, trimmed to fit the given height" shape.
            ReplaceDrawCall(onPaintContentMethod, "OnPaintContent",
                pushText: new List<Instruction> { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, contentField) },
                pushFont: new List<Instruction> { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Callvirt, controlGetFontRef) },
                pushColor: new List<Instruction> { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Callvirt, controlGetForeColorRef) },
                // -7: content DOES need a left-bearing correction after all -- confirmed via a
                // clean, controlled, SAME-SESSION A/B comparison (not a cross-session/cross-scale
                // reference, which is what produced two earlier wrong conclusions in a row here).
                // Deployed the UN-migrated `output-theme` build (still TextRenderer.DrawText,
                // never touched by this patch) in the exact same live environment, confirmed via
                // the existing --patch-notification-geometry-diag logging that its `contentRect`/
                // `imageRect` values are IDENTICAL to the DrawString build's (image.X=8,
                // content.X=17, both builds -- doLayout() itself was never touched by this whole
                // migration). With that confirmed-identical rect input: the OLD TextRenderer
                // mechanism renders content's first ink at native x=22 (avatar edge x=19, offset
                // +3); the NEW DrawString mechanism (no correction) renders it at x=29-30 (offset
                // +10). Same rect, different visual result -- conclusively isolating the ~7px gap
                // to the rendering API switch, exactly the same class of bug as title's, which an
                // earlier flawed cross-session-image comparison wrongly ruled out for content.
                pushRectF: new List<Instruction> {
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldflda, contentRectField), Instruction.Create(OpCodes.Call, rectGetXRef),
                    Instruction.Create(OpCodes.Ldc_I4, 7), Instruction.Create(OpCodes.Sub), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldflda, contentRectField), Instruction.Create(OpCodes.Call, rectGetYRef), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldflda, contentRectField), Instruction.Create(OpCodes.Call, rectGetWidthRef), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldflda, contentRectField), Instruction.Create(OpCodes.Call, rectGetHeightRef), Instruction.Create(OpCodes.Conv_R4),
                    Instruction.Create(OpCodes.Newobj, rectangleFCtor4Ref)
                },
                formatFlags: stringFormatFlagsLineLimit);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle/OnPaintContent -- now draw via Graphics.DrawString instead of TextRenderer/TextRendererEx.DrawText, sidestepping a Wine gap in GDI's transparent-background text rendering onto an offscreen bitmap");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-force-onload <input-dir> <output-dir>
//
// Confirmed via --patch-notification-layout-diag's live readback: `headerFont` is null at paint
// time. `FormGenericNotification.OnLoad()` is the ONLY place that sets `headerFont`/`Font` (from
// `FontManager.UIFont` at 11pt/10pt) -- if it never fires, `headerFont` stays permanently null and
// `Font` stays whatever ambient default Control.Font falls back to, neither being the intended
// Segoe-UI-family font. This is the exact same class of bug this project already found and fixed
// once before (reports/settings-panel-clip-region-findings.md: formSettings's Load event never
// firing under Wine at all) -- a WinForms lifecycle method silently not being raised, not a
// Wine rendering gap. Passing a null Font to TextRenderer.DrawText doesn't throw -- the native
// DrawTextEx call falls back to whatever font is already selected into the HDC (a raw GDI stock
// font, not a TrueType one), which plausibly explains both the wrong font size/family AND the
// missing anti-aliasing the user noticed in the title specifically.
//
// Fix: same playbook as the formSettings fix -- call the method directly rather than relying on
// the event/lifecycle callback to fire. Insert `if (!loaded) { OnLoad(EventArgs.Empty); }` at the
// very top of OnShown (same safe insertion point already used for --patch-diag/
// --patch-auto-test-notification/--patch-close-listener/--patch-notification-periodic-reblit in
// this file). Guarding on the existing `loaded` field (already set true at the end of OnLoad, and
// otherwise unused for gating anything else) makes this correctly run-once despite OnShown firing
// repeatedly across the form's reused lifetime -- necessary because OnLoad's own body is NOT
// idempotent: it unconditionally does `ThemeManager.Instance.ThemeChanged += Instance_ThemeChanged;`
// with no matching `-=`, so calling it more than once would reproduce the exact double-subscription
// bug already found and fixed for `layeredWindow.Click` in Stage 8.
static int RunPatchNotificationForceOnLoad(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-force-onload <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        var onLoadMethod = type.Methods.FirstOrDefault(m => m.Name == "OnLoad" && m.HasBody);
        if (onLoadMethod is null) { Console.Error.WriteLine("FAIL: OnLoad not found"); return 1; }
        var loadedField = type.Fields.FirstOrDefault(f => f.Name == "loaded");
        if (loadedField is null) { Console.Error.WriteLine("FAIL: loaded field not found"); return 1; }

        var eventArgsEmptyRef = module.ImportReference(typeof(EventArgs).GetField("Empty")!);

        var body = onShownMethod.Body;
        var il = body.GetILProcessor();
        var first = body.Instructions[0];
        void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

        var skipOnLoad = first;
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, loadedField),
            Instruction.Create(OpCodes.Brtrue, skipOnLoad),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldsfld, eventArgsEmptyRef),
            Instruction.Create(OpCodes.Call, module.ImportReference(onLoadMethod))
        );

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- now calls OnLoad(EventArgs.Empty) directly (guarded by the existing `loaded` field, so exactly once per form instance) if it hasn't already run, since OnLoad appears to never fire on its own under Wine -- this is what sets headerFont/Font to the intended UI font instead of leaving headerFont permanently null");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-title-singleline <input-dir> <output-dir>
//
// The user directly caught this looking wrong live (monogram-avatar test screenshot: title
// visibly hugging the TOP of the header band with a large empty gap below it, not centered) after
// an earlier, too-hasty pixel measurement on a different, shorter test wrongly concluded this was
// "close enough" post-font-fix. It is not -- it's a real, separate bug, independent of the
// headerFont-null fix.
//
// `OnPaintTitle` calls `TextRendererEx.DrawText(..., TextFormatFlags.EndEllipsis |
// TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter)` -- flags value `34820` (0x8804),
// confirmed via the raw IL (`ldc.i4 34820` immediately before the `TextRendererEx::DrawText`
// call). `TextFormatFlags.VerticalCenter` is documented (MSDN) to have NO EFFECT unless
// `TextFormatFlags.SingleLine` is also specified -- which it is not here. This is a real gap in
// the app's own flags (plausibly harmless on real Windows if its GDI32 is more lenient about this
// undocumented-behavior case, or simply not something anyone noticed since real Windows renders
// notifications reliably enough that a few pixels of vertical misplacement isn't paired with the
// far more obvious "whole box is blank" bug this project spent most of its effort on).
//
// Fix: flip the single constant from `34820` (0x8804) to `34852` (0x8824) -- adding
// `TextFormatFlags.SingleLine` (0x20) via a plain OR, changing nothing else. A title/sender name
// is exactly the kind of text `SingleLine` is meant for (already never wraps -- `EndEllipsis`
// alone already truncates a too-long name to one line with "..."; `SingleLine` just makes
// `VerticalCenter` actually take effect, per the documented API contract). Simplest possible
// patch shape: a single Ldc_I4 operand rewrite, no insertion, no branch/handler concerns at all --
// the same low-risk category as the very first patch in this whole project (the
// InterpolationMode fix).
static int RunPatchNotificationTitleSingleLine(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-title-singleline <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    const int oldFlags = 34820; // EndEllipsis | NoPrefix | VerticalCenter
    const int newFlags = 34852; // + SingleLine (0x20) -- required for VerticalCenter to take effect

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle(PaintEventArgs) not found"); return 1; }

        var instrs = onPaintTitleMethod.Body.Instructions;
        int matchCount = 0;
        Instruction? match = null;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].OpCode == OpCodes.Ldc_I4 && instrs[i].Operand is int v && v == oldFlags &&
                (instrs[i + 1].OpCode == OpCodes.Call || instrs[i + 1].OpCode == OpCodes.Callvirt) &&
                instrs[i + 1].Operand is MethodReference mr && mr.Name == "DrawText")
            {
                match = instrs[i];
                matchCount++;
            }
        }
        if (matchCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 `ldc.i4 {oldFlags}` immediately before a DrawText call in OnPaintTitle, found {matchCount} -- method shape changed, review needed"); return 1; }

        match!.Operand = newFlags;

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle -- TextFormatFlags {oldFlags} (0x{oldFlags:X}) -> {newFlags} (0x{newFlags:X}), adding SingleLine so VerticalCenter actually takes effect (MSDN: VerticalCenter is ignored without SingleLine)");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-toolbar-icons <input-dir> <output-dir>
//
// Fixes the reply/flag/delete/previous/next row (bottom strip of a mail notification) having the
// exact same invisible-until-fade bug as everything else in this project -- confirmed live (user:
// "I could only see them as they faded out"). Unlike close/settings (--patch-notification-icon-
// bitmap, simple Image fields drawn directly by OnPaint), these five are REAL child controls
// (MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton, hosted in
// tableLayoutPanel1), each with real image-list/tint-based hover rendering already built into
// their own OnPaint -- confirmed via --patch-diag-toolstrip-buttons/--patch-diag-toolstrip-
// controls (see reports/notification-empty-until-fade-findings.md) that they're genuinely Control-
// derived (their own `Bounds`/`Visible` are public, directly-callable properties). OnPaint/
// OnMouseEnter/OnMouseLeave, though, are `protected`, declared on a class in a different assembly
// (MailClient.Common.UI.dll) than FormMailNotification (MailClient.dll) -- calling them directly
// from an unrelated class would violate CLR member accessibility. Rather than reimplementing their
// genuinely complex, theme/hover/tint-dependent paint logic by hand, this reuses them.
//
// **How this patch reaches across that accessibility boundary went through three iterations,
// each worth remembering:**
// 1. First attempt: WIDEN OnPaint/OnMouseEnter/OnMouseLeave's own visibility from protected to
//    public directly in MailClient.Common.UI.dll (the user's own suggestion, since this is raw IL
//    editing anyway). Decompiled clean, deployed without error -- but broke icon rendering on
//    EVERY toolbar button across the entire app (confirmed live via screenshot -- user: "The
//    whole UI is broken now"), not just this notification's own buttons: ControlToolStripButton
//    is the shared control behind every toolbar in eM Client, and the C# compiler's rule against
//    widening an override's accessibility beyond its base declaration turned out to matter at the
//    CLR level too, not just at compile time -- the mismatch broke the type broadly. Reverted
//    immediately (screenshot-confirmed the recovery). Lesson: editing a *shared library* type's
//    own member metadata needs a blast-radius check across its OTHER call sites first.
// 2. Second attempt: reflection (Type.GetMethod(NonPublic|Instance) + MethodBase.Invoke, via a
//    shared __invokeProtected helper). Zero blast radius -- touches nothing outside
//    FormMailNotification's own new methods -- and it worked. But it's real overhead: an
//    object[]-boxing call site for every paint/hover event, and a string-based method-name lookup
//    that could silently no-op if a name's ever mistyped, with no compiler to catch it.
// 3. **This version**: adds three brand-new PUBLIC wrapper methods to ControlToolStripButton
//    itself -- RaisePaint(PaintEventArgs)/RaiseMouseEnter(EventArgs)/RaiseMouseLeave(EventArgs),
//    each just calling the corresponding protected On* method from within the SAME class (legal --
//    protected access from the declaring type itself is always fine, no CLR accessibility issue).
//    These are NEW, ADDITIVE members: nothing about any EXISTING method changes, so there's no
//    override-accessibility mismatch to trigger attempt 1's failure mode, and no existing call
//    site anywhere else in the app is affected -- confirmed by re-running the exact same
//    blast-radius check attempt 1 skipped (a normal, unrelated toolbar button, screenshot-
//    verified unaffected) before ever calling this "done". FormMailNotification then calls
//    button.RaisePaint(...)/button.RaiseMouseEnter(...)/button.RaiseMouseLeave(...) directly, a
//    normal Callvirt -- no reflection, no string lookup, no object[] boxing. Strictly better than
//    both earlier attempts: the "no reflection" property attempt 1 was after, without its blast
//    radius.
//
// Confirmed live (before this patch) that native click routing does NOT work on these controls
// either, matching everything else in this project that lives on `this` form's own unreliable
// window -- user: "Any click across the button had no effect". So this also needs its own click
// hit-testing, layered on top of (and checked BEFORE, since contentRect's hit-test rectangle
// genuinely overlaps the toolbar strip's Y range -- confirmed via --patch-notification-geometry-
// diag's content={Y=56,Height=64} vs. the toolbar's Y=94..116) the existing performMouseClick.
//
// Adds, all on FormMailNotification (MailClient.dll) -- a derived class of FormGenericNotification
// with its own reply/flag/delete/previous/next fields the shared base class doesn't have, so this
// can't just extend FormGenericNotification's existing updateBackgroundBitmap/OnMouseMove/
// performMouseClick in place the way earlier patches did for close/settings:
//   - five new private bool fields: mouseOverReply/mouseOverFlag/mouseOverDelete/
//     mouseOverPrevious/mouseOverNext
//   - __toolbarButtonRect(ControlToolStripButton) -> Rectangle -- tableLayoutPanel1.Bounds.Location
//     + button.Bounds, translated into `this`-relative coordinates
//   - __drawToolbarButton(Graphics, ControlToolStripButton) -- translates to the button's rect,
//     calls its real OnPaint via the new RaisePaint wrapper, translates back
//   - a NEW override of updateBackgroundBitmap() -- calls base (draws everything the existing
//     chain already does, title/content/close/settings/border), then draws all 5 toolbar buttons
//     on top via __drawToolbarButton (skipping any not currently Visible -- previous/next are only
//     Visible when multiple notifications are queued, and their Bounds are stale/unreliable while
//     hidden, confirmed via --patch-diag-toolstrip-controls)
//   - a NEW override of OnMouseMove(MouseEventArgs) -- calls base first (preserves existing close/
//     settings hover, unaffected since it lives in a different Y range), then for each Visible
//     button compares __toolbarButtonRect(button).Contains(e.Location) against its own mouseOverX
//     field, forwarding into the real button's RaiseMouseEnter/RaiseMouseLeave wrappers directly
//     on change, and calling updateLayeredBackground(refreshBitmap: true) once if anything changed
//   - a NEW override of performMouseClick(Point) -- checks all 5 toolbar rects FIRST (priority
//     over contentRect's overlapping hit-test region -- see above), calling the real, unmodified,
//     already-private click handlers (Replyaction/Flag/Delete/button_Previous_Click/
//     button_Next_Click) directly with (this, EventArgs.Empty) on a hit, only falling through to
//     base.performMouseClick(location) if none matched
//
// Since every new method above is written from scratch (not inserted into an existing method's
// body), none of the branch-retargeting/exception-handler-boundary concerns from IL-patching
// lessons 2/3 apply here -- full control over control flow from the start, multiple early `ret`s
// are simply fine in a method with no try/catch regions.
static int RunPatchNotificationToolbarIcons(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-toolbar-icons <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string commonUiAssembly = "MailClient.Common.UI.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormMailNotification";
    const string buttonTypeName = "MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patchedMailClient = false;
    bool patchedCommonUi = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        // Adds three brand-new PUBLIC wrapper methods to ControlToolStripButton --
        // RaisePaint/RaiseMouseEnter/RaiseMouseLeave, each just calling the corresponding
        // PROTECTED OnPaint/OnMouseEnter/OnMouseLeave from within the SAME class (legal --
        // protected access from the declaring type itself is always fine). This is the refined
        // approach after a first attempt WIDENED OnPaint/OnMouseEnter/OnMouseLeave's own
        // visibility from protected to public, which broke icon rendering app-wide (see
        // IL-patching lesson 10) -- these are NEW, ADDITIVE members instead, so no existing
        // method's accessibility changes and no existing call site anywhere else in the app is
        // affected. FormMailNotification below calls these new wrappers directly (no reflection).
        if (fileName == commonUiAssembly)
        {
            var resolver0 = new DefaultAssemblyResolver();
            resolver0.AddSearchDirectory(inDir);
            using var commonModule = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
            {
                AssemblyResolver = resolver0,
                ReadWrite = false
            });

            var buttonType = commonModule.GetType(buttonTypeName);
            if (buttonType is null) { Console.Error.WriteLine($"FAIL: type not found: {buttonTypeName}"); return 1; }

            var onPaintDef = buttonType.Methods.FirstOrDefault(m => m.Name == "OnPaint" && m.Parameters.Count == 1);
            var onMouseEnterDef = buttonType.Methods.FirstOrDefault(m => m.Name == "OnMouseEnter" && m.Parameters.Count == 1);
            var onMouseLeaveDef = buttonType.Methods.FirstOrDefault(m => m.Name == "OnMouseLeave" && m.Parameters.Count == 1);
            if (onPaintDef is null || onMouseEnterDef is null || onMouseLeaveDef is null)
            { Console.Error.WriteLine("FAIL: OnPaint/OnMouseEnter/OnMouseLeave not found on ControlToolStripButton"); return 1; }

            string[] existingNames = { "RaisePaint", "RaiseMouseEnter", "RaiseMouseLeave" };
            foreach (var n in existingNames)
            {
                if (buttonType.Methods.Any(m => m.Name == n))
                { Console.Error.WriteLine($"FAIL: {buttonTypeName} already has a method named {n} -- naming collision, pick a different wrapper name"); return 1; }
            }

            MethodDefinition MakeWrapper(string name, MethodDefinition target)
            {
                var wrapper = new MethodDefinition(name, MethodAttributes.Public | MethodAttributes.HideBySig, commonModule.TypeSystem.Void);
                wrapper.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, target.Parameters[0].ParameterType));
                var wil = wrapper.Body.GetILProcessor();
                wil.Emit(OpCodes.Ldarg_0);
                wil.Emit(OpCodes.Ldarg_1);
                wil.Emit(OpCodes.Call, target);
                wil.Emit(OpCodes.Ret);
                return wrapper;
            }

            buttonType.Methods.Add(MakeWrapper("RaisePaint", onPaintDef));
            buttonType.Methods.Add(MakeWrapper("RaiseMouseEnter", onMouseEnterDef));
            buttonType.Methods.Add(MakeWrapper("RaiseMouseLeave", onMouseLeaveDef));

            Console.WriteLine($"OK   {fileName}: {buttonTypeName} -- added RaisePaint/RaiseMouseEnter/RaiseMouseLeave public wrapper methods (OnPaint/OnMouseEnter/OnMouseLeave themselves untouched)");
            patchedCommonUi = true;
            commonModule.Write(destPath);
            continue;
        }

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var baseType = type.BaseType?.Resolve();
        if (baseType is null) { Console.Error.WriteLine("FAIL: couldn't resolve FormGenericNotification (base type)"); return 1; }

        string[] buttonFieldNames = { "button_Reply", "button_Flag", "button_Delete", "button_Previous", "button_Next" };
        string[] mouseOverFieldNames = { "mouseOverReply", "mouseOverFlag", "mouseOverDelete", "mouseOverPrevious", "mouseOverNext" };
        string[] clickHandlerNames = { "Replyaction", "Flag", "Delete", "button_Previous_Click", "button_Next_Click" };

        var buttonFields = new FieldDefinition[5];
        for (int i = 0; i < 5; i++)
        {
            var f = type.Fields.FirstOrDefault(f => f.Name == buttonFieldNames[i]);
            if (f is null) { Console.Error.WriteLine($"FAIL: field not found: {buttonFieldNames[i]}"); return 1; }
            buttonFields[i] = f;
        }
        var tlpField = type.Fields.FirstOrDefault(f => f.Name == "tableLayoutPanel1");
        if (tlpField is null) { Console.Error.WriteLine("FAIL: tableLayoutPanel1 field not found"); return 1; }

        var clickHandlerDefs = new MethodDefinition[5];
        for (int i = 0; i < 5; i++)
        {
            var m = type.Methods.FirstOrDefault(m => m.Name == clickHandlerNames[i] && m.HasBody);
            if (m is null) { Console.Error.WriteLine($"FAIL: click handler method not found: {clickHandlerNames[i]}"); return 1; }
            clickHandlerDefs[i] = m;
        }

        var buttonTypeDef = buttonFields[0].FieldType.Resolve();
        if (buttonTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve ControlToolStripButton"); return 1; }
        var buttonTypeRef = module.ImportReference(buttonTypeDef);

        // Bounds/Visible are PUBLIC (confirmed via --patch-diag-toolstrip-controls: calling
        // get_Bounds directly on a Controls-collection-typed reference worked at runtime without
        // any access violation) -- found by walking ControlToolStripButton's own base chain, same
        // technique as everywhere else in this file for a foreign-assembly member.
        MethodDefinition? FindInBaseChain(TypeDefinition start, string name, int paramCount)
        {
            for (var t = start; t is not null; t = t.BaseType?.Resolve())
            {
                var m = t.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);
                if (m is not null) return m;
            }
            return null;
        }

        var getBoundsDef = FindInBaseChain(buttonTypeDef, "get_Bounds", 0);
        if (getBoundsDef is null) { Console.Error.WriteLine("FAIL: get_Bounds not found in ControlToolStripButton's base chain"); return 1; }
        var getBoundsRef = module.ImportReference(getBoundsDef);
        var getVisibleDef = FindInBaseChain(buttonTypeDef, "get_Visible", 0);
        if (getVisibleDef is null) { Console.Error.WriteLine("FAIL: get_Visible not found in ControlToolStripButton's base chain"); return 1; }
        var getVisibleRef = module.ImportReference(getVisibleDef);

        // OnPaint/OnMouseEnter/OnMouseLeave are `protected`, declared on ControlToolStripButton
        // (MailClient.Common.UI.dll) -- an unrelated class relative to FormMailNotification
        // (MailClient.dll), so calling them directly would violate CLR member accessibility.
        // A first attempt instead flipped their visibility to public via raw IL/metadata editing
        // (the C# compiler would block widening an override's accessibility, but this bypasses the
        // compiler entirely) -- deployed fine in isolation, but ControlToolStripButton is the
        // shared control behind EVERY toolbar button in the whole app, and the visibility change
        // broke icon rendering app-wide (confirmed live via screenshot: every toolbar button
        // reduced to text-only, no icons -- user: "The whole UI is broken now"). Reverted
        // immediately. This uses reflection instead (Type.GetMethod(NonPublic|Instance) +
        // MethodBase.Invoke, via a small shared helper below) -- more code, but touches nothing
        // outside FormMailNotification's own new methods, zero blast radius on the shared library.
        var rectangleTypeDef = getBoundsDef.ReturnType.Resolve();
        if (rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle"); return 1; }
        var rectangleTypeRef = module.ImportReference(rectangleTypeDef);
        MethodDefinition? RectMethod(string name, int paramCount) => rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == paramCount);
        var rectGetX = RectMethod("get_X", 0); var rectGetY = RectMethod("get_Y", 0);
        var rectGetWidth = RectMethod("get_Width", 0); var rectGetHeight = RectMethod("get_Height", 0);
        var rectCtor4 = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 4);
        var rectContains = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "Contains" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "Point");
        var rectToString = RectMethod("ToString", 0);
        if (rectGetX is null || rectGetY is null || rectGetWidth is null || rectGetHeight is null || rectCtor4 is null || rectContains is null || rectToString is null)
        { Console.Error.WriteLine("FAIL: one or more Rectangle members (get_X/get_Y/get_Width/get_Height/.ctor(4)/Contains(Point)/ToString) not found"); return 1; }
        var rectGetXRef = module.ImportReference(rectGetX); var rectGetYRef = module.ImportReference(rectGetY);
        var rectGetWidthRef = module.ImportReference(rectGetWidth); var rectGetHeightRef = module.ImportReference(rectGetHeight);
        var rectCtor4Ref = module.ImportReference(rectCtor4); var rectContainsRef = module.ImportReference(rectContains);
        var rectToStringRef = module.ImportReference(rectToString);

        var pointTypeDef = rectContains.Parameters[0].ParameterType.Resolve();
        if (pointTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Point from Rectangle.Contains' parameter"); return 1; }
        var pointTypeRef = module.ImportReference(pointTypeDef);

        // TranslateTransform -- reuse the exact already-in-module reference updateBackgroundBitmap
        // itself uses (Graphics.TranslateTransform(Single,Single)), confirmed present via
        // --dump-il, rather than resolving System.Drawing.Common.dll fresh.
        var updateBackgroundBitmapDef = baseType.Methods.FirstOrDefault(m => m.Name == "updateBackgroundBitmap" && m.HasBody);
        if (updateBackgroundBitmapDef is null) { Console.Error.WriteLine("FAIL: FormGenericNotification.updateBackgroundBitmap not found"); return 1; }
        var translateTransformInstr = updateBackgroundBitmapDef.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr && mr.Name == "TranslateTransform");
        if (translateTransformInstr is null) { Console.Error.WriteLine("FAIL: TranslateTransform call not found in updateBackgroundBitmap"); return 1; }
        var translateTransformRef = (MethodReference)translateTransformInstr.Operand;
        var graphicsTypeDef = translateTransformRef.DeclaringType.Resolve();
        if (graphicsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics from TranslateTransform's DeclaringType"); return 1; }
        var graphicsTypeRef = module.ImportReference(graphicsTypeDef);
        var graphicsSaveDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "Save" && m.Parameters.Count == 0);
        if (graphicsSaveDef is null) { Console.Error.WriteLine("FAIL: Graphics.Save() not found"); return 1; }
        var graphicsStateTypeRef = module.ImportReference(graphicsSaveDef.ReturnType);
        var graphicsRestoreDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "Restore" && m.Parameters.Count == 1);
        if (graphicsRestoreDef is null) { Console.Error.WriteLine("FAIL: Graphics.Restore(GraphicsState) not found"); return 1; }
        var graphicsSaveRef = module.ImportReference(graphicsSaveDef);
        var graphicsRestoreRef = module.ImportReference(graphicsRestoreDef);

        // PaintEventArgs -- same assembly as Control/ControlToolStripButton's base chain
        // (System.Windows.Forms.dll), reached sideways from that already-correctly-resolved
        // module rather than typeof() on the patcher's own net10 process (lesson 5).
        var winFormsModule = getVisibleDef.DeclaringType.Module;
        var paintEventArgsTypeDef = winFormsModule.GetType("System.Windows.Forms.PaintEventArgs");
        if (paintEventArgsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't find System.Windows.Forms.PaintEventArgs in System.Windows.Forms.dll"); return 1; }
        var paintEventArgsCtor = paintEventArgsTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2
            && m.Parameters[0].ParameterType.Name == "Graphics" && m.Parameters[1].ParameterType.Name == "Rectangle");
        if (paintEventArgsCtor is null) { Console.Error.WriteLine("FAIL: PaintEventArgs(Graphics, Rectangle) ctor not found"); return 1; }
        var paintEventArgsCtorRef = module.ImportReference(paintEventArgsCtor);
        var paintEventArgsTypeRef = module.ImportReference(paintEventArgsTypeDef);

        // EventArgs.Empty -- a static READONLY FIELD (not a property -- GetProperty("Empty")
        // returns null, confirmed the hard way), for the click-handler and OnMouseEnter/Leave
        // calls. EventArgs is CoreLib-forwarded, lesson 5-safe.
        var eventArgsEmptyFieldRef = module.ImportReference(typeof(EventArgs).GetField("Empty")!);
        var eventArgsTypeRef = eventArgsEmptyFieldRef.FieldType;
        var boolTypeRef = module.TypeSystem.Boolean;

        // RaisePaint/RaiseMouseEnter/RaiseMouseLeave -- new PUBLIC wrapper methods added to
        // ControlToolStripButton by the commonUiAssembly branch above (same patch run, always
        // applied together), each just calling the real protected OnPaint/OnMouseEnter/
        // OnMouseLeave from within that class. Called directly here, no reflection needed. Built
        // as plain MethodReferences (not resolved off buttonTypeDef, which was read from the
        // INPUT directory's copy of MailClient.Common.UI.dll -- the wrapper methods only exist in
        // the OUTPUT copy the commonUiAssembly branch above just wrote) -- their signature is
        // already fully known (matches MakeWrapper's own shape above), so this is safe.
        MethodReference MakeWrapperRef(string name, TypeReference paramType)
        {
            var r = new MethodReference(name, module.TypeSystem.Void, buttonTypeRef) { HasThis = true };
            r.Parameters.Add(new ParameterDefinition(paramType));
            return r;
        }
        var raisePaintRef = MakeWrapperRef("RaisePaint", paintEventArgsTypeRef);
        var raiseMouseEnterRef = MakeWrapperRef("RaiseMouseEnter", eventArgsTypeRef);
        var raiseMouseLeaveRef = MakeWrapperRef("RaiseMouseLeave", eventArgsTypeRef);

        // --- five new private bool fields ---
        var mouseOverFields = new FieldDefinition[5];
        for (int i = 0; i < 5; i++)
        {
            var f = new FieldDefinition(mouseOverFieldNames[i], FieldAttributes.Private, boolTypeRef);
            type.Fields.Add(f);
            mouseOverFields[i] = f;
        }

        // --- __toolbarButtonRect(ControlToolStripButton button) -> Rectangle ---
        var toolbarRectMethod = new MethodDefinition("__toolbarButtonRect",
            MethodAttributes.Private | MethodAttributes.HideBySig,
            rectangleTypeRef);
        toolbarRectMethod.Parameters.Add(new ParameterDefinition("button", ParameterAttributes.None, buttonTypeRef));
        {
            var body = toolbarRectMethod.Body;
            var il = body.GetILProcessor();
            var tLocal = new VariableDefinition(rectangleTypeRef);
            var bLocal = new VariableDefinition(rectangleTypeRef);
            body.Variables.Add(tLocal);
            body.Variables.Add(bLocal);
            body.InitLocals = true;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, tlpField);
            il.Emit(OpCodes.Callvirt, getBoundsRef);
            il.Emit(OpCodes.Stloc, tLocal);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getBoundsRef);
            il.Emit(OpCodes.Stloc, bLocal);

            il.Emit(OpCodes.Ldloca, tLocal);
            il.Emit(OpCodes.Call, rectGetXRef);
            il.Emit(OpCodes.Ldloca, bLocal);
            il.Emit(OpCodes.Call, rectGetXRef);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldloca, tLocal);
            il.Emit(OpCodes.Call, rectGetYRef);
            il.Emit(OpCodes.Ldloca, bLocal);
            il.Emit(OpCodes.Call, rectGetYRef);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldloca, bLocal);
            il.Emit(OpCodes.Call, rectGetWidthRef);
            il.Emit(OpCodes.Ldloca, bLocal);
            il.Emit(OpCodes.Call, rectGetHeightRef);
            il.Emit(OpCodes.Newobj, rectCtor4Ref);
            il.Emit(OpCodes.Ret);
        }
        type.Methods.Add(toolbarRectMethod);

        // --- __drawToolbarButton(Graphics g, ControlToolStripButton button) ---
        var drawToolbarButtonMethod = new MethodDefinition("__drawToolbarButton",
            MethodAttributes.Private | MethodAttributes.HideBySig,
            module.TypeSystem.Void);
        drawToolbarButtonMethod.Parameters.Add(new ParameterDefinition("g", ParameterAttributes.None, graphicsTypeRef));
        drawToolbarButtonMethod.Parameters.Add(new ParameterDefinition("button", ParameterAttributes.None, buttonTypeRef));
        {
            // ControlToolStripButton.OnPaint's own image-drawing branch calls
            // Graphics.ResetTransform() internally (confirmed via decompile -- part of its
            // rotation-transform handling, unconditional whenever an image actually draws), which
            // wipes ANY transform applied before calling it -- including a naive manual
            // TranslateTransform(+X,+Y) / TranslateTransform(-X,-Y) "undo" pair around the call
            // (the first draft here). That undo then applies to an already-reset (identity)
            // matrix instead of the intended baseline, corrupting the transform for every
            // subsequent button drawn on the same Graphics -- confirmed live: the first button
            // drawn (Reply) landed correctly, everything drawn after it did not (Delete/Flag
            // observed rendering near the avatar instead of the toolbar strip). Graphics.Save()/
            // Restore(GraphicsState) is immune to this: Restore() puts the ENTIRE transform/clip
            // state back to exactly what Save() captured, regardless of what OnPaint did with it
            // in between (translate, rotate, or reset), so each button's paint call is fully
            // isolated from the others.
            var body = drawToolbarButtonMethod.Body;
            var il = body.GetILProcessor();
            var rLocal = new VariableDefinition(rectangleTypeRef);
            var stateLocal = new VariableDefinition(graphicsStateTypeRef);
            body.Variables.Add(rLocal);
            body.Variables.Add(stateLocal);
            body.InitLocals = true;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, toolbarRectMethod);
            il.Emit(OpCodes.Stloc, rLocal);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, graphicsSaveRef);
            il.Emit(OpCodes.Stloc, stateLocal);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloca, rLocal);
            il.Emit(OpCodes.Call, rectGetXRef);
            il.Emit(OpCodes.Conv_R4);
            il.Emit(OpCodes.Ldloca, rLocal);
            il.Emit(OpCodes.Call, rectGetYRef);
            il.Emit(OpCodes.Conv_R4);
            il.Emit(OpCodes.Callvirt, translateTransformRef);

            // button.RaisePaint(new PaintEventArgs(g, new Rectangle(0, 0, r.Width, r.Height)));
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldloca, rLocal);
            il.Emit(OpCodes.Call, rectGetWidthRef);
            il.Emit(OpCodes.Ldloca, rLocal);
            il.Emit(OpCodes.Call, rectGetHeightRef);
            il.Emit(OpCodes.Newobj, rectCtor4Ref);
            il.Emit(OpCodes.Newobj, paintEventArgsCtorRef);
            il.Emit(OpCodes.Callvirt, raisePaintRef);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc, stateLocal);
            il.Emit(OpCodes.Callvirt, graphicsRestoreRef);
            il.Emit(OpCodes.Ret);
        }
        type.Methods.Add(drawToolbarButtonMethod);

        // --- override updateBackgroundBitmap() ---
        var newUpdateBackgroundBitmap = new MethodDefinition("updateBackgroundBitmap",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            module.TypeSystem.Void);
        {
            var body = newUpdateBackgroundBitmap.Body;
            var il = body.GetILProcessor();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, module.ImportReference(updateBackgroundBitmapDef));

            // if (backgroundBitmap == null) return;
            var backgroundBitmapField = FindFieldInBaseChain(baseType, "backgroundBitmap");
            if (backgroundBitmapField is null) { Console.Error.WriteLine("FAIL: backgroundBitmap field not found"); return 1; }
            var retInstr = Instruction.Create(OpCodes.Ret);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, module.ImportReference(backgroundBitmapField));
            il.Emit(OpCodes.Brfalse, retInstr);

            // using Graphics g = Graphics.FromImage(backgroundBitmap);
            var graphicsFromImage = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "FromImage" && m.Parameters.Count == 1);
            if (graphicsFromImage is null) { Console.Error.WriteLine("FAIL: Graphics.FromImage(Image) not found"); return 1; }
            var graphicsDispose = FindInBaseChain(graphicsTypeDef, "Dispose", 0);
            if (graphicsDispose is null) { Console.Error.WriteLine("FAIL: Graphics.Dispose() not found"); return 1; }
            var gLocal = new VariableDefinition(graphicsTypeRef);
            body.Variables.Add(gLocal);
            body.InitLocals = true;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, module.ImportReference(backgroundBitmapField));
            il.Emit(OpCodes.Call, module.ImportReference(graphicsFromImage));
            il.Emit(OpCodes.Stloc, gLocal);

            // if (ShadowVisible) g.TranslateTransform(9f, 9f);
            var shadowVisibleDef = FindPropertyGetterInBaseChain(baseType, "ShadowVisible");
            if (shadowVisibleDef is null) { Console.Error.WriteLine("FAIL: get_ShadowVisible not found"); return 1; }
            var afterShadow = Instruction.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, module.ImportReference(shadowVisibleDef));
            il.Emit(OpCodes.Brfalse, afterShadow);
            il.Emit(OpCodes.Ldloc, gLocal);
            il.Emit(OpCodes.Ldc_R4, 9f);
            il.Emit(OpCodes.Ldc_R4, 9f);
            il.Emit(OpCodes.Callvirt, translateTransformRef);
            il.Append(afterShadow);

            foreach (var bf in buttonFields)
            {
                var skip = Instruction.Create(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Callvirt, getVisibleRef);
                il.Emit(OpCodes.Brfalse, skip);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, gLocal);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Call, drawToolbarButtonMethod);
                il.Append(skip);
            }

            il.Emit(OpCodes.Ldloc, gLocal);
            il.Emit(OpCodes.Callvirt, module.ImportReference(graphicsDispose));
            il.Append(retInstr);
        }
        type.Methods.Add(newUpdateBackgroundBitmap);

        // --- override OnMouseMove(MouseEventArgs e) ---
        var baseOnMouseMove = FindInBaseChain(baseType, "OnMouseMove", 1);
        if (baseOnMouseMove is null) { Console.Error.WriteLine("FAIL: OnMouseMove(MouseEventArgs) not found in base chain"); return 1; }
        var mouseEventArgsTypeRef = module.ImportReference(baseOnMouseMove.Parameters[0].ParameterType);
        var getLocationDef = FindInBaseChain(baseOnMouseMove.Parameters[0].ParameterType.Resolve(), "get_Location", 0);
        if (getLocationDef is null) { Console.Error.WriteLine("FAIL: MouseEventArgs.get_Location not found"); return 1; }
        var getLocationRef = module.ImportReference(getLocationDef);
        var updateLayeredBackgroundDef = FindInBaseChain(baseType, "updateLayeredBackground", 1);
        if (updateLayeredBackgroundDef is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground(bool) not found in base chain"); return 1; }

        var newOnMouseMove = new MethodDefinition("OnMouseMove",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            module.TypeSystem.Void);
        newOnMouseMove.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, mouseEventArgsTypeRef));
        {
            var body = newOnMouseMove.Body;
            var il = body.GetILProcessor();
            var ptLocal = new VariableDefinition(pointTypeRef);
            var rLocal = new VariableDefinition(rectangleTypeRef);
            var anyChangedLocal = new VariableDefinition(boolTypeRef);
            body.Variables.Add(ptLocal);
            body.Variables.Add(rLocal);
            body.Variables.Add(anyChangedLocal);
            body.InitLocals = true;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, module.ImportReference(baseOnMouseMove));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getLocationRef);
            il.Emit(OpCodes.Stloc, ptLocal);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc, anyChangedLocal);

            for (int i = 0; i < 5; i++)
            {
                var bf = buttonFields[i];
                var mf = mouseOverFields[i];
                var skipAll = Instruction.Create(OpCodes.Nop);
                var notContained = Instruction.Create(OpCodes.Nop);
                var afterEnterCheck = Instruction.Create(OpCodes.Nop);
                var afterLeaveCheck = Instruction.Create(OpCodes.Nop);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Callvirt, getVisibleRef);
                il.Emit(OpCodes.Brfalse, skipAll);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Call, toolbarRectMethod);
                il.Emit(OpCodes.Stloc, rLocal);

                il.Emit(OpCodes.Ldloca, rLocal);
                il.Emit(OpCodes.Ldloc, ptLocal);
                il.Emit(OpCodes.Call, rectContainsRef);
                il.Emit(OpCodes.Brfalse, notContained);

                // contained: if (!mouseOverX) { button.RaiseMouseEnter(EventArgs.Empty); mouseOverX = true; anyChanged = true; }
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, mf);
                il.Emit(OpCodes.Brtrue, afterEnterCheck);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Ldsfld, eventArgsEmptyFieldRef);
                il.Emit(OpCodes.Callvirt, raiseMouseEnterRef);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stfld, mf);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stloc, anyChangedLocal);
                il.Append(afterEnterCheck);
                il.Emit(OpCodes.Br, skipAll);

                // not contained: if (mouseOverX) { button.RaiseMouseLeave(EventArgs.Empty); mouseOverX = false; anyChanged = true; }
                il.Append(notContained);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, mf);
                il.Emit(OpCodes.Brfalse, afterLeaveCheck);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Ldsfld, eventArgsEmptyFieldRef);
                il.Emit(OpCodes.Callvirt, raiseMouseLeaveRef);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stfld, mf);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stloc, anyChangedLocal);
                il.Append(afterLeaveCheck);

                il.Append(skipAll);
            }

            var endInstr = Instruction.Create(OpCodes.Ret);
            il.Emit(OpCodes.Ldloc, anyChangedLocal);
            il.Emit(OpCodes.Brfalse, endInstr);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Call, module.ImportReference(updateLayeredBackgroundDef));
            il.Append(endInstr);
        }
        type.Methods.Add(newOnMouseMove);

        // --- override performMouseClick(Point location) ---
        var basePerformMouseClick = baseType.Methods.FirstOrDefault(m => m.Name == "performMouseClick" && m.HasBody);
        if (basePerformMouseClick is null) { Console.Error.WriteLine("FAIL: FormGenericNotification.performMouseClick not found"); return 1; }

        var newPerformMouseClick = new MethodDefinition("performMouseClick",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            module.TypeSystem.Void);
        newPerformMouseClick.Parameters.Add(new ParameterDefinition("location", ParameterAttributes.None, pointTypeRef));
        {
            var body = newPerformMouseClick.Body;
            var il = body.GetILProcessor();
            var rLocal = new VariableDefinition(rectangleTypeRef);
            body.Variables.Add(rLocal);
            body.InitLocals = true;

            for (int i = 0; i < 5; i++)
            {
                var bf = buttonFields[i];
                var handler = clickHandlerDefs[i];
                var skip = Instruction.Create(OpCodes.Nop);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Callvirt, getVisibleRef);
                il.Emit(OpCodes.Brfalse, skip);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, bf);
                il.Emit(OpCodes.Call, toolbarRectMethod);
                il.Emit(OpCodes.Stloc, rLocal);
                il.Emit(OpCodes.Ldloca, rLocal);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, rectContainsRef);
                il.Emit(OpCodes.Brfalse, skip);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldsfld, eventArgsEmptyFieldRef);
                il.Emit(OpCodes.Call, module.ImportReference(handler));
                il.Emit(OpCodes.Ret);

                il.Append(skip);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, module.ImportReference(basePerformMouseClick));
            il.Emit(OpCodes.Ret);
        }
        type.Methods.Add(newPerformMouseClick);

        Console.WriteLine($"OK   {fileName}: {targetType} -- added updateBackgroundBitmap/OnMouseMove/performMouseClick overrides (bitmap-baked paint + hover-forward + click hit-testing) for button_Reply/button_Flag/button_Delete/button_Previous/button_Next, calling their real OnPaint/OnMouseEnter/OnMouseLeave via the new RaisePaint/RaiseMouseEnter/RaiseMouseLeave wrapper methods (no reflection)");
        patchedMailClient = true;

        module.Write(destPath);
    }

    if (!patchedMailClient) { Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}"); return 1; }
    if (!patchedCommonUi) { Console.Error.WriteLine($"FAIL: {commonUiAssembly} not found in {inDir}"); return 1; }

    return 0;

    static FieldDefinition? FindFieldInBaseChain(TypeDefinition start, string name)
    {
        for (var t = start; t is not null; t = t.BaseType?.Resolve())
        {
            var f = t.Fields.FirstOrDefault(f => f.Name == name);
            if (f is not null) return f;
        }
        return null;
    }

    static MethodDefinition? FindPropertyGetterInBaseChain(TypeDefinition start, string propertyName)
    {
        for (var t = start; t is not null; t = t.BaseType?.Resolve())
        {
            var m = t.Methods.FirstOrDefault(m => m.Name == "get_" + propertyName && m.Parameters.Count == 0);
            if (m is not null) return m;
        }
        return null;
    }
}

// --patch-notification-invalidate <input-dir> <output-dir>
//
// Fixes notification toasts (FormMailNotification etc., via their shared base
// FormGenericNotification) appearing as an empty box: real content is painted correctly
// (confirmed via --patch-diag instrumentation, and via CX_DEBUGMSG trace all the way down to
// individual glyph rasterization -- see reports/notification-empty-until-fade-findings.md) but
// doesn't reach the visible screen until the rapid repaint burst during fade-out.
//
// Two earlier attempts, both confirmed by the user to make no observable difference:
// 1. A single Invalidate() call at the Appearing->Visible transition -- ruled out "missing
//    repaint request" as the cause.
// 2. A single synchronous Opacity nudge (1.0 -> 0.999 -> 1.0, no delay in between) at the same
//    point -- ruled out "needs one more SetLayeredWindowAttributes call" as the cause, since both
//    calls landed in the same message-loop pass with no real elapsed time between them.
//
// Third attempt (this one): the working fade-out phase isn't just "opacity changes" -- it's
// dozens of *separate* ticks ~25ms apart, each a genuine pass through the OS message loop, giving
// Wine's X11 compositor real wall-clock time and repeated opportunities to catch up. A single
// synchronous property-setter pair can't replicate that. This patch deliberately triggers a
// brief, real dip-and-recover using the actual opacity/Invalidate() machinery, WITH real elapsed
// time between each step (Application.DoEvents() to pump the message loop, then Thread.Sleep to
// give the compositor genuine wall-clock time) right at the Appearing->Visible transition, before
// settling into the normal multi-second hold. Expected to cause a brief (~150ms), visible flicker
// -- deliberate, for this test; worth minimizing later if it turns out to actually fix the bug.
//
// Touches MailClient.dll only (FormGenericNotification.timer_OnTimer). Insertion point is deep
// inside the method's single try block, nowhere near TryStart/TryEnd/HandlerStart/HandlerEnd,
// and confirmed (both by inspection via `ilspycmd -il` before writing this, and by a runtime
// check below) not to be the target of any branch in the method -- the two IL-patching pitfalls
// documented in CLAUDE.md that would silently produce wrong or invalid IL here. No new branches
// are introduced (the dip-and-recover sequence is fully unrolled, not a loop), so neither pitfall
// is a risk from this patch's own insertion either.
static int RunPatchNotificationInvalidate(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-invalidate <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const string targetMethod = "timer_OnTimer";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        // il-patcher doesn't reference System.Windows.Forms directly -- resolve Control's
        // Invalidate() by walking up the base-type chain via Cecil, same technique used
        // elsewhere in this file.
        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        // Opacity is declared on Form, not Control -- separate walk up the base-type chain.
        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        // Application.DoEvents() is a static method, not reachable via the instance base-type
        // chain walk above -- Application lives in the same module Control resolved into, so
        // look it up there directly.
        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);

        MethodReference Import2(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var threadSleepRef = Import2(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);

        var body = method.Body;
        // SimplifyMacros converts every short-form branch (bne.un.s, br.s, etc. -- single-byte
        // relative offsets) in this method to its long-form equivalent (4-byte offset) before any
        // insertion happens. Mono.Cecil does NOT do this automatically: inserting enough new
        // instructions can push a short branch's target further away than an sbyte can encode,
        // and module.Write() silently emits a corrupted offset rather than erroring -- hit for
        // real on this patch's first version (48 new instructions was enough to break a nearby
        // bne.un.s; the two earlier, much smaller notification-invalidate attempts happened to
        // stay under the range by luck). Decompiled as garbled logic with "stack underflow"
        // errors in an unrelated branch -- caught by the project's own "always re-decompile
        // instruction-insertion patches" rule, not by a clean scan-mode pass. Cheap and has no
        // functional downside to call unconditionally, so do it before every insertion here.
        body.SimplifyMacros();
        var instrs = body.Instructions;

        // Find the method's one-and-only `stfld state` -- confirmed unique by inspection
        // (ilspycmd -il) before writing this patch.
        Instruction? stateStfld = null;
        foreach (var instr in instrs)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference fr && fr.Name == "state")
            {
                if (stateStfld is not null)
                {
                    Console.Error.WriteLine("FAIL: more than one `stfld state` found in timer_OnTimer -- method shape changed, review needed");
                    return 1;
                }
                stateStfld = instr;
            }
        }
        if (stateStfld is null) { Console.Error.WriteLine("FAIL: couldn't find `stfld state` in timer_OnTimer"); return 1; }

        int stfldIndex = instrs.IndexOf(stateStfld);
        var insertBefore = instrs[stfldIndex + 1];

        // insertBefore must not itself be a branch target -- otherwise a branch landing here
        // would skip the newly-inserted Invalidate() call entirely (lesson #2 in CLAUDE.md).
        foreach (var instr in instrs)
        {
            if (instr.Operand == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == insertBefore || handler.TryEnd == insertBefore ||
                handler.HandlerStart == insertBefore || handler.HandlerEnd == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var il = body.GetILProcessor();
        void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(insertBefore, i); }

        // One dip-and-recover step: Opacity = value; Invalidate(); DoEvents(); Sleep(ms).
        // DoEvents() pumps the message loop (processing whatever WM_PAINT/X11 events are
        // pending right now); the Sleep after it gives the compositor genuine wall-clock time
        // to actually act on them before the next step -- the two things a single synchronous
        // property-set (attempt #2) couldn't provide.
        void EmitStep(double opacity, int sleepMs)
        {
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, opacity),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Call, doEventsRef),
                Instruction.Create(OpCodes.Ldc_I4, sleepMs),
                Instruction.Create(OpCodes.Call, threadSleepRef)
            );
        }

        // Fully unrolled (no branches introduced) brief dip down and back up, ~150ms total.
        EmitStep(0.6, 20);
        EmitStep(0.3, 20);
        EmitStep(0.1, 20);
        EmitStep(0.3, 20);
        EmitStep(0.6, 20);
        EmitStep(1.0, 20);

        Console.WriteLine($"OK   {fileName}: inserted dip-and-recover sequence after state=Visible in {targetType}::{targetMethod}");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-long-burst <input-dir> <output-dir> [totalMs] [stepMs] [minOpacity]
//
// Fourth attempt at the notification empty-box bug (see reports/notification-empty-until-fade-findings.md
// for the first three, all confirmed ineffective). The third attempt's dip-and-recover was brief
// (~150ms, 6 steps) and had zero observable effect even though executed within 150ms of the box
// appearing -- the box stayed frozen for the full ~3s hold regardless. That result is consistent
// with two different explanations that a brief burst can't distinguish between: (a) a fixed
// wall-clock compositor timeout unrelated to app activity, or (b) a burst just needs to run much
// longer / cover far more ticks than 150ms's worth to have any effect. This patch tests which, by
// running the *same* dip-and-recover mechanism for far longer -- long enough to either (1) show
// text appearing at a point clearly decoupled from any fixed ~3s mark (supports "just needs more
// ticks"), or (2) still not show anything until ~3s regardless of burst length (supports "fixed
// timeout, app activity is irrelevant"). Ends the burst pinned at Opacity=1.0 with one final
// Invalidate(), then falls through into the method's normal state machine unmodified -- if the
// freeze does lift during the burst, the window is left sitting fully opaque with (hopefully)
// visible text for whatever's left of the configured hold, then fades and closes exactly as
// normal. That's the "pause it there" behavior: not a separate hold step, just letting the
// pre-existing Visible-state hold do its job once the freeze is already gone.
//
// totalMs/stepMs/minOpacity default to 6000/25/0.05 -- a triangle wave (1.0 -> minOpacity -> 1.0,
// repeating) at stepMs spacing until totalMs elapses, landing exactly on 1.0. All parameters are
// baked in at patch-build time (fully unrolled, same reason as the third attempt: no branches
// introduced, so neither of the two branch-related IL-patching pitfalls in CLAUDE.md apply) --
// re-run this command with different values and rebuild to iterate, rather than expecting runtime
// configurability. At 6000ms/25ms this unrolls to ~240 steps (~1900 new instructions); confirmed
// SimplifyMacros (already called below) keeps the method's existing short-form branches valid at
// this size -- verify again with ilspycmd if these defaults are increased substantially further.
//
// Touches MailClient.dll only (FormGenericNotification.timer_OnTimer), same insertion point and
// same validated safety checks (not a branch target, not an exception-handler region boundary) as
// --patch-notification-invalidate.
static int RunPatchNotificationLongBurst(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-long-burst <input-dir> <output-dir> [totalMs] [stepMs] [minOpacity]");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    int totalMs = args.Length > 3 ? int.Parse(args[3]) : 6000;
    int stepMs = args.Length > 4 ? int.Parse(args[4]) : 25;
    double minOpacity = args.Length > 5 ? double.Parse(args[5]) : 0.05;
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const string targetMethod = "timer_OnTimer";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);

        MethodReference Import2(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var threadSleepRef = Import2(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);

        var body = method.Body;
        body.SimplifyMacros();
        var instrs = body.Instructions;

        Instruction? stateStfld = null;
        foreach (var instr in instrs)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference fr && fr.Name == "state")
            {
                if (stateStfld is not null)
                {
                    Console.Error.WriteLine("FAIL: more than one `stfld state` found in timer_OnTimer -- method shape changed, review needed");
                    return 1;
                }
                stateStfld = instr;
            }
        }
        if (stateStfld is null) { Console.Error.WriteLine("FAIL: couldn't find `stfld state` in timer_OnTimer"); return 1; }

        int stfldIndex = instrs.IndexOf(stateStfld);
        var insertBefore = instrs[stfldIndex + 1];

        foreach (var instr in instrs)
        {
            if (instr.Operand == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == insertBefore || handler.TryEnd == insertBefore ||
                handler.HandlerStart == insertBefore || handler.HandlerEnd == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var il = body.GetILProcessor();
        void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(insertBefore, i); }

        void EmitStep(double opacity)
        {
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, opacity),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Call, doEventsRef),
                Instruction.Create(OpCodes.Ldc_I4, stepMs),
                Instruction.Create(OpCodes.Call, threadSleepRef)
            );
        }

        // Triangle wave 1.0 -> minOpacity -> 1.0, repeating, for totalMs; always ends the loop
        // (at build time, not runtime -- this is fully unrolled) exactly on an upward step so the
        // very last EmitStep call always lands on 1.0.
        int stepCount = Math.Max(2, totalMs / stepMs);
        int half = Math.Max(1, stepCount / 2);
        int emitted = 0;
        while (emitted < stepCount)
        {
            int intoCycle = emitted % (2 * half);
            double frac = intoCycle < half ? (double)intoCycle / half : (double)(2 * half - intoCycle) / half;
            double opacity = 1.0 - frac * (1.0 - minOpacity);
            EmitStep(opacity);
            emitted++;
        }
        EmitStep(1.0);

        Console.WriteLine($"OK   {fileName}: inserted {emitted + 1}-step long-burst sequence (~{totalMs}ms, min opacity {minOpacity}) after state=Visible in {targetType}::{targetMethod}");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-silent-wait <input-dir> <output-dir> [waitMs]
//
// Fifth attempt at the notification empty-box bug. Two long-burst tests (--patch-notification-
// long-burst, 6000ms and 2000ms) both released at almost exactly the same wall-clock mark (~6s),
// matching this bottle's configured NotificationsHideTimeout regardless of burst length -- meaning
// the burst's own repaint/opacity activity had no measurable causal effect on release timing. A
// likely reason: WinForms' Timer keeps ticking at its normal fast fade-animation interval
// throughout the burst (never stopped), and Application.DoEvents() inside the burst can let that
// timer re-fire *reentrantly* into this same handler -- possibly re-triggering the app's own
// natural Hide()/fade-out path early and often, independent of anything the burst intended.
//
// This patch isolates that: stop the timer before waiting (blocking WM_TIMER from firing at all,
// so no reentrancy is possible), Thread.Sleep for waitMs with *zero* Invalidate/Opacity/DoEvents
// calls, then restart the timer and let the method's original autoHide logic proceed exactly as
// before. If release still happens at ~waitMs regardless of doing literally nothing during the
// wait, that's strong evidence of a pure wall-clock/compositor-side timer fully decoupled from
// app paint activity. If it does NOT release until later (after the real second natural tick,
// i.e. waitMs + timeToStay), that means *some* paint/pump activity during the window is actually
// necessary after all, contrary to what the two burst tests suggested.
//
// waitMs defaults to 6000 (the two burst tests' observed release point). Touches MailClient.dll
// only (FormGenericNotification.timer_OnTimer), same insertion point/safety checks as the two
// burst patches above.
static int RunPatchNotificationSilentWait(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-silent-wait <input-dir> <output-dir> [waitMs]");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    int waitMs = args.Length > 3 ? int.Parse(args[3]) : 6000;
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const string targetMethod = "timer_OnTimer";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        // timer is a field on the type itself (System.Windows.Forms.Timer), not inherited --
        // resolve its Stop()/Start() directly rather than walking a base-type chain.
        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        if (timerField is null) { Console.Error.WriteLine("FAIL: couldn't find `timer` field on type"); return 1; }
        var timerType = timerField.FieldType.Resolve();
        if (timerType is null) { Console.Error.WriteLine("FAIL: couldn't resolve timer field's type"); return 1; }
        var stopDef = timerType.Methods.FirstOrDefault(m => m.Name == "Stop" && m.Parameters.Count == 0);
        var startDef = timerType.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (stopDef is null || startDef is null) { Console.Error.WriteLine("FAIL: timer type missing Stop()/Start()"); return 1; }
        var stopRef = module.ImportReference(stopDef);
        var startRef = module.ImportReference(startDef);

        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);

        var body = method.Body;
        body.SimplifyMacros();
        var instrs = body.Instructions;

        Instruction? stateStfld = null;
        foreach (var instr in instrs)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference fr && fr.Name == "state")
            {
                if (stateStfld is not null)
                {
                    Console.Error.WriteLine("FAIL: more than one `stfld state` found in timer_OnTimer -- method shape changed, review needed");
                    return 1;
                }
                stateStfld = instr;
            }
        }
        if (stateStfld is null) { Console.Error.WriteLine("FAIL: couldn't find `stfld state` in timer_OnTimer"); return 1; }

        int stfldIndex = instrs.IndexOf(stateStfld);
        var insertBefore = instrs[stfldIndex + 1];

        foreach (var instr in instrs)
        {
            if (instr.Operand == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == insertBefore || handler.TryEnd == insertBefore ||
                handler.HandlerStart == insertBefore || handler.HandlerEnd == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var il = body.GetILProcessor();
        void Emit(params Instruction[] ins) { foreach (var i in ins) il.InsertBefore(insertBefore, i); }

        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, timerField),
            Instruction.Create(OpCodes.Callvirt, stopRef),
            Instruction.Create(OpCodes.Ldc_I4, waitMs),
            Instruction.Create(OpCodes.Call, threadSleepRef),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, timerField),
            Instruction.Create(OpCodes.Callvirt, startRef)
        );

        Console.WriteLine($"OK   {fileName}: inserted silent {waitMs}ms wait (timer stopped, zero paint activity) after state=Visible in {targetType}::{targetMethod}");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-no-fading <input-dir> <output-dir>
//
// Separate test from the burst/silent-wait isolation patches above -- rather than guessing at the
// freeze's mechanism, remove the animation apparatus entirely and see whether a plain, static
// (non-layered-opacity-animated) notification is affected at all. `LayeredBaseForm` (the shared
// base of FormGenericNotification and unrelated things like FormPopup) already has a built-in
// `ShowWithoutFading` property that, when true, skips the whole Appearing/Disappearing opacity
// state machine on both the show and hide paths (see FormGenericNotification.OnShown and .Hide)
// -- existing, already-exercised app logic, not something invented for this test. Setting it via
// LayeredBaseForm's own getter would also affect FormPopup and other unrelated layered popups, so
// this patch instead sets it narrowly in FormGenericNotification's own constructor (right after
// the existing `autoHide = Program.Settings.NotificationsHideAfterTimeout;` assignment), scoping
// the change to notification toasts only.
//
// Caveat worth watching for when testing this: OnShown's ShowWithoutFading branch never starts
// `timer` at all (the fade-driven Appearing/Disappearing branches are the only place it's
// started in that method) -- meaning it's not yet confirmed whether auto-hide-after-timeout still
// works with fading disabled, or whether some other, not-yet-found mechanism drives it. If the
// notification never disappears on its own during testing, that's a real (if separate) finding,
// not a sign this patch is broken -- don't read it as "the fix worked, it just never closes."
//
// Touches MailClient.dll only (FormGenericNotification's instance constructor).
static int RunPatchNotificationNoFading(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-no-fading <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var ctor = type.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0 && m.HasBody);
        if (ctor is null) { Console.Error.WriteLine($"FAIL: parameterless .ctor not found on {targetType}"); return 1; }

        var baseType = type.BaseType?.Resolve();
        if (baseType is null || baseType.FullName != "MailClient.UI.Forms.LayeredBaseForm")
        {
            Console.Error.WriteLine($"FAIL: expected base type MailClient.UI.Forms.LayeredBaseForm, got {baseType?.FullName}");
            return 1;
        }
        var setShowWithoutFadingDef = baseType.Methods.FirstOrDefault(m => m.Name == "set_ShowWithoutFading");
        if (setShowWithoutFadingDef is null) { Console.Error.WriteLine("FAIL: LayeredBaseForm missing set_ShowWithoutFading"); return 1; }
        var setShowWithoutFadingRef = module.ImportReference(setShowWithoutFadingDef);

        var body = ctor.Body;
        body.SimplifyMacros();
        var instrs = body.Instructions;

        // `autoHide` is assigned twice: a compiler-generated field initializer (`autoHide = true;`)
        // ahead of the `base..ctor()` call, and the real one inside `if (!UIUtils.DesignMode)`
        // (`autoHide = Program.Settings.NotificationsHideAfterTimeout;`). Only the second is a
        // reliable insertion anchor (past DesignMode-guard, past base-ctor, method body settled)
        // -- identify it by the preceding call to the settings getter rather than by assuming
        // ordering, so a compiler-output shuffle doesn't silently pick the wrong one.
        Instruction? autoHideStfld = null;
        for (int idx = 0; idx < instrs.Count; idx++)
        {
            var instr = instrs[idx];
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference fr && fr.Name == "autoHide")
            {
                var prev = idx > 0 ? instrs[idx - 1] : null;
                bool isRealAssignment = prev is not null && (prev.OpCode == OpCodes.Call || prev.OpCode == OpCodes.Callvirt) &&
                    prev.Operand is MethodReference mr && mr.Name == "get_NotificationsHideAfterTimeout";
                if (isRealAssignment)
                {
                    if (autoHideStfld is not null)
                    {
                        Console.Error.WriteLine("FAIL: more than one qualifying `stfld autoHide` found in .ctor -- method shape changed, review needed");
                        return 1;
                    }
                    autoHideStfld = instr;
                }
            }
        }
        if (autoHideStfld is null) { Console.Error.WriteLine("FAIL: couldn't find the real `stfld autoHide` (preceded by get_NotificationsHideAfterTimeout) in .ctor"); return 1; }

        // Insert immediately before the `stfld` itself, not after it: the instruction after it is
        // the if(!DesignMode)-block's own join point (the branch target the guard's initial
        // brfalse jumps to when skipping the block) -- CLAUDE.md lesson #2 territory. Inserting
        // before `stfld` instead is both stack-neutral (our sequence is a self-contained
        // ldarg.0/ldc.i4.1/callvirt with net-zero stack effect, so it doesn't disturb the pending
        // objref+value already pushed for the real stfld beneath it) and correctly stays inside
        // the same DesignMode guard as the assignment it's anchored to, without needing to retarget
        // anything.
        var insertBefore = autoHideStfld;

        foreach (var instr in instrs)
        {
            if (instr.Operand == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == insertBefore || handler.TryEnd == insertBefore ||
                handler.HandlerStart == insertBefore || handler.HandlerEnd == insertBefore)
            {
                Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var il = body.GetILProcessor();
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldc_I4_1));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Callvirt, setShowWithoutFadingRef));

        Console.WriteLine($"OK   {fileName}: inserted `this.ShowWithoutFading = true;` after autoHide assignment in {targetType}::.ctor");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-click-resubscribe <input-dir> <output-dir>
//
// Root cause of the double click-dispatch found via --patch-diag instrumentation (sixth round,
// reports/notification-empty-until-fade-findings.md): FormGenericNotification.OnShown() runs
// `layeredWindow.Click += layeredWindow_Click;` unconditionally every time it runs, with no
// matching `-=` anywhere in the class -- and OnShown() runs at least twice per notification shown
// (confirmed via instrumentation: once early with the form still blank/Hidden, once again once
// Title/Content are populated), each adding another subscription of the *same* handler to the
// *same* event. A single physical click then invokes layeredWindow_Click (and everything
// downstream -- performMouseClick -> notificationForm_Click -> PerformAction -> ShowMailForm) once
// per accumulated subscription. Confirmed via log: one click fired layeredWindow_Click and
// notificationForm_Click twice, 207ms apart, on a mailForm shown only once so far in that Wine
// session -- the count would only grow further for a mailForm instance reused across more
// notifications, since it's a persistent singleton (see MailNotificationHandler.
// EnsureValidNotificationForm), with each Show() adding one more subscription on top of whatever's
// already there. Plausible explanation for this investigation's flakiness across attempts (fine
// once, hung once, crashed once): reentrant/concurrent calls into PerformAction/ShowMailForm from
// the same click landing badly under Wine/CEF, worse the more redundant subscriptions have piled
// up in a given session.
//
// Fix: insert `layeredWindow.Click -= layeredWindow_Click;` immediately before the existing
// `layeredWindow.Click += layeredWindow_Click;` in OnShown() -- the standard unsubscribe-then-
// subscribe idiom. Removing a delegate that was never added is a documented no-op on .NET
// multicast events, so this is safe on the very first call too, and caps the subscription count at
// exactly one no matter how many times OnShown() runs. All five operands (the layeredWindow field,
// the layeredWindow_Click method, and the EventHandler ctor) are read back from the existing
// add_Click call's own instructions rather than assumed, so the new remove_Click call is
// guaranteed to reference the exact same field/handler/delegate type as the code it's paired with.
static int RunPatchNotificationClickResubscribe(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-click-resubscribe <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var onShown = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        if (onShown is null) { Console.Error.WriteLine($"FAIL: OnShown not found on {targetType}"); return 1; }

        var body = onShown.Body;
        body.SimplifyMacros();
        var instrs = body.Instructions;

        // SimplifyMacros() (called above, per CLAUDE.md's IL-patching lessons, before any
        // insertion) turns the short-form `ldarg.0` macro into the long-form `ldarg <this>` --
        // both need to be accepted below, not just the short form.
        bool IsLdThis(Instruction i) => i.OpCode == OpCodes.Ldarg_0 ||
            (i.OpCode == OpCodes.Ldarg && ReferenceEquals(i.Operand, body.ThisParameter));

        // Find the `callvirt add_Click` that wires layeredWindow_Click up, then walk back over the
        // exact 5-instruction sequence that pushes its arguments (ldarg.0; ldfld layeredWindow;
        // ldarg.0; ldftn layeredWindow_Click; newobj EventHandler::.ctor) so every operand reused
        // for the new remove_Click call is read from the real code, not assumed.
        int addClickIndex = -1;
        MethodReference? addClickRef = null;
        for (int idx = 0; idx < instrs.Count; idx++)
        {
            if (instrs[idx].OpCode == OpCodes.Callvirt && instrs[idx].Operand is MethodReference mr && mr.Name == "add_Click")
            {
                if (addClickIndex != -1)
                {
                    Console.Error.WriteLine($"FAIL: more than one `add_Click` call in {targetType}::OnShown -- method shape changed, review needed");
                    return 1;
                }
                addClickIndex = idx;
                addClickRef = mr;
            }
        }
        if (addClickIndex == -1 || addClickRef is null) { Console.Error.WriteLine($"FAIL: no `add_Click` call found in {targetType}::OnShown"); return 1; }
        if (addClickIndex < 5) { Console.Error.WriteLine("FAIL: add_Click found too early in method body to have the expected 5-instruction preamble"); return 1; }

        var i0 = instrs[addClickIndex - 5]; // ldarg.0
        var i1 = instrs[addClickIndex - 4]; // ldfld layeredWindow
        var i2 = instrs[addClickIndex - 3]; // ldarg.0
        var i3 = instrs[addClickIndex - 2]; // ldftn layeredWindow_Click
        var i4 = instrs[addClickIndex - 1]; // newobj EventHandler::.ctor

        if (!IsLdThis(i0) ||
            !(i1.OpCode == OpCodes.Ldfld && i1.Operand is FieldReference fieldRef && fieldRef.Name == "layeredWindow") ||
            !IsLdThis(i2) ||
            !(i3.OpCode == OpCodes.Ldftn && i3.Operand is MethodReference clickHandlerRef && clickHandlerRef.Name == "layeredWindow_Click") ||
            !(i4.OpCode == OpCodes.Newobj && i4.Operand is MethodReference ctorRef && ctorRef.DeclaringType.FullName == "System.EventHandler"))
        {
            Console.Error.WriteLine("FAIL: unexpected instruction shape immediately before add_Click -- method shape changed, review needed");
            return 1;
        }

        var fieldRefFinal = (FieldReference)i1.Operand;
        var clickHandlerRefFinal = (MethodReference)i3.Operand;
        var ctorRefFinal = (MethodReference)i4.Operand;

        var removeClickDef = addClickRef.Resolve()?.DeclaringType.Methods.FirstOrDefault(m => m.Name == "remove_Click");
        if (removeClickDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve sibling remove_Click for add_Click's declaring type"); return 1; }
        var removeClickRef = module.ImportReference(removeClickDef);

        var anchor = i0;
        foreach (var instr in instrs)
        {
            if (instr.Operand == anchor)
            {
                Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == anchor || handler.TryEnd == anchor ||
                handler.HandlerStart == anchor || handler.HandlerEnd == anchor)
            {
                Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var il = body.GetILProcessor();
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Ldfld, fieldRefFinal));
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Ldftn, clickHandlerRefFinal));
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Newobj, ctorRefFinal));
        il.InsertBefore(anchor, Instruction.Create(OpCodes.Callvirt, removeClickRef));

        Console.WriteLine($"OK   {fileName}: inserted `layeredWindow.Click -= layeredWindow_Click;` immediately before the existing += in {targetType}::OnShown");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-hover-forward <input-dir> <output-dir>
//
// Fixes the "hovering over the notification doesn't pause the auto-hide countdown" bug (see
// reports/notification-empty-until-fade-findings.md's twenty-eighth round for the original
// root-cause investigation). `timer_OnTimer`'s own pre-existing logic already implements the
// pause-while-hovering feature correctly, gated on a `mouseOver` field that only
// `OnMouseEnter`/`OnMouseLeave` (both pre-existing, unmodified) ever set -- but `this`
// (FormGenericNotification) does not reliably receive real Wine mouse-move/enter/leave messages,
// the same underlying gap Stage 8's click-routing fix already worked around for *clicks*
// specifically by forwarding from `layeredWindow` (the drop-shadow companion window, confirmed
// elsewhere in this investigation to be the window that actually receives reliable input).
//
// Studied `layeredWindow`'s own class, `LayeredForm`, for how the EXISTING click forwarding
// actually works under the hood (not just at the C# level) -- its `WndProc` override manually
// intercepts `WM_SETCURSOR` (msg 32) and inspects the HIWORD of `lParam`, which Windows sets to
// the identity of the mouse message that triggered the WM_SETCURSOR (`WM_LBUTTONDOWN`=513,
// `WM_RBUTTONDOWN`=516, `WM_MBUTTONDOWN`=519, `WM_XBUTTONDOWN`=523) -- when one of those matches,
// it calls `this.OnClick(EventArgs.Empty)` directly, synthesizing a reliable `Click` event
// independent of whatever native click-dispatch quirk exists under Wine. `WM_SETCURSOR` is ALSO
// sent for plain mouse movement (Windows sends it any time the cursor is over a window and could
// need updating, not just on clicks), with `WM_MOUSEMOVE`=512 as the HIWORD in that case --
// meaning the exact same already-proven-reliable trigger this project's own Stage 8 fix already
// depends on can be extended to synthesize a `MouseMove` event too, with no new message-hooking
// risk at all (the WM_SETCURSOR interception itself is unchanged, just one more HIWORD value
// checked inside it).
//
// Two-part fix, both in MailClient.dll:
// 1. `LayeredForm.WndProc`: add a HIWORD==512 (WM_MOUSEMOVE) case alongside the existing
//    click-HIWORD switch, calling `this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, 0,
//    0, 0))` -- raises the base `Control.MouseMove` event (LayeredForm doesn't override it, so
//    this is Control's own implementation) with placeholder coordinates; the actual client-space
//    position is computed on the RECEIVING end instead (see below), matching how
//    `layeredWindow_Click` already computes `PointToClient(Control.MousePosition)` itself rather
//    than trusting any position carried by the triggering event. Inserted at the very top of the
//    method (never a branch target, no exception handlers here -- the standard safe insertion
//    point used throughout this file), independently re-deriving `m.Msg`/HIWORD rather than
//    reusing the ORIGINAL branch's locals, so this addition is fully self-contained and doesn't
//    touch the existing click-detection code path at all.
// 2. `FormGenericNotification`: subscribes to `layeredWindow.MouseMove` in `OnShown` (same
//    unsubscribe-then-subscribe idiom already used for `layeredWindow.Click`, prepended fresh at
//    the top of `OnShown` rather than reusing the Click subscription's own exact anchor, since
//    there's no existing MouseMove subscription instructions to model the insertion point on).
//    New handler `layeredWindow_MouseMove(object, MouseEventArgs)`: computes
//    `PointToClient(Control.MousePosition)` (the SAME technique `layeredWindow_Click` ->
//    `performMouseClick` already uses successfully), compares it against `ClientRectangle` to
//    detect a genuine enter/leave transition relative to the CURRENT `mouseOver` field value, and
//    calls the EXISTING, unmodified `OnMouseEnter`/`OnMouseLeave`/`OnMouseMove` methods directly
//    -- their own internal logic (pause-the-timer, icon-hover-swap, cursor-over-content-check) is
//    entirely untouched; this patch only ensures they actually get CALLED when they should.
static int RunPatchNotificationHoverForward(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-hover-forward <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string notifType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const string layeredFormType = "MailClient.UI.Forms.LayeredForm";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(notifType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {notifType}"); return 1; }
        var layeredFormTypeDef = module.GetType(layeredFormType);
        if (layeredFormTypeDef is null) { Console.Error.WriteLine($"FAIL: type not found: {layeredFormType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var onMouseMoveMethod = type.Methods.FirstOrDefault(m => m.Name == "OnMouseMove" && m.HasBody);
        var onMouseEnterMethod = type.Methods.FirstOrDefault(m => m.Name == "OnMouseEnter" && m.HasBody);
        var onMouseLeaveMethod = type.Methods.FirstOrDefault(m => m.Name == "OnMouseLeave" && m.HasBody);
        // Cancel an in-progress fade-out on hover-enter. First attempt called the pre-existing
        // public `Reshow()` (`if (state == Disappearing || state == Visible) Show();`) -- not
        // currently called from anywhere hover-related in the original app (OnMouseEnter has its
        // OWN narrower inline reshow check instead, gated on `reShowOnMouseOver &&
        // notifications.Count > 0`, which never fires for a single non-queued notification, the
        // exact scenario being fixed here). `Reshow()` DID trigger, confirmed live, but
        // `Show()`/`OnShown()`'s own `Disappearing` case only resumes the SAME gradual fade-in
        // animation from whatever opacity it's currently at (`state = Appearing; alphaIncrement =
        // 25f / timeToShow;`) -- visibly "fades back in slowly" rather than snapping instantly, per
        // live user feedback. Fixed by directly replicating `OnShown`'s OWN `Appearing`/`Visible`
        // case branch instead (the one that actually snaps straight to opacity 1.0) -- `state =
        // Visible; alphaIncrement = 0f; timer.Interval = timeToStay; Opacity = 1.0;
        // updateLayeredBackground(refreshBitmap: false); if (autoHide) timer.Start();` -- rather
        // than modifying the shared `Show()`/`Reshow()` methods themselves, which other callers
        // (e.g. advancing to the next queued notification) may rely on fading in gradually.
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var autoHideField = type.Fields.FirstOrDefault(f => f.Name == "autoHide");
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (stateField is null || alphaIncrementField is null || timerField is null || timeToStayField is null || autoHideField is null || updateLayeredBackgroundMethod is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find state/alphaIncrement/timer/timeToStay/autoHide field(s) or updateLayeredBackground");
            return 1;
        }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);
        var timerSetIntervalRef = module.ImportReference(timerField.FieldType.Resolve().Methods.First(m => m.Name == "set_Interval"));
        var timerStartRef = module.ImportReference(timerField.FieldType.Resolve().Methods.First(m => m.Name == "Start" && m.Parameters.Count == 0));

        // Opacity -- a Form property (not all Controls have it), resolve by walking the base-type
        // chain to System.Windows.Forms.Form (app-deployed assembly -- IL-patching lesson 5).
        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Form.set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var mouseOverField = type.Fields.FirstOrDefault(f => f.Name == "mouseOver");
        // layeredWindow is declared on the base class (LayeredBaseForm), not directly on
        // FormGenericNotification -- type.Fields only lists directly-declared fields.
        var layeredWindowField = type.Fields.FirstOrDefault(f => f.Name == "layeredWindow") ??
            type.BaseType?.Resolve()?.Fields.FirstOrDefault(f => f.Name == "layeredWindow");
        var disposedField = type.Fields.FirstOrDefault(f => f.Name == "disposed");
        var disposingField = type.Fields.FirstOrDefault(f => f.Name == "disposing");
        if (onShownMethod is null || onMouseMoveMethod is null || onMouseEnterMethod is null || onMouseLeaveMethod is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find OnShown/OnMouseMove/OnMouseEnter/OnMouseLeave");
            return 1;
        }
        if (mouseOverField is null || layeredWindowField is null || disposedField is null || disposingField is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find mouseOver/layeredWindow/disposed/disposing field(s)");
            return 1;
        }

        var wndProcMethod = layeredFormTypeDef.Methods.FirstOrDefault(m => m.Name == "WndProc" && m.HasBody);
        if (wndProcMethod is null) { Console.Error.WriteLine($"FAIL: WndProc not found on {layeredFormType}"); return 1; }

        // MouseEventArgs -- resolve from FormGenericNotification's own OnMouseMove parameter type
        // (app-deployed System.Windows.Forms.dll -- IL-patching lesson 5), not typeof().
        var mouseEventArgsTypeRef = onMouseMoveMethod.Parameters[0].ParameterType;
        var mouseEventArgsTypeDef = mouseEventArgsTypeRef.Resolve();
        if (mouseEventArgsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve MouseEventArgs from OnMouseMove's own parameter type"); return 1; }
        var mouseEventArgsCtorDef = mouseEventArgsTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 5);
        if (mouseEventArgsCtorDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve MouseEventArgs(MouseButtons,int,int,int,int)"); return 1; }
        var mouseEventArgsCtorRef = module.ImportReference(mouseEventArgsCtorDef);
        var mouseButtonsTypeRef = mouseEventArgsCtorDef.Parameters[0].ParameterType;

        // Message.get_Msg()/get_LParam() -- reuse the exact MethodReferences WndProc's own
        // existing code already calls, rather than resolving fresh.
        var existingGetMsgCall = wndProcMethod.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr && mr.Name == "get_Msg");
        var existingGetLParamCall = wndProcMethod.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr2 && mr2.Name == "get_LParam");
        if (existingGetMsgCall is null || existingGetLParamCall is null) { Console.Error.WriteLine("FAIL: couldn't find existing get_Msg()/get_LParam() calls in WndProc"); return 1; }
        var getMsgRef = (MethodReference)existingGetMsgCall.Operand;
        var getLParamRef = (MethodReference)existingGetLParamCall.Operand;
        var messageParamRef = wndProcMethod.Parameters[0];

        // Control.OnMouseMove(MouseEventArgs) / .MouseMove add/remove -- walk the base-type chain
        // from LayeredForm (same technique used elsewhere in this file for Control members)
        // rather than typeof(Control) reflection.
        TypeDefinition? controlType = layeredFormTypeDef;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in LayeredForm's base-type chain"); return 1; }
        var controlOnMouseMoveDef = controlType.Methods.FirstOrDefault(m => m.Name == "OnMouseMove" && m.Parameters.Count == 1);
        var addMouseMoveDef = controlType.Methods.FirstOrDefault(m => m.Name == "add_MouseMove");
        var removeMouseMoveDef = controlType.Methods.FirstOrDefault(m => m.Name == "remove_MouseMove");
        var getMousePositionDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_MousePosition" && m.Parameters.Count == 0 && m.IsStatic);
        var pointToClientDef = controlType.Methods.FirstOrDefault(m => m.Name == "PointToClient");
        var getClientRectangleDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_ClientRectangle");
        if (controlOnMouseMoveDef is null || addMouseMoveDef is null || removeMouseMoveDef is null || getMousePositionDef is null || pointToClientDef is null || getClientRectangleDef is null)
        {
            Console.Error.WriteLine("FAIL: couldn't resolve one of Control.OnMouseMove/add_MouseMove/remove_MouseMove/get_MousePosition/PointToClient/get_ClientRectangle");
            return 1;
        }
        var controlOnMouseMoveRef = module.ImportReference(controlOnMouseMoveDef);
        var addMouseMoveRef = module.ImportReference(addMouseMoveDef);
        var removeMouseMoveRef = module.ImportReference(removeMouseMoveDef);
        var getMousePositionRef = module.ImportReference(getMousePositionDef);
        var pointToClientRef = module.ImportReference(pointToClientDef);
        var getClientRectangleRef = module.ImportReference(getClientRectangleDef);

        // MouseEventHandler -- resolve from add_MouseMove's own parameter type (guaranteed the
        // correctly-versioned delegate type for this exact event), not typeof() reflection.
        var mouseEventHandlerTypeRef = addMouseMoveDef.Parameters[0].ParameterType;
        var mouseEventHandlerTypeDef = mouseEventHandlerTypeRef.Resolve();
        if (mouseEventHandlerTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve MouseEventHandler from add_MouseMove's own parameter type"); return 1; }
        var mouseEventHandlerCtorDef = mouseEventHandlerTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor");
        if (mouseEventHandlerCtorDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve MouseEventHandler's constructor"); return 1; }
        var mouseEventHandlerCtorRef = module.ImportReference(mouseEventHandlerCtorDef);

        // Point/Rectangle -- resolve from PointToClient's own parameter/return types. Resolve()'d
        // to a TypeDefinition for member lookups AND separately imported into `module` (these
        // live in System.Drawing.Primitives.dll, a different module than the one being edited --
        // using an un-imported foreign TypeReference as a local variable's type fails at
        // module.Write() time with "declared in another module and needs to be imported", hit for
        // real here).
        var pointTypeDef = pointToClientDef.Parameters[0].ParameterType.Resolve();
        var rectangleTypeDef = getClientRectangleDef.ReturnType.Resolve();
        if (pointTypeDef is null || rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Point/Rectangle"); return 1; }
        var pointTypeRef = module.ImportReference(pointTypeDef);
        var rectangleTypeRef = module.ImportReference(rectangleTypeDef);
        var rectangleContainsDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "Contains" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Drawing.Point");
        var pointGetXDef = pointTypeDef.Methods.FirstOrDefault(m => m.Name == "get_X");
        var pointGetYDef = pointTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Y");
        if (rectangleContainsDef is null || pointGetXDef is null || pointGetYDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle.Contains(Point)/Point.get_X/get_Y"); return 1; }
        var rectangleContainsRef = module.ImportReference(rectangleContainsDef);
        var pointGetXRef = module.ImportReference(pointGetXDef);
        var pointGetYRef = module.ImportReference(pointGetYDef);

        var eventArgsEmptyRef = module.ImportReference(typeof(EventArgs).GetField("Empty")!);

        // --- Part 1: LayeredForm.WndProc, prepend a self-contained WM_MOUSEMOVE-via-WM_SETCURSOR
        // check at the very top (never a branch target; this method has no exception handlers).
        {
            var body = wndProcMethod.Body;
            body.SimplifyMacros();
            var il = body.GetILProcessor();
            var first = body.Instructions[0];

            var skip = Instruction.Create(OpCodes.Nop);

            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, messageParamRef));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, getMsgRef));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 32)); // WM_SETCURSOR
            il.InsertBefore(first, Instruction.Create(OpCodes.Bne_Un, skip));

            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, messageParamRef));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, getLParamRef));
            il.InsertBefore(first, Instruction.Create(OpCodes.Conv_I4));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 16));
            il.InsertBefore(first, Instruction.Create(OpCodes.Shr));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 512)); // WM_MOUSEMOVE
            il.InsertBefore(first, Instruction.Create(OpCodes.Bne_Un, skip));

            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0)); // MouseButtons.None
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0)); // clicks
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0)); // x
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0)); // y
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0)); // delta
            il.InsertBefore(first, Instruction.Create(OpCodes.Newobj, mouseEventArgsCtorRef));
            il.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, controlOnMouseMoveRef));

            il.InsertBefore(first, skip);
        }

        // --- Part 2a: FormGenericNotification.OnShown, prepend the MouseMove resubscribe
        // (unsubscribe-then-subscribe idiom, same as the existing Click one).
        var handlerMethod = new MethodDefinition("layeredWindow_MouseMove", MethodAttributes.Private, module.TypeSystem.Void);
        handlerMethod.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
        handlerMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, mouseEventArgsTypeRef));
        type.Methods.Add(handlerMethod);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, layeredWindowField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, handlerMethod),
                Instruction.Create(OpCodes.Newobj, mouseEventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, removeMouseMoveRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, layeredWindowField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, handlerMethod),
                Instruction.Create(OpCodes.Newobj, mouseEventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, addMouseMoveRef)
            );
        }

        // --- Part 2b: the new handler itself.
        //   private void layeredWindow_MouseMove(object sender, MouseEventArgs e)
        //   {
        //       if (disposed || disposing) return;
        //       Point p = PointToClient(Control.MousePosition);
        //       bool isOver = ClientRectangle.Contains(p);
        //       if (isOver && !mouseOver) OnMouseEnter(EventArgs.Empty);
        //       else if (!isOver && mouseOver) OnMouseLeave(EventArgs.Empty);
        //       if (isOver) OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0));
        //   }
        {
            var hBody = handlerMethod.Body;
            hBody.InitLocals = true;
            var pointLocal = new VariableDefinition(pointTypeRef);
            var isOverLocal = new VariableDefinition(module.TypeSystem.Boolean);
            var rectLocal = new VariableDefinition(rectangleTypeRef);
            hBody.Variables.Add(pointLocal);
            hBody.Variables.Add(isOverLocal);
            hBody.Variables.Add(rectLocal);
            var il = hBody.GetILProcessor();
            var ret = Instruction.Create(OpCodes.Ret);
            var afterEnterLeave = Instruction.Create(OpCodes.Ldloc, isOverLocal);
            var elseLeaveCheck = Instruction.Create(OpCodes.Ldloc, isOverLocal);
            var afterMove = Instruction.Create(OpCodes.Ret);

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, disposedField));
            var checkDisposing = Instruction.Create(OpCodes.Ldarg_0);
            il.Append(Instruction.Create(OpCodes.Brtrue, ret));
            il.Append(checkDisposing);
            il.Append(Instruction.Create(OpCodes.Ldfld, disposingField));
            il.Append(Instruction.Create(OpCodes.Brtrue, ret));

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Call, getMousePositionRef));
            il.Append(Instruction.Create(OpCodes.Callvirt, pointToClientRef));
            il.Append(Instruction.Create(OpCodes.Stloc, pointLocal));

            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Callvirt, getClientRectangleRef));
            il.Append(Instruction.Create(OpCodes.Stloc, rectLocal));
            // Rectangle.Contains(Point) is an instance method on a value type -- its receiver
            // must be a managed pointer (Ldloca on an addressable local), not the raw struct
            // value ClientRectangle's getter just returned directly on the stack (IL-patching
            // lesson 6's class of bug: a struct instance call needs `this` pushed via Ldloca,
            // caught here by tracing the stack by hand before building, not by a live failure).
            il.Append(Instruction.Create(OpCodes.Ldloca, rectLocal));
            il.Append(Instruction.Create(OpCodes.Ldloc, pointLocal));
            il.Append(Instruction.Create(OpCodes.Call, rectangleContainsRef));
            il.Append(Instruction.Create(OpCodes.Stloc, isOverLocal));

            // if (isOver && !mouseOver) OnMouseEnter(...)
            il.Append(Instruction.Create(OpCodes.Ldloc, isOverLocal));
            il.Append(Instruction.Create(OpCodes.Brfalse, elseLeaveCheck));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, mouseOverField));
            il.Append(Instruction.Create(OpCodes.Brtrue, afterEnterLeave));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldsfld, eventArgsEmptyRef));
            il.Append(Instruction.Create(OpCodes.Callvirt, module.ImportReference(onMouseEnterMethod)));
            // Also cancel any in-progress fade-out on hover-enter, snapping straight to fully
            // visible (see the field-resolution comment above for why this doesn't just call the
            // existing Show()/Reshow(), which resume a gradual fade-in from wherever opacity
            // currently is instead of snapping instantly).
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_2)); // NotificationFormState.Visible
            il.Append(Instruction.Create(OpCodes.Stfld, stateField));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldc_R4, 0f));
            il.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, timerField));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
            il.Append(Instruction.Create(OpCodes.Callvirt, timerSetIntervalRef));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldc_R8, 1.0));
            il.Append(Instruction.Create(OpCodes.Callvirt, setOpacityRef));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Callvirt, updateLayeredBackgroundRef));
            // Nop, not a copy of some other real instruction: this is purely a branch-target
            // marker for the `if (autoHide)` skip below and must have zero stack effect
            // (IL-patching lesson 8 -- reusing a real load instruction here would leave a stray
            // value on the stack for the skip-path).
            var skipTimerStart = Instruction.Create(OpCodes.Nop);
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, autoHideField));
            il.Append(Instruction.Create(OpCodes.Brfalse, skipTimerStart));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, timerField));
            il.Append(Instruction.Create(OpCodes.Callvirt, timerStartRef));
            il.Append(skipTimerStart);
            il.Append(Instruction.Create(OpCodes.Br, afterEnterLeave));
            // else if (!isOver && mouseOver) OnMouseLeave(...)
            il.Append(elseLeaveCheck);
            il.Append(Instruction.Create(OpCodes.Brtrue, afterEnterLeave));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, mouseOverField));
            il.Append(Instruction.Create(OpCodes.Brfalse, afterEnterLeave));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldsfld, eventArgsEmptyRef));
            il.Append(Instruction.Create(OpCodes.Callvirt, module.ImportReference(onMouseLeaveMethod)));

            il.Append(afterEnterLeave);
            il.Append(Instruction.Create(OpCodes.Brfalse, afterMove));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Ldloca, pointLocal));
            il.Append(Instruction.Create(OpCodes.Call, pointGetXRef));
            il.Append(Instruction.Create(OpCodes.Ldloca, pointLocal));
            il.Append(Instruction.Create(OpCodes.Call, pointGetYRef));
            il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            il.Append(Instruction.Create(OpCodes.Newobj, mouseEventArgsCtorRef));
            il.Append(Instruction.Create(OpCodes.Callvirt, module.ImportReference(onMouseMoveMethod)));

            il.Append(afterMove);
            il.Append(ret);
        }

        Console.WriteLine($"OK   {fileName}: {layeredFormType}::WndProc now synthesizes a MouseMove event from WM_SETCURSOR's WM_MOUSEMOVE hint (same mechanism already used for Click); {notifType}::OnShown subscribes and forwards to the existing OnMouseEnter/OnMouseMove/OnMouseLeave, restoring hover-pause-the-auto-hide-timer behavior");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-timer-kick <input-dir> <output-dir>
//
// Ninth-round follow-up test (reports/notification-empty-until-fade-findings.md): a
// wall-clock-synchronized recording confirmed content appears within ~33-67ms of Hide() actually
// running, after a full ~6s of confirmed-blank display despite the correct bitmap reaching the
// window within ~30ms of the notification appearing. Hide()'s Visible->Disappearing branch does
// `timer.Interval = 25; timer.Start();` on an *already-running* Timer -- under the hood, a native
// SetTimer call even though the timer never stopped. Does that re-arm alone unstick the
// compositor, independent of everything else Hide() does (state -> Disappearing, alphaIncrement,
// starting a real opacity fade)?
//
// Test: force the Visible-hold auto-hide timer to fire quickly (2s instead of the real
// timeToStay) via an override write right after OnShown's own `timer.Interval = timeToStay;` in
// its Appearing/Visible case. The first time timer_OnTimer's Visible-branch would normally call
// Hide(), it's redirected (by swapping that one call instruction's operand, not by restructuring
// any control flow) to a new __diagKickOrHide() method instead: the first call just re-arms the
// timer (Interval = the real timeToStay field, Start()) and returns -- no state change, no
// Opacity change, nothing else Hide() would do; the second call (the real timeToStay later) calls
// Hide() as normal, via a new private bool field tracking which call this is. If content appears
// right around the first (kick-only) tick, the timer re-arm itself is the trigger; if it stays
// blank until the second tick's real Hide(), the re-arm alone isn't sufficient.
//
// Tenth round found the timer re-arm alone unsticks the avatar/icon (~70-110ms after the kick)
// but NOT the text, which stayed invisible for at least 4.5 more seconds of recording. The
// `alsoInvalidate` variant (--patch-notification-timer-kick-invalidate) additionally calls
// `this.Invalidate();` in the kick branch, testing whether forcing a genuine WM_PAINT dispatch --
// something every real Hide()-driven fade tick does on each 25ms step, but the idle Visible-hold
// and the plain kick never do -- is what specifically unsticks text. Result: still no, text stays
// blank right up to the real Hide().
//
// --patch-notification-timer-kick-state-flip (`alsoFlipState`) narrows further: does the `state`
// field itself actually changing to Disappearing (even transiently, flipped straight back to
// Visible in the same call, no real fade) unstick text, independent of Invalidate() (already
// ruled out) and without an actual visible fade? Also uses a much shorter `kickAfterMs` (100ms
// instead of 2000ms) -- prompted by the user's question about whether the avatar could render
// close to immediately rather than after an arbitrary delay: since the kick unsticks the avatar
// in ~70-110ms regardless of when it fires, firing it almost immediately after the notification
// appears (instead of waiting) should make the avatar appear near-instantly instead of after a
// multi-second wait, independent of whatever this round finds about text.
static int RunPatchNotificationTimerKick(string[] args, bool alsoInvalidate, bool alsoFlipState, int kickAfterMs)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-timer-kick[-invalidate|-state-flip] <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (alsoFlipState && stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }

        // Resolve Timer's set_Interval/Start from timerField's own FieldType, not via typeof()
        // reflection -- System.Windows.Forms is an app-deployed assembly, same version-mismatch
        // trap as CLAUDE.md's IL-patching lesson 5.
        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        MethodReference? invalidateRef = null;
        if (alsoInvalidate)
        {
            // Invalidate() is inherited from System.Windows.Forms.Control -- walk the base-type
            // chain to resolve it, not typeof() reflection (CLAUDE.md's IL-patching lesson 5).
            TypeDefinition? controlType = type;
            while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
            {
                controlType = controlType.BaseType?.Resolve();
            }
            if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
            var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
            if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
            invalidateRef = module.ImportReference(invalidateDef);
        }

        // --- Step 1: OnShown -- override the Visible-transition's `timer.Interval = timeToStay;`
        // with a hardcoded short interval, right after the original call (leaving the original
        // instructions untouched -- just adding a second write that wins).
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;
            Instruction? anchor = null;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    idx >= 1 && instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference fr && fr.Name == "timeToStay")
                {
                    if (anchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    anchor = instrs[idx];
                }
            }
            if (anchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            var il = body.GetILProcessor();
            // Insert *after* anchor -- reverse order since each InsertAfter(anchor, x) lands
            // immediately after anchor, so building forward means inserting the last instruction
            // first (CLAUDE.md lesson 1's mirror image for InsertAfter instead of InsertBefore).
            var call = Instruction.Create(OpCodes.Call, setIntervalRef);
            var pushMs = Instruction.Create(OpCodes.Ldc_I4, kickAfterMs);
            var pushTimer = Instruction.Create(OpCodes.Ldfld, timerField);
            var pushThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(anchor, pushThis);
            il.InsertAfter(pushThis, pushTimer);
            il.InsertAfter(pushTimer, pushMs);
            il.InsertAfter(pushMs, call);
        }

        // --- Step 2: add `private bool __diagTimerKicked;` field.
        var kickedField = new FieldDefinition("__diagTimerKicked", FieldAttributes.Private, module.TypeSystem.Boolean);
        type.Fields.Add(kickedField);

        // --- Step 3: add `private void __diagKickOrHide() { if (!__diagTimerKicked) { ...
        // re-arm ... } else { Hide(); } }`.
        var kickMethod = new MethodDefinition("__diagKickOrHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(kickMethod);
        var kickBody = kickMethod.Body;
        var kickIl = kickBody.GetILProcessor();
        var elseLabel = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, kickedField));
        kickIl.Append(Instruction.Create(OpCodes.Brtrue, elseLabel));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_1));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, kickedField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        kickIl.Append(Instruction.Create(OpCodes.Call, setIntervalRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Call, startRef));
        if (alsoFlipState)
        {
            // state = Disappearing; state = Visible; -- transiently flips the field and straight
            // back, in the same call, with no real fade -- tests whether the field actually
            // changing (even momentarily) is what unsticks text, independent of Invalidate()
            // (already ruled out) and without ever letting Opacity move.
            kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_3));
            kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
            kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
            kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
        }
        if (alsoInvalidate)
        {
            kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            kickIl.Append(Instruction.Create(OpCodes.Callvirt, invalidateRef));
        }
        kickIl.Append(Instruction.Create(OpCodes.Br, ret));
        kickIl.Append(elseLabel); // Ldarg_0, reused as the else-branch's first instruction
        kickIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        kickIl.Append(ret);

        // --- Step 4: timer_OnTimer -- redirect the single `Hide()` call to `__diagKickOrHide()`
        // by swapping just its operand (same calling shape: `call instance void
        // FormGenericNotification::X()`), not restructuring any control flow.
        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = kickMethod;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- forced Visible-hold timer to {kickAfterMs}ms; {targetType}::timer_OnTimer -- first tick now re-arms{(alsoFlipState ? " + flips state to Disappearing and back" : "")}{(alsoInvalidate ? " + Invalidate()s" : "")} via __diagKickOrHide() instead of calling Hide(), second tick calls Hide() as normal");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-real-fade-abort <input-dir> <output-dir>
//
// Eleventh-round test (reports/notification-empty-until-fade-findings.md): three isolation
// attempts (plain Timer re-arm, re-arm+Invalidate(), re-arm+transient state-flip) all unstuck the
// avatar but never the text. All three were also *instantaneous* -- state/Opacity written and
// (for the flip variant) written back in the very same call, no real elapsed time with the field
// actually holding its changed value across an actual timer tick. This test drives the REAL,
// unmodified fade-tick code path instead of imitating it: sets state = Disappearing and
// alphaIncrement exactly as Hide()'s Visible branch does, re-arms the timer to 25ms, then pumps
// the message loop (Application.DoEvents() + Thread.Sleep(), the same proven-safe technique
// `--patch-notification-invalidate`'s dip-and-recover step already used successfully in this
// codebase) for ~120ms -- long enough for several *genuine* WM_TIMER-dispatched ticks to run
// through timer_OnTimer's own unmodified Disappearing-branch code (real Opacity decrement, real
// updateLayeredBackground(false), real Invalidate() on each tick) -- before aborting: snapping
// Opacity back to 1.0, state back to Visible, alphaIncrement to 0, and re-arming the timer for
// the real remaining timeToStay. If text appears during the pumped window (even if the abort
// then makes it invisible again, or leaves it visible at full opacity), a few genuine fade ticks
// are what's needed, not any single field write in isolation. If it still doesn't, the trigger is
// something tied to the fade actually completing (reaching Opacity 0 and calling
// setFormHidden()), not merely being underway.
// Result: text DID render during the pumped real fade, and reverted to blank within ~35ms of the
// abort's `Opacity = 1.0`. Wine's winex11.drv takes a genuinely different path depending on
// whether the resolved alpha is *exactly* 255 (Opacity == 1.0) -- XChangeProperty for any other
// value, XDeleteProperty specifically at exactly 1.0 (see round 9's reading of
// X11DRV_SetLayeredWindowAttributes). The `abortOpacity` parameter (wired to
// --patch-notification-real-fade-abort-0999, landing at 0.999 instead of exactly 1.0, state
// still returns to Visible as before) tests whether avoiding that exact value keeps text
// visible -- isolating the opacity-property code path from the state field itself.
static int RunPatchNotificationRealFadeAbort(string[] args, double abortOpacity)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-real-fade-abort[-0999] <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int kickAfterMs = 100;
    const int pumpStepMs = 30;
    const int pumpSteps = 4; // ~120ms of real elapsed time, pumped -- several genuine 25ms ticks

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        // Resolve Timer's set_Interval/Start from timerField's own FieldType, not via typeof()
        // reflection -- System.Windows.Forms is an app-deployed assembly, same version-mismatch
        // trap as CLAUDE.md's IL-patching lesson 5.
        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        // Invalidate() (Control), Opacity (Form), and Application.DoEvents() -- same base-type-
        // chain-walk technique as --patch-notification-invalidate, not typeof() reflection.
        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);

        // --- Step 1: OnShown -- override the Visible-transition's `timer.Interval = timeToStay;`
        // with a hardcoded short interval, right after the original call.
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;
            Instruction? anchor = null;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference fr && fr.Name == "timeToStay")
                {
                    if (anchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    anchor = instrs[idx];
                }
            }
            if (anchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            var il = body.GetILProcessor();
            var call = Instruction.Create(OpCodes.Call, setIntervalRef);
            var pushMs = Instruction.Create(OpCodes.Ldc_I4, kickAfterMs);
            var pushTimer = Instruction.Create(OpCodes.Ldfld, timerField);
            var pushThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(anchor, pushThis);
            il.InsertAfter(pushThis, pushTimer);
            il.InsertAfter(pushTimer, pushMs);
            il.InsertAfter(pushMs, call);
        }

        // --- Step 2: add `private bool __diagTimerKicked;` field.
        var kickedField = new FieldDefinition("__diagTimerKicked", FieldAttributes.Private, module.TypeSystem.Boolean);
        type.Fields.Add(kickedField);

        // --- Step 3: add `private void __diagKickOrHide()`:
        //   if (!__diagTimerKicked) {
        //       __diagTimerKicked = true;
        //       state = Disappearing; alphaIncrement = -0.05f; timer.Interval = 25; timer.Start();
        //       for (4 times) { Invalidate(); DoEvents(); Sleep(30); }  // real ticks run here
        //       Opacity = 1.0; state = Visible; alphaIncrement = 0f;
        //       updateLayeredBackground(false); Invalidate();
        //       timer.Interval = timeToStay; timer.Start();
        //   } else { Hide(); }
        var kickMethod = new MethodDefinition("__diagKickOrHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(kickMethod);
        var kickBody = kickMethod.Body;
        var kickIl = kickBody.GetILProcessor();
        var elseLabel = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, kickedField));
        kickIl.Append(Instruction.Create(OpCodes.Brtrue, elseLabel));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_1));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, kickedField));

        // state = Disappearing; alphaIncrement = -0.05f; timer.Interval = 25; timer.Start();
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_3));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R4, -0.05f));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4, 25));
        kickIl.Append(Instruction.Create(OpCodes.Call, setIntervalRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Call, startRef));

        // Pump the message loop so genuine WM_TIMER-dispatched ticks can run through
        // timer_OnTimer's own unmodified code for real (fully unrolled, no branches introduced).
        for (int i = 0; i < pumpSteps; i++)
        {
            kickIl.Append(Instruction.Create(OpCodes.Call, doEventsRef));
            kickIl.Append(Instruction.Create(OpCodes.Ldc_I4, pumpStepMs));
            kickIl.Append(Instruction.Create(OpCodes.Call, threadSleepRef));
        }

        // Abort: Opacity = abortOpacity; state = Visible; alphaIncrement = 0f;
        // updateLayeredBackground(false); Invalidate(); timer.Interval = timeToStay; timer.Start();
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R8, abortOpacity));
        kickIl.Append(Instruction.Create(OpCodes.Call, setOpacityRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R4, 0f));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        kickIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Call, invalidateRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        kickIl.Append(Instruction.Create(OpCodes.Call, setIntervalRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Call, startRef));
        kickIl.Append(Instruction.Create(OpCodes.Br, ret));
        kickIl.Append(elseLabel); // Ldarg_0, reused as the else-branch's first instruction
        kickIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        kickIl.Append(ret);

        // --- Step 4: timer_OnTimer -- redirect the single `Hide()` call to `__diagKickOrHide()`.
        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = kickMethod;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- forced Visible-hold timer to {kickAfterMs}ms; {targetType}::timer_OnTimer -- first tick now runs a real ~{pumpSteps * pumpStepMs}ms fade (state=Disappearing, real ticks pumped) then aborts back to Visible/Opacity={abortOpacity}, via __diagKickOrHide() instead of calling Hide(); second tick calls Hide() as normal");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-state-vs-alpha <input-dir> <output-dir>
//
// Twelfth-round test, directly prompted by the user's question about whether the notification
// could just be held in "whatever state it's in right before text hides" for the whole display
// hold, instead of chasing the exact Wine mechanism. Round eleven found text renders during a
// real fade and reverts once `state` returns to Visible -- but every abort tried so far reset
// BOTH `state` (to Visible) AND `alphaIncrement` (to 0) together, so it's still unknown which one
// actually matters. This test separates them: after the same real-pumped-fade opening as
// --patch-notification-real-fade-abort, it flips `state` back to Visible while deliberately
// LEAVING `alphaIncrement` at a small non-zero value (-0.01f, chosen small enough that a few more
// ticks barely move Opacity) for a second pumped window (~90ms, several more genuine ticks) --
// then, only at the very end, does the real cleanup (alphaIncrement = 0, Opacity = 1.0 exactly,
// normal timer re-arm) so the notification doesn't end up in the same stuck-forever state
// `--patch-notification-real-fade-abort-0999` hit (Opacity/alphaIncrement combination must
// satisfy timer_OnTimer's own `>= 1.0` re-entry condition by the time this method returns).
//
// If text stays visible through the state==Visible+alphaIncrement!=0 window (frames captured
// mid-way, before the final real cleanup), alphaIncrement is what matters, not state -- and the
// user's "hold the pre-hide state" idea is directly buildable on `state == Visible` (the normal,
// side-effect-free value; see the OnMouseEnter reshow-on-hover check and Hide()'s own
// Disappearing-is-a-no-op case, both keyed on `state == Disappearing` specifically, which is why
// holding *that* state for the whole display hold would be risky). If text disappears the moment
// `state` flips back to Visible regardless of `alphaIncrement`, state itself is confirmed as the
// gate, and the "hold the pre-hide state" strategy would need to hold `state == Disappearing`
// with the guard logic that implies.
static int RunPatchNotificationStateVsAlpha(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-state-vs-alpha <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int kickAfterMs = 100;
    const int pumpStepMs = 30;
    const int pumpSteps = 4;       // first pumped window: real fade, state=Disappearing
    const int secondPumpSteps = 3; // second pumped window: state=Visible, alphaIncrement!=0

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);

        // --- Step 1: OnShown -- override the Visible-transition's `timer.Interval = timeToStay;`
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;
            Instruction? anchor = null;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference fr && fr.Name == "timeToStay")
                {
                    if (anchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    anchor = instrs[idx];
                }
            }
            if (anchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            var il = body.GetILProcessor();
            var call = Instruction.Create(OpCodes.Call, setIntervalRef);
            var pushMs = Instruction.Create(OpCodes.Ldc_I4, kickAfterMs);
            var pushTimer = Instruction.Create(OpCodes.Ldfld, timerField);
            var pushThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(anchor, pushThis);
            il.InsertAfter(pushThis, pushTimer);
            il.InsertAfter(pushTimer, pushMs);
            il.InsertAfter(pushMs, call);
        }

        var kickedField = new FieldDefinition("__diagTimerKicked", FieldAttributes.Private, module.TypeSystem.Boolean);
        type.Fields.Add(kickedField);

        var kickMethod = new MethodDefinition("__diagKickOrHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(kickMethod);
        var kickBody = kickMethod.Body;
        var kickIl = kickBody.GetILProcessor();
        var elseLabel = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, kickedField));
        kickIl.Append(Instruction.Create(OpCodes.Brtrue, elseLabel));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_1));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, kickedField));

        // Phase A: state = Disappearing; alphaIncrement = -0.05f; timer.Interval = 25; Start();
        // pump ~120ms of genuine ticks (same as --patch-notification-real-fade-abort).
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_3));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R4, -0.05f));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4, 25));
        kickIl.Append(Instruction.Create(OpCodes.Call, setIntervalRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Call, startRef));
        for (int i = 0; i < pumpSteps; i++)
        {
            kickIl.Append(Instruction.Create(OpCodes.Call, doEventsRef));
            kickIl.Append(Instruction.Create(OpCodes.Ldc_I4, pumpStepMs));
            kickIl.Append(Instruction.Create(OpCodes.Call, threadSleepRef));
        }

        // Phase B: state = Visible (back to the normal, side-effect-free value) but
        // alphaIncrement STAYS non-zero (-0.01f, small) -- the field this phase is testing.
        // Opacity is left wherever phase A's real ticks put it (no explicit set here). Pump a
        // second ~90ms window of genuine ticks with this combination in effect.
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, stateField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R4, -0.01f));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        for (int i = 0; i < secondPumpSteps; i++)
        {
            kickIl.Append(Instruction.Create(OpCodes.Call, doEventsRef));
            kickIl.Append(Instruction.Create(OpCodes.Ldc_I4, pumpStepMs));
            kickIl.Append(Instruction.Create(OpCodes.Call, threadSleepRef));
        }

        // Real cleanup: alphaIncrement = 0; Opacity = 1.0 exactly; updateLayeredBackground(false);
        // Invalidate(); timer.Interval = timeToStay; timer.Start() -- must land here so
        // timer_OnTimer's own `>= 1.0` re-entry condition is satisfied (Opacity==1.0,
        // alphaIncrement==0), avoiding the stuck-forever loop
        // --patch-notification-real-fade-abort-0999 hit.
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R4, 0f));
        kickIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_R8, 1.0));
        kickIl.Append(Instruction.Create(OpCodes.Call, setOpacityRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        kickIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Call, invalidateRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        kickIl.Append(Instruction.Create(OpCodes.Call, setIntervalRef));
        kickIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kickIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        kickIl.Append(Instruction.Create(OpCodes.Call, startRef));
        kickIl.Append(Instruction.Create(OpCodes.Br, ret));
        kickIl.Append(elseLabel);
        kickIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        kickIl.Append(ret);

        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = kickMethod;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- forced Visible-hold timer to {kickAfterMs}ms; {targetType}::timer_OnTimer -- first tick runs a real fade, then flips state back to Visible while keeping alphaIncrement non-zero for a second pumped window, then does real cleanup, via __diagKickOrHide() instead of calling Hide(); second tick calls Hide() as normal");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive <input-dir> <output-dir>
//
// First real fix attempt (not a diagnostic probe) for the empty-box-until-fade bug, built on
// round twelve's finding: `alphaIncrement` remaining non-zero (i.e. real 25ms ticks actively
// running) is what keeps notification text visible -- `state` itself is not the gate. The
// current design holds the Visible state with `alphaIncrement = 0f` and a single long
// `timer.Interval = timeToStay` wait -- an inert, non-ticking hold, which is exactly the
// condition round twelve showed goes blank.
//
// Fix: override two of OnShown's own writes in its Appearing/Visible case (both via the same
// insert-an-override-right-after-the-original technique used throughout this investigation, not
// replacing the original instructions):
//   - `alphaIncrement = 0f;` -> `alphaIncrement = 0.0001f;` (tiny positive, not zero)
//   - `timer.Interval = timeToStay;` -> `timer.Interval = 25;` (real ticks, not one long wait)
// With alphaIncrement slightly positive, `Opacity + alphaIncrement` is always >= 1.0, so every
// 25ms tick re-enters timer_OnTimer's *existing, unmodified* first branch, which clamps `Opacity`
// back to exactly 1.0 and calls `updateLayeredBackground(refreshBitmap: false)` unconditionally --
// keeping real ticks flowing (and Opacity visibly pinned at 1.0, no drift) for the whole display
// hold, instead of the current single inert wait.
//
// That same branch is also where the existing Visible-state Hide() call lives, which would now
// fire on almost every tick rather than once after timeToStay -- so its call site (the same one
// every prior kick test in this investigation redirected) is pointed at a new
// __keepAliveMaybeHide() method instead: it tracks real elapsed time via Environment.TickCount
// (recorded in a new field `__keepAliveStartTick`, set at the same point as the two overrides
// above) and only calls the real, unmodified `Hide()` once `timeToStay` has genuinely elapsed --
// otherwise it's a no-op, leaving Opacity already correctly clamped to 1.0 by the surrounding
// code that ran just before this call on every tick. This preserves the real auto-hide timing
// exactly, decoupled from the tick interval. No existing method's control flow is altered --
// only field-write overrides and one call-site operand swap, the same safe techniques already
// verified throughout this investigation.
static int RunPatchNotificationKeepAlive(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const float keepAliveAlpha = 0.0001f;
    const int keepAliveIntervalMs = 25;

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);

        var tickCountGetterDef = typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!;
        var tickCountGetterRef = module.ImportReference(tickCountGetterDef);

        // --- Step 1: OnShown -- find the Visible case's `alphaIncrement = 0f;` (the simple
        // constant-zero assignment; the Hidden/Disappearing cases assign `25f / timeToShow`
        // instead, a different instruction shape, so this uniquely identifies the right site) and
        // override it right after with the tiny keep-alive value; then find the same case's
        // `timer.Interval = timeToStay;` (same anchor-finding code used by every kick test this
        // session) and override it right after with the short real-tick interval.
        Instruction? afterOverridePoint;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? alphaZeroAnchor = null;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if (instrs[idx].OpCode == OpCodes.Stfld && instrs[idx].Operand is FieldReference afr && afr.Name == "alphaIncrement" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldc_R4 && instrs[idx - 1].Operand is float fv && fv == 0f)
                {
                    if (alphaZeroAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `alphaIncrement = 0f;` found in OnShown -- method shape changed, review needed"); return 1; }
                    alphaZeroAnchor = instrs[idx];
                }
            }
            if (alphaZeroAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `alphaIncrement = 0f;` in OnShown"); return 1; }

            Instruction? intervalAnchor = null;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            var il = body.GetILProcessor();

            // Override alphaIncrement right after its original write.
            var alphaCall = Instruction.Create(OpCodes.Stfld, alphaIncrementField);
            var alphaPush = Instruction.Create(OpCodes.Ldc_R4, keepAliveAlpha);
            var alphaThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(alphaZeroAnchor, alphaThis);
            il.InsertAfter(alphaThis, alphaPush);
            il.InsertAfter(alphaPush, alphaCall);

            // Override timer.Interval right after its original write, and capture
            // Environment.TickCount into the new field right after that (see Step 2).
            var intervalCall = Instruction.Create(OpCodes.Call, setIntervalRef);
            var intervalPush = Instruction.Create(OpCodes.Ldc_I4, keepAliveIntervalMs);
            var intervalTimer = Instruction.Create(OpCodes.Ldfld, timerField);
            var intervalThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(intervalAnchor, intervalThis);
            il.InsertAfter(intervalThis, intervalTimer);
            il.InsertAfter(intervalTimer, intervalPush);
            il.InsertAfter(intervalPush, intervalCall);
            afterOverridePoint = intervalCall;
        }

        // --- Step 2: add `private int __keepAliveStartTick;` field, and record
        // Environment.TickCount into it right after the timer.Interval override above.
        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);
        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var stfldStart = Instruction.Create(OpCodes.Stfld, startTickField);
            var callTick = Instruction.Create(OpCodes.Call, tickCountGetterRef);
            var ldThis = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertAfter(afterOverridePoint, ldThis);
            il.InsertAfter(ldThis, callTick);
            il.InsertAfter(callTick, stfldStart);
        }

        // --- Step 3: add `private void __keepAliveMaybeHide() { if (Environment.TickCount -
        // __keepAliveStartTick < timeToStay) return; Hide(); }`.
        var keepAliveMethod = new MethodDefinition("__keepAliveMaybeHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(keepAliveMethod);
        var kaBody = keepAliveMethod.Body;
        var kaIl = kaBody.GetILProcessor();
        var ret = Instruction.Create(OpCodes.Ret);
        var doHide = Instruction.Create(OpCodes.Ldarg_0);

        kaIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        kaIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kaIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        kaIl.Append(Instruction.Create(OpCodes.Sub));
        kaIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kaIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        kaIl.Append(Instruction.Create(OpCodes.Blt, ret));
        kaIl.Append(doHide); // Ldarg_0, reused as the call's target-push
        kaIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        kaIl.Append(ret);

        // --- Step 4: timer_OnTimer -- redirect the single `Hide()` call to
        // `__keepAliveMaybeHide()` by swapping just its operand.
        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = keepAliveMethod;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now uses alphaIncrement={keepAliveAlpha}/timer.Interval={keepAliveIntervalMs}ms instead of 0/timeToStay, tracking real elapsed time in __keepAliveStartTick; {targetType}::timer_OnTimer -- Hide() call redirected to __keepAliveMaybeHide() which only calls the real Hide() once timeToStay has genuinely elapsed");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive-v2 <input-dir> <output-dir>
//
// --patch-notification-keep-alive (v1) alone was tested live and did NOT fix text: it kept
// `alphaIncrement` non-zero and ticking for the whole hold from a cold start, and confirmed the
// avatar rendered early (consistent with earlier rounds) but text still only appeared right after
// the real Hide(). Corrected understanding: round twelve's "alphaIncrement staying non-zero keeps
// text visible" finding only held *after* first passing through a real Disappearing-state fade
// (its phase A) -- ticking in Visible state alone, from a cold start, never unlocks text in the
// first place. v2 is two-phase: phase 1 pulses a brief *real* Disappearing-state fade right at
// notification setup (same DoEvents()+Thread.Sleep() pump technique as the diagnostic tests, now
// used in a real fix rather than a probe) to unlock text, then phase 2 switches to v1's sustained
// Visible-state ticking (tiny non-zero alphaIncrement, real 25ms ticks) to keep it unlocked for
// the rest of the display hold, before the real Hide() fires at the genuine configured timeout
// (tracked the same way as v1, via Environmet.TickCount vs timeToStay). Everything happens inside
// OnShown before it returns (a one-time ~100-150ms synchronous pump when a notification first
// shows, the same technique already proven safe throughout this investigation) -- timer_OnTimer's
// own code is touched only via the same Hide()-call-site operand swap v1 already uses; no new
// branch logic is added to its existing branches.
static int RunPatchNotificationKeepAliveV2(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive-v2 <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int unlockPumpSteps = 4;
    const int unlockPumpStepMs = 30;
    const float unlockAlpha = -0.05f;
    const float sustainAlpha = 0.0001f;
    const int sustainIntervalMs = 25;

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);
        var tickCountGetterRef = module.ImportReference(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);

        // --- Step 1: OnShown -- find the Visible case's `timer.Interval = timeToStay;` (same
        // anchor as every prior test this session), then scan forward for the *next*
        // `timer.Start()` call after it -- inside this same case's `if (autoHide) { timer.Start();
        // }` block. Insert the whole phase-1-pump + phase-2-setup sequence right after that
        // Start() call, still inside the same try block, well before the method's own `leave.s`.
        Instruction? insertAfter;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? intervalAnchor = null;
            int intervalIdx = -1;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                    intervalIdx = idx;
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            Instruction? startAnchor = null;
            for (int idx = intervalIdx + 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference smr && smr.Name == "Start" && smr.Parameters.Count == 0 &&
                    smr.DeclaringType.FullName == timerType!.FullName)
                {
                    startAnchor = instrs[idx];
                    break;
                }
            }
            if (startAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find the Visible case's `timer.Start();` after `timer.Interval = timeToStay;` in OnShown"); return 1; }

            foreach (var instr in instrs)
            {
                if (instr.Operand == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == startAnchor || handler.TryEnd == startAnchor ||
                    handler.HandlerStart == startAnchor || handler.HandlerEnd == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }

            insertAfter = startAnchor;
        }

        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }

            // Phase 1: state = Disappearing; alphaIncrement = unlockAlpha; timer.Interval = 25;
            // timer.Start(); pump `unlockPumpSteps` genuine ticks.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_3),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, unlockAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, sustainIntervalMs),
                Instruction.Create(OpCodes.Call, setIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Call, startRef)
            );
            for (int i = 0; i < unlockPumpSteps; i++)
            {
                Emit(
                    Instruction.Create(OpCodes.Call, doEventsRef),
                    Instruction.Create(OpCodes.Ldc_I4, unlockPumpStepMs),
                    Instruction.Create(OpCodes.Call, threadSleepRef)
                );
            }

            // Phase 2: state = Visible; alphaIncrement = sustainAlpha (tiny, keeps real ticks
            // flowing for the rest of the hold); Opacity = 1.0 (undo phase 1's real dip);
            // updateLayeredBackground(false); Invalidate(); record __keepAliveStartTick *now* (so
            // the real timeToStay countdown starts after the unlock pulse, not before it).
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_2),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, sustainAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, 1.0),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, tickCountGetterRef),
                Instruction.Create(OpCodes.Stfld, startTickField)
            );
        }

        var keepAliveMethod = new MethodDefinition("__keepAliveMaybeHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(keepAliveMethod);
        var kaBody = keepAliveMethod.Body;
        var kaIl = kaBody.GetILProcessor();
        var ret = Instruction.Create(OpCodes.Ret);

        kaIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        kaIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kaIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        kaIl.Append(Instruction.Create(OpCodes.Sub));
        kaIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kaIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        kaIl.Append(Instruction.Create(OpCodes.Blt, ret));
        kaIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        kaIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        kaIl.Append(ret);

        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = keepAliveMethod;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now pulses a brief real Disappearing-state fade ({unlockPumpSteps} pumped ticks) to unlock text, then switches to sustained Visible-state ticking (alphaIncrement={sustainAlpha}, timer.Interval={sustainIntervalMs}ms) for the rest of the hold; {targetType}::timer_OnTimer -- Hide() call redirected to __keepAliveMaybeHide() which only calls the real Hide() once timeToStay has genuinely elapsed since the unlock pulse");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive-v3 <input-dir> <output-dir>
//
// v2 tested live and still failed: text appeared briefly during the unlock pulse but reverted
// once phase 2 began, staying blank until the real Hide(). Root cause of v2's own failure: its
// sustain phase used a *positive* alphaIncrement (0.0001), chosen so `Opacity + alphaIncrement`
// stays >= 1.0 -- but that means every tick hits timer_OnTimer's *first* branch, which clamps
// Opacity back to the *same* constant 1.0 every time. Opacity's observed value never actually
// changes tick-to-tick. Round twelve's success used a *negative* alphaIncrement (-0.01), which
// hits the *third* branch instead (`Opacity += alphaIncrement`) -- a real, different Opacity
// value every tick. That distinction -- Opacity genuinely changing value each tick, not merely
// "alphaIncrement != 0" -- looks like the actual gate.
//
// v3's sustain phase uses a small *negative* alphaIncrement so the third branch runs on every
// tick for the whole remaining hold (real per-tick Opacity change, matching round twelve). Picked
// small enough (-0.0001) that even over a full ~6s hold (240 ticks at 25ms) the total drift stays
// under 3% (barely perceptible) with no periodic correction needed. The real complication: with
// alphaIncrement persistently negative, the first branch (where v1/v2's Hide()-redirect lived)
// never fires again, so that redirect point is unreachable here. Instead, this redirects
// timer_OnTimer's two `updateLayeredBackground(bool)` call sites (one in each of the first and
// third branches -- both real, existing call sites, still just an operand swap, not new control
// flow) to a new __keepAliveTick(bool) wrapper that runs on literally every tick regardless of
// which branch: if real elapsed time (Environment.TickCount vs timeToStay, tracked exactly as v1/
// v2 did) has passed and state is still Visible, it calls the real Hide() instead of updating the
// background; otherwise it calls the real, original updateLayeredBackground(bool) normally.
// Self-limiting: once Hide() runs, state moves to Disappearing, so the condition naturally stops
// re-triggering on later ticks -- no extra "already hidden" flag needed.
static int RunPatchNotificationKeepAliveV3(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive-v3 <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int unlockPumpSteps = 4;
    const int unlockPumpStepMs = 30;
    const float unlockAlpha = -0.05f;
    const float sustainAlpha = -0.0001f;
    const int sustainIntervalMs = 25;

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);
        var tickCountGetterRef = module.ImportReference(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);

        // --- Step 1: OnShown -- same anchor-finding as v2 (find `timer.Interval = timeToStay;`,
        // then the next `timer.Start()` after it, inside the Visible case's `if (autoHide)` block).
        Instruction? insertAfter;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? intervalAnchor = null;
            int intervalIdx = -1;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                    intervalIdx = idx;
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            Instruction? startAnchor = null;
            for (int idx = intervalIdx + 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference smr && smr.Name == "Start" && smr.Parameters.Count == 0 &&
                    smr.DeclaringType.FullName == timerType!.FullName)
                {
                    startAnchor = instrs[idx];
                    break;
                }
            }
            if (startAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find the Visible case's `timer.Start();` after `timer.Interval = timeToStay;` in OnShown"); return 1; }

            foreach (var instr in instrs)
            {
                if (instr.Operand == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == startAnchor || handler.TryEnd == startAnchor ||
                    handler.HandlerStart == startAnchor || handler.HandlerEnd == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }

            insertAfter = startAnchor;
        }

        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }

            // Phase 1: state = Disappearing; alphaIncrement = unlockAlpha; timer.Interval = 25;
            // timer.Start(); pump `unlockPumpSteps` genuine ticks -- unchanged from v2.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_3),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, unlockAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, sustainIntervalMs),
                Instruction.Create(OpCodes.Call, setIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Call, startRef)
            );
            for (int i = 0; i < unlockPumpSteps; i++)
            {
                Emit(
                    Instruction.Create(OpCodes.Call, doEventsRef),
                    Instruction.Create(OpCodes.Ldc_I4, unlockPumpStepMs),
                    Instruction.Create(OpCodes.Call, threadSleepRef)
                );
            }

            // Phase 2: state = Visible; alphaIncrement = sustainAlpha (small NEGATIVE -- keeps
            // the real third branch running every tick, genuinely changing Opacity, for the rest
            // of the hold); Opacity = 1.0 (undo phase 1's real dip, fresh baseline for the slow
            // drift to start from); updateLayeredBackground(false); Invalidate(); record
            // __keepAliveStartTick now (real timeToStay countdown starts after the unlock pulse).
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_2),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, sustainAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, 1.0),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, tickCountGetterRef),
                Instruction.Create(OpCodes.Stfld, startTickField)
            );
        }

        // --- New wrapper: __keepAliveTick(bool refreshBitmap) { if (state == Visible &&
        // Environment.TickCount - __keepAliveStartTick >= timeToStay) { Hide(); } else {
        // updateLayeredBackground(refreshBitmap); } }
        var tickWrapper = new MethodDefinition("__keepAliveTick", MethodAttributes.Private,
            module.TypeSystem.Void);
        tickWrapper.Parameters.Add(new ParameterDefinition("refreshBitmap", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(tickWrapper);
        var twIl = tickWrapper.Body.GetILProcessor();
        var elseLabel = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, stateField));
        twIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        twIl.Append(Instruction.Create(OpCodes.Bne_Un, elseLabel));
        twIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        twIl.Append(Instruction.Create(OpCodes.Sub));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        twIl.Append(Instruction.Create(OpCodes.Blt, elseLabel));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        twIl.Append(Instruction.Create(OpCodes.Br, ret));
        twIl.Append(elseLabel); // Ldarg_0, reused as the else-branch's target push
        twIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        twIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        twIl.Append(ret);

        // --- Redirect timer_OnTimer's two `updateLayeredBackground(bool)` call sites (one in the
        // first branch, one in the third) to the wrapper above -- operand swap only, same
        // technique used throughout this investigation, no control-flow changes.
        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int callCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "updateLayeredBackground")
                {
                    instr.Operand = tickWrapper;
                    callCount++;
                }
            }
            if (callCount != 2) { Console.Error.WriteLine($"FAIL: expected exactly 2 updateLayeredBackground(bool) calls in timer_OnTimer, found {callCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now pulses a brief real Disappearing-state fade ({unlockPumpSteps} pumped ticks) to unlock text, then switches to sustained Visible-state ticking with a small NEGATIVE alphaIncrement={sustainAlpha} (genuine per-tick Opacity change, not a static clamp) for the rest of the hold; {targetType}::timer_OnTimer -- both updateLayeredBackground(bool) call sites redirected to __keepAliveTick(bool), which calls the real Hide() once timeToStay has genuinely elapsed since the unlock pulse, otherwise updates normally");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive-v4 <input-dir> <output-dir>
//
// v3 tested live: a real, qualitative improvement -- text was genuinely visible for most of the
// hold instead of fully blank -- but the drift was far larger than intended: user reported it
// fading "a lot, to almost not seen" over the real ~6.27s hold, versus the ~2.5% drift the fixed
// -0.0001 constant was expected to produce at that duration (240ish ticks). Rather than chase why
// the fixed constant produced more drift than calculated, v4 removes the guesswork: it computes
// the per-tick decrement *dynamically* at runtime from the real `timeToStay` field
// (`alphaIncrement = -targetTotalDrift / (timeToStay / 25f)`), so the total drift across the
// *entire* real hold is always pinned to `targetTotalDrift` (1%) regardless of how long
// `timeToStay` actually is or how many ticks really fire -- removing the fixed-magnitude-vs-
// actual-duration mismatch entirely rather than re-guessing a smaller constant. Everything else
// (phase 1 unlock pulse, __keepAliveTick wrapper redirecting both updateLayeredBackground(bool)
// call sites) is identical to v3.
static int RunPatchNotificationKeepAliveV4(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive-v4 <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int unlockPumpSteps = 4;
    const int unlockPumpStepMs = 30;
    const float unlockAlpha = -0.05f;
    const int sustainIntervalMs = 25;
    const float targetTotalDrift = 0.01f; // total Opacity drop across the WHOLE real hold

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);
        var tickCountGetterRef = module.ImportReference(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);

        // --- Step 1: OnShown -- same anchor-finding as v2/v3.
        Instruction? insertAfter;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? intervalAnchor = null;
            int intervalIdx = -1;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                    intervalIdx = idx;
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            Instruction? startAnchor = null;
            for (int idx = intervalIdx + 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference smr && smr.Name == "Start" && smr.Parameters.Count == 0 &&
                    smr.DeclaringType.FullName == timerType!.FullName)
                {
                    startAnchor = instrs[idx];
                    break;
                }
            }
            if (startAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find the Visible case's `timer.Start();` after `timer.Interval = timeToStay;` in OnShown"); return 1; }

            foreach (var instr in instrs)
            {
                if (instr.Operand == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == startAnchor || handler.TryEnd == startAnchor ||
                    handler.HandlerStart == startAnchor || handler.HandlerEnd == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }

            insertAfter = startAnchor;
        }

        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }

            // Phase 1: identical to v3 -- state = Disappearing; alphaIncrement = unlockAlpha;
            // timer.Interval = 25; timer.Start(); pump `unlockPumpSteps` genuine ticks.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_3),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, unlockAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, sustainIntervalMs),
                Instruction.Create(OpCodes.Call, setIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Call, startRef)
            );
            for (int i = 0; i < unlockPumpSteps; i++)
            {
                Emit(
                    Instruction.Create(OpCodes.Call, doEventsRef),
                    Instruction.Create(OpCodes.Ldc_I4, unlockPumpStepMs),
                    Instruction.Create(OpCodes.Call, threadSleepRef)
                );
            }

            // Phase 2: state = Visible; alphaIncrement = -targetTotalDrift / (timeToStay / 25f)
            // -- computed at RUNTIME from the real timeToStay field, so the total drift across the
            // entire real hold is always pinned to targetTotalDrift regardless of how long
            // timeToStay actually is. Opacity = 1.0 (fresh baseline); updateLayeredBackground(false);
            // Invalidate(); record __keepAliveStartTick now.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_2),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, -targetTotalDrift),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timeToStayField),
                Instruction.Create(OpCodes.Conv_R4),
                Instruction.Create(OpCodes.Ldc_R4, (float)sustainIntervalMs),
                Instruction.Create(OpCodes.Div),
                Instruction.Create(OpCodes.Div),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, 1.0),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, tickCountGetterRef),
                Instruction.Create(OpCodes.Stfld, startTickField)
            );
        }

        var tickWrapper = new MethodDefinition("__keepAliveTick", MethodAttributes.Private,
            module.TypeSystem.Void);
        tickWrapper.Parameters.Add(new ParameterDefinition("refreshBitmap", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(tickWrapper);
        var twIl = tickWrapper.Body.GetILProcessor();
        var elseLabel = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, stateField));
        twIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        twIl.Append(Instruction.Create(OpCodes.Bne_Un, elseLabel));
        twIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        twIl.Append(Instruction.Create(OpCodes.Sub));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        twIl.Append(Instruction.Create(OpCodes.Blt, elseLabel));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        twIl.Append(Instruction.Create(OpCodes.Br, ret));
        twIl.Append(elseLabel); // Ldarg_0, reused as the else-branch's target push
        twIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        twIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        twIl.Append(ret);

        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int callCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "updateLayeredBackground")
                {
                    instr.Operand = tickWrapper;
                    callCount++;
                }
            }
            if (callCount != 2) { Console.Error.WriteLine($"FAIL: expected exactly 2 updateLayeredBackground(bool) calls in timer_OnTimer, found {callCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now pulses a brief real Disappearing-state fade ({unlockPumpSteps} pumped ticks) to unlock text, then switches to sustained Visible-state ticking with alphaIncrement computed at runtime as -{targetTotalDrift}/(timeToStay/{sustainIntervalMs}f) (pins total drift to {targetTotalDrift * 100}% regardless of actual timeToStay) for the rest of the hold; {targetType}::timer_OnTimer -- both updateLayeredBackground(bool) call sites redirected to __keepAliveTick(bool), which calls the real Hide() once timeToStay has genuinely elapsed since the unlock pulse, otherwise updates normally");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive-v5 <input-dir> <output-dir>
//
// v4 tested live: user reported text was "even less visible" than v3 -- surprising, since v4's
// computed drift (~0.0000406/tick for a ~6.16s hold) was smaller in magnitude than v3's fixed
// -0.0001/tick, and smaller drift was expected to look *better*, not worse. Sharper theory: Wine's
// actual SetLayeredWindowAttributes call quantizes Opacity to a 0-255 native alpha *byte*, not the
// raw float. A per-tick delta smaller than roughly 1/255 (~0.0039) may not shift that byte at all
// for many consecutive ticks -- meaning Wine could be issuing the *same* alpha value tick after
// tick despite the internal float genuinely changing, which would look exactly like the "static"
// signature this whole investigation has tied to blanking. v4's tiny delta was *below* that
// threshold far more often than v3's cruder one, plausibly explaining why it performed worse
// despite being "more correct" by the total-drift metric.
//
// v5 tests this directly: fixed step size well above the byte-quantization threshold (0.01, ~2.5
// byte-levels), but the step *alternates sign every tick* instead of decaying monotonically --
// guaranteeing a genuine native-alpha-byte change on literally every tick, with zero net drift
// (Opacity returns to exactly 1.0 every other tick, via timer_OnTimer's own existing first-branch
// clamp). The sign flip happens inside __keepAliveTick itself (extended from v3/v4): after
// deciding it's not yet time to call the real Hide(), it negates alphaIncrement before calling the
// real updateLayeredBackground(bool) -- so the NEXT tick's `Opacity += alphaIncrement` uses the
// opposite sign from this one. No visible fade at all, if the byte-quantization theory is right.
static int RunPatchNotificationKeepAliveV5(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive-v5 <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int unlockPumpSteps = 4;
    const int unlockPumpStepMs = 30;
    const float unlockAlpha = -0.05f;
    const int sustainIntervalMs = 25;
    const float oscillationStep = 0.01f; // ~2.5 native alpha byte-levels, well above the 1/255 quantization threshold

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);
        var tickCountGetterRef = module.ImportReference(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);

        // --- Step 1: OnShown -- same anchor-finding as v2/v3/v4.
        Instruction? insertAfter;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? intervalAnchor = null;
            int intervalIdx = -1;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                    intervalIdx = idx;
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            Instruction? startAnchor = null;
            for (int idx = intervalIdx + 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference smr && smr.Name == "Start" && smr.Parameters.Count == 0 &&
                    smr.DeclaringType.FullName == timerType!.FullName)
                {
                    startAnchor = instrs[idx];
                    break;
                }
            }
            if (startAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find the Visible case's `timer.Start();` after `timer.Interval = timeToStay;` in OnShown"); return 1; }

            foreach (var instr in instrs)
            {
                if (instr.Operand == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == startAnchor || handler.TryEnd == startAnchor ||
                    handler.HandlerStart == startAnchor || handler.HandlerEnd == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }

            insertAfter = startAnchor;
        }

        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }

            // Phase 1: identical to v2/v3/v4.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_3),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, unlockAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, sustainIntervalMs),
                Instruction.Create(OpCodes.Call, setIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Call, startRef)
            );
            for (int i = 0; i < unlockPumpSteps; i++)
            {
                Emit(
                    Instruction.Create(OpCodes.Call, doEventsRef),
                    Instruction.Create(OpCodes.Ldc_I4, unlockPumpStepMs),
                    Instruction.Create(OpCodes.Call, threadSleepRef)
                );
            }

            // Phase 2: state = Visible; alphaIncrement = -oscillationStep (fixed, byte-safe
            // magnitude; sign alternates every tick via __keepAliveTick below); Opacity = 1.0;
            // updateLayeredBackground(false); Invalidate(); record __keepAliveStartTick now.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_2),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, -oscillationStep),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, 1.0),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, tickCountGetterRef),
                Instruction.Create(OpCodes.Stfld, startTickField)
            );
        }

        // --- __keepAliveTick(bool refreshBitmap):
        //   if (state != Visible) { updateLayeredBackground(refreshBitmap); return; }  // real
        //                                     Hide()-driven fade in progress -- don't touch alpha
        //   if (elapsed >= timeToStay) { Hide(); return; }
        //   alphaIncrement = -alphaIncrement;
        //   updateLayeredBackground(refreshBitmap);
        //
        // The `state != Visible` guard is essential and was missing from the first version of
        // this patch (caught live, not by any static check): once the real Hide() runs, state
        // becomes Disappearing, but this wrapper's two call sites in timer_OnTimer keep firing on
        // every tick of the *real* fade-out too. Without the guard, the sign-flip logic ran
        // unconditionally regardless of state -- flipping alphaIncrement's sign on every real
        // fade-out tick as well, turning what should be a monotonic decrease to 0 into another
        // oscillation that never reaches <= 0, so setFormHidden() never ran. Live symptom: visible
        // flicker and the notification never closing. The guard restores the real fade-out's
        // original, unmodified behavior once Hide() has actually been called.
        var tickWrapper = new MethodDefinition("__keepAliveTick", MethodAttributes.Private,
            module.TypeSystem.Void);
        tickWrapper.Parameters.Add(new ParameterDefinition("refreshBitmap", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(tickWrapper);
        var twIl = tickWrapper.Body.GetILProcessor();
        var doUpdate = Instruction.Create(OpCodes.Ldarg_0);
        var doFlip = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, stateField));
        twIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        twIl.Append(Instruction.Create(OpCodes.Bne_Un, doUpdate)); // state != Visible -> skip straight to the plain update, no hide-check, no flip
        twIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        twIl.Append(Instruction.Create(OpCodes.Sub));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        twIl.Append(Instruction.Create(OpCodes.Blt, doFlip));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        twIl.Append(Instruction.Create(OpCodes.Br, ret));
        twIl.Append(doFlip); // Ldarg_0, reused as this block's target push
        // alphaIncrement = -alphaIncrement; (flips the sign for the NEXT tick's `Opacity +=
        // alphaIncrement`, which already ran with the OLD sign before this wrapper was called)
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, alphaIncrementField));
        twIl.Append(Instruction.Create(OpCodes.Neg));
        twIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        twIl.Append(doUpdate); // Ldarg_0, reused as this block's target push (also the not-Visible fallthrough target)
        twIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        twIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        twIl.Append(ret);

        // --- Unlike v3/v4 (whose persistently-negative alphaIncrement meant `Opacity +
        // alphaIncrement` never rose back to >= 1.0, so timer_OnTimer's *first* branch -- and the
        // ORIGINAL, still-unredirected `Hide()` call inside it -- was structurally unreachable
        // during the sustain phase), v5's oscillation revisits that branch every OTHER tick (any
        // tick where the sign happens to be positive). Left alone, that branch's own
        // `else if (state == Visible && ...) { Hide(); }` would call the real Hide() almost
        // immediately -- on the second sustain tick -- instead of waiting for the real timeout.
        // Redirect that call site too, to a second, parameterless wrapper with the same
        // elapsed-time gate as __keepAliveTick, so both paths into Hide() agree on timing.
        var maybeHideWrapper = new MethodDefinition("__keepAliveMaybeHide", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(maybeHideWrapper);
        var mhIl = maybeHideWrapper.Body.GetILProcessor();
        var mhRet = Instruction.Create(OpCodes.Ret);
        mhIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        mhIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        mhIl.Append(Instruction.Create(OpCodes.Sub));
        mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        mhIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        mhIl.Append(Instruction.Create(OpCodes.Blt, mhRet));
        mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        mhIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        mhIl.Append(mhRet);

        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int callCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "updateLayeredBackground")
                {
                    instr.Operand = tickWrapper;
                    callCount++;
                }
            }
            if (callCount != 2) { Console.Error.WriteLine($"FAIL: expected exactly 2 updateLayeredBackground(bool) calls in timer_OnTimer, found {callCount} -- method shape changed, review needed"); return 1; }

            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = maybeHideWrapper;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now pulses a brief real Disappearing-state fade ({unlockPumpSteps} pumped ticks) to unlock text, then switches to sustained Visible-state ticking with a fixed {oscillationStep} alphaIncrement whose sign flips every tick (real per-tick byte-level change, zero net drift) for the rest of the hold; {targetType}::timer_OnTimer -- both updateLayeredBackground(bool) call sites redirected to __keepAliveTick(bool) (flips alphaIncrement's sign and updates normally, or calls the real Hide() once timeToStay has elapsed) and the original Hide() call site redirected to __keepAliveMaybeHide() (same elapsed-time gate, since the oscillation can re-enter that branch too)");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-keep-alive-v6 <input-dir> <output-dir>
//
// v5 (after fixing its infinite-loop bug) tested live: no more stuck loop, but text was only
// very faintly visible during the oscillation -- not readable, "not satisfactory". Five rounds of
// tuning the oscillating/decaying-Opacity mechanism have each fixed a real bug or found a real
// partial effect, but none has landed on a clearly readable result. Per the user's explicit
// request, v6 steps back from tuning and runs the simplest possible direct test: a plain
// MONOTONIC fade (no oscillation, no dynamic total-drift calculation) at the exact magnitude
// round twelve directly confirmed worked (-0.01/tick) -- sustained for the whole hold instead of
// just a few ticks, with only a floor guard (stop decrementing once Opacity reaches 0.5, so it
// never goes negative or fully invisible) added for safety. This directly answers two questions
// at once: (1) does text stay readable while Opacity is genuinely, continuously counting down at
// a real, working rate (not a tiny or oscillating one)? (2) once it hits the floor and stops
// changing, does text revert to invisible again, the same way v1/v2's static alphaIncrement=0
// always did -- which would confirm continuous change, not merely having once been unlocked, is
// what text visibility actually depends on.
static int RunPatchNotificationKeepAliveV6(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-keep-alive-v6 <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int unlockPumpSteps = 4;
    const int unlockPumpStepMs = 30;
    const float unlockAlpha = -0.05f;
    const int sustainIntervalMs = 25;
    const float monotonicStep = -0.01f; // round twelve's directly-confirmed-working magnitude
    const double opacityFloor = 0.5;    // stop decrementing once Opacity reaches this, hold steady

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var timerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        var timeToStayField = type.Fields.FirstOrDefault(f => f.Name == "timeToStay");
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var alphaIncrementField = type.Fields.FirstOrDefault(f => f.Name == "alphaIncrement");
        if (timerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        if (timeToStayField is null) { Console.Error.WriteLine("FAIL: timeToStay field not found"); return 1; }
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        if (alphaIncrementField is null) { Console.Error.WriteLine("FAIL: alphaIncrement field not found"); return 1; }

        var timerType = timerField.FieldType.Resolve();
        var setIntervalDef = timerType?.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var startDef = timerType?.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (setIntervalDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.set_Interval from timer field's own FieldType"); return 1; }
        if (startDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer.Start from timer field's own FieldType"); return 1; }
        var setIntervalRef = module.ImportReference(setIntervalDef);
        var startRef = module.ImportReference(startDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var invalidateDef = controlType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing parameterless Invalidate()"); return 1; }
        var invalidateRef = module.ImportReference(invalidateDef);

        TypeDefinition? formType = type;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Form")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Form in base-type chain"); return 1; }
        var setOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Opacity");
        var getOpacityDef = formType.Methods.FirstOrDefault(m => m.Name == "get_Opacity");
        if (setOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing set_Opacity"); return 1; }
        if (getOpacityDef is null) { Console.Error.WriteLine("FAIL: Form missing get_Opacity"); return 1; }
        var setOpacityRef = module.ImportReference(setOpacityDef);
        var getOpacityRef = module.ImportReference(getOpacityDef);

        var applicationType = controlType.Module.GetType("System.Windows.Forms.Application");
        if (applicationType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Application"); return 1; }
        var doEventsDef = applicationType.Methods.FirstOrDefault(m => m.Name == "DoEvents" && m.Parameters.Count == 0);
        if (doEventsDef is null) { Console.Error.WriteLine("FAIL: Application missing DoEvents()"); return 1; }
        var doEventsRef = module.ImportReference(doEventsDef);
        var threadSleepRef = module.ImportReference(typeof(System.Threading.Thread).GetMethod("Sleep", new[] { typeof(int) })!);
        var tickCountGetterRef = module.ImportReference(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);

        // --- Step 1: OnShown -- same anchor-finding as v2-v5.
        Instruction? insertAfter;
        {
            var body = onShownMethod.Body;
            body.SimplifyMacros();
            var instrs = body.Instructions;

            Instruction? intervalAnchor = null;
            int intervalIdx = -1;
            for (int idx = 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference mr && mr.Name == "set_Interval" &&
                    instrs[idx - 1].OpCode == OpCodes.Ldfld && instrs[idx - 1].Operand is FieldReference ifr && ifr.Name == "timeToStay")
                {
                    if (intervalAnchor is not null) { Console.Error.WriteLine("FAIL: more than one `timer.Interval = timeToStay;` found in OnShown -- method shape changed, review needed"); return 1; }
                    intervalAnchor = instrs[idx];
                    intervalIdx = idx;
                }
            }
            if (intervalAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find `timer.Interval = timeToStay;` in OnShown"); return 1; }

            Instruction? startAnchor = null;
            for (int idx = intervalIdx + 1; idx < instrs.Count; idx++)
            {
                if ((instrs[idx].OpCode == OpCodes.Call || instrs[idx].OpCode == OpCodes.Callvirt) &&
                    instrs[idx].Operand is MethodReference smr && smr.Name == "Start" && smr.Parameters.Count == 0 &&
                    smr.DeclaringType.FullName == timerType!.FullName)
                {
                    startAnchor = instrs[idx];
                    break;
                }
            }
            if (startAnchor is null) { Console.Error.WriteLine("FAIL: couldn't find the Visible case's `timer.Start();` after `timer.Interval = timeToStay;` in OnShown"); return 1; }

            foreach (var instr in instrs)
            {
                if (instr.Operand == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is a branch target -- would need retargeting, review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == startAnchor || handler.TryEnd == startAnchor ||
                    handler.HandlerStart == startAnchor || handler.HandlerEnd == startAnchor)
                {
                    Console.Error.WriteLine("FAIL: insertion point is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }

            insertAfter = startAnchor;
        }

        var startTickField = new FieldDefinition("__keepAliveStartTick", FieldAttributes.Private, module.TypeSystem.Int32);
        type.Fields.Add(startTickField);

        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }

            // Phase 1: identical to v2-v5.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_3),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, unlockAlpha),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, sustainIntervalMs),
                Instruction.Create(OpCodes.Call, setIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Call, startRef)
            );
            for (int i = 0; i < unlockPumpSteps; i++)
            {
                Emit(
                    Instruction.Create(OpCodes.Call, doEventsRef),
                    Instruction.Create(OpCodes.Ldc_I4, unlockPumpStepMs),
                    Instruction.Create(OpCodes.Call, threadSleepRef)
                );
            }

            // Phase 2: state = Visible; alphaIncrement = monotonicStep (fixed, NOT oscillating --
            // this is a plain, continuous decay at the proven-working rate); Opacity = 1.0;
            // updateLayeredBackground(false); Invalidate(); record __keepAliveStartTick now.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_2),
                Instruction.Create(OpCodes.Stfld, stateField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R4, monotonicStep),
                Instruction.Create(OpCodes.Stfld, alphaIncrementField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_R8, 1.0),
                Instruction.Create(OpCodes.Call, setOpacityRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, invalidateRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, tickCountGetterRef),
                Instruction.Create(OpCodes.Stfld, startTickField)
            );
        }

        // --- __keepAliveTick(bool refreshBitmap):
        //   if (state != Visible) { updateLayeredBackground(refreshBitmap); return; }
        //   if (elapsed >= timeToStay) { Hide(); return; }
        //   if (Opacity <= opacityFloor) { alphaIncrement = 0f; }   // stop decaying, hold steady
        //   updateLayeredBackground(refreshBitmap);
        var tickWrapper = new MethodDefinition("__keepAliveTick", MethodAttributes.Private,
            module.TypeSystem.Void);
        tickWrapper.Parameters.Add(new ParameterDefinition("refreshBitmap", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(tickWrapper);
        var twIl = tickWrapper.Body.GetILProcessor();
        var doUpdate = Instruction.Create(OpCodes.Ldarg_0);
        var checkFloor = Instruction.Create(OpCodes.Ldarg_0);
        var ret = Instruction.Create(OpCodes.Ret);

        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, stateField));
        twIl.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        twIl.Append(Instruction.Create(OpCodes.Bne_Un, doUpdate));
        twIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
        twIl.Append(Instruction.Create(OpCodes.Sub));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
        twIl.Append(Instruction.Create(OpCodes.Blt, checkFloor));
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
        twIl.Append(Instruction.Create(OpCodes.Br, ret));
        twIl.Append(checkFloor); // Ldarg_0, reused as this block's target push
        twIl.Append(Instruction.Create(OpCodes.Callvirt, getOpacityRef));
        twIl.Append(Instruction.Create(OpCodes.Ldc_R8, opacityFloor));
        twIl.Append(Instruction.Create(OpCodes.Bgt, doUpdate)); // Opacity > floor -> nothing to do, skip straight to update
        twIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        twIl.Append(Instruction.Create(OpCodes.Ldc_R4, 0f));
        twIl.Append(Instruction.Create(OpCodes.Stfld, alphaIncrementField));
        twIl.Append(doUpdate); // Ldarg_0, reused as this block's target push (also the not-Visible / above-floor fallthrough target)
        twIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        twIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        twIl.Append(ret);

        {
            var body = timerMethod.Body;
            var instrs = body.Instructions;
            int callCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "updateLayeredBackground")
                {
                    instr.Operand = tickWrapper;
                    callCount++;
                }
            }
            if (callCount != 2) { Console.Error.WriteLine($"FAIL: expected exactly 2 updateLayeredBackground(bool) calls in timer_OnTimer, found {callCount} -- method shape changed, review needed"); return 1; }

            var maybeHideWrapper = new MethodDefinition("__keepAliveMaybeHide", MethodAttributes.Private, module.TypeSystem.Void);
            type.Methods.Add(maybeHideWrapper);
            var mhIl = maybeHideWrapper.Body.GetILProcessor();
            var mhRet = Instruction.Create(OpCodes.Ret);
            mhIl.Append(Instruction.Create(OpCodes.Call, tickCountGetterRef));
            mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            mhIl.Append(Instruction.Create(OpCodes.Ldfld, startTickField));
            mhIl.Append(Instruction.Create(OpCodes.Sub));
            mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            mhIl.Append(Instruction.Create(OpCodes.Ldfld, timeToStayField));
            mhIl.Append(Instruction.Create(OpCodes.Blt, mhRet));
            mhIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            mhIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(hideMethod)));
            mhIl.Append(mhRet);

            int hideCallCount = 0;
            foreach (var instr in instrs)
            {
                if ((instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) && instr.Operand is MethodReference mr && mr.Name == "Hide" && mr.Parameters.Count == 0)
                {
                    instr.Operand = maybeHideWrapper;
                    hideCallCount++;
                }
            }
            if (hideCallCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 Hide() call in timer_OnTimer, found {hideCallCount} -- method shape changed, review needed"); return 1; }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- Visible case now pulses a brief real Disappearing-state fade ({unlockPumpSteps} pumped ticks) to unlock text, then switches to sustained Visible-state ticking with a plain monotonic alphaIncrement={monotonicStep} (stops once Opacity reaches {opacityFloor}) for the rest of the hold; {targetType}::timer_OnTimer -- both updateLayeredBackground(bool) call sites redirected to __keepAliveTick(bool) and the original Hide() call site redirected to __keepAliveMaybeHide()");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-text-in-bitmap <input-dir> <output-dir>
//
// The "tune the Timer-driven sustain tick" family (v1-v6, six iterations -- see
// reports/notification-empty-until-fade-findings.md's thirteenth round) is a dead end: every
// version that got real per-tick alpha changes running did keep text visible for as long as those
// ticks were genuinely occurring, but none has managed to keep the timer itself reliably ticking
// for the *entire* ~6s hold without either freezing the UI (a full synchronous DoEvents() pump)
// or silently reverting to the original broken timeToStay-interval wait (v6, confirmed via a
// diag-log tick trace showing a 6.003s silent gap almost exactly equal to timeToStay, right after
// its brief unlock pump ended).
//
// This is a structurally different fix, prompted by re-reading LayeredBaseForm's own decompiled
// source (from a clean Stage 8 build): FormGenericNotification ("this") and its `layeredWindow`
// companion (a separate LayeredForm instance, field declared on LayeredBaseForm) are TWO SEPARATE
// WINDOWS. `layeredWindow` receives `backgroundBitmap` -- built by updateBackgroundBitmap(),
// confirmed by reading its full decompiled body to contain ONLY the background gradient, border,
// drop shadow, and avatar/image, never title or content text -- via
// `layeredWindow.UpdateWindow(backgroundBitmap, opacity, ...)`, a genuine per-pixel-alpha native
// blit forced fresh on every call. That's why the box/border/avatar are reliably visible from the
// very first frame in every recording this investigation has made. Title and content text, by
// contrast, are drawn live by OnPaintTitle()/OnPaintContent() from `this` form's own OnPaint,
// directly onto `this`'s own device context -- and `this` is ALSO independently a layered window
// (LayeredBaseForm's constructor sets `base.AllowTransparency = true` plus a TransparencyKey, and
// drives `this.Opacity`, which under Wine's winex11.drv goes through the exact
// SetLayeredWindowAttributes-only-updates-on-a-real-tick mechanism this whole investigation has
// been chasing). So `this`'s own client-area painting (the text) is subject to the bug;
// `layeredWindow`'s forced bitmap blit is not.
//
// Fix: draw title/content text directly into `backgroundBitmap` itself, inside
// updateBackgroundBitmap() (which already runs on every real content refresh and gets pushed to
// screen via the same reliable UpdateWindow() blit as the background/avatar), by calling the
// EXISTING OnPaintTitle(PaintEventArgs)/OnPaintContent(PaintEventArgs) virtual methods against a
// fresh Graphics created on backgroundBitmap -- reusing their real bounds/ellipsis/image-offset/
// color logic exactly (and honoring any subclass override -- FormIMMessageNotification overrides
// OnPaintContent -- via Callvirt) rather than re-deriving any of it by hand. Implemented as a new
// private helper, __drawNotificationTextIntoBitmap(), called once at the very end of
// updateBackgroundBitmap() (after all existing background/avatar/border drawing, right before its
// final `ret` -- the method's earlier `if (...) return;` guard for the no-bitmap-yet case is
// untouched, so the new call is skipped exactly when there's nothing to draw into, same as the
// rest of the method). Does not touch OnPaintTitle/OnPaintContent's own call site on `this`'s
// OnPaint at all -- if `this`'s own painting is still just as delayed, it now draws an invisible,
// harmless duplicate on top of the already-visible copy baked into backgroundBitmap; the new copy
// doesn't depend on `this`'s own painting working at all.
static int RunPatchNotificationTextInBitmap(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-text-in-bitmap <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var updateBackgroundBitmapMethod = type.Methods.FirstOrDefault(m => m.Name == "updateBackgroundBitmap" && m.HasBody);
        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        var onPaintContentMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintContent" && m.HasBody);
        if (updateBackgroundBitmapMethod is null) { Console.Error.WriteLine("FAIL: updateBackgroundBitmap not found"); return 1; }
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle(PaintEventArgs) not found"); return 1; }
        if (onPaintContentMethod is null) { Console.Error.WriteLine("FAIL: OnPaintContent(PaintEventArgs) not found"); return 1; }

        var layeredBaseFormType = type.BaseType?.Resolve();
        if (layeredBaseFormType is null || layeredBaseFormType.FullName != "MailClient.UI.Forms.LayeredBaseForm")
        {
            Console.Error.WriteLine($"FAIL: expected base type MailClient.UI.Forms.LayeredBaseForm, got {layeredBaseFormType?.FullName}");
            return 1;
        }
        var backgroundBitmapField = layeredBaseFormType.Fields.FirstOrDefault(f => f.Name == "backgroundBitmap");
        if (backgroundBitmapField is null) { Console.Error.WriteLine("FAIL: LayeredBaseForm missing backgroundBitmap field"); return 1; }
        var shadowVisibleGetterDef = layeredBaseFormType.Methods.FirstOrDefault(m => m.Name == "get_ShadowVisible");
        if (shadowVisibleGetterDef is null) { Console.Error.WriteLine("FAIL: LayeredBaseForm missing get_ShadowVisible"); return 1; }
        var shadowVisibleGetterRef = module.ImportReference(shadowVisibleGetterDef);

        // PaintEventArgs/Graphics/Rectangle -- resolve from OnPaintTitle's own parameter type and
        // its friends' own signatures, not typeof() reflection: System.Windows.Forms and
        // System.Drawing.Primitives are app-deployed assemblies the patching tool's own .NET 10
        // runtime doesn't share a version with (IL-patching lesson 5 in CLAUDE.md).
        var paintEventArgsTypeRef = onPaintTitleMethod.Parameters[0].ParameterType;
        var paintEventArgsTypeDef = paintEventArgsTypeRef.Resolve();
        if (paintEventArgsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve PaintEventArgs from OnPaintTitle's own parameter type"); return 1; }
        var graphicsPropGetterDef = paintEventArgsTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Graphics");
        if (graphicsPropGetterDef is null) { Console.Error.WriteLine("FAIL: PaintEventArgs missing get_Graphics"); return 1; }
        // graphicsPropGetterDef was resolved from a foreign (System.Windows.Forms) module's own
        // PaintEventArgs TypeDefinition, so its ReturnType is scoped to that foreign module --
        // must go through module.ImportReference before use as a bare type reference (e.g. for a
        // VariableDefinition) in *this* module, unlike a MethodReference/FieldReference used as a
        // whole (ImportReference on those recursively imports their signature types already).
        var graphicsTypeRef = module.ImportReference(graphicsPropGetterDef.ReturnType);
        var graphicsTypeDef = graphicsTypeRef.Resolve();
        if (graphicsTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics from PaintEventArgs.get_Graphics's own return type"); return 1; }

        var paintEventArgsCtorDef = paintEventArgsTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        if (paintEventArgsCtorDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve PaintEventArgs(Graphics, Rectangle) constructor"); return 1; }
        var paintEventArgsCtorRef = module.ImportReference(paintEventArgsCtorDef);
        var rectangleTypeRef = paintEventArgsCtorDef.Parameters[1].ParameterType;
        var rectangleTypeDef = rectangleTypeRef.Resolve();
        if (rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle from PaintEventArgs ctor's own parameter type"); return 1; }
        var rectangleCtorDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 4);
        if (rectangleCtorDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle(int,int,int,int) constructor"); return 1; }
        var rectangleCtorRef = module.ImportReference(rectangleCtorDef);

        var graphicsFromImageDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "FromImage" && m.Parameters.Count == 1);
        if (graphicsFromImageDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics.FromImage(Image)"); return 1; }
        var graphicsFromImageRef = module.ImportReference(graphicsFromImageDef);
        var graphicsTranslateTransformDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "TranslateTransform" && m.Parameters.Count == 2);
        if (graphicsTranslateTransformDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics.TranslateTransform(float,float)"); return 1; }
        var graphicsTranslateTransformRef = module.ImportReference(graphicsTranslateTransformDef);
        var graphicsDisposeDef = graphicsTypeDef.Methods.FirstOrDefault(m => m.Name == "Dispose" && m.Parameters.Count == 0);
        if (graphicsDisposeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Graphics.Dispose()"); return 1; }
        var graphicsDisposeRef = module.ImportReference(graphicsDisposeDef);

        // Bitmap (backgroundBitmapField's own FieldType) -- Width/Height are declared on Image,
        // Bitmap's base type; walk the chain rather than assume they're on Bitmap directly.
        var bitmapTypeDef = backgroundBitmapField.FieldType.Resolve();
        if (bitmapTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Bitmap from backgroundBitmap's own FieldType"); return 1; }
        TypeDefinition? imageLikeType = bitmapTypeDef;
        MethodDefinition? getWidthDef = null;
        MethodDefinition? getHeightDef = null;
        while (imageLikeType is not null && (getWidthDef is null || getHeightDef is null))
        {
            getWidthDef ??= imageLikeType.Methods.FirstOrDefault(m => m.Name == "get_Width");
            getHeightDef ??= imageLikeType.Methods.FirstOrDefault(m => m.Name == "get_Height");
            imageLikeType = imageLikeType.BaseType?.Resolve();
        }
        if (getWidthDef is null || getHeightDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Bitmap/Image get_Width/get_Height"); return 1; }
        var getWidthRef = module.ImportReference(getWidthDef);
        var getHeightRef = module.ImportReference(getHeightDef);
        var onPaintTitleRef = module.ImportReference(onPaintTitleMethod);
        var onPaintContentRef = module.ImportReference(onPaintContentMethod);

        // --- Build __drawNotificationTextIntoBitmap():
        //   if (backgroundBitmap == null) return;
        //   Graphics g = Graphics.FromImage(backgroundBitmap);
        //   if (ShadowVisible) g.TranslateTransform(9f, 9f);   // match updateBackgroundBitmap's own shadow offset
        //   PaintEventArgs pe = new PaintEventArgs(g, new Rectangle(0, 0, backgroundBitmap.Width, backgroundBitmap.Height));
        //   OnPaintTitle(pe);      // Callvirt -- FormIMMessageNotification overrides OnPaintContent, honor overrides
        //   OnPaintContent(pe);
        //   g.Dispose();
        var helper = new MethodDefinition("__drawNotificationTextIntoBitmap", MethodAttributes.Private, module.TypeSystem.Void);
        type.Methods.Add(helper);
        var hIl = helper.Body.GetILProcessor();
        var hRet = Instruction.Create(OpCodes.Ret);
        var gLocal = new VariableDefinition(graphicsTypeRef);
        var peLocal = new VariableDefinition(paintEventArgsTypeRef);
        helper.Body.Variables.Add(gLocal);
        helper.Body.Variables.Add(peLocal);

        var startOfG = Instruction.Create(OpCodes.Ldarg_0);
        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Ldfld, backgroundBitmapField));
        hIl.Append(Instruction.Create(OpCodes.Brtrue, startOfG));
        hIl.Append(Instruction.Create(OpCodes.Ret)); // early return, a separate Ret instruction from hRet below -- Cecil instructions can't be reused

        hIl.Append(startOfG); // Ldarg_0, reused as the branch target
        hIl.Append(Instruction.Create(OpCodes.Ldfld, backgroundBitmapField));
        hIl.Append(Instruction.Create(OpCodes.Call, graphicsFromImageRef));
        hIl.Append(Instruction.Create(OpCodes.Stloc, gLocal));

        var afterTransform = Instruction.Create(OpCodes.Ldloc, gLocal); // first instr of the continuation, and the branch target for the ShadowVisible==false case
        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, shadowVisibleGetterRef));
        hIl.Append(Instruction.Create(OpCodes.Brfalse, afterTransform));
        hIl.Append(Instruction.Create(OpCodes.Ldloc, gLocal));
        hIl.Append(Instruction.Create(OpCodes.Ldc_R4, 9f));
        hIl.Append(Instruction.Create(OpCodes.Ldc_R4, 9f));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, graphicsTranslateTransformRef));

        hIl.Append(afterTransform); // Ldloc gLocal, reused as the branch target -- pushes g for the PaintEventArgs ctor below
        hIl.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        hIl.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Ldfld, backgroundBitmapField));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, getWidthRef));
        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Ldfld, backgroundBitmapField));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, getHeightRef));
        hIl.Append(Instruction.Create(OpCodes.Newobj, rectangleCtorRef));
        hIl.Append(Instruction.Create(OpCodes.Newobj, paintEventArgsCtorRef));
        hIl.Append(Instruction.Create(OpCodes.Stloc, peLocal));

        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Ldloc, peLocal));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, onPaintTitleRef));
        hIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        hIl.Append(Instruction.Create(OpCodes.Ldloc, peLocal));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, onPaintContentRef));

        hIl.Append(Instruction.Create(OpCodes.Ldloc, gLocal));
        hIl.Append(Instruction.Create(OpCodes.Callvirt, graphicsDisposeRef));
        hIl.Append(hRet);

        // --- Insert a call to the new helper at the very end of updateBackgroundBitmap(), right
        // before its final `ret` (the method's own early `if (...) return;` guard is untouched --
        // that path never reaches our new call, same as it never reaches the rest of the method).
        //
        // The final `ret` here IS a branch target -- for entirely ordinary reasons. The whole
        // method body (after the early-return guard) is one big `using (Graphics graphics = ...)`,
        // itself containing two more nested `using (Pen ...)` blocks for the border strokes. Each
        // nested try's normal exit is a `leave` instruction that names the ULTIMATE target
        // (skipping past the textually-intervening finally blocks in the CIL stream -- the CLR
        // automatically cascades through every enclosing finally between a leave's origin and its
        // target), and the innermost try's leave happens to target this exact final `ret`
        // directly. Per IL-patching lesson 2, inserting new code immediately before a branch
        // target does NOT make that branch fall through the new code -- it would jump straight
        // over it to the original `ret`, skipping our new call entirely. And per lesson 3,
        // separately, the outermost `using`'s own `finally` (which disposes `graphics`) has its
        // `HandlerEnd` set to this same final `ret` instruction (the standard "one past the
        // handler's last instruction" exclusive marker) -- inserting new code physically before it
        // without reassigning `HandlerEnd` would silently grow that handler's own region to
        // swallow the new code, placing it *after* the handler's `endfinally` but still nominally
        // inside the region, where it would never actually execute (endfinally unconditionally
        // transfers control past the whole region, not into trailing code within it).
        //
        // Fix (both a consequence of the same insertion): retarget every instruction whose operand
        // is this exact `ret` (the `leave` described above) to point at our new first inserted
        // instruction instead, and retarget any exception-handler boundary field
        // (TryStart/TryEnd/HandlerStart/HandlerEnd) that equals it the same way. Both cases reduce
        // to "anything that currently points at the old final instruction should point at the new
        // first instruction instead" -- there's exactly one such branch and one such handler
        // boundary here, but the loop below handles however many there turn out to be.
        {
            var body = updateBackgroundBitmapMethod.Body;
            body.SimplifyMacros();
            var il = body.GetILProcessor();
            var instrs = body.Instructions;
            var lastInstr = instrs[instrs.Count - 1];
            if (lastInstr.OpCode != OpCodes.Ret) { Console.Error.WriteLine($"FAIL: updateBackgroundBitmap's last instruction isn't Ret (got {lastInstr.OpCode}) -- method shape changed, review needed"); return 1; }

            var newFirst = Instruction.Create(OpCodes.Ldarg_0);
            il.InsertBefore(lastInstr, newFirst);
            il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, helper));

            int retargetedBranches = 0;
            foreach (var instr in instrs)
            {
                if (instr != newFirst && instr.Operand == lastInstr)
                {
                    instr.Operand = newFirst;
                    retargetedBranches++;
                }
            }
            int retargetedHandlerBounds = 0;
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == lastInstr) { handler.TryStart = newFirst; retargetedHandlerBounds++; }
                if (handler.TryEnd == lastInstr) { handler.TryEnd = newFirst; retargetedHandlerBounds++; }
                if (handler.HandlerStart == lastInstr) { handler.HandlerStart = newFirst; retargetedHandlerBounds++; }
                if (handler.HandlerEnd == lastInstr) { handler.HandlerEnd = newFirst; retargetedHandlerBounds++; }
            }
            Console.WriteLine($"     ({retargetedBranches} branch operand(s), {retargetedHandlerBounds} handler boundary field(s) retargeted from the old final ret to the new pre-ret call)");
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::updateBackgroundBitmap -- now also draws title/content text (via the existing OnPaintTitle/OnPaintContent virtual methods, honoring subclass overrides) directly into backgroundBitmap before it's pushed to screen via the layeredWindow companion's own reliable UpdateWindow() blit, instead of relying solely on `this` form's own (bug-affected) live OnPaint");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-suppress-self-paint <input-dir> <output-dir>
//
// Immediately follows --patch-notification-text-in-bitmap and was needed because that fix alone,
// live-tested via the fully automated launch+recording loop, made NO visible difference: text was
// baked correctly into backgroundBitmap (confirmed via decompile), yet still appeared only right
// after OnShown and right at Hide(), blank for the whole hold in between -- identical to baseline.
//
// Root cause of *that*, found by grepping the raw IL for actual OnPaintTitle/OnPaintContent call
// sites (there are none inside FormGenericNotification's own class body -- they're only ever
// invoked from ITS OWN `OnPaintBackground` override, previously misread as design-mode-only
// because a *different*, base-class OnPaintBackground on LayeredBaseForm genuinely is
// design-mode-only and was the one actually decompiled and read earlier). FormGenericNotification
// overrides OnPaintBackground itself and, at real runtime (not design mode), draws its OWN
// redundant copy of backgroundBitmap directly onto `this` form's own device context via
// `e.Graphics.DrawImage(backgroundBitmap, ...)`, immediately followed by `OnPaintTitle(e)` and
// `OnPaintContent(e)` painting text onto that same DC. This means the background/avatar/border are
// actually painted TWICE per frame -- once via `layeredWindow`'s reliable, forced UpdateWindow()
// blit (a separate window sitting behind `this`), and again via `this`'s own OnPaintBackground,
// which (per this whole investigation's central finding) is subject to the
// SetLayeredWindowAttributes-only-updates-on-a-real-tick compositing bug. The working theory this
// now points to: `this`'s own copy gets "stuck" showing whatever it last managed to actually
// composite -- most likely a partially-flushed frame from partway through its own OnPaintBackground
// (background drawn, text not yet drawn, if the underlying WM_PAINT gets torn/frozen mid-sequence)
// -- sitting OPAQUE on top of `layeredWindow`'s already-correct (now text-included, thanks to the
// prior patch) content underneath, blocking it from view for the whole hold, until ticks resume
// near Hide() and `this` finally gets a fresh, complete paint through.
//
// Fix: since `layeredWindow` alone (with the prior patch) already reliably provides background,
// avatar, border, AND text, `this` form's own duplicate painting in OnPaintBackground is now pure
// redundant risk -- make it a no-op for the real-runtime (non-DesignMode) case, leaving `this` as
// a fully transparent (TransparencyKey-keyed) pass-through window over `layeredWindow`'s own
// reliable content, so there's nothing of `this`'s own left to get stuck mid-composite. Implemented
// as the smallest possible change: OnPaintBackground already branches on
// `if (backgroundBitmap != null) { <the whole redundant draw+text block> }` via a single
// `brfalse(.s) <after-the-block>` instruction right after loading the field; flipping that one
// instruction's opcode to the unconditional `br(.s)` (same operand/target, so no branch-target or
// exception-handler-boundary retargeting is needed at all -- lessons 2 and 3 don't apply here)
// makes the whole block unreachable at runtime while leaving every earlier guard (disposed check,
// base.OnPaintBackground() call, DesignMode early-return, the updateBitmapPending ->
// updateBackgroundBitmap() trigger some other code path may still rely on) completely untouched.
static int RunPatchNotificationSuppressSelfPaint(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-suppress-self-paint <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onPaintBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintBackground" && m.HasBody);
        if (onPaintBackgroundMethod is null) { Console.Error.WriteLine("FAIL: OnPaintBackground(PaintEventArgs) not found"); return 1; }

        var layeredBaseFormType = type.BaseType?.Resolve();
        if (layeredBaseFormType is null || layeredBaseFormType.FullName != "MailClient.UI.Forms.LayeredBaseForm")
        {
            Console.Error.WriteLine($"FAIL: expected base type MailClient.UI.Forms.LayeredBaseForm, got {layeredBaseFormType?.FullName}");
            return 1;
        }
        var backgroundBitmapField = layeredBaseFormType.Fields.FirstOrDefault(f => f.Name == "backgroundBitmap");
        if (backgroundBitmapField is null) { Console.Error.WriteLine("FAIL: LayeredBaseForm missing backgroundBitmap field"); return 1; }

        var instrs = onPaintBackgroundMethod.Body.Instructions;
        int matchCount = 0;
        int matchIdx = -1;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].OpCode == OpCodes.Ldfld && instrs[i].Operand is FieldReference fr && fr.Name == "backgroundBitmap" &&
                (instrs[i + 1].OpCode == OpCodes.Brfalse || instrs[i + 1].OpCode == OpCodes.Brfalse_S))
            {
                matchCount++;
                matchIdx = i + 1;
            }
        }
        if (matchCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 `if (backgroundBitmap != null)` branch in OnPaintBackground, found {matchCount} -- method shape changed, review needed"); return 1; }

        var branchInstr = instrs[matchIdx];
        branchInstr.OpCode = branchInstr.OpCode == OpCodes.Brfalse_S ? OpCodes.Br_S : OpCodes.Br;
        // Operand (the branch target) is untouched -- same instruction, now reached unconditionally.

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintBackground -- the `if (backgroundBitmap != null) {{ draw background copy + OnPaintTitle + OnPaintContent onto `this`'s own DC }}` block is now unconditionally skipped at real runtime (DesignMode early-return above it is untouched), since layeredWindow's own blit (see --patch-notification-text-in-bitmap) already reliably provides all of it");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-refresh-on-content-change <input-dir> <output-dir>
//
// A structurally different fix from every prior attempt in this investigation (six-iteration
// Timer-sustain family, then the two-part "separate rendering path" pivot, both dead ends -- see
// reports/notification-empty-until-fade-findings.md's thirteenth/fourteenth rounds) -- this one is
// grounded in an actual discovered ordering bug, found by a full systematic decompile pass of the
// whole notification pipeline (not another Wine-compositor-timing guess).
//
// The real trigger path (`FormNotificationPresenter.ShowNotification`) calls
// `formGenericNotification.Show()` FIRST (while the form is still blank -- this is what starts the
// Hidden->Appearing fade timer and builds the very first backgroundBitmap, with empty title/
// content), and only THEN calls `formGenericNotification.ShowNotification(notification)`, which is
// what actually sets Title/Content/Image (via virtual dispatch into
// `FormMailNotification.OnDisplayedNotificationChanged`, called from
// `FormGenericNotification.ShowNotification` itself). The `--patch-auto-test-notification` trigger
// did the opposite (set Title/Content, then Show()) -- a real, consequential difference, not just
// a cosmetic one: it means every earlier live test reproduced the bug, but via a different
// mechanism shape than a real notification actually takes.
//
// The actual gap: neither the `Title` setter (`Text = (title = value);`, no repaint side effect)
// nor the `Content` setter (`if (content != value) { content = value; if (autoHeight)
// setAutoHeight(); }`, and `FormMailNotification`'s constructor sets `AutoHeight = false`, making
// this a no-op too) ever triggers a fresh rebuild-and-blit through the reliable `layeredWindow`
// path. The `Image` setter is the only one with any effect at all (`updateBitmapPending = true;`),
// and that flag is only ever read inside `this` form's OWN `OnPaintBackground` -- the exact
// mechanism this whole investigation has shown to be unreliable under Wine without a genuine,
// already-ticking alpha change. So on the real path, nothing forces `layeredWindow` -- confirmed
// via full decompile to be a genuine, always-forced native `UpdateLayeredWindow` P/Invoke call,
// the reliable half of this whole story -- to ever receive a bitmap rebuilt with the real title/
// content. The only way content has ever reached the screen at all is `this` form's own paint
// cycle happening to fire on its own, which is exactly the unreliable mechanism previously chased.
//
// Fix: `FormGenericNotification.ShowNotification(Notification)` already calls
// `OnDisplayedNotificationChanged(EventArgs.Empty)` (virtual -- this is what sets Title/Content/
// Image on the real `FormMailNotification` subclass) before `Reshow()`/`PerformLayout()`/
// `Invalidate()`. Insert `updateLayeredBackground(refreshBitmap: true)` immediately after that
// call returns -- forcing an immediate, genuine rebuild-and-blit through the already-reliable
// `layeredWindow` path with the now-correct content, independent of whether `this` form's own
// paint pipeline ever manages to fire. `ShowNotification` has no branches or exception handlers of
// its own (straight-line code), so this is a plain "capture anchor once, insert after it, verify
// it isn't itself a branch target" insertion -- no lesson-3 handler-boundary concerns here.
static int RunPatchNotificationRefreshOnContentChange(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-refresh-on-content-change <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var showNotificationMethod = type.Methods.FirstOrDefault(m => m.Name == "ShowNotification" && m.HasBody && m.Parameters.Count == 1);
        if (showNotificationMethod is null) { Console.Error.WriteLine("FAIL: ShowNotification(Notification) not found"); return 1; }

        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground(bool) not found on type or base type"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var body = showNotificationMethod.Body;
        body.SimplifyMacros();
        var il = body.GetILProcessor();
        var instrs = body.Instructions;

        // Anchor on PerformLayout(), not OnDisplayedNotificationChanged() -- found the hard way via
        // a real-Windows-screenshot comparison (supporting/notifications-working-example-from-
        // windows-with-long-subject.png vs a live capture): content text rendered much closer to
        // the box's left edge than the reference. FormGenericNotification.OnLayout calls
        // doLayout(), which is what actually (re)computes headerRect/contentRect/imageRect for the
        // CURRENT title/content/image -- and PerformLayout() is what triggers OnLayout(). The
        // original anchor (right after OnDisplayedNotificationChanged()) ran BEFORE
        // ShowNotification's own later PerformLayout() call, so the very first rebuilt bitmap used
        // stale layout metrics from before the new avatar/content was accounted for. Anchoring
        // after PerformLayout() instead means doLayout() has already run with the current content
        // by the time the bitmap gets rebuilt.
        Instruction? anchor = null;
        int matchCount = 0;
        for (int i = 0; i < instrs.Count; i++)
        {
            if ((instrs[i].OpCode == OpCodes.Call || instrs[i].OpCode == OpCodes.Callvirt) &&
                instrs[i].Operand is MethodReference mr && mr.Name == "PerformLayout")
            {
                anchor = instrs[i];
                matchCount++;
            }
        }
        if (matchCount != 1) { Console.Error.WriteLine($"FAIL: expected exactly 1 PerformLayout() call in ShowNotification, found {matchCount} -- method shape changed, review needed"); return 1; }

        foreach (var instr in instrs)
        {
            if (instr.Operand == anchor)
            {
                Console.Error.WriteLine("FAIL: insertion anchor is itself a branch target -- would need retargeting, review needed");
                return 1;
            }
        }
        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == anchor || handler.TryEnd == anchor || handler.HandlerStart == anchor || handler.HandlerEnd == anchor)
            {
                Console.Error.WriteLine("FAIL: insertion anchor is an exception-handler region boundary -- review needed");
                return 1;
            }
        }

        var insertAfter = anchor!;
        void Emit(params Instruction[] toEmit) { foreach (var i in toEmit) { il.InsertAfter(insertAfter, i); insertAfter = i; } }
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef)
        );

        Console.WriteLine($"OK   {fileName}: {targetType}::ShowNotification -- now calls updateLayeredBackground(refreshBitmap: true) immediately after PerformLayout() (so doLayout() has already recomputed headerRect/contentRect/imageRect for the current content), forcing a fresh rebuild-and-blit through the already-reliable layeredWindow path instead of relying on `this` form's own paint cycle to ever pick up the new content");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-periodic-reblit <input-dir> <output-dir>
//
// Directly tests the narrowest remaining question from the fifteenth round (see
// reports/notification-empty-until-fade-findings.md): with --patch-notification-refresh-on-content-change
// and --patch-notification-text-in-bitmap both applied (content correct in backgroundBitmap from
// before the Appearing tick burst even starts, confirmed visible on screen throughout that burst),
// text still goes blank the moment ticking stops for the ~6s hold, while the icon -- baked into the
// exact same static Bitmap object -- keeps showing fine. Every value (Opacity, the bitmap's own
// pixel data) is confirmed unchanged and correct at that point; only whether the compositor
// re-asserts it is in question. This is a clean, narrow test of that: does merely RE-CALLING the
// already-proven-reliable layeredWindow.UpdateWindow() blit periodically during the hold -- with
// the *identical* bitmap and *identical* opacity, no value changing at all -- keep text visible, or
// does it not matter?
//
// Structurally much simpler and safer than the v1-v6 keep-alive family: those all mutated
// alphaIncrement/Opacity/state to force genuine per-tick value changes, which repeatedly collided
// with timer_OnTimer's own state machine (the Hide()-reachability bug, the sign-flip-during-
// fade-out bug, the interval-reversion-after-unlock-pump bug). This patch touches NONE of that --
// it adds a completely separate, independent Timer that does nothing but call the existing
// updateLayeredBackground(refreshBitmap: false) once every 300ms while state == Visible, changing
// nothing else. If it works, it also has a real shot at being closer to a final fix than any
// prior attempt; if it doesn't, that's strong, clean evidence the gate is something other than
// "the compositor needs periodic re-assertion" (e.g. a per-pixel alpha problem specific to how
// GDI's TextRenderer/ExtTextOut writes into a 32bppArgb bitmap destined for UpdateLayeredWindow's
// per-pixel blend, which would need a completely different kind of fix).
//
// Implementation: add `private Timer __periodicReblitTimer;` and
// `private void __periodicReblitTick(object, EventArgs)` to FormGenericNotification, and insert a
// null-guarded one-time creation/start of that timer at the very top of OnShown (same safe
// insertion point --patch-diag/--patch-auto-test-notification already use throughout this
// investigation -- a method's own first instruction is never a branch target, and there's no
// exception-handler region there to accidentally grow). Guarding on the field being null makes
// this correctly "once per form instance" even though OnShown fires once per notification shown
// and the form instance is reused across notifications (confirmed by the full decompile pass --
// MailNotificationHandler.ChooseForm() only constructs a new form if none exists yet).
static int RunPatchNotificationPeriodicReblit(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-periodic-reblit <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
    const int reblitIntervalMs = 300;

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        if (stateField is null) { Console.Error.WriteLine("FAIL: state field not found"); return 1; }
        var updateLayeredBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            type.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground(bool) not found on type or base type"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        // Timer -- resolve from the existing `timer` field's own FieldType, not typeof()
        // reflection (System.Windows.Forms is app-deployed -- IL-patching lesson 5).
        var existingTimerField = type.Fields.FirstOrDefault(f => f.Name == "timer");
        if (existingTimerField is null) { Console.Error.WriteLine("FAIL: timer field not found"); return 1; }
        var timerTypeDef = existingTimerField.FieldType.Resolve();
        if (timerTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Timer from the existing timer field's own FieldType"); return 1; }
        var timerCtorDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        var timerSetIntervalDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var timerAddTickDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "add_Tick");
        var timerStartDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        if (timerCtorDef is null || timerSetIntervalDef is null || timerAddTickDef is null || timerStartDef is null)
        {
            Console.Error.WriteLine("FAIL: Timer missing one of .ctor()/set_Interval/add_Tick/Start()");
            return 1;
        }
        var timerCtorRef = module.ImportReference(timerCtorDef);
        var timerSetIntervalRef = module.ImportReference(timerSetIntervalDef);
        var timerAddTickRef = module.ImportReference(timerAddTickDef);
        var timerStartRef = module.ImportReference(timerStartDef);
        var timerFieldTypeRef = module.ImportReference(existingTimerField.FieldType);

        var eventHandlerCtorRef = module.ImportReference(typeof(EventHandler).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);

        // --- New field + tick method.
        var reblitTimerField = new FieldDefinition("__periodicReblitTimer", FieldAttributes.Private, timerFieldTypeRef);
        type.Fields.Add(reblitTimerField);

        var tickMethod = new MethodDefinition("__periodicReblitTick", MethodAttributes.Private, module.TypeSystem.Void);
        tickMethod.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
        tickMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, module.ImportReference(typeof(EventArgs))));
        type.Methods.Add(tickMethod);
        var tmIl = tickMethod.Body.GetILProcessor();
        var tmRet = Instruction.Create(OpCodes.Ret);
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldfld, stateField));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4_2)); // NotificationFormState.Visible == 2
        tmIl.Append(Instruction.Create(OpCodes.Bne_Un, tmRet));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        tmIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        tmIl.Append(tmRet);

        // --- Prepend to OnShown: if (__periodicReblitTimer == null) { create, configure, start }.
        // Guard means this runs exactly once per form instance, regardless of how many times
        // OnShown fires afterward (the form is reused across notifications).
        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var originalFirst = body.Instructions[0];

            var createBlock = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Newobj, timerCtorRef),
                Instruction.Create(OpCodes.Stfld, reblitTimerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, reblitTimerField),
                Instruction.Create(OpCodes.Ldc_I4, reblitIntervalMs),
                Instruction.Create(OpCodes.Callvirt, timerSetIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, reblitTimerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, tickMethod),
                Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, timerAddTickRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, reblitTimerField),
                Instruction.Create(OpCodes.Callvirt, timerStartRef)
            };

            var guardCheck = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, reblitTimerField),
                Instruction.Create(OpCodes.Brtrue, originalFirst)
            };
            foreach (var i in guardCheck) { il.InsertBefore(originalFirst, i); }
            foreach (var i in createBlock) { il.InsertBefore(originalFirst, i); }
        }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnShown -- now also starts a separate, independent {reblitIntervalMs}ms __periodicReblitTimer (once per form instance) that calls updateLayeredBackground(refreshBitmap: false) while state == Visible -- no Opacity/state change, purely a repeated re-assertion of the already-correct bitmap, to test whether the compositor needs periodic re-blitting independent of any value actually changing");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-suppress-self-text-only <input-dir> <output-dir>
//
// Sixteenth round's fix (text-in-bitmap + refresh-on-content-change + periodic-reblit) works --
// text is now visible for the entire notification lifecycle, confirmed both by the user directly
// ("I saw text the whole time... YAY") and against a genuine real email. But the user also caught
// a real, if minor, cosmetic side effect: a single-frame "ghost" of the text -- faint, slightly
// offset -- flashes behind the correct text right at the fade-out transition ("the formatting was
// wrong... I think I briefly saw the original text in the background"). Confirmed via frame
// extraction: one frame at the Hide()-triggered fade-out shows the crisp, correctly-positioned
// text from layeredWindow's blit *and* a fainter, ~9px-offset duplicate behind it.
//
// Root cause: `this` form's own OnPaintBackground -- never touched by this fix, deliberately, since
// --patch-notification-suppress-self-paint (which disables it entirely) is a confirmed dead end,
// see below -- still independently draws a redundant copy of the background PLUS title/content
// text directly onto `this`'s own device context. `this`'s own paint is only reliable while real
// ticks are actively running (the same Wine bug this whole investigation has chased), which is
// exactly when it transiently *does* succeed: right during the Disappearing fade-out tick burst.
// Its text lands ~9px offset from layeredWindow's copy because of the ShadowVisible
// TranslateTransform(9,9) applied when building backgroundBitmap (padded for the drop-shadow) --
// `this`'s own paint doesn't apply that same offset, since it isn't drawing into the padded bitmap.
//
// --patch-notification-suppress-self-paint was retried in combination with periodic-reblit on the
// theory that periodic-reblit might avoid the earlier regression (a "broken image" red-X glyph) by
// keeping layeredWindow's surface constantly refreshed regardless of `this`'s own paint state --
// it did not: the same red-X regression reappeared, now spanning a much larger area. Disabling
// `this`'s paint entirely is confirmed, again, to not produce a clean transparent pass-through.
//
// This is a narrower, safer alternative: leave `this`'s own background-copy draw (the
// `e.Graphics.DrawImage(backgroundBitmap, ...)` call) fully intact -- so `this`'s own surface is
// still genuinely painted with *something* every time, avoiding whatever unrealized-surface state
// caused the red-X glyph -- and remove ONLY the two `OnPaintTitle(e)`/`OnPaintContent(e)` call
// sites immediately after it. `this` keeps its own (harmless, redundant, already-invisible-under-
// Wine-without-real-ticks) background copy, but never draws the duplicate text that causes the
// ghost, since only layeredWindow's copy (fixed by text-in-bitmap) ever has text at all now.
//
// Implementation: this REMOVES instructions from an existing method body -- a new operation for
// this file (every prior patch only inserted or flipped an opcode). Straight-line code within the
// `try` block (no other branch enters or exits mid-sequence), so removal is safe provided none of
// the six target instructions (Ldarg_0/Ldarg_1 pairs plus the two Callvirt calls) is itself a
// branch target or an exception-handler region boundary -- checked explicitly before removing
// anything, same defensive pattern as every insertion elsewhere in this file.
static int RunPatchNotificationSuppressSelfTextOnly(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-suppress-self-text-only <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var onPaintBackgroundMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintBackground" && m.HasBody);
        if (onPaintBackgroundMethod is null) { Console.Error.WriteLine("FAIL: OnPaintBackground(PaintEventArgs) not found"); return 1; }

        var body = onPaintBackgroundMethod.Body;
        var instrs = body.Instructions;

        int titleCallIdx = -1, contentCallIdx = -1;
        for (int i = 0; i < instrs.Count; i++)
        {
            if ((instrs[i].OpCode == OpCodes.Call || instrs[i].OpCode == OpCodes.Callvirt) && instrs[i].Operand is MethodReference mr)
            {
                if (mr.Name == "OnPaintTitle") { if (titleCallIdx != -1) { Console.Error.WriteLine("FAIL: more than one OnPaintTitle call in OnPaintBackground"); return 1; } titleCallIdx = i; }
                if (mr.Name == "OnPaintContent") { if (contentCallIdx != -1) { Console.Error.WriteLine("FAIL: more than one OnPaintContent call in OnPaintBackground"); return 1; } contentCallIdx = i; }
            }
        }
        if (titleCallIdx == -1 || contentCallIdx == -1) { Console.Error.WriteLine($"FAIL: OnPaintTitle/OnPaintContent call(s) not found in OnPaintBackground (title={titleCallIdx}, content={contentCallIdx})"); return 1; }
        if (contentCallIdx != titleCallIdx + 3) { Console.Error.WriteLine($"FAIL: expected OnPaintContent's call exactly 3 instructions after OnPaintTitle's (Ldarg_0;Ldarg_1;Callvirt each) -- method shape changed, review needed (title={titleCallIdx}, content={contentCallIdx})"); return 1; }
        if (instrs[titleCallIdx - 2].OpCode != OpCodes.Ldarg_0 || instrs[titleCallIdx - 1].OpCode != OpCodes.Ldarg_1 ||
            instrs[contentCallIdx - 2].OpCode != OpCodes.Ldarg_0 || instrs[contentCallIdx - 1].OpCode != OpCodes.Ldarg_1)
        {
            Console.Error.WriteLine("FAIL: expected Ldarg_0;Ldarg_1 immediately before each of OnPaintTitle/OnPaintContent -- method shape changed, review needed");
            return 1;
        }

        var toRemove = new[]
        {
            instrs[titleCallIdx - 2], instrs[titleCallIdx - 1], instrs[titleCallIdx],
            instrs[contentCallIdx - 2], instrs[contentCallIdx - 1], instrs[contentCallIdx]
        };

        foreach (var target in toRemove)
        {
            foreach (var instr in instrs)
            {
                if (instr.Operand == target)
                {
                    Console.Error.WriteLine("FAIL: one of the instructions to remove is itself a branch target -- review needed");
                    return 1;
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == target || handler.TryEnd == target || handler.HandlerStart == target || handler.HandlerEnd == target)
                {
                    Console.Error.WriteLine("FAIL: one of the instructions to remove is an exception-handler region boundary -- review needed");
                    return 1;
                }
            }
        }

        var il = body.GetILProcessor();
        foreach (var target in toRemove) { il.Remove(target); }

        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintBackground -- removed the OnPaintTitle(e)/OnPaintContent(e) calls (and their argument-loading instructions) that drew a redundant, ghost-prone duplicate of the text directly onto `this` form's own DC; the background/avatar copy draw immediately before them is untouched, so `this`'s own surface is still genuinely painted with something every frame");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-settings-refresh <input-dir> <output-dir>
//
// Root cause (found through several rounds of --patch-diag instrumentation, all superseded by
// this point): the Load event is simply never raised for formSettings under Wine (`base.Load +=
// new EventHandler(formSettings_Load);` is correctly wired near the end of a clean
// InitializeComponent(), but the handler's first instruction never runs) -- eM Client evidently
// shows Settings through a path that doesn't trigger WinForms' normal Show()/OnLoad() sequence.
// dataGridCategory is the one thing formSettings_Load exclusively configures (columns, category
// data, focus/selection), so without it the grid stays permanently empty; other settings pages
// render fine because they populate independently.
//
// Fix: MailClient.dll, UI.Forms.formSettings(string tabName)'s public constructor -- insert
// `try { formSettings_Load(this, EventArgs.Empty); } catch (Exception ex) { log }` right after
// the constructor's own `SwitchToTabPanel(...)` call. Must be there and not in the private
// parameterless constructor (tried first): formSettings_Load's tail end does
// `dataGridCategory.SelectedRows.Add(dataGridCategory.FocusedRow)`, which needs
// controlPanelSwitcher.CurrentPanel to already be set -- true only after SwitchToTabPanel has
// run, which is the public constructor's job, called after the private one via `: this()`.
// Confirmed working via --patch-diag: loadCategories() now runs end-to-end, SetDataSource fires
// for dataGridCategory with real data (12 groups, 47 items), and it now paints with real content
// instead of columns.Count==0. The try/catch is a permanent safety net, not just a diagnostic:
// formSettings_Load also calls `this.BeginInvoke(...)` in some builds of this codebase, which
// throws InvalidOperationException this early (window handle doesn't exist yet during
// construction) -- caught and logged rather than crashing the app.
static int RunPatchSettingsRefresh(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-settings-refresh <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.formSettings";
    const string targetMethod = "formSettings_Load";
    const string logPath = @"Z:\tmp\claude-diag.log";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var appendAllText = module.ImportReference(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);
        var stringConcat2 = module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);

        var ctorMethod = type.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 1 &&
            m.Parameters[0].ParameterType.FullName == "System.String");
        if (ctorMethod is null) { Console.Error.WriteLine($"FAIL: couldn't find {targetType}'s formSettings(string) constructor"); return 1; }
        var loadHandler = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (loadHandler is null) { Console.Error.WriteLine($"FAIL: couldn't re-find {targetType}::{targetMethod} for the constructor call"); return 1; }
        var loadHandlerRef = module.ImportReference(loadHandler);
        var eventArgsEmpty = module.ImportReference(typeof(EventArgs).GetField("Empty")!);
        var exceptionToString = module.ImportReference(typeof(Exception).GetMethod("ToString", Type.EmptyTypes)!);
        var exceptionTypeRef = module.ImportReference(typeof(Exception));

        var ctorBody = ctorMethod.Body;
        var ctorIl = ctorBody.Instructions;
        Instruction? switchToTabPanelCall = null;
        for (int k = 0; k < ctorIl.Count; k++)
        {
            if ((ctorIl[k].OpCode == OpCodes.Callvirt || ctorIl[k].OpCode == OpCodes.Call) &&
                ctorIl[k].Operand is MethodReference mr3 && mr3.Name == "SwitchToTabPanel" &&
                mr3.Parameters.Count == 1 && mr3.Parameters[0].ParameterType.FullName == "System.String")
            {
                switchToTabPanelCall = ctorIl[k];
                break;
            }
        }
        if (switchToTabPanelCall is null)
        {
            Console.Error.WriteLine($"FAIL: couldn't find `SwitchToTabPanel(string)` call in {targetType}'s formSettings(string) constructor -- refusing to patch");
            return 1;
        }
        // insert right after the call; the ctor is just `ldarg.0; call .ctor(); ...; ldarg.0;
        // ldstr "tab"; ldarg.1; call Concat; call SwitchToTabPanel(string); ret` -- nothing
        // branches into the space between this call and the final ret, so no retargeting needed.
        var ctorProc = ctorBody.GetILProcessor();
        var afterCall = switchToTabPanelCall.Next!; // the original `ret`
        var excLocal = new VariableDefinition(exceptionTypeRef);
        ctorBody.Variables.Add(excLocal);

        var tryFirst = Instruction.Create(OpCodes.Ldarg_0);
        var tryLast = Instruction.Create(OpCodes.Call, loadHandlerRef);
        var leaveTry = Instruction.Create(OpCodes.Leave, afterCall);
        var catchFirst = Instruction.Create(OpCodes.Stloc, excLocal);
        var leaveCatch = Instruction.Create(OpCodes.Leave, afterCall);

        ctorProc.InsertBefore(afterCall, tryFirst);                                    // try: ldarg.0 (receiver for formSettings_Load call)
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldarg_0));          //      ldarg.0 (sender)
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldsfld, eventArgsEmpty));
        ctorProc.InsertBefore(afterCall, tryLast);                                      //      call formSettings_Load(object, EventArgs)
        ctorProc.InsertBefore(afterCall, leaveTry);                                     //      leave.s afterCall
        ctorProc.InsertBefore(afterCall, catchFirst);                                   // catch: stloc excLocal
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldstr, logPath));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldstr, "formSettings ctor: EXCEPTION calling formSettings_Load: "));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldloc, excLocal));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Callvirt, exceptionToString));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Call, stringConcat2));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Ldstr, "\n"));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Call, stringConcat2));
        ctorProc.InsertBefore(afterCall, Instruction.Create(OpCodes.Call, appendAllText));
        ctorProc.InsertBefore(afterCall, leaveCatch);                                   //      leave.s afterCall

        ctorBody.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tryFirst,
            TryEnd = catchFirst,
            HandlerStart = catchFirst,
            HandlerEnd = afterCall,
            CatchType = exceptionTypeRef
        });

        Console.WriteLine($"OK   {fileName}: {targetType}(string)'s constructor -- inserted try {{ formSettings_Load(this, EventArgs.Empty) }} catch (Exception) {{ log }} right after SwitchToTabPanel(...)");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-default-client-notimpl <input-dir> <output-dir>
//
// New bug, surfaced once the Settings panel itself started working: clicking into a category
// (e.g. General, the default) crashes the app. Full stack trace captured from the app's own
// generated bug-report file (C:\users\crossover\AppData\Local\Temp\bug.*.txt):
//   System.NotImplementedException: The method or operation is not implemented.
//     at MailClient.Utils.IApplicationAssociationRegistration.QueryAppIsDefaultAll(...)
//     at MailClient.Utils.Integration.IsDefaultClientVista()
//     at MailClient.Utils.Integration.IsDefaultClient()
//     at ControlSettingsGeneral.checkDefaultClient() / LoadSettings() / ControlSettingsBase.OnLoad()
//
// IApplicationAssociationRegistration is a Windows Shell COM interface ("is this app the default
// mail client") that Wine's shell32 doesn't implement -- confirmed by matching CX_DEBUGMSG fixmes
// seen earlier in this investigation: `fixme:shell:ApplicationAssociationRegistration_QueryInterface
// ... interface not supported` / `fixme:shell:ApplicationAssociationRegistration_QueryAppIsDefaultAll`.
// Rather than a real COM failure (which would normally surface as a COMException), Wine's stub
// apparently throws a raw CLR NotImplementedException through the interop layer.
//
// The good news: IsDefaultClientVista() already has a `catch (COMException) { return false; }`
// handler -- the app's own authors already anticipated "this COM call can fail, and if it does,
// just report not-default" as a safe, defensively-correct fallback. Wine just throws a
// differently-typed exception than expected for that exact scenario. Fix: add a sibling
// `catch (NotImplementedException) { return false; }` handler, structurally identical to (and
// copied from) the existing COMException one -- same protected try region, same handler body,
// just a different CatchType. Not a workaround or a guess about behavior; it's completing the
// error handling the app already has for this exact "the OS integration call isn't available"
// case.
static int RunPatchDefaultClientNotImpl(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-default-client-notimpl <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.Utils.Integration";
    const string targetMethod = "IsDefaultClientVista";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        var body = method.Body;
        var comHandler = body.ExceptionHandlers.FirstOrDefault(h =>
            h.HandlerType == ExceptionHandlerType.Catch &&
            h.CatchType?.FullName == "System.Runtime.InteropServices.COMException");
        if (comHandler is null)
        {
            Console.Error.WriteLine($"FAIL: couldn't find `catch (COMException)` handler in {targetType}::{targetMethod} -- refusing to patch");
            return 1;
        }
        // Paranoid shape check: the handler body must be exactly `pop; ldc.i4.0; stloc.X; leave*`
        // (the pattern for `catch (COMException) { return false; }` compiled with a shared exit
        // point) -- if the compiled shape ever changes, refuse rather than guess.
        var il = body.Instructions;
        int hIdx = il.IndexOf(comHandler.HandlerStart);
        VariableDefinition? stlocVar = null;
        bool shapeOk = hIdx >= 0 && hIdx + 3 < il.Count &&
            il[hIdx].OpCode == OpCodes.Pop &&
            IsLdcI4(il[hIdx + 1], out int zeroCheck) && zeroCheck == 0 &&
            IsStloc(il[hIdx + 2], body, out stlocVar) && stlocVar is not null &&
            (il[hIdx + 3].OpCode == OpCodes.Leave || il[hIdx + 3].OpCode == OpCodes.Leave_S);
        if (!shapeOk)
        {
            Console.Error.WriteLine($"FAIL: {targetType}::{targetMethod}'s catch(COMException) handler body doesn't match the expected `pop; ldc.i4.0; stloc; leave` shape -- refusing to patch");
            return 1;
        }

        var notImplTypeRef = module.ImportReference(typeof(NotImplementedException));
        var proc = body.GetILProcessor();
        var newHandlerFirst = Instruction.Create(OpCodes.Pop);
        var newHandlerLast = Instruction.Create(il[hIdx + 3].OpCode, (Instruction)il[hIdx + 3].Operand!);
        // insert the new handler body right after the existing COMException handler's body, so
        // it isn't itself mistaken for lying inside any other handler's range.
        var afterComHandler = comHandler.HandlerEnd;
        proc.InsertBefore(afterComHandler, newHandlerFirst);
        proc.InsertBefore(afterComHandler, Instruction.Create(OpCodes.Ldc_I4_0));
        proc.InsertBefore(afterComHandler, Instruction.Create(OpCodes.Stloc, stlocVar!));
        proc.InsertBefore(afterComHandler, newHandlerLast);
        // comHandler.HandlerEnd is still the SAME instruction object (afterComHandler) -- an
        // exclusive boundary marker, not a fixed offset. Since 4 new instructions were just
        // inserted immediately before it, its effective (serialized) offset moved later,
        // silently growing the ORIGINAL COMException handler's own range to swallow the new
        // code too (confirmed via --dump-handlers: without this line, COMException's handler
        // range overlapped the new NotImplementedException handler's range entirely -- this,
        // not just handler-table order, is what actually produced the InvalidProgramException).
        // Retarget it to end exactly where the new handler begins.
        comHandler.HandlerEnd = newHandlerFirst;

        // Must be inserted at comHandler's position among its siblings, NOT appended to the end
        // of body.ExceptionHandlers -- the outer try/finally (a wider, less-nested protected
        // region) is already in that list, positioned after all four inner catches, and the CLR
        // exception-handler table requires entries ordered from most-nested to least-nested.
        // Appending unconditionally put this new (more-nested, matches the inner try) handler
        // after the outer finally (less-nested) -- invalid ordering. The CLR verifier catches
        // this at JIT time even though ilspycmd's decompiler didn't complain about it
        // (confirmed: deployed and crashed with `InvalidProgramException: Common Language
        // Runtime detected an invalid program` from this exact method -- decompiling clean is
        // necessary but not sufficient for insertion patches that touch exception regions;
        // getting handler table ORDER right matters even when the instruction stream itself is
        // fine).
        int comHandlerIndex = body.ExceptionHandlers.IndexOf(comHandler);
        body.ExceptionHandlers.Insert(comHandlerIndex + 1, new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = comHandler.TryStart,
            TryEnd = comHandler.TryEnd,
            HandlerStart = newHandlerFirst,
            HandlerEnd = afterComHandler,
            CatchType = notImplTypeRef
        });

        Console.WriteLine($"OK   {fileName}: {targetType}::{targetMethod} -- added `catch (NotImplementedException) {{ return false; }}` alongside the existing catch (COMException)");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-notification-title-icon-clip <input-dir> <output-dir>
//
// User caught live, after --patch-notification-icon-bitmap made the close/settings icons always
// visible (see that patch's own doc comment -- they used to only ever be drawn while
// `mouseOver`, gated by the same `if (mouseOver)` OnPaint block this project's fix removed):
// the title/sender text now runs straight underneath the icons instead of stopping before them.
//
// `OnPaintTitle`'s own IL (dumped via --dump-il) shows the app ALREADY has logic for exactly
// this -- it's just conditional on the wrong thing:
//   Rectangle bounds = headerRect;
//   bounds.X += Padding.Left;
//   if (mouseOver) { bounds.Width = settingsRect.Left - Padding.Horizontal; }
//   else           { bounds.Width -= Padding.Horizontal; }
// i.e. the app was already designed to narrow the title's clip/ellipsis width to stop before the
// icons -- but only while actively hovering, matching the icons' old hover-only visibility. Now
// that the icons are unconditionally visible (drawn into backgroundBitmap regardless of
// mouseOver -- --patch-notification-icon-bitmap's Part 1), the *width* calculation needs to be
// unconditional too, or else the non-hover width (full header minus padding, no icon allowance)
// lets EndEllipsis-truncated text run under the always-visible icons whenever the mouse isn't
// over the notification.
//
// Fix: force the `if (mouseOver)` branch's THEN-path (narrow to settingsRect.Left) to always be
// taken. The raw IL at the branch point is:
//   ldarg.0; ldfld mouseOver; brfalse IL_0074   (else: bounds.Width -= Padding.Horizontal)
// `brfalse` and `pop` have the identical stack effect (pop 1, push 0) -- replacing the `Brfalse`
// instruction's opcode with `Pop` (and clearing its now-inapplicable branch-target operand) is a
// literal drop-in swap: `this`/`mouseOver` are still loaded and evaluated (no other instruction
// needs removing), just no longer branched on, so control always falls straight through into the
// existing then-block instead. The else-block's own instructions are left in place, physically
// unreachable now (no remaining predecessor targets it) but syntactically valid IL -- the same
// "dead but harmless" shape as any other branch-removal in this file; nothing else in the method
// references IL_0074 (confirmed via the same --dump-il output this patch was planned against, and
// re-confirmed against body.Instructions live before editing), so no retargeting (IL-patching
// lesson 2) is needed.
static int RunPatchNotificationTitleIconClip(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-title-icon-clip <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }
        var onPaintTitleMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaintTitle" && m.HasBody);
        if (onPaintTitleMethod is null) { Console.Error.WriteLine("FAIL: OnPaintTitle(PaintEventArgs) not found"); return 1; }
        var mouseOverField = type.Fields.FirstOrDefault(f => f.Name == "mouseOver");
        if (mouseOverField is null) { Console.Error.WriteLine("FAIL: mouseOver field not found"); return 1; }

        var body = onPaintTitleMethod.Body;
        var il = body.GetILProcessor();
        var instrs = body.Instructions;

        // Find the single `Ldfld mouseOver` immediately followed by `Brfalse` -- compare by
        // field .Name, not reference equality (IL-patching lesson 9).
        Instruction? branch = null;
        int matchCount = 0;
        for (int i = 0; i < instrs.Count - 1; i++)
        {
            if (instrs[i].OpCode == OpCodes.Ldfld && instrs[i].Operand is FieldReference fr && fr.Name == mouseOverField.Name &&
                instrs[i + 1].OpCode == OpCodes.Brfalse)
            {
                branch = instrs[i + 1];
                matchCount++;
            }
        }
        if (matchCount != 1 || branch is null)
        {
            Console.Error.WriteLine($"FAIL: expected exactly 1 `Ldfld mouseOver` immediately followed by `Brfalse` in OnPaintTitle, found {matchCount} -- method shape changed, review needed");
            return 1;
        }

        // Confirm nothing else in the method (or its exception handlers, though this method has
        // none) targets the branch's own target instruction -- if something did, leaving it
        // reachable via that other path would be fine, but it's worth knowing either way.
        var branchTarget = (Instruction)branch.Operand;
        int otherRefs = instrs.Count(i => !ReferenceEquals(i, branch) && ReferenceEquals(i.Operand, branchTarget));
        if (otherRefs != 0)
        {
            Console.Error.WriteLine($"FAIL: else-branch target IL offset 0x{branchTarget.Offset:x4} has {otherRefs} other reference(s) besides this branch -- not safe to assume unreachable, review needed");
            return 1;
        }

        branch.OpCode = OpCodes.Pop;
        branch.Operand = null;

        module.Write(destPath);
        Console.WriteLine($"OK   {fileName}: {targetType}::OnPaintTitle -- title width now always narrows to stop before the close/settings icons (previously only while `mouseOver`, which no longer matches the icons' own always-visible behavior since --patch-notification-icon-bitmap)");
        patched = true;
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-license-icon <input-dir> <output-dir>
//
// The License dialog's "Get a license" button (formLicense.buttonGetLicense) shows two tofu
// boxes after the label text. Root cause: the button's Text resource
// (MailClient.UI.Forms.formLicense.resources/buttonGetLicense.Text) contains two literal
// Unicode C1 control characters (U+0083) baked directly into the app's own embedded resource
// data -- confirmed NOT a Wine font-substitution gap: the exact byte pattern (UTF-8 C2 83 C2
// 83) occurs exactly once in the whole assembly (no other button reuses it as a convention),
// there's no "en" satellite resource to compare against (English is the neutral culture baked
// into MailClient.dll itself), and ControlButton's paint code does a plain
// TextRendererEx.DrawText with no icon-glyph special-casing anywhere in its class hierarchy.
// See reports/license-icon-findings.md.
//
// Fix: byte-level replace within the embedded resource blob, C2 83 C2 83 -> C2 A0 C2 A0 (two
// U+0083 -> two U+00A0 non-breaking spaces). Chosen specifically to keep the raw byte length
// identical (2 UTF-8 bytes -> 2 UTF-8 bytes per character) so the .resources container's
// data-section offset table -- which stores absolute byte offsets for every OTHER resource in
// the same container -- needs no adjustment. A length-changing edit (e.g. deleting the two
// characters outright) would require correctly rewriting that offset table for every
// subsequent resource entry, which needs full binary-format parsing this patch deliberately
// avoids by never changing any entry's length.
static int RunPatchLicenseIcon(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-license-icon <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string resourceName = "MailClient.UI.Forms.formLicense.resources";
    byte[] oldBytes = { 0xC2, 0x83, 0xC2, 0x83 };
    byte[] newBytes = { 0xC2, 0xA0, 0xC2, 0xA0 };

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var resource = module.Resources.OfType<EmbeddedResource>().FirstOrDefault(r => r.Name == resourceName);
        if (resource is null)
        {
            Console.Error.WriteLine($"FAIL: embedded resource not found: {resourceName}");
            return 1;
        }

        byte[] data = resource.GetResourceData();
        int idx = IndexOfBytes(data, oldBytes, 0);
        if (idx < 0)
        {
            Console.Error.WriteLine($"FAIL: expected byte pattern C2 83 C2 83 not found in {resourceName} -- refusing to patch");
            return 1;
        }
        int idx2 = IndexOfBytes(data, oldBytes, idx + 1);
        if (idx2 >= 0)
        {
            Console.Error.WriteLine($"FAIL: byte pattern found more than once in {resourceName} -- ambiguous, refusing to patch");
            return 1;
        }

        byte[] newData = (byte[])data.Clone();
        Array.Copy(newBytes, 0, newData, idx, newBytes.Length);

        int resIndex = module.Resources.IndexOf(resource);
        module.Resources[resIndex] = new EmbeddedResource(resource.Name, resource.Attributes, newData);

        Console.WriteLine($"OK   {fileName}: {resourceName} -- replaced buttonGetLicense.Text's trailing U+0083 U+0083 with U+00A0 U+00A0 (non-breaking space) at byte offset 0x{idx:X}");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

// --patch-splash-tip-icon <input-dir> <output-dir>
//
// The splash screen's rotating "tip" label (FormSplashScreen.labelTip) shows two tofu boxes
// before the tip text on every launch. Root cause: labelTip's baseline Text resource
// (MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text) is a single genuine emoji,
// U+1F4A1 (light bulb) -- unlike the license-button bug, this is a real, correctly-encoded
// character (not corrupted control-character bytes), so this one IS a legitimate Wine gap: no
// emoji-capable font is available/consulted for it under Wine, so the surrogate pair renders as
// two missing-glyph boxes. Investigated a proper fix first (vendoring real Segoe UI/Segoe UI
// Emoji fonts + Wine FontLink\SystemLink registry entries into the target bottle -- see
// fonts/ and reports/splash-tip-icon-findings.md for the full investigation, including a real
// `wine regedit /S` multi-string import bug found and worked around along the way): confirmed
// via CX_DEBUGMSG=+font trace that Wine correctly LOADS the SystemLink fallback config, but the
// glyph still doesn't render -- the gap is deeper, in Wine's actual glyph-shaping/ExtTextOut
// code path, not just registry configuration. Falling back to the same narrow, low-risk
// resource-string patch used for the license icon rather than chasing that further.
//
// Fix: byte-level replace within the embedded resource blob, F0 9F 92 A1 (the emoji's 4-byte
// UTF-8 encoding) -> C2 A0 C2 A0 (two U+00A0 non-breaking spaces, also 4 bytes) -- same
// byte-length-preserving rationale as --patch-license-icon: keeps the .resources container's
// data-section offset table untouched. NOTE: the same 4-byte emoji sequence also appears
// elsewhere in MailClient.dll as part of MailClient.Resources.UI.Form.SplashScreenHints'
// EmoticonLookup table (a legitimate, unrelated feature -- compose-window emoticon shortcuts
// like "*IDEA*" -> the bulb emoji) -- irrelevant here since this patch scopes its byte search to
// the FormSplashScreen.resources container specifically, not the whole assembly.
static int RunPatchSplashTipIcon(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-splash-tip-icon <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string resourceName = "MailClient.UI.Forms.FormSplashScreen.resources";
    byte[] oldBytes = { 0xF0, 0x9F, 0x92, 0xA1 };
    byte[] newBytes = { 0xC2, 0xA0, 0xC2, 0xA0 };

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var resource = module.Resources.OfType<EmbeddedResource>().FirstOrDefault(r => r.Name == resourceName);
        if (resource is null)
        {
            Console.Error.WriteLine($"FAIL: embedded resource not found: {resourceName}");
            return 1;
        }

        byte[] data = resource.GetResourceData();
        int idx = IndexOfBytes(data, oldBytes, 0);
        if (idx < 0)
        {
            Console.Error.WriteLine($"FAIL: expected byte pattern F0 9F 92 A1 not found in {resourceName} -- refusing to patch");
            return 1;
        }
        int idx2 = IndexOfBytes(data, oldBytes, idx + 1);
        if (idx2 >= 0)
        {
            Console.Error.WriteLine($"FAIL: byte pattern found more than once in {resourceName} -- ambiguous, refusing to patch");
            return 1;
        }

        byte[] newData = (byte[])data.Clone();
        Array.Copy(newBytes, 0, newData, idx, newBytes.Length);

        int resIndex = module.Resources.IndexOf(resource);
        module.Resources[resIndex] = new EmbeddedResource(resource.Name, resource.Attributes, newData);

        Console.WriteLine($"OK   {fileName}: {resourceName} -- replaced labelTip.Text's U+1F4A1 (light bulb emoji) with U+00A0 U+00A0 (non-breaking space) at byte offset 0x{idx:X}");
        patched = true;

        module.Write(destPath);
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }

    return 0;
}

static int IndexOfBytes(byte[] haystack, byte[] needle, int start)
{
    for (int i = start; i <= haystack.Length - needle.Length; i++)
    {
        bool match = true;
        for (int j = 0; j < needle.Length; j++)
        {
            if (haystack[i + j] != needle[j]) { match = false; break; }
        }
        if (match) return i;
    }
    return -1;
}

// --version <dll> [<dll> ...]
//
// Prints each assembly's AssemblyFileVersion (the precise "10.4.5674.0" style version used to
// gate the release scripts under releases/<version>/ -- see that folder's deploy.sh) and, where
// present, its AssemblyInformationalVersion (includes eM Client's own build commit hash, e.g.
// "10.4.5674+fcbf2a4bb4" -- a stronger identity check than the version number alone, useful if
// two builds ever ship the same version number with different code). Read-only, no patching.
static int RunVersion(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("usage: il-patcher --version <dll> [<dll> ...]");
        return 2;
    }

    foreach (var dllPath in args.Skip(1))
    {
        using var module = ModuleDefinition.ReadModule(dllPath);
        var asmName = module.Assembly.Name;
        string? fileVersion = null;
        string? infoVersion = null;
        foreach (var ca in module.Assembly.CustomAttributes)
        {
            if (ca.AttributeType.Name == "AssemblyFileVersionAttribute" && ca.ConstructorArguments.Count > 0)
                fileVersion = ca.ConstructorArguments[0].Value as string;
            if (ca.AttributeType.Name == "AssemblyInformationalVersionAttribute" && ca.ConstructorArguments.Count > 0)
                infoVersion = ca.ConstructorArguments[0].Value as string;
        }
        Console.WriteLine($"{Path.GetFileName(dllPath)}\tAssemblyVersion={asmName.Version}\tFileVersion={fileVersion ?? "(none)"}\tInformationalVersion={infoVersion ?? "(none)"}");
    }
    return 0;
}

// --check-patched <dll>
//
// Reports (via exit code + a one-line message) whether this exact MailClient.dll already has
// the Stage 5 (license-Activation OAEP) patch applied, by checking whether it references the
// MailClient.Licensing.BouncyCastlePatch assembly -- an assembly reference that cannot exist
// unless our pipeline created it, regardless of eM Client's version or file size. This is the
// check releases/<version>/deploy.sh's re-run safety leans on: file size isn't a reliable
// "already patched" signal across future eM Client releases (an unrelated app change could
// easily land on the same size, or the same patch logic could land on a different size next
// release, independent of whether these patches are applied) -- an assembly-reference check
// has no such failure mode, since the referenced assembly name is unique to this project.
// Since Stage 5 always runs after Stages 1-4 in the pipeline, its presence is treated as a
// reliable proxy for "the whole pipeline has already been applied" -- not airtight proof every
// individual stage is intact (someone could in principle hand-revert one earlier stage and
// leave this one), but far more robust than relying on any single stage's own incidental
// shape-check (which exists to protect that one stage, not to answer this question cleanly).
// Exit 0 + "PATCHED" if the reference is present, exit 1 + "NOT PATCHED" if not.
static int RunCheckPatched(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("usage: il-patcher --check-patched <dll>");
        return 2;
    }
    string dllPath = args[1];
    using var module = ModuleDefinition.ReadModule(dllPath);
    bool found = module.AssemblyReferences.Any(r => r.Name == "MailClient.Licensing.BouncyCastlePatch");
    Console.WriteLine(found ? "PATCHED" : "NOT PATCHED");
    return found ? 0 : 1;
}

static OpCode ShortFormOpCode(int value) => value switch
{
    -1 => OpCodes.Ldc_I4_M1,
    0 => OpCodes.Ldc_I4_0,
    1 => OpCodes.Ldc_I4_1,
    2 => OpCodes.Ldc_I4_2,
    3 => OpCodes.Ldc_I4_3,
    4 => OpCodes.Ldc_I4_4,
    5 => OpCodes.Ldc_I4_5,
    6 => OpCodes.Ldc_I4_6,
    7 => OpCodes.Ldc_I4_7,
    8 => OpCodes.Ldc_I4_8,
    _ => OpCodes.Ldc_I4
};

static bool NeedsOperand(int value) => value is < -1 or > 8;

static string ModeName(int value) => value switch
{
    -1 => "Invalid",
    0 => "Default",
    1 => "Low",
    2 => "High",
    3 => "Bilinear",
    4 => "Bicubic",
    5 => "NearestNeighbor",
    6 => "HighQualityBilinear",
    7 => "HighQualityBicubic",
    _ => $"Unknown({value})"
};

static bool IsLdcI4(Instruction insn, out int value)
{
    value = 0;
    if (insn.OpCode == OpCodes.Ldc_I4)
    {
        value = (int)insn.Operand;
        return true;
    }
    if (insn.OpCode == OpCodes.Ldc_I4_S)
    {
        value = (sbyte)insn.Operand;
        return true;
    }
    if (insn.OpCode == OpCodes.Ldc_I4_M1) { value = -1; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_0) { value = 0; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_1) { value = 1; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_2) { value = 2; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_3) { value = 3; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_4) { value = 4; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_5) { value = 5; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_6) { value = 6; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_7) { value = 7; return true; }
    if (insn.OpCode == OpCodes.Ldc_I4_8) { value = 8; return true; }
    return false;
}

static bool IsStloc(Instruction insn, MethodBody body, out VariableDefinition? variable)
{
    variable = null;
    if (insn.OpCode == OpCodes.Stloc || insn.OpCode == OpCodes.Stloc_S)
    {
        variable = (VariableDefinition)insn.Operand;
        return true;
    }
    if (insn.OpCode == OpCodes.Stloc_0) { variable = body.Variables[0]; return true; }
    if (insn.OpCode == OpCodes.Stloc_1) { variable = body.Variables[1]; return true; }
    if (insn.OpCode == OpCodes.Stloc_2) { variable = body.Variables[2]; return true; }
    if (insn.OpCode == OpCodes.Stloc_3) { variable = body.Variables[3]; return true; }
    return false;
}

// --patch-account-manager-sync-async <input-dir> <output-dir>
//
// Root-cause fix for the freeze traced in reports/exchange-sync-freeze-findings.md: the
// once-a-minute auto-sync timer (DesktopAccountManager.timerSendAndReceive, a plain
// System.Windows.Forms.Timer -- confirmed via decompile its Tick always fires on the UI
// thread) calls AccountManager.SendAndReceiveAll(...) directly and synchronously, whose own
// sequential per-account loop can block for up to 20 seconds per account on Wine's
// GetAddrInfoExW (which can't honor a connection-timeout cancellation), freezing the whole
// app for however long that takes.
//
// A first version of this fix patched only the timer's own call site
// (DesktopAccountManager.timerSendAndReceive_Tick). Live testing found the app can ALSO
// freeze immediately on startup, before the timer ever fires -- traced to eM Client's own
// "check for mail on startup" option and other direct call sites (menu items, shortcuts, a
// Synchronize() handler tied to initial folder selection) that all call
// AccountManager.SendAndReceiveAll(...) directly too, none of which that narrower, timer-only
// version touched. Superseded by this patch and not shipped.
//
// This patch moves the guard+background-dispatch down into SendAndReceiveAll itself (in
// MailClient.Accounts.dll, the shared base every one of those call sites funnels through),
// so it's fixed once for all callers instead of needing every call site found and patched
// individually.
//
// To avoid hand-authoring a faithful copy of SendAndReceiveAll's own body (it has a couple of
// null-conditional delegate/event invokes -- `logger?.Invoke(...)`, `SendingAndOrReceiving?.
// Invoke(...)` -- that are easy to get subtly wrong by hand), this patch REUSES the existing,
// already-correct compiled MethodBody verbatim in a new private method
// (__RunSendAndReceiveAllCore), and replaces the original method's body with a small fresh
// guard/dispatch wrapper. Since the new method keeps the exact same (bool) parameter shape
// and stays on the same type, no operand rewriting is needed inside the reused body at all
// -- its ldarg_0/ldarg_1 and internal member references are already correct as-is.
static int RunPatchAccountManagerSyncAsync(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-account-manager-sync-async <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.Accounts.dll";
    const string targetType = "MailClient.Accounts.AccountManager";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var sendAndReceiveAll = type.Methods.FirstOrDefault(m =>
            m.Name == "SendAndReceiveAll" && m.Parameters.Count == 1 && !m.IsStatic);
        if (sendAndReceiveAll is null) { Console.Error.WriteLine("FAIL: SendAndReceiveAll(bool) not found"); return 1; }

        var sendReceiveAllowedField = type.Fields.FirstOrDefault(f => f.Name == "sendReceiveAllowed");
        if (sendReceiveAllowedField is null) { Console.Error.WriteLine("FAIL: sendReceiveAllowed field not found"); return 1; }

        var boolType = module.TypeSystem.Boolean;
        var voidType = module.TypeSystem.Void;

        var syncInProgressField = new FieldDefinition("__syncInProgress", FieldAttributes.Private, boolType);
        type.Fields.Add(syncInProgressField);
        var pendingArgField = new FieldDefinition("__pendingCheckIncludeInGlobalOperations", FieldAttributes.Private, boolType);
        type.Fields.Add(pendingArgField);

        // Dedicated background Thread, not Task.Run -- Task.Run schedules onto the shared
        // .NET ThreadPool, which this project's OWN account-level sync already avoids for
        // exactly this reason (DefaultSynchronizationQueue uses a raw dedicated Thread per
        // account, not the ThreadPool). A first version of this patch used Task.Run and was
        // deployed; live testing then showed a new stutter pattern (freeze, ~1s unfreeze,
        // freeze again) once multiple accounts' and folders' Task.Run calls could be in
        // flight together, each able to block a pooled worker for up to 20s on the same Wine
        // DNS issue -- the ThreadPool's own slow ramp-up under sustained demand (roughly one
        // new thread per 500ms-1s) produces exactly that stutter shape. Matching the app's own
        // established pattern (a dedicated Thread per unit of potentially-blocking work)
        // avoids competing with the ThreadPool at all.
        var threadCtor = module.ImportReference(
            typeof(System.Threading.Thread).GetConstructor(new[] { typeof(System.Threading.ThreadStart) }));
        var threadStartCtor = module.ImportReference(
            typeof(System.Threading.ThreadStart).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        var isBackgroundSetter = module.ImportReference(
            typeof(System.Threading.Thread).GetProperty("IsBackground")!.GetSetMethod());
        var threadStartMethod = module.ImportReference(
            typeof(System.Threading.Thread).GetMethod("Start", Type.EmptyTypes));
        var exceptionType = module.ImportReference(typeof(Exception));

        // --- New method: __RunSendAndReceiveAllCore(bool) -- takes over SendAndReceiveAll's
        // ORIGINAL body verbatim (see doc comment above for why: reuse over hand-authoring).
        var coreMethod = new MethodDefinition("__RunSendAndReceiveAllCore",
            MethodAttributes.Private | MethodAttributes.HideBySig, voidType);
        coreMethod.Parameters.Add(new ParameterDefinition("checkIncludeInGlobalOperations", ParameterAttributes.None, boolType));
        coreMethod.Body = sendAndReceiveAll.Body;
        type.Methods.Add(coreMethod);

        // --- New method: __syncTaskEntry() -- parameterless (a plain Thread's ThreadStart
        // delegate needs a parameterless target), runs the reused core body on the dedicated
        // background thread, always resetting __syncInProgress afterward regardless of
        // outcome. Single try/catch(Exception), not a hand-built try/finally -- simpler,
        // already-precedented IL shape (see --patch-folder-sync-async for the same pattern),
        // same guarantee that the flag always clears.
        var entryMethod = new MethodDefinition("__syncTaskEntry",
            MethodAttributes.Private | MethodAttributes.HideBySig, voidType);
        type.Methods.Add(entryMethod);
        {
            var body = entryMethod.Body;
            var il = body.GetILProcessor();
            var exVar = new VariableDefinition(exceptionType);
            body.Variables.Add(exVar);

            var tryStart = il.Create(OpCodes.Ldarg_0);
            il.Append(tryStart);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, pendingArgField));
            il.Append(il.Create(OpCodes.Call, coreMethod));
            var leaveAfterTry = il.Create(OpCodes.Leave, tryStart);
            il.Append(leaveAfterTry);

            var catchStart = il.Create(OpCodes.Stloc, exVar);
            il.Append(catchStart);
            var leaveAfterCatch = il.Create(OpCodes.Leave, tryStart);
            il.Append(leaveAfterCatch);

            var resetPoint = il.Create(OpCodes.Ldarg_0);
            il.Append(resetPoint);
            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Stfld, syncInProgressField));
            il.Append(il.Create(OpCodes.Ret));

            leaveAfterTry.Operand = resetPoint;
            leaveAfterCatch.Operand = resetPoint;

            body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = resetPoint,
                CatchType = exceptionType
            });

            body.SimplifyMacros();
        }

        // --- Fresh, small body for SendAndReceiveAll itself: guard, stash the arg, fire the
        // background task. Everything the original body used to do now happens inside
        // __RunSendAndReceiveAllCore instead.
        {
            var body = sendAndReceiveAll.Body = new MethodBody(sendAndReceiveAll);
            var il = body.GetILProcessor();
            var end = il.Create(OpCodes.Ret);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, sendReceiveAllowedField));
            il.Append(il.Create(OpCodes.Brfalse, end));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Ldfld, syncInProgressField));
            il.Append(il.Create(OpCodes.Brtrue, end));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1)); // checkIncludeInGlobalOperations
            il.Append(il.Create(OpCodes.Stfld, pendingArgField));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Stfld, syncInProgressField));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldftn, entryMethod));
            il.Append(il.Create(OpCodes.Newobj, threadStartCtor));
            il.Append(il.Create(OpCodes.Newobj, threadCtor));
            il.Append(il.Create(OpCodes.Dup));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Callvirt, isBackgroundSetter));
            il.Append(il.Create(OpCodes.Callvirt, threadStartMethod));

            il.Append(end);

            body.SimplifyMacros();
        }

        module.Write(destPath);
        patched = true;
        Console.WriteLine($"OK   {fileName}: MailClient.Accounts.AccountManager::SendAndReceiveAll -- moved the original method body into a new __RunSendAndReceiveAllCore, guarded by a volatile __syncInProgress field and dispatched via a dedicated background Thread (not Task.Run -- see doc comment); fixes every call site (timer, startup check, menu, shortcuts), not just the periodic timer");
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }
    return 0;
}

// --patch-folder-sync-async <input-dir> <output-dir>
//
// Second guarded entry point alongside --patch-account-manager-sync-async: Folder.Synchronize
// (bool, bool) (MailClient.Accounts.dll) is called directly and synchronously by the manual
// refresh path
// (formMain.sendReceiveAll() calls SelectedFolder?.Synchronize(forced: true, fromUI: true)
// BEFORE it ever reaches AccountManager.SendAndReceiveAll), and is also the entry point the
// "download messages for offline use" folder-tree walk very likely funnels through (its own
// OfflineSynchronizationScope/Mode settings only gate how much gets downloaded per folder --
// see reports/exchange-sync-freeze-findings.md -- not a separate trigger path). Its own
// private recursive overload (Synchronize(SynchronizationPriority, bool)) walks subfolders
// sequentially and isn't touched by this patch at all -- unlike the SendAndReceiveAll fix,
// there's no need to reuse or duplicate its body: the public method's original body is just
// one ternary and one call to that private overload, so it's simpler to hand-author the
// guard/dispatch wrapper directly and leave the entire recursive walk (however deep) running
// on the single background thread the wrapper dispatches to -- it never touches the UI thread
// either way, regardless of how long the walk takes.
//
// SynchronizationPriority's enum values (Background/BackgroundForced) are read directly from
// this field's Constant in the TARGET module, not hardcoded or guessed via reflection --
// the same class of mistake (assuming an enum's numeric values instead of reading them) was
// made and caught, via the mandatory decompile check, getting GCLargeObjectHeapCompactionMode
// wrong in an earlier draft of --patch-account-manager-sync-async; reading the real value
// from the assembly being patched removes the guesswork entirely.
static int RunPatchFolderSyncAsync(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-folder-sync-async <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.Accounts.dll";
    const string targetType = "MailClient.Storage.Application.Folder";
    const string priorityType = "MailClient.Storage.Synchronization.SynchronizationPriority";

    var allDlls = Directory.GetFiles(inDir, "*.dll", SearchOption.TopDirectoryOnly);
    bool patched = false;

    foreach (var dllPath in allDlls)
    {
        string fileName = Path.GetFileName(dllPath);
        string destPath = Path.Combine(outDir, fileName);

        if (fileName != targetAssembly)
        {
            File.Copy(dllPath, destPath, overwrite: true);
            continue;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(inDir);
        using var module = ModuleDefinition.ReadModule(dllPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            ReadWrite = false
        });

        var type = module.GetType(targetType);
        if (type is null) { Console.Error.WriteLine($"FAIL: type not found: {targetType}"); return 1; }

        var publicSync = type.Methods.FirstOrDefault(m =>
            m.Name == "Synchronize" && m.IsPublic && m.Parameters.Count == 2 &&
            m.Parameters[0].ParameterType.FullName == "System.Boolean");
        if (publicSync is null) { Console.Error.WriteLine("FAIL: public Synchronize(bool,bool) not found"); return 1; }

        var privateSync = type.Methods.FirstOrDefault(m =>
            m.Name == "Synchronize" && !m.IsPublic && m.Parameters.Count == 2 &&
            m.Parameters[0].ParameterType.FullName == priorityType);
        if (privateSync is null) { Console.Error.WriteLine("FAIL: private Synchronize(SynchronizationPriority,bool) not found"); return 1; }

        var priorityTypeDef = module.GetType(priorityType);
        if (priorityTypeDef is null) { Console.Error.WriteLine($"FAIL: type not found: {priorityType}"); return 1; }
        var backgroundField = priorityTypeDef.Fields.FirstOrDefault(f => f.Name == "Background");
        var backgroundForcedField = priorityTypeDef.Fields.FirstOrDefault(f => f.Name == "BackgroundForced");
        if (backgroundField?.Constant is null || backgroundForcedField?.Constant is null)
        {
            Console.Error.WriteLine("FAIL: SynchronizationPriority.Background/BackgroundForced constants not found");
            return 1;
        }
        int backgroundValue = Convert.ToInt32(backgroundField.Constant);
        int backgroundForcedValue = Convert.ToInt32(backgroundForcedField.Constant);

        var boolType = module.TypeSystem.Boolean;
        var voidType = module.TypeSystem.Void;

        var syncInProgressField = new FieldDefinition("__folderSyncInProgress", FieldAttributes.Private, boolType);
        type.Fields.Add(syncInProgressField);
        var pendingForcedField = new FieldDefinition("__pendingForced", FieldAttributes.Private, boolType);
        type.Fields.Add(pendingForcedField);
        var pendingFromUiField = new FieldDefinition("__pendingFromUI", FieldAttributes.Private, boolType);
        type.Fields.Add(pendingFromUiField);

        // Dedicated background Thread, not Task.Run -- see the matching comment in
        // --patch-account-manager-sync-async for why: Task.Run's shared ThreadPool caused a
        // new stutter pattern once multiple accounts' and folders' dispatches could overlap,
        // fixed by matching the app's own dedicated-Thread-per-account pattern instead.
        var threadCtor = module.ImportReference(
            typeof(System.Threading.Thread).GetConstructor(new[] { typeof(System.Threading.ThreadStart) }));
        var threadStartCtor = module.ImportReference(
            typeof(System.Threading.ThreadStart).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        var isBackgroundSetter = module.ImportReference(
            typeof(System.Threading.Thread).GetProperty("IsBackground")!.GetSetMethod());
        var threadStartMethod = module.ImportReference(
            typeof(System.Threading.Thread).GetMethod("Start", Type.EmptyTypes));
        var exceptionType = module.ImportReference(typeof(Exception));

        // --- New method: __folderSyncTaskEntry() -- calls the ORIGINAL, untouched private
        // recursive Synchronize(SynchronizationPriority, bool) directly (not the public
        // overload -- that's the one being replaced below), on whatever thread Task.Run
        // schedules it on. Single try/catch(Exception), same reasoning as the previous two
        // patches in this chain: simpler, already-precedented shape than a hand-built
        // try/finally, same guarantee that the flag always clears.
        var entryMethod = new MethodDefinition("__folderSyncTaskEntry",
            MethodAttributes.Private | MethodAttributes.HideBySig, voidType);
        type.Methods.Add(entryMethod);
        {
            var body = entryMethod.Body;
            var il = body.GetILProcessor();
            var exVar = new VariableDefinition(exceptionType);
            body.Variables.Add(exVar);

            var tryStart = il.Create(OpCodes.Ldarg_0);
            il.Append(tryStart);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, pendingForcedField));
            var pushBackground = il.Create(OpCodes.Ldc_I4, backgroundValue);
            il.Append(il.Create(OpCodes.Brfalse, pushBackground));
            il.Append(il.Create(OpCodes.Ldc_I4, backgroundForcedValue));
            var afterPriority = il.Create(OpCodes.Ldarg_0);
            il.Append(il.Create(OpCodes.Br, afterPriority));
            il.Append(pushBackground);
            il.Append(afterPriority);
            il.Append(il.Create(OpCodes.Ldfld, pendingFromUiField));
            il.Append(il.Create(privateSync.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, module.ImportReference(privateSync)));
            var leaveAfterTry = il.Create(OpCodes.Leave, tryStart);
            il.Append(leaveAfterTry);

            var catchStart = il.Create(OpCodes.Stloc, exVar);
            il.Append(catchStart);
            var leaveAfterCatch = il.Create(OpCodes.Leave, tryStart);
            il.Append(leaveAfterCatch);

            var resetPoint = il.Create(OpCodes.Ldarg_0);
            il.Append(resetPoint);
            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Stfld, syncInProgressField));
            il.Append(il.Create(OpCodes.Ret));

            leaveAfterTry.Operand = resetPoint;
            leaveAfterCatch.Operand = resetPoint;

            body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = resetPoint,
                CatchType = exceptionType
            });

            body.SimplifyMacros();
        }

        // --- Fresh body for the PUBLIC Synchronize(bool, bool): guard, stash both args,
        // fire the background task. The private recursive overload is never touched.
        {
            var body = publicSync.Body = new MethodBody(publicSync);
            var il = body.GetILProcessor();
            var end = il.Create(OpCodes.Ret);

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Ldfld, syncInProgressField));
            il.Append(il.Create(OpCodes.Brtrue, end));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1)); // forced
            il.Append(il.Create(OpCodes.Stfld, pendingForcedField));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_2)); // fromUI
            il.Append(il.Create(OpCodes.Stfld, pendingFromUiField));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Volatile));
            il.Append(il.Create(OpCodes.Stfld, syncInProgressField));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldftn, entryMethod));
            il.Append(il.Create(OpCodes.Newobj, threadStartCtor));
            il.Append(il.Create(OpCodes.Newobj, threadCtor));
            il.Append(il.Create(OpCodes.Dup));
            il.Append(il.Create(OpCodes.Ldc_I4_1));
            il.Append(il.Create(OpCodes.Callvirt, isBackgroundSetter));
            il.Append(il.Create(OpCodes.Callvirt, threadStartMethod));

            il.Append(end);

            body.SimplifyMacros();
        }

        module.Write(destPath);
        patched = true;
        Console.WriteLine($"OK   {fileName}: MailClient.Storage.Application.Folder::Synchronize(bool,bool) -- guarded by a new volatile __folderSyncInProgress field and dispatched via a dedicated background Thread (not Task.Run -- see doc comment); the private recursive Synchronize(SynchronizationPriority,bool) overload is untouched and now always runs off the UI thread, however deep the subfolder walk goes");
    }

    if (!patched)
    {
        Console.Error.WriteLine($"FAIL: {targetAssembly} not found in {inDir}");
        return 1;
    }
    return 0;
}

record PatchEntry(string Assembly, string Type, string Method, string IlOffset, int OldValue, int NewValue);
