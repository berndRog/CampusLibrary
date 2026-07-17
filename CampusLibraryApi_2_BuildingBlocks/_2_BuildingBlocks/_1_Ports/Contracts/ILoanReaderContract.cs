namespace CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;

// Contract provided by the Loans module for the Readers module.
// It exposes only whether current loans exist for one reader.
public interface ILoanReaderContract {

   Task<bool> ExistsForReaderAsync(
      Guid readerId,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Readers besitzt das Reader-Aggregate. Loans besitzt die aktuellen
Ausleihvorgänge.

Bevor Readers einen Reader deaktiviert, muss das Modul bei Loans nachfragen,
ob für diesen Reader noch aktuelle Ausleihen existieren. Das Readers-Modul
darf nicht direkt auf die Loans-Tabelle zugreifen.

Der Contract gibt deshalb nur die Information zurück, die der Command
benötigt: Existiert mindestens eine aktuelle Ausleihe für diesen Reader?

Der Contract liefert bewusst weder ein Loan-Aggregate noch ein
Persistenzmodell. Dadurch bleibt die Modulgrenze sichtbar:

- Readers entscheidet, ob ein Reader deaktiviert werden darf.
- Loans beantwortet, ob aktuelle Ausleihen existieren.
- Infrastructure implementiert die technische Datenbankabfrage.
*/
