namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);

// Response Dto
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string Email,
   AddressDto? AddressDto,
   bool IsActive,
   bool IsProfileCompleted
);

// Administrative creation contract to support deterministic tests.
// Request Dto
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id = null
);

// Initial self-service profile completion after provisioning.
// Request Dto
public sealed record ReaderProfileDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);

// Later self-service update.
// Request Dto
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
