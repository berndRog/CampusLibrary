using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CampusLibraryClient.Extensions;

public static class AuthenticationExtensions {

   public static IServiceCollection ConfigureAuthN(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      IConfigurationSection section = configuration.GetRequiredSection("IdentityAccessServer");

      services
         .AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
         })
         .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
            options.LoginPath = "/identity/login";
            options.LogoutPath = "/identity/logout";
            options.AccessDeniedPath = "/access-denied";
         })
         .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            options.Authority = section.GetValue<string>("Authority")
               ?? throw new InvalidOperationException("IdentityAccessServer:Authority is missing.");

            options.ClientId = section.GetValue<string>("ClientId")
               ?? throw new InvalidOperationException("IdentityAccessServer:ClientId is missing.");

            options.ClientSecret = section.GetValue<string>("ClientSecret");
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            string[] scopes = section.GetSection("Scopes").Get<string[]>()
               ?? ["openid", "profile", "email"];

            foreach(string scope in scopes)
               options.Scope.Add(scope);

            // The client authorizes exclusively with the role claim.
            // Keep the standard .NET role claim representation so that
            // User.IsInRole(...) and [Authorize(Roles = ...)] use the same claim.
            options.TokenValidationParameters = new TokenValidationParameters {
               NameClaimType = "preferred_username",
               RoleClaimType = ClaimTypes.Role
            };

            options.ClaimActions.MapUniqueJsonKey(
               "preferred_username",
               "preferred_username"
            );
            options.ClaimActions.MapUniqueJsonKey(
               ClaimTypes.NameIdentifier,
               "sub"
            );
            options.ClaimActions.MapUniqueJsonKey(
               ClaimTypes.Email,
               "email"
            );
            options.ClaimActions.MapJsonKey(
               ClaimTypes.Role,
               "role"
            );
            options.ClaimActions.MapJsonKey(
               ClaimTypes.Role,
               "roles"
            );

            options.Events = new OpenIdConnectEvents {
               OnTicketReceived = context => {
                  ILogger logger = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("CampusLibraryClient.Oidc");

                  logger.LogInformation(
                     "OIDC ticket received for {Username}. Redirecting through /entry.",
                     context.Principal?.Identity?.Name
                  );

                  // Every successful technical login must pass through the
                  // fachliche entry flow. The entry controller provisions a
                  // Reader and redirects an incomplete profile to the profile page.
                  context.ReturnUri = "/entry";

                  return Task.CompletedTask;
               }
            };
         });

      return services;
   }
}
