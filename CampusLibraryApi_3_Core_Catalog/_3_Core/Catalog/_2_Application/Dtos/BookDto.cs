namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   int BookItemCount
);