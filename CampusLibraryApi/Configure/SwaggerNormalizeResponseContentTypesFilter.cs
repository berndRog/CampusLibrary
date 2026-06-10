using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace CampusLibraryApi.Configure;

public sealed class SwaggerNormalizeResponseContentTypesFilter : IOperationFilter {
   public void Apply(OpenApiOperation operation, OperationFilterContext context) {
      var responses = operation.Responses;
      if (responses is null)
         return;

      foreach (var response in responses) {
         if (IsNoContentResponse(response.Key))
            continue;

         if (IsProblemDetailsResponse(response.Key, response.Value)) {
            KeepOnlyContentType(response.Value, "application/problem+json");
            continue;
         }

         if (IsSuccessResponse(response.Key))
            KeepOnlyContentType(response.Value, "application/json");
      }
   }

   private static bool IsSuccessResponse(string statusCode) =>
      int.TryParse(statusCode, out var code) && code is >= 200 and < 300;

   private static bool IsNoContentResponse(string statusCode) =>
      statusCode is "204" or "304";

   private static bool IsProblemDetailsResponse(string statusCode, IOpenApiResponse response) =>
      response.Content is { Count: > 0 } &&
      (IsClientOrServerErrorResponse(statusCode) ||
         response.Content.Values.Any(mediaType =>
            mediaType.Schema is OpenApiSchemaReference { Id: nameof(ProblemDetails) }));

   private static bool IsClientOrServerErrorResponse(string statusCode) =>
      int.TryParse(statusCode, out var code) && code >= 400;

   private static void KeepOnlyContentType(IOpenApiResponse response, string contentType) {
      if (response.Content is not { Count: > 0 } content)
         return;

      var preferredContent = content.TryGetValue(contentType, out var existingContent)
         ? existingContent
         : content.Values.First();

      content.Clear();
      content[contentType] = preferredContent;
   }
}