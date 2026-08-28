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

return RunScan(args);

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
// The real fix, found via --patch-diag: dataGridCategory paints exactly once during the whole
// Settings session, and at that one paint columns.Count==0 -- it fires before
// formSettings.loadCategories() has added its three columns, so ControlDataGrid.doPaint()'s
// `columns.Count == 0 && !ownerDraw` branch fires (background fill only, drawCells() never
// called), and nothing ever invalidates/repaints the control again afterward. This is not the
// interpolation bug and not (only) the clip-region bug -- it's a stale premature paint that's
// never superseded by a correct one.
//
// Fix: MailClient.dll, UI.Forms.formSettings::formSettings_Load -- insert
// `this.BeginInvoke(new Action(dataGridCategory.Refresh));` immediately after the existing
// `dataGridCategory.EndUpdate();` call. A direct synchronous `dataGridCategory.Refresh();` at
// this spot was tried first and made no observable difference (confirmed via --patch-diag: still
// exactly one paint, still columns.Count==0) -- Refresh()/Update() during the Load event, before
// the form is actually shown, is a known-unreliable pattern in WinForms since the paint
// infrastructure isn't necessarily ready. BeginInvoke(Action) defers the repaint to after the
// current message finishes processing, which is the standard fix for that class of bug.
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
        var method = type.Methods.FirstOrDefault(m => m.Name == targetMethod && m.HasBody);
        if (method is null) { Console.Error.WriteLine($"FAIL: method not found: {targetType}::{targetMethod}"); return 1; }

        var dataGridCategoryField = type.Fields.FirstOrDefault(f => f.Name == "dataGridCategory");
        if (dataGridCategoryField is null) { Console.Error.WriteLine("FAIL: couldn't find dataGridCategory field"); return 1; }

        var gridType = dataGridCategoryField.FieldType.Resolve();
        // Resolve Control::Refresh()/BeginInvoke(Action) by walking ControlDataGrid's own
        // base-type chain, same approach as --patch-diag (this project doesn't reference
        // System.Windows.Forms directly).
        TypeDefinition? controlType = gridType;
        while (controlType is not null && controlType.FullName != "System.Windows.Forms.Control")
        {
            controlType = controlType.BaseType?.Resolve();
        }
        if (controlType is null) { Console.Error.WriteLine("FAIL: couldn't resolve System.Windows.Forms.Control in base-type chain"); return 1; }
        var refreshDef = controlType.Methods.FirstOrDefault(m => m.Name == "Refresh" && m.Parameters.Count == 0);
        if (refreshDef is null) { Console.Error.WriteLine("FAIL: Control has no parameterless Refresh()"); return 1; }
        var refreshRef = module.ImportReference(refreshDef);
        var beginInvokeDef = controlType.Methods.FirstOrDefault(m => m.Name == "BeginInvoke" &&
            m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "Action");
        if (beginInvokeDef is null) { Console.Error.WriteLine("FAIL: Control has no BeginInvoke(Action)"); return 1; }
        var beginInvokeRef = module.ImportReference(beginInvokeDef);
        var actionCtor = module.ImportReference(typeof(Action).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);
        var appendAllText = module.ImportReference(typeof(File).GetMethod("AppendAllText", new[] { typeof(string), typeof(string) })!);

        var il = method.Body.Instructions;

        // Checkpoint markers bracketing the loadCategories() call: SetDataSource is never called
        // for dataGridCategory (confirmed via --patch-diag), which only makes sense if
        // loadCategories() itself never runs to completion. If BEFORE logs but AFTER doesn't,
        // that's proof something in loadCategories() throws.
        var loadCategoriesMethod = type.Methods.FirstOrDefault(m => m.Name == "loadCategories" && m.HasBody);
        if (loadCategoriesMethod is null) { Console.Error.WriteLine("FAIL: couldn't find loadCategories()"); return 1; }
        Instruction? loadCategoriesCall = null;
        for (int k = 0; k < il.Count; k++)
        {
            if ((il[k].OpCode == OpCodes.Call || il[k].OpCode == OpCodes.Callvirt) &&
                il[k].Operand is MethodReference mr2 && mr2.Name == "loadCategories")
            {
                loadCategoriesCall = il[k];
                break;
            }
        }
        if (loadCategoriesCall is null) { Console.Error.WriteLine($"FAIL: couldn't find loadCategories() call in {targetType}::{targetMethod}"); return 1; }
        var checkpointProc = method.Body.GetILProcessor();
        // AFTER marker first (so retargeting, if ever needed, only has to consider one anchor at a time)
        var afterTarget = loadCategoriesCall.Next;
        checkpointProc.InsertBefore(afterTarget, Instruction.Create(OpCodes.Ldstr, logPath));
        checkpointProc.InsertBefore(afterTarget, Instruction.Create(OpCodes.Ldstr, "formSettings_Load:AFTER loadCategories()\n"));
        checkpointProc.InsertBefore(afterTarget, Instruction.Create(OpCodes.Call, appendAllText));
        // BEFORE marker: loadCategoriesCall itself is not a branch target in this straight-line
        // method (verified by reading the IL directly), so a plain InsertBefore is safe here.
        checkpointProc.InsertBefore(loadCategoriesCall, Instruction.Create(OpCodes.Ldstr, logPath));
        checkpointProc.InsertBefore(loadCategoriesCall, Instruction.Create(OpCodes.Ldstr, "formSettings_Load:BEFORE loadCategories()\n"));
        checkpointProc.InsertBefore(loadCategoriesCall, Instruction.Create(OpCodes.Call, appendAllText));
        // ENTER marker: the log file didn't even get created in the previous round (neither
        // BEFORE nor AFTER fired), so check whether formSettings_Load is entered at all, or
        // whether something in setFont()/dataGridCategory.BeginUpdate() -- both of which run
        // before the BEFORE marker -- throws first.
        // NB: must capture the anchor ONCE and reuse it for all three inserts (not re-read
        // il[0] each call) -- InsertBefore(fixedAnchor, x) three times in order naturally
        // stacks x1,x2,x3 correctly right before the anchor; re-reading il[0] each time picks
        // up the previous insert as the new "first" and reverses the order, corrupting the
        // stack (this bit the first version of this exact block).
        var methodFirst = il[0];
        checkpointProc.InsertBefore(methodFirst, Instruction.Create(OpCodes.Ldstr, logPath));
        checkpointProc.InsertBefore(methodFirst, Instruction.Create(OpCodes.Ldstr, "formSettings_Load:ENTER\n"));
        checkpointProc.InsertBefore(methodFirst, Instruction.Create(OpCodes.Call, appendAllText));
        Instruction? endUpdateCall = null;
        for (int k = 0; k < il.Count; k++)
        {
            if ((il[k].OpCode == OpCodes.Callvirt || il[k].OpCode == OpCodes.Call) &&
                il[k].Operand is MethodReference mr && mr.Name == "EndUpdate" &&
                il[k - 1].OpCode == OpCodes.Ldfld && il[k - 1].Operand is FieldReference fr && fr.Name == "dataGridCategory")
            {
                endUpdateCall = il[k];
                break;
            }
        }
        if (endUpdateCall is null)
        {
            Console.Error.WriteLine($"FAIL: couldn't find `dataGridCategory.EndUpdate()` call in {targetType}::{targetMethod} -- refusing to patch");
            return 1;
        }

        var proc = method.Body.GetILProcessor();
        var insertPoint = endUpdateCall.Next; // right after the EndUpdate() call
        // this.BeginInvoke(new Action(dataGridCategory.Refresh)); -- deferred via the message
        // loop rather than called synchronously inline. A direct dataGridCategory.Refresh() call
        // right here was tried first and made no difference (still one paint, still
        // columns.Count==0) -- plausible explanation: Refresh()/Update() during the Load event,
        // before the form is actually shown, is a known-unreliable WinForms pattern since the
        // paint infrastructure isn't necessarily ready yet. BeginInvoke defers to after the
        // current message is done processing, which is the standard fix for that class of bug.
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Ldfld, dataGridCategoryField));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Ldftn, refreshRef));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Newobj, actionCtor));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Callvirt, beginInvokeRef));
        proc.InsertBefore(insertPoint, Instruction.Create(OpCodes.Pop));
        // retarget anything branching to the old insertPoint isn't needed here: insertPoint is
        // the CultureChanger.get_Instance() call, not a branch target in this method (verified
        // by reading the IL directly -- no branches land past EndUpdate() in formSettings_Load).

        Console.WriteLine($"OK   {fileName}: {targetType}::{targetMethod} -- inserted this.BeginInvoke(new Action(dataGridCategory.Refresh)) right after dataGridCategory.EndUpdate()");
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

record PatchEntry(string Assembly, string Type, string Method, string IlOffset, int OldValue, int NewValue);
