using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.FinancialDto;
using ApartmentManagementSystem.Domain.Entities.Financial;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.InvoiceCommand;

public class CreateInvoiceCommand : IRequest<InvoiceDto>
{
    public string? InvoiceNumber { get; set; }
    public int CustomerId { get; set; }
    public int? LeaseId { get; set; }
    public int? PropertyId { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<CreateInvoiceLineDto>? Lines { get; set; }
}

public class CreateInvoiceLineDto
{
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreateInvoiceCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand req, CancellationToken ct)
    {
        if (req.CustomerId <= 0)
            throw new ArgumentException("customerId is required", nameof(req.CustomerId));

        var userProvidedNumber = !string.IsNullOrWhiteSpace(req.InvoiceNumber);
        var invoiceNumber = userProvidedNumber
            ? req.InvoiceNumber!.Trim()
            : await GenerateUniqueInvoiceNumberAsync(ct);

        if (userProvidedNumber)
        {
            var clash = await _ctx.Invoices.AnyAsync(i => i.InvoiceNumber == invoiceNumber, ct);
            if (clash)
                throw new InvalidOperationException($"Invoice number '{invoiceNumber}' already exists.");
        }

        var entity = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = req.CustomerId,
            LeaseId = req.LeaseId,
            PropertyId = req.PropertyId,
            IssueDate = req.IssueDate,
            DueDate = req.DueDate,
            PeriodStart = req.PeriodStart,
            PeriodEnd = req.PeriodEnd,
            Subtotal = req.Subtotal,
            Tax = req.Tax,
            Total = req.Total,
            AmountPaid = 0,
            Currency = req.Currency,
            Status = "draft",
            Notes = req.Notes,
            CreatedBy = req.CreatedBy,
        };

        // Add invoice lines if provided
        if (req.Lines?.Count > 0)
        {
            foreach (var line in req.Lines)
            {
                entity.InvoiceLines.Add(new InvoiceLine
                {
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = line.Quantity * line.UnitPrice,
                });
            }
        }

        _ctx.Invoices.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    private async Task<string> GenerateUniqueInvoiceNumberAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            var exists = await _ctx.Invoices.AnyAsync(i => i.InvoiceNumber == candidate, ct);
            if (!exists) return candidate;
        }
        throw new InvalidOperationException("Failed to generate a unique invoice number after 5 attempts.");
    }

    internal static InvoiceDto ToDto(Invoice e) => new()
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

// =============================================================================
// Update
// =============================================================================
public class UpdateInvoiceCommand : IRequest<InvoiceDto?>
{
    public int Id { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Tax { get; set; }
    public decimal? Total { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, InvoiceDto?>
{
    private readonly ApplicationDbContext _ctx;
    public UpdateInvoiceCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoiceDto?> Handle(UpdateInvoiceCommand req, CancellationToken ct)
    {
        var e = await _ctx.Invoices.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        // Block edits on terminal-state invoices to keep the audit trail honest.
        if (e.Status is "paid" or "void")
            throw new InvalidOperationException($"Cannot edit an invoice in '{e.Status}' status.");

        if (req.IssueDate.HasValue) e.IssueDate = req.IssueDate.Value;
        if (req.DueDate.HasValue) e.DueDate = req.DueDate.Value;
        if (req.PeriodStart.HasValue) e.PeriodStart = req.PeriodStart;
        if (req.PeriodEnd.HasValue) e.PeriodEnd = req.PeriodEnd;
        if (req.Subtotal.HasValue) e.Subtotal = req.Subtotal.Value;
        if (req.Tax.HasValue) e.Tax = req.Tax.Value;
        if (req.Total.HasValue) e.Total = req.Total.Value;
        if (!string.IsNullOrWhiteSpace(req.Currency)) e.Currency = req.Currency.Trim();
        if (req.Notes is not null) e.Notes = req.Notes;

        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateInvoiceCommandHandler.ToDto(e);
    }
}

// =============================================================================
// Delete (only allowed for drafts; sent/paid become "void" instead via VoidInvoice)
// =============================================================================
public class DeleteInvoiceCommand : IRequest<bool>
{
    public int Id { get; set; }
}

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly ApplicationDbContext _ctx;
    public DeleteInvoiceCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<bool> Handle(DeleteInvoiceCommand req, CancellationToken ct)
    {
        var e = await _ctx.Invoices
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return false;

        if (e.Status != "draft")
            throw new InvalidOperationException(
                $"Only draft invoices can be deleted. Use Void on '{e.Status}' invoices.");

        _ctx.Invoices.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}

// =============================================================================
// Send (transition draft -> sent, stamp SentAt; email delivery is a future hook)
// =============================================================================
public class SendInvoiceCommand : IRequest<InvoiceDto?>
{
    public int Id { get; set; }
}

public class SendInvoiceCommandHandler : IRequestHandler<SendInvoiceCommand, InvoiceDto?>
{
    private readonly ApplicationDbContext _ctx;
    public SendInvoiceCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoiceDto?> Handle(SendInvoiceCommand req, CancellationToken ct)
    {
        var e = await _ctx.Invoices.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        if (e.Status == "void")
            throw new InvalidOperationException("Cannot send a void invoice.");

        e.Status = "sent";
        e.SentAt = DateTime.UtcNow;
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateInvoiceCommandHandler.ToDto(e);
    }
}

// =============================================================================
// Void
// =============================================================================
public class VoidInvoiceCommand : IRequest<InvoiceDto?>
{
    public int Id { get; set; }
}

public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, InvoiceDto?>
{
    private readonly ApplicationDbContext _ctx;
    public VoidInvoiceCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<InvoiceDto?> Handle(VoidInvoiceCommand req, CancellationToken ct)
    {
        var e = await _ctx.Invoices.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        if (e.Status == "paid")
            throw new InvalidOperationException("Cannot void a paid invoice. Issue a credit note instead.");

        e.Status = "void";
        e.VoidedAt = DateTime.UtcNow;
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateInvoiceCommandHandler.ToDto(e);
    }
}
