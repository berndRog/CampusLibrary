namespace CampusLibraryApi._3_Core.Loans._3_Domain.Policies;

// Central domain rules for loans.
public static class LoanRules {

   // Standard number of days for a newly created loan.
   public const int StandardLoanDays = 28;

   // Standard number of days added when a loan is renewed.
   public const int StandardRenewalDays = 14;

   // Maximum number of renewals for one active loan.
   public const int MaxRenewals = 3;
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse sammelt einfache fachliche Regeln des Loans-Moduls.

Die Werte gehören nicht in Controller, DTOs oder Tests, weil sie keine
Eingabedaten des Clients sind. Sie beschreiben fachliche Vorgaben der
Bibliothek.

StandardLoanDays legt fest, wie lange eine neue Ausleihe standardmäßig läuft.

StandardRenewalDays legt fest, um wie viele Tage eine Ausleihe bei einer
Verlängerung erweitert wird.

MaxRenewals begrenzt, wie oft eine aktive Ausleihe verlängert werden darf.

Für den Anfang reicht eine statische Regelklasse. Später könnte daraus eine
konfigurierbare Policy werden, wenn unterschiedliche Medienarten,
Reader-Gruppen oder Ausleihbedingungen eingeführt werden.
*/