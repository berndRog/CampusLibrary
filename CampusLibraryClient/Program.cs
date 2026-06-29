using CampusLibraryClient.Api.Auth;
using CampusLibraryClient.Core;
using CampusLibraryClient.Extensions;
using CampusLibrary.Shared.Logging;

namespace CampusLibraryClient;

public static class Program {

   public static void Main(string[] args) {

      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      bool authNEnabled = builder.Configuration.GetValue<bool>(FeatureFlags.AuthNEnabled);
      bool apiAccessTokenEnabled = builder.Configuration.GetValue<bool>(FeatureFlags.ApiAccessTokenEnabled);
      bool authZEnabled = builder.Configuration.GetValue<bool>(FeatureFlags.AuthZEnabled);

      // MVC controllers are already present because they are needed in Part 6
      // for login, logout and entry redirects. In Part 5 they remain inactive.
      builder.Services.AddControllers();

      // Blazor Server-Side Rendering with interactive server components.
      builder.Services
         .AddRazorComponents()
         .AddInteractiveServerComponents();

      // Already registered in Part 5 so the prepared AccessTokenHandler can be
      // activated later without changing the surrounding infrastructure.
      builder.Services.AddHttpContextAccessor();

      if(authNEnabled) {
         // Part 6: Cookie + OpenID Connect login/logout in the SSR client.
         builder.Services.ConfigureAuthN(builder.Configuration);

         // Makes the authentication state available to Blazor components.
         builder.Services.AddCascadingAuthenticationState();

         // Required for controllers or pages that use [Authorize].
         builder.Services.AddAuthorization();
      }

      if(authZEnabled) {
         // Part 8: role/policy based authorization for UI decisions.
         builder.Services.ConfigureAuthZ();
      }

      if(apiAccessTokenEnabled) {
         // Part 8: forwards the current user's access token to CampusLibraryApi.
         builder.Services.AddTransient<AccessTokenHandler>();
      }

      // Optional outgoing request logging for teaching and debugging.
      builder.Services.AddTransient<OutgoingHttpLoggingHandler>();

      // CampusLibrary API clients. In Part 5 they call the API without a Bearer token.
      builder.Services.AddCampusLibraryClients(
         configuration: builder.Configuration,
         useAccessToken: apiAccessTokenEnabled
      );

      WebApplication app = builder.Build();

      if(!app.Environment.IsDevelopment()) {
         app.UseExceptionHandler("/error");
         app.UseHsts();
      }

      app.UseHttpsRedirection();

      app.UseStaticFiles();
      app.UseAntiforgery();

      if(authNEnabled)
         app.UseAuthentication();

      if(authNEnabled || authZEnabled)
         app.UseAuthorization();

      app.MapControllers();

      app.MapRazorComponents<App>()
         .AddInteractiveServerRenderMode();

      app.Run();
   }
}
