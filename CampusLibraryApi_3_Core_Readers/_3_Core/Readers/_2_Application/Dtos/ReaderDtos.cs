namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);

// Public Reader view. A provisioned Reader may still have an incomplete profile.
// Null therefore has one clear meaning: the profile value has not been supplied yet.
public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string Email,
   AddressDto? AddressDto,
   bool IsActive,
   bool IsProfileCompleted
);

// Administrative creation contract. Subject links the technical identity to the
// fachlicher Reader. Id remains optional to support deterministic tests.
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id = null
);

// Initial self-service profile completion after provisioning.
// Subject and initial fachliche email come from the authenticated identity flow.
public sealed record ReaderProfileDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);

// Later self-service update. Null means that the corresponding value is unchanged.
// Firstname and Subject are intentionally not mutable through this command.
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
