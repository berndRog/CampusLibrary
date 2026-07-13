using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

internal sealed class ConfigLoan(
   UtcDateTimeConverter utcDtConv
) : IEntityTypeConfiguration<Loan> {

   public void Configure(EntityTypeBuilder<Loan> builder) {

      // tablename
      builder.ToTable("Loans");

      // primary key
      builder.HasKey(l => l.Id);
      builder.Property(l => l.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);

      // references to other modules
      builder.Property(l => l.ReaderId)
         .HasColumnName("ReaderId").HasColumnOrder(1)
         .IsRequired();

      builder.Property(l => l.BookItemId)
         .HasColumnName("BookItemId").HasColumnOrder(2)
         .IsRequired();

      // Loan period (owned value object)
      builder.OwnsOne(l => l.LoanPeriodVo, lp => {

         lp.Property(p => p.LoanDate)
            .HasConversion(utcDtConv)
            .HasColumnName("LoanDate").HasColumnOrder(3)
            .IsRequired();

         lp.Property(p => p.DueDate)
            .HasConversion(utcDtConv)
            .HasColumnName("DueDate").HasColumnOrder(4)
            .IsRequired();
      });
      builder.Navigation(l => l.LoanPeriodVo).IsRequired();

      builder.Property(l => l.RenewalCount)
         .HasColumnName("RenewalCount").HasColumnOrder(5)
         .IsRequired();

      // Audit timestamps
      builder.Property(l => l.CreatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("CreatedAt").HasColumnOrder(6)
         .IsRequired();

      builder.Property(l => l.UpdatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("UpdatedAt").HasColumnOrder(7)
         .IsRequired();

      // Only current loans are stored. Therefore BookItemId must be unique:
      // one physical copy can have at most one current loan.
      builder.HasIndex(l => l.ReaderId);

      builder.HasIndex(l => l.BookItemId)
         .IsUnique();
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Konfiguration beschreibt, wie das Loan-Aggregate mit EF Core
persistiert wird.

Nur aktuell bestehende Ausleihen werden gespeichert. Deshalb enthält die
Tabelle weder Status noch ReturnedAt. Bei der Rückgabe wird der Datensatz
gelöscht.

Der eindeutige Index auf BookItemId sichert zusätzlich zur Fachlogik ab, dass
ein physisches Exemplar nicht gleichzeitig in mehreren Loans vorkommen kann.
*/
