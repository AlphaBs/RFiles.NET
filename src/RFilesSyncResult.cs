using System.Text.Json.Serialization;

namespace RFiles.NET;

public class RFilesSyncResult
{
    public RFilesSyncResult(
        IEnumerable<RFilesObjectMetadata> objects,
        IEnumerable<RFilesUploadRequest> uploads) =>
        (Objects, Uploads) = (objects, uploads);

    [JsonPropertyName("objects")]
    public IEnumerable<RFilesObjectMetadata> Objects { get; }

    [JsonPropertyName("uploads")]
    public IEnumerable<RFilesUploadRequest> Uploads { get; }
}