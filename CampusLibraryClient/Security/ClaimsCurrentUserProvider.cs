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

      string accountType =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               "account_type",
               "accountType",
               ClaimTypes.Role,
               "role",
               "roles"
            ]
         ) ?? "authenticated";

      Guid? readerId = null;
      string? readerIdText =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               "reader_id",
               "readerId",
               "ReaderId"
            ]
         );

      if(Guid.TryParse(
            input: readerIdText,
            result: out Guid parsedReaderId
         ))
         readerId = parsedReaderId;

      string displayName =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               "name",
               "preferred_username",
               ClaimTypes.Name,
               ClaimTypes.Email
            ]
         ) ?? "Authenticated user";

      string? email =
         FindFirstValue(
            principal: principal,
            claimTypes: [
               ClaimTypes.Email,
               "email"
            ]
         );

      return new CurrentUserInfo(
         IsAuthenticated: true,
         AccountType: accountType,
         ReaderId: readerId,
         DisplayName: displayName,
         Email: email
      );
   }

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

ClaimsCurrentUserProvider ist die spätere Part-6-Quelle für die aktuelle
Benutzeridentität.

Die Seiten fragen weiterhin nur ICurrentUserProvider ab. Dadurch kann Part 5
mit einer Demo-Identität arbeiten und Part 6 dieselben Seiten mit echter
OIDC-Authentifizierung verwenden.
*/
