using System;
using System.Text.Json.Serialization;

namespace EquipmentMonitor.Wpf.Models
{
    public class SensorReading
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("air_temperature")]
        public double AirTemperature { get; set; }

        [JsonPropertyName("process_temperature")]
        public double ProcessTemperature { get; set; }

        [JsonPropertyName("rotational_speed")]
        public int RotationalSpeed { get; set; }

        [JsonPropertyName("torque")]
        public double Torque { get; set; }

        [JsonPropertyName("tool_wear")]
        public int ToolWear { get; set; }

        [JsonPropertyName("machine_failure")]
        public int MachineFailure { get; set; }

        [JsonPropertyName("failure_probability")]
        public double FailureProbability { get; set; }

        [JsonPropertyName("failure_type")]
        public string? FailureType { get; set; }

        [JsonPropertyName("failure_type_probability")]
        public double? FailureTypeProbability { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}