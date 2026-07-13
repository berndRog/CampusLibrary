using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for completing the initial fachliche profile
// of the currently authenticated reader.
//
// This use case maps to:
// PUT /readers/me/profile
//
// It is executed after the Reader has already been provisioned through:
// POST /readers/me/provision
//
// The current Reader is resolved through the access token subject.
// The client does not send a Reader id.
//
// This use case completes the initial profile data:
// - Firstname
// - Lastname
// - Address
//
// Email is intentionally not part of ReaderProfileMeDto.
// The initial fachliche email was already taken from the technical username
// during provisioning.
internal sealed class ReaderUcUpdateMeProfile(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcUpdateMeProfile> logger
) {

   // Executes the initial profile completion workflow for the current reader.
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderProfileMeDto? meDto,
      CancellationToken ct
   ) {
      if (meDto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderUpdateMeProfileDtoRequired);

      // 1) Validate the current identity and read the technical subject.
      // The role check is handled by the controller policy.
      var resultIdentity = IdentitySubject.Check(identityGateway);
      if (resultIdentity.IsFailure)
         return Result<ReaderDto>.Failure(resultIdentity.Error);

      var subject = resultIdentity.Value;

      // 2) Load the current fachliche Reader by subject.
      // The Reader must already have been provisioned.
      var reader = await repository.FindBySubjectAsync(subject, ct);
      if (reader is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      // 3) Address is required for completing the profile.
      if (meDto.AddressDto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.AddressIsRequired);

      var addressVo = meDto.AddressDto.ToAddressVo();

      // 4) Complete the initial profile through the aggregate method.
      // The aggregate decides which values are valid and how the profile
      // completion state changes.
      var resultUpdate = reader.UpdateMyProfile(
         firstname: meDto.Firstname,
         lastname: meDto.Lastname,
         addressVo: addressVo,
         updatedAt: clock.UtcNow
      );

      if (resultUpdate.IsFailure)
         return Result<ReaderDto>.Failure(resultUpdate.Error);

      // 5) Persist the profile completion as one application transaction.
      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcUpdateMeProfile", ct);

      logger.LogInformation(
         "ReaderUcUpdateMeProfile: readerId={ReaderId} subject={Subject} rows={Rows}",
         reader.Id, subject, rows);

      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}

/*
Didaktik
--------

ReaderUcUpdateMeProfile ist der schreibende UseCase für den initialen
Profilabschluss des aktuell angemeldeten Readers.

Der UseCase gehört zu folgendem Endpunkt:

   PUT /readers/me/profile

Er wird nach der Provisionierung ausgeführt:

   POST /readers/me/provision

Die Provisionierung erzeugt zunächst nur den fachlichen Reader-Rumpf. Der
Profilabschluss ergänzt danach die fachlichen Profildaten, die der Reader im
Formular selbst erfassen darf:

- Vorname
- Nachname
- Adresse

Der Client übergibt keine ReaderId. Der aktuelle fachliche Reader wird
serverseitig über das Subject aus dem Access Token ermittelt. Dadurch kann ein
Reader nicht versuchen, durch eine fremde Id einen anderen Reader zu ändern.

Subject und Email sind bewusst nicht Teil von ReaderProfileMeDto. Das Subject
kommt aus dem Access Token. Die initiale fachliche Email wurde bereits beim
Provisioning aus dem technischen Username übernommen.

Eine spätere Änderung von Nachname, fachlicher Reader-Email und Adresse gehört
nicht zu diesem UseCase, sondern zu:

   PUT /readers/me/update

Dafür wird ReaderUpdateMeDto verwendet. Der Vorname bleibt bei dieser späteren
Änderung bewusst unveränderbar.

Der Controller entscheidet über die grobe Autorisierung, z. B. über eine
Reader-Policy. Der UseCase wiederholt diese Rollenprüfung nicht. Er verwendet
aber das IdentityGateway, um die technische Identität des aktuellen Benutzers
fachlich auszuwerten.

Die eigentliche Änderung erfolgt nicht über Setter, sondern über die
Domain-Methode Reader.UpdateMyProfile(...). Dadurch bleiben fachliche Regeln im
Aggregate sichtbar.

Ablauf:

1. DTO prüfen
2. Subject aus dem IdentityGateway lesen
3. aktuellen Reader über Subject laden
4. Adresse als Pflichtangabe prüfen
5. Reader.UpdateMyProfile(...) ausführen
6. UnitOfWork zum Speichern verwenden
7. Ergebnis als DTO zurückgeben

Lernziele
---------

- Provisionierung und initialen Profilabschluss trennen
- technische Identität und fachliches Profil unterscheiden
- Self-Service-Endpunkte über /me statt über Route-IDs modellieren
- aktuellen Reader über das Token-Subject bestimmen
- Subject und Email nicht aus UI-Formularen übernehmen
- initiale fachliche Email aus dem Provisioning verstehen
- späteres Self-Service-Update vom initialen Profilabschluss trennen
- Controller-Autorisierung und fachliche UseCase-Regeln unterscheiden
- Änderungen über Methoden am Aggregate durchführen
*/