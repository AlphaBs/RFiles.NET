using CommandLine;

namespace RFilesCLI;

[Verb("list")]
public class ListCommand : CommandBase
{
    protected async override Task<int> RunCommand()
    {
        Console.WriteLine($"List all objects from {Host}");

        var client = CreateClient();
        var objects = client.GetAllObjects();
        int count = 0;
        await foreach (var robj in objects)
        {
            count++;
            Console.WriteLine("#" + count);
            RFilesUtil.WriteMetadata(robj);
        }
        Console.WriteLine($"Total {count} objects");
        return 0;
    }
}