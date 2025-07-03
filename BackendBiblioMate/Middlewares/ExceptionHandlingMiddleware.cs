using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace BackendBiblioMate.Middlewares
{
    /// <summary>
    /// Middleware to handle exceptions globally and produce standardized JSON error responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger for recording exception details.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next,
                                           ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to process HTTP context and catch exceptions.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions by logging and writing a JSON response with appropriate status code.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            object payload;

            switch (exception)
            {
                case ValidationException vex:
                    _logger.LogWarning(vex, "Validation failed: {Message}", vex.ValidationResult.ErrorMessage ?? vex.Message);
                    statusCode = (int)HttpStatusCode.BadRequest;
                    payload = new
                    {
                        error = "ValidationError",
                        details = vex.ValidationResult.ErrorMessage ?? vex.Message
                    };
                    break;

                case KeyNotFoundException knf:
                    _logger.LogInformation(knf, "Resource not found: {Message}", knf.Message);
                    statusCode = (int)HttpStatusCode.NotFound;
                    payload = new
                    {
                        error = "NotFound",
                        message = knf.Message
                    };
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception");
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    payload = new
                    {
                        error = "InternalError",
                        message = "An unexpected error occurred. Please try again later."
                    };
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(json);
        }
    }
}