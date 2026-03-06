namespace LimitsEditor.Services;

public sealed class BackupService : IBackupService
{
    public Task<BackupResult> CreateBackupAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        // TODO: Create timestamped backup before saving edits.
        throw new NotImplementedException();
    }
}
