using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using EquipmentMonitor.Wpf.Models;
using EquipmentMonitor.Wpf.ViewModels;

namespace EquipmentMonitor.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public ISeries[] TemperatureSeries { get; set; }

        public Axis[] XAxes { get; set; } 

        public ISeries[] FailureTypeSeries { get; set; }

        public Axis[] FailureTypeXAxes { get; set; }

        private bool _isloading = false;

        private readonly MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

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
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadLatestSensorDataAsync();
                await LoadHistoryAsync();

                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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
            if (_isloading)
                return;

            _isloading = true;

            try
            {
                await LoadLatestSensorDataAsync();
                await LoadHistoryAsync();
            }
            catch (HttpRequestException)
            {
                StatusText.Text = "SERVER DISCONNECTED";
                StatusText.Foreground = Brushes.OrangeRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "오류");
            }
            finally
            {
                _isloading = false;
            }
        }

        private async Task LoadHistoryAsync()
        {
            await _viewModel.LoadHistoryAsync();

            List<AnomalyEvent>? history =
                _viewModel.History;

            if (history == null)
                return;

            HistoryGrid.ItemsSource = history;

            AnomalyCountText.Text =
                $"Anomaly Count: {history.Count}";

            LatestFailureTypeText.Text =
                $"Latest Failure Type: {_viewModel.LatestFailureType}";

            FailureTypeCountText.Text =
                $"Failure Types: {_viewModel.FailureTypeSummary}";

            FailureTypeSeries[0].Values =
                _viewModel.FailureCounts;

            FailureTypeXAxes[0].Labels =
                _viewModel.FailureLabels;
        }
    }
}