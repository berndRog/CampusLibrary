using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class AuthorRepositoryEf(
   ICatalogDbContext dbContext
) : IAuthorRepository {
   
   public async Task<Author?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) => await dbContext.Authors
      .FirstOrDefaultAsync(
         predicate: a => a.Id == id,
         cancellationToken: ct
      );

   public async Task<bool> ExistsByNameAsync(
      string firstname,
      string lastname,
      CancellationToken ct = default
   ) {
      var normalizedFirstname = firstname.Trim();
      var normalizedLastname = lastname.Trim();

      if (string.IsNullOrWhiteSpace(normalizedFirstname) ||
          string.IsNullOrWhiteSpace(normalizedLastname))
         return false;

      return await dbContext.Authors
         .AnyAsync(
            predicate: a =>
               a.Firstname == normalizedFirstname &&
               a.Lastname == normalizedLastname,
            cancellationToken: ct
         );
   }

   public void Add(
      Author author
   ) => dbContext.Add(author);

   public void AddRange(
      IEnumerable<Author> authors
   ) => dbContext.AddRange(authors);

}