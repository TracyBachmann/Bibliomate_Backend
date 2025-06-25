using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace backend.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next,
                                           ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                // Specific validation errors
                _logger.LogWarning("Validation failed: {Message}", vex.ValidationResult.ErrorMessage ?? vex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await WriteResponseAsync(context, new
                {
                    error   = "ValidationError",
                    details = vex.ValidationResult.ErrorMessage ?? vex.Message
                });
            }
            catch (KeyNotFoundException knf)
            {
                // Resource not found
                _logger.LogInformation(knf, "Resource not found");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await WriteResponseAsync(context, new
                {
                    error   = "NotFound",
                    message = knf.Message
                });
            }
            catch (Exception ex)
            {
                // Unhandled error
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await WriteResponseAsync(context, new
                {
                    error   = "InternalError",
                    message = "Une erreur est survenue. Merci de réessayer plus tard."
                });
            }
        }

        private static Task WriteResponseAsync(HttpContext context, object payload)
        {
            context.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(payload);
            return context.Response.WriteAsync(json);
        }
    }
}