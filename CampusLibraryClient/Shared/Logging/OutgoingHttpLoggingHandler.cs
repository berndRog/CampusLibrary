using CampusLibraryClient.Shared.Logging;

namespace CampusLibraryClient.Shared.Logging;

public sealed class OutgoingHttpLoggingHandler(
   ILogger<OutgoingHttpLoggingHandler> logger
) : DelegatingHandler {

   protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken ct
   ) {
      bool hasToken = request.Headers.Authorization?.Scheme == "Bearer" &&
                      !string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter);

      AppDiagnosticsLogger.LogTokenAttached(
         logger: logger,
         hasToken: hasToken,
         pathAndQuery: request.RequestUri?.PathAndQuery
      );

      return await base.SendAsync(
         request: request,
         cancellationToken: ct
      );
   }
}
