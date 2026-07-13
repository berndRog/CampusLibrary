namespace CampusLibraryClient.Api.Dtos;

// Result DTO for the self-service provisioning step.
//
// Returned by:
// POST /readers/me/provision
//
// The DTO is intentionally small. The client uses it only to see whether
// provisioning created a new fachlicher Reader or returned an existing one.
// The full current Reader view is loaded separately through GET /readers/me.
public sealed record ReaderProvisionMeDto(
   Guid Id,
   bool WasCreated
);
