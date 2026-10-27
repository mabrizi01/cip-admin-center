using CipAdminCenter.Application.Abstractions;

namespace CipAdminCenter.Infrastructure.Access;

/// <summary>
/// Placeholder for the deferred authentication/authorisation design. It permits every request and
/// must be replaced before any non-development deployment. No roles or permissions are defined yet.
/// </summary>
internal sealed class DeferredPlatformAccessGuard : IPlatformAccessGuard
{
    public Task EnsureAccessAsync(PlatformAccessRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
}
