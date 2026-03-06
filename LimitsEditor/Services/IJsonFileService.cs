using LimitsEditor.Models;

namespace LimitsEditor.Services;

public interface IJsonFileService
{
    Task<LimitaDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default);

    Task SaveAsync(string filePath, LimitaDocument document, CancellationToken cancellationToken = default);
}
