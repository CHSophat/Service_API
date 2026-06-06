using Microsoft.AspNetCore.Http;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// Wire-format envelope every endpoint returns. Mobile + Web client SDKs
/// already unwrap <c>.data</c>; do not change field names without a contract update.
/// </summary>
public class ApiResponse<T>
{
    public required string Status { get; set; }
    public required T Data { get; set; }
    public required string Message { get; set; }
    public int StatusCode { get; set; }
}

internal static class ApiResponseExtensions
{
    public static IResult Ok<T>(T data, string message = "OK") =>
        Results.Ok(new ApiResponse<T>
        {
            Status = "success",
            Data = data,
            Message = message,
            StatusCode = StatusCodes.Status200OK
        });

    public static IResult Created<T>(T data, string message = "Created") =>
        Results.Json(new ApiResponse<T>
        {
            Status = "success",
            Data = data,
            Message = message,
            StatusCode = StatusCodes.Status201Created
        }, statusCode: StatusCodes.Status201Created);

    public static IResult NotFound(string message = "Not found") =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status404NotFound
        }, statusCode: StatusCodes.Status404NotFound);

    public static IResult BadRequest(string message = "Invalid request") =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status400BadRequest
        }, statusCode: StatusCodes.Status400BadRequest);

    public static IResult Conflict(string message) =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status409Conflict
        }, statusCode: StatusCodes.Status409Conflict);

    public static IResult Locked(string message) =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status423Locked
        }, statusCode: StatusCodes.Status423Locked);

    public static IResult Unauthorized(string message = "Unauthorized") =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status401Unauthorized
        }, statusCode: StatusCodes.Status401Unauthorized);

    public static IResult Forbidden(string message = "Forbidden") =>
        Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = message,
            StatusCode = StatusCodes.Status403Forbidden
        }, statusCode: StatusCodes.Status403Forbidden);

    public static IResult Error(Exception ex)
    {
        // Mirror the full exception (including stack and InnerException chain)
        // to the process log so dev/ops can diagnose without re-running.
        Console.Error.WriteLine($"[API ERROR] {ex}");

        // Walk the InnerException chain so the *real* cause shows up in the
        // response. Handlers in the Application layer wrap repository errors in
        // generic "Failed to retrieve X" ApplicationExceptions; without this,
        // the caller only sees the outer wrapper and has no idea whether the
        // root cause is a missing column, a connection failure, or a bug.
        var messages = new List<string>();
        var current = ex;
        var seen = new HashSet<Exception>();
        Exception? pgException = null;
        while (current is not null && seen.Add(current))
        {
            messages.Add($"{current.GetType().Name}: {current.Message}");
            // Detect Npgsql's PostgresException by type name so this project
            // doesn't need a direct Npgsql package reference.
            if (pgException is null && current.GetType().Name == "PostgresException")
                pgException = current;
            current = current.InnerException;
        }

        // Postgres data-integrity violations are caused by bad client input,
        // not server bugs — surface them as 400/409 with a clean message.
        if (pgException is not null && TryMapPostgresViolation(pgException, out var status, out var friendly))
        {
            return Results.Json(new ApiResponse<object>
            {
                Status = "error",
                Data = new object(),
                Message = friendly,
                StatusCode = status
            }, statusCode: status);
        }

        return Results.Json(new ApiResponse<object>
        {
            Status = "error",
            Data = new object(),
            Message = string.Join(" -> ", messages),
            StatusCode = StatusCodes.Status500InternalServerError
        }, statusCode: StatusCodes.Status500InternalServerError);
    }

    // Map common Postgres SQLSTATE codes (23xxx = integrity constraint
    // violations) to client-facing error responses. The SqlState property is
    // read via reflection to avoid a hard Npgsql dependency in the API layer.
    private static bool TryMapPostgresViolation(Exception pgEx, out int statusCode, out string message)
    {
        var sqlState = pgEx.GetType().GetProperty("SqlState")?.GetValue(pgEx) as string;
        var constraint = pgEx.GetType().GetProperty("ConstraintName")?.GetValue(pgEx) as string;
        var column = pgEx.GetType().GetProperty("ColumnName")?.GetValue(pgEx) as string;
        var table = pgEx.GetType().GetProperty("TableName")?.GetValue(pgEx) as string;

        switch (sqlState)
        {
            case "23514": // check_violation
                statusCode = StatusCodes.Status400BadRequest;
                message = !string.IsNullOrEmpty(constraint)
                    ? $"Value violates the '{constraint}' check constraint on table '{table}'. Please send an allowed value."
                    : "Submitted value violates a database check constraint.";
                return true;
            case "23505": // unique_violation
                statusCode = StatusCodes.Status409Conflict;
                message = !string.IsNullOrEmpty(constraint)
                    ? $"A record with this value already exists (unique constraint '{constraint}')."
                    : "A record with this value already exists.";
                return true;
            case "23502": // not_null_violation
                statusCode = StatusCodes.Status400BadRequest;
                message = !string.IsNullOrEmpty(column)
                    ? $"Required field '{column}' is missing."
                    : "A required field is missing.";
                return true;
            case "23503": // foreign_key_violation
                statusCode = StatusCodes.Status400BadRequest;
                message = !string.IsNullOrEmpty(constraint)
                    ? $"Referenced record does not exist (foreign key '{constraint}')."
                    : "Referenced record does not exist.";
                return true;
            default:
                statusCode = 0;
                message = string.Empty;
                return false;
        }
    }
}

/// <summary>Form-upload payload for lease documents.</summary>
public class UploadLeaseDocumentRequest
{
    public required string DocumentType { get; set; }
    public IFormFile? File { get; set; }
    public required string FileUrl { get; set; }
    public required string Description { get; set; }
}

public class SignLeaseRequest
{
    public required string SignedBy { get; set; }
}

public class CreateMoveChecklistRequest
{
    public DateTime InspectionDate { get; set; }
    public required string InspectorName { get; set; }
    public required string OverallCondition { get; set; }
    public required string Notes { get; set; }
}

public class SendCommunicationRequest
{
    public required string Type { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
}
