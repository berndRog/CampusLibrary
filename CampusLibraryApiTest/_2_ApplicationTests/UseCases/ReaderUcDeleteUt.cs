using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases;

public sealed class ReaderUcDeactivateUt {
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

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcDeactivate", ct))
         .ReturnsAsync(1);

      var clock = new Mock<IClock>();
      clock
         .Setup(c => c.UtcNow)
         .Returns(UpdatedAt);

      var uc = CreateUseCase(
         repository: repository,
         unitOfWork: unitOfWork,
         clock: clock
      );

      // Act
      var result = await uc.ExecuteAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      reader.IsActive.Should().BeFalse();
      reader.UpdatedAt.Should().Be(UpdatedAt);

      repository.Verify(
         r => r.FindByIdAsync(reader.Id, ct),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcDeactivate", ct),
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
      var clock = new Mock<IClock>();

      var uc = CreateUseCase(
         repository: repository,
         unitOfWork: unitOfWork,
         clock: clock
      );

      // Act
      var result = await uc.ExecuteAsync(
         id: readerId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      repository.Verify(
         r => r.FindByIdAsync(readerId, ct),
         Times.Once
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
      var clock = new Mock<IClock>();

      var uc = CreateUseCase(
         repository: repository,
         unitOfWork: unitOfWork,
         clock: clock
      );

      // Act
      var result = await uc.ExecuteAsync(
         id: Guid.Empty,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidId);

      repository.Verify(
         r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_current_loans_fails_with_conflict() {
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

      var loanReaderContract = new Mock<ILoanReaderContract>();
      loanReaderContract
         .Setup(c => c.ExistsForReaderAsync(reader.Id, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();
      var clock = new Mock<IClock>();

      var uc = CreateUseCase(
         repository: repository,
         unitOfWork: unitOfWork,
         clock: clock,
         loanReaderContract: loanReaderContract
      );

      // Act
      var result = await uc.ExecuteAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(
         ReaderErrors.ReaderCannotBeDeactivatedWithLoans
      );
      reader.IsActive.Should().BeTrue();

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_already_deactivated_reader_fails() {
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

      var deactivateResult = reader.Deactivate(
         updatedAt: UpdatedAt
      );

      deactivateResult.IsSuccess.Should().BeTrue();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();

      var clock = new Mock<IClock>();
      clock
         .Setup(c => c.UtcNow)
         .Returns(UpdatedAt);

      var uc = CreateUseCase(
         repository: repository,
         unitOfWork: unitOfWork,
         clock: clock
      );

      // Act
      var result = await uc.ExecuteAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.IsAlreadyDeactivated);

      reader.IsActive.Should().BeFalse();

      repository.Verify(
         r => r.FindByIdAsync(reader.Id, ct),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   private static ReaderUcDeactivate CreateUseCase(
      Mock<IReaderRepository> repository,
      Mock<IUnitOfWork> unitOfWork,
      Mock<IClock> clock,
      Mock<ILoanReaderContract>? loanReaderContract = null
   ) => new(
      repository: repository.Object,
      loanReaderContract: loanReaderContract?.Object
         ?? Mock.Of<ILoanReaderContract>(),
      unitOfWork: unitOfWork.Object,
      clock: clock.Object,
      logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
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