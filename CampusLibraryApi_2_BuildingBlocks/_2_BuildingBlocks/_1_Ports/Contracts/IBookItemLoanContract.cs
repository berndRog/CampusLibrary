using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
namespace CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;

// Contract provided by the Catalog module for the Loans module.
// It exposes only book item information that is relevant for lending books.
public interface IBookItemLoanContract {

   // Finds loan-relevant information for one concrete book item.
   // The Loans module must not access Book, BookItem or the Catalog tables directly.
   Task<Result<BookItemLoanInfoDto>> FindBookItemForLoanAsync(
      Guid id,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface ist ein fachlicher Contract des Catalog-Moduls.

Das Catalog-Modul besitzt Books und BookItems. Deshalb definiert auch das
Catalog-Modul, welche Informationen über ein BookItem andere Module verwenden
dürfen.

Das Loans-Modul benötigt beim Ausleihen kein vollständiges Book-Aggregate.
Es muss nur wissen, ob das konkrete BookItem existiert und welche
anzeigerelevanten Informationen dazu gehören.

Das Loans-Modul darf deshalb nicht direkt auf die Books- oder BookItems-
Tabellen zugreifen. Stattdessen fragt es diesen Contract.

Damit bleibt die fachliche Zuständigkeit klar:
Catalog verwaltet den Bestand.
Loans verwaltet Ausleihvorgänge.
*/