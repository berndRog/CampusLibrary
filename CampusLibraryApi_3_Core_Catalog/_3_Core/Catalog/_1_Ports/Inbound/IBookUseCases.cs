using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

// Facade interface for book-related command use cases.
// Controllers depend on this interface instead of depending on concrete use case classes.
public interface IBookUseCases {

   // Creates a new Book aggregate.
   Task<Result<BookDto>> CreateAsync(
      BookCreateDto? dto,
      CancellationToken ct = default
   );

   // Adds a physical BookItem to an existing Book.
   Task<Result<BookItemDto>> AddBookItemAsync(
      Guid bookId,
      BookItemAddDto? dto,
      CancellationToken ct = default
   );
   
   // Deactivates a Book without physically deleting it from the database.
   Task<Result<BookDto>> DeactivateAsync(
      Guid bookId,
      CancellationToken ct = default
   );
}

/*
Lernziele und Didaktik
----------------------

Diese Schnittstelle bündelt die schreibenden Anwendungsfälle für Books.

Die Methoden verändern den Zustand des Systems:

- ein Book wird angelegt
- ein BookItem wird hinzugefügt
- ein Author wird einem Book zugeordnet
- ein Book wird deaktiviert

Such- und Anzeigeoperationen gehören nicht in diese Schnittstelle. Sie werden
über IBookReadModel abgebildet.

Damit wird die Trennung zwischen Command-Seite und Query-Seite sichtbar:

- IBookUseCases: fachliche Änderungen
- IBookReadModel: Lesen, Suchen, Anzeigen

Der Controller verwendet weiterhin nur Schnittstellen. Er kennt aber nicht die
konkreten UseCase-Klassen wie BookUcCreate oder BookUcAssignAuthor.
*/