using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// Single entry point that wires every minimal-API endpoint group into the app.
/// Call <c>app.MapApiEndpoints()</c> once from <c>Program.cs</c>.
/// </summary>
public static class EndpointRegistration
{
    public const string ApiBase = "/api/v1";

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapCustomerEndpoints();
        app.MapPropertyEndpoints();
        app.MapProductEndpoints();
        app.MapListingEndpoints();
        app.MapLeaseEndpoints();
        app.MapMaintenanceEndpoints();
        app.MapInvoiceEndpoints();
        app.MapPaymentEndpoints();
        app.MapMessageEndpoints();
        app.MapAnnouncementEndpoints();
        app.MapNotificationEndpoints();
        app.MapReportEndpoints();
        app.MapSettingsEndpoints();
        app.MapUploadEndpoints();
        app.MapHealthEndpoints();
        return app;
    }
}
