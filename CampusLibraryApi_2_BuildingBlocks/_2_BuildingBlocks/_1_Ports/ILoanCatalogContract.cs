using CampusLibraryApi._2_BuildingBlocks._2_Application.Dtos;

namespace CampusLibraryApi._2_BuildingBlocks._1_Ports;

// Contract provided by the Loans module for the Catalog module.
// It exposes only whether current loans exist for catalog book items.
public interface ILoanCatalogContract {

   Task<bool> ExistsForBookItemsAsync(
      IReadOnlyCollection<Guid> bookItemIds,
      CancellationToken ct
   );

   Task<Result<IReadOnlyList<CurrentBookItemLoanInfoDto>>> FindCurrentLoansForBookItemsAsync(
      IReadOnlyCollection<Guid> bookItemIds,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Catalog owns Books and BookItems, while Loans owns current borrowing
processes. Before Catalog deletes BookItems during book deactivation, it must
ask Loans whether one of these items is still borrowed.

The contract exposes both the required yes/no information for the command side
and a small read projection for the deactivation page. Catalog receives neither
Loan entities nor direct access to the Loans or Readers tables.
*/
