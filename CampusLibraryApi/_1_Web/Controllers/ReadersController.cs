using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Controllers;

// HTTP API controller for Reader resources.
// Translates HTTP requests into calls to read models or use cases.
// Contains no domain logic.
[ApiController]
[Route("library/v1")]
public sealed class ReadersController(
   IReaderReadModel readerReadModel,
   ReaderUcCreate readerUcCreate
) : ControllerBase {
   // Query all readers through the read model.
   [HttpGet("readers", Name = nameof(GetAllAsync))]
   public async Task<ActionResult<IReadOnlyList<ReaderDto>>> GetAllAsync(CancellationToken ct) {
      var result = await readerReadModel.SelectAllAsync(ct);

      return result.IsSuccess
         ? Ok(result.Value)
         : Problem(result.Error?.Message);
   }

   // Query one reader by id through the read model.
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

   // Query one reader by email through the read model.
   [HttpGet("readers/email", Name = nameof(GetByEmailAsync))]
   public async Task<ActionResult<ReaderDto>> GetByEmailAsync(
      [FromQuery] string email,
      CancellationToken ct
   ) {
      var result = await readerReadModel.FindByEmailAsync(email, ct);

      return result.IsSuccess
         ? Ok(result.Value)
         : NotFound(result.Error);
   }

   // Create a new reader through the write-side use case.
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

Der POST-Endpunkt verwendet den schreibenden Use Case:

- CreateAsync      -> ReaderUcCreate.ExecuteAsync

CreatedAtRoute erzeugt bei erfolgreicher Erstellung eine 201-Created-
Antwort mit Location-Header auf die neu erzeugte Ressource.

Lernziele
---------

- Controller als HTTP-Adapter verstehen
- Unterschied zwischen GET/ReadModel und POST/UseCase erkennen
- REST-Verhalten von 201 Created und Location-Header nachvollziehen
- Keine Domainlogik im Controller platzieren
- Result in HTTP-Antworten übersetzen
*/
