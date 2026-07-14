using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;

namespace CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;

// Contract used by the Loans module to ask for loan-relevant reader data.
// The implementation is provided by Infrastructure using the Readers module.
public interface IReaderLoanContract {

   // Finds an active reader for a loan operation.
   // Returns failure if the reader does not exist or is not active.
   Task<Result<ReaderLoanInfoDto>> FindReaderForLoanAsync(
      Guid readerId,
      CancellationToken ct
   );

   // Finds reader data for displaying an already existing loan.
   // Existing loans remain readable even if the Reader was deactivated later.
   Task<Result<ReaderLoanInfoDto>> FindReaderForExistingLoanAsync(
      Guid readerId,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface ist ein modulübergreifender Contract.

Das Loans-Modul benötigt für eine Ausleihe Informationen über einen Reader.
Es soll aber nicht direkt auf das Readers-Modul, die Readers-Tabelle oder das
Reader-Aggregate zugreifen.

Der Contract liegt deshalb in BuildingBlocks. Dadurch können sowohl das
Loans-Modul als auch die Infrastructure-Implementierung auf dieselbe
Schnittstelle zugreifen, ohne dass Loans eine direkte Abhängigkeit auf
Readers bekommt.

Die Implementierung liegt in Infrastructure und darf dort den Readers-
DbContext-Port verwenden. Fachlich bleibt trotzdem klar:
Readers besitzt Reader-Daten.
Loans verwendet nur die freigegebene Auskunft.
*/