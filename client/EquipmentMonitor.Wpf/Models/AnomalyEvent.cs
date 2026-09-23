using System;
using System.Text.Json.Serialization;

namespace EquipmentMonitor.Wpf.Models
{
    public class AnomalyEvent
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("sensor_reading_id")]
        public long SensorReadingId { get; set; }

        [JsonPropertyName("failure_type")]
        public string? FailureType { get; set; }

        [JsonPropertyName("failure_probability")]
        public double FailureProbability { get; set; }

        [JsonPropertyName("failure_type_probability")]
        public double? FailureTypeProbability { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
