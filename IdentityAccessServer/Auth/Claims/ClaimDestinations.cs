using System.Security.Claims;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityAccessServer.Auth.Claims;

/// <summary>
/// Central mapping that controls which claims go into which token.
/// - AccessToken: for APIs (authorization, domain checks)
/// - IdentityToken: for clients (UI display, basic identity info)
/// </summary>
public static class ClaimDestinations {
   public static IEnumerable<string> GetDestinations(
      Claim claim,
      ClaimsPrincipal principal
   ) {
      //--- IdentityToken + AccessToken ----------------------------------------
      // Mandatory OIDC subject
      if (claim.Type == AuthClaims.Subject)
         return new[] { Destinations.IdentityToken, Destinations.AccessToken };

      // email:
      // - useful for clients when the email scope was requested
      // - useful for APIs as technical username/email context
      if (claim.Type == AuthClaims.Email)
         return principal.HasScope(Scopes.Email)
            ? new[] { Destinations.IdentityToken, Destinations.AccessToken }
            : new[] { Destinations.AccessToken };

      // preferred_username:
      // - always in access token for APIs
      // - only in identity token when the client requested profile scope
      if (claim.Type == AuthClaims.PreferredUsername)
         return principal.HasScope(Scopes.Profile)
            ? new[] { Destinations.IdentityToken, Destinations.AccessToken }
            : new[] { Destinations.AccessToken };

      // role -> UI navigation in the client and authorization in APIs.
      if (claim.Type == AuthClaims.Role)
         return new[] { Destinations.AccessToken, Destinations.IdentityToken };

      // account_type is needed by the SSR client as a stable fallback when it
      // decides whether the post-login Reader provisioning flow must start.
      if (claim.Type == AuthClaims.AccountType)
         return new[] { Destinations.AccessToken, Destinations.IdentityToken };

      if (claim.Type == AuthClaims.MustChangePassword)
         return new[] { Destinations.AccessToken, Destinations.IdentityToken };

      // Lifecycle / housekeeping (debuggable in id_token, usable in API)
      if (claim.Type is AuthClaims.CreatedAt or AuthClaims.UpdatedAt)
         return new[] { Destinations.AccessToken, Destinations.IdentityToken };

      //--- AccessToken only ---------------------------------------------------
      // Domain-specific claims → access token only
      if (claim.Type == AuthClaims.AdminRights)
         return new[] { Destinations.AccessToken };

      // Everything else is excluded by default
      return Array.Empty<string>();
   }
}
/*
(Didaktik & Lernziele)
-----------------------------------------------------------------------
Ziel:
   - Studierende verstehen, dass Claims nicht "automatisch" in Tokens landen,
      sondern bewusst pro Token-Typ zugewiesen werden (Destinations).

   Merksätze:
1) Access Token = für APIs (Autorisierung, fachliche Checks)
2) ID Token     = für Clients/UI (Anzeige, Login-Kontext)
3) Client-relevante Identität:
   - Email und preferred_username werden nur für passende Scopes ausgegeben.
   - role und account_type stehen dem SSR-Client im ID Token zur Verfügung.
4) AdminRights gehört NICHT in den ID Token:
   - UI kann es aus dem Access Token / API ableiten,
- verhindert unnötige Daten im Browser-Token.
-----------------------------------------------------------------------
*/
