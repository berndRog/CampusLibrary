namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookCreateDto(
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id
);