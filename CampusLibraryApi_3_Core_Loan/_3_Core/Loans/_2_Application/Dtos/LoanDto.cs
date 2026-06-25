using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;

namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

public sealed record LoanDto(
   Guid Id,
   DateTime LoanDate,
   DateTime DueDate,
   Guid ReaderId,
   Guid BookItemId,
   DateTime? ReturnedAt,
   int Status,
   int RenewalCount
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO beschreibt eine Ausleihe für die Außenwelt des Loans-Moduls.

Das Domain-Objekt Loan verwendet intern ein Value Object LoanPeriodVo.
Dieses Value Object schützt die fachlichen Regeln des Leihzeitraums.

Nach außen wird der Leihzeitraum aber bewusst flach dargestellt:
LoanDate zeigt den Beginn der Ausleihe.
DueDate zeigt das geplante Rückgabedatum.
ReturnedAt zeigt den tatsächlichen Rückgabezeitpunkt, falls das Exemplar
bereits zurückgegeben wurde.

Dadurch bleibt die API einfach verständlich, ohne die interne Struktur des
Domänenmodells nach außen zu leaken.

ReaderId und BookItemId sind Referenzen auf andere Module. Das Loans-Modul
besitzt weder Reader noch BookItem, sondern speichert nur die fachlich
notwendigen IDs.

Status und RenewalCount zeigen den aktuellen Zustand der Ausleihe:
Eine Ausleihe kann aktiv oder zurückgegeben sein, und sie kann nur begrenzt
oft verlängert werden.
*/