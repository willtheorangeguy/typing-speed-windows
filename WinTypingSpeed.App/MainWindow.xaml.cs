using System.ComponentModel;
using System.Windows;
using WinTypingSpeed.Core;

namespace WinTypingSpeed.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public bool AllowClose { get; set; }

    public event EventHandler? PauseRequested;

    public event EventHandler? ResumeRequested;

    public event EventHandler? ResetRequested;

    public void ApplySnapshot(TypingSessionSnapshot snapshot)
    {
        WpmValueText.Text = $"{snapshot.CurrentWpm:F1}";
        StatusValueText.Text = snapshot.IsPaused ? "Paused" : "Tracking";
        CharactersValueText.Text = snapshot.TypedCharacterCount.ToString();
        WordsValueText.Text = snapshot.EstimatedWordCount.ToString();
        ActiveTimeValueText.Text = snapshot.ActiveTime.ToString("hh\\:mm\\:ss");
        PauseButton.IsEnabled = !snapshot.IsPaused;
        ResumeButton.IsEnabled = snapshot.IsPaused;
    }

    public void ShowFromTray()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private void PauseButton_OnClick(object sender, RoutedEventArgs e)
    {
        PauseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ResumeButton_OnClick(object sender, RoutedEventArgs e)
    {
        ResumeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ResetButton_OnClick(object sender, RoutedEventArgs e)
    {
        ResetRequested?.Invoke(this, EventArgs.Empty);
    }
}
