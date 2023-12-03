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

    public async Task<IEnumerable<RFilesObjectMetadata>> GetAllObjects()
    {
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/md5?return=object")
        });
        using var stream = await res.Content.ReadAsStreamAsync();
        var objects = await JsonSerializer.DeserializeAsync<IEnumerable<RFilesObjectMetadata>>(stream, _jsonOptions);
        return objects.Where(obj => obj != null);
    }

    public async Task<IEnumerable<string>> GetAllHashes()
    {
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/md5?return=hash")
        });
        using var stream = await res.Content.ReadAsStreamAsync();
        var hashes = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(stream, _jsonOptions);
        return hashes.Where(hash => !string.IsNullOrEmpty(hash));
    }

    public async Task<IEnumerable<RFilesObjectMetadata>> Query(IEnumerable<string> hashes)
    {
        using var reqContent = serializeHashQueries(hashes);
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Host}/query"),
            Content = reqContent
        });
        using var resStream = await res.Content.ReadAsStreamAsync();
        var objects = await JsonSerializer.DeserializeAsync<IEnumerable<RFilesObjectMetadata>>(resStream, _jsonOptions);
        return objects.Where(obj => obj != null);
    }

    public async Task<RFilesSyncResult> Sync(IEnumerable<string> hashes)
    {
        using var reqContent = serializeHashQueries(hashes);
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Host}/sync"),
            Content = reqContent
        });
        using var resStream = await res.Content.ReadAsStreamAsync();
        var resJson = await JsonSerializer.DeserializeAsync<RFilesSyncResult>(resStream);
        return resJson ?? throw new RFilesException(200, "bad_response");
    }

    private HttpContent serializeHashQueries(IEnumerable<string> hashes)
    {
        var reqBytes = JsonSerializer.SerializeToUtf8Bytes(
            new { md5 = hashes },
            _jsonOptions);
        var reqStream = new MemoryStream(reqBytes);
        var reqContent = new StreamContent(reqStream);
        reqContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
        return reqContent;
    }

    public async Task<RFilesObjectMetadata?> Head(string hash)
    {
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Head,
            RequestUri = new Uri($"{Host}/md5/{hash}")
        });
        return parseMetadataFromHeaders(hash, res.Content.Headers);
    }

    public async Task<RFilesObject> Download(string hash)
    {
        using var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}/md5/{hash}")
        });

        var metadata = parseMetadataFromHeaders(hash, res.Content.Headers);
        var stream = await res.Content.ReadAsStreamAsync(); // do not dispose stream in this time
        return new RFilesObject(stream, metadata);          // RFilesObject would dispose the stream
    }

    public async Task Delete(string hash)
    {
        await request(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{Host}/md5/{hash}"),
        });
    }

    public async Task<RFilesUploadRequest> Upload(string path, string existMode = "")
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        using var stream = File.OpenRead(path);
        using var hasher = MD5.Create();
        var hashBytes = hasher.ComputeHash(stream);
        var hashStr = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        stream.Position = 0;
        return await Upload(stream, hashStr, existMode);
    }

    public async Task<RFilesUploadRequest> Upload(Stream stream, string hash, string existMode = "")
    {
        var reqContent = new StreamContent(stream);
        var res = await request(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Host}/md5/{hash}?exists={existMode}"),
            Content = reqContent
        });
        using var resStream = await res.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<RFilesUploadRequest>(resStream, _jsonOptions) ??
            throw new RFilesException((int)res.StatusCode, "invalid_response");
    }

    private async Task<HttpResponseMessage> request(HttpRequestMessage reqMessage)
    {
        reqMessage.Headers.Add("x-client-secret", ClientSecret);

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