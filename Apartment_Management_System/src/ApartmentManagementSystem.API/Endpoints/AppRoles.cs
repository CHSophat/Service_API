namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// Logical roles used to gate App (Mobile) vs Web (MSI) callers on shared <c>/api/v1/*</c> endpoints.
/// </summary>
public static class AppRoles
{
    public const string Tenant = "Tenant";
    public const string Owner = "Owner";
    public const string PropertyManager = "PropertyManager";
    public const string Admin = "Admin";
    public const string Staff = "Staff";

    public const string AppRoles_Csv = $"{Tenant},{Owner}";
    public const string WebRoles_Csv = $"{PropertyManager},{Admin},{Staff}";
    public const string AnyRole_Csv = $"{Tenant},{Owner},{PropertyManager},{Admin},{Staff}";
}
