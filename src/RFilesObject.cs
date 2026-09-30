namespace RFiles.NET;

public class RFilesObject : IDisposable
{
    public RFilesObject(
        Stream content,
        RFilesObjectMetadata metadata) =>
        (Content, Metadata) = 
        (content, metadata);

    private readonly HttpResponseMessage? _response;

    internal RFilesObject(Stream content, RFilesObjectMetadata metadata, HttpResponseMessage response)
        : this(content, metadata) => _response = response;

    public Stream Content { get; }
    public RFilesObjectMetadata Metadata { get; }

    public void Dispose()
    {
        if (_response is not null)
            _response.Dispose();
        else
            Content.Dispose();
    }
}