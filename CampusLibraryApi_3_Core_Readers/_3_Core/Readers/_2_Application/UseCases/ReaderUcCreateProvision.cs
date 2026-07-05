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

// Use case for provisioning the fachlicher Reader of the current technical user.
internal sealed class ReaderUcCreateProvision(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<ReaderUcCreateProvision> logger
) {

   public async Task<Result<ReaderProvisionDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      
      // 1) check IdentityGateway parameter and get subject    
      var resultIdentity = IdentitySubject.Check(identityGateway);
      if (resultIdentity.IsFailure)
         return Result<ReaderProvisionDto>.Failure(resultIdentity.Error);
      var subject = resultIdentity.Value;
      var username = identityGateway.Username.Trim();
      var createdAt = identityGateway.CreatedAt;
      
      // 2) idempotent lookup
      var existingReader = await repository.FindBySubjectAsync(subject, ct);
      if (existingReader is not null) 
         return Result<ReaderProvisionDto>.Success(
            existingReader.ToReaderProvisionDto(wasCreated: false));
      
      // 3) interpret preferred_username as initial email
      var resultEmail = EmailVo.Create(username);
      if (resultEmail.IsFailure)
         return Result<ReaderProvisionDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;

      // check uniqueness
      var existingReaderByEmail = await repository.FindByEmailAsync(emailVo, ct);
      if (existingReaderByEmail is not null)
         return Result<ReaderProvisionDto>.Failure(ReaderErrors.EmailAlreadyInUse);

      // 4) create reader Id
      var resultId = EntityId.Resolve(id, ReaderErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<ReaderProvisionDto>.Failure(resultId.Error);
      
      // 5) Create aggregate
      var resultReader = Reader.Provision(
         id: resultId.Value,
         subject: subject,
         emailVo: emailVo,
         createdAt: createdAt
      );
      if (resultReader.IsFailure)
         return Result<ReaderProvisionDto>.Failure(resultReader.Error);

      var reader = resultReader.Value;
      repository.Add(reader);

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcCreateProvision", ct);

      logger.LogInformation(
         "ReaderUcCreateProvision: readerId={ReaderId} subject={Subject} rows={Rows}",
         reader.Id, reader.Subject, rows);

      return Result<ReaderProvisionDto>.Success(
         reader.ToReaderProvisionDto(wasCreated: true));
   }
}

/*
Didaktik
--------

ReaderUcProvisionMe ist der zentrale Part-6-UseCase.

Er macht aus einem technischen Benutzerkonto im IdentityAccessServer einen
fachlichen Reader in der CampusLibrary. Subject und Email werden nicht vom
Client-Formular übernommen, sondern aus dem IdentityGateway gelesen.

Die Operation ist idempotent:

- Existiert zum Subject bereits ein Reader, wird er zurückgegeben.
- Existiert noch keiner, wird ein unvollständiger Reader provisioniert.

Dadurch kann der Client den Endpunkt nach jedem Login aufrufen, ohne aus
Versehen mehrere Reader anzulegen.

Lernziele
---------

- Provisioning als Übergang von technischer zu fachlicher Identität verstehen
- Idempotenz bei Setup-/Initialisierungsoperationen einsetzen
- Subject als stabilen Schlüssel verwenden
- Email-Konflikte vor dem Speichern prüfen
*/
