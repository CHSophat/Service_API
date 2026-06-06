using Microsoft.AspNetCore.Mvc.Filters;

namespace ApartmentManagementSystem.API.Filters;

/// <summary>
/// Filter for API key authentication
/// </summary>
public class ApiKeyAuthFilter : IAsyncActionFilter
{
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var apiKey = _configuration.GetValue<string>("ApiKey");

        if (!apiKey?.Equals(extractedApiKey) ?? true)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        await next();
    }
}
