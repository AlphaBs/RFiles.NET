using CommandLine;

namespace RFilesCLI;

[Verb("listhash")]
public class ListHashCommand : CommandBase
{
    protected async override Task<int> RunCommand()
    {
        Console.WriteLine($"List all hashes from {Host}");

        var client = CreateClient();
        var objects = client.GetAllHashes();
        int count = 0;
        await foreach (var hash in objects)
        {
            Console.WriteLine(hash);
            count++;
        }

        Console.WriteLine($"Total {count} objects");
        return 0;
    }
}