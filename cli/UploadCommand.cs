using CommandLine;
using RFiles.NET;

namespace RFilesCLI;

[Verb("upload")]
public class UploadCommand : CommandBase
{
    [Value(0, Required = true)]
    public string? FilePath { get; set; }

    [Option("existMode")]
    public string? ExistMode { get; set; }

    [Option('y', "yes", Default = false, HelpText = "Do not prompt before uploading a file")]
    public bool DoNotPromptUpload { get; set; }

    [Option('h', "hash", HelpText = "")]
    public string? Hash { get; set; }

    protected async override Task<int> RunCommand()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new ArgumentException("FilePath value was not provided");
        if (!File.Exists(FilePath))
            throw new FileNotFoundException(FilePath);

        Console.WriteLine($"Upload the file {FilePath}, existMode={ExistMode}");
        var client = CreateClient();
        RFilesUploadRequest uploadRequest;
        if (string.IsNullOrEmpty(Hash))
            uploadRequest = await client.Upload(FilePath, ExistMode ?? string.Empty);
        else
        {
            using var stream = File.OpenRead(FilePath);
            uploadRequest = await client.Upload(stream, Hash, ExistMode ?? string.Empty);
        }

        Console.WriteLine($"Method: {uploadRequest.Method}");
        Console.WriteLine($"Url: {uploadRequest.Url}");
        foreach (var kv in uploadRequest.Headers)
        {
            Console.WriteLine($"{kv.Key}: {kv.Value}");
        }

        bool uploadPromptResult = false;
        if (DoNotPromptUpload)
        {
            uploadPromptResult = true;
        }
        else
        {
            Console.WriteLine("\nDo you really want to upload this file? (y/n)");
            var prompt = Console.ReadLine()?.ToLowerInvariant();
            if (prompt == "y")
                uploadPromptResult = true;
            else if (prompt == "n")
                uploadPromptResult = false;
        }

        if (!uploadPromptResult)
        {
            Console.WriteLine("Aborted");
            return 1;
        }

        Console.WriteLine("Uploading...");
        using var uploadFile = File.OpenRead(FilePath);
        using var reqMessage = uploadRequest.ToHttpRequestMessage(uploadFile);
        using var res = await HttpClient.SendAsync(reqMessage);

        if (res.IsSuccessStatusCode)
        {
            Console.WriteLine($"{FilePath} was successfully uploaded");
            return 0;
        }
        else
        {
            var resBody = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to upload file ({(int)res.StatusCode})\n{resBody}");
            return -1;
        }
    }
}