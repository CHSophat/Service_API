using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Domain.Entities.Financial;

public class PaymentMethod
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Kind { get; set; } = string.Empty;  // bakong, card, bank, cash, wallet
    public string? Label { get; set; }
    public string? Masked { get; set; }
    public bool IsDefault { get; set; }
    public string? Metadata { get; set; }   // jsonb
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Customer Customer { get; set; } = null!;
}
