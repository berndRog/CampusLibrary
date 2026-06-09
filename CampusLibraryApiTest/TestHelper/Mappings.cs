using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
namespace CampusLibraryApiTest.TestHelper.Mappings;

public static class Mappings {
   
   public static ReaderCreateDto ToReaderCreateDto(Reader reader) => new(
      Firstname: reader.Firstname,
      Lastname: reader.Lastname,
      Email: reader.EmailVo.Value,
      AddressDto: reader.AddressVo.ToAddressDto(),
      Subject: reader.Subject,
      Id: reader.Id.ToString()
   );
   
   public static ReaderUpdateDto ToReaderUpdateDto(Reader reader) => new( 
      Lastname: reader.Lastname,
      Email: reader.EmailVo.Value,
      AddressDto: new AddressDto(
         Street: "Hauptstr. 23",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      )
   );
   
}