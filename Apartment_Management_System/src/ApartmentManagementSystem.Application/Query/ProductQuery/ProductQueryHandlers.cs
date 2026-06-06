using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.ProductDto;
using ApartmentManagementSystem.Application.Query.Repositories;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.ProductQuery
{
    public class ProductQueryHandlers :
        IRequestHandler<GetAllProductsQuery, PaginatedProductResponse<ProductDto>>,
        IRequestHandler<GetAvailableUnitsQuery, PaginatedProductResponse<ProductDto>>,
        IRequestHandler<GetProductByIdQuery, ProductDetailDto>,
        IRequestHandler<GetProductsByTypeQuery, List<ProductDto>>,
        IRequestHandler<GetProductsByStatusQuery, List<ProductDto>>,
        IRequestHandler<GetAvailableAmenitiesQuery, List<ProductDto>>,
        IRequestHandler<GetProductPhotosQuery, List<ProductPhotoDetailDto>>,
        IRequestHandler<GetProductAttributesQuery, Dictionary<string, string>>,
        IRequestHandler<GetProductMaintenanceRequestsQuery, List<MaintenanceRequestDto>>,
        IRequestHandler<GetProductPricingRulesQuery, List<PricingRuleDto>>,
        IRequestHandler<GetProductMeterReadingsQuery, List<MeterReadingDto>>,
        IRequestHandler<SearchProductsQuery, PaginatedProductResponse<ProductDto>>,
        IRequestHandler<GetProductsUnderMaintenanceQuery, List<ProductDetailDto>>,
        IRequestHandler<GetProductCountByStatusQuery, Dictionary<string, int>>,
        IRequestHandler<GetUnitAmenitiesQuery, List<ProductAmenityDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;

        public ProductQueryHandlers(ApplicationDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<PaginatedProductResponse<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(p => p.Status == request.Status);

            if (!string.IsNullOrEmpty(request.ProductType))
                query = query.Where(p => p.ProductType == request.ProductType);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.BasePrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);

            if (request.Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == request.Bedrooms.Value);

            var total = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .Include(p => p.MaintenanceRequests)
                .ToListAsync(cancellationToken);

            var productDtos = products.Select(p => MapToProductDto(p)).ToList();

            return new PaginatedProductResponse<ProductDto>
            {
                Products = productDtos,
                Pagination = new PaginationInfoDto
                {
                    Page = request.PageNumber,
                    Limit = request.PageSize,
                    Total = total,
                    TotalPages = (int)Math.Ceiling(total / (decimal)request.PageSize)
                }
            };
        }

        public async Task<PaginatedProductResponse<ProductDto>> Handle(GetAvailableUnitsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products
                .Where(p => p.ProductType == "unit" && p.Status == "vacant")
                .AsQueryable();

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.BasePrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);

            if (request.Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == request.Bedrooms.Value);

            if (request.Bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms >= request.Bathrooms.Value);

            if (request.MinSqft.HasValue)
                query = query.Where(p => p.SquareFeet >= request.MinSqft.Value);

            if (request.MaxSqft.HasValue)
                query = query.Where(p => p.SquareFeet <= request.MaxSqft.Value);

            if (request.FloorNumber.HasValue)
                query = query.Where(p => p.FloorNumber == request.FloorNumber.Value);

            var total = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .ToListAsync(cancellationToken);

            var productDtos = products.Select(p => MapToProductDto(p)).ToList();

            return new PaginatedProductResponse<ProductDto>
            {
                Products = productDtos,
                Pagination = new PaginationInfoDto
                {
                    Page = request.PageNumber,
                    Limit = request.PageSize,
                    Total = total,
                    TotalPages = (int)Math.Ceiling(total / (decimal)request.PageSize)
                }
            };
        }

        public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .Include(p => p.MaintenanceRequests)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                return null;

            return MapToProductDetailDto(product);
        }

        public async Task<List<ProductDto>> Handle(GetProductsByTypeQuery request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Where(p => p.ProductType == request.ProductType)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .ToListAsync(cancellationToken);

            return products.Select(p => MapToProductDto(p)).ToList();
        }

        public async Task<List<ProductDto>> Handle(GetProductsByStatusQuery request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Where(p => p.Status == request.Status)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .ToListAsync(cancellationToken);

            return products.Select(p => MapToProductDto(p)).ToList();
        }

        public async Task<List<ProductDto>> Handle(GetAvailableAmenitiesQuery request, CancellationToken cancellationToken)
        {
            var amenities = await _context.Products
                .Where(p => p.ProductType == "amenity" && p.Status != "unavailable")
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .ToListAsync(cancellationToken);

            return amenities.Select(p => MapToProductDto(p)).ToList();
        }

        public async Task<List<ProductPhotoDetailDto>> Handle(GetProductPhotosQuery request, CancellationToken cancellationToken)
        {
            var photos = await _context.ProductPhotos
                .Where(pp => pp.ProductId == request.ProductId)
                .OrderBy(pp => pp.SortOrder)
                .Select(pp => new ProductPhotoDetailDto
                {
                    Id = pp.Id,
                    Url = pp.PhotoUrl,
                    IsPrimary = pp.IsPrimary,
                    SortOrder = pp.SortOrder,
                    ThumbnailUrl = GenerateThumbnailUrl(pp.PhotoUrl),
                    UploadedAt = pp.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return photos;
        }

        public async Task<Dictionary<string, string>> Handle(GetProductAttributesQuery request, CancellationToken cancellationToken)
        {
            var attributes = await _context.ProductAttributes
                .Where(pa => pa.ProductId == request.ProductId)
                .ToDictionaryAsync(pa => pa.AttributeName, pa => pa.AttributeValue, cancellationToken);

            return attributes;
        }

        public async Task<List<MaintenanceRequestDto>> Handle(GetProductMaintenanceRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.MaintenanceRequests
                .Where(mr => mr.ProductId == request.ProductId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(mr => mr.Status == request.Status);

            var requests = await query
                .OrderByDescending(mr => mr.CreatedAt)
                .Select(mr => new MaintenanceRequestDto
                {
                    Id = mr.Id,
                    Priority = mr.Priority,
                    Status = mr.Status,
                    Description = mr.Description,
                    CreatedAt = mr.CreatedAt,
                    AssignedTo = mr.AssignedTo.HasValue ? "User-" + mr.AssignedTo : null
                })
                .ToListAsync(cancellationToken);

            return requests;
        }

        public async Task<List<PricingRuleDto>> Handle(GetProductPricingRulesQuery request, CancellationToken cancellationToken)
        {
            // TODO: Implement when pricing_rules table is created in database
            return new List<PricingRuleDto>();
        }

        public async Task<List<MeterReadingDto>> Handle(GetProductMeterReadingsQuery request, CancellationToken cancellationToken)
        {
            // TODO: Implement when meter_readings table is created in database
            return new List<MeterReadingDto>();
        }

        public async Task<PaginatedProductResponse<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products.AsQueryable();

            // Search by term
            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(p => p.Name.Contains(request.SearchTerm) || p.Code.Contains(request.SearchTerm));

            if (!string.IsNullOrEmpty(request.ProductType))
                query = query.Where(p => p.ProductType == request.ProductType);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(p => p.Status == request.Status);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.BasePrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);

            if (request.Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == request.Bedrooms.Value);

            var total = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .ToListAsync(cancellationToken);

            var productDtos = products.Select(p => MapToProductDto(p)).ToList();

            return new PaginatedProductResponse<ProductDto>
            {
                Products = productDtos,
                Pagination = new PaginationInfoDto
                {
                    Page = request.PageNumber,
                    Limit = request.PageSize,
                    Total = total,
                    TotalPages = (int)Math.Ceiling(total / (decimal)request.PageSize)
                }
            };
        }

        public async Task<List<ProductDetailDto>> Handle(GetProductsUnderMaintenanceQuery request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Where(p => p.Status == "maintenance")
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .Include(p => p.MaintenanceRequests)
                .ToListAsync(cancellationToken);

            return products.Select(p => MapToProductDetailDto(p)).ToList();
        }

        public async Task<Dictionary<string, int>> Handle(GetProductCountByStatusQuery request, CancellationToken cancellationToken)
        {
            var counts = await _context.Products
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

            return counts;
        }

        public async Task<List<ProductAmenityDto>> Handle(GetUnitAmenitiesQuery request, CancellationToken cancellationToken)
        {
            // TODO: Implement when product_links junction table is created
            return new List<ProductAmenityDto>();
        }

        // Helper methods
        private ProductDto MapToProductDto(Domain.Entities.Products.Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                ProductType = product.ProductType,
                Status = product.Status,
                BasePrice = product.BasePrice,
                SquareFeet = product.SquareFeet,
                Bedrooms = product.Bedrooms,
                Bathrooms = product.Bathrooms,
                FloorNumber = product.FloorNumber,
                Description = product.Description,
                PrimaryPhoto = product.ProductPhotos?.FirstOrDefault(pp => pp.IsPrimary)?.PhotoUrl,
                Photos = product.ProductPhotos?.Select(pp => pp.PhotoUrl).ToList() ?? new List<string>(),
                Attributes = product.ProductAttributes?
                    .ToDictionary(pa => pa.AttributeName, pa => pa.AttributeValue) ?? new Dictionary<string, string>(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private ProductDetailDto MapToProductDetailDto(Domain.Entities.Products.Product product)
        {
            var baseDto = MapToProductDto(product);
            return new ProductDetailDto
            {
                Id = baseDto.Id,
                Code = baseDto.Code,
                Name = baseDto.Name,
                ProductType = baseDto.ProductType,
                Status = baseDto.Status,
                BasePrice = baseDto.BasePrice,
                SquareFeet = baseDto.SquareFeet,
                Bedrooms = baseDto.Bedrooms,
                Bathrooms = baseDto.Bathrooms,
                FloorNumber = baseDto.FloorNumber,
                Description = baseDto.Description,
                PrimaryPhoto = baseDto.PrimaryPhoto,
                Photos = baseDto.Photos,
                Attributes = baseDto.Attributes,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                PhotoDetails = product.ProductPhotos?
                    .Select(pp => new ProductPhotoDetailDto
                    {
                        Id = pp.Id,
                        Url = pp.PhotoUrl,
                        IsPrimary = pp.IsPrimary,
                        SortOrder = pp.SortOrder,
                        ThumbnailUrl = GenerateThumbnailUrl(pp.PhotoUrl),
                        UploadedAt = pp.CreatedAt
                    })
                    .OrderBy(p => p.SortOrder)
                    .ToList() ?? new List<ProductPhotoDetailDto>(),
                MaintenanceRequests = product.MaintenanceRequests?
                    .Select(mr => new MaintenanceRequestDto
                    {
                        Id = mr.Id,
                        Priority = mr.Priority,
                        Status = mr.Status,
                        Description = mr.Description,
                        CreatedAt = mr.CreatedAt,
                        AssignedTo = mr.AssignedTo.HasValue ? "User-" + mr.AssignedTo : null
                    })
                    .ToList() ?? new List<MaintenanceRequestDto>()
            };
        }

        // Must be static — EF's expression compiler refuses instance methods
        // because they capture `this` as a closure constant. (See exception:
        // "The client projection contains a reference to a constant expression
        //  through the instance method 'GenerateThumbnailUrl'.")
        private static string GenerateThumbnailUrl(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
                return null;

            return originalUrl.Contains('.')
                ? originalUrl.Substring(0, originalUrl.LastIndexOf('.')) + "_thumb" + originalUrl.Substring(originalUrl.LastIndexOf('.'))
                : originalUrl + "_thumb";
        }
    }
}
