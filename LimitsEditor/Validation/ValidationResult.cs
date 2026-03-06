namespace LimitsEditor.Validation;

public sealed class ValidationResult
{
    public bool IsValid => Issues.Count == 0;

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    private readonly List<ValidationIssue> _issues = new();

    public void AddIssue(ValidationIssue issue)
    {
        _issues.Add(issue);
    }

    public void AddIssues(IEnumerable<ValidationIssue> issues)
    {
        _issues.AddRange(issues);
    }
}
