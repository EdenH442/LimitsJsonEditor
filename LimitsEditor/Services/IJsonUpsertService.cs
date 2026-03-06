using LimitsEditor.Models;

namespace LimitsEditor.Services;

public interface IJsonUpsertService
{
    UpsertResult Upsert(LimitaDocument document, UpsertTestRequest request);
}
