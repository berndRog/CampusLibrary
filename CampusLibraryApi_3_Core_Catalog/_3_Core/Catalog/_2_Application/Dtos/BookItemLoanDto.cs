namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookItemLoanInfoDto(
   Guid BookItemId,
   Guid BookId,
   string Title,
   string? Subtitle,
   string Isbn,
   string InventoryNumber
);