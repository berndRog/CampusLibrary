using Microsoft.Extensions.Configuration;

namespace CampusLibraryClient.Security;

// Provides a demo user for Part 5 without real authentication.
public sealed class DevCurrentUserProvider(
   IConfiguration configuration
) : ICurrentUserProvider {

   public Task<CurrentUserInfo> GetCurrentUserAsync(
      CancellationToken ct = default
   ) {
      string activeProfile =
         configuration["DevIdentity:ActiveProfile"]
         ?? throw new InvalidOperationException(
            "Missing configuration value: DevIdentity:ActiveProfile"
         );

      IConfigurationSection profileSection =
         configuration.GetSection($"DevIdentity:Profiles:{activeProfile}");

      if(!profileSection.Exists())
         throw new InvalidOperationException(
            $"DevIdentity profile '{activeProfile}' was not found."
         );

      bool isAuthenticated =
         profileSection.GetValue<bool>("IsAuthenticated");

      string accountType =
         profileSection["AccountType"]
         ?? throw new InvalidOperationException(
            $"Missing AccountType for DevIdentity profile '{activeProfile}'."
         );

      string displayName =
         profileSection["DisplayName"] ?? activeProfile;

      string email =
         profileSection["Email"] ?? string.Empty;

      string? readerIdText =
         profileSection["ReaderId"];

      Guid? readerId = null;

      if(!string.IsNullOrWhiteSpace(readerIdText)) {
         if(!Guid.TryParse(
               input: readerIdText,
               result: out Guid parsedReaderId
            )) {
            throw new InvalidOperationException(
               $"Invalid ReaderId for DevIdentity profile '{activeProfile}': {readerIdText}"
            );
         }

         readerId = parsedReaderId;
      }

      if(accountType.Equals(
            value: CampusLibraryRoles.Reader,
            comparisonType: StringComparison.OrdinalIgnoreCase
         )
         && readerId is null) {
         throw new InvalidOperationException(
            $"DevIdentity profile '{activeProfile}' is a reader but has no ReaderId."
         );
      }

      CurrentUserInfo currentUser = new(
         IsAuthenticated: isAuthenticated,
         AccountType: accountType,
         ReaderId: readerId,
         DisplayName: displayName,
         Email: email
      );

      return Task.FromResult(currentUser);
   }
}

/*
   Lernziele und Didaktik
   ----------------------

   Dieser Provider simuliert in Teil 5 eine angemeldete Identität, ohne bereits
   echte Authentifizierung über den IdentityAccessServer zu verwenden.

   Die aktive Demo-Identität wird über DevIdentity:ActiveProfile ausgewählt.
   Dadurch bleibt die appsettings.json gültiges JSON. Es müssen keine Blöcke
   auskommentiert oder umbenannt werden.

   Für Reader-Profile ist eine ReaderId erforderlich, weil die CampusLibraryApi
   fachliche Reader kennt und Ausleihen einem Reader zugeordnet werden.

   Für Employee-Profile ist keine ReaderId erforderlich. Mitarbeiter gehören
   in diesem didaktischen Modell nicht zur CampusLibrary-Domäne, sondern später
   zum IdentityAccessServer.

   In Teil 6 kann dieser Provider durch einen ClaimsCurrentUserProvider ersetzt
   werden. Die UI-Seiten bleiben dabei weitgehend unverändert, weil sie nur gegen
   ICurrentUserProvider programmiert sind.
*/