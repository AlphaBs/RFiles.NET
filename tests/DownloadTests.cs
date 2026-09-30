using System.Net;
using System.Text.Json;
using Xunit;

namespace RFiles.NET.Tests;

public sealed class DownloadTests
{
    [Fact]
    public async Task Download_keeps_stream_readable_until_object_is_disposed()
    {
        var stream = new TrackingStream();
        var content = new StreamContent(stream);
        using var http = new HttpClient(new Handler(() => new(HttpStatusCode.OK) { Content = content }));
        var client = new RFilesClient("https://example.invalid", http, new JsonSerializerOptions());
        var result = await client.Download("test");
        Assert.False(stream.Disposed);
        Assert.Equal(1, result.Content.ReadByte());
        using var copy = new MemoryStream();
        await result.Content.CopyToAsync(copy);
        Assert.Equal(new byte[] { 2, 3 }, copy.ToArray());
        result.Dispose();
        Assert.True(stream.Disposed);
        result.Dispose();
    }

    [Fact]
    public async Task Download_does_not_buffer_response_body_before_returning()
    {
        var stream = new TrackingStream();
        using var http = new HttpClient(new Handler(() => new(HttpStatusCode.OK) { Content = new StreamContent(stream) }));
        var client = new RFilesClient("https://example.invalid", http, new JsonSerializerOptions());
        using var result = await client.Download("test");
        Assert.Equal(0, stream.Reads);
    }

    [Fact]
    public async Task Failed_download_disposes_response_content()
    {
        var stream = new TrackingStream();
        using var http = new HttpClient(new Handler(() => new(HttpStatusCode.BadRequest) { Content = new StreamContent(stream) }));
        var client = new RFilesClient("https://example.invalid", http, new JsonSerializerOptions());
        await Assert.ThrowsAsync<RFilesException>(() => client.Download("test"));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public async Task Failed_open_disposes_response_content()
    {
        var content = new FailingContent();
        using var http = new HttpClient(new Handler(() => new(HttpStatusCode.OK) { Content = content }));
        var client = new RFilesClient("https://example.invalid", http, new JsonSerializerOptions());
        await Assert.ThrowsAsync<IOException>(() => client.Download("test"));
        Assert.True(content.Disposed);
    }

    [Fact]
    public void Public_constructor_still_owns_its_stream()
    {
        var stream = new TrackingStream();
        using (var result = new RFilesObject(stream, new RFilesObjectMetadata("test", 3, DateTimeOffset.MinValue)))
            Assert.False(stream.Disposed);
        Assert.True(stream.Disposed);
    }

    private sealed class Handler(Func<HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(response());
    }

    private sealed class TrackingStream() : MemoryStream(new byte[] { 1, 2, 3 })
    {
        public bool Disposed { get; private set; }
        public int Reads { get; private set; }
        public override int Read(byte[] buffer, int offset, int count) { Reads++; return base.Read(buffer, offset, count); }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default) { Reads++; return base.ReadAsync(buffer, token); }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }

    private sealed class FailingContent : HttpContent
    {
        public bool Disposed { get; private set; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw new IOException("synthetic read failure");
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task<Stream> CreateContentReadStreamAsync() => throw new IOException("synthetic stream failure");
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
