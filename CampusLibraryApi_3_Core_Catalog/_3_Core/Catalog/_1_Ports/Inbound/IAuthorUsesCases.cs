using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

// Facade interface for author-related command use cases.
// Controllers depend on this interface instead of depending on concrete use case classes.
public interface IAuthorUseCases {

   // Creates a new Author aggregate.
   Task<Result<AuthorDto>> CreateAsync(
      AuthorCreateDto? dto,
      CancellationToken ct = default
   );

   // Deactivates an Author without physically deleting it from the database.
   Task<Result<AuthorDto>> DeactivateAsync(
      Guid authorId,
      CancellationToken ct = default
   );
}

/*
Lernziele und Didaktik
----------------------

Diese Schnittstelle bündelt die schreibenden Anwendungsfälle für Authors.

CreateAsync und DeactivateAsync verändern den Zustand des Systems. Deshalb
gehören sie auf die Command-Seite der Application-Schicht.

Lesende Operationen wie FindByIdAsync, SelectAllAsync oder SearchAsync gehören
nicht in diese UseCase-Fassade. Sie werden über ein separates ReadModel
bereitgestellt.

Damit bleibt die Trennung klar:

- UseCases verändern Zustand.
- ReadModels liefern Daten für Anzeige und Suche.
- Controller hängen nur von Schnittstellen ab, nicht von konkreten Klassen.

Die Fassade ersetzt nicht die einzelnen UseCase-Klassen. Sie bündelt nur die
schreibenden Anwendungsfälle für die Web-Schicht.
*/