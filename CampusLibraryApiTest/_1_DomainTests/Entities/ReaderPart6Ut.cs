using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApiTest._1_DomainTests.Entities;

public sealed class ReaderPart6Ut {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);

   [Fact]
   public void Provision_ok_creates_incomplete_reader_profile() {
      // Act
      var result = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: " identity-subject-001 ",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      var reader = result.Value;
      reader.Subject.Should().Be("identity-subject-001");
      reader.EmailVo.Value.Should().Be("reader.one@example.org");
      reader.Firstname.Should().BeEmpty();
      reader.Lastname.Should().BeEmpty();
      reader.AddressVo.Should().BeNull();
      reader.IsActive.Should().BeTrue();
      reader.IsProfileCompleted.Should().BeFalse();
      reader.CreatedAt.Should().Be(CreatedAt);
      reader.UpdatedAt.Should().Be(CreatedAt);
   }

   [Fact]
   public void Provision_empty_subject_fails() {
      // Act
      var result = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: "   ",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.SubjectRequired);
   }

   [Fact]
   public void Provision_createdAt_not_utc_fails() {
      // Act
      var result = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: "identity-subject-001",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Local)
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(AggregateErrors.CreatedAtMustBeUtc);
   }

   [Fact]
   public void UpdateMyProfile_ok_completes_profile_with_address() {
      // Arrange
      var reader = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: "identity-subject-001",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: CreatedAt
      ).GetValueOrThrow();

      var addressVo = CreateAddress();

      // Act
      var result = reader.UpdateMyProfile(
         firstname: " Alice ",
         lastname: " Reader ",
         addressVo: addressVo,
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      reader.Firstname.Should().Be("Alice");
      reader.Lastname.Should().Be("Reader");
      reader.AddressVo.Should().BeEquivalentTo(addressVo);
      reader.EmailVo.Value.Should().Be("reader.one@example.org");
      reader.Subject.Should().Be("identity-subject-001");
      reader.IsProfileCompleted.Should().BeTrue();
      reader.UpdatedAt.Should().Be(UpdatedAt);
   }

   [Fact]
   public void UpdateMyProfile_null_address_fails() {
      // Arrange
      var reader = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: "identity-subject-001",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: CreatedAt
      ).GetValueOrThrow();

      // Act
      var result = reader.UpdateMyProfile(
         firstname: "Alice",
         lastname: "Reader",
         addressVo: null!,
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.AddressIsRequired);
      reader.IsProfileCompleted.Should().BeFalse();
   }

   [Fact]
   public void UpdateMyProfile_empty_firstname_fails() {
      // Arrange
      var reader = Reader.Provision(
         id: Guid.Parse("10000000-0000-0000-0000-000000000001"),
         subject: "identity-subject-001",
         emailVo: CreateEmail("reader.one@example.org"),
         createdAt: CreatedAt
      ).GetValueOrThrow();

      // Act
      var result = reader.UpdateMyProfile(
         firstname: "   ",
         lastname: "Reader",
         addressVo: CreateAddress(),
         updatedAt: UpdatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.FirstnameIsRequired);
      reader.IsProfileCompleted.Should().BeFalse();
   }

   private static EmailVo CreateEmail(string email) =>
      EmailVo.Create(email).GetValueOrThrow();

   private static AddressVo CreateAddress() =>
      AddressVo.Create(
         street: "Profilstraße 1",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).GetValueOrThrow();
}
