using AwesomeAssertions;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases;

public sealed class ReaderUcDeleteUt {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

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

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcDelete", ct))
         .ReturnsAsync(1);

      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(reader.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      repository.Verify(
         r => r.Remove(reader),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcDelete", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task ExecuteAsync_reader_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var readerId = Guid.Parse("10000000-0000-0000-0000-000000000000");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(readerId, ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(readerId, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      repository.Verify(
         r => r.Remove(It.IsAny<Reader>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_empty_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(Guid.Empty, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidId);

      repository.Verify(
         r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
         Times.Never
      );

      repository.Verify(
         r => r.Remove(It.IsAny<Reader>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   private static ReaderUcDelete CreateUseCase(
      Mock<IReaderRepository> repository,
      Mock<IUnitOfWork> unitOfWork
   ) => new(
      repository: repository.Object,
      unitOfWork: unitOfWork.Object,
      logger: Mock.Of<ILogger<ReaderUcDelete>>()
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
