using LimitsEditor.Models;

namespace LimitsEditor.Services;

public interface IJsonFileService
{
    Task<JsonLoadResult> LoadAsync(string filePath, CancellationToken cancellationToken = default);

    Task<JsonSaveResult> SaveAsync(string filePath, LimitaDocument document, CancellationToken cancellationToken = default);
}
