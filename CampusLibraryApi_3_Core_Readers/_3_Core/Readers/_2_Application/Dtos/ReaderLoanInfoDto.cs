namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   bool IsActive
);