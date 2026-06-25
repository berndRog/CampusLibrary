using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure._2_Persistence.Contracts;

// EF Core implementation of the Catalog contract used by the Loans module.
// This class is allowed to access Books and BookItems because Catalog owns them.
internal sealed class BookItemLoanContractEf(
   ICatalogDbContext catalogDbContext
) : IBookItemLoanContract {

   // Finds loan-relevant information for one concrete book item.
   // The Loans module receives only the DTO, not Catalog entities.
   public async Task<Result<BookItemLoanInfoDto>> FindByIdAsync(
      Guid bookItemId,
      CancellationToken ct
   ) {
      if (bookItemId == Guid.Empty)
         return Result<BookItemLoanInfoDto>.Failure(CatalogErrors.BookItemIdRequired);

      BookItemLoanInfoDto? dto = await (
         from bookItem in catalogDbContext.BookItems.AsNoTracking()
         join book in catalogDbContext.Books.AsNoTracking()
            on bookItem.BookId equals book.Id
         where bookItem.Id == bookItemId
         select new BookItemLoanInfoDto(
            bookItem.Id,
            book.Id,
            bookItem.InventoryNumber,
            book.Title,
            book.Subtitle,
            book.AuthorsText,
            book.IsbnVo.Value,
            book.IsActive,
            book.IsActive &&
               bookItem.Status == BookItemStatus.Available
         )
      ).FirstOrDefaultAsync(ct);

      if (dto is null)
         return Result<BookItemLoanInfoDto>.Failure(CatalogErrors.BookItemNotFound);
      
      return Result<BookItemLoanInfoDto>.Success(dto);
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse implementiert einen fachlichen Contract des Catalog-Moduls für
das Loans-Modul.

Die Implementierung liegt technisch in Infrastructure, weil hier EF Core für
den Datenbankzugriff verwendet wird. Fachlich gehört die Schnittstelle aber
zum Catalog-Modul, weil Catalog die Daten von Books und BookItems besitzt.

Innerhalb dieses Contracts darf auf Books und BookItems zugegriffen werden.
Das ist erlaubt, weil beide Tabellen zum Catalog-Modul gehören.

Das Loans-Modul erhält dagegen keinen direkten Zugriff auf Books, BookItems
oder den Catalog-DbContext. Es fragt nur den Contract und erhält ein
BookItemLoanInfoDto.

Besonders wichtig ist die Übersetzung von BookItemStatus zu
IsAvailableForLoan. Der interne Status des BookItems bleibt im Catalog-Modul.
Das Loans-Modul muss nicht wissen, ob ein Exemplar intern Available,
Unavailable, Lost oder Damaged heißt.

Für das Loans-Modul ist nur die fachliche Entscheidung relevant:
Darf dieses konkrete Exemplar ausgeliehen werden?

Dadurch bleibt die Modulgrenze sauber:
Catalog entscheidet über Bestand und Verfügbarkeit.
Loans entscheidet über den Ausleihvorgang.
*/