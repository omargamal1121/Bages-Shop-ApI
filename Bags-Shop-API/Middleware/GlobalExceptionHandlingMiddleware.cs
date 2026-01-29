using System.Net;
using System.Text.Json;

namespace Bags_Shop_API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var errorResponse = new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while processing your request.",
                
            };

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Invalid request parameters.";
                    if (_environment.IsDevelopment())
                    {
                        errorResponse.Errors = new List<string> { exception.Message };
                    }
                    break;

                case KeyNotFoundException:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = "The requested resource was not found.";
                    if (_environment.IsDevelopment())
                    {
                        errorResponse.Errors = new List<string> { exception.Message };
                    }
                    break;

                case UnauthorizedAccessException:
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "You are not authorized to perform this action.";
                    break;

                case InvalidOperationException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "The operation is not valid in the current state.";
                    if (_environment.IsDevelopment())
                    {
                        errorResponse.Errors = new List<string> { exception.Message };
                    }
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An internal server error occurred.";
  
                  
                        errorResponse.Errors = new List<string> 
                        { 
                            exception.Message,
                            exception.StackTrace ?? "No stack trace available"
                        };
               

                    break;
            }

            context.Response.StatusCode = errorResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(json);
        }
    }
}
