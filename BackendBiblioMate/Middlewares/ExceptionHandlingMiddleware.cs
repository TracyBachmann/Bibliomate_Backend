using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace BackendBiblioMate.Middlewares
{
    /// <summary>
    /// Middleware for global exception handling.
    /// Catches unhandled exceptions in the request pipeline and returns standardized JSON error responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the pipeline.</param>
        /// <param name="logger">The logger used to record exception details.</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the current HTTP request, invoking the next middleware in the pipeline,
        /// and catches exceptions to handle them centrally.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
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
        /// Handles an exception by logging its details and writing a structured JSON response.
        /// </summary>
        /// <param name="context">The current HTTP context in which the exception occurred.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation of writing the response.</returns>
        /// <remarks>
        /// Maps exceptions to standard HTTP status codes:
        /// <list type="bullet">
        ///   <item><see cref="ValidationException"/> → 400 Bad Request</item>
        ///   <item><see cref="KeyNotFoundException"/> → 404 Not Found</item>
        ///   <item>Any other exception → 500 Internal Server Error</item>
        /// </list>
        /// </remarks>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            object payload;

            switch (exception)
            {
                case ValidationException vex:
                    _logger.LogWarning(
                        vex,
                        "Validation failed: {Message}",
                        vex.ValidationResult.ErrorMessage ?? vex.Message);

                    statusCode = (int)HttpStatusCode.BadRequest;
                    payload = new
                    {
                        error = "ValidationError",
                        details = vex.ValidationResult.ErrorMessage ?? vex.Message
                    };
                    break;

                case KeyNotFoundException knf:
                    _logger.LogInformation(
                        knf,
                        "Resource not found: {Message}",
                        knf.Message);

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
