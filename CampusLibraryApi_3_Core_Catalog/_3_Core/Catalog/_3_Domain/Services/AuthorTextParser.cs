namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Services;

public static class AuthorTextParser {

   public static IReadOnlyList<string> ExtractLastnames(
      string? authorsText
   ) {
      if (string.IsNullOrWhiteSpace(authorsText))
         return [];

      string[] authorTokens = authorsText.Split(
         separator: ',',
         options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
      );

      List<string> lastnames = [];

      foreach (string authorToken in authorTokens) {

         string[] nameParts = authorToken.Split(
            separator: ' ',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
         );

         if (nameParts.Length == 0)
            continue;

         string lastname = nameParts[^1];

         if (string.IsNullOrWhiteSpace(lastname))
            continue;

         lastnames.Add(lastname);
      }

      return lastnames;
   }

   public static string NormalizeAuthorsText(
      string? authorsText
   ) {
      if (string.IsNullOrWhiteSpace(authorsText))
         return string.Empty;

      string[] authorTokens = authorsText.Split(
         separator: ',',
         options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
      );

      return string.Join(
         separator: ", ",
         values: authorTokens
      );
   }
}