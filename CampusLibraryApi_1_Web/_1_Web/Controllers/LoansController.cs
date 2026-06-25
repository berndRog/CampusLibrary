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