using CommandLine;

namespace RFilesCLI;

[Verb("head")]
public class HeadCommand : CommandBase
{
    [Value(0, Required = true)]
    public string? Hash { get; set; }

    protected async override Task<int> RunCommand()
    {
        if (string.IsNullOrEmpty(Hash))
            throw new ArgumentException("hash value was not provided");

        Console.WriteLine($"Head {Hash} from {Host}");

        var client = CreateClient();
        var metadata = await client.Head(Hash);
        if (metadata == null)
        {
            Console.WriteLine($"The object {Hash} exists but no metadata");
        }
        else
        {
            RFilesUtil.WriteMetadata(metadata);
        }

        return 0;
    }
}