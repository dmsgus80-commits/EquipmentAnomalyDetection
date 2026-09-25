using System.Text.Json.Serialization;

namespace EquipmentMonitor.Wpf.Models
{
    public class AlarmSummary
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("unacknowledged")]
        public int Unacknowledged { get; set; }

        [JsonPropertyName("acknowledged")]
        public int Acknowledged { get; set; }

        [JsonPropertyName("resolved")]
        public int Resolved { get; set; }
    }
}
