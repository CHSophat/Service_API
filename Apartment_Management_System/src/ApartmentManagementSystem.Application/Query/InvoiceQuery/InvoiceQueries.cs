using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.FinancialDto;
using ApartmentManagementSystem.Domain.Entities.Financial;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.InvoiceQuery;

public class GetInvoicesQuery : IRequest<List<InvoiceDto>>
{
    public string? Status { get; set; }
    public int? PropertyId { get; set; }
    public string? Period { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, List<InvoiceDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetInvoicesQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<InvoiceDto>> Handle(GetInvoicesQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

        var q = _ctx.Invoices.AsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(i => i.Status == req.Status);
        
        if (req.PropertyId.HasValue)
            q = q.Where(i => i.PropertyId == req.PropertyId.Value);

        if (!string.IsNullOrWhiteSpace(req.Q))
        {
            var term = req.Q.Trim().ToLower();
            q = q.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term));
        }

        var rows = await q
            .OrderByDescending(i => i.IssueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(MapToDto).ToList();
    }

    internal static InvoiceDto MapToDto(Invoice e) => new()
    {
        Id = e.Id,
        InvoiceNumber = e.InvoiceNumber,
        CustomerId = e.CustomerId,
        LeaseId = e.LeaseId,
        PropertyId = e.PropertyId,
        IssueDate = e.IssueDate,
        DueDate = e.DueDate,
        PeriodStart = e.PeriodStart,
        PeriodEnd = e.PeriodEnd,
        Subtotal = e.Subtotal,
        Tax = e.Tax,
        Total = e.Total,
        AmountPaid = e.AmountPaid,
        Currency = e.Currency,
        Status = e.Status,
        Notes = e.Notes,
        PdfUrl = e.PdfUrl,
        SentAt = e.SentAt,
        VoidedAt = e.VoidedAt,
        CreatedBy = e.CreatedBy,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}

public class GetInvoiceByIdQuery : IRequest<InvoiceDto?>
{
    public int Id { get; set; }
}

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly ApplicationDbContext _ctx;
    public GetInvoiceByIdQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery req, CancellationToken ct)
    {
        var e = await _ctx.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        return e is null ? null : GetInvoicesQueryHandler.MapToDto(e);
    }
}

// =============================================================================
// Outstanding (tenant scope) — invoices not paid and not void, ordered by due date
// =============================================================================
public class GetOutstandingInvoicesQuery : IRequest<List<InvoiceDto>>
{
    public int CustomerId { get; set; }
}

public class GetOutstandingInvoicesQueryHandler
    : IRequestHandler<GetOutstandingInvoicesQuery, List<InvoiceDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetOutstandingInvoicesQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<InvoiceDto>> Handle(GetOutstandingInvoicesQuery req, CancellationToken ct)
    {
        if (req.CustomerId <= 0) return new List<InvoiceDto>();

        var rows = await _ctx.Invoices.AsNoTracking()
            .Where(i => i.CustomerId == req.CustomerId
                        && i.Status != "paid"
                        && i.Status != "void"
                        && i.AmountPaid < i.Total)
            .OrderBy(i => i.DueDate)
            .ToListAsync(ct);

        return rows.Select(GetInvoicesQueryHandler.MapToDto).ToList();
    }
}

// =============================================================================
// PDF — minimal text PDF render (Phase 1). Replaces the stub so the UI button
// can download something real. A nicer template is a Phase 2 task.
// =============================================================================
public class GetInvoicePdfQuery : IRequest<InvoicePdfResult?>
{
    public int Id { get; set; }
}

public class InvoicePdfResult
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "invoice.pdf";
}

public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, InvoicePdfResult?>
{
    private readonly ApplicationDbContext _ctx;
    public GetInvoicePdfQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoicePdfResult?> Handle(GetInvoicePdfQuery req, CancellationToken ct)
    {
        var e = await _ctx.Invoices.AsNoTracking()
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        var pdf = BuildSimplePdf(e);
        var fileName = $"{(string.IsNullOrWhiteSpace(e.InvoiceNumber) ? $"invoice-{e.Id}" : e.InvoiceNumber)}.pdf";
        return new InvoicePdfResult { Bytes = pdf, FileName = fileName };
    }

    // Tiny hand-rolled PDF (PDF 1.4) so we don't pull in a templating library.
    // It's a single-page text-only invoice summary — good enough to confirm the
    // endpoint is wired and the download flow works. Replace with QuestPDF /
    // similar when invoice templating is a real feature.
    private static byte[] BuildSimplePdf(Invoice e)
    {
        var lines = new List<string>
        {
            $"Invoice {e.InvoiceNumber}",
            "",
            $"Customer ID:  {e.CustomerId}",
            $"Lease ID:     {e.LeaseId?.ToString() ?? "-"}",
            $"Issue date:   {e.IssueDate:yyyy-MM-dd}",
            $"Due date:     {e.DueDate:yyyy-MM-dd}",
            $"Period:       {e.PeriodStart?.ToString("yyyy-MM-dd") ?? "-"} to {e.PeriodEnd?.ToString("yyyy-MM-dd") ?? "-"}",
            $"Status:       {e.Status}",
            "",
            $"Subtotal:     {e.Subtotal:N2} {e.Currency}",
            $"Tax:          {e.Tax:N2} {e.Currency}",
            $"Total:        {e.Total:N2} {e.Currency}",
            $"Amount paid:  {e.AmountPaid:N2} {e.Currency}",
            $"Balance due:  {(e.Total - e.AmountPaid):N2} {e.Currency}",
        };

        var content = new System.Text.StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 12 Tf");
        content.AppendLine("50 780 Td");
        content.AppendLine("14 TL");
        foreach (var line in lines)
        {
            // Escape parentheses and backslashes for PDF literal strings.
            var safe = line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            content.AppendLine($"({safe}) Tj");
            content.AppendLine("T*");
        }
        content.AppendLine("ET");
        var stream = content.ToString();
        var streamBytes = System.Text.Encoding.ASCII.GetBytes(stream);

        var pdfBuilder = new System.Text.StringBuilder();
        var offsets = new List<int>();

        void Write(string s)
        {
            offsets.Add(pdfBuilder.Length);
            pdfBuilder.Append(s);
        }

        pdfBuilder.Append("%PDF-1.4\n");
        Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
              "/Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");
        Write($"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n{stream}endstream\nendobj\n");
        Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        var xrefPos = pdfBuilder.Length;
        pdfBuilder.Append("xref\n0 6\n");
        pdfBuilder.Append("0000000000 65535 f \n");
        foreach (var o in offsets)
            pdfBuilder.Append($"{o:D10} 00000 n \n");

        pdfBuilder.Append("trailer\n<< /Size 6 /Root 1 0 R >>\n");
        pdfBuilder.Append($"startxref\n{xrefPos}\n%%EOF");

        return System.Text.Encoding.ASCII.GetBytes(pdfBuilder.ToString());
    }
}
