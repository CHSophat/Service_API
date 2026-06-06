using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApartmentManagementSystem.API.Swagger;

/// <summary>
/// Per-operation Swagger filter that toggles the JWT lock icon based on
/// whether the endpoint requires authentication.
///
///  • Endpoint has <see cref="AllowAnonymousAttribute"/>   → no lock, no 401 doc
///  • Endpoint requires auth (default fallback policy, or
///    <see cref="AuthorizeAttribute"/>)                    → lock + 401 doc,
///                                                          + role list when present
///
/// This replaces a global <c>AddSecurityRequirement</c> which would stamp every
/// operation (including <c>/auth/login</c>) with a lock incorrectly.
/// </summary>
public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        if (allowsAnonymous)
        {
            // Public endpoint — no lock, no 401 documentation.
            return;
        }

        // Authorize required. Surface 401 + (when present) required roles.
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });

        var authorizeAttrs = metadata.OfType<IAuthorizeData>().ToList();
        var roles = authorizeAttrs
            .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
            .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .ToList();

        if (roles.Count > 0)
        {
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = $"Forbidden — requires one of: {string.Join(", ", roles)}"
            });

            // Surface required roles in the operation description so testers can see them.
            var rolesNote = $"\n\n**Roles required:** {string.Join(", ", roles)}";
            operation.Description = (operation.Description ?? string.Empty) + rolesNote;
        }

        // Attach the Bearer security requirement to this operation only.
        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
        ];
    }
}
