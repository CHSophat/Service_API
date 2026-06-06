namespace ApartmentManagementSystem.Application.DTOs.ReportDto;

public class OccupancyReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int? PropertyId { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal OccupancyPct { get; set; }
    public List<PropertyOccupancyDto> Properties { get; set; } = new();
}

public class PropertyOccupancyDto
{
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal OccupancyPct { get; set; }
}

public class IncomeExpensesReportDto
{
    public string Period { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public List<IncomeItemDto> IncomeBreakdown { get; set; } = new();
    public List<ExpenseItemDto> ExpenseBreakdown { get; set; } = new();
}

public class IncomeItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class ExpenseItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class MaintenanceSlaReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public double AverageResolutionHours { get; set; }
    public int TotalRequests { get; set; }
    public int ResolvedOnTime { get; set; }
    public int ResolvedLate { get; set; }
    public decimal SlaCompliancePercent { get; set; }
    public List<PriorityBreakdownDto> PriorityBreakdown { get; set; } = new();
}

public class PriorityBreakdownDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OnTime { get; set; }
    public int Late { get; set; }
    public double AverageHours { get; set; }
}

public class ExportReportDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
