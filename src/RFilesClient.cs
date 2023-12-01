using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.Json;

namespace RFiles.NET;

public class RFilesClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RFilesClient(
        string host,
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions) =>
        (Host, _httpClient, _jsonOptions) = 
        (host, httpClient, jsonOptions);

    public string Host { get; }
    public string? ClientSecret { get; set; }

    public async IAsyncEnumerable<RFilesObjectMetadata> GetAllObjects()
    {
        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/objects?return=object")
        });
        var stream = await res.Content.ReadAsStreamAsync();
        var objects = JsonSerializer.DeserializeAsyncEnumerable<RFilesObjectMetadata>(stream, _jsonOptions);
        await foreach (var obj in objects)
        {
            if (obj != null)
                yield return obj;
        }
    }

    public async IAsyncEnumerable<string> GetAllHashes()
    {
        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/objects?return=md5")
        });
        var stream = await res.Content.ReadAsStreamAsync();
        var hashes = JsonSerializer.DeserializeAsyncEnumerable<string>(stream, _jsonOptions);
        await foreach (var hash in hashes)
        {
            if (!string.IsNullOrEmpty(hash))
                yield return hash;
        }
    }

    public async IAsyncEnumerable<RFilesObjectMetadata> Query(IEnumerable<string> hashes)
    {
        var reqBytes = JsonSerializer.SerializeToUtf8Bytes(
            new { hashes }, 
            _jsonOptions);
        using var reqStream = new MemoryStream(reqBytes);
        var reqContent = new StreamContent(reqStream);
        reqContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Host}/objects/query"),
            Content = reqContent
        });
        var resStream = await res.Content.ReadAsStreamAsync();

        var objects = JsonSerializer.DeserializeAsyncEnumerable<RFilesObjectMetadata>(resStream, _jsonOptions);
        await foreach (var obj in objects)
        {
            if (obj != null)
                yield return obj;
        }
    }

    public async Task<RFilesObjectMetadata?> Head(string hash)
    {
        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Head,
            RequestUri = new Uri($"{Host}/objects/{hash}")
        });
        return parseMetadataFromHeaders(hash, res.Content.Headers);
    }

    public async Task<RFilesObject> Download(string hash)
    {
        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/objects/{hash}")
        });

        var metadata = parseMetadataFromHeaders(hash, res.Content.Headers);
        var stream = await res.Content.ReadAsStreamAsync();
        return new RFilesObject(stream, metadata);
    }

    public async Task Delete(string hash)
    {
        await request(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{Host}/objects/{hash}"),
        });
    }

    public async Task Upload(string path, string existMode = "")
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);
        
        using var stream = File.OpenRead(path);
        using var hasher = MD5.Create();
        var hashBytes = hasher.ComputeHash(stream);
        var hashStr = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        stream.Position = 0;
        await Upload(stream, hashStr, existMode);
    }

    public async Task Upload(Stream stream, string hash, string existMode = "")
    {
        var reqContent = new StreamContent(stream);
        var res = await _httpClient.PutAsync($"{Host}/objects/{hash}?exists={existMode}", reqContent);
        res.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> request(HttpRequestMessage reqMessage)
    {
        reqMessage.Headers.Add("X-Client-Secret", ClientSecret);

        var resMessage = await _httpClient.SendAsync(reqMessage);
        if (!resMessage.IsSuccessStatusCode)
            throw await createException(resMessage);

        return resMessage;
    }

    private async Task<RFilesException> createException(HttpResponseMessage resMessage)
    {
        string? error = null;
        var statusCode = (int)resMessage.StatusCode;
        try
        {
            var resStream = await resMessage.Content.ReadAsStreamAsync();
            using var resJson = await JsonDocument.ParseAsync(resStream);
            error = resJson.RootElement.GetProperty("error").GetString();
        }
        catch
        {
            // any json-related exception
        }

        return new RFilesException(statusCode, error);
    }

    private RFilesObjectMetadata parseMetadataFromHeaders(string hash, HttpContentHeaders headers) =>
        new RFilesObjectMetadata(
            hash: hash,
            size: headers.ContentLength ?? 0,
            uploaded: headers.LastModified ?? DateTimeOffset.MinValue);
}