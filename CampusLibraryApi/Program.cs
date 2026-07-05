using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using CampusLibraryApi._3_Core;
using CampusLibraryApi._3_Core.Readers;
using CampusLibraryApi._4_Infrastructure;
using CampusLibraryApi.Configure;

namespace CampusLibraryApi;

public class Program {

   public static async Task Main(string[] args) {
   
      //---- Configure DI Container (IServiceCollection) ----
      var builder = WebApplication.CreateBuilder(args);
      
      // Access Http-Request in Infrastructure
      builder.Services.AddHttpContextAccessor();

      // Controllers
      builder.Services.AddControllers()
         // enums as string in JSON API
         .AddJsonOptions(options => {
            options.JsonSerializerOptions.Converters.Add(
               new JsonStringEnumConverter()
            );
         });
      
      // Modules
      builder.Services.AddReadersModule();
      builder.Services.AddCatalogModule();
      builder.Services.AddLoansModule();
      builder.Services.AddInfrastructureModule(builder.Configuration);

      builder.Services.AddEndpointsApiExplorer();
      
      // API versioning 
      builder.Services.AddApiReaderAndVersioning();
      
      // Authentication and authorization
      builder.Services.AddCampusLibraryAuthentication(builder.Configuration);

      // Swagger
      builder.Services.AddSwagger();

      var app = builder.Build();

      if (app.Environment.IsDevelopment()) {
         //app.UseHttpLogging();
         app.UseDeveloperExceptionPage();
      
         // // Keep old student/bookmarked URL working after API version migration.
         // app.Use((context, next) => {
         //    if (context.Request.Path.Equals("/swagger/v1/swagger.json", StringComparison.OrdinalIgnoreCase) ||
         //        context.Request.Path.Equals("/swagger/v1/swagger.json/", StringComparison.OrdinalIgnoreCase)) {
         //       context.Request.Path = "/swagger/v2/swagger.json";
         //    }
         //    return next();
         // });
         
         // Avoid stale Swagger UI config/assets after URL/version changes.
         app.Use(async (context, next) => {
            if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)) {
               context.Response.OnStarting(() => {
                  context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                  context.Response.Headers.Pragma = "no-cache";
                  context.Response.Headers.Expires = "0";
                  return Task.CompletedTask;
               });
            }

            await next();
         });

         app.UseSwagger();
         
         app.UseSwaggerUI(options => {
            // Dynamisch alle API-Versionen anzeigen
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
      
            foreach (var description in provider.ApiVersionDescriptions) {
               options.SwaggerEndpoint(
                  $"/swagger/{description.GroupName}/swagger.json",
                  $"CampusLibraryApi {description.GroupName.ToUpperInvariant()}"
               );
            }
      
            options.RoutePrefix = "swagger";
         });
         
      }

      //app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();
      
      app.MapControllers();

      await app.RunAsync();
   }
}
