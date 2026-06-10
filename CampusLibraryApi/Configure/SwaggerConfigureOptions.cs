using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace CampusLibraryApi.Configure;

public sealed class SwaggerConfigureOptions(
   IApiVersionDescriptionProvider provider
) : IConfigureOptions<SwaggerGenOptions> {
   public void Configure(SwaggerGenOptions options) {
      foreach (var description in provider.ApiVersionDescriptions) {
         options.SwaggerDoc(description.GroupName, new OpenApiInfo {
            Title = "CampusLibraryApi",
            Version = description.GroupName,
            Description = description.IsDeprecated
               ? "This API version has been deprecated."
               : "CampusLibray API ..."
         });
      }
   }
}