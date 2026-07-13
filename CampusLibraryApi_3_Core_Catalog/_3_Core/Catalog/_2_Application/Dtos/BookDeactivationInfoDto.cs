namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookDeactivationLoanDto> CurrentLoans
);
