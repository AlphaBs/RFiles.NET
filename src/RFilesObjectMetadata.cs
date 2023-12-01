using System.Text.Json.Serialization;

namespace RFiles.NET;

public class RFilesObjectMetadata
{
    [JsonConstructor]
    public RFilesObjectMetadata(
        string hash, 
        long size, 
        DateTimeOffset uploaded) =>
        (Hash, Size, Uploaded) = 
        (hash, size, uploaded);

    [JsonPropertyName("md5")]
    public string Hash { get; }
    [JsonPropertyName("size")]
    public long Size { get; }
    [JsonPropertyName("uploaded")]
    public DateTimeOffset Uploaded { get; }
}