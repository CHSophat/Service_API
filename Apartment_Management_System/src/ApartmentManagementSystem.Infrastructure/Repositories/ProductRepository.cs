// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// // using ApartmentManagementSystem.Application.Query.Repositories;
// using ApartmentManagementSystem.Domain.Entities.Products;
// using ApartmentManagementSystem.Infrastructure.Persistence;

// namespace ApartmentManagementSystem.Infrastructure.Repositories
// {
//     // public class ProductRepository : IProductRepository
//     {
//         private readonly ApplicationDbContext _context;

//         public ProductRepository(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<Product> GetProductByIdAsync(int productId)
//         {
//             return await _context.Products
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Include(p => p.MaintenanceRequests)
//                 .FirstOrDefaultAsync(p => p.Id == productId);
//         }

//         public async Task<Product> GetProductByCodeAsync(string code)
//         {
//             return await _context.Products
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Include(p => p.MaintenanceRequests)
//                 .FirstOrDefaultAsync(p => p.Code == code);
//         }

//         public async Task<List<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<Product>> GetProductsByTypeAsync(string productType, int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.ProductType == productType)
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<Product>> GetProductsByStatusAsync(string status, int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.Status == status)
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<Product>> GetAvailableUnitsAsync(int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.ProductType == "unit" && p.Status == "vacant")
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<Product>> GetAmenitiesAsync(int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.ProductType == "amenity" && p.Status != "unavailable")
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm) || p.Description.Contains(searchTerm))
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<int> GetTotalProductsCountAsync()
//         {
//             return await _context.Products.CountAsync();
//         }

//         public async Task<int> GetProductsCountByStatusAsync(string status)
//         {
//             return await _context.Products.CountAsync(p => p.Status == status);
//         }

//         public async Task<Dictionary<string, int>> GetProductCountByStatusAsync()
//         {
//             var counts = await _context.Products
//                 .GroupBy(p => p.Status)
//                 .Select(g => new { Status = g.Key, Count = g.Count() })
//                 .ToDictionaryAsync(x => x.Status, x => x.Count);

//             return counts;
//         }

//         public async Task<List<Product>> GetProductsUnderMaintenanceAsync(int pageNumber, int pageSize)
//         {
//             return await _context.Products
//                 .Where(p => p.Status == "maintenance")
//                 .Include(p => p.ProductAttributes)
//                 .Include(p => p.ProductPhotos)
//                 .Include(p => p.MaintenanceRequests)
//                 .Skip((pageNumber - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//         }

//         public async Task<List<ProductPhoto>> GetProductPhotosAsync(int productId)
//         {
//             return await _context.ProductPhotos
//                 .Where(pp => pp.ProductId == productId)
//                 .OrderBy(pp => pp.SortOrder)
//                 .ToListAsync();
//         }

//         public async Task<ProductPhoto> GetProductPhotoPrimaryAsync(int productId)
//         {
//             return await _context.ProductPhotos
//                 .FirstOrDefaultAsync(pp => pp.ProductId == productId && pp.IsPrimary);
//         }

//         public async Task<List<ProductPhoto>> GetProductPhotosByProductIdAsync(int productId)
//         {
//             return await _context.ProductPhotos
//                 .Where(pp => pp.ProductId == productId)
//                 .OrderBy(pp => pp.SortOrder)
//                 .ToListAsync();
//         }

//         public async Task<List<ProductAttribute>> GetProductAttributesAsync(int productId)
//         {
//             return await _context.ProductAttributes
//                 .Where(pa => pa.ProductId == productId)
//                 .ToListAsync();
//         }

//         public async Task<Dictionary<string, string>> GetProductAttributesDictionaryAsync(int productId)
//         {
//             return await _context.ProductAttributes
//                 .Where(pa => pa.ProductId == productId)
//                 .ToDictionaryAsync(pa => pa.AttributeName, pa => pa.AttributeValue);
//         }

//         public async Task<List<MaintenanceRequest>> GetProductMaintenanceRequestsAsync(int productId, string status = null)
//         {
//             var query = _context.MaintenanceRequests
//                 .Where(mr => mr.ProductId == productId)
//                 .AsQueryable();

//             if (!string.IsNullOrEmpty(status))
//                 query = query.Where(mr => mr.Status == status);

//             return await query
//                 .OrderByDescending(mr => mr.CreatedAt)
//                 .ToListAsync();
//         }

//         public async Task<MaintenanceRequest> GetMaintenanceRequestByIdAsync(int maintenanceRequestId)
//         {
//             return await _context.MaintenanceRequests
//                 .FirstOrDefaultAsync(mr => mr.Id == maintenanceRequestId);
//         }

//         public async Task<List<MaintenanceRequest>> GetOpenMaintenanceRequestsAsync()
//         {
//             return await _context.MaintenanceRequests
//                 .Where(mr => mr.Status != "completed")
//                 .OrderByDescending(mr => mr.CreatedAt)
//                 .ToListAsync();
//         }

//         public async Task<int> GetProductCountAsync()
//         {
//             return await _context.Products.CountAsync();
//         }

//         public async Task<int> GetProductCountByTypeAsync(string productType)
//         {
//             return await _context.Products.CountAsync(p => p.ProductType == productType);
//         }

//         public async Task<int> GetVacantUnitsCountAsync()
//         {
//             return await _context.Products
//                 .CountAsync(p => p.ProductType == "unit" && p.Status == "vacant");
//         }

//         public async Task<int> GetOccupiedUnitsCountAsync()
//         {
//             return await _context.Products
//                 .CountAsync(p => p.ProductType == "unit" && p.Status == "occupied");
//         }

//         public async Task<int> GetMaintenanceUnitsCountAsync()
//         {
//             return await _context.Products
//                 .CountAsync(p => p.Status == "maintenance");
//         }
//     }
// }
