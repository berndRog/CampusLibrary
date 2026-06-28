using CampusLibraryClient.Security;

namespace CampusLibraryClient.Extensions;

public static class AuthorizationExtensions {

   public static IServiceCollection ConfigureAuthZ(
      this IServiceCollection services
   ) {
      services.AddAuthorization(options => {

         options.AddPolicy(
            name: CampusLibraryPolicies.CanReadCatalog,
            configurePolicy: policy => policy.RequireAuthenticatedUser()
         );

         options.AddPolicy(
            name: CampusLibraryPolicies.CanBorrowBooks,
            configurePolicy: policy => policy.RequireRole(CampusLibraryRoles.Student)
         );

         options.AddPolicy(
            name: CampusLibraryPolicies.CanReadOwnLoans,
            configurePolicy: policy => policy.RequireRole(CampusLibraryRoles.Student)
         );

         options.AddPolicy(
            name: CampusLibraryPolicies.CanManageReaders,
            configurePolicy: policy => policy.RequireRole(CampusLibraryRoles.Employee)
         );

         options.AddPolicy(
            name: CampusLibraryPolicies.CanManageCatalog,
            configurePolicy: policy => policy.RequireRole(CampusLibraryRoles.Employee)
         );

         options.AddPolicy(
            name: CampusLibraryPolicies.CanManageLoans,
            configurePolicy: policy => policy.RequireRole(CampusLibraryRoles.Employee)
         );
      });

      return services;
   }
}
