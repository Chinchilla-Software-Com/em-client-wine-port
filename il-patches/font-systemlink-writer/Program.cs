// Registers a set of vendored TrueType fonts (Segoe UI family, Tahoma, Calibri -- copied into
// the bottle's Fonts folder by releases/<version>/deploy.sh before this runs) in a Wine bottle's
// registry, and adds FontLink\SystemLink fallback entries so text using Tahoma/Segoe UI falls
// back to Segoe UI Symbol/Segoe UI Emoji for glyphs those base fonts don't cover -- matching how
// real Windows resolves emoji inside normal UI text. Run inside the target bottle with its own
// installed .NET runtime (published as a win-x86 apphost .exe, invoked via
// `wine <path>\font-systemlink-writer.exe`).
//
// Everything here goes through the real Win32 registry API (Microsoft.Win32.Registry), not a
// `.reg` file import, specifically to work around a `wine regedit /S <file.reg>` bug found while
// investigating reports/splash-tip-icon-findings.md: this Wine build's REG_MULTI_SZ import
// parser mangles multi-string values regardless of .reg syntax used (hex(7) raw bytes and str(7)
// quoted-text-with-\0-escapes both split the intended fallback-font list into one bogus
// single-character "entry" per UTF-16 code unit -- confirmed via CX_DEBUGMSG=+font trace).
// Writing through Registry.SetValue's real Win32 code path sidesteps the broken .reg importer
// entirely (confirmed correct by reading values back, and independently by re-tracing Wine's own
// font-linking code after this runs). The plain string Fonts-key entries below aren't actually
// affected by that bug (it's specific to REG_MULTI_SZ) but are written the same way for a single
// reliable code path instead of mixing this tool with a separate `regedit` import step.
//
// Known effect: makes genuine Segoe UI/Tahoma available for font resolution and may improve
// general text rendering fidelity elsewhere in the app. Does NOT fix the splash-tip emoji-glyph
// tofu-box bug specifically -- confirmed via trace that Wine loads the SystemLink configuration
// correctly but its actual glyph-shaping/ExtTextOut code doesn't act on it (a deeper Wine gap,
// out of scope here). That bug is fixed unconditionally by --patch-splash-tip-icon (a resource
// string patch in il-patcher) regardless of whether this runs.
using Microsoft.Win32;

// (registry font value name, filename) -- "(TrueType)" suffix matches Windows' own convention.
(string Name, string File)[] fontEntries =
[
    ("Calibri (TrueType)", "calibri.ttf"),
    ("Calibri Bold (TrueType)", "calibrib.ttf"),
    ("Calibri Italic (TrueType)", "calibrii.ttf"),
    ("Calibri Bold Italic (TrueType)", "calibriz.ttf"),
    ("Calibri Light (TrueType)", "calibril.ttf"),
    ("Calibri Light Italic (TrueType)", "calibrili.ttf"),
    ("Segoe MDL2 Assets (TrueType)", "segmdl2.ttf"),
    ("Segoe Fluent Icons (TrueType)", "SegoeIcons.ttf"),
    ("Segoe Print (TrueType)", "segoepr.ttf"),
    ("Segoe Print Bold (TrueType)", "segoeprb.ttf"),
    ("Segoe Script (TrueType)", "segoesc.ttf"),
    ("Segoe Script Bold (TrueType)", "segoescb.ttf"),
    ("Segoe UI (TrueType)", "segoeui.ttf"),
    ("Segoe UI Bold (TrueType)", "segoeuib.ttf"),
    ("Segoe UI Italic (TrueType)", "segoeuii.ttf"),
    ("Segoe UI Bold Italic (TrueType)", "segoeuiz.ttf"),
    ("Segoe UI Light (TrueType)", "segoeuil.ttf"),
    ("Segoe UI Light Italic (TrueType)", "seguili.ttf"),
    ("Segoe UI Semilight (TrueType)", "segoeuisl.ttf"),
    ("Segoe UI Semilight Italic (TrueType)", "seguisli.ttf"),
    ("Segoe UI Semibold (TrueType)", "seguisb.ttf"),
    ("Segoe UI Semibold Italic (TrueType)", "seguisbi.ttf"),
    ("Segoe UI Black (TrueType)", "seguibl.ttf"),
    ("Segoe UI Black Italic (TrueType)", "seguibli.ttf"),
    ("Segoe UI Emoji (TrueType)", "seguiemj.ttf"),
    ("Segoe UI Historic (TrueType)", "seguihis.ttf"),
    ("Segoe UI Symbol (TrueType)", "seguisym.ttf"),
    ("Segoe UI Variable (TrueType)", "SegUIVar.ttf"),
    ("Tahoma (TrueType)", "tahoma.ttf"),
    ("Tahoma Bold (TrueType)", "tahomabd.ttf"),
];

const string fontsKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
foreach (var (name, file) in fontEntries)
{
    Registry.SetValue(fontsKey, name, file, RegistryValueKind.String);
}
Console.WriteLine($"registered {fontEntries.Length} font(s) in {fontsKey}");

string[] systemLinkTargets = ["Tahoma", "Segoe UI"];
string[] fallbacks = ["seguisym.ttf,Segoe UI Symbol", "seguiemj.ttf,Segoe UI Emoji"];

const string systemLinkKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontLink\SystemLink";
foreach (var target in systemLinkTargets)
{
    Registry.SetValue(systemLinkKey, target, fallbacks, RegistryValueKind.MultiString);
    Console.WriteLine($"set SystemLink for {target}");
}

// read back to verify
using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontLink\SystemLink");
foreach (var target in systemLinkTargets)
{
    var val = key?.GetValue(target) as string[];
    Console.WriteLine($"{target}: [{string.Join(" | ", val ?? [])}]");
}
