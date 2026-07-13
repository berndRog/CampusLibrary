namespace CampusLibraryClient.Api.Dtos;

public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookDeactivationLoanDto>? CurrentLoans
);
