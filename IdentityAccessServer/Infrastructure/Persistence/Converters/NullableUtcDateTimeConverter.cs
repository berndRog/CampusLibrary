using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace IdentityAccessServer.Infrastructure.Persistence.Converters;

public sealed class NullableUtcDateTimeConverter
   : ValueConverter<DateTime?, DateTime?> {

   public NullableUtcDateTimeConverter()
      : base(
         // Before saving:
         v => v.HasValue
            ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime())
            : v,

         // SQLite does not preserve DateTimeKind.
         v => v.HasValue
            ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            : v
      ) {
   }
}