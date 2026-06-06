using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Financial;

public class Invoice : AuditableEntity
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
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
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "draft";
    public string? Notes { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public int? CreatedBy { get; set; }

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public virtual ICollection<PaymentMatch> PaymentMatches { get; set; } = new List<PaymentMatch>();
}
