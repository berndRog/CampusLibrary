namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookDeactivationLoanDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);
