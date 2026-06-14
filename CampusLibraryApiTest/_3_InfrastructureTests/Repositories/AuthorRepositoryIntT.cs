using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._3_InfrastructureTests.Repositories;

public sealed class AuthorRepositoryIntT : TestBaseIntegration {
   public AuthorRepositoryIntT() {
      DbName = nameof(AuthorRepositoryIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();
      repository.Add(author1);

      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualAuthor = await repository.FindByIdAsync(author1.Id, ct);

      // Assert
      actualAuthor.Should().NotBeNull();
      actualAuthor.Should().BeEquivalentTo(author1);
      
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var actualAuthor = await repository.FindByIdAsync(unknownId, ct);

      // Assert
      actualAuthor.Should().BeNull();
   }

   [Fact]
   public async Task ExistsByNameAsync_existing_author_returns_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();

      repository.Add(author1);

      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var exists = await repository.ExistsByNameAsync(
         firstname: author1.Firstname,
         lastname: author1.Lastname,
         ct: ct
      );

      // Assert
      exists.Should().BeTrue();
   }

   [Fact]
   public async Task ExistsByNameAsync_unknown_author_returns_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

      // Act
      var exists = await repository.ExistsByNameAsync(
         firstname: "Unknown",
         lastname: "Author",
         ct: ct
      );

      // Assert
      exists.Should().BeFalse();
   }

   [Fact]
   public async Task AddRange_persists_multiple_authors() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      repository.AddRange(authors);

      var savedRows = await unitOfWork.SaveAllChangesAsync("Authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualAuthor1 = await repository.FindByIdAsync(
         id: authors[0].Id,
         ct: ct
      );

      var actualAuthor2 = await repository.FindByIdAsync(
         id: authors[1].Id,
         ct: ct
      );

      var actualAuthor3 = await repository.FindByIdAsync(
         id: authors[2].Id,
         ct: ct
      );

      // Assert
      savedRows.Should().BeGreaterThan(0);

      actualAuthor1.Should().NotBeNull();
      actualAuthor2.Should().NotBeNull();
      actualAuthor3.Should().NotBeNull();

      actualAuthor1!.Id.Should().Be(authors[0].Id);
      actualAuthor2!.Id.Should().Be(authors[1].Id);
      actualAuthor3!.Id.Should().Be(authors[2].Id);
      
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_author_returns_author_with_is_active_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();
      var updatedAt = author1.CreatedAt.AddDays(1);

      repository.Add(author1);
      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);
      
      var resultDeactivated = author1.Deactivate(updatedAt);
      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Author1 deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualAuthor = await repository.FindByIdAsync(
         id: author1.Id,
         ct: ct
      );

      // Assert
      actualAuthor.Should().NotBeNull();
      actualAuthor!.IsActive.Should().BeFalse();
      actualAuthor.UpdatedAt.Should().Be(updatedAt);
   }

}