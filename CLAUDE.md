# MailClient — CrossOver stabilization (not a native Linux port)

- original/**      — untouched, read-only. Drop a fresh eM Client install here (overwrite
                      wholesale) when the app updates; everything below regenerates from it.
- decompiled/**     — ilspycmd -p output. If it rebuilds clean, edit here, swap the DLL in.
                      Not used by any fix so far — every fix to date has gone through the IL
                      patcher instead (see "Patch pipeline" below), since none of the touched
                      assemblies (MailClient.dll, MailClient.Common.UI.dll) round-trip cleanly
                      through ilspycmd -p at their size.
- il-patches/**      — ~/tools/il-patcher (Cecil-based patcher/scanner) plus this project's
                      patch specs and a synced copy of the tool's own source
                      (il-patches/il-patcher-Program.cs — copy back to
                      ~/tools/il-patcher/Program.cs and `dotnet build -c Release` before use;
                      the ~/tools/ copy is gitignored, this copy is the tracked source of truth).
                      A second, smaller Cecil tool lives alongside it for the license-activation
                      fix (Stage 5 below): il-patches/license-oaep-patcher-Program.cs (copy to
                      ~/tools/license-oaep-patcher/Program.cs; console app, net10.0, references
                      Mono.Cecil 0.11.6 — same shape as il-patcher's own untracked .csproj) and
                      il-patches/license-oaep-patcher-patch-deps-json.py (copy anywhere, it just
                      needs a path argument). It's a separate tool rather than a new il-patcher
                      mode because this fix's shape — redirecting a call to a newly-added sibling
                      assembly — doesn't fit the existing tool's per-fix flag pattern as cleanly.
                      il-patches/MailClient.Licensing.BouncyCastlePatch/ is the full tracked
                      source (OaepPatch.cs + its .csproj) of the new helper assembly Stage 5
                      builds and deploys — this one *is* shipped into the bottle, unlike the
                      patcher tools, so it's tracked in full rather than as a "copy back" source
                      file. Its .csproj HintPaths BouncyCastle.Cryptography.dll from
                      `original/` by absolute path — adjust if building from a different checkout
                      location.
                      il-patches/font-systemlink-writer/ (tracked in full, same reason as the
                      BouncyCastlePatch helper — it's built and run against a live bottle, not
                      just a dev-time tool) registers fonts/*.ttf in a target bottle's registry
                      and adds Wine FontLink\SystemLink fallback entries, via the real Win32
                      registry API rather than a `.reg` file import — `wine regedit /S` has a
                      confirmed multi-string-value import bug in this Wine build (see
                      reports/splash-tip-icon-findings.md). Optional, wired into
                      releases/<version>/deploy.sh's `--install-fonts` step; not part of the core
                      IL-patch pipeline and not required by any of it.
                      il-patches/notification-repro-stub/ (tracked in full, same reason as
                      font-systemlink-writer/ above) is a minimal standalone WinForms app
                      replicating FormGenericNotification's layered-window mechanism piece by
                      piece, built to test hypotheses about the notification empty-box bug (see
                      Status below and reports/notification-empty-until-fade-findings.md) far
                      faster than patching MailClient.dll via Cecil for every experiment — normal
                      C# source, `dotnet publish` + `wine <exe>`, no IL patching involved. Not
                      wired into deploy.sh or any pipeline; a standalone diagnostic tool, run
                      manually. Five iterations tried against the real bug, none reproduced it —
                      see the findings report for what that ruled out.
                      il-patches/output*/ and il-patches/backup/ are gitignored build output —
                      regenerate via the pipeline below, don't hand-edit or commit them.
- reports/crossover-backlog.json — one entry per CONFIRMED freeze point
                                    (cxlog + VS thread stacks), not a static API scan.
                                    Not populated yet — the two bugs fixed so far were found via
                                    the investigation method below, not a pre-existing backlog.
- reports/cxlog.txt — CX_DEBUGMSG trace, most recent run. Large (100–250MB); gitignored.
  reports/cxlog-*.txt.bak — earlier trace runs kept during investigation, also gitignored.
- reports/*-findings.md — one file per confirmed bug, each documenting root cause, the fix, and
  (importantly) fix attempts that turned out to be wrong and why, so the same dead end isn't
  re-explored next time. Read these before starting new investigation.
- supporting/** — reference screenshots (both "broken" and "known-good/working" states) used to
  confirm fixes visually. Tracked in git; add new ones here when reporting or confirming a bug.
- fonts/** — genuine Microsoft TrueType fonts (Segoe UI family, Segoe UI Emoji/Symbol, Segoe
  MDL2 Assets/Fluent Icons, Tahoma, Calibri — 30 files) the app expects but the bottle doesn't
  ship. Tracked in git on the basis that whoever adds files here holds a valid license to use
  them (these are not open-licensed — don't add font files here without one). Installed into a
  bottle only on explicit opt-in — see `releases/<version>/deploy.sh --install-fonts` and
  `il-patches/font-systemlink-writer/` above. Known to improve general font availability/fidelity
  but confirmed NOT sufficient by itself to fix Wine's emoji-glyph rendering gap — see
  `reports/splash-tip-icon-findings.md`.
- file-associations/** — `.reg` files fixing attachment types eM Client can't launch an external
  viewer for because a fresh CrossOver bottle has no OS-level file association for them (not an
  eM Client bug — its own attachment-open code faithfully mirrors Explorer's own mechanism; see
  `reports/office-file-associations-findings.md`). `office-associations.reg` covers MS Office
  (Word/Excel/PowerPoint/Visio/Publisher + CSV) and OpenDocument formats;
  `common-attachments.reg` covers images/archives/audio/video plus `.json`/`.md`. Every extension
  gets its own `EMClientWinePort.<ext>` ProgID pointed at the same `winebrowser.exe` handler
  already proven working for `.pdf`, deliberately namespaced so these entries can never collide
  with a real Office/LibreOffice install added to the bottle later. `parse-reg-associations.py`
  splits each file into per-extension blocks so `releases/<version>/deploy.sh` can import only
  the extensions actually missing an association on the target bottle (see its own bullet below)
  — the `.reg` files are also valid to import as-is by hand
  (`wine regedit /S file-associations/<file>.reg`) if you want every entry unconditionally.
- releases/<version>/deploy.sh — the actual, practical way to deploy: a single self-contained
  script that finds installed eM Client bottles, reads and reports each one's version, warns (and
  asks) before proceeding against a version other than the one this release was built and tested
  against, backs up the live files, regenerates every patch stage fresh from whatever assemblies
  it actually found (never copies pre-built DLLs out of this repo — an update-prone app makes a
  stale pre-built copy actively dangerous, not just inconvenient), runs every "Verify before
  deploying" check from the "Patch pipeline" section below automatically, and only then deploys —
  rolling back (renaming whatever it wrote to `.new`, restoring the backup) if any step from that
  point on fails. `./deploy.sh --list` reports found bottles + versions without patching anything;
  `./deploy.sh --bottle NAME` skips interactive bottle selection; `-y`/`--yes` skips the
  version-mismatch confirmation prompt; `--install-fonts`/`--no-fonts` answer the fonts/ license
  consent prompt (see below) non-interactively, for scripted/repeated runs. Safe to re-run —
  checks whether the target is already patched (an assembly-reference marker, not file size —
  see `il-patcher --check-patched`'s doc comment) and exits cleanly if so; `--force` skips that.
  Note this short-circuit only skips the DLL patch pipeline itself — fonts and file-type
  associations (below) are independent of it and always still run, since most bottles this
  script targets will already be patched from a previous run. `--install-associations`/
  `--no-associations` answer the file-associations/ prompt non-interactively;
  `--force-associations` also overwrites extensions that already have some association set (see
  file-associations/** above and `reports/office-file-associations-findings.md`) — by default
  only extensions with no existing association are touched, checked individually against the
  target bottle's live registry, so a bottle with a real Office/LibreOffice install isn't
  disturbed.
  Needs dotnet SDK (prints per-distro install instructions and exits if missing) and python3;
  fetches ilspycmd itself into a temp dir if not already installed. **When eM Client updates:** don't edit an existing `releases/<version>/deploy.sh` in
  place — copy the whole `releases/<version>/` folder to a new `releases/<new-version>/`, retest
  by hand against the new build (same process as "Investigation method" below — diff what
  actually changed rather than assuming), and adjust whatever patch logic broke in the new
  folder's copy. Each version's script stays a frozen, working reference for that version.
  `releases/*/backups/` (gitignored) is where each deploy run's pre-patch backup lands.

Main bottle: emClient_win_7_x64. Exe: MailClient.exe. Bottle path:
`~/.cxoffice/emClient_win_7_x64/`; `Z:\` inside the bottle maps to `/` on the Linux side, which
is how diagnostic instrumentation (see below) writes logs readable directly from outside Wine.
A secondary bottle, `emClient_win_7_x64_2` (same layout, same drive_c path shape), exists for
testing changes that would disturb the main bottle's state — e.g. the License Activation fix was
tested there once the main bottle had already been activated, to keep a never-activated instance
available for any future licensing-related testing without needing yet another fresh bottle.
Deploy commands need the `$BOTTLE` path swapped accordingly when testing there instead of main
(or use `releases/<version>/deploy.sh --bottle NAME`, which handles this automatically).

**Multi-OS testing:** additional bottles per Windows version the installer might target
differently — `emClient_win_8_x64`, `emClient_win_10_x64`, and `emClient_win_11_x64`, all
confirmed working with the full deploy.sh pipeline (Windows 10/11 run the Microsoft Store variant
of the app, with no classic License menu entry — licensing there is presumably handled via
`MicrosoftStoreLicenseSource` instead, not yet investigated). `original/` is organized by full eM
Client version string, one subfolder per version tested against — currently
`original/em-10.4.5674/` and `original/em-11.0.196/` (the latter is the separate 11.0.196-beta
release line, see below) — **not** by target Windows OS version: an early investigation
confirmed the installer ships byte-for-byte identical payloads regardless of target OS version
(2408 files, zero diffs, matching hashes on every touched assembly, checked across two OS
versions), so a single snapshot per eM Client version is sufficient. `original/v1/` also exists
in this repo and predates the current naming convention — it's actually another eM Client
10.4.5674 snapshot despite the name, not a distinct version; ignore it, `original/em-10.4.5674/`
is the current canonical one. `deploy.sh` itself never reads from `original/` at runtime — it
always copies fresh from whatever's actually installed in the target bottle (see its own header
comment) — these snapshots exist purely as a read-only reference for diffing/investigation.

**Primary testing bottle shifted permanently from `emClient_win_7_x64` to `emClient_win_8_x64`**
(already set up with test data) — use bottle 8 for new investigation/testing going forward unless
there's a specific reason to use another bottle (e.g. `emClient_win_7_x64_2`'s never-activated
license state, kept available for licensing-related testing).

## eM Client 11 (beta) — separate release line

Everything above (`original/em-10.4.5674/`, `releases/10.4.5674/`, the "Status" section below) is
for eM Client **10.4.5674**. eM Client **11.0.196-beta** is a genuinely different, still-changing
product build (new assembly set, targets .NET 10 instead of .NET 8, different bugs) tracked
completely separately, on purpose — never fold its fixes into `releases/10.4.5674/deploy.sh` or
its Status section, and never run one version's `deploy.sh` against the other's bottle (each
script's own version gate warns and asks before proceeding against an unexpected `FileVersion`,
but don't rely on that alone).

- **Bottle:** `emClient_11_beta_win_11` (`~/.cxoffice/emClient_11_beta_win_11/`), same classic
  install path shape as the v10 bottles (`drive_c/Program Files (x86)/eM Client/MailClient.dll`).
- **`original/em-11.0.196/`** — the pristine reference snapshot for this exact build
  (`MailClient.dll` FileVersion `11.0.196.0`, InformationalVersion
  `11.0.196-beta+b945a075a5`), same `original/em-<version>/` naming convention as v10's own
  `original/em-10.4.5674/` (see the naming-convention note above). `original/em-11.0.282/`
  (FileVersion `11.0.282.0`, InformationalVersion `11.0.282-beta+ff9ca141f2`) is the equivalent
  snapshot for the newer build now also supported by `releases/11.0.196-beta/deploy.sh` (see that
  script's own bullet below) — both snapshots are kept side by side specifically so future
  eM Client updates can be decompile-diffed against either. `original/em-11.0.282-x64/` is the
  **x64** package of that same `11.0.282` build (extracted from the same `.msixbundle` the x86
  snapshot came from — one bundle contains a `.msix` per architecture, see
  `install-msix.sh`'s own bullet below) — 2492 files vs. the x86 package's 2493, kept
  specifically to support the x64-compatibility verification documented there. `original/v1/` also exists in
  this repo and is **not** related to eM Client 11 despite the name — it's an old, differently-
  named snapshot of eM Client 10.4.5674, a leftover from before the current convention was
  adopted; ignore it.
- **`releases/11.0.196-beta/install-msix.sh`** — eM Client 11 ships as an MSIX package (Microsoft
  Store-style), which CrossOver/Wine can't install directly the way `deploy.sh` assumes something
  already is — there's no classic installer to run inside the bottle. This script gets a clean,
  unpatched install into place by hand, entirely from network downloads (all into a fresh `/tmp`
  workdir, cleaned up on success): downloads both the x64 and x86 .NET 10 desktop runtime
  installers (`windowsdesktop-runtime-10.0.11-win-{x64,x86}.exe`) and silently installs them into
  the chosen bottle (`wine <exe> /install /quiet /norestart`, the documented unattended-install
  invocation for Microsoft's Burn-based bootstrapper installers); downloads and parses the app's
  own update feed, `https://licensing.emclient.com/api/update/emclient.appinstaller?beta=true` (a
  small XML manifest whose `<MainBundle Uri="...">` attribute is the *actual* current download
  URL — resolved via `python3`'s `xml.etree.ElementTree`, not hardcoded, since it changes with
  every release) to find and download the current `.msixbundle`; extracts the selected
  architecture's `.msix` (`--arch x86|x64`, default `x86` — every previously-tested install; see
  below) from the bundle (a plain zip containing one `.msix` per architecture — confirmed via
  `unzip -l` against a real bundle: `MailClient_win-{x86,x64,arm64}.msix` plus bundle-level Appx
  metadata) and extracts THAT (also a plain zip, flat layout, no VFS redirection folder —
  confirmed by inspection, and by matching `original/em-11.0.196/`'s own file count exactly,
  2493 files both ways, and `original/em-11.0.282-x64/`'s the same way, 2492 files both ways)
  directly into the bottle's classic per-architecture install location —
  `drive_c/Program Files (x86)/eM Client/` for x86 (matching real Windows' own convention for a
  32-bit app), plain `drive_c/Program Files/eM Client/` for x64; generates a Start Menu shortcut
  fresh via `cscript.exe` + `WScript.Shell`'s `CreateShortcut`, pointed at whichever install
  location was just used — the same real Windows Shell API a native installer would use
  (`cscript.exe`/`wscript.exe` are genuine Wine builtins, confirmed present). This used to copy a
  pre-built, tracked `eM Client.lnk` for x86 and only generate one fresh for x64 (added when x64
  support was first wired in) — unified onto `cscript` for both and the tracked `.lnk` deleted,
  since a plain byte-level path substitution on it was never safe anyway ("Program Files (x86)"
  and "Program Files" are different lengths, and a `.lnk`'s shell-item-ID-list encodes
  lengths/offsets elsewhere in the binary that a substitution wouldn't update) and there's no
  reason to maintain two code paths (plus a tracked binary to keep in sync with wherever this
  script actually installs to) when one already covers both architectures correctly; and runs
  `cxmenu --sync --bottle <name>` to pick up the new shortcut without a manual "Install
  Application into Bottle" pass.
  **x64 support confirmed working, not just plumbed through**: a full decompile-diff plus a dry
  run of every `deploy.sh` patch stage against `original/em-11.0.282-x64/` showed
  `MailClient.Common.UI.dll`/`MailClient.Accounts.dll` are genuinely AnyCPU (byte-for-byte
  identical to the x86 build) and `MailClient.dll` — architecture-specific at the PE level (a
  real x86 vs. AMD64 machine-type difference, confirmed via each file's own PE header) —
  decompiles byte-for-byte identical anyway, so the patch content itself needs no x64-specific
  adaptation at all. `deploy.sh`'s own bottle-discovery loop was updated to check both
  architectures' install paths (see its own bullet below); everything downstream already derives
  its paths from whichever install it actually finds, so no other part of that pipeline needed to
  change. Bottle selection scans
  every bottle under `~/.cxoffice/` (not filtered to ones that already have eM Client, unlike
  `deploy.sh` — the point here is installing into one that doesn't yet) and warns if the chosen
  one's own `cxbottle.conf` `"Template"` setting isn't `win11_*` (confirmed reliable across every
  bottle checked — `win7_64`/`win8_64`/`win10_64`/`win11_64`), since Windows 11 is the only target
  confirmed working so far.
  Right after the MSIX extraction, it also downloads and installs a set of **ICU DLLs** Wine
  doesn't provide but eM Client's spell-checker needs — see
  `reports/emclient11-icu-spellcheck-crash-findings.md` for the full investigation (a
  `DllNotFoundException` crash on essentially every keystroke in a compose window; the fix is a
  fork of the official `unicode-org/icu` built with unversioned symbol names, from
  https://github.com/FaithLife-Community/icu, downloaded fresh each run — not vendored in this
  repo — the same way the .NET runtimes and the msixbundle itself are). This is a missing
  **OS-level** dependency, not an eM Client bug, so it's provisioned here at install time rather
  than as a `deploy.sh` IL-patch stage — no release-number bump, no `MailClient.Wine.dll`
  revision for it.
  At the end, it offers to run `deploy.sh` against the same bottle immediately (prompted, or
  automatic under `-y`) rather than just printing the command as a manual next step. Does
  **not** run `deploy.sh`'s own patch pipeline unless the user says yes to that prompt — it
  remains a logically separate, optional step (same "always regenerate fresh from whatever's
  actually installed" design works identically regardless of how that install got there). The
  XML-parsing, MSIX-entry-detection, and ICU-provenance-verification logic were all validated
  directly against real reference files/APIs before being wired into the script; the ICU fix
  itself was additionally confirmed working live (deployed by hand first, then folded into the
  script) — drafting an email with a deliberate misspelling now shows a normal spell-check
  underline instead of crashing. The script as a whole has not yet been run start-to-finish in
  one pass (its pieces were built and verified incrementally against an already-partially-set-up
  bottle).
- **`releases/11.0.196-beta/deploy.sh`** — this version's own deploy script, same overall shape
  (bottle discovery, running-instance check, version gate, backup, verify, deploy-with-rollback,
  a `MailClient.Wine.dll` revision marker) as `releases/10.4.5674/deploy.sh` but much smaller —
  two DLL-patch stages so far. Deliberately does not share the file-associations step (unrelated
  to anything fixed here yet) or the license-OAEP BouncyCastle helper (neither DLL fix so far
  needs a new sibling assembly). The `MailClient.Wine.dll` marker is built from the exact same
  tracked source as v10's (`il-patches/MailClient.Wine/VersionMarker.cs`), just with this line's
  own version numbers passed at build time — `AssemblyVersion=11.0.196.0` (this eM Client build),
  `FileVersion=11.0.196.<our-release-number>` — so the same leading-three-components/trailing-
  revision convention holds for both release lines without any code changes to the marker itself.
  Fonts (same vendored `fonts/*.ttf` and same `--install-fonts`/`--no-fonts` prompt-or-flag
  pattern as the v10 pipeline) ARE shared with v10 — reuses `il-patches/font-systemlink-writer/`'s
  `Program.cs` verbatim, but builds it with its OWN inline csproj targeting **net10.0**, not the
  shared file's net8.0: this bottle only has the .NET 10 desktop runtime installed (matching eM
  Client 11's own target), so a net8.0 framework-dependent apphost would fail to launch here.
  Independent of the DLL-patch pipeline (runs regardless of which stages applied this time), same
  as v10. Verified live: fonts land in the bottle's `windows/Fonts`, registry SystemLink entries
  read back correctly, `--no-fonts` skips cleanly on a re-run.
  **Now supports multiple eM Client builds** (`11.0.196` and `11.0.282` so far) from this SAME
  script — a deliberate exception to the general "copy the whole `releases/<version>/` folder to
  a new one" convention stated above: when eM Client updated to `11.0.282`, a full decompile-diff
  plus a dry run of every stage against a fresh `11.0.282` install confirmed the patch content is
  completely unaffected (every touched type/method — `AccountManager`, `MailClient.Storage
  .Application.Folder`, `FormMailNotification`, `ControlToolStripButton`, the PBKDF2 call-site
  count, the splash-tip resource bytes — decompiles byte-for-byte identical to `11.0.196`; only
  `LayeredForm` differs at all, losing one unused convenience overload unrelated to the patched
  `WndProc`). Since nothing needed adapting, forking into a second folder would have been pure
  duplication — added `11.0.282` to this script's own `SUPPORTED_EMCLIENT_VERSIONS`-equivalent
  map instead. See `reports/emclient11-startup-stack-overflow-findings.md` for the verification
  details. If a future eM Client build ever DOES need different patch logic, that's the signal to
  go back to forking a new `releases/<version>/` folder — this shared-script approach only holds
  as long as the patch content stays identical across versions.
  This introduced a real split worth understanding before touching the script again:
  `OUR_RELEASE_NUMBER_FOR_VERSION` (an associative array, e.g. `["11.0.196"]=4 ["11.0.282"]=1`)
  is a PER-VERSION release-tag number, used only for `release/<version>-<N>` git tags and
  human-facing log messages — `11.0.282` started its own count at 1 (now at 2) even though the
  pipeline itself was already at stage 4 (now the mandatory pipeline's stage 3, plus optional
  stage 5 — see the Status section's `release/11.0.282-2` entry below for the Stage 4/5
  renumbering), because it's the first tagged release for that specific eM Client build, not
  because fewer stages apply to it. `PIPELINE_LATEST_STAGE` (a plain constant, currently `3` —
  the highest MANDATORY stage; optional stages, currently just Stage 5, are a separate opt-in on
  top of it) is the SEPARATE, version-agnostic axis that actually controls how many patch stages
  get built by default and what the on-bottle `MailClient.Wine.dll` marker's own revision number
  means (shared across every supported version, since the patch content doesn't vary by
  version). Conflating these two would be a real bug, not just a naming nitpick: if the
  per-version release number were used to decide how many stages to apply, a fresh `11.0.282`
  deploy would stop after Stage 1 only (`OUR_RELEASE_NUMBER_FOR_VERSION["11.0.282"]` = 2) instead
  of the mandatory three. Add a new eM Client build to `OUR_RELEASE_NUMBER_FOR_VERSION` only after
  doing the same decompile-diff-plus-dry-run verification described above — never on faith.

  **x64 support**: bottle discovery now checks both architectures' conventional install paths
  (`drive_c/Program Files (x86)/eM Client/` and plain `drive_c/Program Files/eM Client/`) rather
  than hardcoding the x86 one — everything downstream (`BOTTLE_APP_DIR`, the version gate, every
  patch stage, backup/deploy) already derives its own paths from whichever `MailClient.dll` was
  actually selected, so no other part of this script needed to change. Verified via a full
  decompile-diff plus a dry run of every stage against `original/em-11.0.282-x64/` (see
  `install-msix.sh`'s own bullet above for the file-level findings) — the patch content applies
  identically regardless of architecture.
- **Status:** four DLL fixes so far, plus fonts, plus an install-time OS-dependency fix.
  - `release/11.0.196-1` — a startup crash (PBKDF2 key derivation broken under Wine's
    `bcrypt.dll`, blocking `InitOnBackground` before the main window ever appears). Full
    root-cause and fix details: `reports/emclient11-pbkdf2-startup-crash-findings.md`. Fixed via
    a NEW il-patcher flag, `--patch-pbkdf2-instance-api` (Stage 1), which scans every
    `MailClient*.dll` for the exact broken call shape rather than hardcoding one type/method (a
    second, independent call site turned up only after the first was fixed — see the findings
    report).
  - `release/11.0.196-2` — splash screen tip label tofu boxes (Stage 2), the **exact same bug**
    as the already-fixed v10 issue (`reports/splash-tip-icon-findings.md`) — confirmed identical
    root cause, not just a similar-looking symptom (same resource path, same exact
    `F0 9F 92 A1` emoji bytes, same control, same mechanism — see
    `reports/emclient11-splash-tip-icon-findings.md`). Fixed by reusing the existing
    `--patch-splash-tip-icon` flag completely **unmodified** — no new patch code needed, just
    wired into this release line's own Stage 2. Visually confirmed working by the user.
  - `release/11.0.196-3` — new-mail notification toast empty until fade (Stage 3, mirroring v10's
    Stages 8–14 as one combined stage here), the **exact same bug and same 7-part fix** as the
    already-fixed v10 issue (`reports/notification-empty-until-fade-findings.md`) — confirmed via
    decompile diff before porting anything (5 of 7 sub-flags apply completely unmodified; 2
    needed real adaptation: `--patch-notification-hover-forward` for `LayeredForm` moving
    assemblies, `--patch-notification-toolbar-icons` for `FormMailNotification`'s buttons being
    redesigned from 3 fixed named buttons to 5 dynamic ones). Both adapted flags were also made
    auto-detecting (work unmodified against EITHER eM Client version's assembly shape, not just
    11.0.196-beta), and this work caught two previously-latent bugs in `il-patcher` itself (a
    cross-module `MethodReference` import bug, and a `Directory.GetFiles` enumeration-order bug
    that let one patched file silently get clobbered by a later plain copy) — see IL-patching
    lessons 16–17 above and `reports/emclient11-notification-empty-until-fade-findings.md` for
    the full story. Verified via decompile + `--dump-handlers` at every stage and a full
    fresh-from-pristine chain re-run, then deployed live to `emClient_11_beta_win_11` and
    **confirmed working visually by the user**.
  - **ICU DLLs** (not a numbered release — install-time only, no `MailClient.Wine.dll` revision
    bump). Spell-check crashed with `DllNotFoundException: icuuc.dll` on essentially every
    keystroke in a compose window — a missing OS-level dependency real Windows 10 1703+ ships
    itself but Wine implements none of. Fixed by downloading and installing a self-contained
    ICU4C build with unversioned symbols (from
    https://github.com/FaithLife-Community/icu, a verified fork of the official
    `unicode-org/icu`) via `install-msix.sh`, fresh each run, not vendored in this repo. Full
    story, including a dead-end tried first (a forwarder-only `icuuc.dll` found online that
    pointed at Windows' own built-in `icu.dll`, which Wine also doesn't have): see
    `reports/emclient11-icu-spellcheck-crash-findings.md`. Confirmed working live.
  - **Startup crash + font fixes** (not a numbered release — install-time only, no
    `MailClient.Wine.dll` revision bump; full story:
    `reports/emclient11-startup-stack-overflow-findings.md`). Three separate confirmed root
    causes, all fixed and all provisioned in `install-msix.sh` (not `deploy.sh` — none of these
    touch a `MailClient*.dll` assembly): (1) a Wine WinRT stub (`IUISettings2
    ::get_TextScaleFactor`) causing Chromium-side unbounded retry recursion — worked around via a
    `windows.ui` DLL override; (2) a second, unrelated `libcef.dll`-internal recursion triggered
    by a missing sans-serif fallback font — worked around by generating a `sans.ttf` (see
    `make-sans-fallback-font.py`) from whatever real font the bottle already has and registering
    it as Chromium's `"sans"` fallback; (3) the message preview-pane header rendering in the
    wrong font (Tahoma instead of Segoe UI) — root-caused via decompile to
    `SystemFonts.MessageBoxFont` (which eM Client's own `FontManager.UIFont` uses) reading a
    missing `HKCU\Control Panel\Desktop\WindowMetrics\MessageFont` binary `LOGFONTW` registry
    value; fixed by writing a correct 92-byte LOGFONT blob for Segoe UI 9pt. Also installs Aptos
    and Roboto (both real, separate missing-font gaps found along the way in the Chromium/
    DirectWrite HTML-body rendering path, confirmed via trace, kept even though neither was the
    actual cause of (3) — see the report's "Bug C" section for the full account of several wrong
    turns before finding the real fix). All three fixes plus both extra font families confirmed
    working live and visually confirmed by the user.
  - `release/11.0.196-4` — severe, recurring multi-minute freezes during Exchange sync (Stage 4),
    the **exact same bug and same fix** as the already-fixed v10 issue (Stage 15,
    `reports/exchange-sync-freeze-findings.md`) — confirmed via decompile diff before porting
    anything: `AccountManager.SendAndReceiveAll` and `Folder.Synchronize(bool,bool)` are
    byte-for-byte structurally identical between the two eM Client versions, still fully
    synchronous, still reachable from the UI thread. Both `--patch-account-manager-sync-async`
    and `--patch-folder-sync-async` applied completely **unmodified** — no adaptation needed at
    all (unlike release-3's two adapted sub-flags). Verified via decompile + `--dump-handlers`,
    then deployed live to `emClient_11_beta_win_11` (correctly resumed from revision 3 straight
    to Stage 4, then a second run correctly skipped the whole pipeline at revision 4) — not yet
    visually/behaviorally confirmed by the user (pending a real Exchange freeze scenario to test
    against).
  - `release/11.0.282-1` — first tagged release confirming this same `deploy.sh` (all four
    stages, unmodified) also works against eM Client build `11.0.282` — see this file's own
    `deploy.sh` bullet above for the verification method and the per-version release-number
    split this introduced. Tested and confirmed working by the user on a separate machine.
  - `release/11.0.282-2` — Stage 4 (Exchange sync-freeze fix) renumbered to Stage 5 and made
    OPTIONAL (prompted, default skip; `--enable-sync-freeze-fix`/`--skip-sync-freeze-fix` for
    non-interactive use) after it turned out to behave differently across machines — Stage 4
    itself is now a deliberately vacant slot reserved for the next patch that IS safe to apply
    unconditionally (bump `PIPELINE_LATEST_STAGE` to 4 and take that slot when one's ready; shift
    every still-optional stage up by one again, same as this shuffle). A bottle already marked
    revision 4 under the old numbering is transparently remapped to revision 5 (same content,
    just a renumbering) rather than erroring. Also: `install-msix.sh`'s bottle-creation step now
    disables window-manager decorations (`HKCU\Software\Wine\X11 Driver\Decorated` -> `N`, the
    same setting winecfg's Graphics tab exposes) on every freshly created bottle. Verified: `--list`
    and a `--skip-sync-freeze-fix` run against the live `emClient_11_beta_win_11` bottle (at
    revision 3) both behave correctly; an `--enable-sync-freeze-fix` run was also verified to
    correctly reach and apply Stage 5 (deployed for real during testing, then rolled back via
    `deploy.sh`'s own pre-deploy backup since it wasn't an intentional deploy).
  - `release/11.0.282-3` — wired up x64 support in both scripts: `install-msix.sh` gained an
    `--arch x86|x64` flag (default `x86`, unchanged behavior), and `deploy.sh`'s bottle-discovery
    loop now checks both architectures' conventional install paths (see both scripts' own bullets
    above for the verification and the shortcut-generation unification this also introduced —
    `cscript.exe` for both architectures now, the tracked `eM Client.lnk` deleted).
  All four DLL-patch flags live in the SAME tracked `il-patches/il-patcher-Program.cs` as every
  v10 patch flag (shared tooling, not duplicated per release line) — only the deploy scripts and
  release folders are kept separate, not the patcher tool itself. Also added (not DLL patches, so
  not part of the `REVISION_LAST_STAGE`/stage-number scheme, same as v10): vendored font
  installation (`--install-fonts`/`--no-fonts`, described above) and the ICU fix above.
  - **PARTIALLY fixed, two root causes found and fixed at the DNS/socket level; a third,
    different CPU-bound mechanism found and NOT fixed** — a separate freeze affecting Microsoft
    365 / Graph API accounts specifically (not classic IMAP/EWS, so unrelated to release-4's fix
    above despite the similar symptom). Root cause #1: the same underlying Wine `GetAddrInfoExW`
    defect as the original Exchange-sync bug, hit via the Graph SDK's `HttpClient` DNS
    resolution — fixed by a custom `SocketsHttpHandler.ConnectCallback`
    (`il-patches/MailClient.Wine/DnsConnectHelper.cs`) wired into the *shared*
    `InteractionController.CreateHttpClient` factory every protocol uses, resolving hosts via
    the older synchronous `Dns.GetHostAddresses` instead of the broken async path. Root cause
    #2, found once #1 was fixed: the same class of Wine defect extends past DNS to socket
    send/receive itself — fixed by `TimeoutBoundedNetworkStream` (same file), which routes all
    stream I/O through genuinely synchronous `Socket.Receive`/`Send` bounded by
    `SO_RCVTIMEO`/`SO_SNDTIMEO` (30s) instead of .NET's async socket path. Both verified live:
    DNS+connect dropped from ~46s hangs to 40-120ms; a real ~30s stall was captured with the
    timeout firing correctly and the UI heartbeat never stopping. Also fixed in the same session:
    OAuth2 token refresh (`Credentials.GetAccessTokenRefreshResponse`) had zero retry for a
    network failure — added `TokenRefreshRetryHelper` (same directory). None of these three
    fixes are merged into the tracked `il-patcher`/`deploy.sh` pipeline yet — they're real,
    tracked C# source in `il-patches/MailClient.Wine/` plus a scratch, untracked `il-patcher`
    fork with the IL patches that wire them in.
    Root cause #3 (NOT fixed): under heavy load ("sync all folders for offline use" across all
    accounts), a per-account sync worker thread was captured genuinely CPU-bound (not blocked)
    at ~70% CPU with a real, actively-executing, recursive-looking 32-frame managed call stack,
    sustained for over a minute — a different mechanism entirely (CPU starvation of the UI
    thread, not an I/O hang). A concurrency-limiting mitigation
    (`il-patches/MailClient.Wine/AccountConcurrencyGate.cs`, gating `Command.Process()`) was
    tried at several limits and reverted: `limit=1` caused a ~10-hour deadlock (something has one
    `Command` waiting on another, so a single global permit can't work), `limit=2` avoided
    deadlock but still had multi-minute freezes under heavy load, `limit=4` was worse than `2`.
    No value tried has fixed this third mechanism, only changed how it fails. Full writeup of
    everything (every fix, every dead end, every measurement, the concurrency-gate results, and
    the tooling built/fixed along the way — an automatic freeze-context capture, two
    process-architecture discoveries, a `runner-panic` bug found), NOT merged into any tracked
    file: `reports/emclient11-msgraph-sync-freeze-findings.md`.

git is available in this environment (it wasn't in earlier sessions — if CLAUDE.md you're
reading elsewhere says otherwise, this note supersedes it). Local commit identity for this repo
is set to `Claude Code <ai@cdmdotnet.com>` (`git config --local`, not global — never set global
git config without being asked). Commit a checkpoint before starting a new patch attempt, and
after each confirmed-working fix.

Test loop: run under CrossOver with tracing (see "Investigation method" below), reproduce the
target bug's steps, check the target fixme/err/symptom is gone with no new freeze or crash. No
UI automation tooling exists in this environment (no xdotool/ydotool/xte) — a human has to
actually click through the app. `xwininfo`, `wmctrl`, `xprop` (read-only X11 inspection) ARE
available and useful for checking live window geometry without needing automation.

**Dev-only tools removed from `il-patcher` for the release patch** (never in `deploy.sh`, no
loss to recreate if needed again — each is a small, self-contained `RunPatchX` function):
- `--patch-close-listener` — self-close on a `Z:\tmp\claude-close-signal` file, for fast
  iteration without killing the process.
- `--patch-auto-test-notification` / `--patch-test-monogram-avatar` / `--patch-test-monogram-repeat`
  — synthetic test notification on a `Z:\tmp\claude-trigger-notification` file, no real email
  needed.
- `--patch-theme-switcher` — Light/Dark toggle via a file trigger, no OS theme change needed.
- `--patch-diag`, `--patch-notification-layout-diag`, `--patch-notification-geometry-diag`,
  `--patch-diag-toolstrip-buttons`, `--patch-diag-toolstrip-controls` — one-off diagnostic
  instrumentation (`File.AppendAllText` calls into a method), used to root-cause a specific bug
  and superseded once it was found. Write a new one the same way next time: same pattern as any
  other patch in this file, insert `Ldstr`/`Call File.AppendAllText` before the point in question.

## Patch pipeline — regenerate everything from a fresh `original/`

Run in order; each stage's output directory is the next stage's input. All commands assume
`~/tools/il-patcher` is built (`cd ~/tools/il-patcher && dotnet build -c Release`) from
`il-patches/il-patcher-Program.cs` (copy it to `~/tools/il-patcher/Program.cs` first if rebuilding
from a clean checkout — the tool itself isn't tracked in this repo's git history under its own
path), and `~/tools/license-oaep-patcher` + `~/tools/MailClient.Licensing.BouncyCastlePatch` are
built the same way from their `il-patches/` tracked sources (Stage 5 below needs both).

```bash
# Note: use $HOME, not ~, inside these quoted assignments -- ~ does not expand inside double
# quotes in bash, so ILP="dotnet ~/tools/..." silently fails to resolve when $ILP is invoked.
ILP="dotnet $HOME/tools/il-patcher/bin/Release/net10.0/il-patcher.dll"
ILP2="dotnet $HOME/tools/license-oaep-patcher/bin/Release/net10.0/license-oaep-patcher.dll"

# Stage 1: InterpolationMode.Bilinear fix (splash screen banner + Settings icon resize).
# Touches MailClient.dll (FormSplashScreen.OnPaintBackground) and
# MailClient.Common.UI.dll (CommonPaintUtils.ResizeImage's lambda).
$ILP --patch il-patches/stage1-interpolation-mode.json original/ il-patches/output/

# Stage 2: AllPaintingInWmPaint fix (Wine BeginPaint/WM_NCPAINT clip-region bug workaround).
# Touches MailClient.Common.UI.dll only (ControlDataGrid.initialize()).
$ILP --patch-allpaintinginwmpaint il-patches/output/ il-patches/output-stage2/

# Stage 3: Settings panel Load-event fix.
# Touches MailClient.dll only (formSettings's public constructor).
$ILP --patch-settings-refresh il-patches/output-stage2/ il-patches/output-stage3-final/

# Stage 4: category-click crash fix (Integration.IsDefaultClientVista).
# Touches MailClient.dll only. If you add any further exception-handler patch after this
# one, verify with --dump-handlers before deploying -- see "IL-patching lessons".
$ILP --patch-default-client-notimpl il-patches/output-stage3-final/ il-patches/output-stage4/

# Stage 5: License Activate RSA-OAEP decrypt fix (redirects DecryptAndVerify's OAEP calls to a
# new BouncyCastle-backed helper assembly instead of Wine's broken native RSA.Decrypt path).
# Touches MailClient.dll only, and adds a new sibling assembly. Build the helper first:
(cd ~/tools/MailClient.Licensing.BouncyCastlePatch && dotnet build -c Release)
mkdir -p il-patches/output-stage5
$ILP2 il-patches/output-stage4/MailClient.dll \
  ~/tools/MailClient.Licensing.BouncyCastlePatch/bin/Release/net8.0/MailClient.Licensing.BouncyCastlePatch.dll \
  il-patches/output-stage5/MailClient.dll
# output-stage5/ needs the rest of the stage-4 tree too (everything Stage 5 doesn't touch) plus
# the new helper assembly, to be a complete deployable directory:
rsync -a --ignore-existing il-patches/output-stage4/ il-patches/output-stage5/
cp ~/tools/MailClient.Licensing.BouncyCastlePatch/bin/Release/net8.0/MailClient.Licensing.BouncyCastlePatch.dll il-patches/output-stage5/

# Stage 6: License dialog "Get a license" button icon fix. NOT a Wine gap -- the button's own
# Text resource has two literal U+0083 control characters baked into MailClient.dll's embedded
# resource data (see reports/license-icon-findings.md for how this was confirmed isolated, not
# a font-substitution issue). Touches MailClient.dll only, a raw byte-level resource edit (no
# IL, no exception handlers) -- verify by confirming output size is byte-identical to the input
# (see "Verify" below), which proves the resource container's offset table wasn't disturbed.
$ILP --patch-license-icon il-patches/output-stage5/ il-patches/output-stage6/

# Stage 7: splash-screen tip label icon fix. IS a Wine gap (unlike Stage 6) -- the label's own
# Text resource is a genuine emoji (U+1F4A1) Wine has no glyph for; see
# reports/splash-tip-icon-findings.md for the font-linking fix that was tried first and why it
# didn't pan out. Same raw byte-level resource edit pattern as Stage 6.
$ILP --patch-splash-tip-icon il-patches/output-stage6/ il-patches/output-final/

# Deploy: swap all three touched/added files into the live bottle (back up the bottle's current
# copies first if you haven't already — see "Rollback" below), then patch deps.json so the new
# assembly resolves at runtime (this is a real deps.json-managed deployment, 247+ libraries
# listed — a dropped-in DLL isn't found without an explicit entry; the script is idempotent).
BOTTLE="/home/$USER/.cxoffice/emClient_win_7_x64/drive_c/Program Files (x86)/eM Client"
cp il-patches/output-final/MailClient.dll "$BOTTLE/MailClient.dll"
cp il-patches/output-final/MailClient.Common.UI.dll "$BOTTLE/MailClient.Common.UI.dll"
cp il-patches/output-final/MailClient.Licensing.BouncyCastlePatch.dll "$BOTTLE/"
python3 il-patches/license-oaep-patcher-patch-deps-json.py "$BOTTLE/MailClient.deps.json"
```

**In practice, use `releases/<version>/deploy.sh` instead of typing the above by hand** — it runs
every stage above (against whatever's actually installed, not a pre-built copy), every check in
"Verify" below, backs up, deploys, and can roll back on failure, all in one command. The manual
commands above remain the reference for understanding/debugging what each stage actually does;
see the release script's own header comment and `il-patches/**`'s bullet above for details on
when/why you'd still reach for the individual `$ILP`/`$ILP2` invocations directly (mainly:
investigating why a specific stage failed, or building a brand new stage before it's proven
enough to fold into a release script).

Verify before deploying, every time — this has caught real bugs (see "IL-patching lessons"
below):
```bash
# Interpolation sites should show exactly Bilinear at the two known offsets, 13 total unchanged:
$ILP il-patches/output-final/    # (no --patch flag = scan mode)

# Decompile-and-read anything you just patched with instruction insertion (not just an
# operand rewrite) before deploying — ilspycmd will surface invalid IL as a decompile error:
export DOTNET_ROOT=~/.dotnet   # ilspycmd needs this set in this environment (dotnet-install.sh
                                # layout specifically — this dev environment's dotnet lives under
                                # ~/.dotnet; on a global/apt/dnf install, derive DOTNET_ROOT from
                                # `readlink -f "$(command -v dotnet)"`'s directory instead, as
                                # deploy.sh now does, rather than hardcoding this path)
ilspycmd -m "M:MailClient.UI.Forms.formSettings.formSettings_Load(System.Object,System.EventArgs)" il-patches/output-final/MailClient.dll
ilspycmd -t "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid" il-patches/output-final/MailClient.Common.UI.dll | grep AllPaintingInWmPaint
ilspycmd -t "MailClient.Licensing.DecryptAndVerify" il-patches/output-final/MailClient.dll | grep OaepPatch

# Any patch touching exception handlers (Stage 4 and beyond) additionally needs the handler
# table itself checked — decompiling clean is not sufficient (see "IL-patching lessons"):
$ILP --dump-handlers il-patches/output-final/MailClient.dll MailClient.Utils.Integration IsDefaultClientVista

# Stages 6-7 are raw resource byte edits, not IL -- verify with a size check (must match the
# stage-5 output exactly, proving the .resources offset table wasn't disturbed by either) plus a
# content check on each:
stat -c%s il-patches/output-stage5/MailClient.dll il-patches/output-final/MailClient.dll   # must be equal
ilspycmd --resource "MailClient.UI.Forms.formLicense.resources/buttonGetLicense.Text" -o /tmp il-patches/output-final/MailClient.dll && xxd /tmp/buttonGetLicense.Text | tail -2   # should end c2 a0 c2 a0, not c2 83 c2 83
ilspycmd --resource "MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text" -o /tmp il-patches/output-final/MailClient.dll && xxd /tmp/labelTip.Text   # should read c2 a0 c2 a0, not f0 9f 92 a1
```

**Rollback:** back up the bottle's current DLLs before the first-ever deploy in a fresh bottle
(`cp "$BOTTLE/MailClient.dll" il-patches/backup/MailClient.dll.orig`, same for
MailClient.Common.UI.dll) — verify they're byte-identical to `original/` first via `md5sum`. No
git checkpoint substitutes for this: the bottle's live files are outside the git repo.

## Status

**Fixed and confirmed working:**
- Splash banner blurry — Wine's `gdiplus` doesn't implement `HighQualityBicubic`; forced to
  `Bilinear`. `reports/gdiplus-interpolation-findings.md`
- Splash tip showed tofu boxes — genuine emoji Wine can't render; replaced the glyph in the
  resource string. `reports/splash-tip-icon-findings.md`
- Settings category panel blank — `Load` event never fires under Wine; call it directly from the
  constructor. `reports/settings-panel-clip-region-findings.md`
- Settings category click crashed — Wine throws `NotImplementedException` for a COM API instead
  of failing gracefully; catch it. `reports/default-mail-client-notimplemented-findings.md`
- License activation failed silently — Wine's `bcrypt` RSA-OAEP decrypt is broken; replaced with
  a BouncyCastle implementation. `reports/license-activation-oaep-findings.md`
- License dialog button showed tofu boxes — literal bad bytes baked into the app's own resource,
  not a Wine bug; fixed the bytes. `reports/license-icon-findings.md`
- Docx attachments (and other extensions) wouldn't open — fresh bottle has no file-type
  associations; import `.reg` files for the missing ones. `reports/office-file-associations-findings.md`
- Settings grid occasionally didn't paint — Wine clip-region bug; added `AllPaintingInWmPaint`.
  `reports/settings-panel-clip-region-findings.md`
- Severe, recurring multi-minute freezes during Exchange sync (esp. mid-compose) — Wine can't
  honor async DNS cancellation, so `AccountManager.SendAndReceiveAll` and `Folder.Synchronize`
  (both fully synchronous, both reachable from the UI thread) could block on a 20s completion-port
  fallback; moved both onto dedicated guarded background `Thread`s (not `Task.Run` — ThreadPool
  ramp-up caused its own stutter, see the report). `reports/exchange-sync-freeze-findings.md`

**Email notifications not displaying correctly until fade** — one root problem, several visible
symptoms: `this` form's own window doesn't reliably paint or receive input under Wine, so content
only ever appeared during the brief show/hide animation ticks. Fixed by baking everything into the
reliably-blitted `layeredWindow` bitmap instead, and forwarding clicks/hover into `this` from
`layeredWindow`. Full investigation, every dead end, every round: `reports/notification-empty-
until-fade-findings.md`.
  - Title/content text invisible until fade — baked into the bitmap
    (`--patch-notification-text-in-bitmap` + 3 supporting flags, see deploy list below).
  - A click fired its handler twice — missing unsubscribe before a re-subscribe
    (`--patch-notification-click-resubscribe`).
  - Close/settings icons invisible until fade — same bitmap-baking fix
    (`--patch-notification-icon-bitmap`).
  - Title overlapped the now-always-visible icons — width allowance was hover-gated
    (`--patch-notification-title-icon-clip`).
  - Text visibly bolder in Light theme — GDI's transparent-text blend assumes a black background
    offscreen; switched to `Graphics.DrawString` (`--patch-notification-text-drawstring`).
  - Hover didn't pause the auto-hide countdown, and didn't snap back to visible mid-fade —
    forwarded mouse events from `layeredWindow` into `this`
    (`--patch-notification-hover-forward`).
  - Reply/flag/delete/previous/next icons invisible, not clickable, no hover — same
    bitmap-baking + forwarding, extended to real `ControlToolStripButton` child controls
    (`--patch-notification-toolbar-icons`).

**Confirmed as a real, separate Wine bug, fixed opportunistically — unrelated to the above:**
- `ControlDataGrid` could hit a Wine clip-region bug that silently discards paint calls; added
  `AllPaintingInWmPaint`. `reports/settings-panel-clip-region-findings.md`

**Not started (scanned, not confirmed):**
- 7 more `HighQualityBicubic` call sites outside the splash screen — same fix (`Bilinear`) if
  ever confirmed as user-visible.
- 1 site in vendored `QRCoder.dll` (QR export only).
- 2 `InterpolationMode.High` sites — confirmed broken by the same disassembly, not yet patched.

**Deploy status**: all of it is in `releases/<version>/deploy.sh` now (Stages 1–15) — nothing left
to apply manually. `deploy.sh` is revision-aware: it reads a `MailClient.Wine.dll` version marker
from the target bottle to figure out which of these revisions (0/1/2/3, tracked via
`REVISION_LAST_STAGE`) are already applied and resumes from the first stage that isn't, rather
than always running the full pipeline. When revision 3 (Stage 15) was added, stages 8–14 —
previously unconditional, safe only because at most two revisions existed — had to be
retroactively wrapped in the same "already applied, skip" conditional, since re-running an
already-applied patch on its own output is a real corruption risk most of these stages have no
guard against (unlike Stage 1, which refuses outright if it finds an already-patched input).
Validated live, twice: a resume-from-revision-2 run correctly skipped straight to Stage 15, and a
second run against an already-revision-3 bottle correctly skipped the whole pipeline. Stages 8–14
are the notification chain, which MUST run in this exact order
(the tool itself enforces two of these orderings, failing loudly rather than silently
misapplying if violated): `--patch-notification-click-resubscribe` (Stage 8), then Stage 9
(`--patch-notification-content-padding`, `--patch-notification-avatar-title-gap`,
`--patch-notification-title-singleline`, `--patch-notification-title-vcenter-fix` — singleline
before vcenter-fix, the latter expects flags the former adds), Stage 10
(`--patch-notification-text-in-bitmap`, `--patch-notification-refresh-on-content-change`,
`--patch-notification-periodic-reblit`, `--patch-notification-suppress-self-text-only`), Stage 11
(`--patch-notification-icon-bitmap`, `--patch-notification-title-icon-clip` — text-in-bitmap
before icon-bitmap, the latter needs `__drawNotificationTextIntoBitmap`, which the former
creates), Stage 12 (`--patch-notification-text-drawstring`), Stage 13
(`--patch-notification-hover-forward`), Stage 14 (`--patch-notification-toolbar-icons`). Verified
two ways before folding in: a fresh rebuild from a pristine `original/` snapshot (at the time,
`original/8`; the equivalent snapshot today is `original/em-10.4.5674/` — see the naming-
convention note above) confirmed byte-for-byte equivalent (decompiled output identical) to the
build already live-tested, and genuinely cleaner (the previously-live build had accumulated
dev-only diagnostic logging — `--patch-diag` etc., now removed from the tool entirely — inside
several notification methods, e.g. 8 stray `File.AppendAllText` calls inside `OnShown` alone,
none of which exist in this chain); then a full real run of `deploy.sh` itself against a bottle
restored to pristine (`cp -a` that snapshot's files over the live install, removing the stray
`MailClient.Licensing.BouncyCastlePatch.dll` too) —
all 14 stages, every verification check, backup, and deploy succeeded, and the user confirmed
the result live. That run also caught a real gap in `deploy.sh`'s own close-handling: eM
Client's own "Close application to tray" setting can make a graceful close request succeed
(window disappears) without the process ever exiting, which the script didn't previously handle
(would time out and `die` instead of falling back to `kill`) — fixed.

Stage 15 (revision 3) is the Exchange-sync-freeze fix: `--patch-account-manager-sync-async`
(`MailClient.Accounts.dll`, `AccountManager.SendAndReceiveAll`) then
`--patch-folder-sync-async` (same assembly, `Folder.Synchronize(bool, bool)`) — order doesn't
matter between the two (different types, no shared state), but both must run after Stage 14
since they touch a different assembly than the notification chain and have no ordering
dependency on it either way. Both move their target method onto a dedicated guarded
`Thread` (see `reports/exchange-sync-freeze-findings.md` for why `Task.Run` was tried first and
reverted). Verified via decompile plus `--dump-handlers` (both add a `try`/`catch(Exception)`)
before every deploy, and live end-to-end against `emClient_win_8_x64` twice — once resuming from
revision 2 (only Stage 15 ran), once already at revision 3 (nothing ran) — both correct.

## Investigation method (what actually worked this round)

Roughly in order of how cheap/reliable each was, cheapest first:

1. **Read the app's own crash bug reports.** When eM Client's error dialog appears, it writes a
   full XML stack trace to `C:\users\crossover\AppData\Local\Temp\bug.<timestamp>.txt` (=
   `~/.cxoffice/emClient_win_7_x64/drive_c/users/crossover/AppData/Local/Temp/bug.*.txt` on
   Linux) even when it fails to *send* the report (which it will, under Wine — the send itself
   throws on invalid-XML-character issues, a red herring, ignore it and read the file directly).
   This is the single best signal available: a real, symbolicated .NET stack trace, no tracing
   or instrumentation needed. Check for new files here after any crash.
2. **Disassemble the actual Wine binary** when a hypothesis is about what Wine does or doesn't
   implement, rather than inferring it from trace behavior. `objdump -d` on
   `/opt/cxoffice/lib/wine/i386-windows/gdiplus.dll` (or the relevant Wine DLL) settled the
   InterpolationMode question in one pass after two rounds of wrong trace-based guesses. Find the
   fixme string's file offset (`python3 -c "... data.find(b'...')"`), map it to an RVA via
   section headers (`objdump -h`), then find what code references that RVA
   (`objdump -d | grep <rva-hex>`) and read the dispatch logic around it.
3. **Instrument the app directly** (`~/tools/il-patcher --patch-diag`, or a one-off variant of
   it) when the question is about the app's *own* runtime state (field values, which code path
   ran, whether a method was even entered) rather than about Wine. Insert
   `File.AppendAllText(@"Z:\tmp\claude-diag.log", "...")` calls at the point in question via
   Cecil, deploy, have the user reproduce once, read the log directly — no `CX_DEBUGMSG` needed,
   far more precise than inferring app state from GDI trace noise. This is what actually found
   both the clip-region bug's real extent and the Settings-panel Load-event bug; trace-only
   investigation had produced two wrong fixes first, in both cases.
4. **`CX_DEBUGMSG` trace** (`+gdiplus,+region,+clipping,+bitblt,+message,+font,+seh` was the
   working channel set this round — see below) when the question is genuinely about Wine's own
   behavior (window messages, GDI region math, paint dispatch) and disassembly isn't practical.
   Channel names for a given Wine build aren't guessable — confirm via
   `strings /opt/cxoffice/lib/wine/i386-unix/<module>.so | grep -E '^[a-z]+$'` against a
   plausible short list; `CX_DEBUGMSG=help` does not enumerate them on this CrossOver build.
   Traces are large (100MB+ for even a short session) and need correlating by HWND/timestamp —
   slower and noisier than options 1–3, but sometimes it's the only option (e.g. confirming the
   clip-region fix actually changed Wine's behavior, not just app behavior).

For any option requiring the user to reproduce interactively: **always offer both** "you verify,
tell me what happened" and "I start a traced/instrumented launch first, you just reproduce" —
this was an explicit, repeated user preference this round. Coordinate launches: check
`ps aux | grep -i mailclient` before relaunching (never kill a running instance without asking —
it may hold live user data), start the traced/instrumented build, tell the user what to do, then
poll for the process to exit or the log file to gain content before reading results.

## IL-patching lessons (apply to any future Cecil-based patch in this project)

`~/tools/il-patcher` started as a read-only scanner (`--patch`, simple operand rewrite: flips an
`ldc.i4` constant, verified by pattern-matching the few instructions around it — low risk). It
grew instruction-*insertion* modes (`--patch-allpaintinginwmpaint`, `--patch-settings-refresh`,
`--patch-default-client-notimpl`, and the temporary `--patch-diag`) for fixes that need new code,
not just a changed constant, plus one raw-resource-data mode (`--patch-license-icon`, which
touches an embedded `.resources` blob's bytes directly rather than IL — see its own doc comment
in the source for why a length-preserving byte replacement was used instead of the more obvious
"just delete the bad characters"). Several real bugs were introduced and caught during this
session, all worth guarding against explicitly next time:

1. **Reusing a re-read anchor reverses insertion order.** `InsertBefore(anchor, x)` called three
   times with `anchor` re-read as `il[0]`/similar each time (instead of captured once into a
   local) picks up the *previous* insertion as the new anchor, so the three new instructions end
   up in reverse order — e.g. a `call` landing before its arguments are pushed, a stack
   underflow. Always capture the anchor **once** into a local and reuse that local for every
   `InsertBefore` call in the sequence.
2. **Inserting before a branch target without retargeting skips the new code.** If the chosen
   anchor instruction is itself the target of an earlier `br`/`brtrue`/`brfalse` elsewhere in the
   method (very common — it's often a natural "join point" after an `if` block, which is
   *why* it looked like a good insertion point), inserting new code immediately before it does
   **not** make branches "fall through" the new code — a branch jumps to the exact instruction
   object, bypassing anything spliced in before it. The fix landed inside a conditional's
   fall-through path instead of the intended unconditional join point twice this session before
   this was caught. Always search the whole method body (and its exception handlers) for any
   instruction/handler-region boundary whose `Operand`/`TryStart`/`TryEnd`/`HandlerStart`/
   `HandlerEnd` equals the original anchor, and retarget them to the new first inserted
   instruction.
3. **Inserting new code before an existing exception handler's own `HandlerEnd` silently grows
   that handler's range to swallow the new code.** `HandlerEnd` (also `TryEnd`) is an *exclusive
   boundary marker* — a reference to the instruction right after the region, not a fixed offset.
   Adding a sibling `catch` clause by inserting its body immediately before an existing handler's
   `HandlerEnd` extends that *existing* handler's own range to include the new code too, unless
   `HandlerEnd` is explicitly reassigned to the new code's first instruction. The result:
   two handlers with overlapping byte ranges — invalid IL that neither Cecil's writer nor
   ilspycmd's decompiler flags, but the CLR verifier does, as `InvalidProgramException` at JIT
   time (hit for real, deployed, crashed — see `--patch-default-client-notimpl`'s history).
   Separately, when a `try` has multiple sibling `catch` clauses, a *new* one must be inserted at
   that position in `body.ExceptionHandlers` (e.g. right after the existing handler it's modeled
   on) — appending to the end of the list can land it after an enclosing method's outer
   `finally`, violating the CLR's required most-nested-first handler ordering. Use
   `--dump-handlers <dll> <type> <method>` (prints the handler table with resolved offsets and
   flags nesting-order violations) to verify both **before** deploying, any time a patch adds,
   moves, or extends an exception-handler region — this class of bug doesn't show up in a
   decompile.
4. **Inserting enough new instructions can silently corrupt a nearby short-form branch.**
   `bne.un.s`, `br.s`, `brtrue.s`, `brfalse.s`, etc. encode their target as a single **signed
   byte** relative offset (±127 bytes). Mono.Cecil does **not** automatically widen these to their
   long-form equivalents (`bne.un`, `br`, ...) as a method body grows from inserted instructions —
   if enough new code lands between a short branch and its target that the true offset no longer
   fits in an sbyte, `module.Write()` still emits *something*, silently wrong, rather than
   erroring. Decompiles as garbled control flow with spurious "stack underflow" errors, often in a
   completely unrelated branch of the same method (hit for real: a 48-instruction insertion in
   `--patch-notification-invalidate`'s third revision broke a nearby `bne.un.s`; two earlier,
   much smaller insertions in the same method had stayed under the range by luck, which is why
   this wasn't caught sooner). Fix: call `body.SimplifyMacros()` (from `Mono.Cecil.Rocks` — needs
   `using Mono.Cecil.Rocks;`; the assembly ships inside the same `Mono.Cecil` NuGet package, no
   extra package reference needed) once, before making any insertions, on any method whose body
   might grow by more than a trivial amount. It converts every short-form branch in the method to
   long form up front, which has no functional downside and removes the range problem entirely.
   Cheap enough to just always call before inserting, rather than judging case-by-case whether a
   given insertion is "small enough".
5. **`typeof(X)` reflection to build a `MethodReference`/`FieldReference` bakes in the *patching
   tool's own* runtime's assembly version, not the target app's.** Several instrumentation helpers
   resolve BCL members via reflection on the running `il-patcher` process itself (e.g.
   `typeof(Environment).GetProperty("TickCount")`, `typeof(File).GetMethod("AppendAllText", ...)`)
   and this is safe for types in `System.Private.CoreLib`/its forwarding facades (`System.Runtime`,
   `System.Collections`, ...) because the CLR's default framework resolution unifies/forwards these
   across versions transparently. It is **not** safe for a type in an app-local, deps.json-pinned
   assembly the target app ships its own copy of alongside itself — `il-patcher` targets `net10.0`,
   so `typeof(System.Drawing.Rectangle).GetProperty("IsEmpty")` bakes in a
   `System.Drawing.Primitives, Version=10.0.0.0` reference, but MailClient.exe ships and runs on
   .NET 8 with its own `System.Drawing.Primitives.dll` at `Version=8.0.3026.36720` — no v10 copy
   exists alongside it, and unlike CoreLib-forwarded types this doesn't get silently redirected.
   Hit for real: a `--patch-diag` revision instrumenting `headerRect.IsEmpty` crashed the app with
   `FileNotFoundException: Could not load file or assembly 'System.Drawing.Primitives,
   Version=10.0.0.0...'` the moment the instrumented method ran — decompiled clean (ilspycmd
   doesn't validate assembly-reference versions against what's actually deployed) and only
   surfaced as a real runtime crash, exactly the "decompiling clean is not sufficient proof" shape
   as lesson 3's exception-handler bug. Fix: never reflect on the patching tool's own process for a
   type outside CoreLib/its facades — resolve it from a field/parameter/return type *already*
   present in the module being patched instead (e.g. `someExistingField.FieldType.Resolve()`, then
   find the member on that already-correctly-versioned `TypeDefinition`), the same technique
   already used elsewhere in this file for `System.Windows.Forms.Control` (walk the base-type
   chain rather than reflect on `typeof(Control)`).
6. **A struct property setter call needs its target (`this`, via `Ldloca`/`Ldloca_S` on the local)
   pushed *before* the new value, not after.** `bounds.Height = measured.Height;` compiled by hand
   as `Ldloca_S sizeLocal; Call get_Height(); Call set_Height(int)` — missing the
   `Ldloca_S boundsLocal` that should have come first to push `&bounds` as the implicit `this` for
   the instance `set_Height` call. Easy to miss because the *previous* statement in the same
   sequence (`bounds.Y = ...`) got this right (an explicit `Ldloca_S boundsLocal` was already on
   the stack from a `Dup`), making the omission on the very next statement look consistent at a
   glance. Caught by working through each new sequence's stack effect by hand, instruction by
   instruction, rather than trusting that a pattern used correctly once nearby was necessarily
   copied correctly the second time (`--patch-notification-title-vcenter-fix`'s first draft).
7. **Never hold a instruction's list *position* as a bare `int` index across any insertion or
   removal on that same list — capture the `Instruction` object itself instead.** A method's flags
   constant was found once (as an index into `body.Instructions`) *before* ~20 new instructions
   were inserted earlier in the same method, then edited via `instrs[thatIndex].Operand = ...`
   *after* the insertion. The insertion shifted every later instruction to a higher index without
   updating the stale saved index, so the edit landed on one of the *newly inserted* instructions
   instead of the intended one — silently overwriting a `VariableDefinition`-typed `Ldloca_S`
   operand with an `int`. This didn't surface as a decompile error (the corruption only existed in
   the mutated `Instruction` list, and decompiling from a fresh read of the already-written file
   would have looked fine had the write not thrown first) — it crashed as an
   `InvalidCastException: Unable to cast object of type 'System.Int32' to type
   'Mono.Cecil.Cil.VariableDefinition'` deep inside `Mono.Cecil.Cil.CodeWriter.WriteOperand`, only
   at `module.Write()` time, with a stack trace pointing at Cecil's own internals rather than
   anything resembling the actual mistake. Fix: assign `Instruction? target = instrs[i];` inside
   the search loop (not `int targetIndex = i;`) whenever the found instruction will be mutated
   later in the same function, especially after any insertion happens in between — object
   references stay valid across list mutation, indices don't.
8. **A branch-target instruction reused as a join point for multiple incoming paths must have a
   neutral (`Nop`) stack effect — never repurpose a real load instruction as the label.** Building
   a "skip this draw, the image field is null" guard, the natural instinct was to reuse
   `Instruction.Create(OpCodes.Ldarg_0)` as the shared target both the fall-through-after-a-
   successful-draw path and the branch-in-because-null path land on — it looked like a convenient
   existing instruction to jump to rather than manufacturing a bare label. But that instruction
   *executes* on both paths, and `Ldarg_0` has a real effect (pushes `this`, consumed by nothing
   downstream) — it would have silently corrupted the evaluation stack on every single call,
   regardless of which path was taken. Caught by manually tracing the generated IL's stack effect
   before ever building it, not by a compiler or runtime error. Fix: use `OpCodes.Nop` for any
   instruction whose only job is to be a jump target multiple paths converge on — zero stack
   effect, purely structural (`--patch-notification-icon-bitmap`'s null-guard fix).
9. **Comparing a `FieldReference` pulled off an existing instruction to an independently-
   `Resolve()`d `FieldDefinition` via C# `==` doesn't reliably match, even when they name the same
   field.** `instrs[i+1].Operand == mouseOverField` silently failed to match real occurrences —
   `FieldReference`/`FieldDefinition` don't guarantee reference equality just because they resolve
   to the same field, and Cecil doesn't override `==` to value-compare them. Fix: compare `.Name`
   strings instead (`instrs[i+1].Operand is FieldReference fr && fr.Name == mouseOverField.Name`) —
   safe here because the search is already scoped to a single known method/type, so name collision
   isn't a real risk (`--patch-notification-icon-bitmap`'s `OnPaint` block-removal search).
10. **Widening a *shared library* type's own member visibility (e.g. `protected` → `public`) to
    let one new call site reach it directly has a much larger blast radius than editing a method
    body — it can break every OTHER existing call site too, silently. When you need to reach
    across an accessibility boundary into a shared type, add a new, purely ADDITIVE member instead
    of mutating an existing one's metadata.** Changing `ControlToolStripButton.OnPaint`/
    `OnMouseEnter`/`OnMouseLeave` (`MailClient.Common.UI.dll`) from `protected` to `public`
    (clearing `MethodAttributes.MemberAccessMask` and setting `Public`) so `FormMailNotification`
    (`MailClient.dll`) could call them directly decompiled clean and deployed without any error —
    but broke icon rendering on **every toolbar button across the entire app** (confirmed live via
    screenshot: New/Refresh/Save/Reply/etc. all reduced to text-only, no icons), since
    `ControlToolStripButton` is the shared control behind every toolbar in the app, and an
    override's accessibility mismatch against its base declaration — something the C# compiler
    blocks at compile time — turned out to matter at the CLR level too, corrupting the type
    broadly rather than just the one call site being touched. Reverted immediately. An interim fix
    used reflection (`Type.GetMethod(BindingFlags.NonPublic | BindingFlags.Instance)` +
    `MethodBase.Invoke`) — zero blast radius, and it worked, but real ongoing cost (`object[]`
    boxing per call, a string-based method-name lookup that could silently no-op if ever wrong,
    with no compiler to catch it). **The version that actually shipped**, after the user asked for
    a holistic re-review before folding this into `deploy.sh` rather than settling for "it works":
    three brand-new `public` wrapper methods added to `ControlToolStripButton` itself
    (`RaisePaint(PaintEventArgs e) => OnPaint(e);` and two more for `OnMouseEnter`/
    `OnMouseLeave`) — calling a class's own protected members from a NEW method declared on that
    SAME class is always legal (protected access from the declaring type itself is unrestricted),
    and since these are new members rather than edits to existing ones, no existing call site
    anywhere else in the app can possibly be affected. Gets reflection's "zero blast radius"
    property AND plain-`Callvirt` simplicity, strictly better than either earlier attempt. Never
    widen or otherwise mutate a shared type's own existing member metadata for the sake of one new
    caller — add alongside it instead (`--patch-notification-toolbar-icons`).
11. **Reusing someone else's paint code (via reflection or otherwise) that touches the `Graphics`
    transform/clip state can silently corrupt whatever you had applied before calling it — assume
    it might reset, not just add to, that state.** `ControlToolStripButton.OnPaint`'s own
    image-drawing branch calls `Graphics.ResetTransform()` internally (part of its rotation-
    transform handling, unconditional whenever an image actually draws). A first draft wrapped
    each button's `OnPaint` call in a manual `TranslateTransform(+X,+Y)` / `TranslateTransform
    (-X,-Y)` pair to position it and then "undo" the position afterward — but `ResetTransform()`
    wipes the `+X,+Y` translation (and anything applied before it, including an outer shadow
    offset) before the manual undo ever runs, so the undo's `-X,-Y` applies to an already-reset
    identity matrix instead of the intended baseline, corrupting the transform for every
    subsequent button drawn on the same `Graphics` object. Confirmed live: the first button drawn
    landed correctly, every button drawn after it rendered near the top-left corner instead of its
    intended position. Fix: `Graphics.Save()` / `Restore(GraphicsState)` instead of manual
    translate math — `Restore()` puts the ENTIRE transform/clip state back to exactly what
    `Save()` captured, regardless of what the reused code did with it in between (translate,
    rotate, reset, whatever), fully isolating each call from the others. Reach for `Save()`/
    `Restore()` by default around any reused paint call, not just when a manual undo is later
    proven wrong.
12. **When a single patch invocation adds a new member to one assembly and needs a second
    assembly (processed in the same pass) to call it, resolve that member as a hand-built
    `MethodReference` — not by re-reading/searching the first assembly's `TypeDefinition`, which
    was resolved from the INPUT directory's copy and won't see anything written to the OUTPUT
    directory during this same run.** `--patch-notification-toolbar-icons` adds
    `RaisePaint`/`RaiseMouseEnter`/`RaiseMouseLeave` to `ControlToolStripButton`
    (`MailClient.Common.UI.dll`) and then needs `FormMailNotification` (`MailClient.dll`,
    processed later in the same `foreach` over the input directory's files) to call them. A first
    attempt looked them up via `buttonTypeDef.Methods.FirstOrDefault(m => m.Name == "RaisePaint")`
    — `buttonTypeDef` had been resolved earlier by reading `MailClient.Common.UI.dll` from the
    INPUT directory (via the shared `AssemblyResolver`), which is the ORIGINAL, unmodified file;
    the new methods only exist in the modified copy already written out to the OUTPUT directory
    moments earlier in this same function. The lookup silently returned null every time. Fix:
    since the new methods' full signature is known at patch-authoring time anyway (name, return
    type, one parameter of an already-resolved type), build a plain `MethodReference` by hand
    (`new MethodReference(name, module.TypeSystem.Void, buttonTypeRef) { HasThis = true }` plus
    one `Parameters.Add(...)`) instead of trying to resolve one from a file — sidesteps the
    input/output split entirely.
13. **To move an existing method's logic onto a new entry point without hand-authoring a second
    copy of its IL, just reassign the new method's `.Body` to the ORIGINAL method's already-
    compiled `MethodBody` object, then give the ORIGINAL method a fresh, simple body instead.**
    Needed for `--patch-account-manager-sync-async`/`--patch-folder-sync-async`: the real sync
    logic (`SendAndReceiveAll`, `Folder.Synchronize`) has to keep running unchanged, but from a
    background thread, while the original method name becomes a thin guard/dispatch wrapper.
    Hand-copying either body's IL instruction-by-instruction would have been risky — both contain
    null-conditional delegate/event invokes, a construct that's easy to get subtly wrong by hand.
    Instead: create the new method (`__RunSendAndReceiveAllCore`, matching parameter shape),
    `newMethod.Body = originalMethod.Body;` (literally reuse the same `MethodBody` object, not a
    copy), then replace `originalMethod.Body` with a brand new, small `MethodBody` containing just
    the guard and dispatch. Works because the new method has the same parameter shape (so its
    `ldarg` instructions still mean the same thing) and stays declared on the same type (so any
    internal member references the reused body makes remain valid). Confirmed via decompile: the
    new method's decompiled output is byte-for-byte what the original method used to decompile as.
    Prefer this over hand-authoring whenever the goal is "run this exact existing logic from
    somewhere else," not "run modified logic."
14. **`Task.Run` is the wrong dispatch primitive for a fix whose whole point is unblocking the UI
    thread from a long, blocking operation — it schedules onto the shared ThreadPool, which ramps
    up slowly (roughly one new thread per 500ms–1s) under sustained demand, and produces its own
    distinctive "freeze, ~1s unfreeze, freeze again" stutter once multiple such operations are
    in flight concurrently.** The first working version of both sync-freeze patches used
    `Task.Run(...)` to move `SendAndReceiveAll`/`Folder.Synchronize` off the UI thread — this
    genuinely fixed the original multi-minute freeze, but introduced a new, smaller, repeating
    stutter the user caught immediately during live testing (multiple accounts/folders each
    dispatching via `Task.Run`, each blockable for up to 20s by the same Wine DNS-cancellation
    gap, competing for a pool that wasn't growing fast enough to keep up). Fixed by switching both
    to `new Thread(ThreadStart) { IsBackground = true }.Start()` instead — a dedicated thread per
    dispatch, no shared pool to contend over — which also matches a pattern already established
    elsewhere in the app itself (`MailClient.Commands.DefaultSynchronizationQueue` uses one
    dedicated `Thread` per account, never the ThreadPool). When a fix's entire purpose is "don't
    block on this," prefer a dedicated `Thread` over `Task.Run` by default for any operation that
    can block for a long, unbounded, or externally-controlled duration (network I/O with no
    reliable cancellation being the sharpest case) — reserve `Task.Run` for short, CPU-bound work
    where pool reuse is actually a win.
15. **Reading an app-local enum's real member values from the TARGET module's own
    `FieldDefinition.Constant` is the only safe option — guessing the values, or reflecting on the
    patching tool's own runtime for the same type name, are both live risks, not just style
    preferences.** `--patch-folder-sync-async` needs `SynchronizationPriority.Background`/
    `BackgroundForced`'s actual integer values to build a ternary in IL. Guessing (or assuming an
    enum's declaration order matches its underlying values) is exactly the mistake lesson-adjacent
    to this one already caught elsewhere in this session: `GCLargeObjectHeapCompactionMode
    .CompactOnce` was assumed to be `1` and was actually `2` (`Default` is `1`), caught only by
    the mandatory post-patch decompile check. Reflecting on the patching tool's own process for an
    app-local type has its own, separately-documented failure mode (lesson 5). The reliable fix
    used here: resolve the enum's `TypeDefinition` from the target module (already being patched,
    so this is a normal, safe reference walk, not tool-runtime reflection), then
    `priorityTypeDef.Fields.FirstOrDefault(f => f.Name == "Background").Constant` — reads the
    literal value the target assembly itself actually shipped with, no assumption involved.
16. **Every `MethodReference`/`FieldReference` used inside a method body must be imported into
    THAT method's own declaring module — not whichever module happened to be open when the
    reference was built.** Every patch before this one touched exactly one assembly, so
    `module.ImportReference(...)` (where `module` is the single file being read/written) was
    always correct by construction. `--patch-notification-hover-forward` needed to insert IL into
    `LayeredForm.WndProc`, and eM Client 11.0.196-beta moved `LayeredForm` into a *different*
    assembly (`MailClient.Common.UI.dll`) than the one being iterated as the "main" target
    (`MailClient.dll`) — a `MouseEventArgs` constructor reference imported via
    `module.ImportReference(...)` (module = `MailClient.dll`) but then used inside
    `WndProc`'s body (which belongs to `MailClient.Common.UI.dll`'s own module) built without
    error, decompiled fine, and only failed at `module.Write()` time:
    `ArgumentException: ... is declared in another module and needs to be imported`. When the
    SAME underlying member is called from method bodies in two different modules (as this
    constructor was — once from `WndProc`, once from a handler method declared alongside
    `FormGenericNotification`), that's two separate `ImportReference` calls, one per module, even
    though it's conceptually "the same" reference — resolve which module a given piece of
    generated IL will actually live in before choosing which module to import into, don't assume
    it's always the one you happened to open first.
17. **A patch that writes more than one output file cannot rely on `Directory.GetFiles`'
    enumeration order — it isn't alphabetical, and isn't guaranteed to be anything.** Every prior
    multi-file patch's loop shape was "File.Copy everything that isn't `targetAssembly`, patch
    and write whichever file *is*" — safe as long as targetAssembly's own patch-and-write step is
    the LAST thing that ever touches its output path. `--patch-notification-hover-forward`,
    once patching two files (see lesson 16), needed to ALSO write the second file
    (`MailClient.Common.UI.dll`) from the mutated in-memory module — and the same loop iterated
    over `MailClient.dll` first (patching + writing both files correctly, confirmed via debug
    instrumentation: a `FileInfo.Length` check immediately after the write showed a materially
    different size), then reached `MailClient.Common.UI.dll`'s OWN "not targetAssembly, plain
    copy" branch later in the same pass and silently clobbered the just-patched file back to a
    byte-identical copy of the unpatched input. No exception, no warning — the tool's own success
    message printed correctly both times, and the bug only surfaced by explicitly diffing the
    output against the input by hand afterward. Fixed by restructuring: do all patch work up
    front (outside any file-enumeration loop), THEN do one copy-everything-else pass that
    explicitly excludes every filename that got patched, THEN write the patched module(s) —
    removes the ordering dependency entirely rather than trying to control or predict iteration
    order. Worth checking any EXISTING multi-file patch for this same shape if it's ever extended
    to touch a third file.
18. **A single instruction can simultaneously be a branch target AND an exception handler's own
    `TryEnd`/`HandlerEnd`/`TryStart`/`HandlerStart` boundary marker — both need retargeting,
    independently, when inserting before it.** Lesson 2 already covers retargeting branches that
    point at a chosen anchor; lesson 3 already covers not extending a handler's range by
    inserting before its `HandlerEnd`. What's easy to miss is that **the exact same anchor
    instruction can be both at once** — a `leave` exiting a `try` block on its normal path often
    targets the instruction immediately after an enclosing `finally`'s `endfinally`, which is
    *also* that `finally` handler's own `HandlerEnd` (an exclusive boundary reference, per
    lesson 3). Fixing only the branch (retargeting the `leave` to the new first inserted
    instruction, per lesson 2) without ALSO checking `body.ExceptionHandlers` for any
    `TryStart`/`TryEnd`/`HandlerStart`/`HandlerEnd` equal to that same original anchor silently
    grows the handler's range to swallow the newly inserted code anyway — decompiled as
    `ilspycmd` control-flow-resolution errors ("Could not find block for branch target",
    "Discarded unreachable code") rather than garbled-but-parseable C#, since a `leave` landing
    inside what's now considered "the handler's own body" violates a CLR structural invariant
    the decompiler's CFG builder can't route around. `--dump-handlers` reported this as
    correctly nested (the handler's range legitimately grew, it didn't overlap a sibling) — this
    class of bug doesn't fail that check, only a decompile attempt surfaces it. Fix: after
    retargeting any branches pointing at the original anchor, ALSO loop over
    `body.ExceptionHandlers` checking all four boundary fields by reference equality against
    that same anchor and reassign any match to the new first inserted instruction (hit for real,
    scratch `--patch-diag-taskqueue`, instrumenting `TaskQueue`'s dequeue loop in
    `MailClient.Accounts.dll` — see
    `reports/emclient11-msgraph-sync-freeze-findings.md`).
19. **A static helper added to a GENERIC type must be referenced, from within that same type's
    own generic methods, through the type's OWN generic parameters as the instantiation — never
    the bare open generic type definition.** Adding `__safeAppendDiagLog4(string)` directly to
    `MSGraphItemSynchronizer<TItem,TStorageItem,TGraphItem>` and calling it via a plain
    `MethodReference` whose `DeclaringType` was that open generic type definition (the same
    style of reference used for every non-generic patch in this file) decompiled cleanly and
    built without error, but threw `System.InvalidOperationException: Could not execute the
    method because either the method itself or the containing type is not fully instantiated`
    the instant the instrumented generic method (`IterateDeltaPages<TDeltaResponse>`) actually
    ran — caught live via the app's own exception-reporting UI, not by any static check
    (`--dump-handlers` and a clean decompile both passed; this is a runtime-only generics
    validity rule, invisible to both). Worse than a silent corruption: the exception aborted the
    instrumented method immediately, before the real work after the log call ever ran — so the
    diagnostic didn't just fail to collect data, it silently disabled the very code path under
    investigation, which could easily be mistaken for "the freeze is fixed" if the resulting
    lack of a freeze isn't traced back to the broken instrumentation first. Fix: build a
    `GenericInstanceType` of the target generic type, instantiated with that same type's own
    `GenericParameters` as the arguments (`new GenericInstanceType(targetType)` +
    `GenericArguments.Add(gp)` for each of `targetType.GenericParameters`), then construct the
    call's `MethodReference` against THAT instantiation rather than `targetType` directly — the
    same construction a C# compiler emits automatically for a static call from inside a generic
    class to a sibling static member of that same class. Applies to any future patch adding a
    new static (or instance) member to a generic type and calling it from that type's own
    generic-parameter-bearing methods (`--patch-diag-deltasync`, instrumenting
    `MSGraphItemSynchronizer<TItem,TStorageItem,TGraphItem>.IterateDeltaPages` in
    `MailClient.Protocols.MSGraph.dll` — see
    `reports/emclient11-msgraph-sync-freeze-findings.md`).
20. **A `public` method on an `internal` type is still only as accessible as the type itself from
    outside the assembly.** Adding a new helper type to a sibling assembly (following this
    project's usual "smallest necessary visibility" instinct, e.g. `internal static class
    DnsConnectHelper`) and calling one of its `public` methods from a DIFFERENT assembly
    decompiled clean and built without error, but crashed on the very first launch with
    `System.MethodAccessException: Attempt by method '...' to access method '...' failed` the
    instant the caller actually invoked it — the same "decompiling clean is not sufficient proof"
    shape as lessons 3 and 19, a new trigger for it. The CLR's JIT-time accessibility check looks
    at BOTH the member's own accessibility AND its declaring type's, and a caller in another
    assembly fails the type-level check regardless of the member being `public`. Fix: any new
    type in a sibling assembly that will be called from elsewhere must itself be `public`, not
    just its individual members — internal member visibility WITHIN that type (e.g. a private
    implementation-detail class it uses internally, like `DnsConnectHelper`'s own `DnsCache`) is
    unaffected and can stay as restrictive as actually needed
    (`--patch-http-dns-connect-callback`, see `reports/emclient11-msgraph-sync-freeze-findings.md`).

**A new `--dump-il <dll> <type> <method>` utility mode** (same rationale as `--dump-handlers`)
prints a method's real instruction stream with offsets — use it before writing any patch that
edits an *existing* numeric constant or inserts around a specific expression, rather than guessing
at IL shape from decompiled C#. Decompiled C# doesn't distinguish compact-form opcodes (`ldc.i4.5`,
`ldc.i4.8`, `ldarg.1`, whose operand is implicit in the opcode and can't be edited in place — the
instruction must be replaced outright) from general-form ones (`ldc.i4 <n>`, whose operand can be
reassigned directly) — both decompile identically. Two of this round's patches
(`--patch-notification-content-padding`, `--patch-notification-avatar-title-gap`) needed exactly
this distinction, and a third (`--patch-notification-title-vcenter-fix`) needed it to notice that
`body.SimplifyMacros()` (needed anyway, for the insertion) converts a pattern being searched for
elsewhere in the same method to its general form too, breaking a compact-opcode-based search that
worked before `SimplifyMacros()` was added.

**Always re-decompile and read the result before deploying** anything beyond a simple operand
rewrite — `ilspycmd -m "<doc-id>" <dll>` or `ilspycmd -t "<type>" <dll>` on the patched output.
This caught bugs 1, 2, and 4 above before they ever reached the bottle: bug 1 and bug 4 as an
explicit decompiler error ("Stack underflow"), bug 2 as the new code visibly appearing inside the
wrong `if` block in the decompiled C#. A clean scan-mode pass (`$ILP <dir>`, no flags) confirms
operand-rewrite patches but does *not* catch any of these — it doesn't attempt to
decompile control flow or validate exception regions. Bug 3 is the sharpest lesson here: it
decompiled perfectly cleanly (ilspycmd doesn't validate handler-region bounds) and only surfaced
as a real crash after deploying — `--dump-handlers` exists specifically because decompiling
clean is not sufficient proof for any patch touching exception regions.
