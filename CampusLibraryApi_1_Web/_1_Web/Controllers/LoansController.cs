using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

/// <summary>
///    HTTP API controller for loan resources.
/// </summary>
/// <remarks>
///    Loans represent current borrowings of concrete book items.
///    A stored loan means that the referenced book item is borrowed.
///    Returning the item deletes the loan.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("camplib/v{version:apiVersion}")]
public sealed class LoansController(
   ILoanReadModel loanReadModel,
   ILoanUseCases loanUseCases,
   IReaderReadModel readerReadModel
) : ControllerBase {

   /// <summary>
   ///    Returns all currently borrowed loans.
   /// </summary>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>
   ///    A list of currently borrowed loans enriched with reader and book item data.
   /// </returns>
   /// <response code="200">Returns the list of borrowed loans.</response>
   /// <response code="400">The request is invalid.</response>
   [HttpGet("loans", Name = nameof(GetBorrowedLoansAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetBorrowedLoansAsync(
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindAllBorrowedAsync(
         ct: ct
      );

      if(result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Returns all current loans of the authenticated Reader.
   /// </summary>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>The current Reader's borrowed loans.</returns>
   /// <response code="200">Returns the Reader's current loans.</response>
   /// <response code="401">The request is not authenticated.</response>
   /// <response code="403">The authenticated user is not a Reader.</response>
   /// <response code="404">No provisioned Reader exists for the token subject.</response>
   [Authorize(Roles = "Reader")]
   [HttpGet("loans/me", Name = nameof(GetMyBorrowedLoansAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetMyBorrowedLoansAsync(
      CancellationToken ct
   ) {
      var readerResult = await readerReadModel.FindMeAsync(
         ct: ct
      );

      if(readerResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            readerResult.Error,
            HttpContext
         );

         return readerResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            _ => BadRequest(problem)
         };
      }

      var result = await loanReadModel.FindBorrowedByReaderIdAsync(
         readerId: readerResult.Value.Id,
         ct: ct
      );

      if(result.IsSuccess)
         return Ok(result.Value);

      var loanProblem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(loanProblem),
         WebErrorStatus.Unauthorized => Unauthorized(loanProblem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, loanProblem),
         WebErrorStatus.NotFound => NotFound(loanProblem),
         _ => BadRequest(loanProblem)
      };
   }

   /// <summary>
   ///    Returns one current loan of the authenticated Reader.
   /// </summary>
   /// <param name="id">The unique id of the loan.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>The requested loan if it belongs to the current Reader.</returns>
   /// <response code="200">Returns the requested Reader loan.</response>
   /// <response code="401">The request is not authenticated.</response>
   /// <response code="403">The authenticated user is not a Reader.</response>
   /// <response code="404">The loan does not exist for the current Reader.</response>
   [Authorize(Roles = "Reader")]
   [HttpGet("loans/me/{id:guid}", Name = nameof(GetMyLoanByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> GetMyLoanByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var readerResult = await readerReadModel.FindMeAsync(
         ct: ct
      );

      if(readerResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            readerResult.Error,
            HttpContext
         );

         return readerResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            _ => BadRequest(problem)
         };
      }

      var result = await loanReadModel.FindByIdForReaderAsync(
         id: id,
         readerId: readerResult.Value.Id,
         ct: ct
      );

      if(result.IsSuccess)
         return Ok(result.Value);

      var loanProblem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(loanProblem),
         WebErrorStatus.Unauthorized => Unauthorized(loanProblem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, loanProblem),
         WebErrorStatus.NotFound => NotFound(loanProblem),
         _ => BadRequest(loanProblem)
      };
   }

   /// <summary>
   ///    Returns one loan by its id.
   /// </summary>
   /// <param name="id">The unique id of the loan.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>
   ///    The requested loan enriched with reader and book item details.
   /// </returns>
   /// <response code="200">Returns the requested loan.</response>
   /// <response code="400">The loan id is invalid.</response>
   /// <response code="404">No loan with the given id exists.</response>
   [HttpGet("loans/{id:guid}", Name = nameof(GetLoanByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> GetLoanByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindByIdAsync(
         id: id,
         ct: ct
      );

      if(result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         result.Error,
         HttpContext
      );

      return result.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

   /// <summary>
   ///    Borrows one concrete book item for the authenticated Reader.
   /// </summary>
   /// <param name="dto">The self-service borrow request containing the BookItem id.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>The created loan.</returns>
   /// <response code="201">The loan was created successfully.</response>
   /// <response code="400">The request is invalid.</response>
   /// <response code="401">The request is not authenticated.</response>
   /// <response code="403">The authenticated user is not a Reader.</response>
   /// <response code="404">The Reader or BookItem does not exist.</response>
   /// <response code="409">The BookItem or Book is already borrowed.</response>
   [Authorize(Roles = "Reader")]
   [HttpPost("loans/me", Name = nameof(BorrowMyBookItemAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> BorrowMyBookItemAsync(
      [FromBody] LoanBorrowMeDto dto,
      CancellationToken ct
   ) {
      var readerResult = await readerReadModel.FindMeAsync(
         ct: ct
      );

      if(readerResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            readerResult.Error,
            HttpContext
         );

         return readerResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var result = await loanUseCases.BorrowAsync(
         dto: new LoanCreateDto(
            ReaderId: readerResult.Value.Id,
            BookItemId: dto.BookItemId,
            Id: dto.Id
         ),
         ct: ct
      );

      if(result.IsFailure) {
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

      var loanResult = await loanReadModel.FindByIdForReaderAsync(
         id: result.Value,
         readerId: readerResult.Value.Id,
         ct: ct
      );
      if(loanResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            loanResult.Error,
            HttpContext
         );

         return loanResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var requestedApiVersion =
         HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;

      return CreatedAtRoute(
         routeName: nameof(GetMyLoanByIdAsync),
         routeValues: new {
            version = requestedApiVersion?.ToString() ?? "1",
            id = result.Value
         },
         value: loanResult.Value
      );
   }

   /// <summary>
   ///    Borrows one concrete book item for one reader.
   /// </summary>
   /// <param name="dto">
   ///    The borrow request containing reader id, book item id and optional loan id.
   /// </param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>
   ///    The created loan.
   /// </returns>
   /// <response code="201">The loan was created successfully.</response>
   /// <response code="400">The borrow request is invalid.</response>
   /// <response code="404">The reader or book item does not exist.</response>
   /// <response code="409">
   ///    The book item is not available, is already borrowed, or the reader
   ///    already has another copy of the same book on loan.
   /// </response>
   [HttpPost("loans", Name = nameof(BorrowBookItemAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> BorrowBookItemAsync(
      [FromBody] LoanCreateDto dto,
      CancellationToken ct
   ) {
      var result = await loanUseCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      if(result.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            result.Error,
            HttpContext
         );

         return result.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var loanResult = await loanReadModel.FindByIdAsync(result.Value, ct);
      if(loanResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            loanResult.Error,
            HttpContext
         );

         return loanResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var requestedApiVersion =
         HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;

      return CreatedAtRoute(
         routeName: nameof(GetLoanByIdAsync),
         routeValues: new {
            version = requestedApiVersion?.ToString() ?? "1",
            id = result.Value
         },
         value: loanResult.Value
      );
   }

   /// <summary>
   ///    Returns a borrowed book item at the service desk.
   /// </summary>
   /// <param name="id">The unique id of the loan to return.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>No response body.</returns>
   /// <response code="204">The loan was returned and deleted successfully.</response>
   /// <response code="400">The loan id is invalid.</response>
   /// <response code="404">No loan with the given id exists.</response>
   [HttpPatch("loans/{id:guid}/return-at-desk", Name = nameof(ReturnLoanAtDeskAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<IActionResult> ReturnLoanAtDeskAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanUseCases.ReturnAtDeskAsync(id, ct);

      if(result.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            result.Error, HttpContext);

         return result.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.NotFound => NotFound(problem),
            _ => BadRequest(problem)
         };
      }

      return NoContent();
   }

   /// <summary>
   ///    Renews one current loan of the authenticated Reader.
   /// </summary>
   /// <param name="id">The unique id of the loan.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>The renewed loan.</returns>
   /// <response code="200">The Reader's loan was renewed.</response>
   /// <response code="401">The request is not authenticated.</response>
   /// <response code="403">The authenticated user is not a Reader.</response>
   /// <response code="404">The loan does not exist for the current Reader.</response>
   /// <response code="409">The loan cannot be renewed.</response>
   [HttpPatch("loans/me/{id:guid}/renew", Name = nameof(RenewMyLoanAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> RenewMyLoanAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var readerResult = await readerReadModel.FindMeAsync(
         ct: ct
      );

      if(readerResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            readerResult.Error,
            HttpContext
         );

         return readerResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var ownershipResult = await loanReadModel.FindByIdForReaderAsync(
         id: id,
         readerId: readerResult.Value.Id,
         ct: ct
      );

      if(ownershipResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(
            ownershipResult.Error,
            HttpContext
         );

         return ownershipResult.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(problem),
            WebErrorStatus.Unauthorized => Unauthorized(problem),
            WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var result = await loanUseCases.RenewAsync(
         loanId: id,
         ct: ct
      );
      if(result.IsFailure) {
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

      var renewedLoan = await loanReadModel.FindByIdForReaderAsync(
         id: result.Value,
         readerId: readerResult.Value.Id,
         ct: ct
      );

      if(renewedLoan.IsSuccess)
         return Ok(renewedLoan.Value);

      var loanProblem = DomainProblemDetailsFactory.FromDomainError(
         renewedLoan.Error,
         HttpContext
      );

      return renewedLoan.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(loanProblem),
         WebErrorStatus.Unauthorized => Unauthorized(loanProblem),
         WebErrorStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, loanProblem),
         WebErrorStatus.NotFound => NotFound(loanProblem),
         WebErrorStatus.Conflict => Conflict(loanProblem),
         _ => BadRequest(loanProblem)
      };
   }

   /// <summary>
   ///    Renews a borrowed loan.
   /// </summary>
   /// <param name="id">The unique id of the loan to renew.</param>
   /// <param name="ct">Cancellation token for the request.</param>
   /// <returns>
   ///    The updated loan after renewal.
   /// </returns>
   /// <response code="200">The loan was renewed successfully.</response>
   /// <response code="400">The loan id is invalid.</response>
   /// <response code="404">No loan with the given id exists.</response>
   /// <response code="409">
   ///    The loan cannot be renewed because it is overdue or has reached
   ///    the maximum number of renewals.
   /// </response>
   [HttpPatch("loans/{id:guid}/renew", Name = nameof(RenewLoanAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> RenewLoanAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanUseCases.RenewAsync(loanId: id, ct: ct);
      if(result.IsFailure) {
         var renewProblem = DomainProblemDetailsFactory.FromDomainError(
            result.Error, HttpContext);

         return result.Error.Status switch {
            WebErrorStatus.BadRequest => BadRequest(renewProblem),
            WebErrorStatus.NotFound => NotFound(renewProblem),
            WebErrorStatus.Conflict => Conflict(renewProblem),
            _ => BadRequest(renewProblem)
         };
      }

      var renewedLoan = await loanReadModel.FindByIdAsync(result.Value, ct);

      if(renewedLoan.IsSuccess)
         return Ok(renewedLoan.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(
         renewedLoan.Error, HttpContext);

      return renewedLoan.Error.Status switch {
         WebErrorStatus.BadRequest => BadRequest(problem),
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

Der Controller enthält keine Fachlogik. Er übersetzt HTTP-Anfragen in Aufrufe
an ReadModels oder UseCases und übersetzt Result-Objekte zurück in HTTP-
Antworten.

Die lesenden Endpunkte verwenden ILoanReadModel:

- GetBorrowedLoansAsync -> FindAllBorrowedAsync
- GetLoanByIdAsync      -> FindByIdAsync

Die schreibenden Endpunkte verwenden ILoanUseCases:

- BorrowBookItemAsync   -> BorrowAsync
- ReturnLoanAtDeskAsync -> ReturnAtDeskAsync
- RenewLoanAsync        -> RenewAsync

Loans besitzen kein IsActive-Flag. Der Zustand einer Ausleihe wird über
LoanStatus modelliert. Eine aktuell offene Ausleihe hat den Status Borrowed.

Deshalb gibt es keine Route /loans/active. Die Collection-Route GET /loans
liefert im aktuellen Entwicklungsstand die aktuell ausgeliehenen Loans.

Reader und Book verwenden IsActive. BookItem und Loan verwenden Status.

Die XML-Dokumentationskommentare oberhalb der Actions werden von Swagger
verwendet, wenn XML-Kommentare im Projekt aktiviert und in Swagger eingebunden
sind.
*/

// using System.Runtime.InteropServices.JavaScript;
// using Asp.Versioning;
// using CampusLibraryApi._2_BuildingBlocks;
// using CampusLibraryApi._3_Core.Loans._1_Ports;
// using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
// using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
// using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
// using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
//
// namespace CampusLibraryApi._1_Web.Controllers;
//
// [ApiController]
// [ApiVersion("1.0")]
// [Route("camplib/v{version:apiVersion}")]
// public sealed class LoansController(
//    ILoanReadModel loanReadModel,
//    ILoanUseCases loanUseCases
// ) : ControllerBase {
//
//    // GET: camplib/v1/loans
//    // GET: camplib/v1/loans?status=Active
//    // GET: camplib/v1/loans?readerId=...
//    // GET: camplib/v1/loans?bookItemId=...
//    [HttpGet("loans", Name = nameof(GetAllLoansAsync))]
//    [ProducesResponseType(typeof(IReadOnlyList<LoanDto>), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetAllLoansAsync(
//       [FromQuery] LoanStatus? status,
//       [FromQuery] Guid? readerId,
//       [FromQuery] Guid? bookItemId,
//       CancellationToken ct
//    ) {
//       Result<IReadOnlyList<LoanDto>> result = await loanReadModel.SelectAllAsync(
//          status: status,
//          readerId: readerId,
//          bookItemId: bookItemId,
//          ct: ct
//       );
//
//       return ToActionResult(result);
//    }
//
//    // GET: camplib/v1/loans/{id}
//    [HttpGet("loans/{id:guid}", Name = nameof(GetLoanByIdAsync))]
//    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<ActionResult<LoanDto>> GetLoanByIdAsync(
//       Guid id,
//       CancellationToken ct
//    ) {
//       Result<LoanDto> result = await loanReadModel.FindByIdAsync(
//          id: id,
//          ct: ct
//       );
//
//       return ToActionResult(result);
//    }
//
//    // POST: camplib/v1/loans
//    [HttpPost("loans", Name = nameof(BorrowBookItemAsync))]
//    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status409Conflict)]
//    public async Task<ActionResult<LoanDto>> BorrowBookItemAsync(
//       [FromBody] LoanBorrowDto dto,
//       CancellationToken ct
//    ) {
//       Result<LoanDto> result = await loanUseCases.BorrowAsync(
//          dto: dto,
//          ct: ct
//       );
//
//       if(result.IsFailure)
//          return ToActionResult(result);
//
//       return CreatedAtRoute(
//          routeName: nameof(GetLoanByIdAsync),
//          routeValues: new {
//             version = HttpContext.GetRequestedApiVersion()?.ToString(),
//             id = result.Value.Id
//          },
//          value: result.Value
//       );
//    }
//
//    // PATCH: camplib/v1/loans/{id}/return
//    [HttpPatch("loans/{id:guid}/return", Name = nameof(ReturnLoanAsync))]
//    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status409Conflict)]
//    public async Task<ActionResult<LoanDto>> ReturnLoanAsync(
//       Guid id,
//       CancellationToken ct
//    ) {
//       Result<LoanDto> result = await loanUseCases.ReturnAsync(
//          id: id,
//          ct: ct
//       );
//
//       return ToActionResult(result);
//    }
//
//    // PATCH: camplib/v1/loans/{id}/renew
//    [HttpPatch("loans/{id:guid}/renew", Name = nameof(RenewLoanAsync))]
//    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status409Conflict)]
//    public async Task<ActionResult<LoanDto>> RenewLoanAsync(
//       Guid id,
//       CancellationToken ct
//    ) {
//       Result<LoanDto> result = await loanUseCases.RenewAsync(
//          id: id,
//          ct: ct
//       );
//
//       return ToActionResult(result);
//    }
//
//    private ActionResult<T> ToActionResult<T>(
//       Result<T> result
//    ) {
//       if(result.IsSuccess)
//          return Ok(result.Value);
//
//       return ToProblemResult<T>(
//          error: result.Error
//       );
//    }
//
//    private ActionResult<T> ToProblemResult<T>(
//       JSType.Error error
//    ) {
//       return error.Type switch {
//          ErrorType.NotFound => NotFound(error),
//          ErrorType.Conflict => Conflict(error),
//          ErrorType.Validation => BadRequest(error),
//          ErrorType.AccessDenied => Forbid(),
//          _ => BadRequest(error)
//       };
//    }
// }
//
// /*
// Lernziele / Didaktik
// --------------------
//
// Dieser Controller trennt die fachlichen Lebenszykluszustände einer Ausleihe
// klar von der Deaktivierung anderer Stammdaten.
//
// Readers, Books und BookItems verwenden bei Bedarf den Query-Parameter
// includeDeactivated, weil dort ein Soft-Delete-/Deaktivierungsmodell vorliegt.
//
// Loans verwenden dagegen keine Route wie /loans/active. Eine Ausleihe ist
// nicht aktiv oder deaktiviert im Sinne von Stammdaten, sondern befindet sich
// in einem fachlichen Zustand, zum Beispiel Active, Returned oder Overdue.
//
// Die Collection-Route GET /loans bleibt der Standardzugriff. Einschränkungen
// werden über Query-Parameter formuliert:
//
//    GET /loans
//    GET /loans?status=Active
//    GET /loans?readerId=...
//    GET /loans?bookItemId=...
//
// Dadurch bleiben die Routen konsistent, fachlich verständlich und später gut
// für Blazor-, Android- und Multiplatform-Clients nutzbar.
// */