using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinTypingSpeed.App.Services;

public sealed class GlobalKeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;

    private readonly LowLevelKeyboardProc hookCallback;
    private nint hookHandle;

    public GlobalKeyboardHook()
    {
        hookCallback = HandleHook;
    }

    public event EventHandler<KeyCapturedEventArgs>? CharacterCaptured;

    public void Start()
    {
        if (hookHandle != nint.Zero)
        {
            return;
        }

        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        var moduleHandle = module is null ? nint.Zero : GetModuleHandle(module.ModuleName);

        hookHandle = SetWindowsHookEx(WhKeyboardLl, hookCallback, moduleHandle, 0);
        if (hookHandle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to install the global keyboard hook.");
        }
    }

    public void Stop()
    {
        if (hookHandle == nint.Zero)
        {
            return;
        }

        _ = UnhookWindowsHookEx(hookHandle);
        hookHandle = nint.Zero;
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    private nint HandleHook(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && (wParam == WmKeyDown || wParam == WmSysKeyDown))
        {
            var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            if (TryClassifyKey(data.VirtualKeyCode, out var character))
            {
                CharacterCaptured?.Invoke(this, new KeyCapturedEventArgs(character));
            }
        }

        return CallNextHookEx(hookHandle, nCode, wParam, lParam);
    }

    // Classify the virtual key as a word boundary (' '), a regular typing character ('a'),
    // or not a typing key at all (return false).
    //
    // We use VK codes directly rather than calling ToUnicodeEx/GetKeyboardState because
    // calling ToUnicodeEx inside a WH_KEYBOARD_LL hook mutates the system dead-key state
    // and corrupts keyboard input in every running application.
    private static bool TryClassifyKey(uint vk, out char character)
    {
        character = default;

        // Word boundary keys
        if (vk is 0x20 or 0x0D) // VK_SPACE, VK_RETURN
        {
            character = ' ';
            return true;
        }

        if (IsTypingKey(vk))
        {
            character = 'a'; // Only the whitespace distinction matters for session counting
            return true;
        }

        return false;
    }

    private static bool IsTypingKey(uint vk)
    {
        if (vk >= 0x30 && vk <= 0x39) return true; // 0-9
        if (vk >= 0x41 && vk <= 0x5A) return true; // A-Z
        if (vk >= 0x60 && vk <= 0x6F) return true; // Numpad 0-9 and operators
        if (vk >= 0xBA && vk <= 0xC0) return true; // OEM: ; = , - . / `
        if (vk >= 0xDB && vk <= 0xDF) return true; // OEM: [ \ ] '
        if (vk == 0xE2) return true;                // OEM_102: < > |
        return false;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hmod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string? lpModuleName);

    private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct KbdLlHookStruct
    {
        public uint VirtualKeyCode { get; init; }
        public uint ScanCode { get; init; }
        public uint Flags { get; init; }
        public uint Time { get; init; }
        public nint ExtraInfo { get; init; }
    }
}

public sealed class KeyCapturedEventArgs : EventArgs
{
    public KeyCapturedEventArgs(char character)
    {
        Character = character;
    }

    public char Character { get; }
}
