using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
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
   ///    Returns all active books.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of active book resources.</returns>
   // Query all active books through the read model.
   [HttpGet("books", Name = nameof(GetAllBooksAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<BookListItemDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> GetAllBooksAsync(
      CancellationToken ct
   ) {
      var result = await bookReadModel.SelectAllAsync(ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns one active book by id.
   /// </summary>
   /// <param name="id">Book unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested book detail resource.</returns>
   // Query one active book by id through the read model.
   [HttpGet("books/{id:guid}", Name = nameof(GetBookByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<BookDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BookDetailDto>> GetBookByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await bookReadModel.FindByIdAsync(id, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Searches active books by one search criterion.
   /// </summary>
   /// <param name="searchField">Search field: Title, AuthorName or Isbn.</param>
   /// <param name="searchText">Search text.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of matching active book resources.</returns>
   // Search active books through the read model.
   [HttpGet("books/search", Name = nameof(SearchBooksAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<BookListItemDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> SearchBooksAsync(
      [FromQuery] BookSearchField searchField,
      [FromQuery] string searchText,
      CancellationToken ct
   ) {
      var search = new BookSearchDto(
         SearchField: searchField,
         SearchText: searchText
      );

      var result = await bookReadModel.SearchAsync(search, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns all active books assigned to one author.
   /// </summary>
   /// <param name="authorId">Author unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of active books assigned to the author.</returns>
   // Query active books by author id through the read model.
   [HttpGet("books/by-author/{authorId:guid}", Name = nameof(GetBooksByAuthorIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<BookListItemDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> GetBooksByAuthorIdAsync(
      [FromRoute] Guid authorId,
      CancellationToken ct
   ) {
      var result = await bookReadModel.SelectByAuthorIdAsync(authorId, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
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
         return CreatedAtRoute(
            routeName: nameof(GetBookByIdAsync),
            routeValues: new {
               version = HttpContext.GetRequestedApiVersion()?.ToString(),
               id = result.Value.Id
            },
            value: result.Value
         );
      }

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
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
      var result = await bookUseCases.AddBookItemAsync(bookId, dto, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
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
   ///    Assigns an existing author to an existing book.
   /// </summary>
   /// <param name="bookId">Book unique id.</param>
   /// <param name="dto">Author assignment data.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated book resource.</returns>
   // Assign an existing Author through the write-side use case facade.
   [HttpPost("books/{bookId:guid}/authors", Name = nameof(AssignAuthorAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<BookDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<BookDto>> AssignAuthorAsync(
      [FromRoute] Guid bookId,
      [FromBody] BookAssignAuthorDto dto,
      CancellationToken ct
   ) {
      var result = await bookUseCases.AssignAuthorAsync(bookId, dto, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
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
   public async Task<ActionResult<BookDto>> DeactivateBookAsync(
      [FromRoute] Guid bookId,
      CancellationToken ct
   ) {
      var result = await bookUseCases.DeactivateAsync(bookId, ct);
      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.Unauthorized => Unauthorized(problem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
         WebErrorStatus.NotFound => NotFound(problem),
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

* GetAllBooksAsync        -> IBookReadModel.SelectAllAsync
* GetBookByIdAsync        -> IBookReadModel.FindByIdAsync
* SearchBooksAsync        -> IBookReadModel.SearchAsync
* GetBooksByAuthorIdAsync -> IBookReadModel.SelectByAuthorIdAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

* CreateBookAsync     -> IBookUseCases.CreateAsync
* AddBookItemAsync    -> IBookUseCases.AddBookItemAsync
* AssignAuthorAsync   -> IBookUseCases.AssignAuthorAsync
* DeactivateBookAsync -> IBookUseCases.DeactivateAsync

Die Fallunterscheidung im Controller ist bewusst explizit gehalten.
Dadurch sehen Studierende direkt, welcher DomainError.Status zu welcher
HTTP-Antwort führt.

Deactivate ist kein Delete. Ein Book wird fachlich deaktiviert, aber nicht
physisch gelöscht. Das ReadModel entscheidet anschließend, ob inaktive Books
in normalen Listen und Suchen angezeigt werden.

BookItems sind physische Exemplare eines Book. Sie werden über das Book-
Aggregate hinzugefügt. Authors werden über die Book-Author-Beziehung
zugeordnet. Die Join-Tabelle bleibt ein Infrastrukturdetail.

Swagger-Attribute dokumentieren die erwarteten Erfolgs- und Fehlerantworten.
Sie machen die API für Clients und Tests explizit nachvollziehbar.

## Lernziele
* Controller als HTTP-Adapter verstehen
* Unterschied zwischen GET/ReadModel und schreibenden UseCases erkennen
* m:n-Zuordnung zwischen Book und Author über einen HTTP-Endpunkt auslösen
* 1:n-Beziehung zwischen Book und BookItem über einen HTTP-Endpunkt auslösen
* Deactivate als fachliche Zustandsänderung verstehen
* DomainError.Status explizit auf HTTP-Antworten abbilden
* ProblemDetails als standardisiertes Fehlerformat verwenden
* Swagger-Metadaten für API-Dokumentation einsetzen
* Keine Domainlogik im Controller platzieren
  
*/