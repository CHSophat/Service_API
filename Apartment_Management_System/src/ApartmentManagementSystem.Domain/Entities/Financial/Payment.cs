using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Financial;

public class Payment : AuditableEntity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? PropertyId { get; set; }
    public int? PaymentMethodId { get; set; }
    public string MethodKind { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "pending";
    public DateTime? PaidAt { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? BakongQrString { get; set; }
    public string? BakongMd5 { get; set; }
    public string? ExternalRef { get; set; }
    public string? Notes { get; set; }

    public virtual ICollection<PaymentMatch> PaymentMatches { get; set; } = new List<PaymentMatch>();
}
