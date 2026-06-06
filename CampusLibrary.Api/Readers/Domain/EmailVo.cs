using CampusLibrary.Api.Shared;

namespace CampusLibrary.Api.Readers.Domain;

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
