using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApartmentManagementSystem.Domain.Entities.Customers;
using ApartmentManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ApartmentManagementSystem.Application.Query.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Customer queries
        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Leases)
                .Include(c => c.CommunicationLogs)
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<List<Customer>> GetAllCustomersAsync(int pageNumber, int pageSize)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Leases)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetTenantsByPropertyAsync(int propertyId)
        {
            return await _context.Customers
                .Where(c => c.CustomerType == "tenant" || c.CustomerType == "both")
                .Include(c => c.Leases.Where(l => l.ProductId == propertyId && l.Status == "active"))
                .ToListAsync();
        }

        public async Task<List<Customer>> GetOwnersByPropertyAsync(int propertyId)
        {
            return await _context.Customers
                .Where(c => c.CustomerType == "owner" || c.CustomerType == "both")
                .Include(c => c.Leases.Where(l => l.ProductId == propertyId))
                .ToListAsync();
        }

        public async Task<int> GetTotalCustomersCountAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _context.Customers
                .Where(c => c.FirstName.Contains(searchTerm) || 
                           c.LastName.Contains(searchTerm) || 
                           c.Email.Contains(searchTerm))
                .Include(c => c.Addresses)
                .ToListAsync();
        }

        // Address queries
        public async Task<Address> GetAddressByIdAsync(int addressId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
        }

        public async Task<List<Address>> GetCustomerAddressesAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Address> GetPrimaryAddressAsync(int customerId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsPrimary);
        }

        // Lease queries
        public async Task<Lease> GetLeaseByIdAsync(int leaseId)
        {
            return await _context.Leases
                .Include(l => l.Documents)
                .Include(l => l.Checklists)
                .FirstOrDefaultAsync(l => l.Id == leaseId);
        }

        public async Task<List<Lease>> GetCustomerLeasesAsync(int customerId)
        {
            return await _context.Leases
                .Where(l => l.CustomerId == customerId)
                .Include(l => l.Documents)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<Lease> GetActiveLeaseAsync(int customerId)
        {
            return await _context.Leases
                .Where(l => l.CustomerId == customerId && l.Status == "active")
                .OrderByDescending(l => l.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Lease>> GetExpiredLeasesAsync(int customerId)
        {
            return await _context.Leases
                .Where(l => l.CustomerId == customerId && (l.Status == "expired" || l.EndDate < DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<List<Lease>> GetLeasesByPropertyAsync(int propertyId)
        {
            return await _context.Leases
                .Where(l => l.ProductId == propertyId)
                .Include(l => l.Customer)
                .ToListAsync();
        }

        public async Task<List<Lease>> GetFilteredLeasesAsync(string? status = null, int? propertyId = null, string? searchQuery = null, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.Leases
                .Include(l => l.Customer)
                .Include(l => l.Documents)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            // Filter by property ID if provided
            if (propertyId.HasValue)
            {
                query = query.Where(l => l.ProductId == propertyId.Value);
            }

            // Filter by search query (searches tenant name, email, lease ID)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                query = query.Where(l =>
                    l.Id.ToString().Contains(lowerSearch) ||
                    (l.Customer != null && (
                        l.Customer.FirstName.ToLower().Contains(lowerSearch) ||
                        l.Customer.LastName.ToLower().Contains(lowerSearch) ||
                        l.Customer.Email.ToLower().Contains(lowerSearch)))
                );
            }

            // Apply pagination
            return await query
                .OrderByDescending(l => l.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Lease Document queries
        public async Task<LeaseDocument> GetLeaseDocumentByIdAsync(int documentId)
        {
            return await _context.LeaseDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<List<LeaseDocument>> GetLeaseDocumentsAsync(int leaseId)
        {
            return await _context.LeaseDocuments
                .Where(d => d.LeaseId == leaseId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaseDocument>> GetDocumentsByTypeAsync(int leaseId, string documentType)
        {
            return await _context.LeaseDocuments
                .Where(d => d.LeaseId == leaseId && d.DocumentType == documentType)
                .ToListAsync();
        }

        // Communication Log queries
        public async Task<CommunicationLog> GetCommunicationByIdAsync(int communicationId)
        {
            return await _context.CommunicationLogs.FirstOrDefaultAsync(c => c.Id == communicationId);
        }

        public async Task<List<CommunicationLog>> GetCustomerCommunicationsAsync(int customerId, int pageNumber, int pageSize)
        {
            return await _context.CommunicationLogs
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<CommunicationLog>> GetCustomerCommunicationsByTypeAsync(int customerId, string type)
        {
            return await _context.CommunicationLogs
                .Where(c => c.CustomerId == customerId && c.Type == type)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCommunicationsCountAsync(int customerId)
        {
            return await _context.CommunicationLogs
                .Where(c => c.CustomerId == customerId && c.Direction == "inbound")
                .CountAsync();
        }

        // Customer Note queries
        public async Task<CustomerNote> GetNoteByIdAsync(int noteId)
        {
            return await _context.CustomerNotes.FirstOrDefaultAsync(n => n.Id == noteId);
        }

        public async Task<List<CustomerNote>> GetCustomerNotesAsync(int customerId)
        {
            return await _context.CustomerNotes
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalNotesCountAsync(int customerId)
        {
            return await _context.CustomerNotes
                .Where(n => n.CustomerId == customerId)
                .CountAsync();
        }

        // Move Checklist queries
        public async Task<MoveChecklist> GetChecklistByIdAsync(int checklistId)
        {
            return await _context.MoveChecklists
                .Include(m => m.ChecklistItems)
                .FirstOrDefaultAsync(m => m.Id == checklistId);
        }

        public async Task<List<MoveChecklist>> GetLeaseChecklistsAsync(int leaseId)
        {
            return await _context.MoveChecklists
                .Where(m => m.LeaseId == leaseId)
                .Include(m => m.ChecklistItems)
                .ToListAsync();
        }

        public async Task<MoveChecklist> GetChecklistByTypeAsync(int leaseId, string checklistType)
        {
            return await _context.MoveChecklists
                .Include(m => m.ChecklistItems)
                .FirstOrDefaultAsync(m => m.LeaseId == leaseId && m.ChecklistType == checklistType);
        }

        // ChecklistItem queries
        public async Task<ChecklistItem> GetChecklistItemByIdAsync(int itemId)
        {
            return await _context.ChecklistItems.FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public async Task<List<ChecklistItem>> GetChecklistItemsAsync(int checklistId)
        {
            return await _context.ChecklistItems
                .Where(i => i.ChecklistId == checklistId)
                .ToListAsync();
        }
    }
}
