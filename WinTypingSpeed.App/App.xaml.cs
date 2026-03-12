using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using WinTypingSpeed.App.Services;
using WinTypingSpeed.Core;

namespace WinTypingSpeed.App;

public partial class App : System.Windows.Application
{
    private readonly TypingSessionTracker tracker = new();
    private readonly DispatcherTimer refreshTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    private GlobalKeyboardHook? keyboardHook;
    private TrayIconHost? trayIconHost;
    private MainWindow? mainWindow;
    private bool resumeAfterSystemPause;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        refreshTimer.Tick += RefreshTimer_OnTick;

        mainWindow = new MainWindow();
        mainWindow.PauseRequested += (_, _) => PauseTracking();
        mainWindow.ResumeRequested += (_, _) => ResumeTracking();
        mainWindow.ResetRequested += (_, _) => ResetSession();

        trayIconHost = new TrayIconHost(
            openAction: ShowMainWindow,
            pauseAction: PauseTracking,
            resumeAction: ResumeTracking,
            resetAction: ResetSession,
            exitAction: ExitApplication);

        keyboardHook = new GlobalKeyboardHook();
        keyboardHook.CharacterCaptured += KeyboardHook_OnCharacterCaptured;

        try
        {
            keyboardHook.Start();
        }
        catch (Win32Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Win Typing Speed could not start global keyboard tracking.\n\n{ex.Message}",
                "Startup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            ExitApplication();
            return;
        }

        SystemEvents.PowerModeChanged += SystemEvents_OnPowerModeChanged;
        SystemEvents.SessionSwitch += SystemEvents_OnSessionSwitch;

        refreshTimer.Start();
        RefreshUi();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        refreshTimer.Stop();
        refreshTimer.Tick -= RefreshTimer_OnTick;

        SystemEvents.PowerModeChanged -= SystemEvents_OnPowerModeChanged;
        SystemEvents.SessionSwitch -= SystemEvents_OnSessionSwitch;

        if (keyboardHook is not null)
        {
            keyboardHook.CharacterCaptured -= KeyboardHook_OnCharacterCaptured;
            keyboardHook.Dispose();
            keyboardHook = null;
        }

        trayIconHost?.Dispose();
        trayIconHost = null;

        base.OnExit(e);
    }

    private void KeyboardHook_OnCharacterCaptured(object? sender, KeyCapturedEventArgs e)
    {
        // BeginInvoke (async) is required here. The WH_KEYBOARD_LL hook fires on the UI
        // thread; Dispatcher.Invoke (sync) would deadlock waiting for itself to unblock.
        Dispatcher.BeginInvoke(() =>
        {
            tracker.RecordCharacter(e.Character);
            RefreshUi();
        });
    }

    private void RefreshTimer_OnTick(object? sender, EventArgs e)
    {
        RefreshUi();
    }

    private void ShowMainWindow()
    {
        mainWindow?.ShowFromTray();
    }

    private void PauseTracking()
    {
        tracker.Pause();
        resumeAfterSystemPause = false;
        RefreshUi();
    }

    private void ResumeTracking()
    {
        tracker.Resume();
        resumeAfterSystemPause = false;
        RefreshUi();
    }

    private void ResetSession()
    {
        tracker.Reset();
        RefreshUi();
    }

    private void ExitApplication()
    {
        if (mainWindow is not null)
        {
            mainWindow.AllowClose = true;
            mainWindow.Close();
            mainWindow = null;
        }

        Shutdown();
    }

    private void RefreshUi()
    {
        var snapshot = tracker.GetSnapshot();
        mainWindow?.ApplySnapshot(snapshot);
        trayIconHost?.UpdateSnapshot(snapshot);
    }

    private void SystemEvents_OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
        {
            PauseForSystemEvent();
            return;
        }

        if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            ResumeAfterSystemEvent();
        }
    }

    private void SystemEvents_OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
        {
            PauseForSystemEvent();
            return;
        }

        if (e.Mode == PowerModes.Resume)
        {
            ResumeAfterSystemEvent();
        }
    }

    private void PauseForSystemEvent()
    {
        var snapshot = tracker.GetSnapshot();
        if (snapshot.IsPaused)
        {
            return;
        }

        resumeAfterSystemPause = true;
        tracker.Pause();
        RefreshUi();
    }

    private void ResumeAfterSystemEvent()
    {
        if (!resumeAfterSystemPause)
        {
            return;
        }

        resumeAfterSystemPause = false;
        tracker.Resume();
        RefreshUi();
    }
}
