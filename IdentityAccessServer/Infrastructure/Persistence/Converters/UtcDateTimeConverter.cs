using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace IdentityAccessServer.Infrastructure.Persistence.Converters;

public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime> {

   public UtcDateTimeConverter() : base(
      v => v.Kind == DateTimeKind.Utc 
         ? v 
         : v.ToUniversalTime(),
      // SQLite does not preserve DateTimeKind reliably.
      v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
   ) {
   }
}