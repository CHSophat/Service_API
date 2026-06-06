using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/payments/* – tenant payments (Bakong QR, methods, history, receipts)
/// and PM reconciliation (unmatched + match/unmatch). All endpoints are
/// catalog gaps. Bakong webhook is intentionally public (verified by signature).
/// </summary>
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/payments").WithTags("Payments (Shared)");

        // ---- Web (PM) -------------------------------------------------------
        g.MapGet("/", ([FromQuery] DateTime? date, [FromQuery] string? status, [FromQuery] int? propertyId) =>
            EndpointResults.NotImplemented("GetPaymentsQuery (PM)"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/unmatched", () =>
            EndpointResults.NotImplemented("GetUnmatchedPaymentsQuery"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/{id:int}/match", (int id, [FromBody] MatchInvoiceRequest body) =>
            EndpointResults.NotImplemented($"MatchPaymentCommand (paymentId={id}, invoiceId={body.InvoiceId})"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapDelete("/matches/{matchId:int}", (int matchId) =>
            EndpointResults.NotImplemented("RemovePaymentMatchCommand"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        // ---- App (Tenant) ---------------------------------------------------
        g.MapPost("/", () =>
            EndpointResults.NotImplemented("CreatePaymentCommand (tenant pays)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapGet("/{id:int}", (int id) =>
            EndpointResults.NotImplemented("GetPaymentByIdQuery"))
         .RequireAuthorization();

        g.MapGet("/history", ([FromQuery] int customerId) =>
            EndpointResults.NotImplemented("GetPaymentHistoryQuery"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapGet("/methods", () =>
            EndpointResults.NotImplemented("GetPaymentMethodsQuery"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapPost("/methods", () =>
            EndpointResults.NotImplemented("AddPaymentMethodCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapDelete("/methods/{id:int}", (int id) =>
            EndpointResults.NotImplemented("RemovePaymentMethodCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        // Bakong
        g.MapPost("/bakong-qr", () =>
            EndpointResults.NotImplemented("RequestBakongQrCommand (return qrString + paymentId)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapPost("/bakong/webhook", [AllowAnonymous] () =>
            EndpointResults.NotImplemented("HandleBakongWebhookCommand (verify signature, mark paid)"));

        g.MapPost("/{id:int}/confirm", (int id) =>
            EndpointResults.NotImplemented("ConfirmPaymentCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapPost("/{id:int}/cancel", (int id) =>
            EndpointResults.NotImplemented("CancelPaymentCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapGet("/{id:int}/receipt", (int id) =>
            EndpointResults.NotImplemented("GetReceiptPdfQuery"))
         .RequireAuthorization();

        return app;
    }

    public sealed record MatchInvoiceRequest(int InvoiceId);
}
