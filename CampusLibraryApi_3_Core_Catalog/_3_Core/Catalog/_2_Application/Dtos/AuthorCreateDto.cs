namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record AuthorCreateDto(
   string Firstname,
   string Lastname,
   string? Id
);