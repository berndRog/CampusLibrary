using Asp.Versioning;
using CampusLibraryApi._1_Web.Common;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

/// <summary>
/// HTTP API controller for current loan resources.
/// A stored Loan represents a currently borrowed BookItem; returning it deletes the Loan.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("camplib/v{version:apiVersion}")]
public sealed class LoansController(
   ILoanReadModel loanReadModel,
   ILoanUseCases loanUseCases
) : ControllerBase {

   [HttpGet("loans", Name = nameof(GetBorrowedLoansAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IReadOnlyList<LoanDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetBorrowedLoansAsync(
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindAllBorrowedAsync(ct);
      if(result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
      return BadRequest(problem);
   }

   [HttpGet("loans/{id:guid}", Name = nameof(GetLoanByIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<LoanDto>> GetLoanByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanReadModel.FindByIdAsync(id, ct);
      if(result.IsSuccess)
         return Ok(result.Value);

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
      return result.Error.Status switch {
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

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
      var result = await loanUseCases.BorrowAsync(dto, ct);
      if(result.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
         return result.Error.Status switch {
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var loanResult = await loanReadModel.FindByIdAsync(result.Value, ct);
      if(loanResult.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(loanResult.Error, HttpContext);
         return loanResult.Error.Status switch {
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

   [HttpPatch("loans/{id:guid}/return-at-desk", Name = nameof(ReturnLoanAtDeskAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<IActionResult> ReturnLoanAtDeskAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await loanUseCases.ReturnAtDeskAsync(id, ct);
      if(result.IsSuccess)
         return NoContent();

      var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
      return result.Error.Status switch {
         WebErrorStatus.NotFound => NotFound(problem),
         _ => BadRequest(problem)
      };
   }

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
      var result = await loanUseCases.RenewAsync(id, ct);
      if(result.IsFailure) {
         var problem = DomainProblemDetailsFactory.FromDomainError(result.Error, HttpContext);
         return result.Error.Status switch {
            WebErrorStatus.NotFound => NotFound(problem),
            WebErrorStatus.Conflict => Conflict(problem),
            _ => BadRequest(problem)
         };
      }

      var renewedLoan = await loanReadModel.FindByIdAsync(result.Value, ct);
      if(renewedLoan.IsSuccess)
         return Ok(renewedLoan.Value);

      var readProblem = DomainProblemDetailsFactory.FromDomainError(renewedLoan.Error, HttpContext);
      return renewedLoan.Error.Status switch {
         WebErrorStatus.NotFound => NotFound(readProblem),
         WebErrorStatus.Conflict => Conflict(readProblem),
         _ => BadRequest(readProblem)
      };
   }
}
