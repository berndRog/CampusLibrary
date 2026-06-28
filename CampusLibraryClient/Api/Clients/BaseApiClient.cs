using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CampusLibraryClient.Api.Auth;
using CampusLibraryClient.Api.Errors;
using CampusLibraryClient.Core;
using CampusLibraryClient.Shared.Logging;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryClient.Api.Clients;

public abstract class BaseApiClient<TClient>(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<TClient> logger
) where TClient : class {

   // The concrete API clients can use this HttpClient for custom calls.
   protected readonly HttpClient _http = factory.CreateClient(Common.CampusLibraryApiClientName);

   protected readonly JsonSerializerOptions _json = json;

   protected readonly ILogger<TClient> _logger = logger;

   protected async Task<Result<T>> SendAsync<T>(
      Func<Task<HttpResponseMessage>> send,
      CancellationToken ct = default
   ) {
      HttpResponseMessage response;

      try {
         response = await send();
      }
      catch(ApiUnauthorizedException) {
         // Token expired or invalid. The AccessTokenHandler detects this centrally.
         AppDiagnosticsLogger.LogAuthorizationFailure(
            logger: _logger,
            detail: "Session expired - the access token is no longer valid. The user needs to log in again."
         );

         return Result<T>.Failure(
            new ApiError(
               Status: 401,
               Title: "Unauthorized",
               Detail: "Session expired. Please login again."
            )
         );
      }
      catch(OperationCanceledException ex) {
         AppDiagnosticsLogger.LogException(
            logger: _logger,
            exception: ex,
            title: "Request canceled",
            detail: "The request was canceled. This can happen when the user navigates away or the server is slow."
         );

         return Result<T>.Failure(
            new ApiError(
               Status: 0,
               Title: "Request canceled",
               Detail: ex.Message
            )
         );
      }
      catch(Exception ex) {
         AppDiagnosticsLogger.LogException(
            logger: _logger,
            exception: ex,
            title: "Network error",
            detail: "A network error occurred while calling CampusLibraryApi. Check that CampusLibraryApi is running."
         );

         return Result<T>.Failure(
            new ApiError(
               Status: 0,
               Title: "Network error",
               Detail: ex.Message
            )
         );
      }

      // 204 No Content is treated as success. For bool, this means true.
      if(response.StatusCode == HttpStatusCode.NoContent) {
         if(typeof(T) == typeof(bool))
            return Result<T>.Success((T)(object)true);

         return Result<T>.Success(default!);
      }

      if(response.IsSuccessStatusCode) {
         if(typeof(T) == typeof(bool)) {
            Result<T>? boolResult = await TryReadBoolAsync<T>(
               response: response,
               ct: ct
            );

            if(boolResult is not null)
               return boolResult;
         }

         try {
            T? data = await response.Content.ReadFromJsonAsync<T>(
               options: _json,
               cancellationToken: ct
            );

            return Result<T>.Success(data!);
         }
         catch(Exception ex) {
            AppDiagnosticsLogger.LogException(
               logger: _logger,
               exception: ex,
               title: "Failed to deserialize success payload from CampusLibraryApi",
               detail: $"Expected type '{typeof(T).Name}' but the JSON could not be parsed. " +
                       "Check that CampusLibraryApi is returning the correct model."
            );

            return Result<T>.Failure(
               new ApiError(
                  Status: (int)response.StatusCode,
                  Title: "Invalid response payload",
                  Detail: ex.Message
               )
            );
         }
      }

      ApiError apiError = await ToApiError(
         response: response,
         ct: ct
      );

      AppDiagnosticsLogger.LogError(
         logger: _logger,
         title: $"CampusLibraryApi error {apiError.Status}",
         detail: $"{apiError.Title} - {apiError.Detail ?? "no details provided"}",
         extra: $"ErrorCode={apiError.ErrorCode ?? "n/a"}"
      );

      return Result<T>.Failure(apiError);
   }

   private async Task<Result<T>?> TryReadBoolAsync<T>(
      HttpResponseMessage response,
      CancellationToken ct
   ) {
      HttpContent content = response.Content;

      if(content.Headers?.ContentLength == 0)
         return Result<T>.Success((T)(object)true);

      MediaTypeHeaderValue? ctHeader = content.Headers?.ContentType;

      if(ctHeader is null) {
         string raw = await content.ReadAsStringAsync(ct);

         if(string.IsNullOrWhiteSpace(raw))
            return Result<T>.Success((T)(object)true);

         return Result<T>.Failure(
            new ApiError(
               Status: (int)response.StatusCode,
               Title: "Invalid response payload",
               Detail: $"Expected JSON boolean but got: {raw}"
            )
         );
      }

      try {
         bool? value = await content.ReadFromJsonAsync<bool>(
            options: _json,
            cancellationToken: ct
         );

         return Result<T>.Success((T)(object)(value ?? false));
      }
      catch(Exception ex) {
         AppDiagnosticsLogger.LogException(
            logger: _logger,
            exception: ex,
            title: "Failed to parse bool response from CampusLibraryApi",
            detail: "The response body was not a valid JSON boolean. " +
                    "Check that CampusLibraryApi is returning the correct content type."
         );

         return Result<T>.Failure(
            new ApiError(
               Status: (int)response.StatusCode,
               Title: "Invalid response payload",
               Detail: ex.Message
            )
         );
      }
   }

   private async Task<ApiError> ToApiError(
      HttpResponseMessage response,
      CancellationToken ct
   ) {
      // CampusLibraryApi returns ProblemDetails with Title/Detail/Status and a custom code extension.
      try {
         ProblemDetails? pd = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            options: _json,
            cancellationToken: ct
         );

         string? errorCode = null;

         if(pd?.Extensions is not null) {
            if(pd.Extensions.TryGetValue("errorCode", out object? errorCodeObj))
               errorCode = errorCodeObj?.ToString();
            else if(pd.Extensions.TryGetValue("code", out object? codeObj))
               errorCode = codeObj?.ToString();
         }

         return new ApiError(
            Status: (int)response.StatusCode,
            Title: pd?.Title ?? $"HTTP {(int)response.StatusCode}",
            Detail: pd?.Detail,
            ErrorCode: errorCode
         );
      }
      catch {
         string? raw;

         try {
            raw = await response.Content.ReadAsStringAsync(ct);
         }
         catch {
            raw = null;
         }

         return new ApiError(
            Status: (int)response.StatusCode,
            Title: $"HTTP {(int)response.StatusCode}",
            Detail: raw
         );
      }
   }
}
