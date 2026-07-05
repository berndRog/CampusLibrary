namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto? AddressDto,
   string Subject,
   string? Id
);
