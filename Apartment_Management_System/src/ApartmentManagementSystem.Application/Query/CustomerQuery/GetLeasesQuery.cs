using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetLeasesQuery : IRequest<List<LeaseDto>>
    {
        /// <summary>
        /// Optional filter by lease status (e.g., "active", "expired", "pending")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Optional filter by property ID (ProductId)
        /// </summary>
        public int? PropertyId { get; set; }

        /// <summary>
        /// Optional search query (searches tenant name, email, or lease details)
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Pagination: page number (default 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Pagination: page size (default 20)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
