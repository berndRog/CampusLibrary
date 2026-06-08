using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Readers._3_Domain.Entities;

// Aggregate root for a library reader.
// Represents the fachlicher Nutzer of the CampusLibrary domain.
// Identity is inherited from AggregateRoot/Entity and is independent of mutable data.
public sealed class Reader : AggregateRoot {

   // Reader profile data.
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = null!;
   public AddressVo AddressVo { get; private set; } = null!;

   // Technical identity subject from the Identity Server.
   public string Subject { get; private set; } = string.Empty;

   private Reader() {
      // Required by EF Core.
   }

   private Reader(
      Guid id,
      string firstname,
      string lastname,
      EmailVo emailVo,
      AddressVo addressVo,
      string subject
   ) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      EmailVo = emailVo;
      AddressVo = addressVo;
      Subject = subject;
   }

   // Factory method for creating a valid Reader aggregate.
   // Expected validation errors are returned as Result failures.
   public static Result<Reader> Create(
      Guid id,
      string firstname,
      string lastname,
      EmailVo emailVo,
      AddressVo addressVo,
      string subject,
      DateTime createdAt
   ) {
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      subject = subject.Trim();

      if (id == Guid.Empty)
         return Result<Reader>.Failure(ReaderErrors.IdRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(ReaderErrors.SubjectRequired);

      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Reader>.Failure(ReaderErrors.FirstnameIsRequired);

      if (firstname.Length is < 2 or > 80)
         return Result<Reader>.Failure(ReaderErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Reader>.Failure(ReaderErrors.LastnameIsRequired);

      if (lastname.Length is < 2 or > 80)
         return Result<Reader>.Failure(ReaderErrors.InvalidLastname);

      var reader = new Reader(
         id: id,
         firstname: firstname,
         lastname: lastname,
         emailVo: emailVo,
         addressVo: addressVo,
         subject: subject
      );

      var initResult = reader.Initialize(createdAt);
      if (initResult.IsFailure)
         return Result<Reader>.Failure(initResult.Error);

      return Result<Reader>.Success(reader);
   }
}

/*
Didaktik
--------

Reader ist das erste Aggregate Root der CampusLibrary-Domäne.

Das Aggregate beschreibt den fachlichen Bibliotheksnutzer. Es ist
nicht identisch mit dem technischen Benutzerkonto im Identity Server.
Der technische Bezug wird über Subject hergestellt.

Die Factory-Methode Create(...) stellt sicher, dass ein Reader nur in
einem gültigen Zustand erzeugt wird. Erwartbare Regelverletzungen
werden als Result zurückgegeben und nicht als Exceptions geworfen.

Die Value Objects EmailVo und AddressVo kapseln eigene Validierungs-
und Normalisierungsregeln. Dadurch bleibt Reader auf seine fachliche
Hauptverantwortung konzentriert.

Lernziele
---------

- Aggregate Root als Einstiegspunkt eines Konsistenzbereichs verstehen
- Fachlichen Reader vom technischen Benutzerkonto unterscheiden
- Factory-Methode zur Erzeugung gültiger Domain-Objekte einsetzen
- Value Objects zur Kapselung fachlicher Werte verwenden
- Result als Fehlerstrategie in der Domain anwenden
*/
