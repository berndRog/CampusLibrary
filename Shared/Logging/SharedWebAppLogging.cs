using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CampusLibrary.Shared.Logging;

/// <summary>
/// Shared logging setup for the CampusLibrary learning solution.
///
/// The same file is linked into the Blazor SSR client, CampusLibraryApi,
/// and IdentityAccessServer. This avoids duplicated logging configuration
/// without introducing an additional shared project.
/// </summary>
public static class SharedWebAppLogging {

   /// <summary>
   /// Logging setup for CampusLibraryApi.
   /// </summary>
   public static void ConfigureCampusLibraryApi(
      WebApplicationBuilder builder
   ) {
      ConfigureProviders(builder);

      builder.Services.AddHttpLogging(options => {
         options.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.ResponseStatusCode;

         options.RequestBodyLogLimit = 256;
         options.ResponseBodyLogLimit = 256;

         ConfigureCommonHeaders(options);
         ConfigureCommonMediaTypes(options);
      });
   }

   /// <summary>
   /// Logging setup for the Blazor SSR client.
   /// </summary>
   public static void ConfigureBlazorSsr(
      WebApplicationBuilder builder
   ) {
      ConfigureProviders(builder);

      builder.Services.AddHttpLogging(options => {
         options.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.ResponseStatusCode;

         options.RequestBodyLogLimit = 512;
         options.ResponseBodyLogLimit = 512;

         ConfigureCommonHeaders(options);
         ConfigureCommonMediaTypes(options);
      });
   }

   /// <summary>
   /// Logging setup for IdentityAccessServer.
   /// </summary>
   public static void ConfigureIdentityAccessServer(
      WebApplicationBuilder builder
   ) {
      ConfigureProviders(builder);

      builder.Services.AddHttpLogging(options => {
         options.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.ResponseStatusCode;

         options.RequestBodyLogLimit = 512;
         options.ResponseBodyLogLimit = 512;

         ConfigureCommonHeaders(options);
         ConfigureCommonMediaTypes(options);
      });
   }

   /// <summary>
   /// Compatibility extension for the existing Part 5 client style.
   /// Prefer <see cref="ConfigureBlazorSsr" /> for new code.
   /// </summary>
   public static WebApplicationBuilder ConfigureSharedWebAppLogging(
      this WebApplicationBuilder builder
   ) {
      ConfigureBlazorSsr(builder);
      return builder;
   }

   private static void ConfigureProviders(
      WebApplicationBuilder builder
   ) {
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.AddDebug();
   }

   private static void ConfigureCommonHeaders(
      HttpLoggingOptions options
   ) {
      options.RequestHeaders.Clear();
      options.RequestHeaders.Add("Authorization");

      options.ResponseHeaders.Clear();
      options.ResponseHeaders.Add("Content-Type");
   }

   private static void ConfigureCommonMediaTypes(
      HttpLoggingOptions options
   ) {
      options.MediaTypeOptions.AddText("application/json");
      options.MediaTypeOptions.AddText("application/problem+json");
      options.MediaTypeOptions.AddText("application/*+json");
      options.MediaTypeOptions.AddText("text/plain");
   }
}
