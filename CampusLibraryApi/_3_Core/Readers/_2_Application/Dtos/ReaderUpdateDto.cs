namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderUpdateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto
);
