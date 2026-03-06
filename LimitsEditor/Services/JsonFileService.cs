using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed class JsonFileService : IJsonFileService
{
    public Task<JsonLoadResult> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // TODO: Validate path, parse JSON, map to strongly typed models.
        throw new NotImplementedException();
    }

    public Task<JsonSaveResult> SaveAsync(string filePath, LimitaDocument document, CancellationToken cancellationToken = default)
    {
        // TODO: Serialize strongly typed document and persist to file.
        throw new NotImplementedException();
    }
}
