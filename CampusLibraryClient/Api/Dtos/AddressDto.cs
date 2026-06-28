namespace CampusLibraryClient.Api.Dtos;

public sealed record AddressDto(
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
