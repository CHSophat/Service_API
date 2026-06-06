using Microsoft.AspNetCore.Http;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// Shared helpers used by minimal-API endpoint groups to keep error/stub shapes consistent.
/// </summary>
public static class EndpointResults
{
    /// <summary>
    /// Replacement for the old 501-returning stub. Returns an empty success envelope
    /// so client UIs render gracefully (loading -> empty state) while the real
    /// query/command handler is still being built. The <paramref name="todo"/>
    /// note is echoed back in the envelope's <c>message</c> so testers can see
    /// exactly which handler is still pending.
    /// </summary>
    public static IResult EmptyOk(string todo) =>
        Results.Json(new ApiResponse<object[]>
        {
            Status = "success",
            Data = Array.Empty<object>(),
            Message = $"OK (stub — pending: {todo})",
            StatusCode = StatusCodes.Status200OK
        }, statusCode: StatusCodes.Status200OK);

    /// <summary>
    /// Same as <see cref="EmptyOk"/> but with a configurable default payload —
    /// use for endpoints whose contract expects a single object instead of a list.
    /// </summary>
    public static IResult OkStub<T>(T data, string todo) =>
        Results.Json(new ApiResponse<T>
        {
            Status = "success",
            Data = data!,
            Message = $"OK (stub — pending: {todo})",
            StatusCode = StatusCodes.Status200OK
        }, statusCode: StatusCodes.Status200OK);

    // ---- kept for backwards source-compat with any caller still using these ----

    public static IResult NotImplemented(string what) => EmptyOk(what);

    public static IResult NotFound(string what) =>
        Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not found",
            detail: what);
}
