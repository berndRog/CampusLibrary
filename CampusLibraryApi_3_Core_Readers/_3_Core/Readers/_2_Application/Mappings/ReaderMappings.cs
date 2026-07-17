using CampusLibraryApi._2_BuildingBlocks._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Readers._2_Application.Mappings;

public static class ReaderMappings {
   
   public static ReaderDto ToReaderDto(this Reader reader) => new(
      Id: reader.Id,
      Firstname: reader.Firstname,
      Lastname: reader.Lastname,
      Email: reader.EmailVo.Value,
      AddressDto: reader.AddressVo.ToAddressDto(),
      IsActive: reader.IsActive,
      Subject: reader.Subject
   );
   
   public static ReaderLoanInfoDto ToReaderLoanInfoDto(this Reader reader) =>
      new(
         Id: reader.Id,
         Firstname: reader.Firstname,
         Lastname: reader.Lastname,
         Email: reader.EmailVo.Value,
         IsActive: reader.IsActive
      );
   
   public static AddressDto ToAddressDto(this AddressVo addressVo) => new AddressDto(
      Street: addressVo.Street,
      PostalCode: addressVo.PostalCode,
      City: addressVo.City,
      Country: addressVo.Country
   );
   
}