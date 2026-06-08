using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using Microsoft.AspNetCore.Mvc;
namespace CampusLibraryApi._1_Web.Controllers;

[ApiController]
[Route("library/v1")]
public sealed class ReadersController(
   IReaderReadModel readerReadModel,
   ReaderUcCreate readerUcCreate
) : ControllerBase {

   [HttpGet("readers", Name = nameof(GetAllAsync))]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> GetAllAsync(CancellationToken ct) {
      var result = await readerReadModel.SelectAllAsync(ct);
      
      return result.IsSuccess 
         ? Ok(result.Value) 
         : Problem(result.Error?.Message);
   }

   [HttpGet("readers/{id:guid}", Name = nameof(GetByIdAsync))]
   public async Task<ActionResult<ReaderDto>> GetByIdAsync(
      [FromRoute] Guid id, 
      CancellationToken ct
   ) {
      
      var result = await readerReadModel.FindByIdAsync(id, ct);
      
      return result.IsSuccess 
         ? Ok(result.Value) 
         : NotFound(result.Error);
   }

   [HttpGet("readers/email", Name=nameof(GetByEmailAsync))]
   public async Task<ActionResult<ReaderDto>> GetByEmailAsync(
      [FromQuery] string email, 
      CancellationToken ct
   ) {
      
      var result = 
         await readerReadModel.FindByEmailAsync(email, ct);
      
      return result.IsSuccess 
         ? Ok(result.Value) 
         : NotFound(result.Error);
   }
   
   [HttpPost("readers", Name = nameof(CreateAsync))]
   public async Task<ActionResult<ReaderDto>> CreateAsync(
      [FromBody] ReaderCreateDto dto, 
      CancellationToken ct
   ) {
      var result = await readerUcCreate.ExecuteAsync(dto, ct);
      
      if (!result.IsSuccess || result.Value is null)
         return BadRequest(result.Error);
     
      return CreatedAtRoute(
         nameof(GetByIdAsync),
         new { id = result.Value.Id },
         result.Value
      );
   }
}
