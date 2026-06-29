using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CampusLibrary.Shared.Logging;

/// <summary>
/// Logs outgoing HTTP calls with human-readable diagnostics.
///
/// This handler is useful in the Blazor SSR client when it calls CampusLibraryApi.
/// Later it can also be used by other hosts when they call another service.
/// </summary>
public sealed class OutgoingHttpLoggingHandler(
   ILogger<OutgoingHttpLoggingHandler> logger
) : DelegatingHandler {

   protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken ct
   ) {
      var stopwatch = Stopwatch.StartNew();
      string endpoint = request.RequestUri?.PathAndQuery
                        ?? request.RequestUri?.ToString()
                        ?? "unknown";

      bool hasToken = request.Headers.Authorization?.Scheme == "Bearer" &&
                      !string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter);

      AppDiagnosticsLogger.LogApiCall(
         logger: logger,
         method: request.Method,
         endpoint: endpoint
      );

      AppDiagnosticsLogger.LogTokenAttached(
         logger: logger,
         hasToken: hasToken,
         pathAndQuery: endpoint
      );

      try {
         HttpResponseMessage response = await base.SendAsync(
            request: request,
            cancellationToken: ct
         );

         stopwatch.Stop();

         string? errorMessage = response.IsSuccessStatusCode
            ? null
            : $"HTTP {(int)response.StatusCode}";

         AppDiagnosticsLogger.LogApiResponse(
            logger: logger,
            method: request.Method,
            endpoint: endpoint,
            statusCode: (int)response.StatusCode,
            durationMs: stopwatch.ElapsedMilliseconds,
            errorMessage: errorMessage
         );

         return response;
      }
      catch(Exception ex) when(ex is not OperationCanceledException) {
         stopwatch.Stop();

         AppDiagnosticsLogger.LogException(
            logger: logger,
            exception: ex,
            title: $"API call failed: {request.Method} {endpoint}",
            detail: "The outgoing HTTP request failed before a response was received."
         );

         throw;
      }
   }
}
