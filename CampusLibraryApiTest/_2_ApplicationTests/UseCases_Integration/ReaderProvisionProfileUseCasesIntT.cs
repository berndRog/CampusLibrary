using AwesomeAssertions;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using CampusLibraryApiTest.TestInfrastructure;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class ReaderProvisionProfileUseCasesIntT : TestBaseIntegration {
   private const string ReaderId = "10000000-0000-0000-0000-000000000001";

   public ReaderProvisionProfileUseCasesIntT() {
      DbName = nameof(ReaderProvisionProfileUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task CreateProvisionAsync_then_UpdateProfileAsync_persists_reader_profile() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Act 1: provision from FakeIdentityGateway in DiTestModules.
      var resultProvision = await useCases.CreateProvisionAsync(
         id: ReaderId,
         ct: ct
      );

      // Assert 1: reader exists, but profile is incomplete.
      resultProvision.IsSuccess.Should().BeTrue();
      resultProvision.Value.Id.Should().Be(Guid.Parse(ReaderId));
      resultProvision.Value.WasCreated.Should().BeTrue();

      var resultReadAfterProvision = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         ct: ct
      );

      resultReadAfterProvision.IsSuccess.Should().BeTrue();
      resultReadAfterProvision.Value.Firstname.Should().BeEmpty();
      resultReadAfterProvision.Value.Lastname.Should().BeEmpty();
      resultReadAfterProvision.Value.AddressDto.Should().BeNull();
      resultReadAfterProvision.Value.IsProfileCompleted.Should().BeFalse();

      // Act 2: complete profile.
      var resultProfile = await useCases.UpdateProfileAsync(
         dto: new ReaderProfileUpdateDto(
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

      var resultReadAfterProfile = await readModel.FindByIdAsync(
         id: resultProvision.Value.Id,
         ct: ct
      );

      resultReadAfterProfile.IsSuccess.Should().BeTrue();
      resultReadAfterProfile.Value.IsProfileCompleted.Should().BeTrue();
      resultReadAfterProfile.Value.Firstname.Should().Be("Alice");
      resultReadAfterProfile.Value.Lastname.Should().Be("Reader");
      resultReadAfterProfile.Value.AddressDto.Should().BeEquivalentTo(resultProfile.Value.AddressDto);
   }


   [Fact]
   public async Task UpdateProfileAsync_without_address_keeps_profile_incomplete() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Arrange: provisioning may technically persist AddressVo as null.
      var resultProvision = await useCases.CreateProvisionAsync(
         id: ReaderId,
         ct: ct
      );
      resultProvision.IsSuccess.Should().BeTrue();

      // Act: fachlich, address is still required for completing the profile.
      var resultProfile = await useCases.UpdateProfileAsync(
         dto: new ReaderProfileUpdateDto(
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
   public async Task CreateProvisionAsync_is_idempotent() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();

      // Act
      var resultFirst = await useCases.CreateProvisionAsync(
         id: ReaderId,
         ct: ct
      );
      var resultSecond = await useCases.CreateProvisionAsync(
         id: null,
         ct: ct
      );

      // Assert
      resultFirst.IsSuccess.Should().BeTrue();
      resultSecond.IsSuccess.Should().BeTrue();
      resultSecond.Value.Id.Should().Be(resultFirst.Value.Id);
      resultSecond.Value.WasCreated.Should().BeFalse();
   }
}
