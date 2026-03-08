using System.Text.Json.Serialization;

namespace LimitsEditor.Models;

public sealed class Sequence
{
    [JsonPropertyName("seqName")]
    public string SeqName { get; set; } = string.Empty;

    [JsonPropertyName("stepList")]
    public List<Step> StepList { get; set; } = new();
}
