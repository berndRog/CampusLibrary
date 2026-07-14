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
   private static readonly Guid ReaderGuid = Guid.Parse(ReaderId);

   public ReaderProvisionProfileUseCasesIntT() {
      DbName = nameof(ReaderProvisionProfileUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task Provision_then_complete_profile_then_update_mutable_data_persists_reader() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      var provision = await useCases.ProvisionMeAsync(ReaderId, ct);
      provision.IsSuccess.Should().BeTrue();
      provision.Value.Should().BeTrue();

      var incomplete = await readModel.FindByIdAsync(ReaderGuid, false, ct);
      incomplete.IsSuccess.Should().BeTrue();
      incomplete.Value.Firstname.Should().BeNull();
      incomplete.Value.Lastname.Should().BeNull();
      incomplete.Value.Email.Should().Be("reader.one@example.org");
      incomplete.Value.AddressDto.Should().BeNull();
      incomplete.Value.IsProfileCompleted.Should().BeFalse();

      var profile = await useCases.UpdateMeProfileAsync(
         new ReaderProfileDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: new AddressDto(
               Street: "Profilstraße 1",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         ),
         ct
      );

      profile.IsSuccess.Should().BeTrue();
      profile.Value.Firstname.Should().Be("Alice");
      profile.Value.Lastname.Should().Be("Reader");
      profile.Value.Email.Should().Be("reader.one@example.org");
      profile.Value.IsProfileCompleted.Should().BeTrue();

      var update = await useCases.UpdateMeAsync(
         new ReaderUpdateDto(
            Lastname: "Changed",
            Email: "reader.changed@example.org",
            AddressDto: new AddressDto(
               Street: "Neue Straße 7",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         ),
         ct
      );

      update.IsSuccess.Should().BeTrue();
      update.Value.Firstname.Should().Be("Alice");
      update.Value.Lastname.Should().Be("Changed");
      update.Value.Email.Should().Be("reader.changed@example.org");
      update.Value.IsProfileCompleted.Should().BeTrue();

      var persisted = await readModel.FindByIdAsync(ReaderGuid, ct: ct);
      persisted.IsSuccess.Should().BeTrue();
      persisted.Value.Should().BeEquivalentTo(update.Value);
   }

   [Fact]
   public async Task Profile_without_valid_address_keeps_profile_incomplete() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      (await useCases.ProvisionMeAsync(ReaderId, ct)).IsSuccess.Should().BeTrue();

      var profile = await useCases.UpdateMeProfileAsync(
         new ReaderProfileDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: new AddressDto(
               Street: string.Empty,
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         ),
         ct
      );

      profile.IsFailure.Should().BeTrue();
      profile.Error.Should().Be(ReaderErrors.StreetIsRequired);

      var persisted = await readModel.FindByIdAsync(ReaderGuid, ct: ct);
      persisted.Value.IsProfileCompleted.Should().BeFalse();
   }

   [Fact]
   public async Task Provision_is_idempotent() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();

      var first = await useCases.ProvisionMeAsync(ReaderId, ct);
      var second = await useCases.ProvisionMeAsync(null, ct);

      first.Value.Should().BeTrue();
      second.Value.Should().BeFalse();
   }

   [Fact]
   public async Task Update_duplicate_email_fails_and_keeps_existing_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      (await useCases.ProvisionMeAsync(ReaderId, ct)).IsSuccess.Should().BeTrue();
      (await useCases.UpdateMeProfileAsync(
         new ReaderProfileDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: seed.Address1Vo.ToAddressDto()!
         ),
         ct
      )).IsSuccess.Should().BeTrue();

      var otherReader = seed.Reader2();
      repository.Add(otherReader);
      await unitOfWork.SaveAllChangesAsync("Other reader inserted", ct);
      unitOfWork.ClearChangeTracker();

      var update = await useCases.UpdateMeAsync(
         new ReaderUpdateDto(
            Lastname: "Changed",
            Email: otherReader.EmailVo.Value,
            AddressDto: null
         ),
         ct
      );

      update.IsFailure.Should().BeTrue();
      update.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);

      unitOfWork.ClearChangeTracker();
      var persisted = await readModel.FindByIdAsync(ReaderGuid, ct: ct);
      persisted.Value.Lastname.Should().Be("Reader");
      persisted.Value.Email.Should().NotBe(otherReader.EmailVo.Value);
   }
}
