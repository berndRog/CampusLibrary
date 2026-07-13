namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

public sealed record LoanDto(
   Guid Id,
   DateTime LoanDate,
   DateTime DueDate,
   Guid ReaderId,
   Guid BookItemId,
   int RenewalCount
);

/*
Lernziele und Didaktik
----------------------

Dieses DTO beschreibt eine aktuell bestehende Ausleihe für die Außenwelt des
Loans-Moduls.

Das Domain-Objekt Loan verwendet intern ein Value Object LoanPeriodVo. Nach
außen wird der Leihzeitraum bewusst flach durch LoanDate und DueDate
repräsentiert.

ReaderId und BookItemId sind Referenzen auf andere Module. Das Loans-Modul
besitzt weder Reader noch BookItem, sondern speichert nur die fachlich
notwendigen IDs.

Status und ReturnedAt sind nicht erforderlich: Die Existenz eines Loan
bedeutet bereits, dass das BookItem aktuell ausgeliehen ist. Bei der Rückgabe
wird der Loan gelöscht.
*/
