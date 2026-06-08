using AwesomeAssertions;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApiTest._1_DomainTests.ValueObjects;

public sealed class EmailVoUt {
   public static IEnumerable<object[]> InvalidLengths() {
      yield return ["A"]; // too short (1)
      yield return [new string('A', 300) + "@xyz.com"];
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("micky@mouse")]
   [InlineData("@")]
   [InlineData("@.com")]
   [InlineData("abc.yzx@.com")]
   [InlineData("a.b.de")]
   public void Invalid_email_fails(string email) {
      // Act
      var result = EmailVo.Create(email);
      // Assert
      result.IsFailure.Should().BeTrue();
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }
}