namespace CampusLibraryClient.Api.Errors;

public sealed record ApiError(
   int Status,
   string Title,
   string? Detail = null,
   string? ErrorCode = null
);
