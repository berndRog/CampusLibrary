using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Readers._1_Ports;
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

public sealed class ReadersController(
   IReaderReadModel readerReadModel,
   IReaderUseCases readerUseCases
) : ControllerBase {

   /// <summary>
   ///    Returns all active readers.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of reader resources.</returns>
   // Query all readers through the read model.
   [HttpGet("readers", Name = nameof(GetAllAsync))]
   // [Produces("application/json")]
   // [ProducesResponseType<IReadOnlyList<ReaderDto>>(StatusCodes.Status200OK)]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> GetAllAsync(CancellationToken ct) {
      var result = await readerReadModel.SelectAllAsync(ct);

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
   ///    Returns one reader by id.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested reader resource.</returns>
   // Query one reader by id through the read model.
   [HttpGet("readers/{id:guid}", Name = nameof(GetByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByIdAsync(id, ct);

      if (result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);

      return result.Error.Status switch {
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
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested reader resource.</returns>
   // Query one reader by email through the read model.
   [HttpGet("readers/email", Name = nameof(GetByEmailAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetByEmailAsync(
      [FromQuery] string email,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByEmailAsync(email, ct);

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
   ///    Creates a new reader.
   /// </summary>
   /// <param name="dto">Reader data used to create the resource.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created reader resource.</returns>
   // Create a new reader through the write-side use case.
   [HttpPost("readers", Name = nameof(CreateAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> CreateAsync(
      [FromBody] ReaderCreateDto dto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.CreateAsync(dto, ct);

      if (result.IsSuccess) {
         return CreatedAtRoute(
            nameof(GetByIdAsync),
            new { id = result.Value.Id },
            result.Value
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
   [HttpPut("readers/{id:guid}", Name = nameof(UpdateAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> UpdateAsync(
      [FromRoute] Guid id,
      [FromBody] ReaderUpdateDto dto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.UpdateAsync(id, dto, ct);

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
   ///    Deactivates an existing reader.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No content on success.</returns>
   // Deactivate an existing reader through the write-side use case.
   // This is a soft delete: the reader remains stored,
   // but is hidden from normal read model queries.
   [HttpDelete("readers/{id:guid}", Name = nameof(DeactivateAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<IActionResult> DeactivateAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await readerUseCases.DeactivateAsync(
         id,
         ct
      );

      if (result.IsSuccess)
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
   
   /// <summary>
   ///    Returns one reader by id, including inactive readers.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested reader resource, even if it is inactive.</returns>
   // Query one reader by id through the read model, including inactive readers.
   // This endpoint is intended for administrative or internal views.
   [HttpGet("readers/{id:guid}/with-inactive", Name = nameof(GetByIdWithInactiveAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetByIdWithInactiveAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByIdWithInactiveAsync(
         id,
         ct
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
         _ => BadRequest(problem)
      };
   }
   
   /// <summary>
   ///    Returns all readers, including inactive readers.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of reader resources, including inactive readers.</returns>
   // Query all readers through the read model, including inactive readers.
   // This endpoint is intended for administrative or internal views.
   [HttpGet("readers/with-inactive", Name = nameof(GetAllWithInactiveAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<ReaderDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> GetAllWithInactiveAsync(
      CancellationToken ct
   ) {
      var result = await readerReadModel.SelectAllWithInactiveAsync(
         ct
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

- GetAllAsync      -> IReaderReadModel.SelectAllAsync
- GetByIdAsync     -> IReaderReadModel.FindByIdAsync
- GetByEmailAsync  -> IReaderReadModel.FindByEmailAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

- CreateAsync      -> IReaderUseCases.CreateAsync
- UpdateAsync      -> IReaderUseCases.UpdateAsync
- DeactivateAsync  -> IReaderUseCases.DeactivatedAsync

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
- Keine Domainlogik im Controller platzieren
*/
