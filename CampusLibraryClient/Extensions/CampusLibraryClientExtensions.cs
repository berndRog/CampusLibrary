using System.Text.Json;
using CampusLibraryClient.Api.Auth;
using CampusLibraryClient.Api.Clients;
using CampusLibraryClient.Api.Contracts;
using CampusLibraryClient.Core;
using CampusLibrary.Shared.Logging;

namespace CampusLibraryClient.Extensions;

public static class CampusLibraryClientExtensions {

   public static IServiceCollection AddCampusLibraryClients(
      this IServiceCollection services,
      IConfiguration configuration,
      bool useAccessToken = false
   ) {
      string baseUrl = configuration["CampusLibraryApi:BaseUrl"]
         ?? throw new InvalidOperationException("Missing configuration: CampusLibraryApi:BaseUrl");

      services.AddSingleton(
         new JsonSerializerOptions(JsonSerializerDefaults.Web)
      );

      IHttpClientBuilder campusLibraryApiClient = services
         .AddHttpClient(
            name: Common.CampusLibraryApiClientName,
            configureClient: client => {
               client.BaseAddress = new Uri(baseUrl);
            }
         );

      if(useAccessToken) {
         // Part 6: the handler forwards the current access token for Reader /me calls.
         // Part 7 hardens all API client calls systematically.
         campusLibraryApiClient.AddHttpMessageHandler<AccessTokenHandler>();
      }

      campusLibraryApiClient.AddHttpMessageHandler<OutgoingHttpLoggingHandler>();

      // Prepared for token flows. It is harmless while auth is disabled.
      services.AddHttpClient(
         name: Common.IdentityAccessServerClientName
      );

      services.AddScoped<IReaderClient, ReaderClient>();
      services.AddScoped<IBookClient, BookClient>();
      services.AddScoped<ILoanClient, LoanClient>();

      return services;
   }
}
