using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Application.Query.Repositories
{
    public interface IProductRepository
    {
        // Product queries
        Task<Product> GetProductByIdAsync(int productId);
        Task<Product> GetProductByCodeAsync(string code);
        Task<List<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<List<Product>> GetProductsByTypeAsync(string productType, int pageNumber, int pageSize);
        Task<List<Product>> GetProductsByStatusAsync(string status, int pageNumber, int pageSize);
        Task<List<Product>> GetAvailableUnitsAsync(int pageNumber, int pageSize);
        Task<List<Product>> GetAmenitiesAsync(int pageNumber, int pageSize);
        Task<List<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize);
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetProductsCountByStatusAsync(string status);
        Task<Dictionary<string, int>> GetProductCountByStatusAsync();
        Task<List<Product>> GetProductsUnderMaintenanceAsync(int pageNumber, int pageSize);

        // Product photos queries
        Task<List<ProductPhoto>> GetProductPhotosAsync(int productId);
        Task<ProductPhoto> GetProductPhotoPrimaryAsync(int productId);
        Task<List<ProductPhoto>> GetProductPhotosByProductIdAsync(int productId);

        // Product attributes queries
        Task<List<ProductAttribute>> GetProductAttributesAsync(int productId);
        Task<Dictionary<string, string>> GetProductAttributesDictionaryAsync(int productId);

        // Maintenance requests queries
        Task<List<MaintenanceRequest>> GetProductMaintenanceRequestsAsync(int productId, string? status = null);
        Task<MaintenanceRequest> GetMaintenanceRequestByIdAsync(int maintenanceRequestId);
        Task<List<MaintenanceRequest>> GetOpenMaintenanceRequestsAsync();

        // Count methods
        Task<int> GetProductCountAsync();
        Task<int> GetProductCountByTypeAsync(string productType);
        Task<int> GetVacantUnitsCountAsync();
        Task<int> GetOccupiedUnitsCountAsync();
        Task<int> GetMaintenanceUnitsCountAsync();
    }
}
