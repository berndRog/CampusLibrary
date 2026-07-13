namespace CampusLibraryClient.Api.Dtos;

// DTO for the later self-service update of the currently authenticated reader.
//
// Used by:
// PUT /readers/me/update
//
// Firstname is intentionally not part of this DTO.
// The technical username in the IdentityAccessServer is not changed.
public sealed record ReaderUpdateMeDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
