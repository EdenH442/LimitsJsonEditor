using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed class JsonUpsertService : IJsonUpsertService
{
    public UpsertResult Upsert(LimitaDocument document, UpsertTestRequest request)
    {
        // TODO: Implement sequence/test lookup, create-if-missing behavior, and overwrite policy.
        throw new NotImplementedException();
    }
}
