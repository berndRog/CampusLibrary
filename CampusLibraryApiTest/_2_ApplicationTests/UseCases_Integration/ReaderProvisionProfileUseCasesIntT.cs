using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class ReaderProvisionProfileUseCasesIntT : TestBaseIntegration {
   private const string ReaderId = "10000000-0000-0000-0000-000000000001";

   public ReaderProvisionProfileUseCasesIntT() {
      DbName = nameof(ReaderProvisionProfileUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task CreateMeProvisionAsync_then_UpdateMeProfileAsync_then_UpdateMeAsync_persists_reader_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Act 1: provision from FakeIdentityGateway in DiTestModules.
      var resultProvision = await useCases.ProvisionMeAsync(
         id: ReaderId,
         ct: ct
      );

      // Assert 1: reader exists, but profile is incomplete.
      resultProvision.IsSuccess.Should().BeTrue();
      resultProvision.Value.Id.Should().Be(Guid.Parse(ReaderId));
      resultProvision.Value.WasCreated.Should().BeTrue();

      var resultReadAfterProvision = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         includeInactive: false,
         ct: ct
      );

      resultReadAfterProvision.IsSuccess.Should().BeTrue();
      resultReadAfterProvision.Value.Firstname.Should().BeEmpty();
      resultReadAfterProvision.Value.Lastname.Should().BeEmpty();
      resultReadAfterProvision.Value.AddressDto.Should().BeNull();
      resultReadAfterProvision.Value.IsProfileCompleted.Should().BeFalse();

      // Act 2: complete initial profile.
      var resultProfile = await useCases.UpdateMeProfileAsync(
         meDto: new ReaderProfileMeDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: new AddressDto(
               Street: "Profilstraße 1",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         ),
         ct: ct
      );

      // Assert 2: profile is now complete.
      resultProfile.IsSuccess.Should().BeTrue();
      resultProfile.Value.Firstname.Should().Be("Alice");
      resultProfile.Value.Lastname.Should().Be("Reader");
      resultProfile.Value.IsProfileCompleted.Should().BeTrue();
      resultProfile.Value.AddressDto.Should().NotBeNull();

      // Act 3: later self-service update.
      var resultUpdate = await useCases.UpdateMeAsync(
         meDto: new ReaderUpdateMeDto(
            Lastname: "Changed",
            Email: "reader.changed@example.org",
            AddressDto: new AddressDto(
               Street: "Neue Straße 7",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         ),
         ct: ct
      );

      // Assert 3: firstname remains unchanged, mutable data changed.
      resultUpdate.IsSuccess.Should().BeTrue();
      resultUpdate.Value.Firstname.Should().Be("Alice");
      resultUpdate.Value.Lastname.Should().Be("Changed");
      resultUpdate.Value.Email.Should().Be("reader.changed@example.org");
      resultUpdate.Value.IsProfileCompleted.Should().BeTrue();

      var resultReadAfterUpdate = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         ct: ct
      );

      resultReadAfterUpdate.IsSuccess.Should().BeTrue();
      resultReadAfterUpdate.Value.Should().BeEquivalentTo(resultUpdate.Value);
   }

   [Fact]
   public async Task UpdateMeProfileAsync_without_address_keeps_profile_incomplete() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Arrange: provisioning may technically persist AddressVo as null.
      var resultProvision = await useCases.ProvisionMeAsync(
         id: ReaderId,
         ct: ct
      );
      resultProvision.IsSuccess.Should().BeTrue();

      // Act: fachlich, address is still required for completing the profile.
      var resultProfile = await useCases.UpdateMeProfileAsync(
         meDto: new ReaderProfileMeDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: null!
         ),
         ct: ct
      );

      // Assert
      resultProfile.IsFailure.Should().BeTrue();
      resultProfile.Error.Should().Be(ReaderErrors.AddressIsRequired);

      var resultRead = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         ct: ct
      );

      resultRead.IsSuccess.Should().BeTrue();
      resultRead.Value.AddressDto.Should().BeNull();
      resultRead.Value.IsProfileCompleted.Should().BeFalse();
   }

   [Fact]
   public async Task CreateMeProvisionAsync_is_idempotent() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();

      // Act
      var resultFirst = await useCases.ProvisionMeAsync(
         id: ReaderId,
         ct: ct
      );
      var resultSecond = await useCases.ProvisionMeAsync(
         id: null,
         ct: ct
      );

      // Assert
      resultFirst.IsSuccess.Should().BeTrue();
      resultSecond.IsSuccess.Should().BeTrue();
      resultSecond.Value.Id.Should().Be(resultFirst.Value.Id);
      resultSecond.Value.WasCreated.Should().BeFalse();
   }

   [Fact]
   public async Task UpdateMeAsync_duplicate_email_fails_and_keeps_existing_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange: create current reader through self-service flow.
      var resultProvision = await useCases.ProvisionMeAsync(
         id: ReaderId,
         ct: ct
      );
      resultProvision.IsSuccess.Should().BeTrue();

      var resultProfile = await useCases.UpdateMeProfileAsync(
         meDto: new ReaderProfileMeDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: seed.Address1Vo.ToAddressDto()!
         ),
         ct: ct
      );
      resultProfile.IsSuccess.Should().BeTrue();

      // Add another reader with a different email.
      var otherReader = seed.Reader2();
      repository.Add(otherReader);
      await unitOfWork.SaveAllChangesAsync("Other reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act: current reader tries to use the other reader's email.
      var resultUpdate = await useCases.UpdateMeAsync(
         meDto: new ReaderUpdateMeDto(
            Lastname: "Changed",
            Email: otherReader.EmailVo.Value,
            AddressDto: null
         ),
         ct: ct
      );

      unitOfWork.ClearChangeTracker();

      // Assert
      resultUpdate.IsFailure.Should().BeTrue();
      resultUpdate.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);

      var resultRead = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         ct: ct
      );

      resultRead.IsSuccess.Should().BeTrue();
      resultRead.Value.Lastname.Should().Be("Reader");
      resultRead.Value.Email.Should().NotBe(otherReader.EmailVo.Value);
   }
}
