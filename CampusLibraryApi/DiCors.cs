using CampusLibraryApi.Configure;

namespace CampusLibraryApi;

/// <summary>
/// Central extension class for registering Cross-Origin Resource Sharing
/// (CORS) for CampusLibraryApi.
/// </summary>
public static class DiCors {

   /// <summary>
   /// Registers the configured browser origins as a named CORS policy.
   ///
   /// The allowed origins are read from appsettings.json:
   ///
   /// "Cors": {
   ///   "AllowedOrigins": [
   ///     "https://localhost:6040",
   ///     "http://localhost:5040"
   ///   ]
   /// }
   /// </summary>
   public static IServiceCollection AddCampusLibraryCors(
      this IServiceCollection services,
      IConfiguration config
   ) {
      // --------------------------------------------------------------------
      // Bind and validate CORS configuration
      // --------------------------------------------------------------------
      // CORS is a browser security mechanism. An origin consists of:
      //
      // scheme + host + port
      //
      // Examples:
      // https://localhost:6040
      // http://localhost:5040
      //
      // Paths such as /catalog or /api are not part of an origin.
      services.AddOptions<CampusLibraryCorsOptions>()
         .Bind(config.GetSection("Cors"))
         .Validate(
            options => options.AllowedOrigins.Length > 0,
            "Cors:AllowedOrigins must contain at least one origin."
         )
         .ValidateOnStart();

      var corsOptions = config
         .GetSection("Cors")
         .Get<CampusLibraryCorsOptions>()
         ?? throw new InvalidOperationException(
            "Missing configuration section 'Cors'."
         );

      var allowedOrigins = corsOptions.AllowedOrigins
         .Where(origin => !string.IsNullOrWhiteSpace(origin))
         .Select(origin => origin.Trim().TrimEnd('/'))
         .Distinct(StringComparer.OrdinalIgnoreCase)
         .ToArray();

      if (allowedOrigins.Length == 0)
         throw new InvalidOperationException(
            "Cors:AllowedOrigins must contain at least one non-empty origin."
         );

      // --------------------------------------------------------------------
      // Register named CORS policy
      // --------------------------------------------------------------------
      // WithOrigins restricts browser access to the explicitly configured
      // frontends. AllowAnyHeader permits headers such as Authorization and
      // Content-Type. AllowAnyMethod permits GET, POST, PUT and DELETE.
      //
      // AllowAnyOrigin is deliberately not used because it would discard the
      // explicit allow-list from appsettings.json.
      services.AddCors(options => {
         options.AddPolicy(
            CorsPolicyNames.CampusLibraryClients,
            policy => policy
               .WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
         );
      });

      Console.WriteLine(
         $"CORS AllowedOrigins: {string.Join(", ", allowedOrigins)}"
      );

      return services;
   }
}

/*
Lernziele und Didaktik
----------------------

1. CORS ist keine Authentifizierung und keine Autorisierung.
   CORS entscheidet nur, ob ein Browser JavaScript-Aufrufe von einem anderen
   Origin an die API zulassen darf.

2. Server-zu-Server-Aufrufe werden nicht durch CORS geschützt. Ein Blazor-SSR-
   Server kann die API unabhängig von der Browser-CORS-Prüfung aufrufen.

3. Die erlaubten Origins stehen in appsettings.json und nicht fest im Code.
   Dadurch können Entwicklungs-, Test- und Produktionsumgebungen eigene
   Frontend-Adressen konfigurieren.

4. Middleware-Reihenfolge:
   UseRouting -> UseCors -> UseAuthentication -> UseAuthorization
*/
