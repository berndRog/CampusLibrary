namespace CampusLibraryClient.Api.Dtos;

public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);

public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string Email,
   AddressDto? AddressDto,
   bool IsActive,
   bool IsProfileCompleted
);

public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id = null
);

public sealed record ReaderProfileDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);

public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
