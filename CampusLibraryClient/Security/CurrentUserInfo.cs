namespace CampusLibraryClient.Security;

public sealed record CurrentUserInfo(
   bool IsAuthenticated,
   string AccountType,
   Guid? ReaderId,
   string DisplayName,
   string? Email
) {

   public bool IsReader =>
      IsAuthenticated &&
      string.Equals(
         a: AccountType,
         b: CampusLibraryRoles.Reader,
         comparisonType: StringComparison.OrdinalIgnoreCase
      );

   public bool IsEmployee =>
      IsAuthenticated &&
      string.Equals(
         a: AccountType,
         b: CampusLibraryRoles.Employee,
         comparisonType: StringComparison.OrdinalIgnoreCase
      );

   public static CurrentUserInfo Anonymous =>
      new(
         IsAuthenticated: false,
         AccountType: "anonymous",
         ReaderId: null,
         DisplayName: "Anonymous",
         Email: null
      );
}
