using Mono.Cecil;
using Mono.Cecil.Cil;

// Separate standalone tool rather than a new il-patcher.dll mode, same rationale as
// license-oaep-patcher: this fix's shape -- adding calls into a newly-added sibling assembly --
// doesn't fit il-patcher's existing per-fix flag pattern as cleanly. See CLAUDE.md.
//
// Wires FormGenericNotification up to MailClient.Notifications.ButtonOverlay.ButtonOverlayManager
// (option 2 from the reply/flag/delete/close/settings invisible-until-fade investigation --
// reports/notification-empty-until-fade-findings.md): two new small methods forward
// close/settings clicks into the existing performMouseClick logic, one new method gathers the
// existing closeRect/settingsRect/closeImage/etc. fields and theme colors and calls
// ButtonOverlayManager.Sync (all the Rectangle math and Controls.Find lookup live in that C#
// helper, not here, precisely to keep this patch a straight-line sequence of field loads and one
// call -- no branches to get wrong), called once from ShowNotification after layout is fresh, and
// ButtonOverlayManager.HideAll is called from the top of Hide().

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: notification-button-overlay-patcher <input MailClient.dll> <MailClient.Notifications.ButtonOverlay.dll> <output MailClient.dll>");
    return 1;
}

string inputPath = args[0];
string patchDllPath = args[1];
string outputPath = args[2];

var resolver = new DefaultAssemblyResolver();
string? inputDir = Path.GetDirectoryName(Path.GetFullPath(inputPath));
if (inputDir != null) resolver.AddSearchDirectory(inputDir);
string? patchDir = Path.GetDirectoryName(Path.GetFullPath(patchDllPath));
if (patchDir != null) resolver.AddSearchDirectory(patchDir);

using var patchModule = ModuleDefinition.ReadModule(patchDllPath, new ReaderParameters { AssemblyResolver = resolver });
TypeDefinition managerType = patchModule.GetType("MailClient.Notifications.ButtonOverlay.ButtonOverlayManager")
    ?? throw new Exception("ButtonOverlayManager type not found in patch assembly");
MethodDefinition syncMethodDef = managerType.Methods.Single(m => m.Name == "Sync");
MethodDefinition hideAllMethodDef = managerType.Methods.Single(m => m.Name == "HideAll");

using var module = ModuleDefinition.ReadModule(inputPath, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver });
MethodReference syncRef = module.ImportReference(syncMethodDef);
MethodReference hideAllRef = module.ImportReference(hideAllMethodDef);

TypeDefinition type = module.GetType("MailClient.UI.Forms.NotificationForms.FormGenericNotification")
    ?? throw new Exception("FormGenericNotification not found");

FieldDefinition closeRectField = type.Fields.Single(f => f.Name == "closeRect");
FieldDefinition settingsRectField = type.Fields.Single(f => f.Name == "settingsRect");
FieldDefinition closeImageField = type.Fields.Single(f => f.Name == "closeImage");
FieldDefinition closeImageOverField = type.Fields.Single(f => f.Name == "closeImageOver");
FieldDefinition settingsImageField = type.Fields.Single(f => f.Name == "settingsImage");
FieldDefinition settingsImageOverField = type.Fields.Single(f => f.Name == "settingsImageOver");

MethodDefinition performMouseClickDef = type.Methods.Single(m => m.Name == "performMouseClick");
MethodReference performMouseClickRef = module.ImportReference(performMouseClickDef);
TypeDefinition rectangleTypeDef = closeRectField.FieldType.Resolve() ?? throw new Exception("couldn't resolve Rectangle from closeRect's own field type");
MethodReference rectGetLocationRef = module.ImportReference(rectangleTypeDef.Methods.Single(m => m.Name == "get_Location"));

// ThemeManager/IColorTheme live in MailClient.Common.UI.dll, a sibling assembly this tool itself
// has no compile-time reference to -- resolve their exact members from an EXISTING call site
// already in this exact module (updateBackgroundBitmap already calls
// ThemeManager.Instance.GetActiveTheme(this).NotificationWindowHeaderEnd/BackgroundStart when
// drawing the header/content backgrounds) rather than constructing new MethodReferences by hand,
// guaranteeing the correctly-versioned assembly reference (IL-patching lesson 5 in CLAUDE.md).
MethodDefinition updateBackgroundBitmapDef = type.Methods.Single(m => m.Name == "updateBackgroundBitmap" && m.HasBody);
MethodReference? getInstanceRef = null, getActiveThemeRef = null, getHeaderEndRef = null, getBackgroundStartRef = null;
foreach (Instruction instr in updateBackgroundBitmapDef.Body.Instructions)
{
    if (instr.Operand is not MethodReference mr) continue;
    if (mr.Name == "get_Instance" && mr.DeclaringType.Name == "ThemeManager") getInstanceRef ??= mr;
    else if (mr.Name == "GetActiveTheme") getActiveThemeRef ??= mr;
    else if (mr.Name == "get_NotificationWindowHeaderEnd") getHeaderEndRef ??= mr;
    else if (mr.Name == "get_NotificationWindowBackgroundStart") getBackgroundStartRef ??= mr;
}
if (getInstanceRef is null || getActiveThemeRef is null || getHeaderEndRef is null || getBackgroundStartRef is null)
    throw new Exception($"couldn't find all four ThemeManager/IColorTheme call sites in updateBackgroundBitmap (found: instance={getInstanceRef != null} activeTheme={getActiveThemeRef != null} headerEnd={getHeaderEndRef != null} backgroundStart={getBackgroundStartRef != null})");
getInstanceRef = module.ImportReference(getInstanceRef);
getActiveThemeRef = module.ImportReference(getActiveThemeRef);
getHeaderEndRef = module.ImportReference(getHeaderEndRef);
getBackgroundStartRef = module.ImportReference(getBackgroundStartRef);

MethodReference eventHandlerCtorRef = module.ImportReference(typeof(EventHandler).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);

// --- __overlayCloseClick(object, EventArgs) / __overlaySettingsClick(object, EventArgs):
//   performMouseClick(closeRect.Location); / performMouseClick(settingsRect.Location);
MethodDefinition MakeClickForwarder(string name, FieldDefinition rectField)
{
    var m = new MethodDefinition(name, MethodAttributes.Private, module.TypeSystem.Void);
    m.Parameters.Add(new ParameterDefinition("sender", ParameterAttributes.None, module.TypeSystem.Object));
    m.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, module.ImportReference(typeof(EventArgs))));
    type.Methods.Add(m);
    ILProcessor il = m.Body.GetILProcessor();
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldflda, rectField));
    il.Append(Instruction.Create(OpCodes.Call, rectGetLocationRef));
    il.Append(Instruction.Create(OpCodes.Callvirt, performMouseClickRef));
    il.Append(Instruction.Create(OpCodes.Ret));
    return m;
}
MethodDefinition closeClickForwarder = MakeClickForwarder("__overlayCloseClick", closeRectField);
MethodDefinition settingsClickForwarder = MakeClickForwarder("__overlaySettingsClick", settingsRectField);

// --- __syncButtonOverlay(): ButtonOverlayManager.Sync(this, closeRect, closeImage,
//     closeImageOver, new EventHandler(__overlayCloseClick), settingsRect, settingsImage,
//     settingsImageOver, new EventHandler(__overlaySettingsClick),
//     ThemeManager.Instance.GetActiveTheme(this).NotificationWindowHeaderEnd,
//     ThemeManager.Instance.GetActiveTheme(this).NotificationWindowBackgroundStart);
var syncMethod = new MethodDefinition("__syncButtonOverlay", MethodAttributes.Private, module.TypeSystem.Void);
type.Methods.Add(syncMethod);
{
    ILProcessor il = syncMethod.Body.GetILProcessor();
    il.Append(Instruction.Create(OpCodes.Ldarg_0)); // notification
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, closeRectField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, closeImageField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, closeImageOverField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldftn, closeClickForwarder));
    il.Append(Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, settingsRectField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, settingsImageField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldfld, settingsImageOverField));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Ldftn, settingsClickForwarder));
    il.Append(Instruction.Create(OpCodes.Newobj, eventHandlerCtorRef));
    il.Append(Instruction.Create(OpCodes.Call, getInstanceRef));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Callvirt, getActiveThemeRef));
    il.Append(Instruction.Create(OpCodes.Callvirt, getHeaderEndRef));
    il.Append(Instruction.Create(OpCodes.Call, getInstanceRef));
    il.Append(Instruction.Create(OpCodes.Ldarg_0));
    il.Append(Instruction.Create(OpCodes.Callvirt, getActiveThemeRef));
    il.Append(Instruction.Create(OpCodes.Callvirt, getBackgroundStartRef));
    il.Append(Instruction.Create(OpCodes.Call, syncRef));
    il.Append(Instruction.Create(OpCodes.Ret));
}

// --- Insert `__syncButtonOverlay();` into ShowNotification, right after the
//     `updateLayeredBackground(refreshBitmap: true)` call --patch-notification-refresh-on-content-change
//     already inserted there (so doLayout()'s fresh rects and updateBackgroundBitmap()'s fresh
//     state are both ready). ShowNotification is straight-line code (no branches, no exception
//     handlers), confirmed when that earlier patch was written, so a plain "insert after a found
//     instruction" is safe here too.
MethodDefinition showNotificationDef = type.Methods.Single(m => m.Name == "ShowNotification" && m.HasBody);
{
    Instruction? updateLayeredBackgroundCall = showNotificationDef.Body.Instructions.FirstOrDefault(i =>
        (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) &&
        i.Operand is MethodReference mr && mr.Name == "updateLayeredBackground");
    if (updateLayeredBackgroundCall is null)
        throw new Exception("ShowNotification: couldn't find the updateLayeredBackground(bool) call (expected from --patch-notification-refresh-on-content-change)");

    // Insert BEFORE the instruction that immediately follows the anchor, in normal source order --
    // avoids IL-patching lesson 1's InsertAfter-on-the-same-fixed-anchor trap (each call would
    // land immediately after the anchor, reversing emission order relative to source order).
    Instruction insertionPoint = updateLayeredBackgroundCall.Next
        ?? throw new Exception("ShowNotification: updateLayeredBackground call has no following instruction");
    ILProcessor il = showNotificationDef.Body.GetILProcessor();
    il.InsertBefore(insertionPoint, Instruction.Create(OpCodes.Ldarg_0));
    il.InsertBefore(insertionPoint, Instruction.Create(OpCodes.Call, module.ImportReference(syncMethod)));
}

// --- Dev-only: if --patch-test-monogram-avatar's __showTestMonogramNotification is present in
//     this build (it bypasses ShowNotification entirely, calling updateLayeredBackground directly
//     instead, so it would never otherwise trigger the overlay), append a call to
//     __syncButtonOverlay() at its end so the dev test exercises this too. Optional: silently
//     skipped when absent, since most builds of this patch won't have that dev method at all.
MethodDefinition? testMonogramDef = type.Methods.FirstOrDefault(m => m.Name == "__showTestMonogramNotification" && m.HasBody);
if (testMonogramDef is not null)
{
    Instruction ret = testMonogramDef.Body.Instructions.Last();
    if (ret.OpCode != OpCodes.Ret)
        throw new Exception("__showTestMonogramNotification: expected last instruction to be Ret, method shape changed");
    ILProcessor il = testMonogramDef.Body.GetILProcessor();
    il.InsertBefore(ret, Instruction.Create(OpCodes.Ldarg_0));
    il.InsertBefore(ret, Instruction.Create(OpCodes.Call, module.ImportReference(syncMethod)));
    Console.WriteLine("Also wired __syncButtonOverlay() into __showTestMonogramNotification (dev test present in this build)");
}

// --- Insert `ButtonOverlayManager.HideAll(this);` at the very top of Hide() -- a method's own
//     first instruction is never a branch target or handler-region boundary, the same safe
//     top-of-method insertion pattern used throughout this project's other notification patches.
MethodDefinition hideDef = type.Methods.Single(m => m.Name == "Hide" && m.Parameters.Count == 0);
{
    ILProcessor il = hideDef.Body.GetILProcessor();
    Instruction first = hideDef.Body.Instructions[0];
    il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
    il.InsertBefore(first, Instruction.Create(OpCodes.Call, hideAllRef));
}

string? outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
if (outputDir != null) Directory.CreateDirectory(outputDir);
module.Write(outputPath);
Console.WriteLine($"Wrote {outputPath}: added __overlayCloseClick/__overlaySettingsClick/__syncButtonOverlay to FormGenericNotification, wired __syncButtonOverlay into ShowNotification and ButtonOverlayManager.HideAll into Hide()");
return 0;
