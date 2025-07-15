using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Fills in default values for versioned routes so that {version} is replaced
    /// and each operationId is unique.
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;

            // Ensure a unique operationId so that Swagger UI doesn't collide names
            operation.OperationId ??= apiDesc.TryGetMethodInfo(out var mi)
                ? $"{mi.DeclaringType!.Name}_{mi.Name}"
                : null;

            // Replace the {version} param with the actual API version
            if (operation.Parameters == null) return;

            foreach (var param in operation.Parameters.Where(p => p.Name == "version"))
            {
                param.Schema.Default      = new OpenApiString(apiDesc.GroupName);
                param.Description         = "API version";
                param.Required            = true;
            }
        }
    }
}