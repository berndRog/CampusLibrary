using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.Entities;

public sealed class Reader : AggregateRoot {

   //--- Properties ------------------------------------------------------------
// public Guid Id { get; private set; }
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = null!;
   public AddressVo AddressVo { get; private set; } = null!;
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

   public static Result<Reader> Create(
      Guid id,
      string firstname,
      string lastname,
      EmailVo emailVo,
      AddressVo addressVo,
      string subject,
      DateTime createdAt = default!
   ) {
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      subject = subject.Trim();
      
      if (id == Guid.Empty)
         return Result<Reader>.Failure(ReaderErrors.IdRequired);

      if (string.IsNullOrWhiteSpace(subject))
         return Result<Reader>.Failure(ReaderErrors.SubjectRequired);

      // Validate basic fields
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
         subject: subject.Trim()
      );
   
      reader.Initialize(createdAt);
 
      return Result<Reader>.Success(reader);
   }
}
