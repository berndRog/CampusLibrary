namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderDto(
   Guid Id,
   string Subject,
   string Email,
   string DisplayName
);
