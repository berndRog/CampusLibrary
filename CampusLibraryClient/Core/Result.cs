using CampusLibraryClient.Api.Errors;

namespace CampusLibraryClient.Core;

public sealed class Result<T> {

   public bool IsSuccess { get; }

   public bool IsFailure => !IsSuccess;

   public T? Value { get; }

   public ApiError? Error { get; }

   private Result(
      bool isSuccess,
      T? value,
      ApiError? error
   ) {
      IsSuccess = isSuccess;
      Value = value;
      Error = error;
   }

   public static Result<T> Success(T value) =>
      new(
         isSuccess: true,
         value: value,
         error: null
      );

   public static Result<T> Failure(ApiError error) =>
      new(
         isSuccess: false,
         value: default,
         error: error
      );
}
