using System.Security.Claims;
using CampusLibraryClient.Api.Contracts;
using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;
using CampusLibraryClient.Security;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryClient.Ui.Controllers;

// Post-login landing route.
// Part 6: Reader users are provisioned here before they enter the UI.
[Route("entry")]
public sealed class EntryController(
   IConfiguration configuration,
   IReaderClient readerClient,
   ILogger<EntryController> logger
) : Controller {

   [HttpGet]
   public async Task<IActionResult> Index(
      CancellationToken ct
   ) {
      logger.LogInformation(
         "Post-login entry reached: authenticated={Authenticated}, sub={Subject}, username={Username}, role={Role}",
         User.Identity?.IsAuthenticated == true,
         FindFirstValue(User, ClaimTypes.NameIdentifier, "sub"),
         FindFirstValue(User, "preferred_username", ClaimTypes.Name, ClaimTypes.Email, "email"),
         FindFirstValue(User, ClaimTypes.Role, "role", "roles")
      );

      if(configuration.GetValue<bool>(FeatureFlags.AuthNEnabled) &&
         User.Identity?.IsAuthenticated != true) {
         logger.LogWarning(
            "Post-login entry has no authenticated cookie; starting a new OIDC challenge."
         );

         return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
      }

      if(IsReader(User)) {
         Result<ReaderProvisionMeDto> provisionResult =
            await readerClient.ProvisionMeAsync(
               ct: ct
            );

         if(provisionResult.IsFailure) {
            logger.LogWarning(
               "Reader provisioning failed after login: {Title} - {Detail}",
               provisionResult.Error?.Title,
               provisionResult.Error?.Detail
            );

            // The profile page repeats the idempotent provisioning call and
            // displays a possible API error to the user.
            return Redirect("/readers/profile");
         }

         Result<ReaderDto> meResult = await readerClient.GetMeAsync(
            ct: ct
         );

         if(meResult.IsFailure) {
            logger.LogWarning(
               "Reader profile lookup failed after provisioning: {Title} - {Detail}",
               meResult.Error?.Title,
               meResult.Error?.Detail
            );

            return Redirect("/readers/profile");
         }

         if(meResult.Value?.IsProfileCompleted != true)
            return Redirect("/readers/profile");

         return Redirect("/catalog/books");
      }

      if(IsEmployee(User))
         return Redirect("/catalog/books");

      logger.LogWarning(
         "Authenticated post-login user has no supported CampusLibrary role. " +
         "sub={Subject}, role={Role}, claims=[{Claims}]",
         FindFirstValue(User, ClaimTypes.NameIdentifier, "sub"),
         FindFirstValue(User, ClaimTypes.Role, "role", "roles"),
         string.Join(
            separator: ", ",
            values: User.Claims.Select(claim => $"{claim.Type}={claim.Value}")
         )
      );

      return Redirect("/access-denied");
   }

   private static bool IsReader(
      ClaimsPrincipal user
   ) => HasRole(
      user: user,
      role: CampusLibraryRoles.Reader
   );

   private static bool IsEmployee(
      ClaimsPrincipal user
   ) => HasRole(
      user: user,
      role: CampusLibraryRoles.Employee
   );

   private static bool HasRole(
      ClaimsPrincipal user,
      string role
   ) =>
      user.IsInRole(role) ||
      user.Claims.Any(claim =>
         IsRoleClaim(claim.Type) &&
         string.Equals(
            a: claim.Value,
            b: role,
            comparisonType: StringComparison.OrdinalIgnoreCase
         )
      );

   private static string? FindFirstValue(
      ClaimsPrincipal user,
      params string[] claimTypes
   ) {
      foreach(string claimType in claimTypes) {
         string? value = user.FindFirst(claimType)?.Value;

         if(!string.IsNullOrWhiteSpace(value))
            return value;
      }

      return null;
   }

   private static bool IsRoleClaim(
      string claimType
   ) =>
      claimType == ClaimTypes.Role ||
      claimType == "role" ||
      claimType == "roles";
}

/*
Didaktik
--------

EntryController ist der Übergang vom technischen Login zur fachlichen
CampusLibrary-Anwendung.

Nach erfolgreichem OIDC-Login wird für Reader nicht direkt in die Anwendung
verzweigt. Zuerst wird der fachliche Reader über die API provisioniert. Danach
entscheidet GET /readers/me, ob das Profil schon vollständig ist oder ob die
Profilseite angezeigt werden muss.

Die Entscheidung Reader oder Employee basiert ausschließlich auf Rollen.
account_type wird im Client nicht als Autorisierungsmerkmal verwendet.
*/
