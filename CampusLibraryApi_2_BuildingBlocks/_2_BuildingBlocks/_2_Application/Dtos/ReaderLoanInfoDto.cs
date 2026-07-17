namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Dtos;

// Reader data published by Readers for the Loans bounded context.
public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive
// bool IsProfileCompleted
);
