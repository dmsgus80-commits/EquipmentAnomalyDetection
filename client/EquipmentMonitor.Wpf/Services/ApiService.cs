using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EquipmentMonitor.Wpf.Models;

namespace EquipmentMonitor.Wpf.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task<List<SensorReading>?> GetSensorReadingsAsync()
        {
            string url = "http://127.0.0.1:8000/sensor-readings";

            string json = await _httpClient.GetStringAsync(url);

            return JsonSerializer.Deserialize<List<SensorReading>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<List<AnomalyEvent>?> GetHistoryAsync()
        {
            string url = "http://127.0.0.1:8000/history";

            string json = await _httpClient.GetStringAsync(url);

            return JsonSerializer.Deserialize<List<AnomalyEvent>>(json);
        }

        public async Task<AlarmSummary?> GetAlarmSummaryAsync()
        {
            string json = await _httpClient.GetStringAsync(
                "http://127.0.0.1:8000/history/summary"
            );

            return JsonSerializer.Deserialize<AlarmSummary>(json);
        }

        public async Task AcknowledgeAlarmAsync(long alarmId)
        {
            string url =
                $"http://127.0.0.1:8000/alarms/{alarmId}/acknowledge";

            using HttpResponseMessage response = await _httpClient.PatchAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task ResolveAlarmAsync(long alarmId)
        {
            string url =
                $"http://127.0.0.1:8000/alarms/{alarmId}/resolve";

            using HttpResponseMessage response = await _httpClient.PatchAsync(url, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
