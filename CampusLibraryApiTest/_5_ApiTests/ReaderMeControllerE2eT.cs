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
   public async Task GetMe_without_token_returns_unauthorized() {
      var response = await Client.GetAsync($"{Url}/readers/me", _ct);
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task Provision_without_token_returns_unauthorized() {
      var response = await Client.PostAsync(
         $"{Url}/readers/me/provision?id={ReaderId}", null, _ct
      );
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task Provision_with_employee_role_returns_forbidden() {
      using var request = CreateAuthenticatedRequest(
         HttpMethod.Post,
         $"{Url}/readers/me/provision?id={ReaderId}",
         roles: "Employee"
      );
      var response = await Client.SendAsync(request, _ct);
      response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
   }

   [Fact]
   public async Task Provision_profile_and_get_me_ok() {
      await ProvisionReaderAsync();
      var profile = await SaveProfileAsync(NewProfile());

      profile.Id.Should().Be(Guid.Parse(ReaderId));
      profile.Email.Should().Be(Username);
      profile.Firstname.Should().Be("Alice");
      profile.Lastname.Should().Be("Reader");
      profile.IsProfileCompleted.Should().BeTrue();

      using var request = CreateAuthenticatedRequest(HttpMethod.Get, $"{Url}/readers/me");
      var response = await Client.SendAsync(request, _ct);
      var me = await ReadJsonAsync<ReaderDto>(response);

      response.StatusCode.Should().Be(HttpStatusCode.OK);
      me.Should().BeEquivalentTo(profile);
   }

   [Fact]
   public async Task Profile_without_valid_address_returns_bad_request() {
      await ProvisionReaderAsync();
      var dto = new ReaderProfileDto(
         Firstname: "Alice",
         Lastname: "Reader",
         AddressDto: new AddressDto(string.Empty, "29556", "Suderburg", "DE")
      );

      using var request = CreateAuthenticatedRequest(HttpMethod.Put, $"{Url}/readers/me/profile");
      request.Content = JsonContent.Create(dto);
      var response = await Client.SendAsync(request, _ct);

      response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
   }

   [Fact]
   public async Task Profile_without_token_returns_unauthorized() {
      var response = await Client.PutAsJsonAsync(
         $"{Url}/readers/me/profile", NewProfile(), _ct
      );
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task Update_without_token_returns_unauthorized() {
      var response = await Client.PutAsJsonAsync(
         $"{Url}/readers/me/update",
         new ReaderUpdateDto("Updated", null, null),
         _ct
      );
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task Update_after_profile_completion_changes_mutable_data() {
      await ProvisionReaderAsync();
      var initial = await SaveProfileAsync(NewProfile());

      var changed = await SaveUpdateAsync(
         new ReaderUpdateDto(
            Lastname: "Updated",
            Email: "reader.updated@example.org",
            AddressDto: new AddressDto("Updateweg 7", "29556", "Suderburg", "DE")
         )
      );

      changed.Id.Should().Be(initial.Id);
      changed.Firstname.Should().Be("Alice");
      changed.Lastname.Should().Be("Updated");
      changed.Email.Should().Be("reader.updated@example.org");
      changed.AddressDto.Should().BeEquivalentTo(
         new AddressDto("Updateweg 7", "29556", "Suderburg", "DE")
      );
   }

   [Fact]
   public async Task Update_duplicate_email_returns_conflict() {
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
      await SaveProfileAsync(NewProfile());

      using var request = CreateAuthenticatedRequest(HttpMethod.Put, $"{Url}/readers/me/update");
      request.Content = JsonContent.Create(
         new ReaderUpdateDto(null, duplicateEmail, null)
      );
      var response = await Client.SendAsync(request, _ct);

      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task Provision_is_idempotent_and_returns_no_content() {
      await ProvisionReaderAsync();

      using var request = CreateAuthenticatedRequest(HttpMethod.Post, $"{Url}/readers/me/provision");
      var response = await Client.SendAsync(request, _ct);

      response.StatusCode.Should().Be(HttpStatusCode.NoContent);
   }

   private async Task ProvisionReaderAsync() {
      using var request = CreateAuthenticatedRequest(
         HttpMethod.Post,
         $"{Url}/readers/me/provision?id={ReaderId}"
      );
      var response = await Client.SendAsync(request, _ct);
      response.StatusCode.Should().Be(HttpStatusCode.NoContent);
   }

   private async Task<ReaderDto> SaveProfileAsync(ReaderProfileDto dto) {
      using var request = CreateAuthenticatedRequest(HttpMethod.Put, $"{Url}/readers/me/profile");
      request.Content = JsonContent.Create(dto);
      var response = await Client.SendAsync(request, _ct);
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      return (await ReadJsonAsync<ReaderDto>(response))!;
   }

   private async Task<ReaderDto> SaveUpdateAsync(ReaderUpdateDto dto) {
      using var request = CreateAuthenticatedRequest(HttpMethod.Put, $"{Url}/readers/me/update");
      request.Content = JsonContent.Create(dto);
      var response = await Client.SendAsync(request, _ct);
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      return (await ReadJsonAsync<ReaderDto>(response))!;
   }

   private static ReaderProfileDto NewProfile() => new(
      Firstname: "Alice",
      Lastname: "Reader",
      AddressDto: new AddressDto("Profilstraße 1", "29556", "Suderburg", "DE")
   );

   private static HttpRequestMessage CreateAuthenticatedRequest(
      HttpMethod method,
      string url,
      string roles = "Reader",
      string subject = Subject,
      string username = Username,
      string createdAt = "2025-01-01T00:00:00Z"
   ) {
      var request = new HttpRequestMessage(method, url);
      request.Headers.Add(TestAuthHandler.Header, roles);
      request.Headers.Add(TestAuthHandler.SubjectHeader, subject);
      request.Headers.Add(TestAuthHandler.UsernameHeader, username);
      request.Headers.Add(TestAuthHandler.CreatedAtHeader, createdAt);
      return request;
   }

   private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response) {
      var json = await response.Content.ReadAsStringAsync();
      return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions {
         PropertyNameCaseInsensitive = true
      });
   }
}
