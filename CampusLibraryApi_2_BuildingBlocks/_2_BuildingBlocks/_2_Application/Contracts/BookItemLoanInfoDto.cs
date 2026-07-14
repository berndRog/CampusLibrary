namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

// Catalog data published by Catalog for the Loans bounded context.
public sealed record BookItemLoanInfoDto(
   Guid BookItemId,
   Guid BookId,
   string Title,
   string? Subtitle,
   string AuthorsText,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan
);
