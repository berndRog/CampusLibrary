namespace CampusLibraryClient.Api.Auth;

public sealed class ApiUnauthorizedException : Exception {

   public ApiUnauthorizedException()
      : base("Unauthorized (access token expired or invalid).") {
   }
}
