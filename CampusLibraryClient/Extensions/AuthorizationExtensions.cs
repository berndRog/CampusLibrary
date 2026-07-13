namespace CampusLibraryClient.Extensions;

public static class AuthorizationExtensions {

   public static IServiceCollection ConfigureAuthZ(
      this IServiceCollection services
   ) {
      // Part 6 uses roles directly on pages and components:
      // [Authorize(Roles = "Reader")] and
      // [Authorize(Roles = "Employee")].
      // No named client policies are required.
      services.AddAuthorization();

      return services;
   }
}
