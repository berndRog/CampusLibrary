using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Identity;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;

namespace CampusLibraryApiTest._2_ApplicationTests.Identity;

public sealed class IdentitySubjectUt {

   [Fact]
   public void Check_valid_reader_identity_returns_subject() {
      var gateway = new FakeIdentityGateway {
         Subject = "reader-subject-001",
         Username = "reader@example.org",
         CreatedAt = FakeClock.DefaultUtcNow,
         IsAuthenticated = true,
         IsReader = true,
         IsEmployee = false
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().Be(gateway.Subject);
   }

   [Fact]
   public void Check_unauthenticated_identity_returns_unauthorized() {
      var gateway = new FakeIdentityGateway {
         IsAuthenticated = false
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.IdentityUnauthenticated);
   }

   [Fact]
   public void Check_employee_identity_returns_forbidden() {
      var gateway = new FakeIdentityGateway {
         IsAuthenticated = true,
         IsReader = false,
         IsEmployee = true
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.AccessNotAllowed);
   }

   [Fact]
   public void Check_missing_subject_returns_bad_request() {
      var gateway = new FakeIdentityGateway {
         Subject = string.Empty
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.SubjectRequired);
   }

   [Fact]
   public void Check_missing_username_returns_bad_request() {
      var gateway = new FakeIdentityGateway {
         Username = string.Empty
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.IdentityEmailRequired);
   }

   [Fact]
   public void Check_missing_created_at_returns_bad_request() {
      var gateway = new FakeIdentityGateway {
         CreatedAt = default
      };

      Result<string> result = IdentitySubject.Check(gateway);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.TimestampInvalid);
   }
}
