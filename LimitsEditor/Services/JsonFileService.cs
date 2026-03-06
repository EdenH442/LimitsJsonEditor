using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed class JsonFileService : IJsonFileService
{
    public Task<LimitaDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // TODO: Validate path, parse JSON, map to strongly typed models.
        throw new NotImplementedException();
    }

    public Task SaveAsync(string filePath, LimitaDocument document, CancellationToken cancellationToken = default)
    {
        // TODO: Serialize strongly typed document and persist to file.
        throw new NotImplementedException();
    }
}
