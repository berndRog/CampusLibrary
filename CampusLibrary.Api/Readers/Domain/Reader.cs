using CampusLibrary.Api.Shared;

namespace CampusLibrary.Api.Readers.Domain;

public sealed class Reader : AggregateRoot {

   public Guid Id { get; private set; }
   public string Subject { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = null!;
   public string DisplayName { get; private set; } = string.Empty;

   private Reader() {
      // Required by EF Core.
   }

   private Reader(
      Guid id,
      string subject,
      EmailVo emailVo,
      string displayName
   ) {
      Id = id;
      Subject = subject;
      EmailVo = emailVo;
      DisplayName = displayName;
   }

   public static Result<Reader> Create(
      Guid id,
      string subject,
      EmailVo emailVo,
      string displayName
   ) {
      if (id == Guid.Empty)
         return Result<Reader>.Failure(ReaderErrors.IdRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(ReaderErrors.SubjectRequired);

      if (string.IsNullOrWhiteSpace(displayName))
         return Result<Reader>.Failure(ReaderErrors.DisplayNameRequired);

      return Result<Reader>.Success(
         new Reader(
            id,
            subject.Trim(),
            emailVo,
            displayName.Trim()
         )
      );
   }

   public Result UpdateProfile(string displayName) {
      if (string.IsNullOrWhiteSpace(displayName))
         return Result.Failure(ReaderErrors.DisplayNameRequired);

      DisplayName = displayName.Trim();

      return Result.Success();
   }
}
