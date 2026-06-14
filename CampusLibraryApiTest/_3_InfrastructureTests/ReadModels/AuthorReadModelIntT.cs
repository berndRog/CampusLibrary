using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class AuthorReadModelIntT : TestBaseIntegration {

   public AuthorReadModelIntT() {
      DbName = nameof(AuthorReadModelIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();
      var expAuthorDto = author1.ToAuthorDto();

      repository.Add(author1);
      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(author1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualAuthorDto = result.Value;
      actualAuthorDto.Should().NotBeNull();
      actualAuthorDto.Should().BeEquivalentTo(expAuthorDto);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var result = await readModel.FindByIdAsync(unknownId, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_author_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();

      repository.Add(author1);
      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);

      var resultDeactivated = author1.Deactivate(updatedAt: author1.CreatedAt.AddDays(1));
      resultDeactivated.IsSuccess.Should().BeTrue();
      await unitOfWork.SaveAllChangesAsync("Author1 deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(author1.Id, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
   }
   
   [Fact]
   public async Task SelectAllAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      repository.AddRange(authors);
      await unitOfWork.SaveAllChangesAsync("Authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      var expAuthorDtos = authors
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .Select(a => a.ToAuthorDto())
         .ToList();

      // Act
      var result = await readModel.SelectAllAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualAuthorDtos = result.Value;
      actualAuthorDtos.Should().NotBeNull();
      actualAuthorDtos.Count.Should().Be(expAuthorDtos.Count);
      actualAuthorDtos.Should().BeEquivalentTo(
         expAuthorDtos,
         options => options.WithStrictOrdering()
      );
   }
   
   [Fact]
   public async Task SelectAllAsync_does_not_return_deactivated_authors() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;
      var deactivatedAuthor = authors[0];

      repository.AddRange(authors);
      await unitOfWork.SaveAllChangesAsync("Authors inserted", ct);

      var resultDeactivated = deactivatedAuthor.Deactivate(
         updatedAt: deactivatedAuthor.CreatedAt.AddDays(1)
      );
      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Author deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectAllAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualAuthorDtos = result.Value;
      actualAuthorDtos.Should().NotContain(a => a.Id == deactivatedAuthor.Id);
      actualAuthorDtos.Count.Should().Be(authors.Count - 1);
   }

   [Fact]
   public async Task SearchAsync_by_lastname_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      repository.AddRange(authors);
      await unitOfWork.SaveAllChangesAsync("Authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SearchAsync(
         searchText: "Martin",
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualAuthorDtos = result.Value;
      actualAuthorDtos.Should().NotBeNull();
      actualAuthorDtos.Should().NotBeEmpty();

      actualAuthorDtos.Should().OnlyContain(a =>
         a.Lastname.Contains("Martin") ||
         a.Firstname.Contains("Martin") ||
         a.DisplayName.Contains("Martin")
      );
   }

   [Fact]
   public async Task SearchAsync_unknown_name_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      repository.AddRange(authors);
      await unitOfWork.SaveAllChangesAsync("Authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SearchAsync(searchText: "Unknown Author", ct: ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeEmpty();
   }

   [Fact]
   public async Task SearchAsync_does_not_return_deactivated_authors() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var author1 = seed.Author1();
      
      repository.Add(author1);
      await unitOfWork.SaveAllChangesAsync("Author1 inserted", ct);
      var resultDeactivated = author1.Deactivate(
         updatedAt: author1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();
      await unitOfWork.SaveAllChangesAsync("Author1 deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SearchAsync(searchText: author1.Lastname, ct: ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().NotContain(a => a.Id == author1.Id);
   }
}