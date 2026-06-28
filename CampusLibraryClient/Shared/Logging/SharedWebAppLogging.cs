namespace CampusLibraryClient.Shared.Logging;

public static class SharedWebAppLogging {

   public static WebApplicationBuilder ConfigureSharedWebAppLogging(
      this WebApplicationBuilder builder
   ) {
      builder.Logging.AddConsole();
      return builder;
   }
}
