using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Identity;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

internal sealed class ReaderUcCreateMeProvision(
   IIdentityGateway identityGateway,
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<ReaderUcCreateMeProvision> logger
) {
   public async Task<Result<bool>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      var identityResult = IdentitySubject.Check(identityGateway);
      if(identityResult.IsFailure)
         return Result<bool>.Failure(identityResult.Error);

      var subject = identityResult.Value;
      var existingReader = await repository.FindBySubjectAsync(subject, ct);
      if(existingReader is not null) {
         logger.LogInformation(
            "Reader provisioning is idempotent: existing reader {ReaderId} returned",
            existingReader.Id
         );
         return Result<bool>.Success(false);
      }

      var emailResult = EmailVo.Create(identityGateway.Username?.Trim() ?? string.Empty);
      if(emailResult.IsFailure)
         return Result<bool>.Failure(emailResult.Error);

      var existingReaderByEmail = await repository.FindByEmailAsync(emailResult.Value, ct);
      if(existingReaderByEmail is not null)
         return Result<bool>.Failure(ReaderErrors.EmailAlreadyInUse);

      var idResult = EntityId.Resolve(id, ReaderErrors.InvalidId);
      if(idResult.IsFailure)
         return Result<bool>.Failure(idResult.Error);

      var readerResult = Reader.Provision(
         id: idResult.Value,
         subject: subject,
         emailVo: emailResult.Value,
         createdAt: identityGateway.CreatedAt
      );
      if(readerResult.IsFailure)
         return Result<bool>.Failure(readerResult.Error);

      repository.Add(readerResult.Value);
      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcCreateMeProvision", ct);

      logger.LogInformation(
         "Reader provisioned: readerId={ReaderId} subject={Subject} rows={Rows}",
         readerResult.Value.Id,
         subject,
         rows
      );

      return Result<bool>.Success(true);
   }
}
