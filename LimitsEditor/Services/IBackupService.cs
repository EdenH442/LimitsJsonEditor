namespace LimitsEditor.Services;

public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default);
}
