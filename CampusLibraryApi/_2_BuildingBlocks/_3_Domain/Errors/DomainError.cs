using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
namespace CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

public sealed record DomainError(
   WebErrorStatus Status,
   string Title,
   string Message
) {
   public static readonly DomainError None =
      new(
         WebErrorStatus.None,
         "None",
         string.Empty
      );
};
