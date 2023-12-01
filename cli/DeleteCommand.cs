using CommandLine;

namespace RFilesCLI;

[Verb("delete")]
public class DeleteCommand : CommandBase
{
    [Value(0, Required = true)]
    public string? Hash { get; set; }

    protected override async Task<int> RunCommand()
    {
        if (string.IsNullOrEmpty(Hash))
            throw new ArgumentException("hash value was not provided");

        Console.WriteLine($"Delete {Hash} from {Host}");

        var client = CreateClient();
        await client.Delete(Hash);

        Console.WriteLine($"{Hash} was successfully deleted from {Host}");
        return 0;
    }
}