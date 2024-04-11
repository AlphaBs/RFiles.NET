using System.Text.Json.Serialization;

namespace RFiles.NET;

public class RFilesUploadRequest
{
    [JsonConstructor]
    public RFilesUploadRequest(
        string hash,
        string method, 
        string url,
        IReadOnlyDictionary<string, string> headers) =>
        (Hash, Method, Url, Headers) = (hash, method, url, headers);

    [JsonPropertyName("md5")]
    public string Hash { get; }

    [JsonPropertyName("method")]
    public string Method { get; }

    [JsonPropertyName("url")]
    public string Url { get; }

    [JsonPropertyName("headers")]
    public IReadOnlyDictionary<string, string> Headers { get; }

    public HttpRequestMessage ToHttpRequestMessage(Stream uploadStream)
    {
        var content = new StreamContent(uploadStream);
        return ToHttpRequestMessage(content);
    }

    public HttpRequestMessage ToHttpRequestMessage(HttpContent content)
    {
        var reqMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(Method),
            RequestUri = new Uri(Url),
            Content = content
        };
        
        foreach (var kv in Headers)
        {
            if (!content.Headers.TryAddWithoutValidation(kv.Key, kv.Value) && 
                !reqMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                throw new InvalidOperationException($"Cannot add header: {kv.Key}: {kv.Value}");
        }

        return reqMessage;
    }
}