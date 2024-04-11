using RFiles.NET;

namespace RFilesCLI;

public class ObjectWriter
{
    public StreamWriter Output { get; set; }

    public ObjectWriter()
    {
        Output = new StreamWriter(Console.OpenStandardOutput());
    }

    public void WriteLine(object obj)
    {
        if (obj is RFilesObjectMetadata metadata)
        {
            Output.WriteLine($"Hash: {metadata.Hash}");
            Output.WriteLine($"Size: {metadata.Size}");
            Output.WriteLine($"Uploaded: {metadata.Uploaded}");
        }
        else if (obj is RFilesUploadRequest uploadRequest)
        {
            Output.WriteLine($"Hash: {uploadRequest.Hash}");
            Output.WriteLine($"Method: {uploadRequest.Method}");
            Output.WriteLine($"Url: {uploadRequest.Url}");
            foreach (var kv in uploadRequest.Headers)
            {
                Output.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }
        else
        {
            throw new InvalidOperationException("Unsupported data type");
        }

        Output.Flush();
    }
}