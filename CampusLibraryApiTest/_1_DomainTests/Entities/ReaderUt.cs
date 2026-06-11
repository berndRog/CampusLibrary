using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
namespace CampusLibraryApiTest._1_DomainTests.Entities;

public sealed class ReaderUt {

   private readonly TestSeed _seed;
   private readonly Reader _reader1;

   public ReaderUt() {
      _seed = new TestSeed();
      _reader1 = _seed.Reader1();
   }   
   
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);
   
   [Fact]
   public void Create_ok() {
      // Act
      var result = Reader.Create(
         id: _reader1.Id,
         firstname: _reader1.Firstname,
         lastname: _reader1.Lastname,
         emailVo: _reader1.EmailVo,
         addressVo: _reader1.AddressVo,
         subject: _reader1.Subject,
         createdAt: _reader1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actual = result.Value;
      actual.Id.Should().Be(_reader1.Id);
      actual.Firstname.Should().Be(_reader1.Firstname);
      actual.Lastname.Should().Be(_reader1.Lastname);
      actual.EmailVo.Should().Be(_reader1.EmailVo);
      actual.AddressVo.Should().Be(_reader1.AddressVo);
      actual.Subject.Should().Be(_reader1.Subject);
      actual.CreatedAt.Should().Be(CreatedAt);
      actual.UpdatedAt.Should().Be(CreatedAt);
   }

   [Fact]
   public void Create_empty_id_fails() {
      // Act
      var result = Reader.Create(
         id: Guid.Empty,
         firstname: _reader1.Firstname,
         lastname: _reader1.Lastname,
         emailVo: _reader1.EmailVo,
         addressVo: _reader1.AddressVo,
         subject: _reader1.Subject,
         createdAt: _reader1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.IdRequired);
   }

   [Fact]
   public void Create_empty_subject_fails() {
      // Act
      var result = Reader.Create(
         id: _reader1.Id,
         firstname: _reader1.Firstname,
         lastname: _reader1.Lastname,
         emailVo: _reader1.EmailVo,
         addressVo: _reader1.AddressVo,
         subject: "       ",
         createdAt: _reader1.CreatedAt
      );
      
      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.SubjectRequired);
   }

   [Fact]
   public void Create_invalid_firstname_fails() {
      // Act
      var result = CreateReader(firstname: "A");

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidFirstname);
   }

   [Fact]
   public void Create_empty_lastname_fails() {
      // Act
      var result = CreateReader(lastname: "   ");

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.LastnameIsRequired);
   }

   [Fact]
   public void Create_createdAt_default_fails() {
      // Act
      var result = Reader.Create(
         id: _reader1.Id,
         firstname: _reader1.Firstname,
         lastname: _reader1.Lastname,
         emailVo: _reader1.EmailVo,
         addressVo: _reader1.AddressVo,
         subject: _reader1.Subject,
         createdAt: default
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(AggregateErrors.CreatedAtRequired);
   }

   [Fact]
   public void Create_createdAt_not_utc_fails() {
      // Act
      var result = CreateReader(
         createdAt: new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Local)
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(AggregateErrors.CreatedAtMustBeUtc);
   }

   [Fact]
   public void UpdateProfile_ok() {
      // Arrange
      var reader = CreateReader().GetValueOrThrow();
      var newEmailVo = CreateEmail("e.meier@gmx.de");
      var newAddressVo = AddressVo.Create(
         street: "Neue Straße 5",
         postalCode: "30123",
         city: "Hannover",
         country: "DE"
      ).GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         lastname: " Meier ",
         emailVo: newEmailVo,
         addressVo: newAddressVo,
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      
      reader.Firstname.Should().Be("Erika");
      reader.Lastname.Should().Be("Meier");
      reader.EmailVo.Value.Should().Be("e.meier@gmx.de");
      reader.AddressVo.Should().BeEquivalentTo(newAddressVo);
      reader.UpdatedAt.Should().Be(UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_invalid_lastname_fails() {
      // Arrange
      var reader = CreateReader().GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         lastname: "M",
         emailVo: CreateEmail("e.meier@gmx.de"),
         addressVo: CreateAddress(),
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidLastname);
   }

   [Fact]
   public void UpdateProfile_updatedAt_before_createdAt_fails() {
      // Arrange
      var reader = CreateReader().GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         lastname: "Meier",
         emailVo: CreateEmail("e.meier@gmx.de"),
         addressVo: CreateAddress(),
         updatedAt: CreatedAt.AddDays(-1)
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(AggregateErrors.UpdatedAtBeforeCreatedAt);
      reader.Firstname.Should().Be("Erika");
   }

   private static Result<Reader> CreateReader(
      Guid? id = null,
      string firstname = "Erika",
      string lastname = "Mustermann",
      string email = "erika.mustermann@t-online.de",
      string subject = "subject-001",
      DateTime? createdAt = null
   ) => Reader.Create(
      id: id ?? Guid.Parse("10000000-0000-0000-0000-000000000000"),
      firstname: firstname,
      lastname: lastname,
      emailVo: CreateEmail(email),
      addressVo: CreateAddress(),
      subject: subject,
      createdAt: createdAt ?? CreatedAt
   );

   private static EmailVo CreateEmail(string email) =>
      EmailVo.Create(email).GetValueOrThrow();

   private static AddressVo CreateAddress() =>
      AddressVo.Create(
         street: "Hauptstr. 23",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).GetValueOrThrow();
}
