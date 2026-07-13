using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;

// Facade port for Reader command use cases.
// The web layer depends on this interface instead of concrete use case classes.
// Query operations such as FindMeAsync belong to IReaderReadModel.
public interface IReaderUseCases {
   // deleted when using AuthN/AuthZ 
   // // Create a new fully populated Reader aggregate.
   // Task<Result<ReaderDto>> CreateAsync(
   //    ReaderCreateDto dto,
   //    CancellationToken ct
   // );
   //
   // // Update mutable Reader profile data through the administrative flow.
   // Task<Result<ReaderDto>> UpdateAsync(
   //    Guid id,
   //    ReaderUpdateDto dto,
   //    CancellationToken ct
   // );

   // Self-service provisioning for the current authenticated reader.
   //
   // Maps to:
   // POST /readers/me/provision
   //
   // Creates the fachlicher Reader shell for the current technical user.
   // The current user is resolved through the access token subject.
   //
   // The operation is idempotent:
   // if a Reader for the subject already exists, the existing Reader is returned.
   Task<Result<ReaderProvisionMeDto>> ProvisionMeAsync(
      string? id,
      CancellationToken ct
   );

   // Self-service profile completion for the current authenticated reader.
   //
   // Maps to:
   // PUT /readers/me/profile
   //
   // Completes the initial fachliche profile after provisioning.
   // This command uses ReaderProfileDto:
   // - Firstname
   // - Lastname
   // - AddressDto
   //
   // The DTO intentionally does not contain Subject or Email.
   // Subject comes from the access token.
   // The initial fachliche Email was already taken from the technical username
   // during provisioning.
   Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileMeDto meDto,
      CancellationToken ct
   );

   // Self-service update for the current authenticated reader.
   //
   // Maps to:
   // PUT /readers/me/update
   //
   // Updates mutable fachliche Reader data after the initial profile completion.
   // This command uses ReaderUpdateDto:
   // - Lastname optional
   // - Email optional
   // - AddressDto optional
   //
   // Firstname is intentionally not updated here.
   // The technical username in the IdentityAccessServer is not changed.
   // The current Reader is resolved through the access token subject,
   // not through a Reader id supplied by the client.
   Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateMeDto meDto,
      CancellationToken ct
   );

   // Administrative deactivation of an existing Reader aggregate.
   //
   // Maps to:
   // DELETE /readers/{id}
   //
   // This is not a Reader self-service command.
   // It is a soft delete: the Reader remains stored,
   // but is hidden from normal read model queries.
   Task<Result> DeactivateAsync(
      Guid id,
      CancellationToken ct
   );
}

/*
Didaktik
--------

IReaderUseCases ist die Fassade für die schreibende Seite des Readers-Moduls.

Mit Teil 6 verschiebt sich der fachliche Schwerpunkt von einer klassischen
CRUD-orientierten Reader-Verwaltung zu einer geschützten Self-Service-API.

Die frühere Änderung eines Readers über

   PUT /readers/{id}

wird für den Reader-Self-Service nicht mehr verwendet. Der Client übergibt
für Self-Service-Operationen keine ReaderId mehr. Stattdessen wird der aktuelle
fachliche Reader über das Subject aus dem Access Token ermittelt.

Die Self-Service-Operationen orientieren sich direkt an den /me-Routen:

- ProvisionMeAsync       -> POST /readers/me/provision
- UpdateMeProfileAsync   -> PUT  /readers/me/profile
- UpdateMeAsync          -> PUT  /readers/me/update

ProvisionMeAsync erzeugt aus dem technischen Benutzerkonto im
IdentityAccessServer einen fachlichen Reader in der CampusLibrary. Diese
Operation ist idempotent: Wenn zum Subject bereits ein Reader existiert, wird
dieser zurückgegeben und kein zweiter Reader angelegt.

UpdateMeProfileAsync vervollständigt das initiale fachliche Profil nach der
Provisionierung. Das Formular liefert Vorname, Nachname und Adresse. Subject
und Email werden dabei nicht aus dem Formular übernommen. Das Subject kommt aus
dem Access Token. Die initiale fachliche Email wurde bereits beim Provisioning
aus dem technischen Username übernommen.

UpdateMeAsync ist die spätere Self-Service-Änderung. Hier können Nachname,
fachliche Reader-Email und Adresse geändert werden. Der Vorname bleibt bewusst
unveränderbar. Auch der technische Username im IdentityAccessServer wird nicht
geändert.

DeactivateAsync bleibt ein administrativer Command. Er gehört nicht zum
Reader-Self-Service und sollte durch eine passende Employee- oder Verwaltungs-
Policy geschützt werden.

FindMeAsync gehört nicht zu dieser UseCase-Fassade, sondern zum ReadModel,
weil es eine reine Leseoperation ist.

Lernziele
---------

- technische Identität und fachlichen Reader unterscheiden
- Provisioning als Übergang von Login-Identität zu Fachobjekt verstehen
- Self-Service-Endpunkte über /me statt über Route-IDs modellieren
- Subject aus dem Access Token als stabilen technischen Schlüssel verwenden
- initialen Profilabschluss und spätere Profiländerung trennen
- technische Email/Username und fachliche Reader-Email unterscheiden
- Command-UseCases und ReadModel-Abfragen sauber trennen
- Controller-Autorisierung und fachliche UseCase-Regeln unterscheiden
- Controller-Abhängigkeiten über eine UseCase-Fassade klein halten
*/
