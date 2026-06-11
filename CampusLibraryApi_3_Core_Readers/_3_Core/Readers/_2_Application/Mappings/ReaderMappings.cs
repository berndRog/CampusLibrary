using System.Linq.Expressions;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Readers._2_Application.Mappings;

public static class ReaderMappings {
   
   public static ReaderDto ToReaderDto(this Reader reader) => new(
      Id: reader.Id,
      Subject: reader.Subject,
      Firstname: reader.Firstname,
      Lastname: reader.Lastname,
      Email: reader.EmailVo.Value,
      AddressDto: reader.AddressVo.ToAddressDto()
   );
   
   // when using in EFCore Select
   public static readonly Expression<Func<Reader, ReaderDto>> ToReaderDtoExpr =
      reader => new ReaderDto(
         reader.Id,
         reader.Subject,
         reader.Firstname,
         reader.Lastname,
         reader.EmailVo.Value,
         new AddressDto(
            reader.AddressVo.Street,
            reader.AddressVo.PostalCode,
            reader.AddressVo.City,
            reader.AddressVo.Country
         )
      );
   
   
   public static AddressDto ToAddressDto(this AddressVo addressVo) => new AddressDto(
      Street: addressVo.Street,
      PostalCode: addressVo.PostalCode,
      City: addressVo.City,
      Country: addressVo.Country
   );

   
}