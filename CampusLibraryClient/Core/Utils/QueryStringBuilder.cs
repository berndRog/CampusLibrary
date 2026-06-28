namespace CampusLibraryClient.Core.Utils;

public static class QueryStringBuilder {

   public static string Bool(bool value) =>
      value ? "true" : "false";
}
