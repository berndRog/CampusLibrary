using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

public sealed record LoanDetailDto(
   Guid Id,
   Guid ReaderId,
   string ReaderDisplayName,
   Guid BookItemId,
   string InventoryNumber,
   Guid BookId,
   string Title,
   string? Subtitle,
   string Isbn,
   DateTime LoanDate,
   DateTime DueDate,
   DateTime? ReturnedAt,
   LoanStatus Status,
   int RenewalCount,
   bool IsOverdue,
   bool CanRenew
);
