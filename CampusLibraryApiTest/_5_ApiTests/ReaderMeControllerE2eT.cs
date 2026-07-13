using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using CampusLibraryApi._1_Web.Security;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusLibraryApiTest._4_WebTests;

public sealed class ReaderMeControllerE2eT : TestBaseEndToEnd {
   protected override string DatabaseName => nameof(ReaderMeControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;

   // This E2E test authenticates via TestAuthHandler.
   // Therefore the application must read identity data from HttpContext.User,
   // not from a FakeIdentityGateway with static/default values.
   protected override void ConfigureTestServices(IServiceCollection services) {
      services.RemoveAll<IIdentityGateway>();
      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();
   }

   private const string Url = "/camplib/v1";
   private const string Subject = "reader-subject-e2e-001";
   private const string Username = "reader.e2e@example.org";
   private const string ReaderId = "10000000-0000-0000-0000-000000000001";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

   [Fact]
   public async Task GetMeAsync_without_token_returns_unauthorized() {
      // Act
      var response = await Client.GetAsync(
         requestUri: $"{Url}/readers/me",
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task CreateProvisionAsync_without_token_returns_unauthorized() {
      // Act
      var response = await Client.PostAsync(
         requestUri: $"{Url}/readers/me/provision?id={ReaderId}",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task CreateProvisionAsync_with_employee_role_returns_forbidden() {
      // Arrange
      using var provisionRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Post,
         url: $"{Url}/readers/me/provision?id={ReaderId}",
         roles: "Employee"
      );

      // Act
      var response = await Client.SendAsync(
         request: provisionRequest,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
   }

   [Fact]
   public async Task CreateProvisionAsync_then_UpdateProfileAsync_then_GetMeAsync_ok() {
      // Act 1: provision
      var provision = await ProvisionReaderAsync();

      // Assert 1
      provision.Id.Should().Be(Guid.Parse(ReaderId));
      provision.WasCreated.Should().BeTrue();

      // Act 2: complete profile
      var profile = await CompleteProfileAsync();

      // Assert 2
      profile.Id.Should().Be(provision.Id);
      profile.Email.Should().Be(Username);
      profile.Subject.Should().Be(Subject);
      profile.Firstname.Should().Be("Alice");
      profile.Lastname.Should().Be("Reader");
      profile.AddressDto.Should().NotBeNull();
      profile.IsProfileCompleted.Should().BeTrue();

      // Act 3: GET me through the ReadModel
      using var getMeRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Get,
         url: $"{Url}/readers/me"
      );

      var getMeResponse = await Client.SendAsync(
         request: getMeRequest,
         cancellationToken: _ct
      );

      var meBody = await getMeResponse.Content.ReadAsStringAsync(_ct);

      // Assert 3
      getMeResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: meBody
      );

      var me = DeserializeJson<ReaderDto>(meBody);
      me.Should().BeEquivalentTo(profile);
   }

   [Fact]
   public async Task UpdateProfileAsync_without_address_returns_bad_request() {
      // Arrange: provisioned reader may exist without address.
      await ProvisionReaderAsync();

      using var profileRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Put,
         url: $"{Url}/readers/me/profile"
      );
      profileRequest.Content = JsonContent.Create(
         inputValue: new ReaderProfileMeDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: null!
         )
      );

      // Act
      var profileResponse = await Client.SendAsync(
         request: profileRequest,
         cancellationToken: _ct
      );

      // Assert: AddressVo is technically nullable, but fachlich required.
      profileResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
   }

   [Fact]
   public async Task UpdateMeAsync_without_token_returns_unauthorized() {
      // Act
      var response = await Client.PutAsJsonAsync(
         requestUri: $"{Url}/readers/me/update",
         value: new ReaderUpdateMeDto(
            Lastname: "Updated",
            Email: "updated.reader@example.org",
            AddressDto: null
         ),
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task UpdateMeAsync_after_profile_completion_updates_mutable_data_ok() {
      // Arrange
      await ProvisionReaderAsync();
      var profile = await CompleteProfileAsync();

      using var updateRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Put,
         url: $"{Url}/readers/me/update"
      );
      updateRequest.Content = JsonContent.Create(
         inputValue: new ReaderUpdateMeDto(
            Lastname: "Updated",
            Email: "reader.updated@example.org",
            AddressDto: new AddressDto(
               Street: "Updateweg 7",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         )
      );

      // Act
      var updateResponse = await Client.SendAsync(
         request: updateRequest,
         cancellationToken: _ct
      );

      var updateBody = await updateResponse.Content.ReadAsStringAsync(_ct);

      // Assert
      updateResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: updateBody
      );

      var updated = DeserializeJson<ReaderDto>(updateBody);

      updated.Should().NotBeNull();
      updated!.Id.Should().Be(profile.Id);
      updated.Subject.Should().Be(Subject);
      updated.Firstname.Should().Be("Alice");
      updated.Lastname.Should().Be("Updated");
      updated.Email.Should().Be("reader.updated@example.org");
      updated.AddressDto.Should().BeEquivalentTo(
         new AddressDto(
            Street: "Updateweg 7",
            PostalCode: "29556",
            City: "Suderburg",
            Country: "DE"
         )
      );
      updated.IsProfileCompleted.Should().BeTrue();

      using var getMeRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Get,
         url: $"{Url}/readers/me"
      );

      var getMeResponse = await Client.SendAsync(getMeRequest, _ct);
      var meBody = await getMeResponse.Content.ReadAsStringAsync(_ct);

      getMeResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: meBody
      );

      var me = DeserializeJson<ReaderDto>(meBody);
      me.Should().BeEquivalentTo(updated);
   }

   [Fact]
   public async Task UpdateMeAsync_duplicate_email_returns_conflict() {
      // Arrange
      string duplicateEmail = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var otherReader = seed.Reader2();
         duplicateEmail = otherReader.EmailVo.Value;

         repository.Add(otherReader);
         await unitOfWork.SaveAllChangesAsync("Other reader inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      await ProvisionReaderAsync();
      await CompleteProfileAsync();

      using var updateRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Put,
         url: $"{Url}/readers/me/update"
      );
      updateRequest.Content = JsonContent.Create(
         inputValue: new ReaderUpdateMeDto(
            Lastname: null,
            Email: duplicateEmail,
            AddressDto: null
         )
      );

      // Act
      var response = await Client.SendAsync(
         request: updateRequest,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task CreateProvisionAsync_is_idempotent() {
      // Act 1
      var first = await ProvisionReaderAsync();

      // Act 2
      using var secondRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Post,
         url: $"{Url}/readers/me/provision"
      );
      var secondResponse = await Client.SendAsync(secondRequest, _ct);
      var secondBody = await secondResponse.Content.ReadAsStringAsync(_ct);

      // Assert 2
      secondResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: secondBody
      );

      var second = DeserializeJson<ReaderProvisionMeDto>(secondBody);

      second.Should().NotBeNull();
      second!.Id.Should().Be(first.Id);
      second.WasCreated.Should().BeFalse();
   }

   private async Task<ReaderProvisionMeDto> ProvisionReaderAsync() {
      using var provisionRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Post,
         url: $"{Url}/readers/me/provision?id={ReaderId}"
      );

      var provisionResponse = await Client.SendAsync(
         request: provisionRequest,
         cancellationToken: _ct
      );

      var provisionBody = await provisionResponse.Content.ReadAsStringAsync(_ct);

      provisionResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: provisionBody
      );

      var provision = DeserializeJson<ReaderProvisionMeDto>(provisionBody);

      provision.Should().NotBeNull();
      return provision!;
   }

   private async Task<ReaderDto> CompleteProfileAsync() {
      using var profileRequest = CreateAuthenticatedRequest(
         method: HttpMethod.Put,
         url: $"{Url}/readers/me/profile"
      );
      profileRequest.Content = JsonContent.Create(
         inputValue: new ReaderProfileMeDto(
            Firstname: "Alice",
            Lastname: "Reader",
            AddressDto: new AddressDto(
               Street: "Profilstraße 1",
               PostalCode: "29556",
               City: "Suderburg",
               Country: "DE"
            )
         )
      );

      var profileResponse = await Client.SendAsync(
         request: profileRequest,
         cancellationToken: _ct
      );

      var profileBody = await profileResponse.Content.ReadAsStringAsync(_ct);

      profileResponse.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: profileBody
      );

      var profile = DeserializeJson<ReaderDto>(profileBody);

      profile.Should().NotBeNull();
      return profile!;
   }

   private static HttpRequestMessage CreateAuthenticatedRequest(
      HttpMethod method,
      string url,
      string roles = "Reader",
      string subject = Subject,
      string username = Username,
      string createdAt = "2025-01-01T00:00:00Z"
   ) {
      var request = new HttpRequestMessage(
         method: method,
         requestUri: url
      );

      request.Headers.Add(TestAuthHandler.Header, roles);
      request.Headers.Add(TestAuthHandler.SubjectHeader, subject);
      request.Headers.Add(TestAuthHandler.UsernameHeader, username);
      request.Headers.Add(TestAuthHandler.CreatedAtHeader, createdAt);

      return request;
   }

   private static T? DeserializeJson<T>(string json) =>
      JsonSerializer.Deserialize<T>(
         json: json,
         options: new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
         }
      );
}
