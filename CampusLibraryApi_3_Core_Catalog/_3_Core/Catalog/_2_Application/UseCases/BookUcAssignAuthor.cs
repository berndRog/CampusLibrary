using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcAssignAuthor(
   IBookRepository bookRepository,
   IAuthorRepository authorRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<BookUcAssignAuthor> logger
) {
   public async Task<Result<BookDto>> ExecuteAsync(
      Guid bookId,
      BookAssignAuthorDto? dto,
      CancellationToken ct = default
   ) {
      // The book id is required for this use case.
      if (bookId == Guid.Empty)
         return Result<BookDto>.Failure(CatalogErrors.InvalidBookId);
      
      // The dto is required.
      if (dto is null)
         return Result<BookDto>.Failure(CatalogErrors.BookAssignAuthorDtoRequired);

      // The author id is required.
      if (dto.AuthorId == Guid.Empty)
         return Result<BookDto>.Failure(CatalogErrors.InvalidAuthorId);

      // Load the Book aggregate including its assigned Authors.
      var book = await bookRepository.FindByIdAsync(
         id: bookId,
         ct: ct
      );

      if (book is null)
         return Result<BookDto>.Failure(CatalogErrors.BookNotFound);

      // Load the Author aggregate.
      var author = await authorRepository.FindByIdAsync(
         id: dto.AuthorId,
         ct: ct
      );

      if (author is null)
         return Result<BookDto>.Failure(CatalogErrors.AuthorNotFound);

      // The Book aggregate controls the assignment.
      // EF Core maps the m:n relationship to the BookAuthors join table.
      var resultAssigned = book.AssignAuthor(
         author: author,
         updatedAt: clock.UtcNow
      );

      if (resultAssigned.IsFailure)
         return Result<BookDto>.Failure(resultAssigned.Error);

      // Save all changes to the database:
      // inserted BookAuthors row + updated Book.UpdatedAt.
      var rows = await unitOfWork.SaveAllChangesAsync(
         text: "BookUcAssignAuthor",
         ct: ct
      );

      logger.LogDebug(
         "BookUcAssignAuthor completed for book {BookId} and author {AuthorId}. Saved rows: {Rows}.",
         book.Id, author.Id, rows
      );

      return Result<BookDto>.Success(book.ToBookDto());
   }
}