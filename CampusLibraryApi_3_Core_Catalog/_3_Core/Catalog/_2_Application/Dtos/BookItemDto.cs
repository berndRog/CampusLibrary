namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);