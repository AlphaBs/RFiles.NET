using RFiles.NET;

namespace RFilesCLI;

public static class RFilesUtil
{
    public static void WriteMetadata(RFilesObjectMetadata metadata)
    {
        Console.WriteLine($"Hash: {metadata.Hash}");
        Console.WriteLine($"Size: {metadata.Size}");
        Console.WriteLine($"Uploaded: {metadata.Uploaded}");
    }
}