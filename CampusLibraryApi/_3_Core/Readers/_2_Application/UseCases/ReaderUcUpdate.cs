using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

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

      // Normalize and validate email input using the value object.
      var resultEmail = EmailVo.Create(dto.Email);
      if (resultEmail.IsFailure)
         return Result<ReaderDto>.Failure(resultEmail.Error);

      var emailVo = resultEmail.Value;

      // Check email uniqueness, but allow the current reader to keep its own email.
      var readerWithSameEmail = await repository.FindByEmailAsync(emailVo, ct);
      if (readerWithSameEmail is not null && readerWithSameEmail.Id != reader.Id)
         return Result<ReaderDto>.Failure(ReaderErrors.EmailAlreadyInUse);

      var addressDto = dto.AddressDto;
      if (addressDto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.AddressRequired);

      // Validate address input and create AddressVo.
      var resultAddress = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );
      if (resultAddress.IsFailure)
         return Result<ReaderDto>.Failure(resultAddress.Error);

      var addressVo = resultAddress.Value;

      // Update the aggregate through its domain method.
      var updateResult = reader.UpdateProfile(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         emailVo: emailVo,
         addressVo: addressVo,
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
