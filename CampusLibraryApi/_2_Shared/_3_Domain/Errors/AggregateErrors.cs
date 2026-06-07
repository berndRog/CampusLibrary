using CampusLibraryApi._2_Shared._3_Domain.Enums;
namespace CampusLibraryApi._2_Shared._3_Domain.Errors;

public static class AggregateErrors {

   public static readonly DomainError CreatedAtRequired =
      new(
         WebErrorStatus.BadRequest,
         "Aggregate: CreatedAtRequired",
         "CreatedAt must be set."
      );

   public static readonly DomainError CreatedAtMustBeUtc =
      new(
         WebErrorStatus.BadRequest,
         "Aggregate: CreatedAtMustBeUtc",
         "CreatedAt must be UTC."
      );

   public static readonly DomainError UpdatedAtRequired =
      new(
         WebErrorStatus.BadRequest,
         "Aggregate: UpdatedAtRequired",
         "UpdatedAt must be set."
      );

   public static readonly DomainError UpdatedAtMustBeUtc =
      new(
         WebErrorStatus.BadRequest,
         "Aggregate: UpdatedAtMustBeUtc",
         "UpdatedAt must be UTC."
      );

   public static readonly DomainError UpdatedAtBeforeCreatedAt =
      new(
         WebErrorStatus.BadRequest,
         "Aggregate: UpdatedAtBeforeCreatedAt",
         "UpdatedAt must not be before CreatedAt."
      );
}