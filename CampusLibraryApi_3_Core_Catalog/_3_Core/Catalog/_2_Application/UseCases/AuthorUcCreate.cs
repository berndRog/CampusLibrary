using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class AuthorUcCreate(
   IAuthorRepository authorRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AuthorUcCreate> logger
) {
   public async Task<Result<AuthorDto>> ExecuteAsync(
      AuthorCreateDto? authorCreateDto,
      CancellationToken ct = default
   ) {
      if (authorCreateDto is null)
         return Result<AuthorDto>.Failure(CatalogErrors.AuthorCreateDtoRequired);
      var dto = authorCreateDto;
      
      // Resolve the optional external id into a domain id.
      var resultId = EntityId.Resolve(
         dto.Id,
         CatalogErrors.InvalidAuthorId
      );

      if (resultId.IsFailure)
         return Result<AuthorDto>.Failure(resultId.Error);

      // Create the aggregate first, so validation and trimming are applied.
      var resultAuthor = Author.Create(
         id: resultId.Value,
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         createdAt: clock.UtcNow
      );

      if (resultAuthor.IsFailure)
         return Result<AuthorDto>.Failure(resultAuthor.Error);

      var author = resultAuthor.Value;

      // Duplicate author detection requires persistence knowledge
      // and therefore belongs to the use case.
      var exists = await authorRepository.ExistsByNameAsync(
         author.Firstname,
         author.Lastname,
         ct
      );

      if (exists)
         return Result<AuthorDto>.Failure(CatalogErrors.AuthorAlreadyExists);

      // Add to repository.
      authorRepository.Add(author);

      // Save all changes to the database.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "AuthorUcCreate",
         ct
      );

      logger.LogDebug(
         "AuthorUcCreate completed for author {AuthorId}. Saved rows: {Rows}.",
         author.Id,
         rows
      );

      return Result<AuthorDto>.Success(author.ToAuthorDto());
   }
}