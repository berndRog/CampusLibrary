using AwesomeAssertions;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.Repositories;

public sealed class ReaderRepositoryIntT : TestBaseIntegration {

   public ReaderRepositoryIntT() {
      DbName = nameof(ReaderRepositoryIntT);
      DbMode = DbMode.FileUnique;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualReader = await repository.FindByIdAsync(reader1.Id, ct);

      // Assert
      actualReader.Should().NotBeNull();
      // actualReader!.Id.Should().Be(reader1.Id);
      // actualReader.Firstname.Should().Be(reader1.Firstname);
      // actualReader.Lastname.Should().Be(reader1.Lastname);
      // actualReader.Subject.Should().Be(reader1.Subject);
      // actualReader.EmailVo.Value.Should().Be(reader1.EmailVo.Value);
      // actualReader.AddressVo.Should().BeEquivalentTo(reader1.AddressVo);
      // actualReader.CreatedAt.Should().Be(reader1.CreatedAt);
      // actualReader.UpdatedAt.Should().Be(reader1.UpdatedAt);
      // Structural comparison:
      // verifies that the persisted and reloaded aggregate contains the same data.
      actualReader.Should().BeEquivalentTo(reader1);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var actualReader = await repository.FindByIdAsync(unknownId, ct);

      // Assert
      actualReader.Should().BeNull();
   }

   [Fact]
   public async Task FindBySubjectAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualReader = await repository.FindBySubjectAsync(reader1.Subject, ct);

      // Assert
      actualReader.Should().NotBeNull();
      actualReader.Should().BeEquivalentTo(reader1);
   }

   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualReader = await repository.FindByEmailAsync(reader1.EmailVo, ct);

      // Assert
      actualReader.Should().NotBeNull();
      actualReader.Should().BeEquivalentTo(reader1);
   }
   
   [Fact]
   public async Task FindByEmailAsync_unknown_email_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();

      // Arrange
      var unknownEmail = EmailVo.Create("unknown.reader@example.com").GetValueOrThrow();

      // Act
      var actualReader = await repository.FindByEmailAsync(unknownEmail, ct);

      // Assert
      actualReader.Should().BeNull();
   }

   [Fact]
   public async Task ExistsBySubjectAsync_existing_subject_returns_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var exists = await repository.ExistsBySubjectAsync(reader1.Subject, ct);

      // Assert
      exists.Should().BeTrue();
   }

   [Fact]
   public async Task ExistsBySubjectAsync_unknown_subject_returns_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();

      // Arrange
      var unknownSubject = "99999999-0000-0000-0000-000000000000";

      // Act
      var exists = await repository.ExistsBySubjectAsync(unknownSubject, ct);

      // Assert
      exists.Should().BeFalse();
   }

   [Fact]
   public async Task AddRange_persists_multiple_readers() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;

      repository.AddRange(readers);
      var savedRows = await unitOfWork.SaveAllChangesAsync("Three readers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualReader1 = await repository.FindByIdAsync(readers[0].Id, ct);
      var actualReader2 = await repository.FindByIdAsync(readers[1].Id, ct);
      var actualReader3 = await repository.FindByIdAsync(readers[2].Id, ct);

      // Assert
      savedRows.Should().BeGreaterThan(0);
      actualReader1.Should().NotBeNull();
      actualReader2.Should().NotBeNull();
      actualReader3.Should().NotBeNull();

      actualReader1!.Id.Should().Be(readers[0].Id);
      actualReader2!.Id.Should().Be(readers[1].Id);
      actualReader3!.Id.Should().Be(readers[2].Id);
   }
}
