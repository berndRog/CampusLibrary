using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

public sealed class Author : AggregateRoot {
   
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;

   private Author() {
      // Required by EF Core.
   }

   private Author(
      Guid id, 
      string firstname, 
      string lastname
   ) {
      Firstname = firstname;
      Lastname = lastname;
   }

   public static Result<Author> Create(
      Guid id,
      string firstname,
      string lastname
   ) {
      if (string.IsNullOrWhiteSpace(firstname) &&
          string.IsNullOrWhiteSpace(lastname))
         return Result<Author>.Failure(CatalogErrors.AuthorNameIsRequired);

      var author = new Author(
         id,
         firstname.Trim(),
         lastname.Trim()
      );

      return Result<Author>.Success(author);
   }

   public string DisplayName =>
      $"{Firstname} {Lastname}".Trim();
}