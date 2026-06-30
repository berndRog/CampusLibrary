namespace CampusLibraryClient.Security;

public sealed class AnonymousCurrentUserProvider : ICurrentUserProvider {

   public Task<CurrentUserInfo> GetCurrentUserAsync(
      CancellationToken ct = default
   ) =>
      Task.FromResult(CurrentUserInfo.Anonymous);
}
