using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

// HTTP API controller for Loan resources.
// Translates HTTP requests into calls to read models or use cases.
// Contains no domain logic.
[ApiVersion("1.0")]
[Route("camplib/v{version:apiVersion}")]
[ApiController]
public sealed class LoansController(
   ILoanReadModel loanReadModel,
   ILoanUseCases loanUseCases,
   IClock clock
) : ControllerBase {

   /// <summary>
   ///    Returns one loan by id.
   /// </summary>
   /// <param name="id">Loan unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The requested loan resource.</returns>
   // Query one loan through the read model.
   [HttpGet("loans/{id:guid}", Name = nameof(GetLoanByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> GetLoanByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindByIdAsync(
         id: id,
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
   ///    Returns all active loans.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of active loans.</returns>
   // Query all active loans through the read model.
   [HttpGet("loans/active", Name = nameof(GetAllActiveLoansAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetAllActiveLoansAsync(
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindAllActiveAsync(
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
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns all active loans for one reader.
   /// </summary>
   /// <param name="readerId">Reader unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of active loans for the reader.</returns>
   // Query active loans of one reader through the read model.
   [HttpGet("readers/{readerId:guid}/loans/active", Name = nameof(GetLoanByReaderIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetLoanByReaderIdAsync(
      [FromRoute] Guid readerId,
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindActiveByReaderIdAsync(
         readerId: readerId,
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
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns all overdue active loans.
   /// </summary>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A list of overdue active loans.</returns>
   // Query overdue loans through the read model.
   [HttpGet("loans/overdue", Name = nameof(GetAllOverdueAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetAllOverdueAsync(
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindAllOverdueAsync(
         utcNow: clock.UtcNow,
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
         WebErrorStatus.Conflict => Conflict(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Borrows one concrete book item for one reader.
   /// </summary>
   /// <param name="dto">Loan creation data.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created loan resource.</returns>
   // Create a loan through the write-side use case.
   [HttpPost("loans", Name = nameof(BorrowAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> BorrowAsync(
      [FromBody] LoanCreateDto dto,
      CancellationToken ct
   ) {
      var result = await loanUseCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      if (result.IsSuccess) {
         return CreatedAtRoute(
            nameof(GetLoanByIdAsync),
            new { id = result.Value.Id },
            result.Value
         );
      }

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
   ///    Returns a borrowed book item at the service desk.
   /// </summary>
   /// <param name="id">Loan unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated loan resource.</returns>
   // Return a borrowed book item through the write-side use case.
   [HttpPatch("loans/{id:guid}/return-at-desk", Name = nameof(ReturnAtDeskAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> ReturnAtDeskAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanUseCases.ReturnAtDeskAsync(
         loanId: id,
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
   ///    Renews an active loan.
   /// </summary>
   /// <param name="id">Loan unique id.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The updated loan resource.</returns>
   // Renew an active loan through the write-side use case.
   [HttpPatch("loans/{id:guid}/renew", Name = nameof(RenewAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> RenewAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanUseCases.RenewAsync(
         loanId: id,
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
Lernziele und Didaktik
----------------------

Dieser Controller ist die HTTP-Schicht des Loans-Moduls.

Der Controller enthält keine Fachlogik. Er entscheidet nur, welcher
Anwendungsbaustein für einen HTTP-Endpunkt aufgerufen wird und wie das Result
in eine HTTP-Antwort übersetzt wird.

GET-Endpunkte verwenden das ReadModel:

- GetByIdAsync             -> ILoanReadModel.FindByIdAsync
- GetAllActiveAsync        -> ILoanReadModel.FindAllActiveAsync
- GetActiveByReaderIdAsync -> ILoanReadModel.FindActiveByReaderIdAsync
- GetAllOverdueAsync       -> ILoanReadModel.FindAllOverdueAsync

Schreibende Endpunkte verwenden die UseCase-Fassade:

- BorrowAsync       -> ILoanUseCases.BorrowAsync
- ReturnAtDeskAsync -> ILoanUseCases.ReturnAtDeskAsync
- RenewAsync        -> ILoanUseCases.RenewAsync

Der Controller verwendet kein Repository und keinen DbContext. Repositories
gehören zu UseCases. ReadModels gehören zur Query-Seite. Dadurch bleibt die
Trennung zwischen HTTP, Anwendungsschicht und Persistenz sichtbar.

BorrowAsync erzeugt bei Erfolg eine 201-Created-Antwort mit Location-Header
auf die neu erzeugte Loan-Ressource.

ReturnAtDeskAsync und RenewAsync verändern vorhandene Loans und liefern bei
Erfolg die aktualisierte Ressource mit 200 OK zurück.

GetAllOverdueAsync verwendet IClock, damit der aktuelle Zeitpunkt testbar
bleibt. Der Controller verwendet nicht direkt DateTime.UtcNow.

Die Fallunterscheidung bei Fehlern ist bewusst explizit gehalten. Dadurch
sehen Studierende direkt, welcher DomainError.Status zu welcher HTTP-Antwort
führt:

- BadRequest   -> 400
- Unauthorized -> 401
- Forbidden    -> 403
- NotFound     -> 404
- Conflict     -> 409

DomainProblemDetailsFactory erzeugt das standardisierte Fehlerobjekt. Die
HTTP-Schicht entscheidet, welcher Statuscode zurückgegeben wird.
*/