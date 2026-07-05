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

// Use case for completing or updating firstname/lastname of the current reader.
internal sealed class ReaderUcUpdateProfile(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcUpdateProfile> logger
) {
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderProfileUpdateDto dto,
      CancellationToken ct
   ) {
      if (dto == default)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderProfileUpdateDtoRequired);

      // 1) check IdentityGateway parameter and get subject    
      var resultIdentity = IdentitySubject.Check(identityGateway);
      if (resultIdentity.IsFailure)
         return Result<ReaderDto>.Failure(resultIdentity.Error);
      var subject = resultIdentity.Value;
      var username = identityGateway.Username.Trim();
      var createdAt = identityGateway.CreatedAt;

      // 2) reader must be provisioned
      var reader = await repository.FindBySubjectAsync(subject, ct);
      if (reader is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      // 3) address must be given
      if (dto.AddressDto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.AddressIsRequired);

      var resultUpdate = reader.UpdateMyProfile(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         addressVo: dto.AddressDto.ToAddressVo(),
         updatedAt: clock.UtcNow
      );
      if (resultUpdate.IsFailure)
         return Result<ReaderDto>.Failure(resultUpdate.Error);

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcUpdateProfile", ct);
      logger.LogInformation(
         "ReaderUcUpdateProfile: readerId={ReaderId} rows={Rows}", reader.Id, rows);

      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}

/*
Didaktik
--------

ReaderUcUpdateMyProfile ergänzt die fachlichen Profildaten des angemeldeten
Readers.

Der UseCase nimmt keine Email und kein Subject entgegen. Beide Werte gehören
zur technischen Identität und kommen aus dem IdentityAccessServer. Das Formular
darf nur Vorname und Nachname liefern.

Lernziele
---------

- Self-Service-Profil von technischer Identität trennen
- aktuelle fachliche Entität über Subject laden
- Änderungen über Aggregate-Methoden ausführen
*/