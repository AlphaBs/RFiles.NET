using CommandLine;

namespace RFilesCLI;

[Verb("listhash")]
public class ListHashCommand : CommandBase
{
    protected async override Task<int> RunCommand()
    {
        Console.WriteLine($"List all hashes from {Host}");

        var client = CreateClient();
        var objects = await client.GetAllHashes();
        int count = 0;
        foreach (var hash in objects)
        {
            Console.WriteLine(hash);
            count++;
        }

        Console.WriteLine($"Total {count} objects");
        return 0;
    }
}