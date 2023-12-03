using CommandLine;

namespace RFilesCLI;

[Verb("sync")]
public class SyncCommand : CommandBase
{
    [Value(0, Required = true)]
    public IEnumerable<string>? Hashes { get; set; }

    protected override async Task<int> RunCommand()
    {
        if (Hashes == null)
            throw new ArgumentException("hashes value was not provided");
        
        var hashesArr = Hashes.ToArray();
        if (hashesArr.Length == 0)
            throw new ArgumentException("hashes was empty collection");

        Console.WriteLine($"Sync {hashesArr.Length} hashes");

        var client = CreateClient();
        var syncResult = await client.Sync(hashesArr);

        var objects = syncResult.Objects;
        var objectCount = 0;
        foreach (var robj in objects)
        {
            objectCount++;
            Console.WriteLine("#" + objectCount);
            ConsoleWriter.WriteLine(robj);
        }
        Console.WriteLine($"Total {objectCount} objects");

        var uploads = syncResult.Uploads;
        var uploadCount = 0;
        foreach (var upload in uploads)
        {
            uploadCount++;
            Console.WriteLine("#" + uploadCount);
            ConsoleWriter.WriteLine(upload);
        }
        Console.WriteLine($"Total {uploadCount} uploads");

        return 0;
    }
}