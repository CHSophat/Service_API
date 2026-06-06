using Microsoft.AspNetCore.Authorization;

namespace ApartmentManagementSystem.API.Authorization;


public sealed class AllowAllAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        // Snapshot to avoid mutating the collection while iterating.
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
