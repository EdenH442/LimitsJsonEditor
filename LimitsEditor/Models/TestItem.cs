namespace LimitsEditor.Models;

public sealed class TestItem
{
    public string TestName { get; set; } = string.Empty;

    public TestType TestType { get; set; } = TestType.Single;

    public List<TestValue> TestValues { get; set; } = new();
}
