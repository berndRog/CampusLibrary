using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CampusLibraryApiTest.TestController;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
   public const string SchemeName = "TestScheme";

   // e.g. "Reader" or "Employee" or "Reader,Employee"
   public const string RolesHeader = "X-Test-Roles";
   public const string Header = RolesHeader;

   public const string SubjectHeader = "X-Test-Subject";
   public const string UsernameHeader = "X-Test-Username";
   public const string CreatedAtHeader = "X-Test-CreatedAt";

   public TestAuthHandler(
      IOptionsMonitor<AuthenticationSchemeOptions> options,
      ILoggerFactory logger,
      UrlEncoder encoder
   ) : base(options, logger, encoder) {
   }

   protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
      if (!Request.Headers.TryGetValue(RolesHeader, out var rolesRaw))
         return Task.FromResult(AuthenticateResult.NoResult());

      var roles = rolesRaw.ToString()
         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      var subject = Request.Headers.TryGetValue(SubjectHeader, out var subjectRaw)
         ? subjectRaw.ToString()
         : "part6-reader-subject-e2e";

      var username = Request.Headers.TryGetValue(UsernameHeader, out var usernameRaw)
         ? usernameRaw.ToString()
         : "reader.e2e@example.org";

      var createdAt = Request.Headers.TryGetValue(CreatedAtHeader, out var createdAtRaw)
         ? createdAtRaw.ToString()
         : "2025-01-01T00:00:00Z";

      var claims = new List<Claim> {
         new("sub", subject),
         new(ClaimTypes.NameIdentifier, subject),
         new("preferred_username", username),
         new(ClaimTypes.Name, username),
         new("created_at", createdAt),
         new("admin_rights", "0")
      };

      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
      claims.AddRange(roles.Select(role => new Claim("role", role)));

      var identity = new ClaimsIdentity(claims, Scheme.Name);
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, Scheme.Name);

      return Task.FromResult(AuthenticateResult.Success(ticket));
   }
}
