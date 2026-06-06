namespace ApartmentManagementSystem.Application.Common.Interfaces;

/// <summary>
/// Interface for database context operations
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
