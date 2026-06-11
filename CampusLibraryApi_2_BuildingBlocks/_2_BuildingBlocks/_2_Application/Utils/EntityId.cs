using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;


public static class EntityId {
   // <summary>
   // Resolves an entity id from an optional raw string.
   //
   // Behavior:
   // - null               → generates a new Guid
   // - empty/whitespace   → failure
   // - invalid Guid       → failure
   // - valid Guid         → success
   //
   // Purpose:
   // - Centralizes ID parsing logic.
   // - Ensures consistent validation and error handling.
   // - Prevents Guid parsing logic from leaking into controllers or use cases.
   public static Result<Guid> Resolve(
      string? rawId,
      DomainError invalidIdError
   ) {
      // If no id is provided, generate a new identity
      if (rawId is null)
         return Result<Guid>.Success(Guid.NewGuid());

      // Reject empty or whitespace input
      if (string.IsNullOrWhiteSpace(rawId))
         return Result<Guid>.Failure(invalidIdError);

      // Attempt to parse Guid
      if (!Guid.TryParse(rawId, out var guid))
         return Result<Guid>.Failure(invalidIdError);

      return Result<Guid>.Success(guid);
   }
}