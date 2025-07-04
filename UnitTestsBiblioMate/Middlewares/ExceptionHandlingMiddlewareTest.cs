using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BackendBiblioMate.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTestsBiblioMate.Middlewares
{
    /// <summary>
    /// Unit tests for <see cref="ExceptionHandlingMiddleware"/>.
    /// Verifies that various exception types produce the correct HTTP status,
    /// content type, and JSON error payload.
    /// </summary>
    public class ExceptionHandlingMiddlewareTests
    {
        /// <summary>
        /// Helper to execute the middleware with a given exception thrown by the "next" delegate.
        /// </summary>
        private static async Task<HttpContext> InvokeWithExceptionAsync(Exception exception)
        {
            // Arrange: create a default context and swap its response body to a buffer
            var context = new DefaultHttpContext();
            var buffer = new MemoryStream();
            context.Response.Body = buffer;

            // Next delegate simply throws the provided exception
            RequestDelegate next = _ => throw exception;

            var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Rewind body for inspection
            buffer.Seek(0, SeekOrigin.Begin);
            return context;
        }

        /// <summary>
        /// ValidationException should produce a 400 Bad Request with a "ValidationError" payload.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationException_Produces400Json()
        {
            // Arrange
            var validationResult = new ValidationResult("Invalid data");
            var vex = new ValidationException(validationResult, null, null);

            // Act
            var context = await InvokeWithExceptionAsync(vex);

            // Assert status and headers
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read and parse JSON
            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("ValidationError", root.GetProperty("error").GetString());
            Assert.Equal("Invalid data", root.GetProperty("details").GetString());
        }

        /// <summary>
        /// KeyNotFoundException should produce a 404 Not Found with a "NotFound" payload.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_KeyNotFoundException_Produces404Json()
        {
            // Arrange
            var knf = new KeyNotFoundException("Resource missing");

            // Act
            var context = await InvokeWithExceptionAsync(knf);

            // Assert status and headers
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read and parse JSON
            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("NotFound", root.GetProperty("error").GetString());
            Assert.Equal("Resource missing", root.GetProperty("message").GetString());
        }

        /// <summary>
        /// Any other exception should produce a 500 Internal Server Error with an "InternalError" payload.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GenericException_Produces500Json()
        {
            // Arrange
            var ex = new InvalidOperationException("Boom");

            // Act
            var context = await InvokeWithExceptionAsync(ex);

            // Assert status and headers
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read and parse JSON
            var payload = await JsonDocument.ParseAsync(context.Response.Body);
            var root = payload.RootElement;
            Assert.Equal("InternalError", root.GetProperty("error").GetString());
            Assert.Equal("An unexpected error occurred. Please try again later.",
                         root.GetProperty("message").GetString());
        }
    }
}