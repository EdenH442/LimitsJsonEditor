using System.Text.Json.Serialization;

namespace LimitsEditor.Models;

public sealed class Sequence
{
    [JsonPropertyName("SequenceName")]
    public string SequenceName { get; set; } = string.Empty;

    [JsonPropertyName("stepList")]
    public List<Step> StepList { get; set; } = new();
}
