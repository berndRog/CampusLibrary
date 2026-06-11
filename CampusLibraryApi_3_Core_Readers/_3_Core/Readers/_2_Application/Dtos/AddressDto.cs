namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);