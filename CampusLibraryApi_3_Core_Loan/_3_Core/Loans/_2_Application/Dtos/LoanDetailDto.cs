namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

// API Response Dto
public sealed record LoanDetailDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,
   string Email,

   Guid BookItemId,

   Guid BookId,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,
   
   DateTime LoanDate,
   DateTime DueDate,
   DateTime? ReturnedAt,

   int Status,
   int RenewalCount,

   bool IsOverdue,
   bool CanRenew
);