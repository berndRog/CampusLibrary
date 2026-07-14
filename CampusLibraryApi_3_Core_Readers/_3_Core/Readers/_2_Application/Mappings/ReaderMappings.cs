using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._2_Application.Mappings;

public static class ReaderMappings {
   public static ReaderDto ToReaderDto(this Reader reader) => new(
      Id: reader.Id,
      Firstname: NullIfEmpty(reader.Firstname),
      Lastname: NullIfEmpty(reader.Lastname),
      Email: reader.EmailVo.Value,
      AddressDto: reader.AddressVo.ToAddressDto(),
      IsActive: reader.IsActive,
      IsProfileCompleted: reader.IsProfileCompleted
   );

   public static ReaderLoanInfoDto ToReaderLoanInfoDto(this Reader reader) => new(
      Id: reader.Id,
      Firstname: reader.Firstname,
      Lastname: reader.Lastname,
      Email: reader.EmailVo.Value,
      IsActive: reader.IsActive,
      IsProfileCompleted: reader.IsProfileCompleted
   );

   public static AddressDto? ToAddressDto(this AddressVo? addressVo) =>
      addressVo is null
         ? null
         : new AddressDto(
            Street: addressVo.Street,
            PostalCode: addressVo.PostalCode,
            City: addressVo.City,
            Country: addressVo.Country
         );

   private static string? NullIfEmpty(string value) =>
      string.IsNullOrWhiteSpace(value) ? null : value;
}
