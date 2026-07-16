using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Identity;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Self-service update of the Reader represented by the current technical identity.
// Part 5 obtains the identity from the API-side DevIdentity configuration.
// Part 6 obtains the same values from validated access-token claims.
internal sealed class ReaderUcUpdateMe(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcUpdateMe> logger
) {
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderUpdateDto? dto,
      CancellationToken ct
   ) {
      if(dto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderUpdateDtoRequired);

      Result<string> identityResult = IdentitySubject.Check(identityGateway);
      if(identityResult.IsFailure)
         return Result<ReaderDto>.Failure(identityResult.Error);

      string subject = identityResult.Value;

      Reader? reader = await repository.FindBySubjectAsync(subject, ct);
      if(reader is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      EmailVo? newEmailVo = null;

      if(dto.Email is not null) {
         Result<EmailVo> emailResult = EmailVo.Create(dto.Email);
         if(emailResult.IsFailure)
            return Result<ReaderDto>.Failure(emailResult.Error);

         newEmailVo = emailResult.Value;

         Reader? readerWithSameEmail =
            await repository.FindByEmailAsync(newEmailVo, ct);

         if(readerWithSameEmail is not null &&
            readerWithSameEmail.Id != reader.Id) {
            return Result<ReaderDto>.Failure(
               ReaderErrors.EmailAlreadyInUse
            );
         }
      }

      AddressVo? newAddressVo = null;

      if(dto.AddressDto is not null) {
         Result<AddressVo> addressResult = AddressVo.Create(
            street: dto.AddressDto.Street,
            postalCode: dto.AddressDto.PostalCode,
            city: dto.AddressDto.City,
            country: dto.AddressDto.Country
         );

         if(addressResult.IsFailure)
            return Result<ReaderDto>.Failure(addressResult.Error);

         newAddressVo = addressResult.Value;
      }

      Result updateResult = reader.UpdateProfile(
         lastname: dto.Lastname,
         emailVo: newEmailVo,
         addressVo: newAddressVo,
         updatedAt: clock.UtcNow
      );

      if(updateResult.IsFailure)
         return Result<ReaderDto>.Failure(updateResult.Error);

      int rows = await unitOfWork.SaveAllChangesAsync(
         text: nameof(ReaderUcUpdateMe),
         ct: ct
      );

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

ReaderUcUpdateMe aktualisiert den fachlichen Reader der aktuellen technischen
Identität. Der Client sendet keine ReaderId. Dadurch kann er nicht auswählen,
welcher Reader geändert wird.

Teil 5:
- IIdentityGateway wird durch die API-eigene DevIdentity-Konfiguration gespeist.

Teil 6:
- IIdentityGateway wird aus validierten Claims des Access Tokens gespeist.

Der Use Case bleibt in beiden Teilen gleich:

1. technische Reader-Identität mit IdentitySubject.Check prüfen
2. Reader über das stabile Subject laden
3. optionale E-Mail und Adresse validieren
4. E-Mail-Eindeutigkeit prüfen
5. Reader.UpdateProfile(...) ausführen
6. Änderung über IUnitOfWork speichern
*/
