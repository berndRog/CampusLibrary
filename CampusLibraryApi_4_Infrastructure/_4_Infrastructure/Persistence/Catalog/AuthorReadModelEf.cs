using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class AuthorReadModelEf(
   ICatalogDbContext dbContext
) : IAuthorReadModel {

   public async Task<AuthorDto?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      if (id == Guid.Empty)
         return null;

      var author = await dbContext.Authors
         .AsNoTracking()
         .FirstOrDefaultAsync(a => a.Id == id, ct);

      return author?.ToAuthorDto();
   }

   public async Task<IReadOnlyList<AuthorDto>> SelectAllAsync(
      CancellationToken ct = default
   ) {
      var authors = await dbContext.Authors
         .AsNoTracking()
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .ToListAsync(ct);

      return authors
         .Select(author => author.ToAuthorDto())
         .ToList();
   }

   public async Task<IReadOnlyList<AuthorDto>> SearchAsync(
      string searchText,
      CancellationToken ct = default
   ) {
      if (string.IsNullOrWhiteSpace(searchText))
         return [];

      var normalizedSearchText = searchText.Trim();

      var authors = await dbContext.Authors
         .AsNoTracking()
         .Where(a =>
            a.Firstname.Contains(normalizedSearchText) ||
            a.Lastname.Contains(normalizedSearchText) ||
            (a.Firstname + " " + a.Lastname).Contains(normalizedSearchText))
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .ToListAsync(
            cancellationToken: ct
         );

      return authors
         .Select(author => author.ToAuthorDto())
         .ToList();
   }
}