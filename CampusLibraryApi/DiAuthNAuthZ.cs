using System.Security.Claims;
using CampusLibraryApi._1_Web.Security;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._4_Infrastructure.Security;
using CampusLibraryApi.Configure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CampusLibraryApi;

/// <summary>
/// Central extension class for registering authentication and authorization
/// in the dependency injection container.
///
/// AuthN = Authentication = Who is the user?
/// AuthZ = Authorization  = What is the user allowed to do?
/// </summary>
public static class DiAuthNAuthZ {

   /// <summary>
   /// Registers JWT-based authentication and role-/policy-based authorization.
   ///
   /// This method is called in Program.cs:
   ///
   /// builder.Services.AddAuthNAuthZ(builder.Configuration);
   /// </summary>
   public static IServiceCollection AddAuthNAuthZ(
      this IServiceCollection services,
      IConfiguration config
   ) {
      // --------------------------------------------------------------------
      // Configure IdentityAccessServer options
      // --------------------------------------------------------------------
      // Reads the configuration section "IdentityAccessServer" from
      // appsettings.json or environment variables.
      //
      // Example:
      //
      // "IdentityAccessServer": {
      //   "Authority": "https://localhost:7010",
      //   "Audience": "campus-library-api",
      //   "RequireHttpsMetadata": false,
      //   "ValidateAudience": true,
      //   "ClockSkewSeconds": 60
      // }
      //
      // ValidateOnStart() ensures that invalid configuration is detected
      // when the application starts instead of during the first API request.
      services.AddOptions<AuthOptions>()
         .Bind(config.GetSection("IdentityAccessServer"))
         .Validate(
            options => !string.IsNullOrWhiteSpace(options.Authority),
            "IdentityAccessServer:Authority is required."
         )
         .Validate(
            options => !options.ValidateAudience ||
                       !string.IsNullOrWhiteSpace(options.Audience),
            "IdentityAccessServer:Audience is required when audience validation is enabled."
         )
         .Validate(
            options => options.ClockSkewSeconds >= 0,
            "IdentityAccessServer:ClockSkewSeconds must not be negative."
         )
         .ValidateOnStart();

      // --------------------------------------------------------------------
      // Read AuthOptions directly from configuration
      // --------------------------------------------------------------------
      // These values are required while JwtBearerOptions are registered.
      // If the section is missing, the application fails fast with a clear
      // error message.
      var iaServer = config
         .GetSection("IdentityAccessServer")
         .Get<AuthOptions>()
         ?? throw new InvalidOperationException(
            "Missing configuration section 'IdentityAccessServer'."
         );

      // --------------------------------------------------------------------
      // Diagnostic output for JWT configuration
      // --------------------------------------------------------------------
      // These messages make the effective authentication configuration
      // visible during development and in teaching demonstrations.
      Console.WriteLine($"JWT Bearer Authority: {iaServer.Authority}");
      Console.WriteLine($"JWT Bearer Audience: {iaServer.Audience}");
      Console.WriteLine($"JWT Bearer ValidateAudience: {iaServer.ValidateAudience}");
      Console.WriteLine($"JWT Bearer RequireHttpsMetadata: {iaServer.RequireHttpsMetadata}");
      Console.WriteLine($"JWT Bearer ClockSkewSeconds: {iaServer.ClockSkewSeconds}");

      // --------------------------------------------------------------------
      // Register the identity gateway adapter
      // --------------------------------------------------------------------
      // The application/core code uses IIdentityGateway instead of depending
      // directly on HttpContext, ClaimsPrincipal or JWT implementation details.
      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();

      // --------------------------------------------------------------------
      // AuthN: Register JWT Bearer authentication
      // --------------------------------------------------------------------
      // AddAuthentication defines the default authentication scheme.
      // JwtBearerDefaults.AuthenticationScheme is the "Bearer" scheme.
      // With this configuration, the API expects an HTTP header like this:
      //
      // Authorization: Bearer <access_token>
      services
         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

         // AddJwtBearer configures how incoming JWT access tokens are
         // validated. The API only accepts tokens issued by the configured
         // IdentityAccessServer and matching the validation rules below.
         .AddJwtBearer(options => {

            // Authority is the base address of the IdentityAccessServer.
            // JwtBearer uses it to retrieve OpenID Connect metadata and
            // signing keys.
            options.Authority = iaServer.Authority;

            // RequireHttpsMetadata defines whether metadata from the identity
            // provider must be loaded via HTTPS.
            options.RequireHttpsMetadata = iaServer.RequireHttpsMetadata;

            // Prevent Microsoft from automatically mapping JWT claim names to
            // legacy .NET URI claim types.
            //
            // This is the decisive configuration for transparent role checks:
            // the JWT claim "role" remains "role" in ClaimsPrincipal.
            // Therefore both variants use the same role information:
            //
            // [Authorize(Roles = "Employee")]
            // user.IsInRole("Employee")
            options.MapInboundClaims = false;

            // Audience identifies the API for which the access token was
            // issued. It is only assigned when configured.
            if (!string.IsNullOrWhiteSpace(iaServer.Audience))
               options.Audience = iaServer.Audience;

            // ----------------------------------------------------------------
            // TokenValidationParameters define how the JWT is validated.
            // ----------------------------------------------------------------
            options.TokenValidationParameters = new TokenValidationParameters {

               // The token must have been issued by the configured authority.
               ValidateIssuer = true,

               // Defines whether the audience claim must match CampusLibraryApi.
               ValidateAudience = iaServer.ValidateAudience,

               // Reject expired tokens.
               ValidateLifetime = true,

               // Validate the token signature with the IdentityAccessServer's
               // published signing keys.
               ValidateIssuerSigningKey = true,

               // ClockSkew allows a small time difference between the
               // IdentityAccessServer and the API server.
               ClockSkew = TimeSpan.FromSeconds(
                  iaServer.ClockSkewSeconds
               ),

               // Defines which claim is exposed as User.Identity.Name.
               NameClaimType = IdentityClaims.PreferredUsername,

               // Defines which JWT claim is used by User.IsInRole(...) and
               // [Authorize(Roles = "...")].
               //
               // Together with MapInboundClaims = false, the original JWT
               // claim "role" is used without hidden framework conversion.
               RoleClaimType = "role"
            };

            // ----------------------------------------------------------------
            // Events during JWT authentication
            // ----------------------------------------------------------------
            // These events provide useful diagnostics without changing the
            // authentication or authorization result.
            options.Events = new JwtBearerEvents {

               // Runs when the bearer token is read from the request.
               OnMessageReceived = context => {
                  var log = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");

                  if (!string.IsNullOrWhiteSpace(context.Token))
                     log.LogDebug(
                        "JWT bearer token received for {Path}",
                        context.HttpContext.Request.Path
                     );

                  return Task.CompletedTask;
               },

               // Runs after successful token validation.
               OnTokenValidated = context => {
                  var log = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");

                  var username = context.Principal?
                     .FindFirstValue(IdentityClaims.PreferredUsername)
                     ?? context.Principal?.Identity?.Name
                     ?? "(unknown)";

                  var roles = string.Join(
                     ",",
                     context.Principal?
                        .FindAll("role")
                        .Select(claim => claim.Value)
                     ?? Enumerable.Empty<string>()
                  );

                  log.LogInformation(
                     "JWT validated for {User}; roles={Roles}",
                     username,
                     roles
                  );

                  return Task.CompletedTask;
               },

               // Runs when token validation fails.
               // Possible reasons include an invalid signature, issuer,
               // audience or lifetime.
               OnAuthenticationFailed = context => {
                  var log = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");

                  log.LogError(
                     context.Exception,
                     "JWT authentication failed"
                  );

                  return Task.CompletedTask;
               },

               // Runs when an unauthenticated request reaches an endpoint
               // protected with [Authorize]. The usual HTTP result is 401.
               OnChallenge = context => {
                  var log = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");

                  log.LogWarning(
                     "JWT challenge: error={Error}, description={Description}",
                     context.Error,
                     context.ErrorDescription
                  );

                  return Task.CompletedTask;
               },

               // Runs when authentication succeeded but authorization failed.
               // The usual HTTP result is 403 Forbidden.
               OnForbidden = context => {
                  var log = context.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");

                  log.LogWarning(
                     "JWT forbidden for {User} on {Path}",
                     context.HttpContext.User.Identity?.Name ?? "(unknown)",
                     context.HttpContext.Request.Path
                  );

                  return Task.CompletedTask;
               }
            };
         });

      // --------------------------------------------------------------------
      // AuthZ: Register authorization policies
      // --------------------------------------------------------------------
      // Direct role checks and named policies are both supported.
      //
      // Direct role examples:
      //
      // [Authorize(Roles = "Reader")]
      // [Authorize(Roles = "Employee")]
      // [Authorize(Roles = "Reader,Employee")]
      //
      // Policy examples:
      //
      // [Authorize(Policy = CampusLibraryPolicies.ReadersOnly)]
      // [Authorize(Policy = CampusLibraryPolicies.EmployeesOnly)]
      // [Authorize(Policy = CampusLibraryPolicies.ReadersOrEmployees)]
      //
      // Both variants use User.IsInRole(...) internally and therefore rely on
      // exactly the same RoleClaimType = "role" configuration.
      services.AddAuthorization(options => {

         // ----------------------------------------------------------------
         // Policy: ReadersOnly
         // ----------------------------------------------------------------
         // Allows access only for authenticated users with role "Reader".
         options.AddPolicy(
            CampusLibraryPolicies.ReadersOnly,
            policy => policy.RequireAssertion(context =>
               IsReader(context.User)
            )
         );

         // ----------------------------------------------------------------
         // Policy: EmployeesOnly
         // ----------------------------------------------------------------
         // Allows access only for authenticated users with role "Employee".
         options.AddPolicy(
            CampusLibraryPolicies.EmployeesOnly,
            policy => policy.RequireAssertion(context =>
               IsEmployee(context.User)
            )
         );

         // ----------------------------------------------------------------
         // Policy: ReadersOrEmployees
         // ----------------------------------------------------------------
         // Allows access for authenticated Readers and Employees.
         options.AddPolicy(
            CampusLibraryPolicies.ReadersOrEmployees,
            policy => policy.RequireAssertion(context =>
               IsReader(context.User) || IsEmployee(context.User)
            )
         );
      });

      return services;
   }

   /// <summary>
   /// Checks whether the authenticated principal has role "Reader".
   /// </summary>
   private static bool IsReader(ClaimsPrincipal user) =>
      user.Identity?.IsAuthenticated == true &&
      user.IsInRole("Reader");

   /// <summary>
   /// Checks whether the authenticated principal has role "Employee".
   /// </summary>
   private static bool IsEmployee(ClaimsPrincipal user) =>
      user.Identity?.IsAuthenticated == true &&
      user.IsInRole("Employee");
}

/*
Lernziele und Didaktik
----------------------

Lernziele:
1. Die Lernenden verstehen den Unterschied zwischen Authentifizierung
   und Autorisierung.
   - Authentifizierung beantwortet die Frage: Wer ist der Benutzer?
   - Autorisierung beantwortet die Frage: Was darf dieser Benutzer?

2. Die Lernenden erkennen, wie eine ASP.NET-Core-API JWT Bearer Tokens
   validiert.
   Wichtige Begriffe sind:
   - Authority
   - Audience
   - TokenValidationParameters
   - Claims
   - Roles
   - Policies

3. Die Lernenden verstehen die Bedeutung von MapInboundClaims.
   Mit MapInboundClaims = false bleiben Claim-Namen wie "role" und
   "preferred_username" so sichtbar, wie sie im JWT stehen.

4. Die Lernenden können direkte Rollenprüfungen verwenden:
   - [Authorize(Roles = "Reader")]
   - [Authorize(Roles = "Employee")]

5. Die Lernenden können dieselben Rollen in benannten Policies kapseln:
   - ReadersOnly
   - EmployeesOnly
   - ReadersOrEmployees

Didaktischer Hinweis:
Direkte Rollenattribute sind für einfache Zugriffsregeln besonders
transparent. Policies sind sinnvoll, wenn eine Regel wiederverwendet,
benannt oder später um weitere Anforderungen ergänzt werden soll.

Beide Varianten verwenden in diesem Projekt bewusst dieselbe Rollenquelle:
den JWT-Claim "role". Es gibt keine zusätzliche Claim-Fallback-Logik, die
eine fehlerhafte Rollenabbildung verdecken könnte.
*/
