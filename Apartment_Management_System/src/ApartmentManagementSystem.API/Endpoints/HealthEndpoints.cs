using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /health/* – liveness + readiness for k8s / load-balancer probes. Anonymous.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/health").WithTags("Health");

        g.MapGet("/status", [AllowAnonymous] () => Results.Ok(new
        {
            status = "ok",
            service = "ApartmentManagementSystem.API",
            timestamp = DateTime.UtcNow
        }));

        g.MapGet("/ready", [AllowAnonymous] () => Results.Ok(new { ready = true }));
        g.MapGet("/live",  [AllowAnonymous] () => Results.Ok(new { live = true }));

        return app;
    }
}
