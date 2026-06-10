using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace CampusLibraryApi.Configure;

public sealed class SwaggerReplaceVersionWithExactValueInPathFilter : IDocumentFilter {
   public void Apply(
      OpenApiDocument swaggerDoc,
      DocumentFilterContext context
   ) {
      var newPaths = new OpenApiPaths();

      foreach (var (key, value) in swaggerDoc.Paths) {
         var newKey = key.Replace("v{version}", swaggerDoc.Info.Version, StringComparison.OrdinalIgnoreCase);
         newPaths.Add(newKey, value);
      }

      swaggerDoc.Paths = newPaths;
   }
}