namespace CampusLibraryApi.Configure;

/// <summary>
/// Configuration values for Cross-Origin Resource Sharing (CORS).
/// </summary>
public sealed class CampusLibraryCorsOptions {

   /// <summary>
   /// Browser origins that may call CampusLibraryApi.
   /// Origins consist of scheme, host and port, but no path.
   /// </summary>
   public string[] AllowedOrigins { get; init; } = [];
}
