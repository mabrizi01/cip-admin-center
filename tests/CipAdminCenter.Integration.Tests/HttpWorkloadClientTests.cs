using System.Net;
using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Integration;

namespace CipAdminCenter.Integration.Tests;

public class HttpWorkloadClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("done") };
        }
    }

    [Fact]
    public async Task Forwards_request_to_the_workload_base_address()
    {
        var handler = new StubHandler();
        var client = new HttpWorkloadClient(new HttpClient(handler));

        var response = await client.SendAsync(
            new WorkloadRequest(new Uri("https://workload.example.org/api"), "POST", "/items/1", "{}", "application/json"), CancellationToken.None);

        Assert.Equal(202, response.StatusCode);
        Assert.Equal("done", response.Body);
        Assert.Equal("https://workload.example.org/api/items/1", handler.Request!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.Request.Method);
        Assert.Equal("{}", handler.Body);
    }

    [Fact]
    public async Task Rejects_absolute_paths_so_requests_cannot_leave_the_workload()
    {
        var client = new HttpWorkloadClient(new HttpClient(new StubHandler()));

        await Assert.ThrowsAsync<ArgumentException>(() => client.SendAsync(
            new WorkloadRequest(new Uri("https://workload.example.org"), "GET", "https://evil.example.org/x", null, null), CancellationToken.None));
    }
}
