using System;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Defaults;

namespace WPM_Tracker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly KeyboardHook _keyboardHook;
        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        private readonly DispatcherTimer _resetTimer;
        private DateTime _startTime;
        private int _keyStrokes;
        private bool _isTracking;

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyPressed += OnKeyPressed;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppContext.BaseDirectory, "WPM-Tracker.ico"));
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "WPM: 0";

            SeriesCollection = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new ChartValues<double>(),
                    PointGeometrySize = 0,
                    StrokeThickness = 2
                }
            };

            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateWpm;
            timer.Start();

            _resetTimer = new DispatcherTimer();
            _resetTimer.Interval = TimeSpan.FromSeconds(2);
            _resetTimer.Tick += ResetWpm;

            DataContext = this;
        }

        private void OnKeyPressed(object? sender, char e)
        {
            if (!_isTracking)
            {
                _isTracking = true;
                _startTime = DateTime.Now;
                _keyStrokes = 0;
            }
            _keyStrokes++;
            _resetTimer.Stop();
            _resetTimer.Start();
        }

        private void UpdateWpm(object? sender, EventArgs e)
        {
            if (_isTracking)
            {
                var elapsed = (DateTime.Now - _startTime).TotalMinutes;
                if (elapsed > 0)
                {
                    var wpm = (int)((_keyStrokes / 5.0) / elapsed);
                    WpmLabel.Text = wpm.ToString();
                    _notifyIcon.Text = $"WPM: {wpm}";

                    // Add data to chart
                    SeriesCollection[0].Values.Add((double)wpm);
                }
            }
        }

        private void ResetWpm(object? sender, EventArgs? e)
        {
            _isTracking = false;
            _keyStrokes = 0;
            WpmLabel.Text = "0";
            _notifyIcon.Text = "WPM: 0";
            _resetTimer.Stop();
            SeriesCollection[0].Values.Clear(); // Clear chart on reset
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetWpm(null, null);
        }

        private void ClearChartButton_Click(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
        }

        protected override void OnClosed(EventArgs e)
        {
            _keyboardHook.Dispose();
            _notifyIcon.Dispose();
            base.OnClosed(e);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
