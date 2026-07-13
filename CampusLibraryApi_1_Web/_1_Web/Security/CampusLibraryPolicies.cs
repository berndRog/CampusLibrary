namespace CampusLibraryApi._1_Web.Security;

/// <summary>
/// Names of reusable authorization policies in CampusLibraryApi.
///
/// Policies complement direct role attributes such as:
/// [Authorize(Roles = "Employee")]
/// </summary>
public static class CampusLibraryPolicies {
   public const string ReadersOnly = "ReadersOnly";
   public const string EmployeesOnly = "EmployeesOnly";
   public const string ReadersOrEmployees = "ReadersOrEmployees";
}
