using System.Text;
using CipAdminCenter.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CipAdminCenter.Integration;

/// <summary>HTTP adapter for the <see cref="IWorkloadClient"/> port. Contains no business rules; the target is chosen by the Application layer.</summary>
public sealed class HttpWorkloadClient(HttpClient httpClient) : IWorkloadClient
{
    public async Task<WorkloadResponse> SendAsync(WorkloadRequest request, CancellationToken ct)
    {
        if (!Uri.TryCreate(request.RelativePath, UriKind.Relative, out _))
        {
            throw new ArgumentException("The path must be relative to the workload base address.", nameof(request));
        }

        // Keep a trailing slash on the base so relative paths are appended rather than replacing its last segment.
        var baseAddress = request.BaseAddress.AbsoluteUri.EndsWith('/') ? request.BaseAddress : new Uri(request.BaseAddress.AbsoluteUri + "/");
        using var message = new HttpRequestMessage(new HttpMethod(request.Method), new Uri(baseAddress, new Uri(request.RelativePath.TrimStart('/'), UriKind.Relative)));
        if (request.Body is not null)
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentType ?? "application/json");
        }

        using var response = await httpClient.SendAsync(message, ct);
        return new WorkloadResponse(
            (int)response.StatusCode,
            await response.Content.ReadAsStringAsync(ct),
            response.Content.Headers.ContentType?.ToString());
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IWorkloadClient, HttpWorkloadClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
        return services;
    }
}
