namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

// API Request Dto
public sealed record BookCreateDto(
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id
);