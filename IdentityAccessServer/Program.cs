using IdentityAccessServer.Auth.Options;
using IdentityAccessServer.Auth.Seeding;
using IdentityAccessServer.Auth.Dev;
using IdentityAccessServer.Infrastructure.Identity;
using IdentityAccessServer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusLibrary.Shared.Logging;

namespace IdentityAccessServer;

public static class Program {
   public static void Main(string[] args) {
      
      var builder = WebApplication.CreateBuilder(args);

      // ----------------------------
      // Logging
      // ----------------------------
      SharedWebAppLogging.ConfigureIdentityAccessServer(builder);

      // ----------------------------
      // Options binding + validation
      // ----------------------------
      builder.Services.AddOptions<IdentityAccessServerOptions>()
         .Bind(builder.Configuration.GetSection(IdentityAccessServerOptions.SectionName))
         .Validate(o => !string.IsNullOrWhiteSpace(o.IssuerUri),
            "IdentityAccessServer:IssuerUri is required.")
         .Validate(o => Uri.TryCreate(o.IssuerUri, UriKind.Absolute, out _),
            "IdentityAccessServer:IssuerUri must be a valid absolute URI.")
         .Validate(o => o.Tokens.AccessTokenLifetime > TimeSpan.Zero,
            "IdentityAccessServer:Tokens:AccessTokenLifetime must be greater than 00:00:00.")
         .Validate(o => o.Tokens.IdentityTokenLifetime > TimeSpan.Zero,
            "IdentityAccessServer:Tokens:IdentityTokenLifetime must be greater than 00:00:00.")
         .Validate(o => o.Tokens.AuthorizationCodeLifetime > TimeSpan.Zero,
            "IdentityAccessServer:Tokens:AuthorizationCodeLifetime must be greater than 00:00:00.")
         .Validate(o => o.Tokens.RefreshTokenLifetime > TimeSpan.Zero,
            "IdentityAccessServer:Tokens:RefreshTokenLifetime must be greater than 00:00:00.")
         .Validate(o => o.Tokens.AuthorizationCodeLifetime <= o.Tokens.RefreshTokenLifetime,
            "IdentityAccessServer:Tokens:AuthorizationCodeLifetime must not exceed RefreshTokenLifetime.")
         .Validate(o => o.Tokens.IdentityTokenLifetime <= o.Tokens.RefreshTokenLifetime,
            "IdentityAccessServer:Tokens:IdentityTokenLifetime must not exceed RefreshTokenLifetime.")
         .ValidateOnStart();


      var authSection = builder.Configuration.GetSection(IdentityAccessServerOptions.SectionName);
      if (!authSection.Exists())
         throw new InvalidOperationException($"Missing configuration section '{IdentityAccessServerOptions.SectionName}'.");
      var authServer = authSection.Get<IdentityAccessServerOptions>()!;
      Console.WriteLine("ENV=" + builder.Environment.EnvironmentName);
      Console.WriteLine("IssuerUri=" + authServer.IssuerUri);
      
      

      // ----------------------------
      // Services
      // ----------------------------
      ConfigureDatabase(builder.Services, builder.Configuration, builder.Environment);
      ConfigureIdentity(builder.Services);
      ConfigureOpenIddict(builder.Services, authServer, builder.Environment);
      ConfigureMvcAndUi(builder.Services);

      // Seed demo users and standard clients (WASM, MVC, SSR, Android, Service)
     
      builder.Services.AddHostedService<SeedUsersHostedService>();
      builder.Services.AddHostedService<SeedHostedService>();

      // ----------------------------
      // CORS (from config)
      // ----------------------------
      var allowedOrigins = new[] {
         Origin(authServer.BlazorWasm.BaseUrl),    // https://localhost:6010
         Origin(authServer.WebMvc.BaseUrl),        // https://localhost:6020 (optional)
         Origin(authServer.BankingClientSsr.BaseUrl),       // https://localhost:6030 (optional)
         Origin(authServer.CampusLibraryClientSsr.BaseUrl)  // https://localhost:6040 (optional)
      };

      builder.Services.AddCors(options => {
         options.AddPolicy("Frontends", policy => {
            policy.WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
         });
      });

      var app = builder.Build();

      // ----------------------------
      // HTTP pipeline
      // ----------------------------
      if (app.Environment.IsDevelopment()) {
         app.UseDeveloperExceptionPage();
         app.UseHttpLogging(); // only in dev
      }

      //app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      // CORS must be between UseRouting and auth/endpoints.
      app.UseCors("Frontends");

      // Bind CORS to endpoints for maximum reliability (especially /.well-known and /connect/*)
      app.MapControllers().RequireCors("Frontends");
      app.MapDefaultControllerRoute().RequireCors("Frontends");

      // Identity UI is same-origin server-rendered HTML. Applying the frontend
      // CORS policy to Razor Pages is unnecessary and causes misleading CORS
      // failures for normal login and registration form posts.
      app.MapRazorPages();

      app.UseAuthentication();
      app.UseAuthorization();

      
      app.Run();
   }

   // -------------------------------------------------------------------------
   // Helpers
   // -------------------------------------------------------------------------
   private static string Origin(string url) {
      var uri = new Uri(url, UriKind.Absolute);
      return uri.GetLeftPart(UriPartial.Authority);
   }

   private static bool WantsJson(HttpRequest req) {
      var accept = req.Headers.Accept.ToString();
      if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
         return true;

      var xrw = req.Headers["X-Requested-With"].ToString();
      if (xrw.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
         return true;

      return false;
   }

   private static bool IsOidcOrIdentityEndpoint(HttpRequest req) {
      var path = req.Path.Value ?? "";
      return path.StartsWith("/connect/", StringComparison.OrdinalIgnoreCase)
         || path.StartsWith("/.well-known/", StringComparison.OrdinalIgnoreCase)
         || path.StartsWith("/Identity/", StringComparison.OrdinalIgnoreCase);
   }

   // -------------------------------------------------------------------------
   // Service configuration
   // -------------------------------------------------------------------------
   private static void ConfigureDatabase(
      IServiceCollection services,
      IConfiguration config,
      IWebHostEnvironment env) {
      var dbName = config.GetConnectionString("OAuthDb") ?? "openidauth1.0.db";
      var dbFile = Path.Combine(env.ContentRootPath, dbName);
      var sqlite = $"Data Source={dbFile}";

      services.AddDbContext<AuthDbContext>(options => {
         options.UseSqlite(sqlite);
         options.UseOpenIddict();
      });
   }

   private static void ConfigureIdentity(IServiceCollection services) {
      services
         .AddIdentity<ApplicationUser, IdentityRole>()
         .AddEntityFrameworkStores<AuthDbContext>()
         .AddDefaultTokenProviders()
         .AddDefaultUI();

      // Cookie behavior: OIDC/Identity routes must redirect to login UI.
      // Real API calls should return 401/403 instead of HTML.
      services.ConfigureApplicationCookie(o => {
         o.LoginPath = "/Identity/Account/Login";
         o.AccessDeniedPath = "/Identity/Account/AccessDenied";

         o.Events = new CookieAuthenticationEvents {
            OnRedirectToLogin = ctx => {
               if (IsOidcOrIdentityEndpoint(ctx.Request)) {
                  ctx.Response.Redirect(ctx.RedirectUri);
                  return Task.CompletedTask;
               }

               if (WantsJson(ctx.Request)) {
                  ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                  return Task.CompletedTask;
               }

               ctx.Response.Redirect(ctx.RedirectUri);
               return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = ctx => {
               if (IsOidcOrIdentityEndpoint(ctx.Request)) {
                  ctx.Response.Redirect(ctx.RedirectUri);
                  return Task.CompletedTask;
               }

               if (WantsJson(ctx.Request)) {
                  ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                  return Task.CompletedTask;
               }

               ctx.Response.Redirect(ctx.RedirectUri);
               return Task.CompletedTask;
            }
         };
      });

      services.AddAuthorization();
   }

   private static void ConfigureOpenIddict(
      IServiceCollection services,
      IdentityAccessServerOptions auth,
      IWebHostEnvironment env
   ) {
      services.AddOpenIddict()

         // Core (EF storage)
         .AddCore(o => {
            o.UseEntityFrameworkCore()
               .UseDbContext<AuthDbContext>();
         })

         // Server
         .AddServer(options => {
            // Issuer / Authority
            options
               .SetIssuer(new Uri(auth.IssuerUri, UriKind.Absolute));

            // Endpoints as paths
            options
               .SetAuthorizationEndpointUris("/" + IdentityAccessServerOptions.AuthorizationEndpointPath)
               .SetTokenEndpointUris("/" + IdentityAccessServerOptions.TokenEndpointPath)
               .SetUserInfoEndpointUris("/" + IdentityAccessServerOptions.UserInfoEndpointPath)
               .SetEndSessionEndpointUris("/" + IdentityAccessServerOptions.LogoutEndpointPath)
               .SetConfigurationEndpointUris("/" + IdentityAccessServerOptions.ConfigurationEndpointPath);

            // Flows
            options.AllowAuthorizationCodeFlow();
            options.AllowClientCredentialsFlow();
            options.AllowRefreshTokenFlow();

            if (env.IsDevelopment())
               options.AllowCustomFlow(DevGrantTypes.DevPassword);

            // PKCE required for public clients
            options
               .RequireProofKeyForCodeExchange();
            
            // Token lifetimes
            options
               .SetAccessTokenLifetime(auth.Tokens.AccessTokenLifetime)
               .SetIdentityTokenLifetime(auth.Tokens.IdentityTokenLifetime)
               .SetAuthorizationCodeLifetime(auth.Tokens.AuthorizationCodeLifetime)
               .SetRefreshTokenLifetime(auth.Tokens.RefreshTokenLifetime);
            

            // Scopes (standard + configured API scopes)
            options
               .RegisterScopes(
                  new[] { "openid", "profile", "email", "offline_access" }
                     .Concat(auth.Apis.Values.Select(a => a.Scope))
                     .Distinct(StringComparer.Ordinal)
                     .ToArray()
               );
            
            // Dev certs
            options
               .AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

            if (!auth.Tokens.EncryptAccessTokens)
               options.DisableAccessTokenEncryption();

            // ASP.NET Core integration
            options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .EnableStatusCodePagesIntegration();
         })

         // Validation (resource server)
         .AddValidation(o => {
            o.UseLocalServer();
            o.UseAspNetCore();
         });
   }

   private static void ConfigureMvcAndUi(IServiceCollection services) {
      services.AddControllersWithViews();
      services.AddRazorPages();
   }
}