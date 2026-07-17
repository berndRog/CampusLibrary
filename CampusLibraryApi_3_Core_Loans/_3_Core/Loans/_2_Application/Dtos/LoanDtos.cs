namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

// Unified list and detail view for a current Loan.
public sealed record LoanDto(
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
   int RenewalCount,

   bool IsOverdue,
   bool CanRenew
);

// Id remains optional to support deterministic tests.
public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
