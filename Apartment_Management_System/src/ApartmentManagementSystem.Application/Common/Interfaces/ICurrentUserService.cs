namespace ApartmentManagementSystem.Application.Common.Interfaces;

/// <summary>
/// Interface for current user service
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}
