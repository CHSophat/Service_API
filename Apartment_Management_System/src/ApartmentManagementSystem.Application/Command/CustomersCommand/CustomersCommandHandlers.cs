using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Domain.Entities.Customers;
using ApartmentManagementSystem.Infrastructure.Persistence;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    // ==================== Create Customer ====================
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerProfileDto>
    {
        private readonly ApplicationDbContext _context;

        public CreateCustomerCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        // DB CHECK constraint allows only these three values (see
        // customers_customer_type_check). Normalising here turns a 500
        // PostgresException into a 400 ArgumentException at the boundary.
        private static readonly HashSet<string> AllowedCustomerTypes =
            new(StringComparer.Ordinal) { "tenant", "owner", "both" };

        public async Task<CustomerProfileDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerType = (request.CustomerType ?? "tenant").Trim().ToLowerInvariant();
            if (!AllowedCustomerTypes.Contains(customerType))
                throw new ArgumentException(
                    $"customerType must be one of: tenant, owner, both (got '{request.CustomerType}').",
                    nameof(request.CustomerType));

            // Optional contact fields can now stay null — the Customer entity
            // marks them as `string?` so EF and the DB schema agree.
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                CustomerType = customerType,
                DateOfBirth = request.DateOfBirth,
                GovernmentId = request.GovernmentId,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

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
                Addresses = new List<AddressDto>(),
                CurrentLease = null,
                TotalNotes = 0,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }

    // ==================== Update Customer ====================
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerProfileDto>
    {
        private readonly ApplicationDbContext _context;

        public UpdateCustomerCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerProfileDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Leases)
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer == null)
                return null;

            customer.FirstName = request.FirstName ?? customer.FirstName;
            customer.LastName = request.LastName ?? customer.LastName;
            customer.Email = request.Email ?? customer.Email;
            customer.Phone = request.Phone ?? customer.Phone;
            customer.DateOfBirth = request.DateOfBirth ?? customer.DateOfBirth;
            customer.EmergencyContactName = request.EmergencyContactName ?? customer.EmergencyContactName;
            customer.EmergencyContactPhone = request.EmergencyContactPhone ?? customer.EmergencyContactPhone;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var currentLease = customer.Leases?.FirstOrDefault(l => l.Status == "active");

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
                TotalNotes = customer.Notes?.Count ?? 0,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }

    // ==================== Add Address ====================
    public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, int>
    {
        private readonly ApplicationDbContext _context;

        public AddAddressCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(AddAddressCommand request, CancellationToken cancellationToken)
        {
            if (request.IsPrimary)
            {
                var primaryAddresses = await _context.Addresses
                    .Where(a => a.CustomerId == request.CustomerId && a.IsPrimary)
                    .ToListAsync(cancellationToken);

                foreach (var addr in primaryAddresses)
                {
                    addr.IsPrimary = false;
                }
            }

            var address = new Address
            {
                CustomerId = request.CustomerId,
                AddressType = request.AddressType,
                Street = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsPrimary = request.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(cancellationToken);

            return address.Id;
        }
    }

    // ==================== Update Address ====================
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public UpdateAddressCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId, cancellationToken);
            if (address == null)
                return false;

            if (request.IsPrimary && !address.IsPrimary)
            {
                var primaryAddresses = await _context.Addresses
                    .Where(a => a.CustomerId == address.CustomerId && a.IsPrimary && a.Id != request.AddressId)
                    .ToListAsync(cancellationToken);

                foreach (var addr in primaryAddresses)
                {
                    addr.IsPrimary = false;
                }
            }

            address.AddressType = request.AddressType;
            address.Street = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.IsPrimary = request.IsPrimary;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==================== Delete Address ====================
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteAddressCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId, cancellationToken);
            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==================== Set Primary Address ====================
    public class SetPrimaryAddressCommandHandler : IRequestHandler<SetPrimaryAddressCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public SetPrimaryAddressCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(SetPrimaryAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId, cancellationToken);
            if (address == null)
                return false;

            var primaryAddresses = await _context.Addresses
                .Where(a => a.CustomerId == address.CustomerId && a.IsPrimary && a.Id != request.AddressId)
                .ToListAsync(cancellationToken);

            foreach (var addr in primaryAddresses)
            {
                addr.IsPrimary = false;
            }

            address.IsPrimary = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==================== Add Customer Note ====================
    public class AddCustomerNoteCommandHandler : IRequestHandler<AddCustomerNoteCommand, CustomerNoteDto>
    {
        private readonly ApplicationDbContext _context;

        public AddCustomerNoteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerNoteDto> Handle(AddCustomerNoteCommand request, CancellationToken cancellationToken)
        {
            var note = new CustomerNote
            {
                CustomerId = request.CustomerId,
                Content = request.Content,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CustomerNotes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerNoteDto
            {
                Id = note.Id,
                CustomerId = note.CustomerId,
                Content = note.Content,
                CreatedBy = note.CreatedBy,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }

    // ==================== Update Customer Note ====================
    public class UpdateCustomerNoteCommandHandler : IRequestHandler<UpdateCustomerNoteCommand, CustomerNoteDto>
    {
        private readonly ApplicationDbContext _context;

        public UpdateCustomerNoteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerNoteDto> Handle(UpdateCustomerNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.CustomerNotes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);
            if (note == null)
                return null;

            note.Content = request.Content;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerNoteDto
            {
                Id = note.Id,
                CustomerId = note.CustomerId,
                Content = note.Content,
                CreatedBy = note.CreatedBy,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }

    // ==================== Delete Customer Note ====================
    public class DeleteCustomerNoteCommandHandler : IRequestHandler<DeleteCustomerNoteCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteCustomerNoteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteCustomerNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.CustomerNotes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);
            if (note == null)
                return false;

            _context.CustomerNotes.Remove(note);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==================== Upload Lease Document ====================
    public class UploadLeaseDocumentCommandHandler : IRequestHandler<UploadLeaseDocumentCommand, LeaseDocumentDto>
    {
        private readonly ApplicationDbContext _context;

        public UploadLeaseDocumentCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaseDocumentDto> Handle(UploadLeaseDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = new LeaseDocument
            {
                LeaseId = request.LeaseId,
                DocumentType = request.DocumentType,
                FileName = request.FileName,
                FileUrl = request.FileUrl,
                Status = "pending_review",
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.LeaseDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return new LeaseDocumentDto
            {
                Id = document.Id,
                LeaseId = document.LeaseId,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                FileUrl = document.FileUrl,
                Status = document.Status,
                Description = document.Description,
                SignedDate = document.SignedDate,
                SignedBy = document.SignedBy,
                CreatedAt = document.CreatedAt
            };
        }
    }

    // ==================== Sign Lease ====================
    public class SignLeaseCommandHandler : IRequestHandler<SignLeaseCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public SignLeaseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(SignLeaseCommand request, CancellationToken cancellationToken)
        {
            var lease = await _context.Leases.FirstOrDefaultAsync(l => l.Id == request.LeaseId, cancellationToken);
            if (lease == null)
                return false;

            lease.SignedDate = DateTime.UtcNow;
            lease.Status = "active";
            lease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // ==================== Create Move Checklist ====================
    public class CreateMoveChecklistCommandHandler : IRequestHandler<CreateMoveChecklistCommand, MoveChecklistDto>
    {
        private readonly ApplicationDbContext _context;

        public CreateMoveChecklistCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MoveChecklistDto> Handle(CreateMoveChecklistCommand request, CancellationToken cancellationToken)
        {
            var checklist = new MoveChecklist
            {
                LeaseId = request.LeaseId,
                ChecklistType = request.ChecklistType,
                InspectionDate = request.InspectionDate,
                InspectorName = request.InspectorName,
                OverallCondition = request.OverallCondition,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MoveChecklists.Add(checklist);
            await _context.SaveChangesAsync(cancellationToken);

            return new MoveChecklistDto
            {
                Id = checklist.Id,
                LeaseId = checklist.LeaseId,
                ChecklistType = checklist.ChecklistType,
                InspectionDate = checklist.InspectionDate,
                InspectorName = checklist.InspectorName,
                OverallCondition = checklist.OverallCondition,
                Notes = checklist.Notes,
                ChecklistItems = new List<ChecklistItemDto>()
            };
        }
    }

    // ==================== Send Communication ====================
    public class SendCommunicationCommandHandler : IRequestHandler<SendCommunicationCommand, int>
    {
        private readonly ApplicationDbContext _context;

        // Must stay in sync with the DB check constraints on
        // customer.communications_log (type, direction).
        private static readonly HashSet<string> AllowedTypes =
            new(StringComparer.Ordinal) { "email", "sms", "ticket" };
        private static readonly HashSet<string> AllowedDirections =
            new(StringComparer.Ordinal) { "inbound", "outbound" };

        public SendCommunicationCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(SendCommunicationCommand request, CancellationToken cancellationToken)
        {
            var type = (request.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (!AllowedTypes.Contains(type))
                throw new ArgumentException(
                    $"type must be one of: email, sms, ticket (received: '{request.Type}')");

            var direction = (request.Direction ?? "outbound").Trim().ToLowerInvariant();
            if (!AllowedDirections.Contains(direction))
                throw new ArgumentException(
                    $"direction must be one of: inbound, outbound (received: '{request.Direction}')");

            var communication = new CommunicationLog
            {
                CustomerId = request.CustomerId,
                Type = type,
                Subject = request.Subject,
                Message = request.Message,
                Direction = direction,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CommunicationLogs.Add(communication);
            await _context.SaveChangesAsync(cancellationToken);

            return communication.Id;
        }
    }
}
