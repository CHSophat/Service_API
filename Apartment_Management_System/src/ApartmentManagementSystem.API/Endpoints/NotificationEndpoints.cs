using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/notifications/* + /api/v1/push/* + /api/v1/notification-prefs.
/// Shared App + Web with App-only device-token endpoints.
/// </summary>
public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var n = app.MapGroup($"{EndpointRegistration.ApiBase}/notifications")
                   .WithTags("Notifications (Shared)")
                   .RequireAuthorization();

        n.MapGet("/", () =>
            EndpointResults.NotImplemented("GetNotificationsQuery"));

        n.MapGet("/unread-count", () =>
            EndpointResults.NotImplemented("GetUnreadNotificationCountQuery"));

        n.MapPost("/{id:int}/read", (int id) =>
            EndpointResults.NotImplemented("MarkNotificationReadCommand"));

        n.MapPost("/mark-all-read", () =>
            EndpointResults.NotImplemented("MarkAllNotificationsReadCommand"));

        // Push device registration (App)
        var push = app.MapGroup($"{EndpointRegistration.ApiBase}/push")
                      .WithTags("Push (App)")
                      .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        push.MapPost("/devices", () =>
            EndpointResults.NotImplemented("RegisterPushDeviceCommand (FCM/APNs)"));

        push.MapDelete("/devices/{token}", (string token) =>
            EndpointResults.NotImplemented("UnregisterPushDeviceCommand"));

        // Preferences (App)
        var prefs = app.MapGroup($"{EndpointRegistration.ApiBase}/notification-prefs")
                       .WithTags("Notification Prefs (App)")
                       .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        prefs.MapGet("/", () =>
            EndpointResults.NotImplemented("GetNotificationPrefsQuery"));

        prefs.MapPut("/", () =>
            EndpointResults.NotImplemented("UpdateNotificationPrefsCommand"));

        return app;
    }
}
