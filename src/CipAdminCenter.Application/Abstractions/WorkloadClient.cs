namespace CipAdminCenter.Application.Abstractions;

public sealed record WorkloadRequest(Uri BaseAddress, string Method, string RelativePath, string? Body, string? ContentType);

public sealed record WorkloadResponse(int StatusCode, string? Body, string? ContentType);

/// <summary>Port used to forward a request to a deployed workload. Implemented in Integration.</summary>
public interface IWorkloadClient
{
    Task<WorkloadResponse> SendAsync(WorkloadRequest request, CancellationToken ct);
}
