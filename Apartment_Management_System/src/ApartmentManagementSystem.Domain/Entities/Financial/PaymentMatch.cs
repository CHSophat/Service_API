namespace ApartmentManagementSystem.Domain.Entities.Financial;

public class PaymentMatch
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public int? MatchedBy { get; set; }
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

    public virtual Payment Payment { get; set; } = null!;
    public virtual Invoice Invoice { get; set; } = null!;
}
