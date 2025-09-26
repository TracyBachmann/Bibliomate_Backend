using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Swagger operation filter that fills in default values for versioned routes
    /// and ensures unique operation identifiers for the Swagger/OpenAPI specification.
    /// </summary>
    /// <remarks>
    /// - Replaces the <c>{version}</c> placeholder in route parameters with the actual API version.
    /// - Ensures that each <see cref="OpenApiOperation"/> has a unique <c>operationId</c>,
    ///   preventing collisions in Swagger UI and client code generation.
    /// </remarks>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies default values to the given Swagger operation.
        /// </summary>
        /// <param name="operation">The Swagger <see cref="OpenApiOperation"/> being processed.</param>
        /// <param name="context">The current <see cref="OperationFilterContext"/> containing metadata about the API action.</param>
        /// <remarks>
        /// This method will:
        /// <list type="bullet">
        /// <item><description>Generate a unique <c>operationId</c> using the declaring type and method name.</description></item>
        /// <item><description>Mark the <c>version</c> parameter as required and give it a default value.</description></item>
        /// </list>
        /// </remarks>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;

            // Ensure a unique operationId so Swagger UI and generated clients don’t collide.
            operation.OperationId ??= apiDesc.TryGetMethodInfo(out var methodInfo)
                ? $"{methodInfo.DeclaringType!.Name}_{methodInfo.Name}"
                : null;

            // Replace the {version} parameter in routes with the actual API version.
            if (operation.Parameters == null) return;

            foreach (var param in operation.Parameters.Where(p => p.Name == "version"))
            {
                param.Schema.Default  = new OpenApiString(apiDesc.GroupName);
                param.Description     = "The API version to call (injected automatically by versioned routes).";
                param.Required        = true;
            }
        }
    }
}
