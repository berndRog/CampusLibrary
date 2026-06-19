using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class ReaderReadModelIntT : TestBaseIntegration {
   
   private static readonly DateTime DeactivatedAt =
      DateTime.Parse("2025-01-02T00:00:00Z").ToUniversalTime();

   public ReaderReadModelIntT() {
      DbName = nameof(ReaderReadModelIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(reader1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }

   [Fact]
   public async Task FindByIdAsync_inactive_reader_returns_not_found() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      reader1.Deactivate(updatedAt: DeactivatedAt);

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Inactive reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(reader1.Id, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task FindByIdWithInactiveAsync_inactive_reader_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      reader1.Deactivate(updatedAt: DeactivatedAt);

      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Inactive reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdWithInactiveAsync(reader1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }

   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByEmailAsync(reader1.EmailVo.Value, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }

   [Fact]
   public async Task FindByEmailAsync_inactive_reader_returns_not_found() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      reader1.Deactivate(updatedAt: DeactivatedAt);

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Inactive reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByEmailAsync(reader1.EmailVo.Value, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task FindBySubjectAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindBySubjectAsync(reader1.Subject, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }

   [Fact]
   public async Task FindBySubjectAsync_inactive_reader_returns_not_found() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      reader1.Deactivate(updatedAt: DeactivatedAt);

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Inactive reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindBySubjectAsync(reader1.Subject, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task SelectAllAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;

      repository.AddRange(readers);
      await unitOfWork.SaveAllChangesAsync("Readers inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var expReaderDtos = readers
         .OrderBy(r => r.Id)
         .Select(r => r.ToReaderDto())
         .ToList();

      // Act
      var result = await readModel.SelectAllAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualReaderDtos = result.Value
         .OrderBy(r => r.Id)
         .ToList();

      actualReaderDtos.Should().NotBeNull();
      actualReaderDtos.Count.Should().Be(6);
      actualReaderDtos.Should().BeEquivalentTo(expReaderDtos);
   }

   [Fact]
   public async Task SelectAllAsync_returns_only_active_readers() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers.ToList();
      var inactiveReader = readers[0];
      inactiveReader.Deactivate(updatedAt: DeactivatedAt);

      repository.AddRange(readers);
      await unitOfWork.SaveAllChangesAsync("Readers inserted", ct);
      unitOfWork.ClearChangeTracker();

      var expReaderDtos = readers
         .Where(r => r.IsActive)
         .OrderBy(r => r.Id)
         .Select(r => r.ToReaderDto())
         .ToList();

      // Act
      var result = await readModel.SelectAllAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualReaderDtos = result.Value
         .OrderBy(r => r.Id)
         .ToList();

      actualReaderDtos.Count.Should().Be(5);
      actualReaderDtos.Should().BeEquivalentTo(expReaderDtos);
      actualReaderDtos.Should().NotContain(r => r.Id == inactiveReader.Id);
   }

   [Fact]
   public async Task SelectAllWithInactiveAsync_returns_active_and_inactive_readers() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers.ToList();
      var inactiveReader = readers[0];
      inactiveReader.Deactivate(updatedAt: DeactivatedAt);

      repository.AddRange(readers);
      await unitOfWork.SaveAllChangesAsync("Readers inserted", ct);
      unitOfWork.ClearChangeTracker();

      var expReaderDtos = readers
         .OrderBy(r => r.Id)
         .Select(r => r.ToReaderDto())
         .ToList();

      // Act
      var result = await readModel.SelectAllWithInactiveAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualReaderDtos = result.Value
         .OrderBy(r => r.Id)
         .ToList();

      actualReaderDtos.Count.Should().Be(6);
      actualReaderDtos.Should().BeEquivalentTo(expReaderDtos);
      actualReaderDtos.Should().Contain(r => r.Id == inactiveReader.Id);
   }
}