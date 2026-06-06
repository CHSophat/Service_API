using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ApartmentManagementSystem.Application.Command.MessagingCommand;
using ApartmentManagementSystem.Application.Query.MessagingQuery;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/conversations/* – tenant ↔ PM messaging. Used by App + Web.
/// </summary>
public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/conversations")
                   .WithTags("Messaging (Shared)")
                   .RequireAuthorization();

        g.MapGet("/", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var result = await mediator.Send(new GetConversationsQuery { UserId = userId });
                return ApiResponseExtensions.Ok(result, "Conversations retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/", async ([FromBody] CreateConversationRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var result = await mediator.Send(new CreateConversationCommand
                {
                    CreatedByUserId = userId,
                    Subject = request.Subject,
                    PropertyId = request.PropertyId,
                    ParticipantUserIds = request.ParticipantUserIds ?? new List<int>()
                });
                return ApiResponseExtensions.Created(result, "Conversation created successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var result = await mediator.Send(new GetConversationByIdQuery { Id = id, UserId = userId });
                return result is null
                    ? ApiResponseExtensions.NotFound("Conversation not found")
                    : ApiResponseExtensions.Ok(result, "Conversation retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}/messages", async (int id, [FromQuery] DateTime? before, [FromQuery] int limit, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var result = await mediator.Send(new GetMessagesQuery
                {
                    ConversationId = id,
                    UserId = userId,
                    Before = before,
                    Limit = limit > 0 ? limit : 50
                });
                return ApiResponseExtensions.Ok(result, "Messages retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{id:int}/messages", async (int id, [FromBody] SendMessageRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var result = await mediator.Send(new SendMessageCommand
                {
                    ConversationId = id,
                    SenderUserId = userId,
                    Body = request.Body ?? string.Empty,
                    Kind = request.Kind ?? "text",
                    AttachmentUrl = request.AttachmentUrl
                });
                return result is null
                    ? ApiResponseExtensions.Forbidden("You are not a participant of this conversation")
                    : ApiResponseExtensions.Created(result, "Message sent successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{id:int}/read", async (int id, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(user);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var ok = await mediator.Send(new MarkConversationReadCommand { ConversationId = id, UserId = userId });
                return ok
                    ? ApiResponseExtensions.Ok(true, "Conversation marked as read")
                    : ApiResponseExtensions.NotFound("Conversation participant not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // WS handshake placeholder
        app.MapGet("/ws/messages", () =>
            EndpointResults.NotImplemented("WebSocket /ws/messages (events: new_message, typing) — wire SignalR or raw WS"))
           .WithTags("Messaging (Shared)")
           .RequireAuthorization();

        return app;
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("sub")
                 ?? user.FindFirst("userId")
                 ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                 ?? user.FindFirst("nameid");
        return int.TryParse(claim?.Value, out var id) ? id : 0;
    }
}

public class CreateConversationRequest
{
    public string? Subject { get; set; }
    public int? PropertyId { get; set; }
    public List<int>? ParticipantUserIds { get; set; }
}

public class SendMessageRequest
{
    public string? Body { get; set; }
    public string? Kind { get; set; }
    public string? AttachmentUrl { get; set; }
}

