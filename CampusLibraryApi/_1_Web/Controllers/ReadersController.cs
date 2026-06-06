using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using Microsoft.AspNetCore.Mvc;
namespace CampusLibraryApi._1_Web.Controllers;

[ApiController]
[Route("library/v1/readers")]
public sealed class ReadersController(
   IReaderReadModel readerReadModel,
   ReaderUcCreate readerUcCreate
) : ControllerBase {

   [HttpGet]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct) {
      var result = await readerReadModel.SelectAllAsync(ct);
      return result.IsSuccess ? Ok(result.Value) : Problem(result.Error?.Message);
   }

   [HttpGet("{id:guid}")]
   public async Task<ActionResult<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct) {
      var result = await readerReadModel.FindByIdAsync(id, ct);
      return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
   }

   [HttpPost]
   public async Task<ActionResult<ReaderDto>> CreateAsync(ReaderCreateDto dto, CancellationToken ct) {
      var result = await readerUcCreate.ExecuteAsync(dto, ct);

      if (!result.IsSuccess || result.Value is null)
         return BadRequest(result.Error);

      return CreatedAtAction(
         nameof(FindByIdAsync),
         new { id = result.Value.Id },
         result.Value
      );
   }
}
