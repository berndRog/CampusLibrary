using System.Globalization;
using System.Security.Claims;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._4_Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace CampusLibraryApi._1_Web.Security;

// HTTP adapter for IIdentityGateway.
// Reads technical identity information from HttpContext.User claims.
public sealed class IdentityGatewayHttpContext(
   IHttpContextAccessor httpContextAccessor
) : IIdentityGateway {

   // Access to the current ClaimsPrincipal (may be null outside HTTP requests)
   private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

   // OIDC subject ("sub").
   public string Subject =>
      User?.FindFirstValue("sub")
      ?? User?.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? throw new InvalidOperationException("Missing claim: sub");

   // Preferred Username interpreted as initial Email provided by the IdP.
   public string Username =>
      User?.FindFirstValue(IdentityClaims.PreferredUsername)
      ?? throw new InvalidOperationException("Missing claim: preferred_username");

   // Optional creation timestamp of the identity.
   public DateTime CreatedAt {
      get {
         var value = User?.FindFirstValue(IdentityClaims.CreatedAt);

         if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Missing claim: created_at");

         if (!DateTimeOffset.TryParse(
                input: value,
                formatProvider: CultureInfo.InvariantCulture,
                styles: DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                result: out var createdAt
             ))
            throw new InvalidOperationException("Invalid claim: created_at");

         return createdAt.UtcDateTime;
      }
   }
   
   // Bitmask defining administrative rights.
   public int AdminRights =>
      int.TryParse(User?.FindFirstValue(IdentityClaims.AdminRights), out var adminRights)
         ? adminRights
         : 0;
   
   // Is the user authenticated
   public bool IsAuthenticated =>
      User?.Identity?.IsAuthenticated == true;
   
   public bool IsReader =>
      HasRole("Reader") ||
      HasRole("reader");

   public bool IsEmployee =>
      HasRole("Employee") ||
      HasRole("employee");
   
   private bool HasRole(string role) =>
      User?.IsInRole(role) == true ||
      User?.Claims.Any(claim =>
         IsRoleClaim(claim.Type) &&
         string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase)
      ) == true;
   
   private static bool IsRoleClaim(string claimType) =>
      claimType == ClaimTypes.Role ||
      claimType == "role" ||
      claimType == "roles";
}

/*
Didaktik
--------

IdentityGatewayHttpContext ist ein Adapter der Web-Schicht.

Der Adapter liest Claims aus dem aktuellen HTTP-Kontext und übersetzt sie in
fachlich nutzbare Eigenschaften wie Subject, Email, IsReader und IsEmployee.
Die Anwendungsschicht kennt dadurch weder HttpContext noch ClaimTypes noch JWT.

Für die Lehre ist wichtig:

- subject ist die stabile technische Benutzeridentität.
- email ist ein technischer Claim aus dem IdentityAccessServer.
- Rollen werden aus Claims gelesen, nicht aus Formularfeldern.

Die Implementierung akzeptiert sowohl "Reader" als auch "student". Dadurch
kann der IdentityAccessServer didaktisch zunächst mit student arbeiten, während
die CampusLibrary fachlich von Reader spricht.

Lernziele
---------

- Adapter für technische Framework-Abhängigkeiten erkennen
- Claims aus dem Token in einen kleinen Anwendungskontext übersetzen
- Core-Code unabhängig von ASP.NET Core halten
*/
