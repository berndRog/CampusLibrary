using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Readers._3_Domain.Entities;

// Aggregate root for a library reader.
// Represents the fachlicher Nutzer of the CampusLibrary domain.
// Identity is inherited from AggregateRoot/Entity and is independent of mutable data.
public sealed class Reader : AggregateRoot {

   //--- properties ------------------------------------------------------------
   // inherited from Entity + Aggregate root base class
   // public Guid Id { get; private set; } 
   // public DateTimeOffset CreatedAt { get; private set; }
   // public DateTimeOffset UpdatedAt { get; private set; }

   // Reader profile data.
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = null!;
   public AddressVo AddressVo { get; private set; } = null!;

   // Technical identity subject from the Identity Server.
   public string Subject { get; private set; } = string.Empty;

   //--- constructors ----------------------------------------------------------
   // EF Core ctor
   private Reader() {
      // Required by EF Core.
   }

   // Domain ctor (used by factories)
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

   // --- static factory to create a Reader object ---------------------------
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

      // Validate reuired input fields
      if (id == Guid.Empty)
         return Result<Reader>.Failure(ReaderErrors.IdRequired);
      
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Reader>.Failure(ReaderErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Reader>.Failure(ReaderErrors.InvalidFirstname);
      
      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Reader>.Failure(ReaderErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Reader>.Failure(ReaderErrors.InvalidLastname);

      if (emailVo is null)
         return Result<Reader>.Failure(ReaderErrors.InvalidEmail);

      if (addressVo is null)
         return Result<Reader>.Failure(ReaderErrors.AddressRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(ReaderErrors.SubjectRequired);
      
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

   //--- domain methods --------------------------------------------------------
   // Update mutable reader profile data.
   // The technical identity subject is intentionally not changed here.
   public Result UpdateProfile(
      string? lastname,
      EmailVo? emailVo,
      AddressVo? addressVo,
      DateTime updatedAt
   ) {
      lastname = lastname?.Trim();

      if (!string.IsNullOrWhiteSpace(lastname) && lastname.Length is < 2 or > 80)
         return Result.Failure(ReaderErrors.InvalidLastname);
      
      // Apply changes
      if (lastname is not null) Lastname = lastname;
      if (emailVo is not null) EmailVo = emailVo;
      if (addressVo is not null) AddressVo = addressVo;

      var touchResult = Touch(updatedAt);
      if (touchResult.IsFailure)
         return Result.Failure(touchResult.Error);
      
      return Result.Success();
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

UpdateProfile(...) ändert nur fachliche Profildaten. Die technische
Identität Subject bleibt unverändert, weil sie vom Identity Server kommt
und nicht Teil der normalen Profilpflege ist.

Die Value Objects EmailVo und AddressVo kapseln eigene Validierungs-
und Normalisierungsregeln. Dadurch bleibt Reader auf seine fachliche
Hauptverantwortung konzentriert.

Lernziele
---------

- Aggregate Root als Einstiegspunkt eines Konsistenzbereichs verstehen
- Fachlichen Reader vom technischen Benutzerkonto unterscheiden
- Factory-Methode zur Erzeugung gültiger Domain-Objekte einsetzen
- Änderungsmethoden am Aggregate statt direkte Setter verwenden
- Value Objects zur Kapselung fachlicher Werte verwenden
- Result als Fehlerstrategie in der Domain anwenden
*/
