using Mono.Cecil;
using Mono.Cecil.Cil;

if (args.Length == 2 && args[0] == "--dump-method")
{
    // args: --dump-method <dll> (dumps DecryptAndVerifyString + V1 instructions/handlers)
    using var m = ModuleDefinition.ReadModule(args[1]);
    TypeDefinition t = m.GetType("MailClient.Licensing.DecryptAndVerify")!;
    foreach (string name in new[] { "DecryptAndVerifyString", "DecryptAndVerifyStringV1" })
    {
        MethodDefinition method = t.Methods.Single(mm => mm.Name == name);
        Console.WriteLine($"=== {name} ===");
        foreach (Instruction i in method.Body.Instructions)
            Console.WriteLine($"{i.Offset:X4}: {i.OpCode} {i.Operand}");
        Console.WriteLine("--- exception handlers ---");
        foreach (ExceptionHandler eh in method.Body.ExceptionHandlers)
            Console.WriteLine($"{eh.HandlerType} TryStart={eh.TryStart?.Offset:X4} TryEnd={eh.TryEnd?.Offset:X4} HandlerStart={eh.HandlerStart?.Offset:X4} HandlerEnd={eh.HandlerEnd?.Offset:X4} CatchType={eh.CatchType}");
        Console.WriteLine();
    }
    return 0;
}

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: license-oaep-patcher <input MailClient.dll> <MailClient.Licensing.BouncyCastlePatch.dll> <output MailClient.dll>");
    Console.Error.WriteLine("       license-oaep-patcher --dump-method <MailClient.dll>");
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
TypeDefinition oaepPatchType = patchModule.GetType("MailClient.Licensing.BouncyCastlePatch.OaepPatch")
    ?? throw new Exception("OaepPatch type not found in patch assembly");
MethodDefinition oaepDecryptMethod = oaepPatchType.Methods.Single(m => m.Name == "OaepSha1Decrypt");

using var module = ModuleDefinition.ReadModule(inputPath, new ReaderParameters { ReadWrite = true, AssemblyResolver = resolver });
MethodReference importedOaepDecrypt = module.ImportReference(oaepDecryptMethod);

TypeDefinition decryptAndVerifyType = module.GetType("MailClient.Licensing.DecryptAndVerify")
    ?? throw new Exception("MailClient.Licensing.DecryptAndVerify not found");

string[] targetMethodNames = { "DecryptAndVerifyString", "DecryptAndVerifyStringV1" };
int patchedCount = 0;

foreach (string methodName in targetMethodNames)
{
    MethodDefinition method = decryptAndVerifyType.Methods.Single(m => m.Name == methodName);
    ILProcessor il = method.Body.GetILProcessor();
    var instructions = method.Body.Instructions;

    Instruction? getOaepInstr = instructions.FirstOrDefault(i =>
        i.OpCode == OpCodes.Call &&
        i.Operand is MethodReference mr &&
        mr.Name == "get_OaepSHA1" &&
        mr.DeclaringType.Name == "RSAEncryptionPadding");

    if (getOaepInstr == null)
        throw new Exception($"{methodName}: could not find call to RSAEncryptionPadding::get_OaepSHA1");

    Instruction? decryptInstr = getOaepInstr.Next;
    if (decryptInstr == null ||
        decryptInstr.OpCode != OpCodes.Callvirt ||
        decryptInstr.Operand is not MethodReference decryptRef ||
        decryptRef.Name != "Decrypt" ||
        decryptRef.DeclaringType.Name != "RSA")
    {
        throw new Exception($"{methodName}: expected callvirt RSA::Decrypt immediately after get_OaepSHA1, found {decryptInstr?.OpCode} {decryptInstr?.Operand}");
    }

    // getOaepInstr is being deleted -- if anything branches to it or an exception-handler
    // region boundary points at it, retarget to decryptInstr (the next surviving instruction)
    // before removing it. See CLAUDE.md IL-patching lessons: silent corruption otherwise.
    foreach (Instruction candidate in instructions)
    {
        if (candidate.Operand == getOaepInstr)
            candidate.Operand = decryptInstr;
    }
    foreach (ExceptionHandler eh in method.Body.ExceptionHandlers)
    {
        if (eh.TryStart == getOaepInstr) eh.TryStart = decryptInstr;
        if (eh.TryEnd == getOaepInstr) eh.TryEnd = decryptInstr;
        if (eh.HandlerStart == getOaepInstr) eh.HandlerStart = decryptInstr;
        if (eh.HandlerEnd == getOaepInstr) eh.HandlerEnd = decryptInstr;
        if (eh.FilterStart == getOaepInstr) eh.FilterStart = decryptInstr;
    }

    il.Remove(getOaepInstr);
    decryptInstr.OpCode = OpCodes.Call;
    decryptInstr.Operand = importedOaepDecrypt;

    Console.WriteLine($"Patched {methodName}: removed get_OaepSHA1 call, redirected RSA::Decrypt -> OaepPatch::OaepSha1Decrypt");
    patchedCount++;
}

string? outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
if (outputDir != null) Directory.CreateDirectory(outputDir);
module.Write(outputPath);
Console.WriteLine($"Wrote {outputPath} ({patchedCount} method(s) patched)");
return 0;
