# Settings category click crash — `IApplicationAssociationRegistration.QueryAppIsDefaultAll`

> Found once the Settings panel population bug (`reports/settings-panel-clip-region-findings.md`)
> was fixed and the panel became clickable for the first time. Unrelated root cause — a Windows
> Shell COM API gap, not a paint/data bug.

## Root cause

Clicking into any Settings category whose page checks "is this the default mail client"
(`ControlSettingsGeneral`, shown by default) crashed the app. Full stack trace captured directly
from eM Client's own generated bug-report file
(`C:\users\crossover\AppData\Local\Temp\bug.<timestamp>.txt`, readable on the Linux side at
`~/.cxoffice/emClient_win_7_x64/drive_c/users/crossover/AppData/Local/Temp/bug.*.txt` — the app
writes this file even when it fails to *send* the report, which it will under Wine; the send
failure is an unrelated invalid-XML-character red herring):

```
System.NotImplementedException: The method or operation is not implemented.
  at MailClient.Utils.IApplicationAssociationRegistration.QueryAppIsDefaultAll(...)
  at MailClient.Utils.Integration.IsDefaultClientVista()
  at MailClient.Utils.Integration.IsDefaultClient()
  at MailClient.UI.Controls.SettingsControls.ControlSettingsGeneral.checkDefaultClient()
  at MailClient.UI.Controls.SettingsControls.ControlSettingsGeneral.LoadSettings()
  at MailClient.UI.Controls.SettingsControls.ControlSettingsBase.OnLoad(EventArgs)
```

`IApplicationAssociationRegistration` is a Windows Shell COM interface used to ask the OS "is
this application the default handler for mail/etc." Wine's `shell32` doesn't implement it —
matches CX_DEBUGMSG fixmes seen earlier in this investigation:
`fixme:shell:ApplicationAssociationRegistration_QueryInterface (...) interface not supported` and
`fixme:shell:ApplicationAssociationRegistration_QueryAppIsDefaultAll`. Rather than a normal COM
failure (which the .NET COM interop layer would normally surface as `COMException`), Wine's stub
throws a raw CLR `NotImplementedException` straight through the interop call.

## Why the fix is safe, not a guess

`Integration.IsDefaultClientVista()` already anticipated this class of failure:

```csharp
private static bool IsDefaultClientVista()
{
    ApplicationAssociationRegistration applicationAssociationRegistration = null;
    try
    {
        applicationAssociationRegistration = new ApplicationAssociationRegistration();
        if (!((IApplicationAssociationRegistration)applicationAssociationRegistration)
                .QueryAppIsDefaultAll(AssociationLevel.Effective, BrandingUtils.BrandName))
        {
            return false;
        }
    }
    catch (UnauthorizedAccessException innerException) { throw new IntegrationException(...); }
    catch (IOException innerException2) { throw new IntegrationException(...); }
    catch (ArgumentException innerException3) { throw new IntegrationException(...); }
    catch (COMException)
    {
        return false;   // <-- already here: "if the COM call fails, just say not-default"
    }
    finally { /* release COM object */ }
    return IsDefaultMapiClient();
}
```

The app's own authors already treat "the COM call to check default-app status failed" as a safe,
expected case to fall back from — they just didn't anticipate Wine throwing a *differently-typed*
exception (`NotImplementedException` instead of `COMException`) for that same scenario. The fix
isn't a new behavior or a guess; it's completing error handling the app already has.

## Fix

`MailClient.dll`, `Utils.Integration::IsDefaultClientVista` — add a sibling
`catch (NotImplementedException) { return false; }` handler, structurally cloned from the
existing `catch (COMException)` one (same protected `try` region, same handler body shape), via
`--patch-default-client-notimpl` in `~/tools/il-patcher`.

## Two real IL-correctness bugs hit while building this patch

Both produced a runtime `System.InvalidProgramException: Common Language Runtime detected an
invalid program` — thrown from `IsDefaultClientVista` itself — on the **first** deploy attempt,
despite the patched assembly decompiling perfectly cleanly via `ilspycmd`. Full detail and the
general lesson are in `CLAUDE.md`'s "IL-patching lessons" section; specific to this patch:

1. **Handler-table order.** The new `ExceptionHandler` was appended to the end of
   `MethodBody.ExceptionHandlers`, landing it after the method's outer `finally` handler in table
   order. The CLR requires most-nested-first ordering for overlapping protected regions. Fixed by
   inserting at the existing `catch (COMException)` handler's own list position instead of
   appending.
2. **Overlapping handler ranges.** `HandlerEnd` is an exclusive boundary — a reference to the
   instruction right after the handler's body, not a fixed offset. The new handler's body was
   inserted immediately before the COMException handler's `HandlerEnd`, which silently grew the
   *original* handler's own range to include the new code too (both handlers then claimed
   overlapping byte ranges). Fixed by explicitly reassigning `comHandler.HandlerEnd` to the new
   handler's first instruction before adding the new `ExceptionHandler` entry.

Neither bug was visible in the decompiled C# (ilspycmd doesn't validate handler-region bounds or
ordering) — caught only by building `--dump-handlers`, a new verification mode that prints a
method's exception-handler table with resolved instruction offsets and explicitly checks the
nesting-order invariant. Confirmed clean, non-overlapping, correctly-ordered ranges before the
second (successful) deploy.

## Confirmed working

User confirmed: could move between all Settings categories and change/save settings with no
further crash.
