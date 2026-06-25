using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

internal sealed class ConfigLoan(
   UtcDateTimeConverter utcDtConv,
   NullableUtcDateTimeConverter nullUtcDtConv
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

      // Actual return timestamp
      builder.Property(l => l.ReturnedAt)
         .HasConversion(nullUtcDtConv)
         .HasColumnName("ReturnedAt").HasColumnOrder(5)
         .IsRequired(false);

      // Loan state
      builder.Property(l => l.Status)
         .HasConversion<int>()
         .HasColumnName("Status").HasColumnOrder(6)
         .IsRequired();

      builder.Property(l => l.RenewalCount)
         .HasColumnName("RenewalCount").HasColumnOrder(7)
         .IsRequired();

      // Audit timestamps
      builder.Property(l => l.CreatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("CreatedAt").HasColumnOrder(8)
         .IsRequired();

      builder.Property(l => l.UpdatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("UpdatedAt").HasColumnOrder(9)
         .IsRequired();

      // Indexes for common loan queries
      builder.HasIndex(l => l.ReaderId);

      builder.HasIndex(l => l.BookItemId);

      builder.HasIndex(l => l.Status);

      builder.HasIndex(l => new {
         l.BookItemId,
         l.Status
      });
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Konfiguration beschreibt, wie das Loan-Aggregate mit EF Core
persistiert wird.

Loan gehört zum Loans-Modul. Deshalb wird hier nur die Tabelle Loans
konfiguriert. Reader und BookItem werden nicht als Navigation Properties
modelliert, sondern nur über ReaderId und BookItemId referenziert.

Das ist didaktisch wichtig:
Loans besitzt keine Reader und keine BookItems. Loans speichert nur die IDs
der fachlich beteiligten Objekte aus anderen Modulen.

Der Leihzeitraum wird im Domänenmodell als Value Object LoanPeriodVo
modelliert. In der Datenbank werden die Werte dieses Value Objects als
normale Spalten LoanDate und DueDate in der Loans-Tabelle gespeichert.

LoanDate und DueDate beschreiben den geplanten Leihzeitraum.
ReturnedAt beschreibt dagegen den tatsächlichen Rückgabezeitpunkt und ist
deshalb nullable. Solange eine Ausleihe aktiv ist, ist ReturnedAt null.

Der Status wird als int gespeichert. Dadurch bleibt die Datenbank einfach
lesbar, ohne den Namen des Domain-Enums als Text persistieren zu müssen.

Die Indizes unterstützen typische Abfragen:
- aktive Ausleihen eines Readers finden
- aktive Ausleihe eines konkreten BookItems finden
- Ausleihen nach Status filtern

Die fachliche Regel, dass ein BookItem nicht gleichzeitig mehrfach aktiv
ausgeliehen werden darf, wird später im Borrow-Use-Case geprüft. Die
Datenbankindizes unterstützen diese Prüfung, ersetzen aber nicht die
Fachlogik im Anwendungskern.
*/