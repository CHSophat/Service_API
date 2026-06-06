namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// Canonical permission matrix. Each permission is "resource:action" where
/// resource ∈ {customers, properties, leases, maintenance, invoices,
///            payments, listings, messages, announcements, reports,
///            settings, users}
/// and action ∈ {read, create, update, delete}.
///
/// Roles → permissions are hardcoded here AND mirrored on the frontend
/// (core/auth/permissions.ts). When you change one, change both.
///
/// To make the matrix editable from /settings later, replace the static
/// map with a DB-backed lookup; the public Get(role) signature won't change.
/// </summary>
public static class Permissions
{
    public const string CustomersRead   = "customers:read";
    public const string CustomersCreate = "customers:create";
    public const string CustomersUpdate = "customers:update";
    public const string CustomersDelete = "customers:delete";

    public const string PropertiesRead   = "properties:read";
    public const string PropertiesCreate = "properties:create";
    public const string PropertiesUpdate = "properties:update";
    public const string PropertiesDelete = "properties:delete";

    public const string LeasesRead   = "leases:read";
    public const string LeasesCreate = "leases:create";
    public const string LeasesUpdate = "leases:update";
    public const string LeasesDelete = "leases:delete";

    public const string MaintenanceRead   = "maintenance:read";
    public const string MaintenanceCreate = "maintenance:create";
    public const string MaintenanceUpdate = "maintenance:update";
    public const string MaintenanceDelete = "maintenance:delete";

    public const string InvoicesRead   = "invoices:read";
    public const string InvoicesCreate = "invoices:create";
    public const string InvoicesUpdate = "invoices:update";
    public const string InvoicesDelete = "invoices:delete";

    public const string PaymentsRead   = "payments:read";
    public const string PaymentsCreate = "payments:create";
    public const string PaymentsUpdate = "payments:update";
    public const string PaymentsDelete = "payments:delete";

    public const string ListingsRead   = "listings:read";
    public const string ListingsCreate = "listings:create";
    public const string ListingsUpdate = "listings:update";
    public const string ListingsDelete = "listings:delete";

    public const string MessagesRead   = "messages:read";
    public const string MessagesCreate = "messages:create";
    public const string MessagesUpdate = "messages:update";
    public const string MessagesDelete = "messages:delete";

    public const string AnnouncementsRead   = "announcements:read";
    public const string AnnouncementsCreate = "announcements:create";
    public const string AnnouncementsUpdate = "announcements:update";
    public const string AnnouncementsDelete = "announcements:delete";

    public const string ReportsRead = "reports:read";

    public const string SettingsRead   = "settings:read";
    public const string SettingsUpdate = "settings:update";

    public const string UsersRead   = "users:read";
    public const string UsersCreate = "users:create";
    public const string UsersUpdate = "users:update";
    public const string UsersDelete = "users:delete";

    private static readonly string[] AllPermissions = new[]
    {
        CustomersRead, CustomersCreate, CustomersUpdate, CustomersDelete,
        PropertiesRead, PropertiesCreate, PropertiesUpdate, PropertiesDelete,
        LeasesRead, LeasesCreate, LeasesUpdate, LeasesDelete,
        MaintenanceRead, MaintenanceCreate, MaintenanceUpdate, MaintenanceDelete,
        InvoicesRead, InvoicesCreate, InvoicesUpdate, InvoicesDelete,
        PaymentsRead, PaymentsCreate, PaymentsUpdate, PaymentsDelete,
        ListingsRead, ListingsCreate, ListingsUpdate, ListingsDelete,
        MessagesRead, MessagesCreate, MessagesUpdate, MessagesDelete,
        AnnouncementsRead, AnnouncementsCreate, AnnouncementsUpdate, AnnouncementsDelete,
        ReportsRead,
        SettingsRead, SettingsUpdate,
        UsersRead, UsersCreate, UsersUpdate, UsersDelete,
    };

    /// <summary>Permissions granted to a single role. Unknown roles → empty.</summary>
    public static IReadOnlyCollection<string> Get(string role) => role switch
    {
        AppRoles.Admin           => AllPermissions,
        AppRoles.PropertyManager => new[]
        {
            CustomersRead, CustomersCreate, CustomersUpdate,
            PropertiesRead, PropertiesCreate, PropertiesUpdate,
            LeasesRead, LeasesCreate, LeasesUpdate,
            MaintenanceRead, MaintenanceCreate, MaintenanceUpdate,
            InvoicesRead, InvoicesCreate, InvoicesUpdate,
            PaymentsRead, PaymentsCreate, PaymentsUpdate,
            ListingsRead, ListingsCreate, ListingsUpdate, ListingsDelete,
            MessagesRead, MessagesCreate,
            AnnouncementsRead, AnnouncementsCreate, AnnouncementsUpdate,
            ReportsRead,
            SettingsRead,
            UsersRead,
        },
        AppRoles.Staff => new[]
        {
            CustomersRead, CustomersCreate, CustomersUpdate,
            PropertiesRead,
            LeasesRead,
            MaintenanceRead, MaintenanceCreate, MaintenanceUpdate,
            InvoicesRead,
            PaymentsRead,
            ListingsRead,
            MessagesRead, MessagesCreate,
            AnnouncementsRead,
            ReportsRead,
        },
        AppRoles.Owner => new[]
        {
            CustomersRead,
            PropertiesRead,
            LeasesRead,
            InvoicesRead,
            PaymentsRead,
            ReportsRead,
            MessagesRead, MessagesCreate,
            AnnouncementsRead,
        },
        AppRoles.Tenant => new[]
        {
            // Tenant sees only their own lease/invoices/maintenance via API
            // boundaries (handlers must scope by user); the permission set is
            // small intentionally and the UI shells will hide most CRUD.
            LeasesRead,
            MaintenanceRead, MaintenanceCreate,
            InvoicesRead,
            PaymentsRead, PaymentsCreate,
            MessagesRead, MessagesCreate,
            AnnouncementsRead,
        },
        _ => Array.Empty<string>(),
    };

    /// <summary>Union of permissions across all of a user's roles.</summary>
    public static IReadOnlyCollection<string> For(IEnumerable<string> roles)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in roles) foreach (var p in Get(role)) set.Add(p);
        return set;
    }
}
