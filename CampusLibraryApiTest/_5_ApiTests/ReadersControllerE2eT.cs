using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestHelper.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._4_WebTests;

public sealed class ReadersControllerE2eT : TestBaseEndToEnd {
   
   protected override string DatabaseName => nameof(ReadersControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;
   
   private readonly string _url = "/camplib/v1";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
   
   [Fact]
   public async Task GetByIdAsync_ok() {
      // Arrange
      ReaderDto expectedReaderDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         expectedReaderDto = reader.ToReaderDto();

         repository.Add(reader);
         await unitOfWork.SaveAllChangesAsync("Reader1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/readers/{expectedReaderDto.Id}", _ct);
      
      var actualReaderDto = await response.Content
         .ReadFromJsonAsync<ReaderDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDto.Should().NotBeNull();
      actualReaderDto.Should().BeEquivalentTo(expectedReaderDto);
   }

   [Fact]
   public async Task GetAllAsync_ok() {
      // Arrange
      List<ReaderDto> expectedReaderDtos = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var readers = seed.Readers;
         expectedReaderDtos = readers
            .Select(r => r.ToReaderDto())
            .OrderBy(r => r.Id)
            .ToList();

         repository.AddRange(readers);
         await unitOfWork.SaveAllChangesAsync("Readers inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/readers", _ct);
      var actualReaderDtos = await response.Content
         .ReadFromJsonAsync<List<ReaderDto>>(_ct);

      // // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDtos.Should().NotBeNull();
      actualReaderDtos!
         .OrderBy(r => r.Id)
         .Should()
         .BeEquivalentTo(expectedReaderDtos);
   }

   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      ReaderCreateDto dto = default!;

      await Factory.WithScopeAsync(sp => {
         var seed = sp.GetRequiredService<TestSeed>();
         dto = Mappings.ToReaderCreateDto(seed.ReaderRegister());
         return Task.CompletedTask;
      });

      // Act
      var response = await Client.
         PostAsJsonAsync($"{_url}/readers", dto, _ct);
      
      var actualReaderDto = await response.Content.
         ReadFromJsonAsync<ReaderDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();
      actualReaderDto.Should().NotBeNull();
      actualReaderDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualReaderDto.Firstname.Should().Be(dto.Firstname);
      actualReaderDto.Lastname.Should().Be(dto.Lastname);
      actualReaderDto.Email.Should().Be(dto.Email);
      actualReaderDto.Subject.Should().Be(dto.Subject);
   }

   [Fact]
   public async Task UpdateAsync_ok() {
      // Arrange
      ReaderDto expectedReaderDto = default!;
      ReaderUpdateDto updateDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader1 = seed.Reader1();
         var reader2 = seed.Reader2();
         updateDto = Mappings.ToReaderUpdateDto(reader2);

         expectedReaderDto = new ReaderDto(
            Id: reader1.Id,
            Subject: reader1.Subject,
            Firstname: reader1.Firstname,
            Lastname: updateDto.Lastname ?? reader1.Lastname,
            Email: updateDto.Email ?? reader1.EmailVo.Value,
            AddressDto: updateDto.AddressDto ?? reader1.AddressVo.ToAddressDto()
         );

         repository.Add(reader1);
         await unitOfWork.SaveAllChangesAsync("Reader1 inserted", TestContext.Current.CancellationToken);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PutAsJsonAsync(
         $"{_url}/readers/{expectedReaderDto.Id}", updateDto, _ct);
      
      var actualReaderDto = 
         await response.Content.ReadFromJsonAsync<ReaderDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDto.Should().NotBeNull();
      actualReaderDto.Should().BeEquivalentTo(expectedReaderDto);
   }

   [Fact]
   public async Task DeleteAsync_ok() {
      // Arrange
      Guid readerId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader3();
         readerId = reader.Id;

         repository.Add(reader);
         await unitOfWork.SaveAllChangesAsync("Reader3 inserted", TestContext.Current.CancellationToken);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var responseDelete = await Client
         .DeleteAsync($"{_url}/readers/{readerId}",_ct);
      
      var responseGet = await Client
         .GetAsync($"{_url}/readers/{readerId}", _ct);

      // Assert
      responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
      responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }
}
