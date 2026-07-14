using System.Text.Json;
using CampusLibraryClient.Api.Contracts;
using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;
using CampusLibraryClient.Core.Utils;

namespace CampusLibraryClient.Api.Clients;

public sealed class ReaderClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<ReaderClient> logger
) : BaseApiClient<ReaderClient>(factory, json, logger), IReaderClient {
   private const string Base = "camplib/v1";

   // GET /camplib/v1/readers?includeInactive=false
   public Task<Result<IEnumerable<ReaderDto>>> GetAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<IEnumerable<ReaderDto>>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/readers?includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/readers/{id}?includeInactive=false
   public Task<Result<ReaderDto>> GetByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/readers/{id}?includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/readers/email?email={email}&includeInactive=false
   public Task<Result<ReaderDto>> GetByEmailAsync(
      string email,
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.GetAsync(
            requestUri:
            $"{Base}/readers/email?email={Uri.EscapeDataString(email)}&includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/readers/me
   // Returns the current Reader self-service view.
   public Task<Result<ReaderDto>> GetMeAsync(
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/readers/me",
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/readers/me/provision?id={optionalId}
   // Idempotently provisions the fachlicher Reader for the current technical user.
   public Task<Result<bool>> ProvisionMeAsync(
      string? id = null,
      CancellationToken ct = default
   ) {
      string uri = string.IsNullOrWhiteSpace(id)
         ? $"{Base}/readers/me/provision"
         : $"{Base}/readers/me/provision?id={Uri.EscapeDataString(id)}";

      return SendAsync<bool>(
         send: () => _http.PostAsync(
            requestUri: uri,
            content: null,
            cancellationToken: ct
         ),
         ct: ct
      );
   }

   // PUT /camplib/v1/readers/me/profile
   // Completes the initial fachliche profile after provisioning.
   public Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.PutAsJsonAsync(
            requestUri: $"{Base}/readers/me/profile",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // PUT /camplib/v1/readers/me/update
   // Changes selected mutable fachliche Reader data.
   public Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.PutAsJsonAsync(
            requestUri: $"{Base}/readers/me/update",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // DELETE /camplib/v1/readers/{id} -> 204 No Content
   // Administrative deactivation remains available for employee/admin flows.
   public Task<Result<bool>> DeactivateAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<bool>(
         send: () => _http.DeleteAsync(
            requestUri: $"{Base}/readers/{id}",
            cancellationToken: ct
         ),
         ct: ct
      );
}
