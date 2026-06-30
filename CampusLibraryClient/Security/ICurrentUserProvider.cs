namespace CampusLibraryClient.Security;

public interface ICurrentUserProvider {

   Task<CurrentUserInfo> GetCurrentUserAsync(
      CancellationToken ct = default
   );
}
