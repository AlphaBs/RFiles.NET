using CommandLine;

namespace RFilesCLI;

[Verb("upload")]
public class UploadCommand : CommandBase
{
    [Value(0, Required = true)]
    public string? FilePath { get; set; }

    [Option("existMode")]
    public string? ExistMode { get; set; }

    protected async override Task<int> RunCommand()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new ArgumentException("FilePath value was not provided");
        if (!File.Exists(FilePath))
            throw new FileNotFoundException(FilePath);

        Console.WriteLine($"Upload file {FilePath}, existMode={ExistMode}");
        var client = CreateClient();
        await client.Upload(FilePath, ExistMode ?? string.Empty);

        Console.WriteLine($"{FilePath} was successfully uploaded");
        return 0;
    }
}