using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for the self-service update of the currently authenticated reader.
//
// This use case maps to:
// PUT /readers/me/update
//
// The current reader is resolved through the access token subject.
// The client does not send a Reader id.
//
// This use case updates only mutable fachliche Reader data:
// - Lastname
// - fachliche Email
// - Address
//
// Firstname is intentionally not changed here.
// The technical username in the IdentityAccessServer is also not changed.
internal sealed class ReaderUcUpdateMe(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcUpdateMe> logger
) {

   // Executes the self-service update workflow for the current reader.
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderUpdateDto? meDto,
      CancellationToken ct
   ) {
      if (meDto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderUpdateDtoRequired);

      // 1) Validate the current identity and read the technical subject.
      // The role check is handled by the controller policy.
      var resultIdentity = IdentitySubject.Check(identityGateway);
      if (resultIdentity.IsFailure)
         return Result<ReaderDto>.Failure(resultIdentity.Error);

      var subject = resultIdentity.Value;

      // 2) Load the current fachliche Reader by subject.
      // The client must not decide which Reader is updated.
      var reader = await repository.FindBySubjectAsync(subject, ct);
      if (reader is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      // 3) Validate the optional fachliche email if one was provided.
      // This email is the Reader contact email in CampusLibraryApi,
      // not necessarily the technical login name in the IdentityAccessServer.
      EmailVo? newEmailVo = null;

      if (meDto.Email is not null) {
         var resultEmail = EmailVo.Create(meDto.Email);
         if (resultEmail.IsFailure)
            return Result<ReaderDto>.Failure(resultEmail.Error);
         newEmailVo = resultEmail.Value;

         // Check email uniqueness across other Readers.
         var readerWithSameEmail = await repository.FindByEmailAsync(newEmailVo, ct);
         if (readerWithSameEmail is not null &&
             readerWithSameEmail.Id != reader.Id)
            return Result<ReaderDto>.Failure(ReaderErrors.EmailAlreadyInUse);
      }

      // 4) Validate the optional address if one was provided.
      AddressVo? newAddressVo = null;

      if (meDto.AddressDto is not null) {
         var resultAddressVo = AddressVo.Create(
            street: meDto.AddressDto.Street,
            postalCode: meDto.AddressDto.PostalCode,
            city: meDto.AddressDto.City,
            country: meDto.AddressDto.Country
         );

         if (resultAddressVo.IsFailure)
            return Result<ReaderDto>.Failure(resultAddressVo.Error);

         newAddressVo = resultAddressVo.Value;
      }

      // 5) Update the aggregate through its domain method.
      // The aggregate decides which values may actually change.
      var updateResult = reader.UpdateProfile(
         lastname: meDto.Lastname,
         emailVo: newEmailVo,
         addressVo: newAddressVo,
         updatedAt: clock.UtcNow
      );

      if (updateResult.IsFailure)
         return Result<ReaderDto>.Failure(updateResult.Error);

      // 6) Persist the change as one application transaction.
      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcUpdateMe", ct);

      logger.LogInformation(
         "ReaderUcUpdateMe: readerId={ReaderId} subject={Subject} rows={Rows}",
         reader.Id,
         subject,
         rows
      );

      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}

/*
Didaktik
--------

ReaderUcUpdateMe ist der schreibende UseCase für die spätere Self-Service-
Änderung des aktuell angemeldeten Readers.

Der UseCase gehört zu folgendem Endpunkt:

   PUT /readers/me/update

Im Unterschied zur früheren administrativen Änderung über

   PUT /readers/{id}

übergibt der Client hier keine ReaderId. Der aktuelle fachliche Reader wird
serverseitig über das Subject aus dem Access Token ermittelt. Dadurch kann ein
Reader nicht versuchen, durch eine fremde Id einen anderen Reader zu ändern.

Der Controller entscheidet über die grobe Autorisierung, z. B. über

   [Authorize(Policy = CampusLibraryPolicies.Reader)]

Der UseCase wiederholt diese Rollenprüfung nicht. Er verwendet aber das
IdentityGateway, um die technische Identität zu lesen und daraus den aktuellen
fachlichen Reader zu bestimmen.

ReaderUcUpdateMe ändert nur fachlich veränderbare Self-Service-Daten:

- Nachname
- fachliche Reader-Email
- Adresse

Der Vorname ist bewusst nicht Teil von ReaderUpdateDto und wird hier nicht
geändert. Auch der technische Username im IdentityAccessServer bleibt
unverändert. Die Email in ReaderUpdateDto ist die fachliche Kontakt-Email des
Readers in der CampusLibraryApi.

Vor der Änderung werden neue Eingabewerte in Value Objects übersetzt. Dadurch
laufen Format- und Fachvalidierungen vor der eigentlichen Domain-Änderung. Die
Eindeutigkeit der Email wird ebenfalls im UseCase geprüft, weil dafür ein
Repository-Zugriff nötig ist.

Die eigentliche Änderung erfolgt nicht über Setter, sondern über die
Domain-Methode Reader.UpdateProfile(...). Dadurch bleiben fachliche Regeln im
Aggregate sichtbar.

Ablauf:

1. DTO prüfen
2. Subject aus dem IdentityGateway lesen
3. aktuellen Reader über Subject laden
4. optionale Email validieren und Eindeutigkeit prüfen
5. optionale Adresse validieren
6. Reader.UpdateProfile(...) ausführen
7. UnitOfWork zum Speichern verwenden
8. Ergebnis als DTO zurückgeben

Lernziele
---------

- Self-Service-Update über /me statt über eine Route-Id modellieren
- aktuellen Reader über das Token-Subject bestimmen
- Controller-Autorisierung und fachliche UseCase-Regeln trennen
- technische Login-Daten und fachliche Reader-Daten unterscheiden
- fachliche Reader-Email vom technischen Username unterscheiden
- optionale Änderungsdaten gezielt validieren
- Value Objects vor der Domain-Änderung erzeugen
- fachliche Konflikte vor dem Speichern prüfen
- Änderungen über Methoden am Aggregate durchführen
*/