namespace IdentityAccessServer.Auth.Options;

public sealed class TokenOptions
{
   /// <summary>
   /// Lifetime of access tokens issued by OpenIddict.
   /// Configure via appsettings using TimeSpan format, e.g. "00:30:00".
   /// </summary>
   public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(30);

   /// <summary>
   /// Lifetime of identity tokens issued by OpenIddict.
   /// Configure via appsettings using TimeSpan format, e.g. "00:05:00".
   /// </summary>
   public TimeSpan IdentityTokenLifetime { get; init; } = TimeSpan.FromMinutes(5);

   /// <summary>
   /// Lifetime of authorization codes issued by OpenIddict.
   /// Configure via appsettings using TimeSpan format, e.g. "00:05:00".
   /// </summary>
   public TimeSpan AuthorizationCodeLifetime { get; init; } = TimeSpan.FromMinutes(5);

   /// <summary>
   /// Lifetime of refresh tokens issued by OpenIddict.
   /// Configure via appsettings using TimeSpan format, e.g. "14.00:00:00".
   /// </summary>
   public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(14);

   /// <summary>
   /// If true, OpenIddict will encrypt access tokens (JWE).
   /// If false, access tokens are not encrypted (JWS) and can be inspected with jwt.io.
   /// 
   /// Recommended:
   /// - false in Development / teaching (easy debugging)
   /// - true in Production-like setups (token confidentiality)
   /// </summary>
   public bool EncryptAccessTokens { get; init; } = true;
}