using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CampusLibraryClient.Security;

public sealed class ClaimsCurrentUserProvider(
   AuthenticationStateProvider authenticationStateProvider
) : ICurrentUserProvider {

   public async Task<CurrentUserInfo> GetCurrentUserAsync(
      CancellationToken ct = default
   ) {
      AuthenticationState authState =
         await authenticationStateProvider.GetAuthenticationStateAsync();

      ClaimsPrincipal principal = authState.User;

      if(principal.Identity?.IsAuthenticated != true)
         return CurrentUserInfo.Anonymous;

      string role = NormalizeRole(
         FindFirstValue(
            principal: principal,
            claimTypes: [
               ClaimTypes.Role,
               "role",
               "roles"
            ]
         ) ?? "authenticated"
      );

      string displayName =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               "name",
               "preferred_username",
               ClaimTypes.Name,
               ClaimTypes.Email,
               "email"
            ]
         ) ?? "Authenticated user";

      string? email =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               ClaimTypes.Email,
               "email",
               "preferred_username"
            ]
         );

      return new CurrentUserInfo(
         IsAuthenticated: true,
         AccountType: role,
         ReaderId: null,
         DisplayName: displayName,
         Email: email
      );
   }

   private static string NormalizeRole(
      string value
   ) => value.Trim().ToLowerInvariant() switch {
      "reader" => CampusLibraryRoles.Reader,
      "employee" => CampusLibraryRoles.Employee,
      _ => value
   };

   private static string? FindFirstValue(
      ClaimsPrincipal principal,
      IEnumerable<string> claimTypes
   ) {
      foreach(string claimType in claimTypes) {
         string? value = principal.FindFirst(claimType)?.Value;

         if(!string.IsNullOrWhiteSpace(value))
            return value;
      }

      return null;
   }
}

/*
Didaktik
--------

ClaimsCurrentUserProvider liest die echte OIDC-Identität aus dem
ClaimsPrincipal des Blazor-SSR-Clients.

Die UI-Autorisierung basiert ausschließlich auf dem role-Claim. Der technische
account_type-Claim ist dafür nicht erforderlich.

Die fachliche ReaderId kommt nicht aus dem Token, sondern aus dem
CampusLibrary-Provisioning über GET /readers/me.
*/
