using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Abstractions;

/// <summary>Raised when a referenced aggregate does not exist.</summary>
public sealed class NotFoundException(string message) : Exception(message);

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>A request to act on platform functionality in an optional organisational context / Solution.</summary>
public sealed record PlatformAccessRequest(string Operation, ContextRef? Context = null, Guid? SolutionId = null);

/// <summary>
/// Extension point for future authentication and authorisation. Roles, permissions and identity
/// providers are intentionally deferred; Infrastructure currently registers a permissive placeholder.
/// </summary>
public interface IPlatformAccessGuard
{
    Task EnsureAccessAsync(PlatformAccessRequest request, CancellationToken cancellationToken);
}
