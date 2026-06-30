namespace IdentityAccessServer.Auth.Options;

/// <summary>
/// Strongly typed configuration for OAuth2/OIDC/OpenIddict demo setup.
///
/// - No secrets in code.
/// - Issuer is the single source of truth (OIDC relevant).
/// - Client secrets are read from configuration (UserSecrets/Env/KeyVault).
/// </summary>
public sealed class IdentityAccessServerOptions {
   
   public const string SectionName = "IdentityAccessServer";

   // OIDC Issuer (single source of truth)
   // ------------------------------------------------------------------
   /// <summary>
   /// OIDC issuer URI (must end with slash).
   /// Example: https://localhost:7001/
   /// </summary>
   public string IssuerUri { get; init; } = string.Empty;

   /// <summary>
   /// Derived authority base URL (same as issuer, but as string).
   /// </summary>
   public string AuthorityBaseUrl => EnsureTrailingSlash(IssuerUri);
   
   public Uri Issuer => new(EnsureTrailingSlash(IssuerUri));
   
   // Token behavior (dev/teaching vs realistic production setup)
   // ------------------------------------------------------------------
   /// <summary>
   /// Token-related switches (kept together to avoid option-sprawl).
   /// </summary>
   public TokenOptions Tokens { get; init; } = new();

   // Endpoints (paths are stable; actual URIs derived from Issuer)
   // ------------------------------------------------------------------
   // OIDC-standardized well-known prefix (MUST be root-level)
   public const string WellKnownPrefix = ".well-known";
   public const string ConfigurationEndpointPath =
      WellKnownPrefix + "/openid-configuration";

   // OpenIddict protocol endpoints
   public const string ConnectPrefix = "connect";
   public const string AuthorizationEndpointPath = ConnectPrefix + "/authorize";
   public const string TokenEndpointPath = ConnectPrefix + "/token";
   public const string UserInfoEndpointPath = ConnectPrefix + "/userinfo";
   public const string LogoutEndpointPath = ConnectPrefix + "/endsession";

   // APIs (Resources + Scopes)
   // ------------------------------------------------------------------
   public Dictionary<string, ApiOptions> Apis { get; init; } = new();
   
   // Convenience accessors for known APIs
   // ----------------------------------------------------------------
   public ApiOptions CarRentalApi => Apis["CarRentalApi"];
   public ApiOptions BankingApi   => Apis["BankingApi"];
   public ApiOptions ImagesApi    => Apis["ImagesApi"];
   public ApiOptions CampusLibraryApi => Apis["CampusLibraryApi"];
   
   // Clients
   // ------------------------------------------------------------------
   public ClientOptions BlazorWasm { get; init; } = default!;
   public ClientOptions WebMvc { get; init; } = default!;
   public ClientOptions BankingClientSsr { get; init; } = default!;
   public ClientOptions CampusLibraryClientSsr { get; init; } = default!;
   public AndroidClientOptions CampusLibraryAndroidClient { get; init; } = default!;
   public ClientOptions ServiceClient { get; init; } = default!;
   
   // Derived redirect URIs
   // ------------------------------------------------------------------
   public Uri ConfigurationEndpointUri => new(Issuer, ConfigurationEndpointPath);
   public Uri AuthorizationEndpointUri => new(Issuer, AuthorizationEndpointPath);
   public Uri TokenEndpointUri => new(Issuer, TokenEndpointPath);
   public Uri UserInfoEndpointUri => new(Issuer, UserInfoEndpointPath);
   public Uri LogoutEndpointUri => new(Issuer, LogoutEndpointPath);
   
   
   // WASM (Public client)
   public Uri BlazorWasmSignInCallbackUri() =>
      CombineBaseAndPath(BlazorWasm.BaseUrl, BlazorWasm.SignInCallbackPath);

   public Uri BlazorWasmSignOutCallbackUri() =>
      CombineBaseAndPath(BlazorWasm.BaseUrl, BlazorWasm.SignOutCallbackPath);

   // MVC (Confidential client)
   public Uri WebMvcSignInCallbackUri() =>
      CombineBaseAndPath(WebMvc.BaseUrl, WebMvc.SignInCallbackPath);

   public Uri WebMvcSignOutCallbackUri() =>
      CombineBaseAndPath(WebMvc.BaseUrl, WebMvc.SignOutCallbackPath);

   // Banking Blazor SSR (Confidential client)
   public Uri BankingClientSsrSignInCallbackUri() =>
      CombineBaseAndPath(BankingClientSsr.BaseUrl, BankingClientSsr.SignInCallbackPath);

   public Uri BankingClientSsrSignOutCallbackUri() =>
      CombineBaseAndPath(BankingClientSsr.BaseUrl, BankingClientSsr.SignOutCallbackPath);

   // CampusLibrary Blazor SSR (Confidential client)
   public Uri CampusLibraryClientSsrSignInCallbackUri() =>
      CombineBaseAndPath(CampusLibraryClientSsr.BaseUrl, CampusLibraryClientSsr.SignInCallbackPath);

   public Uri CampusLibraryClientSsrSignOutCallbackUri() =>
      CombineBaseAndPath(CampusLibraryClientSsr.BaseUrl, CampusLibraryClientSsr.SignOutCallbackPath);

   // CampusLibrary Android (Public client + PKCE)
   public Uri CampusLibraryAndroidClientCustomSchemeRedirectUri() =>
      new(CampusLibraryAndroidClient.CustomSchemeRedirectUriString, UriKind.Absolute);

   public Uri CampusLibraryAndroidClientLoopbackRedirectUri() =>
      new(CampusLibraryAndroidClient.LoopbackRedirectUriString, UriKind.Absolute);

   public Uri CampusLibraryAndroidClientPostLogoutRedirectUri() =>
      new(CampusLibraryAndroidClient.PostLogoutRedirectUriString, UriKind.Absolute);

   
   // ------------------------------------------------------------------
   // Helpers
   // ------------------------------------------------------------------
   public static string EnsureTrailingSlash(string url)
      => url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";

   public static Uri CombineBaseAndPath(string baseUrl, string path)
      => new Uri($"{baseUrl.TrimEnd('/')}{(path.StartsWith('/') ? "" : "/")}{path}");
}

public enum ClientType {
   Public = 1,
   Confidential = 2
}

public static class IdentityAccessServerSecretKeys {
   public const string WebMvcClientSecret = "IdentityAccessServer:WebMvc:ClientSecret";
   public const string BankingClientSsrSecret = "IdentityAccessServer:BankingClientSsr:ClientSecret";
   public const string CampusLibraryClientSsrSecret = "IdentityAccessServer:CampusLibraryClientSsr:ClientSecret";
   public const string ServiceClientSecret = "IdentityAccessServer:ServiceClient:ClientSecret";
}