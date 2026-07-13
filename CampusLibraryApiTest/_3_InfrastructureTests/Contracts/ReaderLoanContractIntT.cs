using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.Contracts;

public sealed class ReaderLoanContractIntT : TestBaseIntegration {
   public ReaderLoanContractIntT() {
      DbName = nameof(ReaderLoanContractIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindActiveReaderForLoanAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var contract = scope.ServiceProvider.GetRequiredService<IReaderLoanContract>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var reader1 = readers[0];

      readerRepository.AddRange(
         readers: readers
      );

      await unitOfWork.SaveAllChangesAsync(
         "Readers inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await contract.FindReaderForLoanAsync(
         readerId: reader1.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualReaderDto = result.Value;

      actualReaderDto.Should().NotBeNull();
      actualReaderDto.Id.Should().Be(reader1.Id);
      actualReaderDto.Firstname.Should().Be(reader1.Firstname);
      actualReaderDto.Lastname.Should().Be(reader1.Lastname);
      actualReaderDto.IsActive.Should().BeTrue();
   }

   [Fact]
   public async Task FindActiveReaderForLoanAsync_empty_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<IReaderLoanContract>();

      // Act
      var result = await contract.FindReaderForLoanAsync(
         readerId: Guid.Empty,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.ReaderIdRequired);
   }

   [Fact]
   public async Task FindActiveReaderForLoanAsync_unknown_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<IReaderLoanContract>();

      // Arrange
      var unknownReaderId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var result = await contract.FindReaderForLoanAsync(
         readerId: unknownReaderId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.ReaderNotFound);
   }

   [Fact]
   public async Task FindActiveReaderForLoanAsync_deactivated_reader_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var contract = scope.ServiceProvider.GetRequiredService<IReaderLoanContract>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();

      readerRepository.Add(
         reader: reader1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Reader inserted",
         ct
      );

      var resultDeactivated = reader1.Deactivate(
         updatedAt: reader1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Reader deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await contract.FindReaderForLoanAsync(
         reader1.Id, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.ReaderIsDeactivated);
   }

   [Fact]
   public async Task FindReaderForExistingLoanAsync_deactivated_reader_returns_reader() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var contract = scope.ServiceProvider.GetRequiredService<IReaderLoanContract>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();
      var deactivateResult = reader.Deactivate(
         updatedAt: reader.CreatedAt.AddDays(1)
      );

      deactivateResult.IsSuccess.Should().BeTrue();
      readerRepository.Add(reader);

      await unitOfWork.SaveAllChangesAsync(
         "Deactivated reader inserted",
         ct
      );
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await contract.FindReaderForExistingLoanAsync(
         readerId: reader.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(reader.Id);
      result.Value.IsActive.Should().BeFalse();
   }
}