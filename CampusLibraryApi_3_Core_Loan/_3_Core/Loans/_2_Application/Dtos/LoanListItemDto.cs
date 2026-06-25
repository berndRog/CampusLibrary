namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

// API Response Dto
public sealed record LoanListItemDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,

   Guid BookItemId,
   string InventoryNumber,

   string Title,
   string? Subtitle,

   DateTime LoanDate,
   DateTime DueDate,

   int Status,
   bool IsOverdue
);