namespace Bags_Shop_API.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for an admin endpoint
            if (context.Request.Path.StartsWithSegments("/api/admin"))
            {
                // Check if API key is provided in header
                if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
                {
                    _logger.LogWarning("API Key missing for admin endpoint: {Path}", context.Request.Path);
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"success\":false,\"message\":\"API Key is missing\",\"statusCode\":401}");
                    return;
                }

                // Get the valid API key from configuration
                var validApiKey = _configuration["ApiSettings:AdminApiKey"];

                if (string.IsNullOrWhiteSpace(validApiKey))
                {
                    _logger.LogError("Admin API Key is not configured in appsettings");
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"success\":false,\"message\":\"Server configuration error\",\"statusCode\":500}");
                    return;
                }

                // Validate the API key
                if (!validApiKey.Equals(extractedApiKey))
                {
                    _logger.LogWarning("Invalid API Key attempt for admin endpoint: {Path}", context.Request.Path);
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"success\":false,\"message\":\"Invalid API Key\",\"statusCode\":401}");
                    return;
                }

                _logger.LogInformation("Valid API Key provided for admin endpoint: {Path}", context.Request.Path);
            }

            await _next(context);
        }
    }
}
