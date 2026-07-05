using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class ReaderReadModelFindMeIntT : TestBaseIntegration {
   private const string Subject = "find-me-subject-001";
   private const string Username = "find.me@example.org";

   public ReaderReadModelFindMeIntT() {
      DbName = nameof(ReaderReadModelFindMeIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindMeAsync_ok_returns_reader_for_current_subject() {
      await using var provider = Root.CreateCustomProvider(services => {
         services.RemoveAll<IIdentityGateway>();
         services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway {
            Subject = Subject,
            Username = Username,
            IsAuthenticated = true,
            IsReader = true,
            IsEmployee = false
         });
      });

      using var scope = provider.CreateScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

      // Arrange
      var reader = Reader.Create(
         id: Guid.Parse("10000000-0000-0000-0000-000000000010"),
         firstname: "Find",
         lastname: "Me",
         emailVo: EmailVo.Create(Username).GetValueOrThrow(),
         addressVo: AddressVo.Create(
            street: "Profilstraße 1",
            postalCode: "29556",
            city: "Suderburg",
            country: "DE"
         ).GetValueOrThrow(),
         subject: Subject,
         createdAt: FakeClock.DefaultUtcNow
      ).GetValueOrThrow();

      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindMeAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(reader.Id);
      result.Value.Subject.Should().Be(Subject);
      result.Value.Email.Should().Be(Username);
      result.Value.IsProfileCompleted.Should().BeTrue();
   }

   [Fact]
   public async Task FindMeAsync_without_reader_role_returns_forbidden() {
      await using var provider = Root.CreateCustomProvider(services => {
         services.RemoveAll<IIdentityGateway>();
         services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway {
            Subject = Subject,
            Username = Username,
            IsAuthenticated = true,
            IsReader = false,
            IsEmployee = true
         });
      });

      using var scope = provider.CreateScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Act
      var result = await readModel.FindMeAsync(ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.AccessNotAllowed);
   }

   [Fact]
   public async Task FindMeAsync_not_provisioned_returns_not_provisioned() {
      await using var provider = Root.CreateCustomProvider(services => {
         services.RemoveAll<IIdentityGateway>();
         services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway {
            Subject = Subject,
            Username = Username,
            IsAuthenticated = true,
            IsReader = true,
            IsEmployee = false
         });
      });

      using var scope = provider.CreateScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Act
      var result = await readModel.FindMeAsync(ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.NotProvisioned);
   }
}
