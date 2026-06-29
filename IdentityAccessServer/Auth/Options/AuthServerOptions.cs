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
   public ApiOptions BankingApi   => Apis["BankingApi"];
   public ApiOptions ImagesApi    => Apis["ImagesApi"];
   public ApiOptions CampusLibraryApi => Apis["CampusLibraryApi"];
   
   // Clients
   // ------------------------------------------------------------------
   public ClientOptions BlazorWasm { get; init; } = default!;
   public ClientOptions WebMvc { get; init; } = default!;
   public ClientOptions WebBlazorSsr { get; init; } = default!;
   public AndroidClientOptions Android { get; init; } = default!;
   public ClientOptions ServiceClient { get; init; } = default!;
   public ClientOptions CampusLibraryBlazorSsr { get; init; } = default!;
   public AndroidClientOptions CampusLibraryAndroid { get; init; } = default!;
   
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

   // Blazor SSR (Confidential client)
   public Uri WebBlazorSsrSignInCallbackUri() =>
      CombineBaseAndPath(WebBlazorSsr.BaseUrl, WebBlazorSsr.SignInCallbackPath);

   public Uri WebBlazorSsrSignOutCallbackUri() =>
      CombineBaseAndPath(WebBlazorSsr.BaseUrl, WebBlazorSsr.SignOutCallbackPath);



   // CampusLibrary Blazor SSR (Confidential client)
   public Uri CampusLibraryBlazorSsrSignInCallbackUri() =>
      CombineBaseAndPath(CampusLibraryBlazorSsr.BaseUrl, CampusLibraryBlazorSsr.SignInCallbackPath);

   public Uri CampusLibraryBlazorSsrSignOutCallbackUri() =>
      CombineBaseAndPath(CampusLibraryBlazorSsr.BaseUrl, CampusLibraryBlazorSsr.SignOutCallbackPath);

   // CampusLibrary Android Compose (Public client + PKCE)
   public Uri CampusLibraryAndroidCustomSchemeRedirectUri() =>
      new(CampusLibraryAndroid.CustomSchemeRedirectUriString, UriKind.Absolute);

   public Uri CampusLibraryAndroidLoopbackRedirectUri() =>
      new(CampusLibraryAndroid.LoopbackRedirectUriString, UriKind.Absolute);

   public Uri CampusLibraryAndroidPostLogoutRedirectUri() =>
      new(CampusLibraryAndroid.PostLogoutRedirectUriString, UriKind.Absolute);

   // Android bleibt wie gehabt (weil dort die Strings schon semantisch klar sind)
   public Uri AndroidCustomSchemeRedirectUri() =>
      new(Android.CustomSchemeRedirectUriString, UriKind.Absolute);

   public Uri AndroidLoopbackRedirectUri() =>
      new(Android.LoopbackRedirectUriString, UriKind.Absolute);

   public Uri AndroidPostLogoutRedirectUri() =>
      new(Android.PostLogoutRedirectUriString, UriKind.Absolute);

   
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
   public const string WebBlazorSsrSecret = "IdentityAccessServer:WebBlazorSsr:ClientSecret";
   public const string ServiceClientSecret = "IdentityAccessServer:ServiceClient:ClientSecret";
   public const string CampusLibraryBlazorSsrSecret = "IdentityAccessServer:CampusLibraryBlazorSsr:ClientSecret";
}