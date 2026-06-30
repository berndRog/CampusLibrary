using IdentityAccessServer.Auth.Dev;
using IdentityAccessServer.Auth.Options;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityAccessServer.Auth.Seeding;

/// <summary>
/// Seeds demo OpenIddict data idempotently:
/// - API scopes and resource mappings
/// - OIDC clients for browser, SSR, mobile and service scenarios
/// </summary>
public sealed class SeedHostedService(
   IServiceProvider sp,
   IConfiguration config,
   IWebHostEnvironment env,
   ILogger<SeedHostedService> logger
) : IHostedService {

   public async Task StartAsync(CancellationToken ct) {

      using var scope = sp.CreateScope();

      IdentityAccessServerOptions options = scope.ServiceProvider
         .GetRequiredService<IOptions<IdentityAccessServerOptions>>().Value;

      IOpenIddictApplicationManager apps = scope.ServiceProvider
         .GetRequiredService<IOpenIddictApplicationManager>();

      IOpenIddictScopeManager scopes = scope.ServiceProvider
         .GetRequiredService<IOpenIddictScopeManager>();

      // ------------------------------------------------------------
      // Local helper: Create OR Update clients (idempotent)
      // ------------------------------------------------------------
      async Task UpsertAsync(
         OpenIddictApplicationDescriptor descriptor,
         bool requiresSecret,
         string? secretConfigurationKey = null
      ) {
         object? existing = await apps.FindByClientIdAsync(descriptor.ClientId!, ct);

         if(requiresSecret && string.IsNullOrWhiteSpace(descriptor.ClientSecret)) {
            string keyText = string.IsNullOrWhiteSpace(secretConfigurationKey)
               ? "configuration / user-secrets / environment variables"
               : $"'{secretConfigurationKey}'";

            throw new InvalidOperationException(
               $"Client '{descriptor.ClientId}' is confidential but no ClientSecret was provided. " +
               $"Set it via {keyText}.");
         }

         if(existing is null) {
            await apps.CreateAsync(descriptor, ct);

            logger.LogInformation(
               message: "Created OpenIddict client: {ClientId}",
               descriptor.ClientId
            );
            return;
         }

         await apps.UpdateAsync(existing, descriptor, ct);

         logger.LogInformation(
            message: "Updated OpenIddict client: {ClientId}",
            descriptor.ClientId
         );
      }

      // ------------------------------------------------------------
      // Local helper: Seed scopes (idempotent)
      // ------------------------------------------------------------
      async Task UpsertScopeAsync(
         string scopeName,
         string resourceName,
         string displayName
      ) {
         object? existing = await scopes.FindByNameAsync(scopeName, ct);

         // Scope -> Resource mapping. This is what later drives aud/resources in tokens.
         var descriptor = new OpenIddictScopeDescriptor {
            Name = scopeName,
            DisplayName = displayName,
            Resources = { resourceName }
         };

         if(existing is null) {
            await scopes.CreateAsync(descriptor, ct);

            logger.LogInformation(
               message: "Created OpenIddict scope: {Scope} -> {Resource}",
               scopeName,
               resourceName
            );
            return;
         }

         await scopes.UpdateAsync(existing, descriptor, ct);

         logger.LogInformation(
            message: "Updated OpenIddict scope: {Scope} -> {Resource}",
            scopeName,
            resourceName
         );
      }

      // ------------------------------------------------------------
      // Local helper: Add API scopes to a client descriptor
      // ------------------------------------------------------------
      void AddApiScopes(
         OpenIddictApplicationDescriptor descriptor,
         params string[] apiKeys
      ) {
         foreach(string key in apiKeys) {
            if(!options.Apis.TryGetValue(key, out ApiOptions? api))
               throw new InvalidOperationException(
                  $"IdentityAccessServerOptions.Apis does not contain key '{key}'. " +
                  $"Check appsettings: IdentityAccessServer:Apis:{key}");

            descriptor.Permissions.Add(Permissions.Prefixes.Scope + api.Scope);
         }
      }

      // ------------------------------------------------------------
      // 0) Seed API scopes (Scope -> Resource)
      // ------------------------------------------------------------
      if(options.Apis.Count == 0)
         throw new InvalidOperationException(
            "No APIs configured. Add IdentityAccessServer:Apis:{...} in appsettings.json.");

      foreach((string key, ApiOptions api) in options.Apis) {
         if(string.IsNullOrWhiteSpace(api.Scope) || string.IsNullOrWhiteSpace(api.Resource))
            throw new InvalidOperationException(
               $"Invalid API config for '{key}'. Scope and Resource are required.");

         await UpsertScopeAsync(
            scopeName: api.Scope,
            resourceName: api.Resource,
            displayName: key
         );
      }

      // ------------------------------------------------------------
      // 1) Blazor WASM (Public + Code + PKCE)
      // ------------------------------------------------------------
      var blazorWasm = new OpenIddictApplicationDescriptor {
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

      AddApiScopes(blazorWasm, "BankingApi", "CarRentalApi");
      AllowRefreshTokens(blazorWasm);

      await UpsertAsync(
         descriptor: blazorWasm,
         requiresSecret: false
      );

      // ------------------------------------------------------------
      // 2) Web MVC (Confidential + Code)
      // ------------------------------------------------------------
      var webMvc = new OpenIddictApplicationDescriptor {
         ClientId = options.WebMvc.ClientId,
         ClientSecret = 
            config[IdentityAccessServerSecretKeys.WebMvcClientSecret],
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

      AddApiScopes(webMvc, "BankingApi", "CarRentalApi");
      AllowRefreshTokens(webMvc);

      await UpsertAsync(
         descriptor: webMvc,
         requiresSecret: true,
         secretConfigurationKey: IdentityAccessServerSecretKeys.WebMvcClientSecret
      );

      // ------------------------------------------------------------
      // 3) Banking Blazor SSR (Confidential + Code)
      // ------------------------------------------------------------
      var bankingClientSsr = new OpenIddictApplicationDescriptor {
         ClientId = options.BankingClientSsr.ClientId,
         ClientSecret = 
            config[IdentityAccessServerSecretKeys.BankingClientSsrSecret],
         DisplayName = "Banking Client Blazor SSR",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.BankingClientSsrSignInCallbackUri() },
         PostLogoutRedirectUris = { options.BankingClientSsrSignOutCallbackUri() },

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

      AddApiScopes(bankingClientSsr, "BankingApi", "CarRentalApi");
      AllowRefreshTokens(bankingClientSsr);

      await UpsertAsync(
         descriptor: bankingClientSsr,
         requiresSecret: true,
         secretConfigurationKey: IdentityAccessServerSecretKeys.BankingClientSsrSecret
      );

      // ------------------------------------------------------------
      // 4) CampusLibrary Blazor SSR (Confidential + Code)
      // ------------------------------------------------------------
      var campusLibraryClientSsr = new OpenIddictApplicationDescriptor {
         ClientId = options.CampusLibraryClientSsr.ClientId,
         ClientSecret = 
            config[IdentityAccessServerSecretKeys.CampusLibraryClientSsrSecret],
         DisplayName = "CampusLibrary Client Blazor SSR",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.CampusLibraryClientSsrSignInCallbackUri() },
         PostLogoutRedirectUris = { options.CampusLibraryClientSsrSignOutCallbackUri() },

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

      AddApiScopes(campusLibraryClientSsr, "CampusLibraryApi");
      AllowRefreshTokens(campusLibraryClientSsr);

      await UpsertAsync(
         descriptor: campusLibraryClientSsr,
         requiresSecret: true,
         secretConfigurationKey: IdentityAccessServerSecretKeys.CampusLibraryClientSsrSecret
      );

      // ------------------------------------------------------------
      // 5) CampusLibrary Android (Public + Code + PKCE)
      // ------------------------------------------------------------
      var campusLibraryAndroidClient = new OpenIddictApplicationDescriptor {
         ClientId = options.CampusLibraryAndroidClient.ClientId,
         DisplayName = "CampusLibrary Android Client",
         ClientType = ClientTypes.Public,

         RedirectUris = {
            options.CampusLibraryAndroidClientCustomSchemeRedirectUri(),
            options.CampusLibraryAndroidClientLoopbackRedirectUri()
         },
         PostLogoutRedirectUris = {
            options.CampusLibraryAndroidClientPostLogoutRedirectUri()
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

      AddApiScopes(campusLibraryAndroidClient, "CampusLibraryApi");
      AllowRefreshTokens(campusLibraryAndroidClient);

      await UpsertAsync(
         descriptor: campusLibraryAndroidClient,
         requiresSecret: false
      );

      // ------------------------------------------------------------
      // 6) Service Client (Confidential + Client Credentials)
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

      AddApiScopes(
         service,
         "CarRentalApi",
         "BankingApi",
         "ImagesApi",
         "CampusLibraryApi"
      );

      await UpsertAsync(
         descriptor: service,
         requiresSecret: true,
         secretConfigurationKey: IdentityAccessServerSecretKeys.ServiceClientSecret
      );

      // ------------------------------------------------------------
      // 7) Development-only dev-password client (Public + Custom Token Grant)
      // ------------------------------------------------------------
      if(env.IsDevelopment()) {
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

         AddApiScopes(
            devClient,
            "BankingApi",
            "CarRentalApi",
            "ImagesApi",
            "CampusLibraryApi"
         );
         AllowRefreshTokens(devClient);

         await UpsertAsync(
            descriptor: devClient,
            requiresSecret: false
         );
      }
   }

   public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

   private void AllowRefreshTokens(OpenIddictApplicationDescriptor descriptor) {
      descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
      descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.OfflineAccess);
   }
}

/*
==========================================================
DIDAKTIK / LERNZIELE (DE)
==========================================================

1) Warum seedet man Scopes UND Clients?
--------------------------------------
OpenIddict trennt klar:
- Scopes: "Welche Berechtigungsbereiche gibt es?" (z.B. campus_library_api)
- Resources: "Für welche API gilt der Scope?" (z.B. campus-library-api)
- Clients:  "Welche App darf welche Scopes anfordern?"

Der Seed sorgt dafür, dass diese Regeln automatisch und reproduzierbar
in der Datenbank stehen – ohne manuelle Klickarbeit.

2) Scope -> Resource ist der Schlüssel für 'aud'
------------------------------------------------
Die Audience (aud) in Access Tokens entsteht aus den Resources.
Und Resources kommen hier aus den OpenIddictScopeDescriptor.Resources.

Merksatz:
- Client fordert Scope an
- Scope ist mit Resource verknüpft
- Resource wird zu 'aud' im Token

3) Warum konkrete Client-Namen?
-------------------------------
Die Konfiguration enthält mehrere Beispielanwendungen. Deshalb werden
fachlich spezifische Clients benannt:
- BankingClientSsr für den Banking Blazor SSR Client
- CampusLibraryClientSsr für den CampusLibrary Blazor SSR Client
- CampusLibraryAndroidClient für die spätere Android Compose App

Generische Clients wie BlazorWasm und WebMvc bleiben erhalten, weil sie
als allgemeine OIDC-Beispiele genutzt werden.

4) Idempotenz (Create OR Update)
--------------------------------
Wir können den Seed bei jedem Start ausführen:
- existiert der Client/Scope -> Update
- existiert er nicht         -> Create

Damit bleibt die Demo stabil, auch wenn sich Konfigurationen ändern
(z.B. RedirectUris, neue Scopes, neue Clients).

5) Minimalprinzip
-----------------
Wir geben Clients nur die Scopes, die sie wirklich brauchen.
Das ist eine konkrete Umsetzung von "Least Privilege".

==========================================================
*/
