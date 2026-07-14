namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

// Current loan data published by Loans for the Catalog bounded context.
public sealed record CurrentBookItemLoanInfoDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);
