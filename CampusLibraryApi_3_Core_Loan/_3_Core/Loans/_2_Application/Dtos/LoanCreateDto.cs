namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

// API request Dto
public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
