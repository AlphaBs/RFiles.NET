namespace RFiles.NET;

public class RFilesObject : IDisposable
{
    public RFilesObject(
        Stream content,
        RFilesObjectMetadata metadata) =>
        (Content, Metadata) = 
        (content, metadata);

    public Stream Content { get; }
    public RFilesObjectMetadata Metadata { get; }

    public void Dispose()
    {
        Content.Dispose();
    }
}