namespace LimitsEditor.Services;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default);
}
