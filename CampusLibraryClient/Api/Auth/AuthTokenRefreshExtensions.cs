using System.Globalization;
using System.Text.Json.Serialization;
using CampusLibraryClient.Shared.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace CampusLibraryClient.Api.Auth;

public static class AuthTokenRefreshExtensions {

   /// <summary>
   /// Silently refreshes the access token when it is about to expire.
   /// Returns true when the token is valid, either unchanged or just refreshed.
   /// </summary>
   public static async Task<bool> TryRefreshAccessTokenAsync(
      this HttpContext httpContext,
      IHttpClientFactory httpClientFactory,
      IConfiguration config,
      CancellationToken ct = default,
      ILogger? logger = null
   ) {
      // Read current auth ticket from the cookie.
      AuthenticateResult auth = await httpContext.AuthenticateAsync(
         CookieAuthenticationDefaults.AuthenticationScheme
      );

      if(!auth.Succeeded || auth.Properties is null)
         return false;

      // Read tokens from the auth cookie.
      string? refreshToken = auth.Properties.GetTokenValue("refresh_token");
      string? expiresAtRaw = auth.Properties.GetTokenValue("expires_at");

      if(string.IsNullOrWhiteSpace(refreshToken))
         return false;

      // Refresh only if the access token is missing or expires soon.
      if(!IsExpiringSoon(expiresAtRaw)) {
         logger?.LetLogTokenRefresh(
            refreshEvent: TokenRefreshEvent.Skipped,
            detail: $"expires at {expiresAtRaw}"
         );
         return true;
      }

      logger?.LetLogTokenRefresh(
         refreshEvent: TokenRefreshEvent.Attempting,
         detail: $"expires at {expiresAtRaw ?? "unknown"}"
      );

      string tokenEndpointRaw = config["IdentityAccessServer:TokenEndpoint"]
         ?? throw new InvalidOperationException("Missing configuration: IdentityAccessServer:TokenEndpoint");

      if(!Uri.TryCreate(tokenEndpointRaw, UriKind.Absolute, out Uri? tokenEndpoint))
         return false;

      string clientId = config["IdentityAccessServer:ClientId"]
         ?? throw new InvalidOperationException("Missing configuration: IdentityAccessServer:ClientId");

      string clientSecret = config["IdentityAccessServer:ClientSecret"]
         ?? throw new InvalidOperationException("Missing configuration: IdentityAccessServer:ClientSecret");

      HttpClient http = httpClientFactory.CreateClient("IdentityAccessServer");

      using HttpRequestMessage req = new(
         method: HttpMethod.Post,
         requestUri: tokenEndpoint
      );

      req.Content = new FormUrlEncodedContent(
         new Dictionary<string, string> {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
         }
      );

      using HttpResponseMessage res = await http.SendAsync(
         request: req,
         cancellationToken: ct
      );

      if(!res.IsSuccessStatusCode) {
         logger?.LetLogTokenRefresh(
            refreshEvent: TokenRefreshEvent.Failed,
            detail: $"identity server returned HTTP {(int)res.StatusCode}"
         );
         return false;
      }

      TokenResponse? payload = await res.Content.ReadFromJsonAsync<TokenResponse>(
         cancellationToken: ct
      );

      if(payload is null || string.IsNullOrWhiteSpace(payload.AccessToken)) {
         logger?.LetLogTokenRefresh(
            refreshEvent: TokenRefreshEvent.Failed,
            detail: "identity server response did not contain a valid access_token"
         );
         return false;
      }

      // Update tokens in cookie.
      string newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn)
         .ToString("o", CultureInfo.InvariantCulture);

      List<AuthenticationToken> tokens = [
         new AuthenticationToken { Name = "access_token", Value = payload.AccessToken },
         new AuthenticationToken { Name = "expires_at", Value = newExpiresAt }
      ];

      if(!string.IsNullOrWhiteSpace(payload.RefreshToken)) {
         tokens.Add(
            new AuthenticationToken {
               Name = "refresh_token",
               Value = payload.RefreshToken
            }
         );
      }

      auth.Properties.StoreTokens(tokens);

      // Re-issue the cookie with updated tokens.
      await httpContext.SignInAsync(
         scheme: CookieAuthenticationDefaults.AuthenticationScheme,
         principal: auth.Principal!,
         properties: auth.Properties
      );

      logger?.LetLogTokenRefresh(
         refreshEvent: TokenRefreshEvent.Succeeded,
         detail: $"new token expires at {newExpiresAt}"
      );

      return true;
   }

   private static bool IsExpiringSoon(string? expiresAtRaw) {
   
      if(string.IsNullOrWhiteSpace(expiresAtRaw))
         return true;

      if(!DateTimeOffset.TryParse(
            expiresAtRaw,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTimeOffset expiresAt
         ))
         return true;

      return expiresAt <= DateTimeOffset.UtcNow.AddSeconds(60);
   }

   private static void LetLogTokenRefresh(
      this ILogger logger,
      TokenRefreshEvent refreshEvent,
      string detail
   ) =>
      AppDiagnosticsLogger.LogTokenRefresh(
         logger: logger,
         refreshEvent: refreshEvent,
         detail: detail
      );

   private sealed class TokenResponse {

      [JsonPropertyName("access_token")]
      public string AccessToken { get; set; } = string.Empty;

      [JsonPropertyName("expires_in")]
      public int ExpiresIn { get; set; }

      [JsonPropertyName("refresh_token")]
      public string? RefreshToken { get; set; }

      [JsonPropertyName("token_type")]
      public string TokenType { get; set; } = "Bearer";
   }
}
