using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Identity;

// Validates the configured or token-based technical Reader identity.
// The subject is treated as an opaque stable identifier.
public static class IdentitySubject {
   public static Result<string> Check(
      IIdentityGateway identityGateway
   ) {
      if(!identityGateway.IsAuthenticated)
         return Result<string>.Failure(CommonErrors.IdentityUnauthenticated);

      if(!identityGateway.IsReader)
         return Result<string>.Failure(CommonErrors.AccessNotAllowed);

      if(string.IsNullOrWhiteSpace(identityGateway.Subject))
         return Result<string>.Failure(CommonErrors.SubjectRequired);

      if(identityGateway.Subject.Length > 200)
         return Result<string>.Failure(CommonErrors.InvalidIdentitySubject);

      if(string.IsNullOrWhiteSpace(identityGateway.Username))
         return Result<string>.Failure(CommonErrors.IdentityEmailRequired);

      if(identityGateway.CreatedAt == default)
         return Result<string>.Failure(CommonErrors.TimestampInvalid);

      return Result<string>.Success(identityGateway.Subject);
   }
}
