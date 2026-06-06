using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Application.Query.Repositories
{
    public interface ICustomerRepository
    {
        // Customer queries
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<List<Customer>> GetAllCustomersAsync(int pageNumber, int pageSize);
        Task<List<Customer>> GetTenantsByPropertyAsync(int propertyId);
        Task<List<Customer>> GetOwnersByPropertyAsync(int propertyId);
        Task<int> GetTotalCustomersCountAsync();
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);

        // Address queries
        Task<Address> GetAddressByIdAsync(int addressId);
        Task<List<Address>> GetCustomerAddressesAsync(int customerId);
        Task<Address> GetPrimaryAddressAsync(int customerId);

        // Lease queries
        Task<Lease> GetLeaseByIdAsync(int leaseId);
        Task<List<Lease>> GetCustomerLeasesAsync(int customerId);
        Task<Lease> GetActiveLeaseAsync(int customerId);
        Task<List<Lease>> GetExpiredLeasesAsync(int customerId);
        Task<List<Lease>> GetLeasesByPropertyAsync(int propertyId);
        Task<List<Lease>> GetFilteredLeasesAsync(string? status = null, int? propertyId = null, string? searchQuery = null, int pageNumber = 1, int pageSize = 20);

        // Lease Document queries
        Task<LeaseDocument> GetLeaseDocumentByIdAsync(int documentId);
        Task<List<LeaseDocument>> GetLeaseDocumentsAsync(int leaseId);
        Task<List<LeaseDocument>> GetDocumentsByTypeAsync(int leaseId, string documentType);

        // Communication Log queries
        Task<CommunicationLog> GetCommunicationByIdAsync(int communicationId);
        Task<List<CommunicationLog>> GetCustomerCommunicationsAsync(int customerId, int pageNumber, int pageSize);
        Task<List<CommunicationLog>> GetCustomerCommunicationsByTypeAsync(int customerId, string type);
        Task<int> GetUnreadCommunicationsCountAsync(int customerId);

        // Customer Note queries
        Task<CustomerNote> GetNoteByIdAsync(int noteId);
        Task<List<CustomerNote>> GetCustomerNotesAsync(int customerId);
        Task<int> GetTotalNotesCountAsync(int customerId);

        // Move Checklist queries
        Task<MoveChecklist> GetChecklistByIdAsync(int checklistId);
        Task<List<MoveChecklist>> GetLeaseChecklistsAsync(int leaseId);
        Task<MoveChecklist> GetChecklistByTypeAsync(int leaseId, string checklistType);

        // ChecklistItem queries
        Task<ChecklistItem> GetChecklistItemByIdAsync(int itemId);
        Task<List<ChecklistItem>> GetChecklistItemsAsync(int checklistId);
    }
}
