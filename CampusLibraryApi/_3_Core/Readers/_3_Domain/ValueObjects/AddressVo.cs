using System.ComponentModel.DataAnnotations.Schema;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

// Address is a value object without identity.
// It is immutable and fully replaced on change.
[ComplexType]
public sealed record AddressVo {
   // Properties
   public string Street { get; private init; } = string.Empty;
   public string PostalCode { get; private init; } = string.Empty;
   public string City { get; private init; } = string.Empty;
   public string? Country { get; private init; }

   // Ctor required by EF Core.
   private AddressVo() {
   }
   // Domain ctor
   private AddressVo(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      Street = street;
      PostalCode = postalCode;
      City = city;
      Country = country;
   }

   //--- Static factory method -------------------------------------------------
   public static Result<AddressVo> Create(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      // Normalize input early
      street = street.Trim();
      postalCode = postalCode.Trim();
      city = city.Trim();
      country = country?.Trim();

      if (string.IsNullOrWhiteSpace(street))
         return Result<AddressVo>.Failure(ReaderErrors.StreetIsRequired);
      if (street.Length is < 2 or > 80)
         return Result<AddressVo>.Failure(ReaderErrors.InvalidStreet);

      if (string.IsNullOrWhiteSpace(postalCode))
         return Result<AddressVo>.Failure(ReaderErrors.PostalCodeIsRequired);
      if (postalCode.Length is < 2 or > 10)
         return Result<AddressVo>.Failure(ReaderErrors.InvalidPostalCode);

      if (string.IsNullOrWhiteSpace(city))
         return Result<AddressVo>.Failure(ReaderErrors.CityIsRequired);
      if (city.Length is < 2 or > 80)
         return Result<AddressVo>.Failure(ReaderErrors.InvalidCity);

      if (country?.Length is < 2 or > 80)
         return Result<AddressVo>.Failure(ReaderErrors.InvalidCountry);

      return Result<AddressVo>.Success(new AddressVo(street, postalCode, city, country));
   }
}