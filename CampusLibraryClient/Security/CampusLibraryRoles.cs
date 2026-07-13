namespace CampusLibraryClient.Security;

public static class CampusLibraryRoles {

   public const string Reader = "Reader";

   public const string Employee = "Employee";

   public const string ReaderOrEmployee = Reader + "," + Employee;
}
