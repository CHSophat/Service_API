using ApartmentManagementSystem.Application.Query.ReportQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/reports/* – Web-only PM reports (occupancy, income/expenses, SLA, export).
/// </summary>
public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/reports")
                   .WithTags("Reports (Web)")
                   .RequireAuthorization(p => p.RequireRole(
                       AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/occupancy", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? propertyId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOccupancyReportQuery { From = from, To = to, PropertyId = propertyId });
            return ApiResponseExtensions.Ok(result, "Occupancy report retrieved");
        });

        g.MapGet("/income-expenses", async ([FromQuery] string? period, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetIncomeExpensesReportQuery { Period = period });
            return ApiResponseExtensions.Ok(result, "Income/Expenses report retrieved");
        });

        g.MapGet("/maintenance-sla", async ([FromQuery] DateTime? from, [FromQuery] DateTime? to, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMaintenanceSlaReportQuery { From = from, To = to });
            return ApiResponseExtensions.Ok(result, "Maintenance SLA report retrieved");
        });

        g.MapGet("/export", ([FromQuery] string? type, [FromQuery] string? format) =>
        {
            // Placeholder for export functionality - returns CSV/PDF
            var csvData = "type,value\noccupancy,85%\nincome,5000\n";
            return Results.File(
                System.Text.Encoding.UTF8.GetBytes(csvData),
                format == "pdf" ? "application/pdf" : "text/csv",
                $"report_{type}_{DateTime.UtcNow:yyyyMMdd}.{(format == "pdf" ? "pdf" : "csv")}"
            );
        });

        return app;
    }
}
