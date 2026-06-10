using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace CampusLibraryApi.Configure;

public sealed class SwaggerProblemDetailsSchemaFilter : ISchemaFilter {
   public void Apply(IOpenApiSchema schema, SchemaFilterContext context) {
      if (context.Type != typeof(ProblemDetails))
         return;

      if (schema is not OpenApiSchema concreteSchema)
         return;

      concreteSchema.AdditionalPropertiesAllowed = true;
      concreteSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();

      concreteSchema.Properties["code"] = new OpenApiSchema {
         Type = JsonSchemaType.String | JsonSchemaType.Null
      };
      concreteSchema.Properties["traceId"] = new OpenApiSchema {
         Type = JsonSchemaType.String | JsonSchemaType.Null
      };
   }
}