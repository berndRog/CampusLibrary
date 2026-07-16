using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._1_Web.Security;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using Microsoft.AspNetCore.Authorization;
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
   ///    Returns the reader profile of the currently authenticated reader.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The current reader profile.</returns>
   [Authorize(Policy = CampusLibraryPolicies.ReadersOnly)]
   [HttpGet("readers/me", Name = nameof(GetReaderProfileAsync))]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> GetReaderProfileAsync(
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindMeAsync(
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
   ///    Provisions the domain reader for the currently authenticated user.
   /// </summary>
   /// <param name="id">Reader identity.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No response body.</returns>
   [Authorize(Policy = CampusLibraryPolicies.ReadersOnly)]
   [HttpPost("readers/me/provision", Name = nameof(CreateReaderMeProvisionAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<IActionResult> CreateReaderMeProvisionAsync(
      [FromQuery] string? id,
      CancellationToken ct
   ) {
      var result = await readerUseCases.ProvisionMeAsync(id, ct);
      if(result.IsSuccess)
         return NoContent();

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
   ///    Completes the initial profile of the currently authenticated reader.
   /// </summary>
   /// <param name="meDto">Profile data entered by the reader.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated current reader profile.</returns>
   [Authorize(Policy = CampusLibraryPolicies.ReadersOnly)]
   [HttpPut("readers/me/profile", Name = nameof(PutReaderMeProfileAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> PutReaderMeProfileAsync(
      [FromBody] ReaderProfileDto meDto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.UpdateMeProfileAsync(meDto, ct);
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
   ///    Updates mutable data of the currently authenticated reader.
   /// </summary>
   /// <param name="dto">Mutable reader data. Null properties remain unchanged.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated current reader.</returns>
   [Authorize(Policy = CampusLibraryPolicies.ReadersOnly)]
   [HttpPut("readers/me/update", Name = nameof(PutReaderMeUpdateAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<ReaderDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<ReaderDto>> PutReaderMeUpdateAsync(
      [FromBody] ReaderUpdateDto dto,
      CancellationToken ct
   ) {
      var result = await readerUseCases.UpdateMeAsync(dto, ct);
      if(result.IsSuccess)
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
   ///    Deactivates an existing reader.
   /// </summary>
   /// <param name="id">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>No content on success.</returns>
   // Deactivate an existing reader through the write-side use case.
   // This is a soft delete: the reader remains stored,
   // but is hidden from normal read model queries.
   [Authorize(Policy = CampusLibraryPolicies.EmployeesOnly)]
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

ReaderController ist die HTTP-Schicht des Readers-Moduls.

Lesende Endpunkte verwenden IReaderReadModel:

- GET /readers
- GET /readers/{id}
- GET /readers/email
- GET /readers/me

Schreibende Endpunkte verwenden IReaderUseCases:

- POST /readers/me/provision
- PUT /readers/me/profile
- PUT /readers/me/update
- DELETE /readers/{id}

Part 6 trennt die technische Identität vom fachlichen Reader. Bei /me-Endpunkten
wird die ReaderId nicht vom Client übertragen. Das Readers-Modul ermittelt den
aktuellen Reader über das Subject des authentifizierten Benutzers.

DomainProblemDetailsFactory erzeugt ausschließlich die einheitliche Form des
ProblemDetails-Objekts. Die Controller-Methode ordnet WebErrorStatus sichtbar
dem dokumentierten HTTP-Status zu. Dadurch stehen Swagger-Angaben und
tatsächliche Fehlerbehandlung an derselben Stelle.
*/
