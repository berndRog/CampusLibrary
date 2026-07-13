namespace CampusLibraryApi.Configure;

/// <summary>
/// Configuration values used by CampusLibraryApi to validate JWT access tokens
/// issued by the IdentityAccessServer.
/// </summary>
public sealed class AuthOptions {

   /// <summary>
   /// Base address of the IdentityAccessServer.
   /// Example: https://localhost:7010
   /// </summary>
   public string Authority { get; init; } = string.Empty;

   /// <summary>
   /// API resource/audience expected in access tokens.
   /// Example: campus-library-api
   /// </summary>
   public string Audience { get; init; } = string.Empty;

   /// <summary>
   /// Defines whether the OpenID Connect metadata endpoint must use HTTPS.
   /// This should normally be true outside local development environments.
   /// </summary>
   public bool RequireHttpsMetadata { get; init; } = true;

   /// <summary>
   /// Defines whether the audience claim is validated.
   /// </summary>
   public bool ValidateAudience { get; init; } = true;

   /// <summary>
   /// Permitted clock difference between IdentityAccessServer and API.
   /// </summary>
   public int ClockSkewSeconds { get; init; } = 60;
}
