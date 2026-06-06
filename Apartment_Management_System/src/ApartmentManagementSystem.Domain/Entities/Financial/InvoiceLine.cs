namespace ApartmentManagementSystem.Domain.Entities.Financial;

public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string LineType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
