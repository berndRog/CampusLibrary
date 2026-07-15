using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

// HTTP API controller for Reader resources.
// Translates HTTP requests into calls to read models or use cases.
// Contains no domain logic.

[ApiVersion("1.0")]
// [ApiVersion("1.1")]
// [ApiVersion("1.2")]
// [ApiVersion("2.0")]

[Route("camplib/v{version:apiVersion}")]
[ApiController]

public sealed class ReaderController(
   IReaderReadModel readerReadModel,
   IReaderUseCases readerUseCases
) : ControllerBase {

   /// <summary>
   ///    Returns readers.
   /// </summary>
   /// <param name="includeInactive">
   ///    If true, inactive readers are included. Otherwise only active readers are returned.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of reader resources.</returns>
   // Query all readers through the read model.
   // The default view returns only active readers.
   // Administrative views can include inactive readers by using includeInactive=true.
   [HttpGet("readers", Name = nameof(GetAllReadersAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<ReaderDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> GetAllReadersAsync(
      [FromQuery] bool includeInactive,
      CancellationToken ct
   ) {
      var result = await readerReadModel.SelectAllAsync(
         includeInactive: includeInactive,
         ct: ct
      );

      if(result.IsSuccess)
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
   ///    Returns one reader by id.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="includeInactive">
   ///    If true, inactive readers are included. Otherwise only active readers are returned.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested reader resource.</returns>
   // Query one reader by id through the read model.
   // The default view returns only active readers.
   // Administrative views can include inactive readers by using includeInactive=true.
   [HttpGet("readers/{id:guid}", Name = nameof(GetReaderByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetReaderByIdAsync(
      [FromRoute] Guid id,
      [FromQuery] bool includeInactive,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByIdAsync(
         id: id,
         includeInactive: includeInactive,
         ct: ct
      );

      if(result.IsSuccess)
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
   ///    Returns one reader by email address.
   /// </summary>
   /// <param name="email">Reader email address.</param>
   /// <param name="includeInactive">
   ///    If true, inactive readers are included. Otherwise only active readers are returned.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested reader resource.</returns>
   // Query one reader by email through the read model.
   // The default view returns only active readers.
   // Administrative views can include inactive readers by using includeInactive=true.
   [HttpGet("readers/email", Name = nameof(GetReaderByEmailAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetReaderByEmailAsync(
      [FromQuery] string email,
      [FromQuery] bool includeInactive,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByEmailAsync(
         email: email,
         includeInactive: includeInactive,
         ct: ct
      );

      if(result.IsSuccess)
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
   ///    Creates a new reader.
   /// </summary>
   /// <param name="dto">Reader data used to create the resource.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created reader resource.</returns>
   // Create a new reader through the write-side use case.
   [HttpPost("readers", Name = nameof(CreateReaderAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> CreateReaderAsync(
      [FromBody] ReaderCreateDto dto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      if(result.IsSuccess) {
         var requestedApiVersion = HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;

         return CreatedAtRoute(
            routeName: nameof(GetReaderByIdAsync),
            routeValues: new {
               version = requestedApiVersion?.ToString() ?? "1",
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
   ///    Updates an existing reader.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="dto">Reader data used to update the resource.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated reader resource.</returns>
   // Update an existing reader through the write-side use case.
   [HttpPut("readers/{id:guid}", Name = nameof(UpdateReaderAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> UpdateReaderAsync(
      [FromRoute] Guid id,
      [FromBody] ReaderUpdateDto dto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.UpdateAsync(
         id: id,
         dto: dto,
         ct: ct
      );

      if(result.IsSuccess)
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
   ///    Deactivates an existing reader.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No content on success.</returns>
   // Deactivate an existing reader through the write-side use case.
   // This is a soft delete: the reader remains stored,
   // but is hidden from normal read model queries.
   [HttpDelete("readers/{id:guid}", Name = nameof(DeactivateReaderAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<IActionResult> DeactivateReaderAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await readerUseCases.DeactivateAsync(id, ct);

      if(result.IsSuccess)
         return NoContent();

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

ReadersController ist die HTTP-Schicht des Readers-Moduls.

Der Controller enthält keine Fachlogik. Er entscheidet nur, welcher
Anwendungsbaustein für einen HTTP-Endpunkt aufgerufen wird und wie das
Result in eine HTTP-Antwort übersetzt wird.

GET-Endpunkte verwenden das ReadModel:

- GetAllReadersAsync      -> IReaderReadModel.SelectAllAsync
- GetReaderByIdAsync      -> IReaderReadModel.FindByIdAsync
- GetReaderByEmailAsync   -> IReaderReadModel.FindByEmailAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

- CreateReaderAsync       -> IReaderUseCases.CreateAsync
- UpdateReaderAsync       -> IReaderUseCases.UpdateAsync
- DeactivateReaderAsync   -> IReaderUseCases.DeactivateAsync

Die normale Sicht auf Reader liefert nur aktive Reader. Inaktive Reader
werden nicht über zusätzliche Routen wie /with-inactive abgefragt, sondern
über einen Query-Parameter:

   GET /readers
   GET /readers?includeInactive=true

   GET /readers/{id}
   GET /readers/{id}?includeInactive=true

   GET /readers/email?email=max@example.org
   GET /readers/email?email=max@example.org&includeInactive=true

Dadurch bleibt die API ruhiger: Die Ressource bleibt dieselbe, nur die
Sicht auf diese Ressource wird über einen Parameter erweitert.

Die Fallunterscheidung im Controller ist bewusst explizit gehalten.
Dadurch sehen Studierende direkt, welcher DomainError.Status zu welcher
HTTP-Antwort führt.

DomainProblemDetailsFactory erzeugt nur das standardisierte Fehlerobjekt.
Die Entscheidung über BadRequest, Unauthorized, Forbidden, NotFound oder
Conflict bleibt im Controller sichtbar.

401 Unauthorized und 403 Forbidden sind bereits in Swagger dokumentiert.
Die Endpunkte können später mit [Authorize] und Policies geschützt werden,
ohne die API-Dokumentation grundsätzlich neu aufzubauen.

CreatedAtRoute erzeugt bei erfolgreicher Erstellung eine 201-Created-
Antwort mit Location-Header auf die neu erzeugte Ressource.

Swagger-Attribute dokumentieren die erwarteten Erfolgs- und Fehlerantworten.
Sie machen die API für Clients und Tests explizit nachvollziehbar.

Lernziele
---------

- Controller als HTTP-Adapter verstehen
- Unterschied zwischen GET/ReadModel und schreibenden UseCases erkennen
- REST-Verhalten von 201 Created und Location-Header nachvollziehen
- REST-Verhalten von 200 OK bei Update und 204 NoContent bei Delete verstehen
- DomainError.Status explizit auf HTTP-Antworten abbilden
- 401 Unauthorized und 403 Forbidden unterscheiden
- ProblemDetails als standardisiertes Fehlerformat verwenden
- Swagger-Metadaten für API-Dokumentation einsetzen
- Query-Parameter zur Erweiterung einer Standardsicht verwenden
- Keine Domainlogik im Controller platzieren
*/