namespace CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

public sealed record ReaderCreateDto(
   string Subject,
   string Email,
   string DisplayName
);
