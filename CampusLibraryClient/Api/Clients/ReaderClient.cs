using System.Net.Http.Json;
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
) : BaseApiClient<ReaderClient>(
   factory: factory,
   json: json,
   logger: logger
), IReaderClient {

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
            requestUri: $"{Base}/readers/email?email={Uri.EscapeDataString(email)}&includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/readers
   public Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.PostAsJsonAsync(
            requestUri: $"{Base}/readers",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // PUT /camplib/v1/readers/{id}
   public Task<Result<ReaderDto>> UpdateAsync(
      Guid id,
      ReaderUpdateDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<ReaderDto>(
         send: () => _http.PutAsJsonAsync(
            requestUri: $"{Base}/readers/{id}",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // DELETE /camplib/v1/readers/{id} -> 204 No Content
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
