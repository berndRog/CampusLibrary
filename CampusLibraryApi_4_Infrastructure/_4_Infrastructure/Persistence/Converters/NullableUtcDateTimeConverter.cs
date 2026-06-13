using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Converters;

public sealed class NullableUtcDateTimeConverter
   : ValueConverter<DateTime?, DateTime?> {

   public NullableUtcDateTimeConverter()
      : base(
         // Before saving:
         // Domain/Application should already provide UTC DateTime values.
         // Null remains null.
         v => v,

         // After reading:
         // SQLite does not preserve DateTimeKind.
         // If a value exists, restore the UTC kind.
         v => v.HasValue
            ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            : null
      ) {
   }
}