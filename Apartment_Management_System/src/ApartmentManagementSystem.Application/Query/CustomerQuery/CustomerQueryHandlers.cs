using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;
using ApartmentManagementSystem.Application.Query.Repositories;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    // Helper to keep the DTO projection in one place — every list/single
    // endpoint needs the same shape, so duplicating the projection per
    // handler (as the old code did) just creates drift.
    internal static class CustomerProjection
    {
        public static CustomerProfileDto ToDto(
            ApartmentManagementSystem.Domain.Entities.Customers.Customer customer,
            int totalNotes,
            ApartmentManagementSystem.Domain.Entities.Customers.Lease? currentLeaseOverride = null)
        {
            var currentLease = currentLeaseOverride
                ?? customer.Leases?.FirstOrDefault(l => l.Status == "active");

            return new CustomerProfileDto
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                CustomerType = customer.CustomerType,
                DateOfBirth = customer.DateOfBirth,
                GovernmentId = customer.GovernmentId,
                ProfilePhotoUrl = customer.ProfilePhotoUrl,
                EmergencyContact = (string.IsNullOrEmpty(customer.EmergencyContactName)
                                    && string.IsNullOrEmpty(customer.EmergencyContactPhone))
                    ? null
                    : new EmergencyContactDto
                    {
                        Name = customer.EmergencyContactName,
                        Phone = customer.EmergencyContactPhone
                    },
                Addresses = customer.Addresses?.Select(a => new AddressDto
                {
                    Id = a.Id,
                    AddressType = a.AddressType,
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    IsPrimary = a.IsPrimary
                }).ToList() ?? new List<AddressDto>(),
                CurrentLease = currentLease != null ? new LeaseDto
                {
                    Id = currentLease.Id,
                    StartDate = currentLease.StartDate,
                    EndDate = currentLease.EndDate,
                    MonthlyRent = currentLease.MonthlyRent,
                    SecurityDeposit = currentLease.SecurityDeposit,
                    Status = currentLease.Status,
                    SignedDocumentUrl = currentLease.SignedDocumentUrl,
                    SignedDate = currentLease.SignedDate,
                    ProductId = currentLease.ProductId
                } : null,
                TotalNotes = totalNotes,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }

    // ==================== Get Customer Profile ====================
    public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerProfileQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerProfileDto> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(request.CustomerId);
            if (customer == null)
                return null;

            var totalNotes = await _customerRepository.GetTotalNotesCountAsync(request.CustomerId);
            return CustomerProjection.ToDto(customer, totalNotes);
        }
    }

    // ==================== Get All Customers ====================
    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerProfileDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerProfileDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllCustomersAsync(request.PageNumber, request.PageSize);
            var result = new List<CustomerProfileDto>();

            foreach (var customer in customers)
            {
                var totalNotes = await _customerRepository.GetTotalNotesCountAsync(customer.Id);
                result.Add(CustomerProjection.ToDto(customer, totalNotes));
            }

            return result;
        }
    }

    // ==================== Search Customers ====================
    public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, List<CustomerProfileDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerProfileDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.SearchCustomersAsync(request.SearchTerm);
            var result = new List<CustomerProfileDto>();

            foreach (var customer in customers)
            {
                var totalNotes = await _customerRepository.GetTotalNotesCountAsync(customer.Id);
                result.Add(CustomerProjection.ToDto(customer, totalNotes));
            }

            return result;
        }
    }

    // ==================== Get Customer Addresses ====================
    public class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, List<AddressDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerAddressesQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<AddressDto>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _customerRepository.GetCustomerAddressesAsync(request.CustomerId);

            return addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                AddressType = a.AddressType,
                Street = a.Street,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsPrimary = a.IsPrimary
            }).ToList();
        }
    }

    // ==================== Get Customer Leases ====================
    public class GetCustomerLeasesQueryHandler : IRequestHandler<GetCustomerLeasesQuery, List<LeaseDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerLeasesQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<LeaseDto>> Handle(GetCustomerLeasesQuery request, CancellationToken cancellationToken)
        {
            var leases = await _customerRepository.GetCustomerLeasesAsync(request.CustomerId);

            return leases.Select(l => new LeaseDto
            {
                Id = l.Id,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                MonthlyRent = l.MonthlyRent,
                SecurityDeposit = l.SecurityDeposit,
                Status = l.Status,
                SignedDocumentUrl = l.SignedDocumentUrl,
                SignedDate = l.SignedDate,
                ProductId = l.ProductId
            }).ToList();
        }
    }

    // ==================== Get All Leases (with filters) ====================
    public class GetLeasesQueryHandler : IRequestHandler<GetLeasesQuery, List<LeaseDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetLeasesQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<LeaseDto>> Handle(GetLeasesQuery request, CancellationToken cancellationToken)
        {
            var leases = await _customerRepository.GetFilteredLeasesAsync(
                status: request.Status,
                propertyId: request.PropertyId,
                searchQuery: request.SearchQuery,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            return leases.Select(l => new LeaseDto
            {
                Id = l.Id,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                MonthlyRent = l.MonthlyRent,
                SecurityDeposit = l.SecurityDeposit,
                Status = l.Status,
                SignedDocumentUrl = l.SignedDocumentUrl,
                SignedDate = l.SignedDate,
                ProductId = l.ProductId
            }).ToList();
        }
    }

    // ==================== Get Lease By Id ====================
    public class GetLeaseByIdQueryHandler : IRequestHandler<GetLeaseByIdQuery, LeaseDto?>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetLeaseByIdQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<LeaseDto?> Handle(GetLeaseByIdQuery request, CancellationToken cancellationToken)
        {
            var lease = await _customerRepository.GetLeaseByIdAsync(request.LeaseId);
            if (lease == null)
                return null;

            return new LeaseDto
            {
                Id = lease.Id,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                MonthlyRent = lease.MonthlyRent,
                SecurityDeposit = lease.SecurityDeposit,
                Status = lease.Status,
                SignedDocumentUrl = lease.SignedDocumentUrl,
                SignedDate = lease.SignedDate,
                ProductId = lease.ProductId
            };
        }
    }

    // ==================== Get Lease Documents ====================
    public class GetLeaseDocumentsQueryHandler : IRequestHandler<GetLeaseDocumentsQuery, List<LeaseDocumentDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetLeaseDocumentsQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<LeaseDocumentDto>> Handle(GetLeaseDocumentsQuery request, CancellationToken cancellationToken)
        {
            var documents = await _customerRepository.GetLeaseDocumentsAsync(request.LeaseId);

            return documents.Select(d => new LeaseDocumentDto
            {
                Id = d.Id,
                LeaseId = d.LeaseId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                Status = d.Status,
                Description = d.Description,
                SignedDate = d.SignedDate,
                SignedBy = d.SignedBy,
                CreatedAt = d.CreatedAt
            }).ToList();
        }
    }

    // ==================== Get Communications ====================
    public class GetCommunicationsQueryHandler : IRequestHandler<GetCommunicationsQuery, List<CommunicationLogDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCommunicationsQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CommunicationLogDto>> Handle(GetCommunicationsQuery request, CancellationToken cancellationToken)
        {
            var communications = await _customerRepository.GetCustomerCommunicationsAsync(request.CustomerId, request.PageNumber, request.PageSize);

            if (!string.IsNullOrEmpty(request.Type))
                communications = communications.Where(c => c.Type == request.Type).ToList();

            if (!string.IsNullOrEmpty(request.Direction))
                communications = communications.Where(c => c.Direction == request.Direction).ToList();

            return communications.Select(c => new CommunicationLogDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                Type = c.Type,
                Subject = c.Subject,
                Message = c.Message,
                Direction = c.Direction,
                CreatedAt = c.CreatedAt,
                Category = DetermineCommunicationCategory(c.Type, c.Direction),
                Metadata = new CommunicationMetadataDto
                {
                    HasResponse = c.Direction == "inbound",
                    ResponseTimeHours = null
                }
            }).ToList();
        }

        private string DetermineCommunicationCategory(string type, string direction)
        {
            if (type == "ticket")
                return "maintenance_request";
            if (type == "email" && direction == "outbound")
                return "notice_sent";
            return "general";
        }
    }

    // ==================== Get Customer Notes ====================
    public class GetCustomerNotesQueryHandler : IRequestHandler<GetCustomerNotesQuery, List<CustomerNoteDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerNotesQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerNoteDto>> Handle(GetCustomerNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = await _customerRepository.GetCustomerNotesAsync(request.CustomerId);

            return notes.Select(n => new CustomerNoteDto
            {
                Id = n.Id,
                CustomerId = n.CustomerId,
                Content = n.Content,
                CreatedBy = n.CreatedBy,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            }).ToList();
        }
    }

    // ==================== Get Owner Portfolio ====================
    public class GetOwnerPortfolioQueryHandler : IRequestHandler<GetOwnerPortfolioQuery, OwnerPortfolioDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetOwnerPortfolioQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<OwnerPortfolioDto> Handle(GetOwnerPortfolioQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(request.OwnerId);
            if (customer == null || (customer.CustomerType != "owner" && customer.CustomerType != "both"))
                return null;

            var ownedProperties = new List<OwnedPropertyDto>();

            foreach (var lease in (customer.Leases ?? Enumerable.Empty<ApartmentManagementSystem.Domain.Entities.Customers.Lease>())
                                  .Where(l => l.Status == "active"))
            {
                // Lease.Customer is the *renter* on this lease (not the owner queried
                // here). It is not eagerly loaded by GetCustomerByIdAsync, so guard
                // for null instead of NRE-ing.
                var tenant = lease.Customer;
                ownedProperties.Add(new OwnedPropertyDto
                {
                    PropertyId = lease.ProductId,
                    PropertyName = $"Property {lease.ProductId}",
                    Address = "Address TBD",
                    CurrentRent = lease.MonthlyRent,
                    LeaseStatus = lease.Status,
                    CurrentTenant = tenant == null ? null : new TenantDto
                    {
                        TenantId = tenant.Id,
                        TenantName = $"{tenant.FirstName} {tenant.LastName}",
                        TenantPhone = tenant.Phone
                    }
                });
            }

            return new OwnerPortfolioDto
            {
                OwnerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                CustomerType = customer.CustomerType,
                OwnedProperties = ownedProperties,
                PaymentSummary = new PaymentSummaryDto
                {
                    TotalReceived = 0,
                    LastPaymentDate = null,
                    UpcomingPayments = 0
                }
            };
        }
    }

    // ==================== Get Property Tenants ====================
    public class GetPropertyTenantsQueryHandler : IRequestHandler<GetPropertyTenantsQuery, List<CustomerProfileDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetPropertyTenantsQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerProfileDto>> Handle(GetPropertyTenantsQuery request, CancellationToken cancellationToken)
        {
            var tenants = await _customerRepository.GetTenantsByPropertyAsync(request.PropertyId);
            var result = new List<CustomerProfileDto>();

            foreach (var tenant in tenants)
            {
                var currentLease = tenant.Leases?.FirstOrDefault(
                    l => l.ProductId == request.PropertyId && l.Status == "active");
                var totalNotes = await _customerRepository.GetTotalNotesCountAsync(tenant.Id);

                result.Add(CustomerProjection.ToDto(tenant, totalNotes, currentLease));
            }

            return result;
        }
    }

    // ==================== Get Move Checklist ====================
    public class GetMoveChecklistQueryHandler : IRequestHandler<GetMoveChecklistQuery, MoveChecklistDto>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetMoveChecklistQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<MoveChecklistDto> Handle(GetMoveChecklistQuery request, CancellationToken cancellationToken)
        {
            var checklist = await _customerRepository.GetChecklistByIdAsync(request.ChecklistId);
            if (checklist == null)
                return null;

            return new MoveChecklistDto
            {
                Id = checklist.Id,
                LeaseId = checklist.LeaseId,
                ChecklistType = checklist.ChecklistType,
                InspectionDate = checklist.InspectionDate,
                InspectorName = checklist.InspectorName,
                OverallCondition = checklist.OverallCondition,
                Notes = checklist.Notes,
                ChecklistItems = checklist.ChecklistItems?.Select(ci => new ChecklistItemDto
                {
                    Id = ci.Id,
                    ItemName = ci.ItemName,
                    Condition = ci.Condition,
                    Notes = ci.Notes,
                    IsDamage = ci.IsDamage,
                    PhotoUrls = ci.PhotoUrls
                }).ToList() ?? new List<ChecklistItemDto>()
            };
        }
    }
}
