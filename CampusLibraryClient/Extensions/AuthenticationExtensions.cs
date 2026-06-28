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
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
         })
         .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
            options.LoginPath = "/identity/login";
            options.LogoutPath = "/identity/logout";
            options.AccessDeniedPath = "/access-denied";
         })
         .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.Authority = section.GetValue<string>("Authority")
               ?? throw new InvalidOperationException("IdentityAccessServer:Authority is missing.");

            options.ClientId = section.GetValue<string>("ClientId")
               ?? throw new InvalidOperationException("IdentityAccessServer:ClientId is missing.");

            options.ClientSecret = section.GetValue<string>("ClientSecret");
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            string[] scopes = section.GetSection("Scopes").Get<string[]>()
               ?? ["openid", "profile", "email"];

            foreach(string scope in scopes)
               options.Scope.Add(scope);

            // Keep claim names predictable for the Blazor client.
            options.TokenValidationParameters = new TokenValidationParameters {
               NameClaimType = "preferred_username",
               RoleClaimType = ClaimTypes.Role
            };

            options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
            options.ClaimActions.MapUniqueJsonKey(ClaimTypes.NameIdentifier, "sub");
            options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "email");
            options.ClaimActions.MapJsonKey(ClaimTypes.Role, "role");
            options.ClaimActions.MapJsonKey(ClaimTypes.Role, "roles");
         });

      return services;
   }
}
