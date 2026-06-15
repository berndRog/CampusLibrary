using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

// HTTP API controller for Author resources.
// Translates HTTP requests into calls to read models or use cases.
// Contains no domain logic.
[ApiVersion("1.0")]
[Route("camplib/v{version:apiVersion}")]
[ApiController]
public sealed class AuthorsController(
   IAuthorReadModel authorReadModel,
   IAuthorUseCases authorUseCases
) : ControllerBase {
   /// <summary>
   ///    Returns all active authors.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of active author resources.</returns>
   // Query all active authors through the read model.
   [HttpGet("authors", Name = nameof(GetAllAuthorsAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<AuthorDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<AuthorDto>>> GetAllAuthorsAsync(
      CancellationToken ct
   ) {
      var result = await authorReadModel.SelectAllAsync(ct);
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
   ///    Returns one active author by id.
   /// </summary>
   /// <param name="id">Author unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested author resource.</returns>
   // Query one active author by id through the read model.
   [HttpGet("authors/{id:guid}", Name = nameof(GetAuthorByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<AuthorDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AuthorDto>> GetAuthorByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await authorReadModel.FindByIdAsync(id, ct);
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
   ///    Searches active authors by firstname, lastname or display name.
   /// </summary>
   /// <param name="searchText">Search text.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of matching active author resources.</returns>
   // Search active authors through the read model.
   [HttpGet("authors/search", Name = nameof(SearchAuthorsAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<AuthorDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<AuthorDto>>> SearchAuthorsAsync(
      [FromQuery] string searchText,
      CancellationToken ct
   ) {
      var result = await authorReadModel.SearchAsync(searchText, ct);
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
   ///    Creates a new author.
   /// </summary>
   /// <param name="dto">Author data used to create the resource.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created author resource.</returns>
   // Create a new author through the write-side use case facade.
   [HttpPost("authors", Name = nameof(CreateAuthorAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<AuthorDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<AuthorDto>> CreateAuthorAsync(
      [FromBody] AuthorCreateDto dto,
      CancellationToken ct
   ) {
      var result = await authorUseCases.CreateAsync(dto, ct);

      if (result.IsSuccess) {
         return CreatedAtRoute(
            routeName: nameof(GetAuthorByIdAsync),
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
   ///    Deactivates an existing author.
   /// </summary>
   /// <param name="id">Author unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The deactivated author resource.</returns>
   // Deactivate an existing author through the write-side use case facade.
   [HttpPatch("authors/{id:guid}/deactivate", Name = nameof(DeactivateAuthorAsync))]
   [Produces("application/json")]
   [ProducesResponseType<AuthorDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AuthorDto>> DeactivateAuthorAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await authorUseCases.DeactivateAsync(id, ct);
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

AuthorsController ist die HTTP-Schicht für Authors im Catalog-Modul.

Der Controller enthält keine Fachlogik. Er entscheidet nur, welcher
Anwendungsbaustein für einen HTTP-Endpunkt aufgerufen wird und wie das
Result in eine HTTP-Antwort übersetzt wird.

GET-Endpunkte verwenden das ReadModel:

* GetAllAuthorsAsync   -> IAuthorReadModel.SelectAllAsync
* GetAuthorByIdAsync   -> IAuthorReadModel.FindByIdAsync
* SearchAuthorsAsync   -> IAuthorReadModel.SearchAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

* CreateAuthorAsync      -> IAuthorUseCases.CreateAsync
* DeactivateAuthorAsync  -> IAuthorUseCases.DeactivateAsync

Die Fallunterscheidung im Controller ist bewusst explizit gehalten.
Dadurch sehen Studierende direkt, welcher DomainError.Status zu welcher
HTTP-Antwort führt.

Deactivate ist kein Delete. Ein Author wird fachlich deaktiviert, aber nicht
physisch gelöscht. Das ReadModel entscheidet anschließend, ob inaktive Authors
in normalen Listen und Suchen angezeigt werden.

Swagger-Attribute dokumentieren die erwarteten Erfolgs- und Fehlerantworten.
Sie machen die API für Clients und Tests explizit nachvollziehbar.

## Lernziele

* Controller als HTTP-Adapter verstehen
* Unterschied zwischen GET/ReadModel und schreibenden UseCases erkennen
* Deactivate als fachliche Zustandsänderung verstehen
* DomainError.Status explizit auf HTTP-Antworten abbilden
* ProblemDetails als standardisiertes Fehlerformat verwenden
* Swagger-Metadaten für API-Dokumentation einsetzen
* Keine Domainlogik im Controller platzieren
  */