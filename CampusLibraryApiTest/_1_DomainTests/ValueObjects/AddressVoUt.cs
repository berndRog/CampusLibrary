using AwesomeAssertions;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
namespace CampusLibraryApiTest._1_DomainTests.ValueObjects;

public sealed class AddressVoUt {
   private readonly TestSeed _seed = default!;
   private readonly AddressVo _addressVo = default!;

   public AddressVoUt() {
      _seed = new TestSeed();
      _addressVo = _seed.Address1Vo;
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return ["A"]; // too short (1)
      yield return [new string('A', 81)]; // too long (81)
   }

   [Fact]
   public void EqualsUt() {
      // Arrange
      var addressVo1 = AddressVo.Create("Herbert-Meyer-Str.7", "29556", "Sudernburg", "DE").Value;
      var addressVo2 = AddressVo.Create("Herbert-Meyer-Str.7", "29556", "Sudernburg", "DE").Value;

      // Act & Assert
      addressVo1.Equals(addressVo1).Should().BeTrue();
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void Invalid_street_fails(string street) {
      // Act      
      var resultAddress = AddressVo.Create(
         street: street,
         postalCode: _addressVo.PostalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      resultAddress.IsFailure.Should().BeTrue();
      if (string.IsNullOrWhiteSpace(street))
         resultAddress.Error.Should().Be(ReaderErrors.StreetIsRequired);
      else
         resultAddress.Error.Should().Be(ReaderErrors.InvalidStreet);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("A")]
   [InlineData("AAAAAAAAAAA")]
   public void Invalid_postal_code_fails(string postalCode) {
      // Act      
      var resultAddress = AddressVo.Create(
         street: _addressVo.Street,
         postalCode: postalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      resultAddress.IsFailure.Should().BeTrue();
      if (string.IsNullOrWhiteSpace(postalCode))
         resultAddress.Error.Should().Be(ReaderErrors.PostalCodeIsRequired);
      else
         resultAddress.Error.Should().Be(ReaderErrors.InvalidPostalCode);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void Invalid_city_fails(string city) {
      // Act      
      var resultAddress = AddressVo.Create(
         street: _addressVo.Street,
         postalCode: _addressVo.PostalCode,
         city: city,
         country: _addressVo.Country
      );

      // Assert
      resultAddress.IsFailure.Should().BeTrue();
      if (string.IsNullOrWhiteSpace(city))
            resultAddress.Error.Should().Be(ReaderErrors.CityIsRequired);
         else
            resultAddress.Error.Should().Be(ReaderErrors.InvalidCity);
   }
}