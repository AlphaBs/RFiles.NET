using CommandLine;

namespace RFilesCLI;

[Verb("query")]
public class QueryCommand : CommandBase
{
    [Value(0, Required = true)]
    public IEnumerable<string>? Hashes { get; set; }

    protected async override Task<int> RunCommand()
    {
        if (Hashes == null)
            throw new ArgumentException($"hashes value was not provided");

        var hashesArr = Hashes.ToArray();
        if (hashesArr.Length == 0)
            throw new ArgumentException("hashes was empty collection");

        Console.WriteLine($"Query {hashesArr.Length} hashes");

        var client = CreateClient();
        var objects = client.Query(hashesArr);
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
