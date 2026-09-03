// MailClient.Wine.dll -- not loaded by eM Client, not referenced in MailClient.deps.json, has
// no code that does anything. Its sole purpose is to exist as a small file dropped alongside
// the real assemblies in the eM Client install directory, carrying this project's own patch
// version in its standard .NET/Win32 version fields, so releases/<version>/deploy.sh can tell
// which of *our* releases (if any) is currently applied to a given install -- something the
// license-fix marker (MailClient.Licensing.BouncyCastlePatch.dll) alone can't do, since it only
// answers "is Stage 5 applied", not "which release introduced the assemblies currently here".
//
// Two version fields are used, both plain numeric (no semver suffix -- AssemblyVersion in
// particular can't carry one):
//   - AssemblyVersion: the eM Client FileVersion this patch set targets, e.g. 10.4.5674.0 --
//     matches EXPECTED_FILE_VERSION in the matching deploy.sh, revision always 0.
//   - FileVersion: same three leading parts, but the 4th (revision) component is *this
//     project's own* release number against that eM Client version -- e.g. 10.4.5674.2 for the
//     release tagged release/10.4.5674-2. This is the field deploy.sh actually reads (via the
//     same `il-patcher --version` used for the eM Client version gate) to decide which stages,
//     if any, still need applying.
//
// A bottle with the OLD BouncyCastlePatch-only marker but no MailClient.Wine.dll at all predates
// this file -- deploy.sh treats that as release number 1 implicitly (release/10.4.5674, this
// project's first tagged release, before this marker existed).
namespace MailClient.Wine;

internal static class VersionMarker
{
}
