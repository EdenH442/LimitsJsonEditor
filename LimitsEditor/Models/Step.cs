using System.Text.Json.Serialization;

namespace LimitsEditor.Models;

public sealed class Step
{
    [JsonPropertyName("StepName")]
    public string StepName { get; set; } = string.Empty;

    [JsonPropertyName("StepType")]
    public string StepType { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonIgnore]
    public StepType StepTypeValue
    {
        get => StepTypeSerialization.FromSerialized(StepType);
        set => StepType = StepTypeSerialization.ToSerialized(value);
    }

    [JsonPropertyName("limitList")]
    public List<Limit> LimitList { get; set; } = new();
}
