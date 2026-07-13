using System.Security.Claims;
using IdentityAccessServer.Auth.Claims;
using IdentityAccessServer.Auth.Dev;
using IdentityAccessServer.Auth.Options;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityAccessServer.Auth.Controllers;

[ApiController]
public sealed class OidcController(
   UserManager<ApplicationUser> users,
   SignInManager<ApplicationUser> signIn,
   IOptions<IdentityAccessServerOptions> authOptions,
   IWebHostEnvironment env,
   ILogger<OidcController> logger
) : Controller {
   private readonly IdentityAccessServerOptions _auth = authOptions.Value;

   
   #region /connect/authorize --------------------------------------------------------
   [HttpGet("/" + IdentityAccessServerOptions.AuthorizationEndpointPath)]
   public async Task<IActionResult> Authorize(CancellationToken ct) {
      var request = HttpContext.GetOpenIddictServerRequest()
         ?? throw new InvalidOperationException("OpenID Connect request missing.");

      logger.LogInformation(
         "Authorize request: client_id='{ClientId}', redirect_uri='{RedirectUri}', " +
         "scope='{Scope}', response_type='{ResponseType}'",
         request.ClientId, request.RedirectUri, request.Scope, request.ResponseType
      );

      var returnUrl = Request.PathBase + Request.Path + Request.QueryString;

      var authResult = 
         await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

      if (!authResult.Succeeded) {
         logger.LogInformation("Authorize: no Identity cookie -> challenge, returnUrl='{ReturnUrl}'", returnUrl);

         return Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            IdentityConstants.ApplicationScheme
         );
      }

      var user = await users.GetUserAsync(authResult.Principal!);
      if (user is null) {
         logger.LogWarning(
            "Authorize: Identity cookie principal has no user -> challenge, returnUrl='{ReturnUrl}'",
            returnUrl
         );

         return Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            IdentityConstants.ApplicationScheme
         );
      }

      var tokenContext = await CreateTokenContextAsync(user, request.GetScopes());
      
      logger.LogInformation(
         "Authorize: user='{UserName}', sub='{Sub}', role='{Role}', adminRights='{Rights}', scopes=[{Scopes}], resources=[{Resources}]",
         user.UserName,
         tokenContext.Subject,
         tokenContext.Role,
         user.AdminRights,
         string.Join(", ", tokenContext.RequestedScopes),
         tokenContext.Resources.Length == 0 ? "<none>" : string.Join(", ", tokenContext.Resources)
      );

      foreach (var c in tokenContext.Principal.Claims)
         logger.LogDebug("Authorize: claim '{Type}'='{Value}' -> destinations: {Destinations}",
            c.Type, c.Value, string.Join(", ", c.GetDestinations()));

      return SignIn(tokenContext.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
   }

   private static void SetOrReplaceClaim(ClaimsIdentity identity, string type, string value) {
      var existing = identity.Claims.Where(c => c.Type == type).ToList();
      foreach (var c in existing)
         identity.RemoveClaim(c);

      identity.AddClaim(new Claim(type, value));
   }
   #endregion
   
   #region /connect/token ------------------------------------------------------------
   [HttpPost("/" + IdentityAccessServerOptions.TokenEndpointPath)]
   public async Task<IActionResult> Token(CancellationToken ct) {
      var request = HttpContext.GetOpenIddictServerRequest()
         ?? throw new InvalidOperationException("OpenID Connect request missing.");

      logger.LogInformation(
         "Token request: grant_type='{GrantType}', client_id='{ClientId}', scope='{Scope}'",
         request.GrantType, request.ClientId, request.Scope
      );

      if (string.Equals(request.GrantType, DevGrantTypes.DevPassword, StringComparison.Ordinal)) {
         if (!env.IsDevelopment())
            return OpenIddictError(Errors.UnauthorizedClient,
               "The dev_password grant is only available in Development.");

         var email = request.GetParameter("email")?.ToString()
                     ?? request.GetParameter(Parameters.Username)?.ToString();
         var password = request.GetParameter(Parameters.Password)?.ToString();

         if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return OpenIddictError(Errors.InvalidRequest,
               "Parameters 'email' (or 'username') and 'password' are required.");

         var user = await users.FindByEmailAsync(email);
         if (user is null || !await users.CheckPasswordAsync(user, password))
            return OpenIddictError(Errors.InvalidGrant, "Invalid email or password.");

         var tokenContext = await CreateTokenContextAsync(user, request.GetScopes());

         logger.LogInformation(
            "Token(dev_password): user='{UserName}', sub='{Sub}', role='{Role}', scopes=[{Scopes}], resources=[{Resources}]",
            user.UserName,
            tokenContext.Subject,
            tokenContext.Role,
            string.Join(", ", tokenContext.RequestedScopes),
            tokenContext.Resources.Length == 0 ? "<none>" : string.Join(", ", tokenContext.Resources)
         );

         return SignIn(tokenContext.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType()) {
         var result = await HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
         );

         logger.LogInformation("Token: code/refresh -> issuing tokens for client_id='{ClientId}'", request.ClientId);

         return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      if (request.IsClientCredentialsGrantType()) {
         logger.LogInformation(
            "Token: client_credentials -> issuing access token for client_id='{ClientId}'",
            request.ClientId
         );

         var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

         identity.AddClaim(new Claim(AuthClaims.Subject, request.ClientId!));
         identity.AddClaim(new Claim(AuthClaims.AccountType, "service"));

         var principal = new ClaimsPrincipal(identity);

         var requestedScopes = request.GetScopes().ToArray();
         principal.SetScopes(requestedScopes);

         var resources = ResolveResourcesFromScopes(requestedScopes);
         if (resources.Length > 0)
            principal.SetResources(resources);

         foreach (var claim in principal.Claims)
            claim.SetDestinations(Destinations.AccessToken);

         logger.LogInformation(
            "Token: client_credentials -> client_id='{ClientId}', scopes=[{Scopes}], resources=[{Resources}]",
            request.ClientId,
            string.Join(", ", requestedScopes),
            resources.Length == 0 ? "<none>" : string.Join(", ", resources)
         );

         return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      logger.LogWarning("Token: unsupported grant_type '{GrantType}'", request.GrantType);
      return BadRequest(new { error = "unsupported_grant_type" });
   }
   #endregion
   
   #region /connect/userinfo ---------------------------------------------------------
   [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
   [HttpGet("/" + IdentityAccessServerOptions.UserInfoEndpointPath)]
   public IActionResult UserInfo() {
      logger.LogInformation(
         "UserInfo request: sub='{Sub}', azp='{Azp}'",
         User.FindFirst(AuthClaims.Subject)?.Value,
         User.FindFirst("azp")?.Value
      );

      return Ok(new {
         sub = User.FindFirst(AuthClaims.Subject)?.Value,
         preferred_username = User.FindFirst(AuthClaims.PreferredUsername)?.Value,
         email = User.FindFirst(AuthClaims.Email)?.Value,
         role = User.FindFirst(AuthClaims.Role)?.Value,
         account_type = User.FindFirst(AuthClaims.AccountType)?.Value,
         admin_rights = User.FindFirst(AuthClaims.AdminRights)?.Value,
         created_at = User.FindFirst(AuthClaims.CreatedAt)?.Value,
         updated_at = User.FindFirst(AuthClaims.UpdatedAt)?.Value
      });
   }
   #endregion
   
   #region Helpers -------------------------------------------------------------------
   private string[] ResolveResourcesFromScopes(string[] requestedScopes) {
      static bool IsNonApiScope(string s)
         => s.Equals("openid", StringComparison.Ordinal) ||
            s.Equals("profile", StringComparison.Ordinal);

      var apiScopesRequested = requestedScopes
         .Where(s => !IsNonApiScope(s))
         .Distinct(StringComparer.Ordinal)
         .ToArray();

      if (apiScopesRequested.Length == 0)
         return Array.Empty<string>();

      var known = _auth.Apis.Values.ToDictionary(a => a.Scope, a => a.Resource, StringComparer.Ordinal);

      var resources = new List<string>(capacity: apiScopesRequested.Length);

      foreach (var scope in apiScopesRequested) {
         if (known.TryGetValue(scope, out var resource)) {
            resources.Add(resource);
         }
         else {
            logger.LogWarning(
               "Unknown API scope requested: '{Scope}'. No resource/audience mapping found in IdentityAccessServer:Apis.",
               scope
            );
         }
      }

      return resources
         .Distinct(StringComparer.Ordinal)
         .ToArray();
   }

   private async Task<TokenContext> CreateTokenContextAsync(
      ApplicationUser user,
      IEnumerable<string> requestedScopes
   ) {
      var principal = await signIn.CreateUserPrincipalAsync(user);
      var identity = (ClaimsIdentity)principal.Identity!;

      var subject = user.Id;
      SetOrReplaceClaim(identity, AuthClaims.Subject, subject);

      if (!string.IsNullOrWhiteSpace(user.Email))
         SetOrReplaceClaim(identity, AuthClaims.Email, user.Email);

      if (!string.IsNullOrWhiteSpace(user.UserName))
         SetOrReplaceClaim(identity, AuthClaims.PreferredUsername, user.UserName);

      var accountType = user.AccountType.Trim().ToLowerInvariant();
      if (accountType == "student")
         accountType = "reader";

      if (user.AdminRights > 0)
         accountType = "employee";

      SetOrReplaceClaim(identity, AuthClaims.AccountType, accountType);

      var role = accountType switch {
         "employee" => "Employee",
         "reader" => "Reader",
         "customer" => "Customer",
         _ => "Customer"
      };
      SetOrReplaceClaim(identity, AuthClaims.Role, role);
      SetOrReplaceClaim(identity, AuthClaims.AdminRights, ((int)user.AdminRights).ToString());
      SetOrReplaceClaim(identity, AuthClaims.MustChangePassword, user.MustChangePassword ? "true" : "false");

      if (user.CreatedAt != default)
         SetOrReplaceClaim(identity, AuthClaims.CreatedAt, user.CreatedAt.ToUniversalTime().ToString("O"));

      if (user.UpdatedAt != default)
         SetOrReplaceClaim(identity, AuthClaims.UpdatedAt, user.UpdatedAt.ToUniversalTime().ToString("O"));

      var scopes = requestedScopes
         .Where(s => !string.IsNullOrWhiteSpace(s))
         .Distinct(StringComparer.Ordinal)
         .ToArray();

      principal.SetScopes(scopes);

      var resources = ResolveResourcesFromScopes(scopes);
      if (resources.Length > 0)
         principal.SetResources(resources);

      foreach (var claim in principal.Claims)
         claim.SetDestinations(ClaimDestinations.GetDestinations(claim, principal));

      return new TokenContext(principal, subject, role, scopes, resources);
   }

   private IActionResult OpenIddictError(string error, string description)
      => Forbid(
         new AuthenticationProperties(new Dictionary<string, string?> {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
         }),
         OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
      );

   private sealed record TokenContext(
      ClaimsPrincipal Principal,
      string Subject,
      string Role,
      string[] RequestedScopes,
      string[] Resources
   );
   #endregion
}
