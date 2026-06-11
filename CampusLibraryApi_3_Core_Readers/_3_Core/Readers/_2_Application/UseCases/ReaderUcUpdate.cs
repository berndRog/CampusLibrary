using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for updating mutable Reader profile data.
// Coordinates aggregate loading, value object creation, uniqueness checks and persistence.
// This is a command-side application service and therefore returns a Result.
public sealed class ReaderUcUpdate(
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcUpdate> logger
) {
   // Execute the update-reader workflow.
   public async Task<Result<ReaderDto>> ExecuteAsync(
      Guid id,
      ReaderUpdateDto dto,
      CancellationToken ct
   ) {
      if (id == Guid.Empty)
         return Result<ReaderDto>.Failure(ReaderErrors.InvalidId);

      if (dto == default)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderUpdateDtoRequired);

      var reader = await repository.FindByIdAsync(id, ct);
      if (reader is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      // 2) DomainModel
      // Normalize and validate email input
      EmailVo? newEmailVo;
      if (dto.Email is null) {
         newEmailVo = null;
      }
      else {
         var resultEmail = EmailVo.Create(dto.Email);
         if (resultEmail.IsFailure) return Result<ReaderDto>.Failure(resultEmail.Error);
         newEmailVo = resultEmail.Value;
         
         // Check email uniqueness
         var readerWithSameEmail = await repository.FindByEmailAsync(newEmailVo, ct);
         if (readerWithSameEmail is not null && readerWithSameEmail.Id != reader.Id)
            return Result<ReaderDto>.Failure(ReaderErrors.EmailAlreadyInUse);
      }

      // check Address
      AddressVo? newAddressVo;
      if (dto.AddressDto is null) {
         newAddressVo = null;
      }
      else {
         var resultAddressVo = AddressVo.Create(
            street: dto.AddressDto.Street,
            postalCode: dto.AddressDto!.PostalCode,
            city: dto.AddressDto!.City,
            country: dto.AddressDto!.Country
         );
         if (resultAddressVo.IsFailure)
            return Result<ReaderDto>.Failure(resultAddressVo.Error);
         newAddressVo = resultAddressVo.Value;
      }
      
      // Update the aggregate through its domain method.
      var updateResult = reader.UpdateProfile(
         lastname: dto.Lastname,
         emailVo: newEmailVo,
         addressVo: newAddressVo,
         updatedAt: clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<ReaderDto>.Failure(updateResult.Error);

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcUpdate", ct);

      logger.LogInformation("ReaderUcUpdate: readerId={ReaderId} rows={Rows}",
         reader.Id, rows);

      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}

/*
Didaktik
--------

ReaderUcUpdate ist ein schreibender Use Case.

Der Use Case lädt zuerst das bestehende Reader-Aggregate über das Repository.
Danach werden Eingabewerte in Value Objects übersetzt und fachliche
Konflikte geprüft, z. B. die Eindeutigkeit der E-Mail-Adresse.

Die eigentliche Änderung erfolgt nicht über Setter, sondern über die
Domain-Methode Reader.UpdateProfile(...). Dadurch bleiben fachliche Regeln
im Aggregate sichtbar.

Ablauf:

1. Id und DTO prüfen
2. Reader-Aggregate laden
3. EmailVo erzeugen und E-Mail-Konflikt prüfen
4. AddressVo erzeugen
5. Reader.UpdateProfile(...) ausführen
6. UnitOfWork zum Speichern verwenden
7. Ergebnis als DTO zurückgeben

Lernziele
---------

- Use Case als Koordinator eines Änderungsablaufs verstehen
- bestehende Aggregates über Repositories laden
- Value Objects vor der Domain-Änderung erzeugen
- fachliche Konflikte vor dem Speichern prüfen
- Änderungen über Methoden am Aggregate durchführen
*/
