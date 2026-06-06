using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetLeaseByIdQuery : IRequest<LeaseDto?>
    {
        public int LeaseId { get; set; }
    }
}
