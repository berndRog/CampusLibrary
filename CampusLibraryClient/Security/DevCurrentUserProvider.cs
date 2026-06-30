namespace CampusLibraryClient.Security;

public sealed class DevCurrentUserProvider(
   IConfiguration configuration
) : ICurrentUserProvider {

   public Task<CurrentUserInfo> GetCurrentUserAsync(
      CancellationToken ct = default
   ) {
      IConfigurationSection section = configuration.GetSection("DevIdentity");

      bool isAuthenticated = section.GetValue(
         key: "IsAuthenticated",
         defaultValue: true
      );

      if(!isAuthenticated)
         return Task.FromResult(CurrentUserInfo.Anonymous);

      string accountType = section.GetValue<string>("AccountType")
         ?? CampusLibraryRoles.Reader;

      Guid? readerId = null;
      string? readerIdText = section.GetValue<string>("ReaderId");

      if(Guid.TryParse(
            input: readerIdText,
            result: out Guid parsedReaderId
         ))
         readerId = parsedReaderId;

      string displayName = section.GetValue<string>("DisplayName")
         ?? "Dev user";

      string? email = section.GetValue<string>("Email");

      CurrentUserInfo user = new(
         IsAuthenticated: true,
         AccountType: accountType,
         ReaderId: readerId,
         DisplayName: displayName,
         Email: email
      );

      return Task.FromResult(user);
   }
}

/*
Didaktik
--------

DevCurrentUserProvider ist eine bewusste Übergangslösung für Teil 5.

Der Client soll bereits unterschiedliche UI-Perspektiven zeigen können:
Reader sehen ihre eigenen Ausleihen und können Bücher ausleihen.
Mitarbeiter sehen Verwaltungsseiten wie Readers und alle Loans.

Diese Klasse ersetzt keine Authentifizierung und bietet keine Sicherheit.
Sie liest nur eine Demo-Identität aus appsettings.json.

In Teil 6 wird diese Quelle durch ClaimsCurrentUserProvider ersetzt.
Die Razor-Seiten müssen dann nicht neu gedacht werden, weil sie nur das
Interface ICurrentUserProvider verwenden.
*/
