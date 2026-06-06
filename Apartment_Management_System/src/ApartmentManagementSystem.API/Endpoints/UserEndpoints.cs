using ApartmentManagementSystem.Application.Command.UserCommand;
using ApartmentManagementSystem.Application.DTOs.AuthDto;
using ApartmentManagementSystem.Application.Query.UserQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/users/* — admin-facing user management for the Settings → Users tab.
///
/// Authorization model:
///   • GET list / GET {id}  → users:read   → PropertyManager + Admin
///   • PUT  {id} (update)   → users:update → Admin only
///   • DELETE {id} (soft)   → users:delete → Admin only
///
/// Permissions in the GET-by-id response are flattened from role names via
/// <see cref="Permissions.For"/> so the web client can gate UI; backend
/// enforcement still lives on these endpoints.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/users")
                   .WithTags("Users (Web)");

        // ---- List -----------------------------------------------------------
        g.MapGet("/", async (
                [FromQuery] string? search,
                [FromQuery] string? role,
                [FromQuery] bool? active,
                [FromQuery] int? page,
                [FromQuery] int? limit,
                IMediator mediator) =>
            {
                try
                {
                    var rows = await mediator.Send(new ListUsersQuery
                    {
                        Search = search,
                        RoleName = role,
                        IsActive = active,
                        Page = page ?? 1,
                        Limit = limit ?? 50
                    });
                    return ApiResponseExtensions.Ok(rows, "Users retrieved");
                }
                catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
            })
            .RequireAuthorization(p => p.RequireRole(
                AppRoles.PropertyManager, AppRoles.Admin));

        // ---- Get by id (role + permissions) ---------------------------------
        g.MapGet("/{userId:int}", async (int userId, IMediator mediator) =>
            {
                try
                {
                    var user = await mediator.Send(new GetUserByIdQuery { UserId = userId });
                    if (user is null) return ApiResponseExtensions.NotFound("User not found");

                    // Flatten role-derived permissions onto the response so the
                    // client can gate UI without re-doing the matrix lookup.
                    user.Permissions = Permissions.For(user.Roles).ToArray();
                    return ApiResponseExtensions.Ok(user, "User retrieved");
                }
                catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
            })
            .RequireAuthorization(p => p.RequireRole(
                AppRoles.PropertyManager, AppRoles.Admin));

        // ---- Update (phone + IsActive + roles) ------------------------------
        g.MapPut("/{userId:int}", async (
                int userId,
                [FromBody] UpdateUserRequest req,
                IMediator mediator) =>
            {
                try
                {
                    var updated = await mediator.Send(new UpdateUserCommand
                    {
                        UserId = userId,
                        Phone = req.Phone,
                        IsActive = req.IsActive,
                        RoleNames = req.RoleNames
                    });

                    if (updated is null) return ApiResponseExtensions.NotFound("User not found");

                    updated.Permissions = Permissions.For(updated.Roles).ToArray();
                    return ApiResponseExtensions.Ok(updated, "User updated");
                }
                catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
            })
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        // ---- Delete (soft: IsActive=false + revoke sessions) ----------------
        g.MapDelete("/{userId:int}", async (int userId, IMediator mediator) =>
            {
                try
                {
                    var ok = await mediator.Send(new DeleteUserCommand { UserId = userId });
                    return ok
                        ? ApiResponseExtensions.Ok(true, "User disabled")
                        : ApiResponseExtensions.NotFound("User not found");
                }
                catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
            })
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        return app;
    }
}
