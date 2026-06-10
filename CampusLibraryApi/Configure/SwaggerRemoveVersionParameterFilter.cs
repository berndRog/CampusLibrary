using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace CampusLibraryApi.Configure;

public sealed class SwaggerRemoveVersionParameterFilter : IOperationFilter {
   public void Apply(
      OpenApiOperation operation,
      OperationFilterContext context
   ) {
      if (operation.Parameters is null || operation.Parameters.Count == 0)
         return;

      var versionParameter = operation.Parameters
         .FirstOrDefault(p => string.Equals(p.Name, "version", StringComparison.OrdinalIgnoreCase));

      if (versionParameter is not null)
         operation.Parameters.Remove(versionParameter);
   }
}