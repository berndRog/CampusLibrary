namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   AddressDto? AddressDto,
   bool IsActive,
   string Subject,
   bool IsProfileCompleted
);
