using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BackendBiblioMate.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTestsBiblioMate.Middlewares
{
    /// <summary>
    /// Unit tests for <see cref="ExceptionHandlingMiddleware"/>.
    /// Ensures that different exception types are mapped to the correct
    /// HTTP status codes, JSON payloads, and content type headers.
    /// </summary>
    public class ExceptionHandlingMiddlewareTests
    {
        /// <summary>
        /// Executes the middleware with a fake <see cref="HttpContext"/> and a "next" delegate
        /// that always throws the given <paramref name="exception"/>.
        /// This helper rewires the response body to a memory buffer so that the test can
        /// inspect the produced JSON payload afterwards.
        /// </summary>
        /// <param name="exception">The exception to be thrown by the simulated request delegate.</param>
        /// <returns>
        /// A task containing the <see cref="HttpContext"/> whose response has been written
        /// by the middleware after handling the exception.
        /// </returns>
        private static async Task<HttpContext> InvokeWithExceptionAsync(Exception exception)
        {
            // Arrange: build a test HttpContext and redirect response body to a MemoryStream
            var context = new DefaultHttpContext();
            var buffer = new MemoryStream();
            context.Response.Body = buffer;

            // Simulated pipeline: next delegate that throws the supplied exception
            RequestDelegate next = _ => throw exception;

            // Middleware under test (using a NullLogger to avoid side effects)
            var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

            // Act: run the middleware
            await middleware.InvokeAsync(context);

            // Reset stream position so the test can read the response body
            buffer.Seek(0, SeekOrigin.Begin);
            return context;
        }

        /// <summary>
        /// Verifies that a <see cref="ValidationException"/> is translated into:
        /// <list type="bullet">
        ///   <item><description>HTTP 400 Bad Request</description></item>
        ///   <item><description><c>application/json</c> content type</description></item>
        ///   <item><description>A JSON body with <c>error = "ValidationError"</c> and the validation details</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationException_Produces400Json()
        {
            var validationResult = new ValidationResult("Invalid data");
            var vex = new ValidationException(validationResult, null, null);

            var context = await InvokeWithExceptionAsync(vex);

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("ValidationError", root.GetProperty("error").GetString());
            Assert.Equal("Invalid data", root.GetProperty("details").GetString());
        }

        /// <summary>
        /// Verifies that a <see cref="KeyNotFoundException"/> is translated into:
        /// <list type="bullet">
        ///   <item><description>HTTP 404 Not Found</description></item>
        ///   <item><description><c>application/json</c> content type</description></item>
        ///   <item><description>A JSON body with <c>error = "NotFound"</c> and the exception message</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public async Task InvokeAsync_KeyNotFoundException_Produces404Json()
        {
            var knf = new KeyNotFoundException("Resource missing");

            var context = await InvokeWithExceptionAsync(knf);

            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("NotFound", root.GetProperty("error").GetString());
            Assert.Equal("Resource missing", root.GetProperty("message").GetString());
        }

        /// <summary>
        /// Verifies that any generic <see cref="Exception"/> (e.g. <see cref="InvalidOperationException"/>)
        /// is translated into:
        /// <list type="bullet">
        ///   <item><description>HTTP 500 Internal Server Error</description></item>
        ///   <item><description><c>application/json</c> content type</description></item>
        ///   <item><description>A JSON body with <c>error = "InternalError"</c> and a generic message,
        ///   without leaking the internal exception text</description></item>
        /// </list>
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GenericException_Produces500Json()
        {
            var ex = new InvalidOperationException("Boom");

            var context = await InvokeWithExceptionAsync(ex);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("InternalError", root.GetProperty("error").GetString());
            Assert.Equal("An unexpected error occurred. Please try again later.",
                         root.GetProperty("message").GetString());
        }
    }
}
