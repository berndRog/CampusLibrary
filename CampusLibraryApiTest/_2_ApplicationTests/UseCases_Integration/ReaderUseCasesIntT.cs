using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestHelper.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class ReaderUseCasesIntT : TestBaseIntegration {

   public ReaderUseCasesIntT() {
      DbName = nameof(ReaderUseCasesIntT);
      DbMode = DbMode.FileUnique;
      SensitiveDataLogging = true;
   }

   #region ReaderUcCreate
   [Fact]
   public async Task CreateAsync_ok_persists_reader_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var reader1 = seed.Reader1();

      // Arrange
      var dto = Mappings.ToReaderCreateDto(reader1);

      // Act
      var resultCreateReader1Dto = await useCases.CreateAsync(dto, ct);
      resultCreateReader1Dto.IsSuccess.Should().BeTrue();
      var createReader1Dto = resultCreateReader1Dto.Value;
      
      // Assert
      var resultFind = await readModel.FindByIdAsync(createReader1Dto.Id, ct);
      
      resultFind.IsSuccess.Should().BeTrue();
      var actualReader1Dto = resultFind.Value;
      actualReader1Dto.Should().BeEquivalentTo(createReader1Dto);
   }

   [Fact]
   public async Task CreateAsync_duplicate_email_fails_and_does_not_insert_reader() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      
      // Arrange
      var reader1 = seed.Reader1();
      var reader2Dto = Mappings.ToReaderCreateDto(seed.Reader2());
      var reader2DtoWithSameEmail = reader2Dto with {
         Email = reader1.EmailVo.Value
      };

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await useCases.CreateAsync(reader2DtoWithSameEmail, ct);
     
      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);
      
   }
   #endregion

   #region ReadUcUpdate
   [Fact]
   public async Task UpdateAsync_ok_persists_changes_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();
      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      var addressDto = seed.Address4Vo.ToAddressDto();
      var dto = new ReaderUpdateDto(
         Lastname: "Meier",
         Email: "e.meier@gmx.de",
         AddressDto: addressDto
      );

      // Act
      var resultUpdate = await useCases.UpdateAsync(reader.Id, dto, ct);
      
      resultUpdate.IsSuccess.Should().BeTrue();
      var updatedReader1Dto = resultUpdate.Value;
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var resultFind = await readModel.FindByIdAsync(reader.Id, ct);
      resultFind.IsSuccess.Should().BeTrue();
      var actualReader1Dto = resultFind.Value;
      actualReader1Dto.Should().BeEquivalentTo(updatedReader1Dto);
   }

   [Fact]
   public async Task UpdateAsync_duplicate_email_fails_and_keeps_existing_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var reader2 = seed.Reader2();
      repository.AddRange([reader1, reader2]);
      await unitOfWork.SaveAllChangesAsync("Reader1 and Reader2 inserted", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new ReaderUpdateDto(
         Lastname: "Meier",
         Email: reader2.EmailVo.Value,
         AddressDto: reader1.AddressVo.ToAddressDto()
      );

      // Act
      var resultUpdate = await useCases.UpdateAsync(reader1.Id, dto, ct);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      resultUpdate.IsFailure.Should().BeTrue();
      resultUpdate.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);
   }
   #endregion

   [Fact]
   public async Task DeleteAsync_ok_removes_reader_from_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();
      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var resultDelete = await useCases.DeleteAsync(reader.Id, ct);
      resultDelete.IsSuccess.Should().BeTrue();
      unitOfWork.ClearChangeTracker();

      // Assert
      var findResult = await readModel.FindByIdAsync(reader.Id, ct);
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
}