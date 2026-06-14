using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

// Infrastructure join type for the many-to-many table BookAuthors.
// This is not a domain entity. It has no surrogate Id.
// Its primary key is the composite key BookId + AuthorId.
internal sealed class BookAuthorJoin {

   public Guid BookId { get; private set; }
   public Guid AuthorId { get; private set; }

   public Book Book { get; private set; } = null!;
   public Author Author { get; private set; } = null!;

   // Required by EF Core.
   private BookAuthorJoin() {
   }
}