using AwesomeAssertions;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class ReaderUseCasesIntT : TestBaseIntegration {

   public ReaderUseCasesIntT() {
      DbName = nameof(ReaderUseCasesIntT);
      DbMode = DbMode.FileUnique;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task CreateAsync_ok_persists_reader_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Arrange
      var dto = CreateDto(
         id: "81000000-0000-0000-0000-000000000000",
         firstname: "Ina",
         lastname: "Integration",
         email: "INA.INTEGRATION@EXAMPLE.COM",
         subject: "81000000-0000-0000-0000-000000000000"
      );

      // Act
      var createResult = await useCases.CreateAsync(dto, ct);
      var findResult = await readModel.FindByIdAsync(createResult.Value.Id, ct);

      // Assert
      createResult.IsSuccess.Should().BeTrue();
      createResult.Value.Id.Should().Be(Guid.Parse(dto.Id!));
      createResult.Value.Email.Should().Be("ina.integration@example.com");

      findResult.IsSuccess.Should().BeTrue();
      findResult.Value.Should().BeEquivalentTo(createResult.Value);
   }

   [Fact]
   public async Task CreateAsync_duplicate_email_fails_and_does_not_insert_reader() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();

      // Arrange
      var firstDto = CreateDto(
         id: "82000000-0000-0000-0000-000000000000",
         firstname: "Erster",
         lastname: "Reader",
         email: "duplicate.reader@example.com",
         subject: "82000000-0000-0000-0000-000000000000"
      );
      var secondDto = CreateDto(
         id: "83000000-0000-0000-0000-000000000000",
         firstname: "Zweiter",
         lastname: "Reader",
         email: "DUPLICATE.READER@EXAMPLE.COM",
         subject: "83000000-0000-0000-0000-000000000000"
      );

      var firstResult = await useCases.CreateAsync(firstDto, ct);

      // Act
      var secondResult = await useCases.CreateAsync(secondDto, ct);
      var readSecondResult = await readModel.FindByIdAsync(Guid.Parse(secondDto.Id!), ct);

      // Assert
      firstResult.IsSuccess.Should().BeTrue();
      secondResult.IsFailure.Should().BeTrue();
      readSecondResult.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task UpdateAsync_ok_persists_changes_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<CampusLibraryApi._2_Shared._1_Ports.IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();
      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new ReaderUpdateDto(
         Firstname: "Erna",
         Lastname: "Musterfrau",
         Email: "ERNA.MUSTERFRAU@EXAMPLE.COM",
         AddressDto: new AddressDto(
            Street: "Neue Straße 5",
            PostalCode: "30123",
            City: "Hannover",
            Country: "DE"
         )
      );

      // Act
      var updateResult = await useCases.UpdateAsync(reader.Id, dto, ct);
      unitOfWork.ClearChangeTracker();
      var findResult = await readModel.FindByIdAsync(reader.Id, ct);

      // Assert
      updateResult.IsSuccess.Should().BeTrue();
      updateResult.Value.Firstname.Should().Be("Erna");
      updateResult.Value.Lastname.Should().Be("Musterfrau");
      updateResult.Value.Email.Should().Be("erna.musterfrau@example.com");
      updateResult.Value.AddressDto.City.Should().Be("Hannover");

      findResult.IsSuccess.Should().BeTrue();
      findResult.Value.Should().BeEquivalentTo(updateResult.Value);
   }

   [Fact]
   public async Task UpdateAsync_duplicate_email_fails_and_keeps_existing_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<CampusLibraryApi._2_Shared._1_Ports.IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var reader2 = seed.Reader2();
      repository.AddRange([reader1, reader2]);
      await unitOfWork.SaveAllChangesAsync("Reader1 and Reader2 inserted", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new ReaderUpdateDto(
         Firstname: "Erna",
         Lastname: "Musterfrau",
         Email: reader2.EmailVo.Value,
         AddressDto: reader1.AddressVo.ToAddressDtoForTest()
      );

      // Act
      var updateResult = await useCases.UpdateAsync(reader1.Id, dto, ct);
      unitOfWork.ClearChangeTracker();
      var findResult = await readModel.FindByIdAsync(reader1.Id, ct);

      // Assert
      updateResult.IsFailure.Should().BeTrue();
      findResult.IsSuccess.Should().BeTrue();
      findResult.Value.Email.Should().Be(reader1.EmailVo.Value);
      findResult.Value.Firstname.Should().Be(reader1.Firstname);
   }

   [Fact]
   public async Task DeleteAsync_ok_removes_reader_from_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<CampusLibraryApi._2_Shared._1_Ports.IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader3();
      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader3 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var deleteResult = await useCases.DeleteAsync(reader.Id, ct);
      unitOfWork.ClearChangeTracker();
      var findResult = await readModel.FindByIdAsync(reader.Id, ct);

      // Assert
      deleteResult.IsSuccess.Should().BeTrue();
      findResult.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task DeleteAsync_unknown_reader_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();

      // Arrange
      var unknownId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      // Act
      var deleteResult = await useCases.DeleteAsync(unknownId, ct);

      // Assert
      deleteResult.IsFailure.Should().BeTrue();
   }

   private static ReaderCreateDto CreateDto(
      string id,
      string firstname,
      string lastname,
      string email,
      string subject
   ) => new(
      Firstname: firstname,
      Lastname: lastname,
      Email: email,
      AddressDto: new AddressDto(
         Street: "Teststraße 1",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      ),
      Subject: subject,
      Id: id
   );
}

file static class ReaderUseCasesIntTExtensions {
   public static AddressDto ToAddressDtoForTest(
      this CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects.AddressVo addressVo
   ) => new(
      Street: addressVo.Street,
      PostalCode: addressVo.PostalCode,
      City: addressVo.City,
      Country: addressVo.Country
   );
}
