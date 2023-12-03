using CommandLine;

namespace RFilesCLI;

[Verb("download")]
public class DownloadCommand : CommandBase
{
    [Option('f', "file", HelpText = "Download path")]
    public string? FilePath { get; set; }

    [Value(0, Required = true)]
    public string? Hash { get; set; }

    protected override async Task<int> RunCommand()
    {
        if (string.IsNullOrEmpty(Hash))
            throw new ArgumentException("hash value was not provided");

        Console.WriteLine($"Download {Hash} from {Host}");

        var client = CreateClient();
        using var robj = await client.Download(Hash);
        ConsoleWriter.WriteLine(robj.Metadata);

        var path = FilePath;
        if (string.IsNullOrEmpty(path))
            path = Hash;

        Console.WriteLine($"Start Download file to {path}");
        using var fileStream = File.Create(path);
        robj.Content.CopyTo(fileStream);

        Console.WriteLine($"Download was successfully done");
        return 0;
    }
}