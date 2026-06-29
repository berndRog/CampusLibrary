using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityAccessServer.Auth.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityAccessServer.Auth.Dev;

[ApiController]
[Route("dev")]
public sealed class DevTokenController(
   IWebHostEnvironment env
) : Controller {

   /// <summary>
   /// Deprecated Development-only endpoint.
   ///
   /// Token issuance now happens via the OpenIddict token endpoint using
   /// grant_type=dev_password. This shim exists only to guide callers.
   /// </summary>
   [AllowAnonymous]
   [HttpPost("token")]
   public IActionResult Token() {
      if (!env.IsDevelopment())
         return NotFound();

      return Problem(
         title: "Deprecated development endpoint",
         detail:
            $"Use POST /{IdentityAccessServerOptions.TokenEndpointPath} with Content-Type 'application/x-www-form-urlencoded', " +
            $"grant_type={DevGrantTypes.DevPassword}, client_id=dev-token-client, email=<user>, password=<password> and scope=<scopes>.",
         statusCode: StatusCodes.Status410Gone,
         extensions: new Dictionary<string, object?> {
            ["token_endpoint"] = "/" + IdentityAccessServerOptions.TokenEndpointPath,
            [Parameters.GrantType] = DevGrantTypes.DevPassword,
            [Parameters.ClientId] = "dev-token-client",
            ["example"] =
               $"grant_type={DevGrantTypes.DevPassword}&client_id=dev-token-client&email=admin@mail.local&password=Geh1m_&scope=openid profile banking-api"
         }
      );
   }
}

/*
==========================================================
DIDAKTIK / LERNZIELE (DE)
==========================================================

1) Warum gibt es diesen Controller?
----------------------------------
Dieser Controller dient ausschließlich der Entwicklung und dem Testen.
Er erlaubt das Ausstellen von Access Tokens ohne:
- Browser Redirects
- OIDC Authorization Code Flow
- Login UI

Das beschleunigt:
- API-Tests
- Mobile-Client-Entwicklung
- Postman / curl / Integrationstests

2) Was ist der zentrale Lernpunkt hier?
---------------------------------------
"scope" und "resource/audience" sind NICHT dasselbe:

- Scope  (z. B. "carrental_api")  = Berechtigung / Capability (was darf der Client?)
- Resource (z. B. "carrental-api") = Ziel-API / Audience (für wen ist das Token gedacht?)

Im Resource Server wird typischerweise die Audience geprüft.

3) Warum ApiKey im DTO?
-----------------------
Damit man beim Testen gezielt Tokens für verschiedene APIs ausstellen kann:
- BankingApi, ImagesApi

So bleibt das Setup skalierbar, ohne Codeänderungen in diesem Controller.
Die Wahrheit steht in appsettings.json:
IdentityAccessServer:Apis:{Key}:{Scope,Resource}

4) Warum SignInManager + ClaimsPrincipal?
-----------------------------------------
Auch im Dev-Modus:
- wird das echte Identity-System genutzt
- entstehen realistische Claims
- bleiben Tokens kompatibel mit dem echten OIDC-Flow

Kein Mocking, kein Sonderformat.

5) Warum zentrale ClaimDestinations?
------------------------------------
OpenIddict verlangt explizit:
- welche Claims im Access Token landen
- welche im ID Token landen

Durch die zentrale Klasse:
- kein Copy & Paste
- identisches Verhalten in
  - /connect/authorize
  - /connect/token
  - /dev/token

6) Wichtige Regel
-----------------
Dieser Controller darf:
- niemals in Production aktiv sein
- niemals echte Clients ersetzen
- nur Entwicklung beschleunigen
==========================================================
*/
