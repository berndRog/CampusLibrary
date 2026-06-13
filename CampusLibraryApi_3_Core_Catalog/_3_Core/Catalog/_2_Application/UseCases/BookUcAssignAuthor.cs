// using CampusLibraryApi._2_BuildingBlocks;
// using CampusLibraryApi._2_BuildingBlocks._1_Ports;
// using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
// using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
// using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
// using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
// using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
//
// namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
//
// public sealed class BookUcAssignAuthor(
//    IBookRepository bookRepository,
//    IAuthorRepository authorRepository,
//    IUnitOfWork unitOfWork,
//    IClock clock
// ) {
//
//    public async Task<Result<BookAuthorDto>> ExecuteAsync(
//       Guid bookId,
//       BookAssignAuthorDto dto,
//       CancellationToken ct = default
//    ) {
//       // The book id is required for this use case.
//       if(bookId == Guid.Empty)
//          return Result<BookAuthorDto>.Failure(CatalogErrors.InvalidBookId);
//       // The dto is required
//       if (dto is null)
//          return Result<BookAuthorDto>.Failure(CatalogErrors.BookAssignAuthorDtoRequired);
//       // The author id is required
//       if(dto.AuthorId == Guid.Empty)
//          return Result<BookAuthorDto>.Failure(CatalogErrors.InvalidAuthorId);
//       
//       // load book by id
//       var book = await bookRepository.FindByIdAsync(bookId, ct);
//       if (book is null)
//          return Result<BookAuthorDto>.Failure(CatalogErrors.BookNotFound);
//
//       // load author by id
//       var author = await authorRepository.FindByIdAsync(dto.AuthorId, ct);
//       if (author is null)
//          return Result<BookAuthorDto>.Failure(CatalogErrors.AuthorNotFound);
//
//       // Resolve or generate the join entity id.
//       var resultId = EntityId.Resolve(dto.Id, CatalogErrors.InvalidBookAuthorId);
//       if (resultId.IsFailure)
//          return Result<BookAuthorDto>.Failure(resultId.Error);
//       var id = resultId.Value;
//       
//       // Book aggregate controls the BookAuthor assignment.
//       // var bookAuthorResult = book.AssignAuthor(
//       //    bookAuthorId: id,
//       //    authorId: author.Id,
//       //    updatedAt: clock.UtcNow
//       // );
//       if (bookAuthorResult.IsFailure)
//          return Result<BookAuthorDto>.Failure(bookAuthorResult.Error);
//       
//       var rows = await unitOfWork.SaveAllChangesAsync("BookUcAssignAuthor",ct);
//
//       return Result<BookAuthorDto>.Success(bookAuthorResult.Value.ToBookAuthorDto());
//    }
// }