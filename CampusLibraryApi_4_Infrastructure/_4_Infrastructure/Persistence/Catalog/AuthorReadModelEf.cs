using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class AuthorReadModelEf(
   ICatalogDbContext dbContext
) : IAuthorReadModel {

   public async Task<Result<AuthorDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      if (id == Guid.Empty)
         return Result<AuthorDto>.Failure(CatalogErrors.InvalidAuthorId);

      var author = await dbContext.Authors
         .AsNoTracking()
         .SingleOrDefaultAsync(a => a.Id == id && a.IsActive , ct);
      if (author is null)
         return Result<AuthorDto>.Failure(CatalogErrors.AuthorNotFound);

      return Result<AuthorDto>.Success(author.ToAuthorDto());
   }

   public async Task<Result<IReadOnlyList<AuthorDto>>> SelectAllAsync(
      CancellationToken ct = default
   ) {
      var authors = await dbContext.Authors
         .AsNoTracking()
         .Where(a => a.IsActive)
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .ToListAsync(ct);

      var authorDtos = authors
         .Select(author => author.ToAuthorDto())
         .ToList();

      return Result<IReadOnlyList<AuthorDto>>.Success(authorDtos);
   }

   public async Task<Result<IReadOnlyList<AuthorDto>>> SearchAsync(
      string searchText,
      CancellationToken ct = default
   ) {
      if (string.IsNullOrWhiteSpace(searchText))
         return Result<IReadOnlyList<AuthorDto>>.Success([]);

      var pattern = $"%{searchText.Trim()}%";

      var authors = await dbContext.Authors
         .AsNoTracking()
         .Where(a => a.IsActive)
         .Where(a =>
            EF.Functions.Like(a.Lastname, pattern))
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .ToListAsync(ct);

      var authorDtos = authors
         .Select(author => author.ToAuthorDto())
         .ToList();

      return Result<IReadOnlyList<AuthorDto>>.Success(authorDtos);
   }
}

/*
Lernziele und Didaktik
----------------------

Dieses ReadModel gehört zur Query-Seite des Catalog-Moduls.

Es verwendet EF Core direkt, lädt aber keine Domain-Objekte für fachliche
Änderungen, sondern projiziert Daten auf AuthorDto. Dadurch bleibt die
Web-Schicht von der Domain-Schicht entkoppelt.

Inaktive Autoren werden nicht global über einen EF QueryFilter ausgeblendet,
weil bestehende Book-Author-Beziehungen weiterhin stabil sichtbar bleiben
sollen. Für normale Listen und Suchfunktionen filtert dieses ReadModel deshalb
explizit auf IsActive == true.

Damit wird sichtbar:

- Repository: lädt Aggregate für fachliche Änderungen.
- ReadModel: liefert DTOs für Anzeige und Suche.
- UseCase: verändert Zustand.
- Controller: verwendet Schnittstellen, keine konkreten Klassen.
*/