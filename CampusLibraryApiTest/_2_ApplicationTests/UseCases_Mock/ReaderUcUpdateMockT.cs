using AwesomeAssertions;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases;

public sealed class ReaderUcUpdateMockT {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);

   [Fact]
   public async Task ExecuteAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = CreateReader(
         id: Guid.Parse("10000000-0000-0000-0000-000000000000"),
         firstname: "Erika",
         lastname: "Mustermann",
         email: "erika.mustermann@example.com",
         subject: "subject-001",
         createdAt: CreatedAt
      );
      var dto = CreateUpdateDto(
         firstname: "Erna",
         lastname: "Musterfrau",
         email: "ERNA.MUSTERFRAU@EXAMPLE.COM"
      );

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcUpdate", ct))
         .ReturnsAsync(1);

      var uc = CreateUseCase(repository, unitOfWork, UpdatedAt);

      // Act
      var result = await uc.ExecuteAsync(reader.Id, dto, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(reader.Id);
      result.Value.Firstname.Should().Be("Erna");
      result.Value.Lastname.Should().Be("Musterfrau");
      result.Value.Email.Should().Be("erna.musterfrau@example.com");

      reader.Firstname.Should().Be("Erna");
      reader.Lastname.Should().Be("Musterfrau");
      reader.EmailVo.Value.Should().Be("erna.musterfrau@example.com");
      reader.UpdatedAt.Should().Be(UpdatedAt);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcUpdate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task ExecuteAsync_reader_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var readerId = Guid.Parse("10000000-0000-0000-0000-000000000000");
      var dto = CreateUpdateDto();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(readerId, ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork, UpdatedAt);

      // Act
      var result = await uc.ExecuteAsync(readerId, dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_duplicate_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = CreateReader(
         id: Guid.Parse("10000000-0000-0000-0000-000000000000"),
         firstname: "Erika",
         lastname: "Mustermann",
         email: "erika.mustermann@example.com",
         subject: "subject-001",
         createdAt: CreatedAt
      );
      var otherReader = CreateReader(
         id: Guid.Parse("20000000-0000-0000-0000-000000000000"),
         firstname: "Max",
         lastname: "Mustermann",
         email: "max.mustermann@example.com",
         subject: "subject-002",
         createdAt: CreatedAt
      );
      var dto = CreateUpdateDto(email: otherReader.EmailVo.Value);

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync(otherReader);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork, UpdatedAt);

      // Act
      var result = await uc.ExecuteAsync(reader.Id, dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_invalid_firstname_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = CreateReader(
         id: Guid.Parse("10000000-0000-0000-0000-000000000000"),
         firstname: "Erika",
         lastname: "Mustermann",
         email: "erika.mustermann@example.com",
         subject: "subject-001",
         createdAt: CreatedAt
      );
      var dto = CreateUpdateDto(firstname: "A");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork, UpdatedAt);

      // Act
      var result = await uc.ExecuteAsync(reader.Id, dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidFirstname);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   private static ReaderUcUpdate CreateUseCase(
      Mock<IReaderRepository> repository,
      Mock<IUnitOfWork> unitOfWork,
      DateTime utcNow
   ) => new(
      repository: repository.Object,
      unitOfWork: unitOfWork.Object,
      clock: new FakeClock(utcNow),
      logger: Mock.Of<ILogger<ReaderUcUpdate>>()
   );

   private static ReaderUpdateDto CreateUpdateDto(
      string firstname = "Erika",
      string lastname = "Mustermann",
      string email = "erika.mustermann@example.com"
   ) => new(
      Firstname: firstname,
      Lastname: lastname,
      Email: email,
      AddressDto: new AddressDto(
         Street: "Hauptstr. 23",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      )
   );

   private static Reader CreateReader(
      Guid id,
      string firstname,
      string lastname,
      string email,
      string subject,
      DateTime createdAt
   ) {
      var emailVo = EmailVo.Create(email).GetValueOrThrow();
      var addressVo = AddressVo.Create(
         street: "Hauptstr. 23",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).GetValueOrThrow();

      return Reader.Create(
         id: id,
         firstname: firstname,
         lastname: lastname,
         emailVo: emailVo,
         addressVo: addressVo,
         subject: subject,
         createdAt: createdAt
      ).GetValueOrThrow();
   }
}
