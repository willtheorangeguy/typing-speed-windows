# WinTypingSpeed — FAQ

### Is this a keylogger?

It uses the same Windows mechanism, and it cannot record what you type.

`TryClassifyKey` maps every keystroke to one of three things: `' '` for space and enter, `'a'`
for any other typing key, or nothing. The real character is never resolved, so there is nothing
downstream that could reconstruct your typing. Identity is destroyed at the hook, not filtered
afterwards.

There is also no network code in the solution and no file is written outside the installer. The
source is four small C# files — [Architecture](./architecture.md) walks through them.

### Then why does my antivirus flag it?

Because `WH_KEYBOARD_LL` is what a keylogger installs, and heuristics cannot tell intent. The
installer is also unsigned, which does not help.

### Why does WPM drop the longer the app runs?

"Active time" is elapsed time since the session started or was last resumed. It does not exclude
periods when you were not typing, so WPM is your word count divided by however long the app has
been running.

Reset before a stretch of work you want measured. Recorded in
[`internal/known-issues.md`](./internal/known-issues.md).

### Does it pause when I stop typing?

No. It pauses on workstation lock, on system suspend, and when you ask it to. Idleness alone
does nothing.

### Do keyboard shortcuts count as typing?

Yes. Modifier state is not checked, so `Ctrl+S`, `Ctrl+C`, and `Alt+Tab` count their letter as a
typed character. `WM_SYSKEYDOWN` is handled explicitly, so Alt combinations are included.
Recorded in [`internal/known-issues.md`](./internal/known-issues.md).

### Does holding a key down count repeatedly?

Yes — key auto-repeat produces repeated messages and each one is counted.

### Does backspace reduce the count?

No. Corrections are typing too; the count only ever goes up.

### What counts as a word?

Anything ending in space or enter. Tab does not end a word. Consecutive spaces do not create
empty words — the tracker ignores a completion with no characters in progress.

### Is my session saved when I close the app?

No. Everything is in memory, so closing discards it. Nothing to clear, and nothing left behind.

### Does it work in games, terminals, and remote sessions?

A low-level hook sees input at the system level, so mostly yes. Exceptions: applications running
elevated when this one is not, secure desktops such as UAC prompts and the sign-in screen, and
some anti-cheat systems that block hooks outright.

### Why does the window disappear instead of closing?

The app is a tray app. Closing the window leaves it running in the notification area; use the
tray menu to exit.

### Can I configure anything?

No. There is no settings file, no options dialog, and no command-line switch. See
[Configuration](./configuration.md) for the constants you would edit in source.

### Why does the installer warn about an unknown publisher?

It is not signed with a code-signing certificate. **More info → Run anyway**.

### Is there a Linux or macOS version?

No. Global keyboard hooks and WPF are Windows-specific. The Core project is portable, but nothing
else is.
