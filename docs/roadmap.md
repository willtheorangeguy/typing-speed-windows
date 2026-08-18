# WinTypingSpeed — Roadmap

Direction, not a schedule. Defects are tracked in
[`internal/known-issues.md`](./internal/known-issues.md); this page is about what the app is
*for*.

## Where it is

It counts keystrokes system-wide without learning what they are, computes WPM, pauses on lock and
suspend, and ships as a self-contained installer. Core is tested; the hook and UI are not.

## Considered

**Active time that means active time.** The single most valuable change here. "Active time" is
currently elapsed time, so the headline figure decays for as long as the app runs — and the
installer's default startup entry guarantees it runs all day. Excluding gaps longer than an idle
threshold, as the sibling VS Code extension does, would make the number mean what it says.

**Not counting shortcuts.** `Ctrl+S` currently counts as a typed character. The hook already
receives the flags it would need.

**Not counting key repeat.** Holding a key inflates the count in proportion to how long you hold
it.

**Noticing when the hook is removed.** Windows uninstalls a hook that exceeds its timeout, and
nothing here detects it — counting just stops.

**A rolling window.** A session average answers a different question from "how fast am I typing
right now", and the second is usually the one wanted.

**Code signing.** It would remove the SmartScreen warning, which lands at exactly the moment a
user is deciding whether to trust an app that installs a keyboard hook.

**Tests on push.** The workflow runs only on release and manual dispatch.

## Non-goals

**Recording what is typed.** Not a missing feature — an actively rejected one. Key identity is
discarded at the hook, which is also what keeps `ToUnicodeEx` from corrupting dead-key state
system-wide. Any feature needing the real characters would forfeit both properties.

**Per-application breakdowns.** Knowing which app you were typing into means tracking which app
you were using, which is a materially more invasive thing to keep.

**Telemetry or leaderboards.** Nothing leaves the machine, and there is nothing to send.

**Cross-platform.** Global hooks and WPF are Windows-specific. Core is portable; nothing else is.

**Persisting sessions.** A session is deliberately in-memory only, so closing the app leaves
nothing behind — no file to find, and nothing to clear.

## Contributing

Issues and pull requests welcome — see the
[Contributing Guide](https://github.com/willtheorangeguy/.github/blob/main/CONTRIBUTING.md).
Anything touching `GlobalKeyboardHook` should keep the callback minimal and keep key identity out
of it; both properties are load-bearing.
