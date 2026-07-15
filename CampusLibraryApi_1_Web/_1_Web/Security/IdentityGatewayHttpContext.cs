using System.Globalization;
using System.Security.Claims;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._4_Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace CampusLibraryApi._1_Web.Security;

// HTTP adapter for IIdentityGateway.
// Reads technical identity information from HttpContext.User claims.
public sealed class FakeIdentityGateway(
//   IHttpContextAccessor httpContextAccessor
) : IIdentityGateway {

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

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; }
   public string Username { get; }
   public DateTime CreatedAt { get; }
   public int AdminRights { get; }

   public FakeIdentityGateway(
      string subject,
      string username,
      DateTime createdAt,
      int? adminRights = null
   ) {
      Subject = subject;
      Username = username;
      CreatedAt = createdAt;
      if (adminRights.HasValue)
         AdminRights = adminRights.Value;
   }
}