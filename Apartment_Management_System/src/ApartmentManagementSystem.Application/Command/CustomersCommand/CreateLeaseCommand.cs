using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;
using ApartmentManagementSystem.Domain.Entities.Customers;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand;

public class CreateLeaseCommand : IRequest<LeaseDto>
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
}

public class CreateLeaseCommandHandler : IRequestHandler<CreateLeaseCommand, LeaseDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreateLeaseCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<LeaseDto> Handle(CreateLeaseCommand req, CancellationToken ct)
    {
        if (req.CustomerId <= 0)
            throw new ArgumentException("customerId is required", nameof(req.CustomerId));
        if (req.ProductId <= 0)
            throw new ArgumentException("productId is required", nameof(req.ProductId));
        if (req.StartDate >= req.EndDate)
            throw new ArgumentException("startDate must be before endDate", nameof(req.StartDate));
        if (req.MonthlyRent <= 0)
            throw new ArgumentException("monthlyRent must be greater than 0", nameof(req.MonthlyRent));

        var entity = new Lease
        {
            CustomerId = req.CustomerId,
            ProductId = req.ProductId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            MonthlyRent = req.MonthlyRent,
            SecurityDeposit = req.SecurityDeposit,
            Status = "active",
            SignedDocumentUrl = string.Empty,
        };
        _ctx.Leases.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    private static LeaseDto ToDto(Lease e) => new()
    {
        Id = e.Id,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        MonthlyRent = e.MonthlyRent,
        SecurityDeposit = e.SecurityDeposit,
        Status = e.Status,
        SignedDocumentUrl = e.SignedDocumentUrl,
        SignedDate = e.SignedDate,
        ProductId = e.ProductId,
    };
}
