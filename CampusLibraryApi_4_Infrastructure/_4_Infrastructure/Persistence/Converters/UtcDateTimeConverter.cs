using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Converters;

public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime> {

   public UtcDateTimeConverter() : base(
      // Domain objects already validate UTC before persistence.
      value => value,

      // SQLite does not preserve DateTimeKind reliably.
      value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
   ) {
   }
}