using System.Net;
using System.Net.Http.Headers;
using CampusLibrary.Shared.Logging;
using Microsoft.AspNetCore.Authentication;

namespace CampusLibraryClient.Api.Auth;

/// <summary>
/// Attaches the current user's access token as a Bearer header on every outgoing
/// CampusLibraryApi request. Silently refreshes the token when it is about to expire.
/// </summary>
public sealed class AccessTokenHandler(
   IHttpContextAccessor ctxAccessor,
   IHttpClientFactory httpClientFactory,
   IConfiguration config,
   ILogger<AccessTokenHandler> logger
) : DelegatingHandler {

   protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken ct
   ) {
      HttpContext? httpCtx = ctxAccessor.HttpContext;

      if(httpCtx is not null) {
         // Silent token refresh with student-visible lifecycle logging.
         try {
            await httpCtx.TryRefreshAccessTokenAsync(
               httpClientFactory: httpClientFactory,
               config: config,
               ct: ct,
               logger: logger
            );
         }
         catch(Exception ex) {
            // A network or configuration failure during refresh is logged and the
            // request continues. The old token may still be valid.
            AppDiagnosticsLogger.LogException(
               logger: logger,
               exception: ex,
               title: "Silent token refresh",
               detail: "Could not reach the IdentityAccessServer to refresh the access token. " +
                       "The existing token will be used, but it may have already expired."
            );
         }

         // The OIDC middleware stores the access token in the authenticated
         // application ticket because SaveTokens is enabled.
         string? token = await httpCtx.GetTokenAsync("access_token");

         if(!string.IsNullOrWhiteSpace(token)) {
            request.Headers.Authorization = new AuthenticationHeaderValue(
               scheme: "Bearer",
               parameter: token
            );

            logger.LogDebug(
               "Bearer token attached to outgoing API request {Method} {Endpoint}.",
               request.Method,
               request.RequestUri?.PathAndQuery
            );
         }
         else {
            logger.LogWarning(
               "No access token was found in the application cookie for outgoing API request {Method} {Endpoint}.",
               request.Method,
               request.RequestUri?.PathAndQuery
            );
         }
      }
      else {
         logger.LogWarning(
            "No HttpContext is available for outgoing API request {Method} {Endpoint}; no Bearer token can be attached.",
            request.Method,
            request.RequestUri?.PathAndQuery
         );
      }

      HttpResponseMessage response = await base.SendAsync(
         request: request,
         cancellationToken: ct
      );

      if(response.StatusCode == HttpStatusCode.Unauthorized) {
         AppDiagnosticsLogger.LogAuthorizationFailure(
            logger: logger,
            detail: "CampusLibraryApi returned 401 - the access token is expired or invalid. " +
                    "Try logging out and back in."
         );

         throw new ApiUnauthorizedException();
      }

      return response;
   }
}
