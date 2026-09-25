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
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("acknowledged_at")]
        public DateTime? AcknowledgedAt { get; set; }

        [JsonPropertyName("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [JsonIgnore]
        public bool CanAcknowledge =>
            Status == "UNACKNOWLEDGED";

        [JsonIgnore]
        public bool CanResolve =>
            Status == "ACKNOWLEDGED";

        [JsonIgnore]
        public string StatusText =>
            Status switch
            {
                "UNACKNOWLEDGED" => "미확인",
                "ACKNOWLEDGED" => "확인돰",
                "RESOLVED" => "조치 완료",
                _ => Status ?? "_"
            };
    }
}
