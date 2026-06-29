using IdentityAccessServer.Auth.Options;
using IdentityAccessServer.Auth.Dev;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityAccessServer.Auth.Seeding;

/// <summary>
/// Seeds demo OpenIddict data idempotently.
///
/// The server keeps the older Banking clients and additionally
/// registers CampusLibrary-specific clients:
/// - CampusLibrary Blazor SSR client (confidential, Authorization Code)
/// - CampusLibrary Android Compose app (public, Authorization Code + PKCE)
///
/// IMPORTANT:
/// - Resources/Audiences are derived from the scope definitions.
/// - Clients are only allowed to request scopes explicitly added here.
/// </summary>
public sealed class SeedHostedService(
   IServiceProvider sp,
   IConfiguration config,
   IWebHostEnvironment env,
   ILogger<SeedHostedService> logger
) : IHostedService {

   public async Task StartAsync(CancellationToken ct) {

      using var scope = sp.CreateScope();

      var options = scope.ServiceProvider
         .GetRequiredService<IOptions<IdentityAccessServerOptions>>().Value;

      var apps = scope.ServiceProvider
         .GetRequiredService<IOpenIddictApplicationManager>();

      var scopes = scope.ServiceProvider
         .GetRequiredService<IOpenIddictScopeManager>();

      // ------------------------------------------------------------
      // Local helper: Create OR Update (idempotent)
      // ------------------------------------------------------------
      async Task UpsertAsync(
         OpenIddictApplicationDescriptor descriptor,
         bool requiresSecret
      ) {
         var existing = await apps.FindByClientIdAsync(
            descriptor.ClientId!, ct);

         if(existing is null) {
            if(requiresSecret && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
               throw new InvalidOperationException(
                  $"Client '{descriptor.ClientId}' is confidential but no ClientSecret was provided. " +
                  "Set it via configuration/user-secrets."
               );

            await apps.CreateAsync(descriptor, ct);

            logger.LogInformation("Created OpenIddict client: {ClientId}",
               descriptor.ClientId);
            return;
         }

         if(requiresSecret && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
            throw new InvalidOperationException(
               $"Client '{descriptor.ClientId}' exists and is confidential but no ClientSecret was provided. " +
               "Set it via configuration/user-secrets."
            );

         await apps.UpdateAsync(
            application: existing,
            descriptor: descriptor,
            cancellationToken: ct
         );

         logger.LogInformation(
            "Updated OpenIddict client: {ClientId}", descriptor.ClientId);
      }
      
      // ------------------------------------------------------------
      // Local helper: Seed scopes (idempotent)
      // ------------------------------------------------------------
      async Task UpsertScopeAsync(
         string scopeName,
         string resourceName,
         string displayName
      ) {
         var existing = await scopes.FindByNameAsync(scopeName, ct);

         // Scope -> Resource mapping (THIS is what drives aud/resources in tokens)
         var descriptor = new OpenIddictScopeDescriptor {
            Name = scopeName,
            DisplayName = displayName,
            Resources = { resourceName }
         };

         if(existing is null) {
            await scopes.CreateAsync(descriptor, ct);

            logger.LogInformation(
               "Created OpenIddict scope: {Scope} -> {Resource}",
               scopeName, resourceName);
            return;
         }

         await scopes.UpdateAsync(
            scope: existing,
            descriptor: descriptor,
            cancellationToken: ct
         );

         logger.LogInformation(
            "Updated OpenIddict scope: {Scope} -> {Resource}",
            scopeName, resourceName);
      }

      // ------------------------------------------------------------
      // Local helper: Add API scopes to a client descriptor
      //
      // NOTE:
      // - OpenIddict application permissions decide which scopes a client is allowed to request.
      // - The actual aud/resources are derived later from the scope definitions (above).
      // ------------------------------------------------------------
      void AddApiScopes(
         OpenIddictApplicationDescriptor descriptor, 
         params string[] apiKeys
      ) {
         foreach (var key in apiKeys) {
            if (!options.Apis.TryGetValue(key, out var api))
               throw new InvalidOperationException(
                  $"IdentityAccessServerOptions.Apis does not contain key '{key}'. " +
                  $"Check appsettings: IdentityAccessServer:Apis:{key}"
               );

            descriptor.Permissions.Add(Permissions.Prefixes.Scope + api.Scope);
         }
      }

      // ------------------------------------------------------------
      // 0) Seed API scopes (Scope -> Resource)
      // ------------------------------------------------------------
      if (options.Apis.Count == 0)
         throw new InvalidOperationException(
            "No APIs configured. Add IdentityAccessServer:Apis:{...} in appsettings.json."
         );

      foreach(var (key, api) in options.Apis) {
         if(string.IsNullOrWhiteSpace(api.Scope) || string.IsNullOrWhiteSpace(api.Resource))
            throw new InvalidOperationException(
               $"Invalid API config for '{key}'. Scope and Resource are required."
            );

         await UpsertScopeAsync(
            scopeName: api.Scope,
            resourceName: api.Resource,
            displayName: key
         );
      }

      // ------------------------------------------------------------
      // 1) Blazor WASM (Public + Code + PKCE)
      // ------------------------------------------------------------
      var blazor = new OpenIddictApplicationDescriptor {
         ClientId = options.BlazorWasm.ClientId,
         DisplayName = "Blazor WASM",
         ClientType = ClientTypes.Public,

         RedirectUris = { options.BlazorWasmSignInCallbackUri() },
         PostLogoutRedirectUris = { options.BlazorWasmSignOutCallbackUri() },
         
         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         },

         Requirements = {
            Requirements.Features.ProofKeyForCodeExchange
         }
      }; 

      AddApiScopes(blazor, "BankingApi");
      AllowRefreshTokens(blazor);
      await UpsertAsync(blazor, requiresSecret: false);

      // ------------------------------------------------------------
      // 2) Web MVC (Confidential + Code)
      // ------------------------------------------------------------
      var webMvc = new OpenIddictApplicationDescriptor {
         ClientId = options.WebMvc.ClientId,
         ClientSecret = config[IdentityAccessServerSecretKeys.WebMvcClientSecret],
         DisplayName = "WebClient MVC",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.WebMvcSignInCallbackUri() },
         PostLogoutRedirectUris = { options.WebMvcSignOutCallbackUri() },
         
         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         }
      };

      AddApiScopes(webMvc, "BankingApi");
      AllowRefreshTokens(webMvc);
      await UpsertAsync(webMvc, requiresSecret: true);

      // ------------------------------------------------------------
      // 3) Web BlazorSSR (Confidential + Code)
      // ------------------------------------------------------------
      var webBlazorSsr = new OpenIddictApplicationDescriptor {
         ClientId = options.WebBlazorSsr.ClientId,
         ClientSecret = config[IdentityAccessServerSecretKeys.WebBlazorSsrSecret],
         DisplayName = "WebClient Blazor SSR",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.WebBlazorSsrSignInCallbackUri() },
         PostLogoutRedirectUris = { options.WebBlazorSsrSignOutCallbackUri() },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         }
      };

      AddApiScopes(webBlazorSsr, "BankingApi");
      AllowRefreshTokens(webBlazorSsr);
      await UpsertAsync(webBlazorSsr, requiresSecret: true);

      // ------------------------------------------------------------
      // CampusLibrary Blazor SSR client
      // ------------------------------------------------------------
      var campusLibraryBlazorSsr = new OpenIddictApplicationDescriptor {
         ClientId = options.CampusLibraryBlazorSsr.ClientId,
         ClientSecret = config[IdentityAccessServerSecretKeys.CampusLibraryBlazorSsrSecret],
         DisplayName = "CampusLibraryClient Blazor SSR",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.CampusLibraryBlazorSsrSignInCallbackUri() },
         PostLogoutRedirectUris = { options.CampusLibraryBlazorSsrSignOutCallbackUri() },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,
            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         }
      };

      AddApiScopes(campusLibraryBlazorSsr, "CampusLibraryApi");
      AllowRefreshTokens(campusLibraryBlazorSsr);
      await UpsertAsync(campusLibraryBlazorSsr, requiresSecret: true);

     
      // ------------------------------------------------------------
      // 4) Android (Public + Code + PKCE)
      // ------------------------------------------------------------
      var android = new OpenIddictApplicationDescriptor {
         ClientId = options.Android.ClientId,
         DisplayName = "Android App",
         ClientType = ClientTypes.Public,

         RedirectUris = {
            options.AndroidCustomSchemeRedirectUri(),
            options.AndroidLoopbackRedirectUri()
         },
         PostLogoutRedirectUris = {
            options.AndroidPostLogoutRedirectUri()
         },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         },

         Requirements = {
            Requirements.Features.ProofKeyForCodeExchange
         }
      };

      AddApiScopes(android, "BankingApi");
      AllowRefreshTokens(android);
      await UpsertAsync(android, requiresSecret: false);

      // ------------------------------------------------------------
      // CampusLibrary Android Compose app
      // ------------------------------------------------------------
      var campusLibraryAndroid = new OpenIddictApplicationDescriptor {
         ClientId = options.CampusLibraryAndroid.ClientId,
         DisplayName = "CampusLibraryAndroid",
         ClientType = ClientTypes.Public,

         RedirectUris = {
            options.CampusLibraryAndroidCustomSchemeRedirectUri(),
            options.CampusLibraryAndroidLoopbackRedirectUri()
         },
         PostLogoutRedirectUris = {
            options.CampusLibraryAndroidPostLogoutRedirectUri()
         },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,
            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile
         },

         Requirements = {
            Requirements.Features.ProofKeyForCodeExchange
         }
      };

      AddApiScopes(campusLibraryAndroid, "CampusLibraryApi");
      AllowRefreshTokens(campusLibraryAndroid);
      await UpsertAsync(campusLibraryAndroid, requiresSecret: false);
      
      // ------------------------------------------------------------
      // 5) Service Client (Confidential + Client Credentials)
      // ------------------------------------------------------------
      var service = new OpenIddictApplicationDescriptor {
         ClientId = options.ServiceClient.ClientId,
         ClientSecret = config[IdentityAccessServerSecretKeys.ServiceClientSecret],
         DisplayName = "Service Client",
         ClientType = ClientTypes.Confidential,

         Permissions = {
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.ClientCredentials
         }
      };

      // Service client may call multiple APIs:
      AddApiScopes(service, "ImagesApi");

      await UpsertAsync(service, requiresSecret: true);

      // ------------------------------------------------------------
      // 6) Development-only dev-password client (Public + Custom Token Grant)
      // ------------------------------------------------------------
      if (env.IsDevelopment()) {
         var devClient = new OpenIddictApplicationDescriptor {
            ClientId = "dev-token-client",
            DisplayName = "Development Token Client",
            ClientType = ClientTypes.Public,

            Permissions = {
               Permissions.Endpoints.Token,
               Permissions.Prefixes.GrantType + DevGrantTypes.DevPassword,
               Permissions.Prefixes.Scope + Scopes.OpenId,
               Permissions.Prefixes.Scope + Scopes.Profile
            }
         };

         AddApiScopes(devClient, "BankingApi", "ImagesApi", "CampusLibraryApi");
         AllowRefreshTokens(devClient);

         await UpsertAsync(devClient, requiresSecret: false);
      }
   }

   public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

   private static void AllowRefreshTokens(
      OpenIddictApplicationDescriptor descriptor
   ) {
      descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
      descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.OfflineAccess);
   }
}

/*
==========================================================
DIDAKTIK / LERNZIELE (DE)
==========================================================

1) Gleiche Identity-Infrastruktur, mehrere Clients
--------------------------------------------------
Der IdentityAccessServer registriert nicht nur eine Web-App, sondern mehrere
Client-Typen:
- Blazor SSR: vertraulicher Server-Client mit Client Secret
- Android Compose: öffentlicher Native Client mit PKCE
- Service Client: Maschinen-zu-Maschinen-Kommunikation

2) CampusLibrary wird als eigene API sichtbar
---------------------------------------------
Der Scope `campus_library_api` ist mit der Resource `campus-library-api`
verbunden. Dadurch kann die CampusLibraryApi später genau diese Audience prüfen.

3) Schrittweise Einführung
--------------------------
Part 5: Client nutzt die API anonym.
Part 6: Client kann Login/Logout.
Part 7: API validiert Bearer Tokens.
Part 8: Client sendet Access Tokens an die geschützte API.

4) Android früh berücksichtigen
-------------------------------
Die Android-App wird bereits als Public Client registriert. Dadurch muss der
IdentityAccessServer später nicht mehr grundsätzlich umgebaut werden.

==========================================================
*/
