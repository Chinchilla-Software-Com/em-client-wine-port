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

if (args.Length > 0 && args[0] == "--patch-diag")
{
    return RunPatchDiag(args);
}

if (args.Length > 0 && args[0] == "--patch-auto-test-notification")
{
    return RunPatchAutoTestNotification(args);
}

if (args.Length > 0 && args[0] == "--patch-close-listener")
{
    return RunPatchCloseListener(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-layout-diag")
{
    return RunPatchNotificationLayoutDiag(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-force-onload")
{
    return RunPatchNotificationForceOnLoad(args);
}

if (args.Length > 0 && args[0] == "--patch-test-monogram-avatar")
{
    return RunPatchTestMonogramAvatar(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-title-singleline")
{
    return RunPatchNotificationTitleSingleLine(args);
}

if (args.Length > 0 && args[0] == "--patch-notification-geometry-diag")
{
    return RunPatchNotificationGeometryDiag(args);
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

if (args.Length > 0 && args[0] == "--patch-license-icon")
{
    return RunPatchLicenseIcon(args);
}

if (args.Length > 0 && args[0] == "--patch-splash-tip-icon")
{
    return RunPatchSplashTipIcon(args);
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

// --patch-diag <input-dir> <output-dir>
//
// Temporary instrumentation patch (not meant to ship) -- current target superseded from the
// ControlDataGrid/Settings-panel investigation (see git history for that version), then from the
// original OnPaint/timer_OnTimer-only version (see git history for that revision). Now also
// instruments FormGenericNotification.Hide() and OnMouseClick(MouseEventArgs), plus
// MailClient.UI.Notifications.MailNotificationHandler.notificationForm_Click (the click handler
// that fires ContentClick -> PerformAction -> formMail.ShowMailForm), to chase the fifth-round
// finding in reports/notification-empty-until-fade-findings.md: clicking a no-text notification
// (auto-hide disabled) after ~6s (~timeToStay) hangs the app, but clicking within that window
// doesn't. Goal: see the actual interleaving of state/timer/click activity around the hang,
// rather than continuing to infer it from screen recordings.
//
// Sixth round found real clicks never fire OnMouseClick at all (0 hits every time) -- they land
// on LayeredBaseForm's separate drop-shadow companion window, whose own layeredWindow_Click
// handler calls performMouseClick() directly -- and that a single physical click fired
// notificationForm_Click *twice*, 207ms apart. OnShown() does `layeredWindow.Click +=
// layeredWindow_Click;` with no matching -= anywhere in the class, and Show()/OnShown() can run
// more than once per notification's lifecycle (e.g. via Reshow()) -- if OnShown() ran twice
// before the click, the same handler would be subscribed to the event twice, and one Click raise
// would invoke it twice. OnShown and layeredWindow_Click are now instrumented too, to confirm
// this directly: does OnShown fire more than once before the first click, and does
// layeredWindow_Click itself fire twice per physical click (consistent with double-subscription)
// or is there some other explanation (e.g. two genuine separate click messages)?
//
// Original instrumentation still in place: for the notification a user sees as "an empty box
// that only shows text right as it starts to fade", which concrete window (type/handle/size) is
// it, and what are `state`/`title`/`content` (the fields OnPaint/OnPaintTitle draw from) at each
// actual OnPaint call and each timer_OnTimer fade-animation tick?
static int RunPatchDiag(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-diag <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);

    const string targetAssembly = "MailClient.dll";
    const string targetType = "MailClient.UI.Forms.NotificationForms.FormGenericNotification";
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

        var onPaintMethod = type.Methods.FirstOrDefault(m => m.Name == "OnPaint" && m.HasBody && m.Parameters.Count == 1);
        var timerMethod = type.Methods.FirstOrDefault(m => m.Name == "timer_OnTimer" && m.HasBody);
        var hideMethod = type.Methods.FirstOrDefault(m => m.Name == "Hide" && m.HasBody && m.Parameters.Count == 0);
        var onMouseClickMethod = type.Methods.FirstOrDefault(m => m.Name == "OnMouseClick" && m.HasBody && m.Parameters.Count == 1);
        var onShownMethod = type.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody);
        var layeredWindowClickMethod = type.Methods.FirstOrDefault(m => m.Name == "layeredWindow_Click" && m.HasBody);
        var updateBackgroundBitmapMethod = type.Methods.FirstOrDefault(m => m.Name == "updateBackgroundBitmap" && m.HasBody);
        var doLayoutMethod = type.Methods.FirstOrDefault(m => m.Name == "doLayout" && m.HasBody);
        if (onPaintMethod is null) { Console.Error.WriteLine("FAIL: OnPaint(PaintEventArgs) not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }
        if (hideMethod is null) { Console.Error.WriteLine("FAIL: Hide() not found"); return 1; }
        if (onMouseClickMethod is null) { Console.Error.WriteLine("FAIL: OnMouseClick(MouseEventArgs) not found"); return 1; }
        if (onShownMethod is null) { Console.Error.WriteLine("FAIL: OnShown not found"); return 1; }
        if (layeredWindowClickMethod is null) { Console.Error.WriteLine("FAIL: layeredWindow_Click not found"); return 1; }
        if (updateBackgroundBitmapMethod is null) { Console.Error.WriteLine("FAIL: updateBackgroundBitmap not found"); return 1; }
        if (doLayoutMethod is null) { Console.Error.WriteLine("FAIL: doLayout not found"); return 1; }

        var layeredBaseFormType = type.BaseType?.Resolve();
        if (layeredBaseFormType is null || layeredBaseFormType.FullName != "MailClient.UI.Forms.LayeredBaseForm")
        {
            Console.Error.WriteLine($"FAIL: expected base type MailClient.UI.Forms.LayeredBaseForm, got {layeredBaseFormType?.FullName}");
            return 1;
        }
        var backgroundBitmapField = layeredBaseFormType.Fields.FirstOrDefault(f => f.Name == "backgroundBitmap");
        if (backgroundBitmapField is null) { Console.Error.WriteLine("FAIL: LayeredBaseForm missing backgroundBitmap field"); return 1; }
        var headerRectField = type.Fields.FirstOrDefault(f => f.Name == "headerRect");
        if (headerRectField is null) { Console.Error.WriteLine($"FAIL: {targetType} missing headerRect field"); return 1; }
        // Resolve Rectangle.get_IsEmpty from headerRectField's own FieldType rather than via
        // `typeof(System.Drawing.Rectangle)` reflection -- reflecting on the *patching tool's*
        // own .NET 10 runtime bakes in a `System.Drawing.Primitives, Version=10.0.0.0` reference,
        // which doesn't exist alongside the app (it ships its own .NET 8 build, Version=8.0.x) and
        // isn't unified/forwarded the way core BCL types (Environment, File, int) are -- hit this
        // for real: crashed with FileNotFoundException on that exact assembly/version the moment
        // updateBackgroundBitmap() ran. Resolving from the field's already-correctly-versioned
        // FieldType avoids the mismatch entirely.
        var rectangleType = headerRectField.FieldType.Resolve();
        var rectangleIsEmptyGetterDef = rectangleType?.Methods.FirstOrDefault(m => m.Name == "get_IsEmpty");
        if (rectangleIsEmptyGetterDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle.get_IsEmpty from headerRect's own FieldType"); return 1; }
        var rectangleIsEmptyGetter = module.ImportReference(rectangleIsEmptyGetterDef);

        const string handlerTypeName = "MailClient.UI.Notifications.MailNotificationHandler";
        var handlerType = module.GetType(handlerTypeName);
        if (handlerType is null) { Console.Error.WriteLine($"FAIL: type not found: {handlerTypeName}"); return 1; }
        var clickHandlerMethod = handlerType.Methods.FirstOrDefault(m => m.Name == "notificationForm_Click" && m.HasBody);
        if (clickHandlerMethod is null) { Console.Error.WriteLine("FAIL: notificationForm_Click not found"); return 1; }

        var stateField = type.Fields.FirstOrDefault(f => f.Name == "state");
        var titleField = type.Fields.FirstOrDefault(f => f.Name == "title");
        var contentField = type.Fields.FirstOrDefault(f => f.Name == "content");
        if (stateField is null || titleField is null || contentField is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find state/title/content fields");
            return 1;
        }

        // il-patcher doesn't reference System.Windows.Forms directly -- resolve Control's
        // Handle/Width/Height getters by walking up FormGenericNotification's own base-type
        // chain via Cecil, same technique used elsewhere in this file (see doPaint's
        // controlGetName/controlGetVisible resolution, further down in git history for this
        // file's ControlDataGrid-targeted version).
        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var getHandleDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Handle");
        var getWidthDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Width");
        var getHeightDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Height");
        if (getHandleDef is null || getWidthDef is null || getHeightDef is null)
        {
            Console.Error.WriteLine("FAIL: Control missing get_Handle/get_Width/get_Height");
            return 1;
        }
        var getHandle = module.ImportReference(getHandleDef);
        var getWidth = module.ImportReference(getWidthDef);
        var getHeight = module.ImportReference(getHeightDef);

        MethodReference Import(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var intPtrToInt32 = Import(typeof(IntPtr).GetMethod("ToInt32", Type.EmptyTypes)!);
        var int32ToStringX = Import(typeof(int).GetMethod("ToString", new[] { typeof(string) })!);
        var int32ToString = Import(typeof(int).GetMethod("ToString", Type.EmptyTypes)!);
        var objectGetType = Import(typeof(object).GetMethod("GetType")!);
        var typeGetName = Import(typeof(Type).GetProperty("Name")!.GetGetMethod()!);
        var tickCountGetter = Import(typeof(Environment).GetProperty("TickCount")!.GetGetMethod()!);
        var stringConcat2 = Import(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        var appendAllText = Import(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);
        // DateTime is a CoreLib/System.Runtime-forwarded type (safe to reflect via typeof() on
        // il-patcher's own runtime -- see CLAUDE.md's IL-patching lesson 5 on why this is NOT safe
        // for app-deployed assemblies like System.Drawing.Primitives). Wall-clock alongside
        // TickCount lets a log line be matched directly against an ffmpeg screen recording's real
        // timestamps instead of an arbitrary relative counter.
        var dateTimeNowGetter = Import(typeof(DateTime).GetProperty("Now")!.GetGetMethod()!);
        var dateTimeToStringFmt = Import(typeof(DateTime).GetMethod("ToString", new[] { typeof(string) })!);
        var dateTimeTypeRef = module.ImportReference(typeof(DateTime));

        // Shared instrumentation: inserts several labeled log lines at the very start of the
        // given method's body (the one insertion point proven safe against all three
        // IL-patching pitfalls documented in CLAUDE.md -- no branch can target the method's own
        // first instruction, and there's no exception-handler region to accidentally grow).
        void Instrument(MethodDefinition method, string label)
        {
            var body = method.Body;
            body.InitLocals = true;
            var tmpInt = new VariableDefinition(module.TypeSystem.Int32);
            var tmpMsg = new VariableDefinition(module.TypeSystem.String);
            var tmpDate = new VariableDefinition(dateTimeTypeRef);
            body.Variables.Add(tmpInt);
            body.Variables.Add(tmpMsg);
            body.Variables.Add(tmpDate);
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            // "<label> tick=<TickCount> type=<TypeName> wall=<HH:mm:ss.fff>\n"
            Emit(
                Instruction.Create(OpCodes.Call, tickCountGetter),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldstr, label + " tick="),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " type="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, objectGetType),
                Instruction.Create(OpCodes.Callvirt, typeGetName),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " wall="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Call, dateTimeNowGetter),
                Instruction.Create(OpCodes.Stloc, tmpDate),
                Instruction.Create(OpCodes.Ldloca, tmpDate),
                Instruction.Create(OpCodes.Ldstr, "HH:mm:ss.fff"),
                Instruction.Create(OpCodes.Call, dateTimeToStringFmt),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Stloc, tmpMsg),
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldloc, tmpMsg),
                Instruction.Create(OpCodes.Call, appendAllText)
            );

            // "<label> handle=0x<hex> size=<W>x<H> state=<N>\n"
            // int.ToString(string) is an instance method on a value type -- like the
            // parameterless int32ToString calls elsewhere, it needs its receiver as a managed
            // pointer (Ldloca into a stored local), not a raw value straight off the stack, so
            // every numeric piece below goes through the same Stloc tmpInt; Ldloca tmpInt; Call
            // pattern before being concatenated in.
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getHandle),
                Instruction.Create(OpCodes.Callvirt, intPtrToInt32),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldstr, label + " handle=0x"),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Ldstr, "X"),
                Instruction.Create(OpCodes.Call, int32ToStringX),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " size="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getWidth),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "x"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getHeight),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " state="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, stateField),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Stloc, tmpMsg),
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldloc, tmpMsg),
                Instruction.Create(OpCodes.Call, appendAllText)
            );

            // "<label> title=<title>\n"
            Emit(
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldstr, label + " title="),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, titleField),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) })!)),
                Instruction.Create(OpCodes.Call, appendAllText)
            );

            // "<label> content=<content>\n---\n"
            Emit(
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldstr, label + " content="),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, contentField),
                Instruction.Create(OpCodes.Ldstr, "\n---\n"),
                Instruction.Create(OpCodes.Call, module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) })!)),
                Instruction.Create(OpCodes.Call, appendAllText)
            );
        }

        // Minimal instrumentation for a method on a type that doesn't have state/title/content
        // fields or derive from Control (MailNotificationHandler is a plain class) -- just
        // "<label> tick=<TickCount> type=<TypeName>\n", reusing the same tick+type Emit shape as
        // the first block of Instrument() above, but as its own insertion (no shared locals with
        // Instrument's closure, since this runs against a different MethodDefinition/body).
        void InstrumentMinimal(MethodDefinition method, string label)
        {
            var body = method.Body;
            body.InitLocals = true;
            var tmpInt = new VariableDefinition(module.TypeSystem.Int32);
            var tmpMsg = new VariableDefinition(module.TypeSystem.String);
            var tmpDate = new VariableDefinition(dateTimeTypeRef);
            body.Variables.Add(tmpInt);
            body.Variables.Add(tmpMsg);
            body.Variables.Add(tmpDate);
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            Emit(
                Instruction.Create(OpCodes.Call, tickCountGetter),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldstr, label + " tick="),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " type="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, objectGetType),
                Instruction.Create(OpCodes.Callvirt, typeGetName),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " wall="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Call, dateTimeNowGetter),
                Instruction.Create(OpCodes.Stloc, tmpDate),
                Instruction.Create(OpCodes.Ldloca, tmpDate),
                Instruction.Create(OpCodes.Ldstr, "HH:mm:ss.fff"),
                Instruction.Create(OpCodes.Call, dateTimeToStringFmt),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Stloc, tmpMsg),
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldloc, tmpMsg),
                Instruction.Create(OpCodes.Call, appendAllText)
            );
        }

        // Bitmap-caching diagnostic: was backgroundBitmap null *before* this call (i.e. is a
        // rebuild about to happen at all, per updateLayeredBackground's `refreshBitmap ||
        // backgroundBitmap == null` gate), and would updateBackgroundBitmap's own early-return
        // guard (`headerRect.IsEmpty || Width == 0 || Height < headerHeight`) skip the rebuild
        // even if attempted? Chases the theory in reports/notification-empty-until-fade-
        // findings.md's eighth round: FormNotificationPresenter.ShowNotification() calls Show()
        // once before Title/Content are set and once after (via ShowNotification->Reshow), and
        // every OnShown()/timer_OnTimer() call passes refreshBitmap:false -- so if a bitmap
        // already exists (built blank on the first Show()), the second Show() (with real content)
        // never rebuilds it, leaving a stale/blank bitmap on screen until something else forces a
        // refreshBitmap:true rebuild.
        void InstrumentBitmapDiag(MethodDefinition method, string label)
        {
            var mBody = method.Body;
            mBody.InitLocals = true;
            var tmpInt = new VariableDefinition(module.TypeSystem.Int32);
            var tmpMsg = new VariableDefinition(module.TypeSystem.String);
            var tmpDate = new VariableDefinition(dateTimeTypeRef);
            mBody.Variables.Add(tmpInt);
            mBody.Variables.Add(tmpMsg);
            mBody.Variables.Add(tmpDate);
            var il = mBody.GetILProcessor();
            var first = mBody.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            // "<label> tick=<T> type=<Type> wall=<HH:mm:ss.fff> bgWasNull=<0/1>\n"
            Emit(
                Instruction.Create(OpCodes.Call, tickCountGetter),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldstr, label + " tick="),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " type="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, objectGetType),
                Instruction.Create(OpCodes.Callvirt, typeGetName),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " wall="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Call, dateTimeNowGetter),
                Instruction.Create(OpCodes.Stloc, tmpDate),
                Instruction.Create(OpCodes.Ldloca, tmpDate),
                Instruction.Create(OpCodes.Ldstr, "HH:mm:ss.fff"),
                Instruction.Create(OpCodes.Call, dateTimeToStringFmt),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " bgWasNull="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, backgroundBitmapField),
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Ceq),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Stloc, tmpMsg),
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldloc, tmpMsg),
                Instruction.Create(OpCodes.Call, appendAllText)
            );

            // "<label> headerEmpty=<0/1> w=<W> h=<H>\n"
            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldflda, headerRectField),
                Instruction.Create(OpCodes.Call, rectangleIsEmptyGetter),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldstr, label + " headerEmpty="),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " w="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getWidth),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, " h="),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getHeight),
                Instruction.Create(OpCodes.Stloc, tmpInt),
                Instruction.Create(OpCodes.Ldloca, tmpInt),
                Instruction.Create(OpCodes.Call, int32ToString),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Ldstr, "\n"),
                Instruction.Create(OpCodes.Call, stringConcat2),
                Instruction.Create(OpCodes.Stloc, tmpMsg),
                Instruction.Create(OpCodes.Ldstr, logPath),
                Instruction.Create(OpCodes.Ldloc, tmpMsg),
                Instruction.Create(OpCodes.Call, appendAllText)
            );
        }

        Instrument(onPaintMethod, "OnPaint");
        Instrument(timerMethod, "timer_OnTimer");
        Instrument(hideMethod, "Hide");
        Instrument(onMouseClickMethod, "OnMouseClick");
        Instrument(onShownMethod, "OnShown");
        Instrument(layeredWindowClickMethod, "layeredWindow_Click");
        InstrumentMinimal(clickHandlerMethod, "notificationForm_Click");
        InstrumentBitmapDiag(updateBackgroundBitmapMethod, "updateBackgroundBitmap");

        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::OnPaint -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::timer_OnTimer -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::Hide -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::OnMouseClick -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::OnShown -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::layeredWindow_Click -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {handlerTypeName}::notificationForm_Click -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::updateBackgroundBitmap -> {logPath}");
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

// --patch-auto-test-notification <input-dir> <output-dir> [delayMs]
//
// Test-infrastructure patch, not a fix attempt -- layers on top of any other patched output
// directory (same pattern as --patch-diag), so it composes with whichever keep-alive/fix variant
// is currently being tested. Removes the need for the user to manually send a real test email for
// every single iteration: inserts code at the top of formMain.OnShown (the main window's own
// OnShown, fires once when the app's main window first displays) that starts a one-shot
// System.Windows.Forms.Timer; once it fires (default 3000ms after the main window shows, giving
// the app time to finish settling), it constructs a real `new FormMailNotification()` with fixed
// test Title/Content strings and calls the plain inherited `Show()` -- which still fires
// FormGenericNotification's own OnShown() override (Show() firing OnShown is standard WinForms
// behavior for any Form, not something FormGenericNotification's Show(IWin32Window) override-by-
// hiding is required for), exercising the exact same code path a real incoming-mail notification
// would, with no dependency on real mail arriving via IMAP/SMTP at all. Image/avatar is left null
// (a valid real-world case -- no avatar available) since this test is about the empty-box/fade
// mechanism, not avatar rendering specifically.
static int RunPatchAutoTestNotification(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-auto-test-notification <input-dir> <output-dir> [delayMs]");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    int delayMs = args.Length > 3 && int.TryParse(args[3], out var d) ? d : 3000;

    const string targetAssembly = "MailClient.dll";
    const string mainFormType = "MailClient.UI.Forms.formMain";
    const string notifFormType = "MailClient.UI.Forms.NotificationForms.FormMailNotification";

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

        var mainType = module.GetType(mainFormType);
        if (mainType is null) { Console.Error.WriteLine($"FAIL: type not found: {mainFormType}"); return 1; }
        var onShownMethod = mainType.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody && m.Parameters.Count == 1);
        if (onShownMethod is null) { Console.Error.WriteLine($"FAIL: OnShown not found on {mainFormType}"); return 1; }

        var notifType = module.GetType(notifFormType);
        if (notifType is null) { Console.Error.WriteLine($"FAIL: type not found: {notifFormType}"); return 1; }
        var notifCtorDef = notifType.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        if (notifCtorDef is null) { Console.Error.WriteLine($"FAIL: {notifFormType} missing parameterless constructor"); return 1; }
        var notifCtorRef = module.ImportReference(notifCtorDef);

        var genericNotifType = notifType.BaseType?.Resolve();
        if (genericNotifType is null || genericNotifType.FullName != "MailClient.UI.Forms.NotificationForms.FormGenericNotification")
        {
            Console.Error.WriteLine($"FAIL: expected {notifFormType}'s base type to be FormGenericNotification, got {genericNotifType?.FullName}");
            return 1;
        }
        // Set Title/Content via ShowNotification(Notification), not the direct property setters --
        // a full decompile pass of the notification pipeline found the real trigger path
        // (FormNotificationPresenter.ShowNotification) calls Show() FIRST, then
        // FormGenericNotification.ShowNotification(notification), which is what actually sets
        // Title/Content/Image (via virtual dispatch into FormMailNotification's own
        // OnDisplayedNotificationChanged override). The original version of this trigger set Title/
        // Content directly and called Show() last -- the opposite order, and bypassing
        // ShowNotification() (and therefore --patch-notification-refresh-on-content-change's fix,
        // which lives inside it) entirely. Route through the real method instead, using the
        // NewMailsCount branch of OnDisplayedNotificationChanged (simpler to construct than a full
        // fake IMail): a plain `new Notification(new NewMailsCount { Value = 3 }, null)`.
        var notificationType = module.GetType("MailClient.UI.Notifications.Notification");
        if (notificationType is null) { Console.Error.WriteLine("FAIL: type not found: MailClient.UI.Notifications.Notification"); return 1; }
        var notificationCtorDef = notificationType.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        if (notificationCtorDef is null) { Console.Error.WriteLine("FAIL: Notification missing (object, IAccount) constructor"); return 1; }
        var notificationCtorRef = module.ImportReference(notificationCtorDef);

        var newMailsCountType = module.GetType("MailClient.UI.Notifications.NewMailsCount");
        if (newMailsCountType is null) { Console.Error.WriteLine("FAIL: type not found: MailClient.UI.Notifications.NewMailsCount"); return 1; }
        var newMailsCountCtorDef = newMailsCountType.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        var newMailsCountSetValueDef = newMailsCountType.Methods.FirstOrDefault(m => m.Name == "set_Value");
        if (newMailsCountCtorDef is null || newMailsCountSetValueDef is null) { Console.Error.WriteLine("FAIL: NewMailsCount missing .ctor()/set_Value"); return 1; }
        var newMailsCountCtorRef = module.ImportReference(newMailsCountCtorDef);
        var newMailsCountSetValueRef = module.ImportReference(newMailsCountSetValueDef);

        var showNotificationDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "ShowNotification" && m.HasBody && m.Parameters.Count == 1);
        if (showNotificationDef is null) { Console.Error.WriteLine("FAIL: FormGenericNotification missing ShowNotification(Notification)"); return 1; }
        var showNotificationRef = module.ImportReference(showNotificationDef);

        // First attempt used the plain inherited Control.Show() -- decompiled/verified clean and
        // ran the right OnShown() code path, but live-tested: the form rendered at the default
        // (0,0) position, mostly hidden behind the main window (barely visible top-left sliver).
        // Real notifications get positioned explicitly by FormNotificationPresenter (never via
        // Control's own defaults) and shown through FormGenericNotification's own
        // Show(IWin32Window) override, which does the actual topmost SetWindowPos/ShowWindow
        // calls -- neither of which the plain Show() path exercises. Use that override instead
        // (owner: null is valid), and set Location explicitly first.
        var notifShowDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "Show" && m.Parameters.Count == 1);
        if (notifShowDef is null) { Console.Error.WriteLine("FAIL: FormGenericNotification missing Show(IWin32Window)"); return 1; }
        var notifShowRef = module.ImportReference(notifShowDef);

        TypeDefinition? formType = genericNotifType;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Control")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var setLocationDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Location");
        if (setLocationDef is null) { Console.Error.WriteLine("FAIL: Control missing set_Location"); return 1; }
        var setLocationRef = module.ImportReference(setLocationDef);

        // Point is an app-deployed type (System.Drawing.Primitives) -- resolve it from
        // set_Location's own parameter type (already correctly versioned within this module),
        // not via typeof() reflection (CLAUDE.md's IL-patching lesson 5).
        var pointTypeRef = setLocationDef.Parameters[0].ParameterType;
        var pointTypeDef = pointTypeRef.Resolve();
        if (pointTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Point from set_Location's parameter type"); return 1; }
        var pointCtorDef = pointTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        if (pointCtorDef is null) { Console.Error.WriteLine("FAIL: Point missing (int, int) constructor"); return 1; }
        var pointCtorRef = module.ImportReference(pointCtorDef);

        // Timer lives in the same module as Form/Control -- look it up directly rather than via
        // typeof() reflection (CLAUDE.md's IL-patching lesson 5: System.Windows.Forms is an
        // app-deployed assembly, not a CoreLib-forwarded one).
        var timerTypeDef = formType.Module.GetType("System.Windows.Forms.Timer");
        if (timerTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Timer"); return 1; }
        var timerCtorDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        var timerSetIntervalDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var timerAddTickDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "add_Tick");
        var timerStartDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        var timerStopDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "Stop" && m.Parameters.Count == 0);
        if (timerCtorDef is null || timerSetIntervalDef is null || timerAddTickDef is null || timerStartDef is null || timerStopDef is null)
        {
            Console.Error.WriteLine("FAIL: Timer missing one of .ctor()/set_Interval/add_Tick/Start()/Stop()");
            return 1;
        }
        var timerCtorRef = module.ImportReference(timerCtorDef);
        var timerSetIntervalRef = module.ImportReference(timerSetIntervalDef);
        var timerAddTickRef = module.ImportReference(timerAddTickDef);
        var timerStartRef = module.ImportReference(timerStartDef);
        var timerStopRef = module.ImportReference(timerStopDef);
        var timerTypeRef = module.ImportReference(timerTypeDef);

        // EventHandler(object, IntPtr) ctor -- same CoreLib-forwarded type already used safely
        // via typeof() elsewhere in this file for Tick/Click delegate construction.
        var eventHandlerCtorRef = module.ImportReference(typeof(EventHandler).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);

        // --- Step 1: add `private Timer __autoTestTimer;` field on formMain, and
        // `private void __autoTestNotificationTick(object sender, EventArgs e)` that stops the
        // timer, constructs the test notification, and shows it.
        var timerField = new FieldDefinition("__autoTestTimer", FieldAttributes.Private, timerTypeRef);
        mainType.Fields.Add(timerField);

        var tickMethod = new MethodDefinition("__autoTestNotificationTick", MethodAttributes.Private, module.TypeSystem.Void);
        tickMethod.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
        tickMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, module.ImportReference(typeof(EventArgs))));
        mainType.Methods.Add(tickMethod);
        var tmBody = tickMethod.Body;
        tmBody.InitLocals = true;
        var notifLocal = new VariableDefinition(module.ImportReference(notifType));
        var nmcLocal = new VariableDefinition(module.ImportReference(newMailsCountType));
        var notificationLocal = new VariableDefinition(module.ImportReference(notificationType));
        tmBody.Variables.Add(notifLocal);
        tmBody.Variables.Add(nmcLocal);
        tmBody.Variables.Add(notificationLocal);
        var tmIl = tmBody.GetILProcessor();

        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, timerStopRef));
        tmIl.Append(Instruction.Create(OpCodes.Newobj, notifCtorRef));
        tmIl.Append(Instruction.Create(OpCodes.Stloc, notifLocal));
        // Location = new Point(100, 100) -- deliberately top-LEFT, not the real bottom-right spot
        // real notifications use. Moved here after discovering the bottom-right corner of this
        // 1680x1188 test screen also happens to be where the terminal's own inline image-preview
        // widget renders (it displays whatever screenshot was last viewed via the Read tool) --
        // several rounds of recording analysis were contaminated by reading that widget's content
        // instead of the actual notification, since both occupy the same corner. Top-left is clear
        // of both the terminal panel and that widget. Real notifications get their real position
        // from FormNotificationPresenter, which this synthetic trigger bypasses entirely, so
        // Location must be set explicitly here regardless of which corner is chosen.
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 100));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 100));
        tmIl.Append(Instruction.Create(OpCodes.Newobj, pointCtorRef));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, setLocationRef));
        // Show(null) FIRST, while still blank -- matches the real trigger order found by the
        // decompile pass (FormNotificationPresenter calls Show() before ShowNotification()).
        // FormGenericNotification's own override (topmost SetWindowPos/ShowWindow), not the plain
        // inherited Control.Show() (no owner needed, null is valid).
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldnull));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, notifShowRef));
        // THEN ShowNotification(new Notification(new NewMailsCount { Value = 3 }, null)) -- sets
        // Title/Content/Image via the real virtual OnDisplayedNotificationChanged dispatch, and
        // (once --patch-notification-refresh-on-content-change is layered on top of this) exercises
        // the actual fixed code path, not a synthetic shortcut around it.
        tmIl.Append(Instruction.Create(OpCodes.Newobj, newMailsCountCtorRef));
        tmIl.Append(Instruction.Create(OpCodes.Stloc, nmcLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, nmcLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4_3));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, newMailsCountSetValueRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, nmcLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldnull));
        tmIl.Append(Instruction.Create(OpCodes.Newobj, notificationCtorRef));
        tmIl.Append(Instruction.Create(OpCodes.Stloc, notificationLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, notificationLocal));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, showNotificationRef));
        tmIl.Append(Instruction.Create(OpCodes.Ret));

        // --- Step 2: insert at the very top of formMain.OnShown (same safe insertion point used
        // by --patch-diag throughout this investigation -- no branch can target a method's own
        // first instruction, and there's no exception-handler region to accidentally grow here):
        //   __autoTestTimer = new Timer();
        //   __autoTestTimer.Interval = delayMs;
        //   __autoTestTimer.Tick += __autoTestNotificationTick;
        //   __autoTestTimer.Start();
        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Newobj, timerCtorRef),
                Instruction.Create(OpCodes.Stfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, delayMs),
                Instruction.Create(OpCodes.Callvirt, timerSetIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, tickMethod),
                Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, timerAddTickRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Callvirt, timerStartRef)
            );
        }

        Console.WriteLine($"OK   {fileName}: {mainFormType}::OnShown -- starts a one-shot {delayMs}ms timer that constructs and shows a test {notifFormType} (fixed Title/Content, no dependency on real mail arriving)");
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

// --patch-close-listener <input-dir> <output-dir>
//
// Dev/testing-only tool, not a real fix: lets the session controlling the Linux side trigger a
// graceful eM Client exit itself, without needing to ask the user to close it by hand every test
// iteration. Never kill/terminate MailClient.exe directly (an unclean termination triggers a
// DB-repair dialog on next launch -- confirmed the hard way earlier in this project) -- this
// instead has the app watch for a marker file and, when it appears, calls the app's own REAL
// File > Exit menu handler (`menuItem_File_Exit_Click` -- found by decompiling formMain and
// confirming it's what the actual menu item calls: sets `closingFromFileExitMenu = true;` then
// `Close();`), so the shutdown is indistinguishable from a user actually choosing File > Exit.
// Same safe top-of-OnShown insertion pattern as --patch-auto-test-notification and --patch-diag.
//
// Signal file path: Z:\tmp\claude-close-signal (Z:\ maps to the Linux host's own / -- see
// CLAUDE.md -- so this is literally /tmp/claude-close-signal from the Linux side). Create that
// file (`touch /tmp/claude-close-signal` or equivalent) to request a graceful close; the app
// deletes it and exits within one polling interval (500ms).
static int RunPatchCloseListener(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-close-listener <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    const int pollIntervalMs = 500;
    const string signalPath = @"Z:\tmp\claude-close-signal";

    const string targetAssembly = "MailClient.dll";
    const string mainFormType = "MailClient.UI.Forms.formMain";

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

        var mainType = module.GetType(mainFormType);
        if (mainType is null) { Console.Error.WriteLine($"FAIL: type not found: {mainFormType}"); return 1; }
        var onShownMethod = mainType.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody && m.Parameters.Count == 1);
        if (onShownMethod is null) { Console.Error.WriteLine($"FAIL: OnShown not found on {mainFormType}"); return 1; }
        var exitHandlerMethod = mainType.Methods.FirstOrDefault(m => m.Name == "menuItem_File_Exit_Click" && m.HasBody);
        if (exitHandlerMethod is null) { Console.Error.WriteLine($"FAIL: menuItem_File_Exit_Click not found on {mainFormType}"); return 1; }

        TypeDefinition? formType = mainType;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Control")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var timerTypeDef = formType.Module.GetType("System.Windows.Forms.Timer");
        if (timerTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Timer"); return 1; }
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
        var timerTypeRef = module.ImportReference(timerTypeDef);

        // File.Exists/File.Delete and EventArgs.Empty are all CoreLib (System.Private.CoreLib,
        // forwarded via System.Runtime) -- safe to resolve via typeof() reflection, unlike
        // app-deployed assemblies (IL-patching lesson 5 in CLAUDE.md).
        var fileExistsRef = module.ImportReference(typeof(File).GetMethod("Exists", new[] { typeof(string) })!);
        var fileDeleteRef = module.ImportReference(typeof(File).GetMethod("Delete", new[] { typeof(string) })!);
        var eventArgsEmptyRef = module.ImportReference(typeof(EventArgs).GetField("Empty")!);
        var eventHandlerCtorRef = module.ImportReference(typeof(EventHandler).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);

        var timerField = new FieldDefinition("__closeListenerTimer", FieldAttributes.Private, timerTypeRef);
        mainType.Fields.Add(timerField);

        // --- __closeListenerTick(object, EventArgs):
        //   if (File.Exists(signalPath)) { File.Delete(signalPath); menuItem_File_Exit_Click(null, EventArgs.Empty); }
        var tickMethod = new MethodDefinition("__closeListenerTick", MethodAttributes.Private, module.TypeSystem.Void);
        tickMethod.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
        tickMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, module.ImportReference(typeof(EventArgs))));
        mainType.Methods.Add(tickMethod);
        var tmIl = tickMethod.Body.GetILProcessor();
        var tmRet = Instruction.Create(OpCodes.Ret);
        tmIl.Append(Instruction.Create(OpCodes.Ldstr, signalPath));
        tmIl.Append(Instruction.Create(OpCodes.Call, fileExistsRef));
        tmIl.Append(Instruction.Create(OpCodes.Brfalse, tmRet));
        tmIl.Append(Instruction.Create(OpCodes.Ldstr, signalPath));
        tmIl.Append(Instruction.Create(OpCodes.Call, fileDeleteRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldnull));
        tmIl.Append(Instruction.Create(OpCodes.Ldsfld, eventArgsEmptyRef));
        tmIl.Append(Instruction.Create(OpCodes.Call, module.ImportReference(exitHandlerMethod)));
        tmIl.Append(tmRet);

        // --- Prepend to OnShown: __closeListenerTimer = new Timer(); ...Interval = pollIntervalMs;
        // ...Tick += __closeListenerTick; ...Start(); -- same safe top-of-method insertion as
        // --patch-diag/--patch-auto-test-notification (a method's own first instruction is never a
        // branch target, and there's no exception-handler region here to accidentally grow).
        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Newobj, timerCtorRef),
                Instruction.Create(OpCodes.Stfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, pollIntervalMs),
                Instruction.Create(OpCodes.Callvirt, timerSetIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, tickMethod),
                Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, timerAddTickRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Callvirt, timerStartRef)
            );
        }

        Console.WriteLine($"OK   {fileName}: {mainFormType}::OnShown -- now polls every {pollIntervalMs}ms for {signalPath}; when present, deletes it and calls the real menuItem_File_Exit_Click (same as File > Exit) for a graceful shutdown");
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

// --patch-notification-layout-diag <input-dir> <output-dir>
//
// One-off diagnostic for the cosmetic padding/font follow-up to the empty-box fix (see
// reports/notification-empty-until-fade-findings.md's eighteenth round): a full decompile pass
// found that FormGenericNotification DOES have a real mechanism that should set
// `defaultPadding` to (8,6,8,6) -- a `public new Padding Padding { get => defaultPadding; set {
// defaultPadding = value; base.Padding = getScaledPadding(); } }` property, fed by
// `ApplyResources(this, "$this")` in InitializeComponent from an embedded resx value -- and a
// separate mechanism (`OnLoad()`) that should set `headerFont`/`Font` from `FontManager.UIFont`
// at 11pt/10pt. Both *should* work; whether they actually do under Wine is unconfirmed without
// reading the live runtime values back -- exactly the kind of question this project's own
// established investigation method (CLAUDE.md) says to answer via direct instrumentation rather
// than more guessing, especially given this project has a confirmed precedent
// (reports/settings-panel-clip-region-findings.md) of a WinForms lifecycle method
// (formSettings's Load event) simply never firing under Wine at all.
//
// Logs, once per doLayout() call (a frequently-called, already-safe top-of-method insertion
// point used elsewhere in this file): defaultPadding.Left/Top, whether headerFont/Font are null,
// and CornerRadius's value.
static int RunPatchNotificationLayoutDiag(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-layout-diag <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    const string logPath = @"Z:\tmp\claude-diag.log";

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
        var defaultPaddingField = type.Fields.FirstOrDefault(f => f.Name == "defaultPadding");
        if (defaultPaddingField is null) { Console.Error.WriteLine("FAIL: defaultPadding field not found"); return 1; }
        var headerFontField = type.Fields.FirstOrDefault(f => f.Name == "headerFont");
        if (headerFontField is null) { Console.Error.WriteLine("FAIL: headerFont field not found"); return 1; }
        var cornerRadiusGetterDef = type.Methods.FirstOrDefault(m => m.Name == "get_CornerRadius");
        if (cornerRadiusGetterDef is null) { Console.Error.WriteLine("FAIL: get_CornerRadius not found"); return 1; }
        var cornerRadiusGetterRef = module.ImportReference(cornerRadiusGetterDef);
        var getScaledPaddingDef = type.Methods.FirstOrDefault(m => m.Name == "getScaledPadding");
        if (getScaledPaddingDef is null) { Console.Error.WriteLine("FAIL: getScaledPadding not found"); return 1; }
        var getScaledPaddingRef = module.ImportReference(getScaledPaddingDef);

        var paddingTypeDef = defaultPaddingField.FieldType.Resolve();
        if (paddingTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Padding from defaultPadding's own FieldType"); return 1; }
        var paddingGetLeftDef = paddingTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Left");
        var paddingGetTopDef = paddingTypeDef.Methods.FirstOrDefault(m => m.Name == "get_Top");
        if (paddingGetLeftDef is null || paddingGetTopDef is null) { Console.Error.WriteLine("FAIL: Padding missing get_Left/get_Top"); return 1; }
        var paddingGetLeftRef = module.ImportReference(paddingGetLeftDef);
        var paddingGetTopRef = module.ImportReference(paddingGetTopDef);

        TypeDefinition? controlType = type;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var getFontDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Font");
        if (getFontDef is null) { Console.Error.WriteLine("FAIL: Control missing get_Font"); return 1; }
        var getFontRef = module.ImportReference(getFontDef);

        MethodReference Import(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var int32ToString = Import(typeof(int).GetMethod("ToString", Type.EmptyTypes)!);
        var stringConcat2 = Import(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        var appendAllText = Import(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);

        var body = doLayoutMethod.Body;
        body.InitLocals = true;
        var tmpInt = new VariableDefinition(module.TypeSystem.Int32);
        var tmpMsg = new VariableDefinition(module.TypeSystem.String);
        var tmpScaledPadding = new VariableDefinition(module.ImportReference(defaultPaddingField.FieldType));
        body.Variables.Add(tmpInt);
        body.Variables.Add(tmpMsg);
        body.Variables.Add(tmpScaledPadding);
        var il = body.GetILProcessor();
        var first = body.Instructions[0];
        void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

        // Each null-check below follows the same shape: push the field, Brtrue to the
        // "not null" string literal (skipping the "is null" literal + an unconditional Br past
        // it), falling through to the "is null" literal otherwise -- both paths converge on the
        // shared `merge` instruction (the next real instruction, a Call to stringConcat2), which
        // is captured once and only ever appears once in the emitted list (per IL-patching lesson
        // 1: an Instruction object can only occupy one position in a method body -- reusing one
        // as both a branch target AND a second literal list entry, as an earlier draft of this
        // function mistakenly did, corrupts the instruction list).
        var headerFontIsNull = Instruction.Create(OpCodes.Ldstr, "headerFontNull=1 ");
        var headerFontNotNull = Instruction.Create(OpCodes.Ldstr, "headerFontNull=0 ");
        var mergeHeaderFont = Instruction.Create(OpCodes.Call, stringConcat2);
        var fontIsNull = Instruction.Create(OpCodes.Ldstr, "fontNull=1 ");
        var fontNotNull = Instruction.Create(OpCodes.Ldstr, "fontNull=0 ");
        var mergeFont = Instruction.Create(OpCodes.Call, stringConcat2);

        // "LAYOUT paddingL=<n> paddingT=<n> headerFontNull=<0|1> fontNull=<0|1> cornerRadius=<n>\n"
        Emit(
            Instruction.Create(OpCodes.Ldstr, "LAYOUT paddingL="),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldflda, defaultPaddingField),
            Instruction.Create(OpCodes.Call, paddingGetLeftRef),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldloca, tmpInt),
            Instruction.Create(OpCodes.Call, int32ToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, " paddingT="),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldflda, defaultPaddingField),
            Instruction.Create(OpCodes.Call, paddingGetTopRef),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldloca, tmpInt),
            Instruction.Create(OpCodes.Call, int32ToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, " scaledL="),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, getScaledPaddingRef),
            Instruction.Create(OpCodes.Stloc, tmpScaledPadding),
            Instruction.Create(OpCodes.Ldloca, tmpScaledPadding),
            Instruction.Create(OpCodes.Call, paddingGetLeftRef),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldloca, tmpInt),
            Instruction.Create(OpCodes.Call, int32ToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, " "),
            Instruction.Create(OpCodes.Call, stringConcat2),
            // headerFont == null ?
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, headerFontField),
            Instruction.Create(OpCodes.Brtrue, headerFontNotNull),
            headerFontIsNull,
            Instruction.Create(OpCodes.Br, mergeHeaderFont),
            headerFontNotNull,
            mergeHeaderFont,
            Instruction.Create(OpCodes.Ldstr, " "),
            Instruction.Create(OpCodes.Call, stringConcat2),
            // Font == null ?
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, getFontRef),
            Instruction.Create(OpCodes.Brtrue, fontNotNull),
            fontIsNull,
            Instruction.Create(OpCodes.Br, mergeFont),
            fontNotNull,
            mergeFont,
            Instruction.Create(OpCodes.Ldstr, " cornerRadius="),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, cornerRadiusGetterRef),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldloca, tmpInt),
            Instruction.Create(OpCodes.Call, int32ToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, tmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, tmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );

        Console.WriteLine($"OK   {fileName}: {targetType}::doLayout -- now logs defaultPadding.Left/Top, headerFont/Font null-ness, and CornerRadius to {logPath}");
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

// --patch-test-monogram-avatar <input-dir> <output-dir> [pollIntervalMs]
//
// Dev/testing-only tool (not a real fix, not for release builds), requested to verify the "no
// avatar image" case: when a sender has no photo, `AvatarHelper.GetAvatarWithFallback` falls back
// to `UIAvatar.FromMonogram(monogram, hashString)` -- a colored-circle-plus-initials avatar,
// confirmed by decompile to be generated via plain GDI+ vector drawing (`Graphics.FillEllipse` +
// `Graphics.DrawString`, no `InterpolationMode` involved at all), then fed into the exact same
// `Image` property and `updateBackgroundBitmap()` rendering path as any other avatar. Cannot be
// exercised via the real-mail test path (the test account's contacts all have photos) or via
// --patch-auto-test-notification's synthetic NewMailsCount path (which always uses the app icon,
// never a monogram) -- this patch exercises `UIAvatar.FromMonogram` directly instead, independent
// of both.
//
// Originally fired from a fixed one-shot delay timer (guess how long startup takes, then race a
// screenshot/recording against it) -- lost that race for real: a delay-timed capture attempt found
// the notification window already `IsUnMapped` (auto-hidden) by the time it ran. Switched to the
// same file-trigger polling idiom as --patch-close-listener instead: start the capture (recording
// or screenshot) FIRST, with nothing time-pressured about it, then `touch
// /tmp/claude-trigger-notification` once ready -- the notification fires on this side's own
// schedule instead of a guessed delay, so there's no capture race to lose.
//
// Adds a new PUBLIC method `__showTestMonogramNotification(string, string, string, string)` to
// FormGenericNotification (public, not protected, specifically so formMain -- a different class,
// not a subclass -- can call it directly without needing to route through Notification/
// ShowNotification's virtual-dispatch machinery the way --patch-auto-test-notification does), and
// a new one-shot timer in formMain.OnShown (same safe top-of-method insertion pattern used
// throughout this file) that constructs a FormMailNotification, positions and shows it, then
// calls that new method with fixed test values matching supporting/email-with-no-image.png
// ("Jaguar Workshop" / "JW"). Standalone -- not combined with --patch-auto-test-notification's own
// timer in this version, to keep this one-off verification simple.
static int RunPatchTestMonogramAvatar(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-test-monogram-avatar <input-dir> <output-dir> [pollIntervalMs]");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    // A fixed delay meant guessing how long app startup takes, then racing a screen
    // recording/screenshot against it -- lost that race for real (see this project's own
    // findings: notification was already IsUnMapped by the time a delay-timed capture ran).
    // File-trigger instead, same polling idiom as --patch-close-listener: start the capture
    // FIRST, then touch the trigger file once ready, so the notification fires at a moment this
    // side controls exactly instead of guessing.
    int pollIntervalMs = args.Length > 3 && int.TryParse(args[3], out var d) ? d : 150;
    const string triggerPath = @"Z:\tmp\claude-trigger-notification";

    const string targetAssembly = "MailClient.dll";
    const string mainFormType = "MailClient.UI.Forms.formMain";
    const string notifFormType = "MailClient.UI.Forms.NotificationForms.FormMailNotification";

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

        var mainType = module.GetType(mainFormType);
        if (mainType is null) { Console.Error.WriteLine($"FAIL: type not found: {mainFormType}"); return 1; }
        var onShownMethod = mainType.Methods.FirstOrDefault(m => m.Name == "OnShown" && m.HasBody && m.Parameters.Count == 1);
        if (onShownMethod is null) { Console.Error.WriteLine($"FAIL: OnShown not found on {mainFormType}"); return 1; }

        var notifType = module.GetType(notifFormType);
        if (notifType is null) { Console.Error.WriteLine($"FAIL: type not found: {notifFormType}"); return 1; }
        var notifCtorDef = notifType.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        if (notifCtorDef is null) { Console.Error.WriteLine($"FAIL: {notifFormType} missing parameterless constructor"); return 1; }
        var notifCtorRef = module.ImportReference(notifCtorDef);

        var genericNotifType = notifType.BaseType?.Resolve();
        if (genericNotifType is null || genericNotifType.FullName != "MailClient.UI.Forms.NotificationForms.FormGenericNotification")
        {
            Console.Error.WriteLine($"FAIL: expected {notifFormType}'s base type to be FormGenericNotification, got {genericNotifType?.FullName}");
            return 1;
        }

        var setTitleDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "set_Title");
        var setContentDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "set_Content");
        var setImageDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "set_Image");
        if (setTitleDef is null || setContentDef is null || setImageDef is null) { Console.Error.WriteLine("FAIL: FormGenericNotification missing set_Title/set_Content/set_Image"); return 1; }
        var setTitleRef = module.ImportReference(setTitleDef);
        var setContentRef = module.ImportReference(setContentDef);
        var setImageRef = module.ImportReference(setImageDef);

        var updateLayeredBackgroundMethod = genericNotifType.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground" && m.HasBody) ??
            genericNotifType.BaseType?.Resolve()?.Methods.FirstOrDefault(m => m.Name == "updateLayeredBackground");
        if (updateLayeredBackgroundMethod is null) { Console.Error.WriteLine("FAIL: updateLayeredBackground(bool) not found"); return 1; }
        var updateLayeredBackgroundRef = module.ImportReference(updateLayeredBackgroundMethod);

        var notifShowDef = genericNotifType.Methods.FirstOrDefault(m => m.Name == "Show" && m.Parameters.Count == 1);
        if (notifShowDef is null) { Console.Error.WriteLine("FAIL: FormGenericNotification missing Show(IWin32Window)"); return 1; }
        var notifShowRef = module.ImportReference(notifShowDef);

        // timeToHide/timeToShow (protected int fields, default 500ms) directly control fade speed
        // via alphaIncrement = +/-25f / timeToHide|timeToShow (confirmed by decompile of
        // timer_OnTimer/Hide). The still-open empty-box-until-fade bug only reveals content during
        // the brief Appearing/Disappearing tick bursts -- a real capture (screen recording or
        // screenshot) has to land in that narrow, fast-fading window, which produces exactly the
        // partial-opacity blending that makes precise padding/line-height pixel measurement
        // unreliable. Slowing both down 10x for this dev-only test method (not touching the real
        // notification path at all) turns that narrow window into several seconds of near-full
        // opacity, giving a far more trustworthy capture for that kind of comparison.
        var timeToHideField = genericNotifType.Fields.FirstOrDefault(f => f.Name == "timeToHide");
        var timeToShowField = genericNotifType.Fields.FirstOrDefault(f => f.Name == "timeToShow");
        if (timeToHideField is null || timeToShowField is null) { Console.Error.WriteLine("FAIL: timeToHide/timeToShow field(s) not found"); return 1; }

        TypeDefinition? formType = genericNotifType;
        while (formType is not null && formType.FullName != "System.Windows.Forms.Control")
        {
            formType = formType.BaseType?.Resolve();
        }
        if (formType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var setLocationDef = formType.Methods.FirstOrDefault(m => m.Name == "set_Location");
        var performLayoutDef = formType.Methods.FirstOrDefault(m => m.Name == "PerformLayout" && m.Parameters.Count == 0);
        var invalidateDef = formType.Methods.FirstOrDefault(m => m.Name == "Invalidate" && m.Parameters.Count == 0);
        if (setLocationDef is null || performLayoutDef is null || invalidateDef is null) { Console.Error.WriteLine("FAIL: Control missing set_Location/PerformLayout()/Invalidate()"); return 1; }
        var setLocationRef = module.ImportReference(setLocationDef);
        var performLayoutRef = module.ImportReference(performLayoutDef);
        var invalidateRef = module.ImportReference(invalidateDef);

        var pointTypeRef = setLocationDef.Parameters[0].ParameterType;
        var pointTypeDef = pointTypeRef.Resolve();
        if (pointTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Point from set_Location's parameter type"); return 1; }
        var pointCtorDef = pointTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        if (pointCtorDef is null) { Console.Error.WriteLine("FAIL: Point missing (int, int) constructor"); return 1; }
        var pointCtorRef = module.ImportReference(pointCtorDef);

        const string avatarTypeName = "MailClient.UI.UIAvatar";
        var avatarType = module.GetType(avatarTypeName);
        if (avatarType is null) { Console.Error.WriteLine($"FAIL: type not found: {avatarTypeName}"); return 1; }
        var fromMonogramDef = avatarType.Methods.FirstOrDefault(m => m.Name == "FromMonogram" && m.Parameters.Count == 2);
        if (fromMonogramDef is null) { Console.Error.WriteLine("FAIL: UIAvatar.FromMonogram(string,string) not found"); return 1; }
        var fromMonogramRef = module.ImportReference(fromMonogramDef);
        var getRoundImageDef = avatarType.Methods.FirstOrDefault(m => m.Name == "GetRoundImage");
        if (getRoundImageDef is null) { Console.Error.WriteLine("FAIL: UIAvatar.GetRoundImage(Size) not found"); return 1; }
        var getRoundImageRef = module.ImportReference(getRoundImageDef);
        var avatarDisposeDef = avatarType.Methods.FirstOrDefault(m => m.Name == "Dispose" && m.Parameters.Count == 0);
        if (avatarDisposeDef is null) { Console.Error.WriteLine("FAIL: UIAvatar.Dispose() not found"); return 1; }
        var avatarDisposeRef = module.ImportReference(avatarDisposeDef);

        // Size -- resolve from GetRoundImage's own parameter type, not typeof() reflection
        // (System.Drawing.Primitives is app-deployed -- IL-patching lesson 5).
        var sizeTypeRef = getRoundImageDef.Parameters[0].ParameterType;
        var sizeTypeDef = sizeTypeRef.Resolve();
        if (sizeTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Size from GetRoundImage's own parameter type"); return 1; }
        var sizeCtorDef = sizeTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 2);
        if (sizeCtorDef is null) { Console.Error.WriteLine("FAIL: Size missing (int, int) constructor"); return 1; }
        var sizeCtorRef = module.ImportReference(sizeCtorDef);

        // Timer -- resolve from formType's own module, same pattern as --patch-close-listener.
        var timerTypeDef = formType.Module.GetType("System.Windows.Forms.Timer");
        if (timerTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Timer"); return 1; }
        var timerCtorDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
        var timerSetIntervalDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "set_Interval");
        var timerAddTickDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "add_Tick");
        var timerStartDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "Start" && m.Parameters.Count == 0);
        var timerStopDef = timerTypeDef.Methods.FirstOrDefault(m => m.Name == "Stop" && m.Parameters.Count == 0);
        if (timerCtorDef is null || timerSetIntervalDef is null || timerAddTickDef is null || timerStartDef is null || timerStopDef is null)
        {
            Console.Error.WriteLine("FAIL: Timer missing one of .ctor()/set_Interval/add_Tick/Start()/Stop()");
            return 1;
        }
        var timerCtorRef = module.ImportReference(timerCtorDef);
        var timerSetIntervalRef = module.ImportReference(timerSetIntervalDef);
        var timerAddTickRef = module.ImportReference(timerAddTickDef);
        var timerStartRef = module.ImportReference(timerStartDef);
        var timerStopRef = module.ImportReference(timerStopDef);
        var timerTypeRef = module.ImportReference(timerTypeDef);
        var eventHandlerCtorRef = module.ImportReference(typeof(EventHandler).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);
        // File.Exists/Delete are CoreLib -- safe via typeof() reflection (IL-patching lesson 5
        // only bites app-deployed assemblies like System.Drawing.Primitives, not CoreLib/its
        // forwarding facades), same as --patch-close-listener's own use of these two methods.
        var fileExistsRef = module.ImportReference(typeof(File).GetMethod("Exists", new[] { typeof(string) })!);
        var fileDeleteRef = module.ImportReference(typeof(File).GetMethod("Delete", new[] { typeof(string) })!);

        // --- New public method on FormGenericNotification:
        //   public void __showTestMonogramNotification(string title, string content, string monogram, string hashSeed)
        //   {
        //       Title = title; Content = content;
        //       UIAvatar avatar = UIAvatar.FromMonogram(monogram, hashSeed);
        //       Image = avatar.GetRoundImage(new Size(32, 32));
        //       avatar.Dispose();
        //       PerformLayout();
        //       updateLayeredBackground(refreshBitmap: true);
        //       Invalidate();
        //   }
        var testMethod = new MethodDefinition("__showTestMonogramNotification", MethodAttributes.Public, module.TypeSystem.Void);
        testMethod.Parameters.Add(new ParameterDefinition("title", ParameterAttributes.None, module.TypeSystem.String));
        testMethod.Parameters.Add(new ParameterDefinition("content", ParameterAttributes.None, module.TypeSystem.String));
        testMethod.Parameters.Add(new ParameterDefinition("monogram", ParameterAttributes.None, module.TypeSystem.String));
        testMethod.Parameters.Add(new ParameterDefinition("hashSeed", ParameterAttributes.None, module.TypeSystem.String));
        genericNotifType.Methods.Add(testMethod);
        var tmBody = testMethod.Body;
        tmBody.InitLocals = true;
        var avatarLocal = new VariableDefinition(module.ImportReference(avatarType));
        tmBody.Variables.Add(avatarLocal);
        var tmIl = tmBody.GetILProcessor();
        // this.timeToHide = 5000; this.timeToShow = 5000; -- see the field-lookup comment above.
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 5000));
        tmIl.Append(Instruction.Create(OpCodes.Stfld, timeToHideField));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 5000));
        tmIl.Append(Instruction.Create(OpCodes.Stfld, timeToShowField));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_1));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, setTitleRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_2));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, setContentRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_3));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg, testMethod.Parameters[3]));
        tmIl.Append(Instruction.Create(OpCodes.Call, fromMonogramRef));
        tmIl.Append(Instruction.Create(OpCodes.Stloc, avatarLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, avatarLocal));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 32));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4, 32));
        tmIl.Append(Instruction.Create(OpCodes.Newobj, sizeCtorRef));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, getRoundImageRef));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, setImageRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldloc, avatarLocal));
        tmIl.Append(Instruction.Create(OpCodes.Callvirt, avatarDisposeRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Call, performLayoutRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Ldc_I4_1));
        tmIl.Append(Instruction.Create(OpCodes.Call, updateLayeredBackgroundRef));
        tmIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        tmIl.Append(Instruction.Create(OpCodes.Call, invalidateRef));
        tmIl.Append(Instruction.Create(OpCodes.Ret));
        var testMethodRef = module.ImportReference(testMethod);

        // --- New public method on FormMailNotification itself (not FormGenericNotification --
        // button_Reply/Flag/Delete are FormMailNotification's own private fields, only accessible
        // from a method the CLR verifier considers part of that same type): shows the reply/flag/
        // delete buttons directly, bypassing the real OnDisplayedNotificationChanged's requirement
        // for a genuine IMail-backed CurrentMailItemNotification (which this synthetic test has no
        // easy way to construct) -- purely for visual layout testing (does the new content padding
        // collide with these controls?), matching this whole patch's own "exercise the rendering
        // path directly, skip the parts that need real data" approach.
        var buttonReplyField = notifType.Fields.FirstOrDefault(f => f.Name == "button_Reply");
        var buttonFlagField = notifType.Fields.FirstOrDefault(f => f.Name == "button_Flag");
        var buttonDeleteField = notifType.Fields.FirstOrDefault(f => f.Name == "button_Delete");
        if (buttonReplyField is null || buttonFlagField is null || buttonDeleteField is null) { Console.Error.WriteLine("FAIL: button_Reply/button_Flag/button_Delete field(s) not found on FormMailNotification"); return 1; }

        TypeDefinition? buttonType = buttonReplyField.FieldType.Resolve();
        while (buttonType is not null && buttonType.FullName != "System.Windows.Forms.Control")
        {
            buttonType = buttonType.BaseType?.Resolve();
        }
        if (buttonType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control from button_Reply's own field type"); return 1; }
        var buttonShowRef = module.ImportReference(buttonType.Methods.First(m => m.Name == "Show" && m.Parameters.Count == 0));

        var showButtonsMethod = new MethodDefinition("__showTestNotificationButtons", MethodAttributes.Public, module.TypeSystem.Void);
        notifType.Methods.Add(showButtonsMethod);
        var sbIl = showButtonsMethod.Body.GetILProcessor();
        foreach (var f in new[] { buttonReplyField, buttonFlagField, buttonDeleteField })
        {
            sbIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            sbIl.Append(Instruction.Create(OpCodes.Ldfld, f));
            sbIl.Append(Instruction.Create(OpCodes.Callvirt, buttonShowRef));
        }
        // Show() alone left the buttons visually absent on the first live test -- consistent with
        // this whole project's throughline (Wine not repainting on a state change alone); force a
        // real layout+paint pass the same way the actual test method already does, reusing
        // performLayoutRef/invalidateRef already resolved above for that.
        sbIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        sbIl.Append(Instruction.Create(OpCodes.Call, performLayoutRef));
        sbIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        sbIl.Append(Instruction.Create(OpCodes.Call, invalidateRef));
        sbIl.Append(Instruction.Create(OpCodes.Ret));
        var showButtonsMethodRef = module.ImportReference(showButtonsMethod);

        // --- Timer field + tick method on formMain, calling the new public method.
        var timerField = new FieldDefinition("__monogramTestTimer", FieldAttributes.Private, timerTypeRef);
        mainType.Fields.Add(timerField);

        var tickMethod = new MethodDefinition("__monogramTestTick", MethodAttributes.Private, module.TypeSystem.Void);
        tickMethod.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
        tickMethod.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, module.ImportReference(typeof(EventArgs))));
        mainType.Methods.Add(tickMethod);
        var ttBody = tickMethod.Body;
        ttBody.InitLocals = true;
        var notifLocal = new VariableDefinition(module.ImportReference(notifType));
        ttBody.Variables.Add(notifLocal);
        var ttIl = ttBody.GetILProcessor();
        // if (!File.Exists(triggerPath)) return;  -- keep polling, don't fire yet.
        var ttRet = Instruction.Create(OpCodes.Ret);
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, triggerPath));
        ttIl.Append(Instruction.Create(OpCodes.Call, fileExistsRef));
        ttIl.Append(Instruction.Create(OpCodes.Brfalse, ttRet));
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, triggerPath));
        ttIl.Append(Instruction.Create(OpCodes.Call, fileDeleteRef));
        ttIl.Append(Instruction.Create(OpCodes.Ldarg_0));
        ttIl.Append(Instruction.Create(OpCodes.Ldfld, timerField));
        ttIl.Append(Instruction.Create(OpCodes.Callvirt, timerStopRef));
        ttIl.Append(Instruction.Create(OpCodes.Newobj, notifCtorRef));
        ttIl.Append(Instruction.Create(OpCodes.Stloc, notifLocal));
        ttIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        ttIl.Append(Instruction.Create(OpCodes.Ldc_I4, 100));
        ttIl.Append(Instruction.Create(OpCodes.Ldc_I4, 100));
        ttIl.Append(Instruction.Create(OpCodes.Newobj, pointCtorRef));
        ttIl.Append(Instruction.Create(OpCodes.Callvirt, setLocationRef));
        ttIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        ttIl.Append(Instruction.Create(OpCodes.Ldnull));
        ttIl.Append(Instruction.Create(OpCodes.Callvirt, notifShowRef));
        ttIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, "Jaguar Workshop Auto Restoration Specialists Ltd"));
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, "Re: Your 1967 Jaguar E-Type Series 1 MK2 Restoration Quote and Timeline Estimate for Review"));
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, "JW"));
        ttIl.Append(Instruction.Create(OpCodes.Ldstr, "Jaguar Workshop Auto Restoration Specialists Ltd"));
        ttIl.Append(Instruction.Create(OpCodes.Callvirt, testMethodRef));
        ttIl.Append(Instruction.Create(OpCodes.Ldloc, notifLocal));
        ttIl.Append(Instruction.Create(OpCodes.Callvirt, showButtonsMethodRef));
        ttIl.Append(ttRet);

        // --- Prepend to formMain.OnShown: create/configure/start __monogramTestTimer.
        {
            var body = onShownMethod.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            Emit(
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Newobj, timerCtorRef),
                Instruction.Create(OpCodes.Stfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldc_I4, pollIntervalMs),
                Instruction.Create(OpCodes.Callvirt, timerSetIntervalRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldftn, tickMethod),
                Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef),
                Instruction.Create(OpCodes.Callvirt, timerAddTickRef),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, timerField),
                Instruction.Create(OpCodes.Callvirt, timerStartRef)
            );
        }

        Console.WriteLine($"OK   {fileName}: added FormGenericNotification::__showTestMonogramNotification and {mainFormType}::OnShown -- polls every {pollIntervalMs}ms for {triggerPath}; when present, deletes it and shows a test FormMailNotification with a UIAvatar.FromMonogram(\"JW\", \"Jaguar Workshop\") avatar, matching supporting/email-with-no-image.png");
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

// --patch-notification-geometry-diag <input-dir> <output-dir>
//
// User directly disputed the eighteenth round's "padding is fine" conclusion after watching the
// title-singleline fix live -- rightly so; that conclusion was based on eyeballing/measuring
// screenshots, not ground truth. This logs the actual computed `headerRect`/`contentRect`/
// `imageRect` (via their own `ToString()`, far simpler than rebuilding each field manually the
// way --patch-notification-layout-diag did) at the very end of `doLayout()` -- once it has
// actually finished computing them, not the stale pre-call values a top-of-method insertion would
// see. `doLayout()` has a single, branch-free exit (all its if/else branches converge before the
// end, no exception handlers), so this is a plain "insert before the method's one and only final
// ret" -- same low-risk shape as the append-only insertions already used elsewhere in this file
// (e.g. --patch-notification-text-in-bitmap's call into updateBackgroundBitmap), verified the same
// way (checking the anchor isn't itself a branch target or handler boundary before inserting).
static int RunPatchNotificationGeometryDiag(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("usage: il-patcher --patch-notification-geometry-diag <input-dir> <output-dir>");
        return 2;
    }

    string inDir = args[1];
    string outDir = args[2];
    Directory.CreateDirectory(outDir);
    const string logPath = @"Z:\tmp\claude-diag.log";

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
        var headerRectField = type.Fields.FirstOrDefault(f => f.Name == "headerRect");
        var contentRectField = type.Fields.FirstOrDefault(f => f.Name == "contentRect");
        var imageRectField = type.Fields.FirstOrDefault(f => f.Name == "imageRect");
        if (headerRectField is null || contentRectField is null || imageRectField is null) { Console.Error.WriteLine("FAIL: headerRect/contentRect/imageRect field(s) not found"); return 1; }

        var rectangleTypeDef = headerRectField.FieldType.Resolve();
        if (rectangleTypeDef is null) { Console.Error.WriteLine("FAIL: couldn't resolve Rectangle from headerRect's own FieldType"); return 1; }
        var rectangleToStringDef = rectangleTypeDef.Methods.FirstOrDefault(m => m.Name == "ToString" && m.Parameters.Count == 0);
        if (rectangleToStringDef is null) { Console.Error.WriteLine("FAIL: Rectangle.ToString() not found"); return 1; }
        var rectangleToStringRef = module.ImportReference(rectangleToStringDef);

        MethodReference Import(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var stringConcat2 = Import(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        var appendAllText = Import(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);

        var body = doLayoutMethod.Body;
        body.SimplifyMacros();
        var il = body.GetILProcessor();
        var instrs = body.Instructions;
        var lastInstr = instrs[instrs.Count - 1];
        if (lastInstr.OpCode != OpCodes.Ret) { Console.Error.WriteLine($"FAIL: doLayout's last instruction isn't Ret (got {lastInstr.OpCode}) -- method shape changed, review needed"); return 1; }

        // The final `ret` IS a branch target here -- doLayout's if/else-if/else-if chain (bigImage
        // / image / imageList) compiles with each branch jumping to a shared exit point at the
        // method's end. Per IL-patching lesson 2, inserting new code immediately before it would
        // get skipped by those branches; retarget them (and any exception-handler boundary
        // pointing at it, per lesson 3) to the new first inserted instruction instead, same
        // pattern already used in RunPatchNotificationTextInBitmap.
        var newFirst = Instruction.Create(OpCodes.Ldstr, "GEOMETRY header=");
        il.InsertBefore(lastInstr, newFirst);
        int retargetedBranches = 0;
        foreach (var instr in instrs)
        {
            if (instr != newFirst && instr.Operand == lastInstr) { instr.Operand = newFirst; retargetedBranches++; }
        }
        // The bigImage/image branches in doLayout's if/else-if/else-if chain don't actually jump to
        // a shared exit point at all -- each ends its own block with its own standalone `ret`
        // (confirmed via raw IL dump: IL_00f0 and IL_0145 are independent Ret instructions, not
        // branches to the method's real final ret). The branch-operand retargeting above can't catch
        // these -- a bare `ret` has no Operand to compare. Convert every OTHER Ret instruction in the
        // body to `br newFirst` so those paths also flow through the new logging before falling
        // through to the real final ret.
        int retargetedRets = 0;
        foreach (var instr in instrs.ToList())
        {
            if (instr != lastInstr && instr.OpCode == OpCodes.Ret)
            {
                instr.OpCode = OpCodes.Br;
                instr.Operand = newFirst;
                retargetedRets++;
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
        Console.WriteLine($"     ({retargetedBranches} branch operand(s), {retargetedRets} standalone early-ret instruction(s), {retargetedHandlerBounds} handler boundary field(s) retargeted from the old final ret to the new pre-ret logging)");

        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldflda, headerRectField));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, rectangleToStringRef));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldstr, " content="));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldflda, contentRectField));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, rectangleToStringRef));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldstr, " image="));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldflda, imageRectField));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, rectangleToStringRef));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldstr, "\n"));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, stringConcat2));
        // Stack is now [msg]. AppendAllText(string path, string contents) needs [path, msg] in
        // that push order -- store the built message in a local first, then push path and reload
        // the message, rather than trying to have built `path` before `msg` above (which would
        // have needed the whole concatenation chain reordered).
        var tmpMsg = new VariableDefinition(module.TypeSystem.String);
        body.Variables.Add(tmpMsg);
        body.InitLocals = true;
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Stloc, tmpMsg));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldstr, logPath));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Ldloc, tmpMsg));
        il.InsertBefore(lastInstr, Instruction.Create(OpCodes.Call, appendAllText));

        Console.WriteLine($"OK   {fileName}: {targetType}::doLayout -- now logs headerRect/contentRect/imageRect (via ToString()) to {logPath}");
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
            // chain to resolve it (same technique as RunPatchDiag's getHandle/getWidth/getHeight),
            // not typeof() reflection (CLAUDE.md's IL-patching lesson 5).
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

record PatchEntry(string Assembly, string Type, string Method, string IlOffset, int OldValue, int NewValue);
