using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using EquipmentMonitor.Wpf.Models;
using EquipmentMonitor.Wpf.ViewModels;
using System.Windows.Controls;

namespace EquipmentMonitor.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public ISeries[] TemperatureSeries { get; set; }

        public Axis[] XAxes { get; set; }

        public ISeries[] FailureTypeSeries { get; set; }

        public Axis[] FailureTypeXAxes { get; set; }

        private bool _isLoading = false;

        private readonly MainViewModel _viewModel = new MainViewModel();

        public ISeries[] AlarmStatusSeries { get; set; }

        public Axis[] AlarmStatusXAxes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            AlertStatusFilter.SelectionChanged += AlertStatusFilter_SelectionChanged;

            Loaded += MainWindow_Loaded;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            TemperatureSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Air Temperature",
                    Values = new List<double>()
                },

                new LineSeries<double>
                {
                    Name = "Process Temperature",
                    Values = new List<double>()
                }
            };

            TemperatureChart.Series = TemperatureSeries;

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string>()
                }
            };

            TemperatureChart.XAxes = XAxes;

            FailureTypeSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new List<int>()
                }
            };

            FailureTypeChart.Series = FailureTypeSeries;

            FailureTypeXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string>()
                }
            };

            FailureTypeChart.XAxes = FailureTypeXAxes;

            AlarmStatusSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new List<int> { 0, 0, 0 }
                }
            };

            AlarmStatusXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string>
                    {
                        "미확인",
                        "확인됨",
                        "조치 완료"
                    }
                }
            };
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Start();
            _isLoading = true;

            try
            {
                await LoadLatestSensorDataAsync();
                await LoadHistoryAsync();
                ShowConnectionStatus(true);
            }
            catch (Exception)
            {
                ShowConnectionStatus(false);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ShowConnectionStatus(bool connected)
        {
            ConnectionText.Text = connected ? "서버 연결됨" : "서버 연결 끊김";
            ConnectionText.Foreground = connected ? Brushes.ForestGreen : Brushes.OrangeRed;
            if (connected)
                LastUpdatedText.Text = $"마지막 갱신: {DateTime.Now:HH:mm:ss}";
        }

        private async Task LoadLatestSensorDataAsync()
        {
            await _viewModel.LoadSensorReadingsAsync();

            List<SensorReading>? readings =
                _viewModel.SensorReadings;

            if (readings == null || readings.Count == 0)
                return;

            SensorReading? latest = _viewModel.LatestReading;

            if (latest == null)
                return;

            AirTemperatureText.Text =
                $"Air Temperature: {latest.AirTemperature} K";

            ProcessTemperatureText.Text =
                $"Process Temperature: {latest.ProcessTemperature} K";

            RpmText.Text =
                $"RPM: {latest.RotationalSpeed}";

            TorqueText.Text =
                $"Torque: {latest.Torque} Nm";

            ToolWearText.Text =
                $"Tool Wear: {latest.ToolWear} min";

            FailureProbabilityText.Text =
                $"Failure Probability: {latest.FailureProbability:P1}";

            if (latest.MachineFailure == 1 &&
                latest.FailureTypeProbability.HasValue)
            {
                FailureTypeProbabilityText.Text =
                    $"Failure Type Probability: {latest.FailureTypeProbability.Value:P1}";
            }
            else
            {
                FailureTypeProbabilityText.Text = "";
            }

            StatusText.Text = _viewModel.StatusMessage;

            StatusText.Foreground =
                _viewModel.IsFailure
                    ? Brushes.Red
                    : Brushes.Green;

            TemperatureSeries[0].Values =
                _viewModel.AirTemperatureValues;

            TemperatureSeries[1].Values =
                _viewModel.ProcessTemperatureValues;

            XAxes[0].Labels =
                _viewModel.TimeLabels;

            AverageTemperatureText.Text =
                $"Average Air Temperature: {_viewModel.AverageTemperature:F1} K";

            AverageTorqueText.Text =
                $"Average Torque: {_viewModel.AverageTorque:F1} Nm";

            AnomalyRateText.Text =
                $"Anomaly Rate: {_viewModel.AnomalyRate:P1}";
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            if (_isLoading)
                return;

            _isLoading = true;

            try
            {
                await LoadLatestSensorDataAsync();
                await LoadHistoryAsync();
                ShowConnectionStatus(true);
            }
            catch (HttpRequestException)
            {
                ShowConnectionStatus(false);
            }
            catch (TaskCanceledException)
            {
                ShowConnectionStatus(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "오류");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadHistoryAsync()
        {
            await _viewModel.LoadHistoryAsync();

            List<AnomalyEvent>? history =
                _viewModel.History;

            if (history == null)
                return;

            ApplyAlarmFilter();

            UnacknowledgedCountText.Text =
                _viewModel.UnacknowledgedCount.ToString();

            UnacknowledgedHeaderText.Text =
                $"미확인 알림: {_viewModel.UnacknowledgedCount}";

            AlertTabCountText.Text =
                $" ({_viewModel.UnacknowledgedCount})";

            AcknowledgedCountText.Text =
                _viewModel.AcknowledgedCount.ToString();

            ResolvedCountText.Text =
                _viewModel.ResolvedCount.ToString();

            AnomalyCountText.Text =
                $"전체 알림: {_viewModel.TotalAlarmCount}";

            LatestFailureTypeText.Text =
                $"Latest Failure Type: {_viewModel.LatestFailureType}";

            FailureTypeCountText.Text =
                $"Failure Types: {_viewModel.FailureTypeSummary}";

            FailureTypeSeries[0].Values =
                _viewModel.FailureCounts;

            FailureTypeXAxes[0].Labels =
                _viewModel.FailureLabels;

            AlarmStatusSeries[0].Values =
                _viewModel.AlarmStatusCounts;

            AlarmStatusXAxes[0].Labels =
                _viewModel.AlarmStatusLabels;
        }

        private void AlertStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyAlarmFilter();
        }

        private void ApplyAlarmFilter()
        {
            List<AnomalyEvent>? history = _viewModel.History;
            if (history == null)
                return;

            string? status = (AlertStatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            HistoryGrid.ItemsSource = status == null || status == "ALL"
                ? history
                : history.Where(alarm => alarm.Status == status).ToList();
        }

        private async void AcknowledgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext is AnomalyEvent alarm &&
                alarm.CanAcknowledge && !_isLoading)
            {
                _isLoading = true;
                try
                {
                    await _viewModel.AcknowledgeAlarmAsync(alarm.Id);
                    await LoadHistoryAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "알림 확인 실패");
                }
                finally
                {
                    _isLoading = false;
                }
            }
        }

        private async void ResolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext is AnomalyEvent alarm &&
                alarm.CanResolve && !_isLoading)
            {
                _isLoading = true;
                try
                {
                    await _viewModel.ResolveAlarmAsync(alarm.Id);
                    await LoadHistoryAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "조치 완료 실패");
                }
                finally
                {
                    _isLoading = false;
                }
            }
        }
    }
}
