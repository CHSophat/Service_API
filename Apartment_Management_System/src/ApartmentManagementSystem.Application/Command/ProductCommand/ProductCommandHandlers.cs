using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.ProductDto;
using ApartmentManagementSystem.Domain.Entities.Products;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.ProductCommand
{
    public class ProductCommandHandlers :
        IRequestHandler<CreateProductCommand, ProductDto>,
        IRequestHandler<UpdateProductCommand, ProductDto>,
        IRequestHandler<UpdateProductStatusCommand, ProductDto>,
        IRequestHandler<DeleteProductCommand, bool>,
        IRequestHandler<UpdateMaintenanceStatusCommand, ProductDetailDto>,
        IRequestHandler<CreatePricingRulesCommand, List<PricingRuleDto>>,
        IRequestHandler<RecordMeterReadingsCommand, Dictionary<string, object>>,
        IRequestHandler<AssignAmenitiesToUnitCommand, ProductDetailDto>,
        IRequestHandler<AddProductPhotosCommand, List<ProductPhotoDetailDto>>,
        IRequestHandler<AddProductAttributesCommand, Dictionary<string, string>>,
        IRequestHandler<UpdateProductAttributesCommand, Dictionary<string, string>>,
        IRequestHandler<CreateMaintenanceRequestCommand, MaintenanceRequestDto>,
        IRequestHandler<UpdateMaintenanceRequestCommand, MaintenanceRequestDto>,
        IRequestHandler<CompleteMaintenanceRequestCommand, MaintenanceRequestDto>,
        IRequestHandler<BulkUpdateProductStatusCommand, Dictionary<string, object>>,
        IRequestHandler<RemoveProductPhotoCommand, bool>,
        IRequestHandler<SetPrimaryPhotoCommand, ProductPhotoDetailDto>,
        IRequestHandler<RemoveAmenityFromUnitCommand, bool>,
        IRequestHandler<ReorderProductPhotosCommand, List<ProductPhotoDetailDto>>
    {
        private readonly ApplicationDbContext _context;

        public ProductCommandHandlers(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly HashSet<string> AllowedProductTypes =
            new(StringComparer.Ordinal) { "unit", "parking", "storage", "amenity" };

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var productType = (request.ProductType ?? string.Empty).Trim().ToLowerInvariant();
            if (!AllowedProductTypes.Contains(productType))
                throw new ArgumentException(
                    $"productType must be one of: unit, parking, storage, amenity (received: '{request.ProductType}')");

            var product = new Product
            {
                ProductType = productType,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                BasePrice = request.BasePrice,
                SquareFeet = request.SquareFeet,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                FloorNumber = request.FloorNumber,
                Status = "vacant"
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToProductDto(product);
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken: cancellationToken);
            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            product.Name = request.Name ?? product.Name;
            product.Description = request.Description ?? product.Description;
            product.BasePrice = request.BasePrice > 0 ? request.BasePrice : product.BasePrice;
            product.SquareFeet = request.SquareFeet ?? product.SquareFeet;
            product.Bedrooms = request.Bedrooms ?? product.Bedrooms;
            product.Bathrooms = request.Bathrooms ?? product.Bathrooms;
            product.FloorNumber = request.FloorNumber ?? product.FloorNumber;

            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToProductDto(product);
        }

        public async Task<ProductDto> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken: cancellationToken);
            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            product.Status = request.Status;
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToProductDto(product);
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken: cancellationToken);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ProductDetailDto> Handle(UpdateMaintenanceStatusCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .Include(p => p.MaintenanceRequests)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            if (request.MaintenanceStatus == "under_repair" || request.MaintenanceStatus == "maintenance")
            {
                product.Status = "maintenance";
            }
            else if (request.MaintenanceStatus == "resolved")
            {
                product.Status = "vacant";
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToProductDetailDto(product);
        }

        public async Task<List<PricingRuleDto>> Handle(CreatePricingRulesCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement when pricing_rules table is created
            return new List<PricingRuleDto>();
        }

        public async Task<Dictionary<string, object>> Handle(RecordMeterReadingsCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement when meter_readings table is created
            return new Dictionary<string, object>();
        }

        public async Task<ProductDetailDto> Handle(AssignAmenitiesToUnitCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductPhotos)
                .Include(p => p.ProductAttributes)
                .Include(p => p.MaintenanceRequests)
                .FirstOrDefaultAsync(p => p.Id == request.UnitProductId, cancellationToken);

            if (product == null)
                throw new Exception($"Unit {request.UnitProductId} not found");

            // TODO: Implement when product_links junction table is created

            await _context.SaveChangesAsync(cancellationToken);
            return MapToProductDetailDto(product);
        }

        public async Task<List<ProductPhotoDetailDto>> Handle(AddProductPhotosCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductPhotos)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            for (int i = 0; i < request.PhotoUrls.Count; i++)
            {
                var photo = new ProductPhoto
                {
                    ProductId = request.ProductId,
                    PhotoUrl = request.PhotoUrls[i],
                    IsPrimary = i == request.PrimaryPhotoIndex,
                    SortOrder = i
                };
                product.ProductPhotos.Add(photo);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var photos = product.ProductPhotos
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
                .ToList();

            return photos;
        }

        public async Task<Dictionary<string, string>> Handle(AddProductAttributesCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductAttributes)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            foreach (var attr in request.Attributes)
            {
                var productAttr = new ProductAttribute
                {
                    ProductId = request.ProductId,
                    AttributeName = attr.Key,
                    AttributeValue = attr.Value
                };
                product.ProductAttributes.Add(productAttr);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return product.ProductAttributes
                .ToDictionary(pa => pa.AttributeName, pa => pa.AttributeValue);
        }

        public async Task<Dictionary<string, string>> Handle(UpdateProductAttributesCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.ProductAttributes)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                throw new Exception($"Product {request.ProductId} not found");

            // Remove old attributes
            _context.ProductAttributes.RemoveRange(product.ProductAttributes);

            // Add new attributes
            foreach (var attr in request.Attributes)
            {
                var productAttr = new ProductAttribute
                {
                    ProductId = request.ProductId,
                    AttributeName = attr.Key,
                    AttributeValue = attr.Value
                };
                product.ProductAttributes.Add(productAttr);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return product.ProductAttributes
                .ToDictionary(pa => pa.AttributeName, pa => pa.AttributeValue);
        }

        public async Task<MaintenanceRequestDto> Handle(CreateMaintenanceRequestCommand request, CancellationToken cancellationToken)
        {
            var maintenance = new MaintenanceRequest
            {
                ProductId = request.ProductId,
                CustomerId = request.CustomerId,
                Priority = request.Priority ?? "medium",
                Description = request.Description,
                PhotoUrls = string.Join(",", request.PhotoUrls ?? new List<string>()),
                Status = "open"
            };

            _context.MaintenanceRequests.Add(maintenance);
            await _context.SaveChangesAsync(cancellationToken);

            return new MaintenanceRequestDto
            {
                Id = maintenance.Id,
                Priority = maintenance.Priority,
                Status = maintenance.Status,
                Description = maintenance.Description,
                CreatedAt = maintenance.CreatedAt
            };
        }

        public async Task<MaintenanceRequestDto> Handle(UpdateMaintenanceRequestCommand request, CancellationToken cancellationToken)
        {
            var maintenance = await _context.MaintenanceRequests
                .FindAsync(new object[] { request.MaintenanceRequestId }, cancellationToken: cancellationToken);

            if (maintenance == null)
                throw new Exception($"Maintenance request {request.MaintenanceRequestId} not found");

            maintenance.Status = request.Status ?? maintenance.Status;
            maintenance.Priority = request.Priority ?? maintenance.Priority;
            maintenance.AssignedTo = request.AssignedToUserId ?? maintenance.AssignedTo;

            _context.MaintenanceRequests.Update(maintenance);
            await _context.SaveChangesAsync(cancellationToken);

            return new MaintenanceRequestDto
            {
                Id = maintenance.Id,
                Priority = maintenance.Priority,
                Status = maintenance.Status,
                Description = maintenance.Description,
                CreatedAt = maintenance.CreatedAt,
                AssignedTo = maintenance.AssignedTo.HasValue ? "User-" + maintenance.AssignedTo : null
            };
        }

        public async Task<MaintenanceRequestDto> Handle(CompleteMaintenanceRequestCommand request, CancellationToken cancellationToken)
        {
            var maintenance = await _context.MaintenanceRequests
                .FindAsync(new object[] { request.MaintenanceRequestId }, cancellationToken: cancellationToken);

            if (maintenance == null)
                throw new Exception($"Maintenance request {request.MaintenanceRequestId} not found");

            maintenance.Status = "completed";
            maintenance.CompletedAt = DateTime.UtcNow;

            _context.MaintenanceRequests.Update(maintenance);
            await _context.SaveChangesAsync(cancellationToken);

            return new MaintenanceRequestDto
            {
                Id = maintenance.Id,
                Priority = maintenance.Priority,
                Status = maintenance.Status,
                Description = maintenance.Description,
                CreatedAt = maintenance.CreatedAt
            };
        }

        public async Task<Dictionary<string, object>> Handle(BulkUpdateProductStatusCommand request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Where(p => request.ProductIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var updates = new List<Dictionary<string, object>>();
            var failedCount = 0;

            foreach (var product in products)
            {
                try
                {
                    var oldStatus = product.Status;
                    product.Status = request.Status;
                    _context.Products.Update(product);

                    updates.Add(new Dictionary<string, object>
                    {
                        { "product_id", product.Id },
                        { "old_status", oldStatus },
                        { "new_status", product.Status },
                        { "success", true }
                    });
                }
                catch
                {
                    failedCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new Dictionary<string, object>
            {
                { "updated_count", updates.Count },
                { "failed_count", failedCount },
                { "updates", updates }
            };
        }

        public async Task<bool> Handle(RemoveProductPhotoCommand request, CancellationToken cancellationToken)
        {
            var photo = await _context.ProductPhotos
                .FindAsync(new object[] { request.PhotoId }, cancellationToken: cancellationToken);

            if (photo == null)
                return false;

            _context.ProductPhotos.Remove(photo);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ProductPhotoDetailDto> Handle(SetPrimaryPhotoCommand request, CancellationToken cancellationToken)
        {
            var photos = await _context.ProductPhotos
                .Where(pp => pp.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);

            // Remove primary flag from all photos
            foreach (var photo in photos)
                photo.IsPrimary = false;

            // Set new primary
            var primaryPhoto = photos.FirstOrDefault(pp => pp.Id == request.PhotoId);
            if (primaryPhoto != null)
                primaryPhoto.IsPrimary = true;

            _context.ProductPhotos.UpdateRange(photos);
            await _context.SaveChangesAsync(cancellationToken);

            return new ProductPhotoDetailDto
            {
                Id = primaryPhoto.Id,
                Url = primaryPhoto.PhotoUrl,
                IsPrimary = primaryPhoto.IsPrimary,
                SortOrder = primaryPhoto.SortOrder,
                ThumbnailUrl = GenerateThumbnailUrl(primaryPhoto.PhotoUrl),
                UploadedAt = primaryPhoto.CreatedAt
            };
        }

        public async Task<bool> Handle(RemoveAmenityFromUnitCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement when product_links junction table is created
            return true;
        }

        public async Task<List<ProductPhotoDetailDto>> Handle(ReorderProductPhotosCommand request, CancellationToken cancellationToken)
        {
            var photos = await _context.ProductPhotos
                .Where(pp => pp.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);

            foreach (var photo in photos)
            {
                if (request.PhotoIdToSortOrder.TryGetValue(photo.Id, out var newSortOrder))
                    photo.SortOrder = newSortOrder;
            }

            _context.ProductPhotos.UpdateRange(photos);
            await _context.SaveChangesAsync(cancellationToken);

            return photos
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
                .ToList();
        }

        // Helper methods
        private ProductDto MapToProductDto(Product product)
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
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private ProductDetailDto MapToProductDetailDto(Product product)
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
                        CreatedAt = mr.CreatedAt
                    })
                    .ToList() ?? new List<MaintenanceRequestDto>()
            };
        }

        // Static so EF's expression compiler can lift it into the SQL projection.
        // See ProductQueryHandlers for the matching version.
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
