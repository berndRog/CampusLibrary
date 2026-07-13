using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for self-service provisioning of the currently authenticated reader.
//
// This use case maps to:
// POST /readers/me/provision
//
// It creates the fachlicher Reader shell for the current technical user.
// The current user is resolved through the access token subject.
//
// The client may optionally provide a deterministic Reader id for tests.
// The client does not provide subject, username, email or profile data.
internal sealed class ReaderUcCreateMeProvision(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<ReaderUcCreateMeProvision> logger
) {

   // Executes the self-service provisioning workflow for the current reader.
   //
   // The operation is idempotent:
   // if a Reader for the current subject already exists, the existing Reader
   // is returned instead of creating a second Reader.
   public async Task<Result<ReaderProvisionMeDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) Validate the current identity and read the technical subject.
      // The role check is handled by the controller policy.
      var resultIdentity = IdentitySubject.Check(identityGateway);
      if (resultIdentity.IsFailure)
         return Result<ReaderProvisionMeDto>.Failure(resultIdentity.Error);

      var subject = resultIdentity.Value;
      var username = identityGateway.Username?.Trim() ?? string.Empty;
      var createdAt = identityGateway.CreatedAt;

      // 2) Check whether the current technical user already has a Reader.
      // This makes provisioning safe to call after every login.
      var existingReader = await repository.FindBySubjectAsync(subject, ct);

      if (existingReader is not null) {
         logger.LogInformation(
            "ReaderUcCreateMeProvision: existing reader returned readerId={ReaderId} subject={Subject}",
            existingReader.Id, existingReader.Subject);

         return Result<ReaderProvisionMeDto>.Success(
            existingReader.ToReaderProvisionMeDto(wasCreated: false)
         );
      }

      // 3) Use the technical username as the initial fachliche reader email.
      // The email can later be changed through PUT /readers/me/update.
      // The technical username in the IdentityAccessServer is not changed.
      var resultEmail = EmailVo.Create(username);
      if (resultEmail.IsFailure)
         return Result<ReaderProvisionMeDto>.Failure(resultEmail.Error);

      var emailVo = resultEmail.Value;

      // 4) Check fachliche email uniqueness before creating the aggregate.
      var existingReaderByEmail = await repository.FindByEmailAsync(emailVo, ct);
      if (existingReaderByEmail is not null)
         return Result<ReaderProvisionMeDto>.Failure(ReaderErrors.EmailAlreadyInUse);

      // 5) Resolve the optional Reader id.
      // In normal runtime this will usually create a new id.
      // In manual HTTP tests a deterministic id can be supplied.
      var resultId = EntityId.Resolve(id, ReaderErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<ReaderProvisionMeDto>.Failure(resultId.Error);

      // 6) Create the fachlicher Reader shell.
      // At this point the Reader is provisioned but the fachliche profile
      // is not completed yet.
      var resultReader = Reader.Provision(
         id: resultId.Value,
         subject: subject,
         emailVo: emailVo,
         createdAt: createdAt
      );
      if (resultReader.IsFailure)
         return Result<ReaderProvisionMeDto>.Failure(resultReader.Error);

      var reader = resultReader.Value;

      repository.Add(reader);

      // 7) Persist the new Reader as one application transaction.
      var rows = 
         await unitOfWork.SaveAllChangesAsync("ReaderUcCreateMeProvision", ct);

      logger.LogInformation(
         "ReaderUcCreateMeProvision: readerId={ReaderId} subject={Subject} rows={Rows}",
         reader.Id, reader.Subject, rows);

      return Result<ReaderProvisionMeDto>.Success(
         reader.ToReaderProvisionMeDto(wasCreated: true)
      );
   }
}

/*
Didaktik
--------

ReaderUcCreateMeProvision ist der schreibende UseCase für die Self-Service-
Provisionierung des aktuell angemeldeten Readers.

Der UseCase gehört zu folgendem Endpunkt:

   POST /readers/me/provision

Provisionierung bedeutet hier: Aus einem technischen Benutzerkonto im
IdentityAccessServer wird ein fachlicher Reader in der CampusLibraryApi
angelegt.

Der Client übergibt für diesen UseCase keine technischen Identitätsdaten.
Insbesondere werden Subject und Username nicht aus einem Formular übernommen.
Das Subject kommt aus dem Access Token und wird über das IdentityGateway
gelesen. Der technische Username wird beim Provisioning als initiale fachliche
Reader-Email verwendet.

Die Operation ist idempotent:

- Existiert zum Subject bereits ein Reader, wird dieser zurückgegeben.
- Existiert noch kein Reader, wird ein neuer unvollständiger Reader angelegt.

Dadurch kann der Client den Provisioning-Endpunkt nach jedem Login aufrufen,
ohne versehentlich mehrere Reader für denselben technischen Benutzer zu
erzeugen.

Die fachlichen Profildaten werden hier noch nicht vervollständigt. Das geschieht
im nächsten Self-Service-Schritt über:

   PUT /readers/me/profile

Eine spätere Änderung von Nachname, fachlicher Reader-Email und Adresse erfolgt
über:

   PUT /readers/me/update

Die optionale Id ist nur für deterministische Tests und manuelle HTTP-Szenarien
gedacht. Im normalen Betrieb kann die Anwendung selbst eine neue fachliche
ReaderId erzeugen.

Der Controller entscheidet über die grobe Autorisierung, z. B. über eine
Reader-Policy. Der UseCase wiederholt diese Rollenprüfung nicht. Er verwendet
aber das IdentityGateway, um die technische Identität des aktuellen Benutzers
fachlich auszuwerten.

Lernziele
---------

- Provisionierung als Übergang von technischer Identität zu Fachobjekt verstehen
- technischen Benutzer und fachlichen Reader unterscheiden
- Subject aus dem Access Token als stabilen technischen Schlüssel verwenden
- technische Token-Daten nicht aus UI-Formularen übernehmen
- initiale fachliche Reader-Email aus dem technischen Username ableiten
- idempotente Self-Service-Operationen modellieren
- deterministische Ids für Tests von produktivem Verhalten unterscheiden
- Provisionierung und Profilabschluss fachlich trennen
- Controller-Autorisierung und fachliche UseCase-Regeln unterscheiden
*/