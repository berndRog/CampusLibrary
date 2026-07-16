using CampusLibraryClient.Api.Auth;
using CampusLibraryClient.Core;
using CampusLibraryClient.Extensions;
using CampusLibrary.Shared.Logging;
using CampusLibraryClient.Security;

namespace CampusLibraryClient;

public static class Program {

   public static void Main(string[] args) {

      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      bool authNEnabled = builder.Configuration.GetValue<bool>(FeatureFlags.AuthNEnabled);
      bool devIdentityEnabled = builder.Configuration.GetValue<bool>(FeatureFlags.DevIdentityEnabled);
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
      }

      if(authNEnabled || authZEnabled) {
         // Part 6: role-based authorization for routable Blazor pages.
         // Named client policies are intentionally not used.
         builder.Services.ConfigureAuthZ();
      }

      if(apiAccessTokenEnabled) {
         // Part 6: forwards the current user's access token for the protected /readers/me flow.
         // Part 7 will harden all API client calls systematically.
         builder.Services.AddTransient<AccessTokenHandler>();
      }



      if(authNEnabled) {
         // Part 6: the current user comes from the authenticated ClaimsPrincipal.
         builder.Services.AddScoped<ICurrentUserProvider, ClaimsCurrentUserProvider>();
      }
      else if(devIdentityEnabled) {
         // Part 5: a demo identity can simulate reader or employee UI flows.
         builder.Services.AddScoped<ICurrentUserProvider, DevCurrentUserProvider>();
      }
      else {
         builder.Services.AddScoped<ICurrentUserProvider, AnonymousCurrentUserProvider>();
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
