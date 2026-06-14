using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class AuthorUcDeactivate(
   IAuthorRepository authorRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AuthorUcDeactivate> logger
) {

   public async Task<Result<AuthorDto>> ExecuteAsync(
      Guid authorId,
      CancellationToken ct = default
   ) {
      if (authorId == Guid.Empty)
         return Result<AuthorDto>.Failure(CatalogErrors.InvalidAuthorId);

      // Load the Author aggregate.
      // With a global query filter, this returns active authors only.
      var author = await authorRepository.FindByIdAsync(
         id: authorId,
         ct: ct
      );

      if (author is null)
         return Result<AuthorDto>.Failure(CatalogErrors.AuthorNotFound);

      // The aggregate controls its active state.
      var resultDeactivated = author.Deactivate(
         updatedAt: clock.UtcNow
      );

      if (resultDeactivated.IsFailure)
         return Result<AuthorDto>.Failure(resultDeactivated.Error);

      // No repository.Update(author) is needed.
      // The aggregate was loaded by EF Core and is already tracked.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "AuthorUcDeactivate",
         ct
      );

      logger.LogDebug(
         "AuthorUcDeactivate completed for author {AuthorId}. Saved rows: {Rows}.",
         author.Id,
         rows
      );

      return Result<AuthorDto>.Success(author.ToAuthorDto());
   }
}