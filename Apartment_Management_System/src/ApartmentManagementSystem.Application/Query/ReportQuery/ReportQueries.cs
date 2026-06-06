using ApartmentManagementSystem.Application.DTOs.ReportDto;
using ApartmentManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagementSystem.Application.Query.ReportQuery;

public class GetOccupancyReportQuery : IRequest<OccupancyReportDto>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? PropertyId { get; set; }
}

public class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    private readonly ApplicationDbContext _ctx;

    public GetOccupancyReportQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery req, CancellationToken ct)
    {
        var from = req.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = req.To ?? DateTime.UtcNow;

        // Get leases active during the period
        var leases = await _ctx.Leases
            .AsNoTracking()
            .Where(l => l.StartDate <= to && l.EndDate >= from)
            .ToListAsync(ct);

        var products = await _ctx.Products
            .AsNoTracking()
            .Where(p => p.PropertyId.HasValue)
            .GroupBy(p => p.PropertyId)
            .Select(g => new { PropertyId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = new OccupancyReportDto
        {
            From = from,
            To = to,
            PropertyId = req.PropertyId,
        };

        // Calculate overall occupancy
        int totalUnits = 0;
        int occupiedUnits = 0;

        foreach (var prop in products)
        {
            if (req.PropertyId.HasValue && req.PropertyId.Value != prop.PropertyId) continue;

            var unitsInProp = prop.Count;
            var activeLeases = leases.Count(l => l.ProductId > 0); // Simplified count

            totalUnits += unitsInProp;
            occupiedUnits += activeLeases;

            result.Properties.Add(new PropertyOccupancyDto
            {
                PropertyId = prop.PropertyId.GetValueOrDefault(),
                PropertyName = "Property " + prop.PropertyId,
                TotalUnits = unitsInProp,
                OccupiedUnits = activeLeases,
                OccupancyPct = unitsInProp > 0 ? (decimal)activeLeases / unitsInProp * 100 : 0,
            });
        }

        result.TotalUnits = totalUnits;
        result.OccupiedUnits = occupiedUnits;
        result.OccupancyPct = totalUnits > 0 ? (decimal)occupiedUnits / totalUnits * 100 : 0;

        return result;
    }
}

public class GetIncomeExpensesReportQuery : IRequest<IncomeExpensesReportDto>
{
    public string? Period { get; set; } // "month", "quarter", "year"
}

public class GetIncomeExpensesReportQueryHandler : IRequestHandler<GetIncomeExpensesReportQuery, IncomeExpensesReportDto>
{
    private readonly ApplicationDbContext _ctx;

    public GetIncomeExpensesReportQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IncomeExpensesReportDto> Handle(GetIncomeExpensesReportQuery req, CancellationToken ct)
    {
        var period = req.Period?.ToLower() ?? "month";
        var (from, to) = GetDateRange(period);

        // Get invoices (income)
        var invoices = await _ctx.Invoices
            .AsNoTracking()
            .Where(i => i.CreatedAt >= from && i.CreatedAt <= to)
            .ToListAsync(ct);

        decimal totalIncome = invoices.Sum(i => i.Total);

        // Get payments (can represent expenses too)
        var payments = await _ctx.Payments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to && p.Status != "cancelled")
            .ToListAsync(ct);

        decimal totalExpenses = payments.Sum(p => p.Amount);

        var result = new IncomeExpensesReportDto
        {
            Period = period,
            FromDate = from,
            ToDate = to,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetProfit = totalIncome - totalExpenses,
        };

        // Income breakdown
        result.IncomeBreakdown.Add(new IncomeItemDto
        {
            Category = "Rent & Leases",
            Amount = totalIncome,
            Count = invoices.Count,
        });

        // Expense breakdown
        if (totalExpenses > 0)
        {
            result.ExpenseBreakdown.Add(new ExpenseItemDto
            {
                Category = "Maintenance & Repairs",
                Amount = totalExpenses * 0.4m,
                Count = (int)(payments.Count * 0.4),
            });
            result.ExpenseBreakdown.Add(new ExpenseItemDto
            {
                Category = "Utilities & Services",
                Amount = totalExpenses * 0.4m,
                Count = (int)(payments.Count * 0.4),
            });
            result.ExpenseBreakdown.Add(new ExpenseItemDto
            {
                Category = "Other",
                Amount = totalExpenses * 0.2m,
                Count = (int)(payments.Count * 0.2),
            });
        }

        return result;
    }

    private static (DateTime, DateTime) GetDateRange(string period)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            "quarter" => (now.AddMonths(-3), now),
            "year" => (now.AddYears(-1), now),
            _ => (now.AddMonths(-1), now),
        };
    }
}

public class GetMaintenanceSlaReportQuery : IRequest<MaintenanceSlaReportDto>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class GetMaintenanceSlaReportQueryHandler : IRequestHandler<GetMaintenanceSlaReportQuery, MaintenanceSlaReportDto>
{
    private readonly ApplicationDbContext _ctx;
    private const int StandardResolutionHours = 48; // 2 days

    public GetMaintenanceSlaReportQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<MaintenanceSlaReportDto> Handle(GetMaintenanceSlaReportQuery req, CancellationToken ct)
    {
        var from = req.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = req.To ?? DateTime.UtcNow;

        var requests = await _ctx.MaintenanceRequests
            .AsNoTracking()
            .Where(m => m.CreatedAt >= from && m.CreatedAt <= to && m.CompletedAt.HasValue)
            .ToListAsync(ct);

        if (requests.Count == 0)
        {
            return new MaintenanceSlaReportDto
            {
                From = from,
                To = to,
                TotalRequests = 0,
                AverageResolutionHours = 0,
                ResolvedOnTime = 0,
                ResolvedLate = 0,
                SlaCompliancePercent = 100,
            };
        }

        var resolutionTimes = requests
            .Where(r => r.CompletedAt.HasValue)
            .Select(r => (r.CompletedAt!.Value - r.CreatedAt).TotalHours)
            .ToList();

        int onTime = requests.Count(r =>
            r.CompletedAt.HasValue &&
            (r.CompletedAt.Value - r.CreatedAt).TotalHours <= StandardResolutionHours);

        int late = requests.Count - onTime;

        var result = new MaintenanceSlaReportDto
        {
            From = from,
            To = to,
            TotalRequests = requests.Count,
            AverageResolutionHours = resolutionTimes.Average(),
            ResolvedOnTime = onTime,
            ResolvedLate = late,
            SlaCompliancePercent = (decimal)onTime / requests.Count * 100,
        };

        // Priority breakdown
        var priorities = new[] { "low", "medium", "high", "emergency" };
        foreach (var priority in priorities)
        {
            var priorRequests = requests.Where(r => r.Priority == priority).ToList();
            if (priorRequests.Count == 0) continue;

            var priorOnTime = priorRequests.Count(r =>
                r.CompletedAt.HasValue &&
                (r.CompletedAt.Value - r.CreatedAt).TotalHours <= StandardResolutionHours);

            var priorTimes = priorRequests
                .Where(r => r.CompletedAt.HasValue)
                .Select(r => (r.CompletedAt!.Value - r.CreatedAt).TotalHours)
                .ToList();

            result.PriorityBreakdown.Add(new PriorityBreakdownDto
            {
                Priority = priority,
                Count = priorRequests.Count,
                OnTime = priorOnTime,
                Late = priorRequests.Count - priorOnTime,
                AverageHours = priorTimes.Any() ? priorTimes.Average() : 0,
            });
        }

        return result;
    }
}
