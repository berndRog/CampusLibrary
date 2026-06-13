using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class ConfigBook(
   UtcDateTimeConverter utcDtConv
) : IEntityTypeConfiguration<Book> {
   public void Configure(EntityTypeBuilder<Book> builder) {

      // Table
      builder.ToTable("Books");

      // Primary key
      builder.HasKey(b => b.Id);

      builder.Property(b => b.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);

      // Book -> BookItem [1:n]
      // A BookItem is a physical copy of a Book.
      builder.HasMany(b => b.BookItems)
         .WithOne()
         .HasForeignKey(bi => bi.BookId)
         .OnDelete(DeleteBehavior.Cascade);

      builder.Navigation(b => b.BookItems)
         .UsePropertyAccessMode(PropertyAccessMode.Field);

      // Book <-> Author [m:n]
      // There is no BookAuthor domain class.
      // EF Core creates and manages the join table "BookAuthors".
      builder.HasMany(b => b.Authors)
         .WithMany()
         .UsingEntity<Dictionary<string, object>>(
            "BookAuthors",

            // Author side of the join table
            right => right
               .HasOne<Author>()
               .WithMany()
               .HasForeignKey("AuthorId")
               .OnDelete(DeleteBehavior.Restrict),

            // Book side of the join table
            left => left
               .HasOne<Book>()
               .WithMany()
               .HasForeignKey("BookId")
               .OnDelete(DeleteBehavior.Cascade),

            // Join table configuration
            join => {
               join.ToTable("BookAuthors");

               join.HasKey("BookId", "AuthorId");

               join.Property<Guid>("BookId")
                  .HasColumnName("BookId").HasColumnOrder(0)
                  .IsRequired();

               join.Property<Guid>("AuthorId")
                  .HasColumnName("AuthorId").HasColumnOrder(1)
                  .IsRequired();
            }
         );

      builder.Navigation(b => b.Authors)
         .UsePropertyAccessMode(PropertyAccessMode.Field);

      // Properties
      builder.Property(b => b.Title)
         .HasMaxLength(200)
         .HasColumnName("Title").HasColumnOrder(1)
         .IsRequired();

      builder.Property(b => b.Subtitle)
         .HasMaxLength(200)
         .HasColumnName("Subtitle").HasColumnOrder(2)
         .IsRequired(false);

      // ISBN value object
      builder.Property(b => b.IsbnVo)
         .HasConversion(
            isbnVo => isbnVo.Value,
            value => IsbnVo.FromPersisted(value)
         )
         .HasMaxLength(13)
         .HasColumnName("Isbn").HasColumnOrder(3)
         .IsRequired();

      builder.HasIndex(b => b.IsbnVo)
         .IsUnique();

      // Audit timestamps
      builder.Property(b => b.CreatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("CreatedAt").HasColumnOrder(4)
         .IsRequired();

      builder.Property(b => b.UpdatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("UpdatedAt").HasColumnOrder(5)
         .IsRequired();
   }
}