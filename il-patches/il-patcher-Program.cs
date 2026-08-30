using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
// ControlDataGrid/Settings-panel investigation (see git history for that version). Now
// instruments MailClient.dll's UI.Forms.NotificationForms.FormGenericNotification (the shared
// base class behind every notification toast, e.g. FormMailNotification) to answer: for the
// notification a user sees as "an empty box that only shows text right as it starts to fade",
// which concrete window (type/handle/size) is it, and what are `state`/`title`/`content` (the
// fields OnPaint/OnPaintTitle draw from) at each actual OnPaint call and each timer_OnTimer
// fade-animation tick? A CX_DEBUGMSG=+win,+win32u,+x11drv,+event,+message trace of this
// investigation found two different LayeredBaseForm windows in one repro with ambiguous
// evidence pointing at either as "the" box (see reports/ -- not yet written up, investigation
// in progress) -- this settles it directly from the app's own state instead of guessing from
// window-surface flush geometry.
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
        if (onPaintMethod is null) { Console.Error.WriteLine("FAIL: OnPaint(PaintEventArgs) not found"); return 1; }
        if (timerMethod is null) { Console.Error.WriteLine("FAIL: timer_OnTimer not found"); return 1; }

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
            body.Variables.Add(tmpInt);
            body.Variables.Add(tmpMsg);
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            void Emit(params Instruction[] instrs) { foreach (var i in instrs) il.InsertBefore(first, i); }

            // "<label> tick=<TickCount> type=<TypeName>\n"
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

        Instrument(onPaintMethod, "OnPaint");
        Instrument(timerMethod, "timer_OnTimer");

        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::OnPaint -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::timer_OnTimer -> {logPath}");
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
// (confirmed via --patch-diag instrumentation -- title/content fields are already correct on
// the very first OnPaint) but Wine's layered-window compositor doesn't push that painted
// content to the visible screen surface until much later, when the rapid repaint burst during
// fade-out finally forces a recomposite. See reports/notification-empty-until-fade-findings.md.
//
// First attempt (Invalidate() alone at the Appearing->Visible transition, matching what every
// other opacity-changing branch in timer_OnTimer already does) made no observable difference --
// user-confirmed. Root cause is therefore NOT a missing repaint *request*; a plain WM_PAINT was
// almost certainly already happening from the very next tick or two regardless. What's actually
// different about the fade-out phase (where content reliably becomes visible) isn't "more
// repaints" but "the window's layered alpha genuinely changes on every one of those ticks" --
// Opacity ramps down, which calls SetLayeredWindowAttributes with a new byte value each time.
// During the long "hold" (multi-second, Opacity pinned at exactly 1.0 the whole time), nothing
// ever calls SetLayeredWindowAttributes again after the single call that set it to 1.0 on entry.
// Revised fix: force a second, genuine SetLayeredWindowAttributes call right as the hold begins
// by nudging Opacity to a value that rounds to a different byte and immediately back to 1.0 --
// giving Wine's compositor the same kind of "alpha actually changed" signal it evidently needs
// to recomposite, rather than relying on repaint requests alone.
//
// Touches MailClient.dll only (FormGenericNotification.timer_OnTimer). Insertion point is deep
// inside the method's single try block, nowhere near TryStart/TryEnd/HandlerStart/HandlerEnd,
// and confirmed (both by inspection via `ilspycmd -il` before writing this, and by a runtime
// check below) not to be the target of any branch in the method -- the two IL-patching pitfalls
// documented in CLAUDE.md that would silently produce wrong or invalid IL here.
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

        var body = method.Body;
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
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Call, invalidateRef));
        // Nudge Opacity to a value that rounds to a different native alpha byte and immediately
        // back to 1.0 -- two genuine SetLayeredWindowAttributes calls, not just a repaint
        // request. See doc comment above for why this, not Invalidate() alone, is the fix.
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldc_R8, 0.999));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Call, setOpacityRef));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Ldc_R8, 1.0));
        il.InsertBefore(insertBefore, Instruction.Create(OpCodes.Call, setOpacityRef));

        Console.WriteLine($"OK   {fileName}: inserted Invalidate() + Opacity nudge after state=Visible in {targetType}::{targetMethod}");
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
