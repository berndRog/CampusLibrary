using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

public sealed record EmailVo {

   public string Value { get; }

   private EmailVo(string value) {
      Value = value;
   }

   public static Result<EmailVo> Create(string? value) {
      if (string.IsNullOrWhiteSpace(value))
         return Result<EmailVo>.Failure(ReaderErrors.EmailRequired);

      var normalized = value.Trim().ToLowerInvariant();

      if (!normalized.Contains('@'))
         return Result<EmailVo>.Failure(ReaderErrors.EmailInvalid);

      if (normalized.Length > 120)
         return Result<EmailVo>.Failure(ReaderErrors.EmailTooLong);

      return Result<EmailVo>.Success(new EmailVo(normalized));
   }

   public static EmailVo FromPersisted(string value)
      => new(value);

   public override string ToString()
      => Value;
}
