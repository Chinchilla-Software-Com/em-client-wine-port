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
// Temporary instrumentation patch (not meant to ship): inserts logging at the top of
// MailClient.Common.UI.dll's Controls.ControlDataGrid.ControlDataGrid::drawCellsWithGrouping
// that appends the live values of groups.Count, itemsCount, UseGroupsInCurrentView, and
// whether cachedTopItemGroup is null to Z:\tmp\claude-diag.log (== /tmp/claude-diag.log on
// the Linux side, readable directly with no CX_DEBUGMSG trace needed) every time the method
// runs. Exists to answer one question directly instead of inferring it from GDI trace noise:
// is dataGridCategory's data genuinely empty at paint time in this environment, or is
// something else suppressing the draw despite real data being present.
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

    const string targetAssembly = "MailClient.Common.UI.dll";
    const string targetType = "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid";
    const string targetMethod = "drawCellsWithGrouping";
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
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        var groupsField = type.Fields.FirstOrDefault(f => f.Name == "groups");
        var itemsCountField = type.Fields.FirstOrDefault(f => f.Name == "itemsCount");
        var cachedTopItemGroupField = type.Fields.FirstOrDefault(f => f.Name == "cachedTopItemGroup");
        var useGroupsProp = type.Methods.FirstOrDefault(m => m.Name == "get_UseGroupsInCurrentView");
        if (groupsField is null || itemsCountField is null || cachedTopItemGroupField is null || useGroupsProp is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find one of groups/itemsCount/cachedTopItemGroup fields or get_UseGroupsInCurrentView");
            return 1;
        }
        var groupsCountGetter = groupsField.FieldType.Resolve().Methods.FirstOrDefault(m => m.Name == "get_Count");
        if (groupsCountGetter is null) { Console.Error.WriteLine("FAIL: DataGridGroupCollection has no get_Count"); return 1; }

        MethodReference Import(System.Reflection.MethodBase mb) => module.ImportReference(mb);
        var int32ToString = Import(typeof(int).GetMethod("ToString", Type.EmptyTypes)!);
        var boolToString = Import(typeof(bool).GetMethod("ToString", Type.EmptyTypes)!);
        var stringConcat2 = Import(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        var appendAllText = Import(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);
        var groupsCountGetterRef = module.ImportReference(groupsCountGetter);
        var useGroupsPropRef = module.ImportReference(useGroupsProp);

        var body = method.Body;
        body.InitLocals = true;
        var tmpInt = new VariableDefinition(module.TypeSystem.Int32);
        var tmpBool = new VariableDefinition(module.TypeSystem.Boolean);
        var tmpMsg = new VariableDefinition(module.TypeSystem.String);
        body.Variables.Add(tmpInt);
        body.Variables.Add(tmpBool);
        body.Variables.Add(tmpMsg);

        var il = body.GetILProcessor();
        var first = body.Instructions[0];

        void Emit(params Instruction[] instrs)
        {
            foreach (var i in instrs) il.InsertBefore(first, i);
        }

        // groups.Count=N
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, groupsField),
            Instruction.Create(OpCodes.Callvirt, groupsCountGetterRef),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldstr, "groups.Count="),
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

        // itemsCount=N
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, itemsCountField),
            Instruction.Create(OpCodes.Stloc, tmpInt),
            Instruction.Create(OpCodes.Ldstr, "itemsCount="),
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

        // UseGroupsInCurrentView=True/False
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, useGroupsPropRef),
            Instruction.Create(OpCodes.Stloc, tmpBool),
            Instruction.Create(OpCodes.Ldstr, "UseGroupsInCurrentView="),
            Instruction.Create(OpCodes.Ldloca, tmpBool),
            Instruction.Create(OpCodes.Call, boolToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, tmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, tmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );

        // cachedTopItemGroupIsNull=True/False
        Emit(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, cachedTopItemGroupField),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Stloc, tmpBool),
            Instruction.Create(OpCodes.Ldstr, "cachedTopItemGroupIsNull="),
            Instruction.Create(OpCodes.Ldloca, tmpBool),
            Instruction.Create(OpCodes.Call, boolToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n---\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, tmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, tmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );

        // Second instrumentation point: bracket handleVisibleItemsChange with plain ENTER/EXIT
        // markers (no field reads needed) to test whether an exception thrown between setting
        // refreshingCachedItemList=true and resetting it to false (there's no try/finally) is
        // what's leaving drawCells()'s "if (!refreshingCachedItemList)" guard permanently
        // blocking both the grouped and non-grouped draw paths. If ENTER logs but EXIT never
        // does, that's the proof.
        var hvicMethod = type.Methods.FirstOrDefault(m => m.Name == "handleVisibleItemsChange" && m.HasBody);
        var refreshingField = type.Fields.FirstOrDefault(f => f.Name == "refreshingCachedItemList");
        if (hvicMethod is null || refreshingField is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find handleVisibleItemsChange or refreshingCachedItemList field");
            return 1;
        }
        var hvicBody = hvicMethod.Body;
        var hvicIl = hvicBody.GetILProcessor();
        var hvicInstrs = hvicBody.Instructions;

        void EmitPlainLog(ILProcessor proc, Instruction before, string message)
        {
            proc.InsertBefore(before, Instruction.Create(OpCodes.Ldstr, logPath));
            proc.InsertBefore(before, Instruction.Create(OpCodes.Ldstr, message));
            proc.InsertBefore(before, Instruction.Create(OpCodes.Call, appendAllText));
        }

        // EXIT marker: find `ldarg.0; ldc.i4.0; stfld refreshingCachedItemList; ret` and log
        // immediately before that ldarg.0 -- i.e. only reached if nothing above threw.
        Instruction? exitTarget = null;
        for (int k = 0; k + 3 < hvicInstrs.Count; k++)
        {
            if (hvicInstrs[k].OpCode == OpCodes.Ldarg_0 &&
                hvicInstrs[k + 1].OpCode == OpCodes.Ldc_I4_0 &&
                hvicInstrs[k + 2].OpCode == OpCodes.Stfld &&
                hvicInstrs[k + 2].Operand is FieldReference fr && fr.Name == "refreshingCachedItemList" &&
                hvicInstrs[k + 3].OpCode == OpCodes.Ret)
            {
                exitTarget = hvicInstrs[k];
                break;
            }
        }
        if (exitTarget is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find the refreshingCachedItemList=false; ret pattern in handleVisibleItemsChange");
            return 1;
        }
        // exitTarget may itself be a branch target (e.g. the brfalse.s skipping refreshCachedItemsList()
        // jumps straight to it) -- InsertBefore doesn't retarget existing branches, so without fixing
        // this up, any branch landing on exitTarget would skip over the newly-inserted log entirely,
        // making the EXIT marker fire on only one of the two paths instead of unconditionally.
        var exitLogFirst = Instruction.Create(OpCodes.Ldstr, logPath);
        hvicIl.InsertBefore(exitTarget, exitLogFirst);
        hvicIl.InsertBefore(exitTarget, Instruction.Create(OpCodes.Ldstr, "handleVisibleItemsChange:EXIT (reached false-reset)\n"));
        hvicIl.InsertBefore(exitTarget, Instruction.Create(OpCodes.Call, appendAllText));
        foreach (var instr in hvicInstrs)
        {
            if (instr.Operand == exitTarget) instr.Operand = exitLogFirst;
        }
        foreach (var handler in hvicBody.ExceptionHandlers)
        {
            if (handler.TryStart == exitTarget) handler.TryStart = exitLogFirst;
            if (handler.TryEnd == exitTarget) handler.TryEnd = exitLogFirst;
            if (handler.HandlerStart == exitTarget) handler.HandlerStart = exitLogFirst;
            if (handler.HandlerEnd == exitTarget) handler.HandlerEnd = exitLogFirst;
        }
        // ENTER marker: insert before the method's very first instruction.
        EmitPlainLog(hvicIl, hvicInstrs[0], "handleVisibleItemsChange:ENTER\n");

        // Third instrumentation point: doPaint()'s very first instruction. doPaint has two
        // early-exit branches above the drawCells() call (scrollbar-only repaint; columns.Count==0
        // && !ownerDraw) that would explain zero content draws without groups/itemsCount/
        // refreshingCachedItemList being at fault. Logs Name so log entries can finally be
        // attributed to a specific ControlDataGrid instance (dataGridCategory specifically),
        // since drawCellsWithGrouping's own diagnostic has only ever shown some other 3-item grid.
        var doPaintMethod = type.Methods.FirstOrDefault(m => m.Name == "doPaint" && m.HasBody);
        var columnsField = type.Fields.FirstOrDefault(f => f.Name == "columns");
        var ownerDrawField = type.Fields.FirstOrDefault(f => f.Name == "ownerDraw");
        var vScrollBarField = type.Fields.FirstOrDefault(f => f.Name == "vScrollBar");
        if (doPaintMethod is null || columnsField is null || ownerDrawField is null || vScrollBarField is null)
        {
            Console.Error.WriteLine("FAIL: couldn't find doPaint or columns/ownerDraw/vScrollBar fields");
            return 1;
        }
        var columnsCountGetter = columnsField.FieldType.Resolve().Methods.FirstOrDefault(m => m.Name == "get_Count");
        if (columnsCountGetter is null) { Console.Error.WriteLine("FAIL: DataGridColumnCollection has no get_Count"); return 1; }
        var columnsCountGetterRef = module.ImportReference(columnsCountGetter);

        // il-patcher itself doesn't reference System.Windows.Forms, so resolve Control (and its
        // Name/Visible getters) by walking up ControlDataGrid's own base-type chain via Cecil
        // instead of via System.Reflection typeof().
        TypeDefinition? controlType = type.Resolve();
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var nameGetterDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Name");
        var visibleGetterDef = controlType.Methods.FirstOrDefault(m => m.Name == "get_Visible");
        if (nameGetterDef is null || visibleGetterDef is null) { Console.Error.WriteLine("FAIL: Control missing get_Name/get_Visible"); return 1; }
        var controlGetName = module.ImportReference(nameGetterDef);
        var controlGetVisible = module.ImportReference(visibleGetterDef);

        var doPaintBody = doPaintMethod.Body;
        doPaintBody.InitLocals = true;
        var dpTmpInt = new VariableDefinition(module.TypeSystem.Int32);
        var dpTmpBool = new VariableDefinition(module.TypeSystem.Boolean);
        var dpTmpMsg = new VariableDefinition(module.TypeSystem.String);
        doPaintBody.Variables.Add(dpTmpInt);
        doPaintBody.Variables.Add(dpTmpBool);
        doPaintBody.Variables.Add(dpTmpMsg);
        var dpIl = doPaintBody.GetILProcessor();
        var dpFirst = doPaintBody.Instructions[0];
        void EmitDp(params Instruction[] instrs) { foreach (var i in instrs) dpIl.InsertBefore(dpFirst, i); }

        // Name=<name>
        EmitDp(
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldstr, "doPaint:Name="),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, controlGetName),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) })!)),
            Instruction.Create(OpCodes.Call, appendAllText)
        );
        // columns.Count=N
        EmitDp(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, columnsField),
            Instruction.Create(OpCodes.Callvirt, columnsCountGetterRef),
            Instruction.Create(OpCodes.Stloc, dpTmpInt),
            Instruction.Create(OpCodes.Ldstr, "doPaint:columns.Count="),
            Instruction.Create(OpCodes.Ldloca, dpTmpInt),
            Instruction.Create(OpCodes.Call, int32ToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, dpTmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, dpTmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );
        // ownerDraw=True/False
        EmitDp(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, ownerDrawField),
            Instruction.Create(OpCodes.Stloc, dpTmpBool),
            Instruction.Create(OpCodes.Ldstr, "doPaint:ownerDraw="),
            Instruction.Create(OpCodes.Ldloca, dpTmpBool),
            Instruction.Create(OpCodes.Call, boolToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, dpTmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, dpTmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );
        // vScrollBar.Visible=True/False
        EmitDp(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, vScrollBarField),
            Instruction.Create(OpCodes.Callvirt, controlGetVisible),
            Instruction.Create(OpCodes.Stloc, dpTmpBool),
            Instruction.Create(OpCodes.Ldstr, "doPaint:vScrollBar.Visible="),
            Instruction.Create(OpCodes.Ldloca, dpTmpBool),
            Instruction.Create(OpCodes.Call, boolToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n---\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, dpTmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, dpTmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );

        // Fourth instrumentation point: SetDataSource<TItem>(source, filterName). All four
        // doPaint samples for dataGridCategory showed columns.Count==0 -- not just the first,
        // "premature" one -- which no longer fits "one early paint before Load finishes". This
        // logs Name + whether the source argument is null every time SetDataSource runs, to
        // check whether ReloadCategories() (called from loadCategories(), called from
        // formSettings_Load) is even reaching dataGridCategory at all.
        var setDataSourceMethod = type.Methods.FirstOrDefault(m => m.Name == "SetDataSource" && m.HasBody && m.Parameters.Count == 2);
        if (setDataSourceMethod is null) { Console.Error.WriteLine("FAIL: couldn't find SetDataSource<TItem>(source, filterName)"); return 1; }
        var sdsBody = setDataSourceMethod.Body;
        sdsBody.InitLocals = true;
        var sdsTmpBool = new VariableDefinition(module.TypeSystem.Boolean);
        var sdsTmpMsg = new VariableDefinition(module.TypeSystem.String);
        sdsBody.Variables.Add(sdsTmpBool);
        sdsBody.Variables.Add(sdsTmpMsg);
        var sdsIl = sdsBody.GetILProcessor();
        var sdsFirst = sdsBody.Instructions[0];
        void EmitSds(params Instruction[] instrs) { foreach (var i in instrs) sdsIl.InsertBefore(sdsFirst, i); }
        // Name=<name>
        EmitSds(
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldstr, "SetDataSource:Name="),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, controlGetName),
            Instruction.Create(OpCodes.Ldstr, "\n"),
            Instruction.Create(OpCodes.Call, module.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) })!)),
            Instruction.Create(OpCodes.Call, appendAllText)
        );
        // sourceIsNull=True/False
        EmitSds(
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Ceq),
            Instruction.Create(OpCodes.Stloc, sdsTmpBool),
            Instruction.Create(OpCodes.Ldstr, "SetDataSource:sourceIsNull="),
            Instruction.Create(OpCodes.Ldloca, sdsTmpBool),
            Instruction.Create(OpCodes.Call, boolToString),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Ldstr, "\n---\n"),
            Instruction.Create(OpCodes.Call, stringConcat2),
            Instruction.Create(OpCodes.Stloc, sdsTmpMsg),
            Instruction.Create(OpCodes.Ldstr, logPath),
            Instruction.Create(OpCodes.Ldloc, sdsTmpMsg),
            Instruction.Create(OpCodes.Call, appendAllText)
        );

        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::{targetMethod} -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted ENTER/EXIT markers around {targetType}::handleVisibleItemsChange -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::doPaint -> {logPath}");
        Console.WriteLine($"OK   {fileName}: inserted diagnostic logging at top of {targetType}::SetDataSource -> {logPath}");
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
