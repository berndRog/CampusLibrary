namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

// API Response Dto
public sealed record BookDto(
   Guid Id,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   int BookItemCount,
   bool IsActive
);