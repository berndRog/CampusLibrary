using IdentityAccessServer.Data;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityAccessServer.Auth.Seeding;

/// <summary>
/// Seeds demo users for the course.
/// - customer@demo.local
/// - admin@demo.local (employee with AdminRights bitmask)
///
/// The same technical users can be used by Banking, CampusLibraryClient and
/// CampusLibraryAndroid. Domain-specific entities such as Reader or Employee
/// still live in the respective API/domain and are connected later.
/// </summary>
public sealed class SeedUsersHostedService(
   IServiceProvider sp
) : IHostedService {

   public async Task StartAsync(CancellationToken ct) {
      using var scope = sp.CreateScope();

      var users = scope.ServiceProvider
         .GetRequiredService<UserManager<ApplicationUser>>();

      await EnsureUserAsync(
         users: users,
         id: Guid.Parse("00000000-0000-0000-0001-000000000001"),
         email: "reader@mail.local",
         password: "Geh1m_",
         accountType: "customer",
         adminRights: AdminRights.None
      );

      await EnsureUserAsync(
         users: users,
         id: Guid.Parse("00000000-0001-0000-0000-000000000000"),
         email: "librarian@mail.local",
         password: "Geh1m_",
         accountType: "employee",
         adminRights: (AdminRights)511
      );

      await EnsureUserAsync(
         users: users,
         id: Guid.Parse("00000000-0099-0000-0000-000000000000"),
         email: "admin@mail.local",
         password: "Geh1m_",
         accountType: "employee",
         adminRights: (AdminRights)511
      );
   }

   private static async Task EnsureUserAsync(
      UserManager<ApplicationUser> users,
      Guid id,
      string email,
      string password,
      string accountType = "customer",
      AdminRights adminRights = AdminRights.None
   ) {
      ApplicationUser? existing = await users.FindByEmailAsync(email);
      if(existing is not null)
         return;

      var user = new ApplicationUser {
         Id = id.ToString(),
         UserName = email,
         Email = email,
         EmailConfirmed = true,
         AccountType = accountType,
         AdminRights = adminRights,
         
         CreatedAt = DateTime.UtcNow,
         UpdatedAt = DateTime.UtcNow
      };

      IdentityResult result = await users.CreateAsync(
         user: user,
         password: password
      );

      if(!result.Succeeded) {
         string errors = string.Join(
            separator: "; ",
            values: result.Errors.Select(e => $"{e.Code}:{e.Description}")
         );

         throw new InvalidOperationException(
            $"Failed to seed user '{email}': {errors}"
         );
      }
   }

   public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

/*
DE:
- Seedet Demo-User für CampusLibrary und die späteren Auth-Teile.
- `reader@mail.local` steht didaktisch für einen normalen Benutzer.
- `librarian@mail.local` steht didaktisch für Bibliotheks-/Mitarbeiterrechte.
- Die technische Identität wird später mit fachlichen Entitäten verknüpft.
*/
