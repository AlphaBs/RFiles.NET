using System.Text.Json;
using CommandLine;
using RFiles.NET;

namespace RFilesCLI;

public abstract class CommandBase
{
    [Option('h', "host", Required = false, HelpText = "Server host")]
    public string? Host { get; set; }

    [Option('s', "secret", Required = false, HelpText = "X-Client-Secret")]
    public string? ClientSecret { get; set; }

    protected ObjectWriter ConsoleWriter = new();
    protected HttpClient HttpClient { get; set; } = new HttpClient();
    protected JsonSerializerOptions JsonOptions { get; set; } = JsonSerializerOptions.Default;

    protected RFilesClient CreateClient()
    {
        if (string.IsNullOrEmpty(Host))
            throw new ArgumentException("Host value was not provided");
        
        var client = new RFilesClient(Host, HttpClient, JsonOptions);
        client.ClientSecret = ClientSecret;
        return client;
    }

    public int Run()
    {
        if (string.IsNullOrEmpty(Host))
            Host = Environment.GetEnvironmentVariable("RFILES_HOST");
        if (string.IsNullOrEmpty(Host))
            Host = "http://localhost";
        if (string.IsNullOrEmpty(ClientSecret))
            ClientSecret = Environment.GetEnvironmentVariable("RFILES_CLIENT_SECRET");

        return asyncWrapper().GetAwaiter().GetResult();
    }

    private async Task<int> asyncWrapper()
    {
        try
        {
            return await RunCommand();
        }
        catch (Exception ex)
        {
            if (ex is RFilesException)
            {
                Console.WriteLine($"RFilesError: {ex.Message}");
                return -1;
            }
            else
            {
                throw;
            }
        }
    }

    protected abstract Task<int> RunCommand();
}