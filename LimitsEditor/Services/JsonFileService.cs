using LimitsEditor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LimitsEditor.Services;

public sealed class JsonFileService : IJsonFileService
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Include,
        Formatting = Formatting.Indented,
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter(new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy(), allowIntegerValues: true)
        }
    };

    public async Task<JsonLoadResult> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new JsonLoadResult
            {
                Status = OperationStatus.ValidationFailed,
                Message = "File path is required."
            };
        }

        if (!File.Exists(filePath))
        {
            return new JsonLoadResult
            {
                Status = OperationStatus.NotFound,
                Message = "JSON file was not found."
            };
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            var document = JsonConvert.DeserializeObject<LimitaDocument>(json, SerializerSettings) ?? new LimitaDocument();

            return new JsonLoadResult
            {
                Status = OperationStatus.Success,
                Document = document,
                Message = "File loaded successfully."
            };
        }
        catch (OperationCanceledException)
        {
            return new JsonLoadResult
            {
                Status = OperationStatus.Cancelled,
                Message = "File load cancelled."
            };
        }
        catch (Exception ex)
        {
            return new JsonLoadResult
            {
                Status = OperationStatus.Failed,
                Message = $"Failed to load JSON file: {ex.Message}"
            };
        }
    }

    public async Task<JsonSaveResult> SaveAsync(string filePath, LimitaDocument document, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new JsonSaveResult
            {
                Status = OperationStatus.ValidationFailed,
                Message = "File path is required."
            };
        }

        try
        {
            var json = JsonConvert.SerializeObject(document, SerializerSettings);
            await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);

            return new JsonSaveResult
            {
                Status = OperationStatus.Success,
                Message = "File saved successfully."
            };
        }
        catch (OperationCanceledException)
        {
            return new JsonSaveResult
            {
                Status = OperationStatus.Cancelled,
                Message = "File save cancelled."
            };
        }
        catch (Exception ex)
        {
            return new JsonSaveResult
            {
                Status = OperationStatus.Failed,
                Message = $"Failed to save JSON file: {ex.Message}"
            };
        }
    }
}
