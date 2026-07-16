using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Mock;

public sealed class ReaderUseCasesMockT {
   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);

   #region ReaderUcDeactivate

   [Fact]
   public async Task DeactivateAsync_ok_deactivates_reader() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcDeactivate", ct))
         .ReturnsAsync(1);

      var sut = new ReaderUcDeactivate(
         repository: repository.Object,
         loanReaderContract: Mock.Of<ILoanReaderContract>(),
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      reader.IsActive.Should().BeFalse();

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcDeactivate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task DeactivateAsync_unknown_reader_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var unknownId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(unknownId, ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcDeactivate(
         repository: repository.Object,
         loanReaderContract: Mock.Of<ILoanReaderContract>(),
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_empty_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcDeactivate(
         repository: repository.Object,
         loanReaderContract: Mock.Of<ILoanReaderContract>(),
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
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
         u => u.SaveAllChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_with_current_loans_returns_conflict() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var loanReaderContract = new Mock<ILoanReaderContract>();
      loanReaderContract
         .Setup(c => c.ExistsForReaderAsync(reader.Id, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcDeactivate(
         repository: repository.Object,
         loanReaderContract: loanReaderContract.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
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
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_already_deactivated_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();
      var deactivateResult = reader.Deactivate(UpdatedAt);
      deactivateResult.IsSuccess.Should().BeTrue();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcDeactivate(
         repository: repository.Object,
         loanReaderContract: Mock.Of<ILoanReaderContract>(),
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcDeactivate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.IsAlreadyDeactivated);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   #endregion
}
