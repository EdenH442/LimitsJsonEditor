using System.Text.Json.Serialization;

namespace LimitsEditor.Models;

public sealed class Limit
{
    [JsonPropertyName("MultipleStepNameCheck")]
    public string MultipleStepNameCheck { get; set; } = string.Empty;

    [JsonPropertyName("LimitType")]
    public string LimitType { get; set; } = string.Empty;

    [JsonPropertyName("ComparisonType")]
    public string ComparisonType { get; set; } = string.Empty;

    [JsonPropertyName("ThresholdType")]
    public string ThresholdType { get; set; } = string.Empty;

    [JsonPropertyName("ExpectedRes")]
    public string ExpectedRes { get; set; } = string.Empty;

    [JsonPropertyName("Low")]
    public double? Low { get; set; }

    [JsonPropertyName("High")]
    public double? High { get; set; }

    [JsonPropertyName("Unit")]
    public string Unit { get; set; } = string.Empty;
}
