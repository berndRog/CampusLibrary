namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

// Reader self-service request.
// The ReaderId is derived by the API from the authenticated token subject.
public sealed record LoanBorrowMeDto(
   Guid BookItemId,
   string? Id = null
);

/*
Didaktik
--------

Ein Reader darf bei einer Self-Service-Ausleihe keine ReaderId mitsenden.
Andernfalls könnte ein Client versuchen, eine Ausleihe für einen anderen
Reader anzulegen.

POST /loans/me akzeptiert deshalb nur die BookItemId. Die API bestimmt den
fachlichen Reader über GET /readers/me beziehungsweise IReaderReadModel.FindMeAsync.
*/
