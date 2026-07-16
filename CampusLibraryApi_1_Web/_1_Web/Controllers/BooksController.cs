using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

// HTTP API controller for Book resources.
// Translates HTTP requests into calls to read models or use cases.
// Contains no domain logic.
[ApiVersion("1.0")]
[Route("camplib/v{version:apiVersion}")]
[ApiController]
public sealed class BooksController(
   IBookReadModel bookReadModel,
   IBookUseCases bookUseCases
) : ControllerBase {

   /// <summary>
   ///    Returns books.
   /// </summary>
   /// <param name="includeInactive">
   ///    If true, inactive books are included. The default is false.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of book resources.</returns>
   // Query books through the read model.
   [HttpGet("books", Name = nameof(GetAllBooksAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<BookDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<BookDto>>> GetAllBooksAsync(
      [FromQuery] bool includeInactive = false,
      CancellationToken ct = default
   ) {
      var result = await bookReadModel.SelectAllAsync(
         includeInactive, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns one book by id.
   /// </summary>
   /// <param name="id">Book unique id.</param>
   /// <param name="includeInactive">
   ///    If true, inactive books are included. The default is false.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested book detail resource.</returns>
   // Query one book by id through the read model.
   [HttpGet("books/{id:guid}", Name = nameof(GetBookByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<BookDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BookDto>> GetBookByIdAsync(
      [FromRoute] Guid id,
      [FromQuery] bool includeInactive = false,
      CancellationToken ct = default
   ) {
      var result = await bookReadModel.FindByIdAsync(
         id, includeInactive, ct);

      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error, HttpContext);
      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns the current loan blockers for book deactivation.
   /// </summary>
   /// <param name="bookId">Book unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>Current loans that prevent deactivation.</returns>
   [Authorize(Roles = "Employee")]
   [HttpGet("books/{bookId:guid}/deactivation-info", Name = nameof(GetBookDeactivationInfoAsync))]
   [Produces("application/json")]
   [ProducesResponseType<BookDeactivationInfoDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BookDeactivationInfoDto>> GetBookDeactivationInfoAsync(
      [FromRoute] Guid bookId,
      CancellationToken ct = default
   ) {
      var result = await bookReadModel.FindDeactivationInfoAsync(bookId, ct);

      if(result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Searches books by one search criterion.
   /// </summary>
   /// <param name="searchField">
   ///    Search field: Title, AuthorLastName or Isbn.
   /// </param>
   /// <param name="searchText">Search text.</param>
   /// <param name="includeInactive">
   ///    If true, inactive books are included in the search. The default is false.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of matching book resources.</returns>
   // Search books through the read model.
   [HttpGet("books/search", Name = nameof(SearchBooksAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<BookDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<BookDto>>> SearchBooksAsync(
      [FromQuery] BookSearchField searchField,
      [FromQuery] string searchText,
      [FromQuery] bool includeInactive = false,
      CancellationToken ct = default
   ) {
      var result = await bookReadModel.SearchAsync(
         searchField, searchText, includeInactive, ct);
      
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Creates a new book.
   /// </summary>
   /// <param name="dto">Book data used to create the resource.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created book resource.</returns>
   // Create a new book through the write-side use case facade.
   [HttpPost("books", Name = nameof(CreateBookAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<BookDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<BookDto>> CreateBookAsync(
      [FromBody] BookCreateDto dto,
      CancellationToken ct
   ) {
      var result = await bookUseCases.CreateAsync(dto, ct);
      if (result.IsSuccess) {
         var requestedApiVersion =
            HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;

         return CreatedAtRoute(
            routeName: nameof(GetBookByIdAsync),
            routeValues: new {
               version = requestedApiVersion?.ToString() ?? "1",
               id = result.Value.Id
            },
            value: result.Value
         );
      }

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Adds a physical item to an existing book.
   /// </summary>
   /// <param name="bookId">Book unique id.</param>
   /// <param name="dto">Book item data used to create the physical item.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created book item resource.</returns>
   // Add a physical BookItem through the write-side use case facade.
   [HttpPost("books/{bookId:guid}/items", Name = nameof(AddBookItemAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<BookItemDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<BookItemDto>> AddBookItemAsync(
      [FromRoute] Guid bookId,
      [FromBody] BookItemAddDto dto,
      CancellationToken ct
   ) {
      var result = await bookUseCases.AddBookItemAsync(
         id: bookId,
         dto: dto,
         ct: ct
      );

      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Deactivates an existing book.
   /// </summary>
   /// <param name="bookId">Book unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The deactivated book resource.</returns>
   // Deactivate an existing book through the write-side use case facade.
   [HttpPatch("books/{bookId:guid}/deactivate", Name = nameof(DeactivateBookAsync))]
   [Produces("application/json")]
   [ProducesResponseType<BookDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<BookDto>> DeactivateBookAsync(
      [FromRoute] Guid bookId,
      CancellationToken ct
   ) {
      var result = await bookUseCases.DeactivateAsync(
         id: bookId,
         ct: ct
      );

      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }
}

/*
Didaktik
--------

BooksController ist die HTTP-Schicht für Books im Catalog-Modul.

Der Controller enthält keine Fachlogik. Er entscheidet nur, welcher
Anwendungsbaustein für einen HTTP-Endpunkt aufgerufen wird und wie das
Result in eine HTTP-Antwort übersetzt wird.

GET-Endpunkte verwenden das ReadModel:

* GetAllBooksAsync -> IBookReadModel.SelectAllAsync
* GetBookByIdAsync -> IBookReadModel.FindByIdAsync
* SearchBooksAsync -> IBookReadModel.SearchAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

* CreateBookAsync     -> IBookUseCases.CreateAsync
* AddBookItemAsync    -> IBookUseCases.AddBookItemAsync
* DeactivateBookAsync -> IBookUseCases.DeactivateAsync

Die normale Sicht auf den Catalog liefert nur aktive Books:

   includeInactive = false

Eine administrative oder interne Sicht kann inaktive Books einbeziehen:

   includeInactive = true

Die Ressource bleibt dabei dieselbe. Es gibt keinen zusätzlichen Endpunkt wie
/books/with-inactive. Stattdessen wird die Sicht auf die Ressource über einen
Query-Parameter erweitert:

   GET /books?includeInactive=true
   GET /books/{id}?includeInactive=true
   GET /books/search?searchField=Title&searchText=clean&includeInactive=true

Deactivate ist kein Delete. Ein Book wird fachlich deaktiviert, aber nicht
physisch gelöscht. Das ReadModel entscheidet anschließend, ob inaktive Books
in Listen, Details und Suchen sichtbar sind.

BookItems sind physische Exemplare eines Book. Sie werden über das Book-
Aggregate hinzugefügt. Ihr Status beschreibt den fachlichen Zustand eines
Exemplars, zum Beispiel Available, Unavailable, Lost oder Damaged.

In dieser reduzierten Catalog-Version gibt es keine eigene Author-Entity mehr.
Autorinnen und Autoren werden als kommaseparierter Text im Book gespeichert.
Eine Suche nach Autor erfolgt deshalb über SearchAsync mit dem Suchfeld
AuthorLastName. Einen Endpunkt wie /books/by-author/{authorId} gibt es nicht
mehr.

Swagger-Attribute dokumentieren die erwarteten Erfolgs- und Fehlerantworten.
Sie machen die API für Clients und Tests explizit nachvollziehbar.

## Lernziele
* Controller als HTTP-Adapter verstehen
* Unterschied zwischen GET/ReadModel und schreibenden UseCases erkennen
* ReadModel-Parameter für unterschiedliche Sichten verwenden
* 1:n-Beziehung zwischen Book und BookItem über einen HTTP-Endpunkt auslösen
* Deactivate als fachliche Zustandsänderung verstehen
* DomainError.Status explizit auf HTTP-Antworten abbilden
* ProblemDetails als standardisiertes Fehlerformat verwenden
* Swagger-Metadaten für API-Dokumentation einsetzen
* Keine Domainlogik im Controller platzieren
*/
