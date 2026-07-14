using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
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
   // public DateTime CreatedAt { get; private set; }
   // public DateTime UpdatedAt { get; private set; }

   // Reader profile data.
   // A provisioned reader starts with an incomplete profile.
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = null!;
   public AddressVo? AddressVo { get; private set; } = null!;

   // Is reader active or deactivated?
   public bool IsActive { get; private set; } = true;

   // Technical identity subject from the IdentityAccessServer.
   public string Subject { get; private set; } = string.Empty;

   // A reader may exist before the domain profile is completed.
   public bool IsProfileCompleted =>
      !string.IsNullOrWhiteSpace(Firstname) &&
      !string.IsNullOrWhiteSpace(Lastname) &&
      AddressVo is not null;
   
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
      AddressVo? addressVo,
      string subject
   ) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      EmailVo = emailVo;
      AddressVo = addressVo;
      Subject = subject;
   }

   //--- static factory for classic administrative creation --------------------
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

      // Validate required input fields
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
         return Result<Reader>.Failure(ReaderErrors.AddressIsRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(CommonErrors.SubjectRequired);
      
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

   //--- static factory for Part 6 provisioning --------------------------------
   // Creates the fachlicher Reader shell from trusted token claims.
   // The fachliches Profil is completed afterwards by UpdateMyProfile(...).
   public static Result<Reader> Provision(
      Guid id,
      string subject,
      EmailVo emailVo,
      DateTime createdAt
   ) {
      subject = subject.Trim();

      if (id == Guid.Empty)
         return Result<Reader>.Failure(ReaderErrors.IdRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(CommonErrors.SubjectRequired);

      if (emailVo is null)
         return Result<Reader>.Failure(ReaderErrors.InvalidEmail);

      var reader = new Reader(
         id: id,
         firstname: string.Empty,
         lastname: string.Empty,
         emailVo: emailVo,
         addressVo: null,
         subject: subject
      );

      var initResult = reader.Initialize(createdAt);
      if (initResult.IsFailure)
         return Result<Reader>.Failure(initResult.Error);

      return Result<Reader>.Success(reader);
   }

   //--- domain methods --------------------------------------------------------
   // Partially updates mutable reader profile data in the later self-service flow.
   // Null means: keep the current value.
   // Firstname and Subject are intentionally not changed here.
   public Result UpdateProfile(
      string? lastname,
      EmailVo? emailVo,
      AddressVo? addressVo,
      DateTime updatedAt
   ) {
      var hasChange = lastname is not null || emailVo is not null || addressVo is not null;
      if (!hasChange)
         return Result.Success();

      string? normalizedLastname = null;
      if (lastname is not null) {
         normalizedLastname = lastname.Trim();

         if (string.IsNullOrWhiteSpace(normalizedLastname))
            return Result.Failure(ReaderErrors.LastnameIsRequired);

         if (normalizedLastname.Length is < 2 or > 80)
            return Result.Failure(ReaderErrors.InvalidLastname);
      }

      var touchResult = Touch(updatedAt);
      if (touchResult.IsFailure)
         return Result.Failure(touchResult.Error);

      // Apply changes only after all validations have succeeded.
      if (normalizedLastname is not null) Lastname = normalizedLastname;
      if (emailVo is not null) EmailVo = emailVo;
      if (addressVo is not null) AddressVo = addressVo;

      return Result.Success();
   }

   // Completes or changes the self-service profile data entered by the reader.
   // Subject and email remain bound to the IdentityAccessServer token.
   public Result UpdateMyProfile(
      string firstname,
      string lastname,
      AddressVo addressVo,
      DateTime updatedAt
   ) {
      firstname = firstname.Trim();
      lastname = lastname.Trim();

      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(ReaderErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result.Failure(ReaderErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(ReaderErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result.Failure(ReaderErrors.InvalidLastname);
      
      if(addressVo is null)
         return Result.Failure(ReaderErrors.AddressIsRequired);
      
      var touchResult = Touch(updatedAt);
      if (touchResult.IsFailure)
         return Result.Failure(touchResult.Error);

      Firstname = firstname;
      Lastname = lastname;
      AddressVo = addressVo;

      return Result.Success();
   }
   
   public Result Deactivate(
      DateTime updatedAt
   ) {
      if (!IsActive)
         return Result.Failure(ReaderErrors.IsAlreadyDeactivated);

      IsActive = false;
      Touch(updatedAt: updatedAt);
      
      return Result.Success();
   }
}

/*
Didaktik
--------

Reader ist das fachliche Aggregate Root für Bibliotheksnutzer.

Part 6 ergänzt eine wichtige Unterscheidung:

- Ein technischer Benutzer entsteht im IdentityAccessServer.
- Ein fachlicher Reader entsteht in der CampusLibraryApi.
- Beide werden über Subject verbunden.

Create(...) bleibt der klassische Erzeugungsweg für vollständig bekannte
Reader-Stammdaten. Provision(...) ist der neue Part-6-Erzeugungsweg:
Subject und Email kommen aus dem Token, das fachliche Profil ist zunächst
unvollständig.

UpdateMyProfile(...) ergänzt danach Vorname und Nachname. Subject und Email
werden dabei nicht aus einem Formular übernommen und nicht verändert.

IsProfileCompleted wird berechnet. Ein provisionierter Reader darf bereits
existieren, aber fachliche Aktionen wie Ausleihen können zusätzlich verlangen,
dass das Profil vollständig ist.

Lernziele
---------

- technischen Benutzer und fachliches Aggregate unterscheiden
- Provisioning als idempotenten Übergang modellieren
- vertrauenswürdige Token-Daten von UI-Profildaten trennen
- unvollständige fachliche Profile explizit modellieren
- Aggregate-Methoden statt öffentliche Setter verwenden
*/
