namespace CampusLibraryClient.Api.Dtos;

// DTO for completing the initial profile of the currently authenticated reader.
//
// Used by:
// PUT /readers/me/profile
//
// Subject and email are intentionally not part of this DTO.
// Subject comes from the access token. The initial fachliche reader email
// is taken from the technical username during provisioning.
public sealed record ReaderProfileMeDto(
   string Firstname,
   string Lastname,
   AddressDto AddressDto
);
