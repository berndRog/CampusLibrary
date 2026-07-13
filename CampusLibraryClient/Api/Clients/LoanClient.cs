using System.Net.Http.Json;
using System.Text.Json;
using CampusLibraryClient.Api.Contracts;
using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;

namespace CampusLibraryClient.Api.Clients;

public sealed class LoanClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<LoanClient> logger
) : BaseApiClient<LoanClient>(factory, json, logger), ILoanClient {

   private const string Base = "camplib/v1";

   // GET /camplib/v1/loans
   public Task<Result<IEnumerable<LoanListItemDto>>> GetBorrowedAsync(
      CancellationToken ct = default
   ) =>
      SendAsync<IEnumerable<LoanListItemDto>>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/loans",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/loans/me
   public Task<Result<IEnumerable<LoanListItemDto>>> GetMyBorrowedAsync(
      CancellationToken ct = default
   ) =>
      SendAsync<IEnumerable<LoanListItemDto>>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/loans/me",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/loans/{id}
   public Task<Result<LoanDetailDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDetailDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/loans/{id}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/loans/me/{id}
   public Task<Result<LoanDetailDto>> GetMyByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDetailDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/loans/me/{id}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/loans
   public Task<Result<LoanDto>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDto>(
         send: () => _http.PostAsJsonAsync(
            requestUri: $"{Base}/loans",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/loans/me
   public Task<Result<LoanDto>> BorrowMyAsync(
      LoanBorrowMeDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDto>(
         send: () => _http.PostAsJsonAsync(
            requestUri: $"{Base}/loans/me",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // PATCH /camplib/v1/loans/{id}/return-at-desk
   public Task<Result<LoanDto>> ReturnAtDeskAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDto>(
         send: () => _http.PatchAsync(
            requestUri: $"{Base}/loans/{id}/return-at-desk",
            content: null,
            cancellationToken: ct
         ),
         ct: ct
      );

   // PATCH /camplib/v1/loans/{id}/renew
   public Task<Result<LoanDto>> RenewAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDto>(
         send: () => _http.PatchAsync(
            requestUri: $"{Base}/loans/{id}/renew",
            content: null,
            cancellationToken: ct
         ),
         ct: ct
      );

   // PATCH /camplib/v1/loans/me/{id}/renew
   public Task<Result<LoanDto>> RenewMyAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      SendAsync<LoanDto>(
         send: () => _http.PatchAsync(
            requestUri: $"{Base}/loans/me/{id}/renew",
            content: null,
            cancellationToken: ct
         ),
         ct: ct
      );
}
