using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for creating a new Reader aggregate.
// Coordinates validation, value object creation, uniqueness checks and persistence.
// This is a command-side application service and therefore returns a Result.
public sealed class ReaderUcCreate(
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcCreate> logger
) {
   // Execute the create-reader workflow.
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   ) {
      if (dto == default)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderCreateDtoRequired);

      var subject = dto.Subject.Trim();
      if (await repository.ExistsBySubjectAsync(subject, ct))
         return Result<ReaderDto>.Failure(ReaderErrors.SubjectAlreadyExists);

      // Normalize and validate email input using the value object.
      var resultEmail = EmailVo.Create(dto.Email);
      if (resultEmail.IsFailure)
         return Result<ReaderDto>.Failure(resultEmail.Error);

      var emailVo = resultEmail.Value;

      // Check email uniqueness on the write side.
      if (await repository.FindByEmailAsync(emailVo, ct) is not null)
         return Result<ReaderDto>.Failure(ReaderErrors.EmailAlreadyInUse);

      // Validate address input and create AddressVo.
      var addressDto = dto.AddressDto;
      var resultAddress = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );
      if (resultAddress.IsFailure)
         return Result<ReaderDto>.Failure(resultAddress.Error);

      var addressVo = resultAddress.Value;

      // Resolve optional external id or generate a new id.
      var resultId = EntityId.Resolve(dto.Id, ReaderErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<ReaderDto>.Failure(resultId.Error);

      var id = resultId.Value;

      // Create Reader aggregate through the domain factory.
      var resultReader = Reader.Create(
         id: id,
         subject: subject,
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         emailVo: emailVo,
         addressVo: addressVo,
         createdAt: clock.UtcNow
      );
      if (resultReader.IsFailure)
         return Result<ReaderDto>.Failure(resultReader.Error);

      var reader = resultReader.Value;

      // Add reader to repository. SaveChanges is handled by the UnitOfWork.
      repository.Add(reader);

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcCreate", ct);

      logger.LogInformation("ReaderUcCreate: readerId={ReaderId} rows={Rows}",
         reader.Id, rows);

      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}

/*
Didaktik
--------

ReaderUcCreate ist ein schreibender Use Case.

Der Use Case koordiniert den fachlichen Ablauf, enthält aber möglichst
keine Detailregeln selbst. Detailvalidierungen liegen in Value Objects
oder im Aggregate. Der Use Case entscheidet die Reihenfolge der Schritte
und nutzt Ports, um Infrastruktur zu erreichen.

Ablauf:

1. DTO prüfen
2. Subject auf Eindeutigkeit prüfen
3. EmailVo erzeugen und E-Mail-Eindeutigkeit prüfen
4. AddressVo erzeugen
5. technische Id auflösen oder erzeugen
6. Reader-Aggregate über Factory erzeugen
7. Repository + UnitOfWork zum Speichern verwenden
8. Ergebnis als DTO zurückgeben

Lernziele
---------

- Use Case als Koordinator eines schreibenden Ablaufs verstehen
- Value Objects und Aggregate für fachliche Regeln nutzen
- Repository und UnitOfWork als Ports verwenden
- IClock für testbare Zeitpunkte einsetzen
- Result als expliziten Erfolgs-/Fehlerpfad verwenden
*/
