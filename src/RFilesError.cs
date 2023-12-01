using System.Text.Json.Serialization;

namespace RFiles.NET;

public class RFilesError
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}