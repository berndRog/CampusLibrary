using AwesomeAssertions;
using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApiTest._1_DomainTests.Entities;

public sealed class ReaderUt {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);

   [Fact]
   public void Create_ok() {
      // Act
      var result = Reader.Create(
         id: Guid.Parse("10000000-0000-0000-0000-000000000000"),
         firstname: " Erika ",
         lastname: " Mustermann ",
         emailVo: CreateEmail("ERIKA.MUSTERMANN@EXAMPLE.COM"),
         addressVo: CreateAddress(),
         subject: " subject-001 ",
         createdAt: CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(Guid.Parse("10000000-0000-0000-0000-000000000000"));
      result.Value.Firstname.Should().Be("Erika");
      result.Value.Lastname.Should().Be("Mustermann");
      result.Value.EmailVo.Value.Should().Be("erika.mustermann@example.com");
      result.Value.Subject.Should().Be("subject-001");
      result.Value.CreatedAt.Should().Be(CreatedAt);
      result.Value.UpdatedAt.Should().Be(CreatedAt);
   }

   [Fact]
   public void Create_empty_id_fails() {
      // Act
      var result = CreateReader(id: Guid.Empty);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.IdRequired);
   }

   [Fact]
   public void Create_empty_subject_fails() {
      // Act
      var result = CreateReader(subject: "   ");

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
      var result = CreateReader(createdAt: default);

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
      var newEmailVo = CreateEmail("ERNA.MUSTERFRAU@EXAMPLE.COM");
      var newAddressVo = AddressVo.Create(
         street: "Neue Straße 5",
         postalCode: "30123",
         city: "Hannover",
         country: "DE"
      ).GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         firstname: " Erna ",
         lastname: " Musterfrau ",
         emailVo: newEmailVo,
         addressVo: newAddressVo,
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      reader.Firstname.Should().Be("Erna");
      reader.Lastname.Should().Be("Musterfrau");
      reader.EmailVo.Value.Should().Be("erna.musterfrau@example.com");
      reader.AddressVo.Should().BeEquivalentTo(newAddressVo);
      reader.UpdatedAt.Should().Be(UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_invalid_firstname_fails() {
      // Arrange
      var reader = CreateReader().GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         firstname: "A",
         lastname: "Musterfrau",
         emailVo: CreateEmail("erna.musterfrau@example.com"),
         addressVo: CreateAddress(),
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidFirstname);
      reader.Firstname.Should().Be("Erika");
   }

   [Fact]
   public void UpdateProfile_updatedAt_before_createdAt_fails() {
      // Arrange
      var reader = CreateReader().GetValueOrThrow();

      // Act
      var result = reader.UpdateProfile(
         firstname: "Erna",
         lastname: "Musterfrau",
         emailVo: CreateEmail("erna.musterfrau@example.com"),
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
      string email = "erika.mustermann@example.com",
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
