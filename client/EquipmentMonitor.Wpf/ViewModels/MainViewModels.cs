using System.Collections.Generic;
using System.Threading.Tasks;
using EquipmentMonitor.Wpf.Models;
using EquipmentMonitor.Wpf.Services;
using System.Linq;

namespace EquipmentMonitor.Wpf.ViewModels
{
    public class MainViewModel
    {
        private readonly ApiService _apiService;

        public List<SensorReading>? SensorReadings { get; private set; }

        public List<AnomalyEvent>? History { get; private set; }

        public double AverageTemperature { get; private set; }

        public double AverageTorque { get; private set; }

        public int AnomalyCount { get; private set; }

        public double AnomalyRate { get; private set; }

        public string LatestFailureType { get; private set; } = "None";

        public string FailureTypeSummary { get; private set; } = "";

        public List<int> FailureCounts { get; private set; } = new();

        public List<string> FailureLabels { get; private set; } = new();

        public List<double> AirTemperatureValues { get; private set; } = new();

        public List<double> ProcessTemperatureValues { get; private set; } = new();

        public List<string> TimeLabels { get; private set; } = new();

        public SensorReading? LatestReading { get; private set; }

        public bool IsFailure { get; private set; }

        public string StatusMessage { get; private set; } = "NORMAL";

        public MainViewModel()
        {
            _apiService = new ApiService();
        }

        public async Task LoadSensorReadingsAsync()
        {
            SensorReadings =
                await _apiService.GetSensorReadingsAsync();

            if (SensorReadings == null || SensorReadings.Count == 0)
                return;

            LatestReading = SensorReadings[0];

            IsFailure = LatestReading.MachineFailure == 1;

            if (IsFailure)
                StatusMessage = $"DANGER - {LatestReading.FailureType}";
            else
                StatusMessage = "NORMAL";

            var recentReadings =
                SensorReadings
                    .Take(20)
                    .Reverse()
                    .ToList();

            AverageTemperature =
                recentReadings.Average(r => r.AirTemperature);

            AverageTorque = 
                recentReadings.Average(t => t.Torque);

            AnomalyCount = 
                SensorReadings.Count(r => r.MachineFailure == 1);

            AnomalyRate =
                (double)AnomalyCount / SensorReadings.Count;

            AirTemperatureValues =
                recentReadings
                    .Select(r => r.AirTemperature)
                    .ToList();

            ProcessTemperatureValues =
                recentReadings
                    .Select(r => r.ProcessTemperature)
                    .ToList();

            TimeLabels =
                recentReadings
                    .Select(r => r.CreatedAt.ToString("HH:mm:ss"))
                    .ToList();
        }

        public async Task LoadHistoryAsync()
        {
            History =
                await _apiService.GetHistoryAsync();

            if (History == null)
                return;

            if (History.Count > 0)
                LatestFailureType = History[0].FailureType ?? "Unknown";
            else
                LatestFailureType = "None";

            var failureGroups =
                History
                    .GroupBy(h => h.FailureType)
                    .ToList();

            FailureCounts =
                failureGroups
                    .Select(g => g.Count())
                    .ToList();

            FailureLabels =
                failureGroups
                    .Select(g => g.Key ?? "Unknown")
                    .ToList();

            FailureTypeSummary =
                string.Join(
                    " | ",
                    failureGroups.Select(
                        g => $"{g.Key ?? "Unknown"}: {g.Count()}"
                    )
                );
        }
    }
}