using System.Net.Http.Json;
using System.Text.Json;
using CampusLibraryClient.Api.Contracts;
using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;
using CampusLibraryClient.Core.Utils;

namespace CampusLibraryClient.Api.Clients;

public sealed class BookClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<BookClient> logger
) : BaseApiClient<BookClient>(
   factory: factory,
   json: json,
   logger: logger
), IBookClient {

   private const string Base = "camplib/v1";

   // GET /camplib/v1/books?includeInactive=false
   public Task<Result<IEnumerable<BookDto>>> GetAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<IEnumerable<BookDto>>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/books?includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/books/{id}?includeInactive=false
   public Task<Result<BookDto>> GetByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<BookDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/books/{id}?includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/books/search?searchField=Title&searchText=Clean%20Code&includeInactive=false
   public Task<Result<IEnumerable<BookDto>>> SearchAsync(
      BookSearchField searchField,
      string searchText,
      bool includeInactive = false,
      CancellationToken ct = default
   ) =>
      SendAsync<IEnumerable<BookDto>>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/books/search?searchField={searchField}&searchText={Uri.EscapeDataString(searchText)}&includeInactive={QueryStringBuilder.Bool(includeInactive)}",
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/books
   public Task<Result<BookDto>> CreateAsync(
      BookCreateDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<BookDto>(
         send: () => _http.PostAsJsonAsync(
            requestUri: $"{Base}/books",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // POST /camplib/v1/books/{bookId}/items
   public Task<Result<BookItemDto>> AddBookItemAsync(
      Guid bookId,
      BookItemAddDto dto,
      CancellationToken ct = default
   ) =>
      SendAsync<BookItemDto>(
         send: () => _http.PostAsJsonAsync(
            requestUri: $"{Base}/books/{bookId}/items",
            value: dto,
            options: _json,
            cancellationToken: ct
         ),
         ct: ct
      );

   // GET /camplib/v1/books/{bookId}/deactivation-info
   public Task<Result<BookDeactivationInfoDto>> GetDeactivationInfoAsync(
      Guid bookId,
      CancellationToken ct = default
   ) =>
      SendAsync<BookDeactivationInfoDto>(
         send: () => _http.GetAsync(
            requestUri: $"{Base}/books/{bookId}/deactivation-info",
            cancellationToken: ct
         ),
         ct: ct
      );

   // PATCH /camplib/v1/books/{bookId}/deactivate
   public Task<Result<BookDto>> DeactivateAsync(
      Guid bookId,
      CancellationToken ct = default
   ) =>
      SendAsync<BookDto>(
         send: () => _http.PatchAsync(
            requestUri: $"{Base}/books/{bookId}/deactivate",
            content: null,
            cancellationToken: ct
         ),
         ct: ct
      );
}
