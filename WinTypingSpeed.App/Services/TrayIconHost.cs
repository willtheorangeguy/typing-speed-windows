using System.Drawing;
using System.Windows.Forms;
using WinTypingSpeed.Core;

namespace WinTypingSpeed.App.Services;

public sealed class TrayIconHost : IDisposable
{
    private readonly NotifyIcon notifyIcon;
    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem wpmItem;
    private readonly ToolStripMenuItem charactersItem;
    private readonly ToolStripMenuItem wordsItem;
    private readonly ToolStripMenuItem activeTimeItem;
    private readonly ToolStripMenuItem pauseItem;
    private readonly ToolStripMenuItem resumeItem;

    public TrayIconHost(Action openAction, Action pauseAction, Action resumeAction, Action resetAction, Action exitAction)
    {
        statusItem = new ToolStripMenuItem("Status: Tracking") { Enabled = false };
        wpmItem = new ToolStripMenuItem("Current WPM: 0.0") { Enabled = false };
        charactersItem = new ToolStripMenuItem("Typed characters: 0") { Enabled = false };
        wordsItem = new ToolStripMenuItem("Estimated words: 0") { Enabled = false };
        activeTimeItem = new ToolStripMenuItem("Active time: 00:00:00") { Enabled = false };
        pauseItem = new ToolStripMenuItem("Pause", null, (_, _) => pauseAction());
        resumeItem = new ToolStripMenuItem("Resume", null, (_, _) => resumeAction());

        var openItem = new ToolStripMenuItem("Open", null, (_, _) => openAction());
        var resetItem = new ToolStripMenuItem("Reset", null, (_, _) => resetAction());
        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) => exitAction());

        var menu = new ContextMenuStrip();
        menu.Items.Add(openItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(statusItem);
        menu.Items.Add(wpmItem);
        menu.Items.Add(charactersItem);
        menu.Items.Add(wordsItem);
        menu.Items.Add(activeTimeItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(pauseItem);
        menu.Items.Add(resumeItem);
        menu.Items.Add(resetItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = SystemIcons.Application,
            Text = "Win Typing Speed",
            Visible = true
        };

        notifyIcon.DoubleClick += (_, _) => openAction();
    }

    public void UpdateSnapshot(TypingSessionSnapshot snapshot)
    {
        statusItem.Text = $"Status: {(snapshot.IsPaused ? "Paused" : "Tracking")}";
        wpmItem.Text = $"Current WPM: {snapshot.CurrentWpm:F1}";
        charactersItem.Text = $"Typed characters: {snapshot.TypedCharacterCount}";
        wordsItem.Text = $"Estimated words: {snapshot.EstimatedWordCount}";
        activeTimeItem.Text = $"Active time: {snapshot.ActiveTime:hh\\:mm\\:ss}";
        pauseItem.Enabled = !snapshot.IsPaused;
        resumeItem.Enabled = snapshot.IsPaused;
        notifyIcon.Text = BuildTooltip(snapshot);
    }

    public void Dispose()
    {
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string BuildTooltip(TypingSessionSnapshot snapshot)
    {
        var tooltip = $"WPM {snapshot.CurrentWpm:F1} | Words {snapshot.EstimatedWordCount} | {snapshot.ActiveTime:hh\\:mm\\:ss}";
        return tooltip.Length <= 63 ? tooltip : tooltip[..63];
    }
}
