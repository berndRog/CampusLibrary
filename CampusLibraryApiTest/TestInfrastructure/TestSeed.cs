using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApi._4_Infrastructure.Persistence;

namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class TestSeed {
   private DateTime _utcNow;
   private readonly IClock _clock;
   private Seed _seed;

   public IClock Clock => _clock;

   public TestSeed() {
      _utcNow = DateTime.Parse("2025-01-01T00:00:00Z").ToUniversalTime();
      _clock = new FakeClock(_utcNow);
      _seed = new Seed(_clock);
   }

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1Vo => _seed.Address1Vo;
   public AddressVo Address2Vo => _seed.Address2Vo;
   public AddressVo Address3Vo => _seed.Address3Vo;
   public AddressVo Address4Vo => _seed.Address4Vo;
   public AddressVo Address5Vo => _seed.Address5Vo;
   public AddressVo Address6Vo => _seed.Address6Vo;
   public AddressVo AddressRegVo => _seed.AddressRegVo;
   #endregion

   #region -------------- Test Readers (Entities) -------------------------------------------
   public string Reader1Id => Seed.Reader1Id;
   public string Reader2Id => Seed.Reader2Id;
   public string Reader3Id => Seed.Reader3Id;
   public string Reader4Id => Seed.Reader4Id;
   public string Reader5Id => Seed.Reader5Id;
   public string Reader6Id => Seed.Reader6Id;
   public string ReaderRegisterId => Seed.ReaderRegisterId;

   public Reader Reader1() => _seed.Reader1();
   public Reader Reader2() => _seed.Reader2();
   public Reader Reader3() => _seed.Reader3();
   public Reader Reader4() => _seed.Reader4();
   public Reader Reader5() => _seed.Reader5();
   public Reader Reader6() => _seed.Reader6();

   public Reader ReaderRegister() => _seed.ReaderRegister();


   public IReadOnlyList<Reader> Readers => _seed.Readers;
   #endregion

   #region -------------- Test Books (Entities) ------------------------------------------
   public string Book1Id => Seed.Book1Id;
   public string Book2Id => Seed.Book2Id;
   public string Book3Id => Seed.Book3Id;
   public string Book4Id => Seed.Book4Id;

   public Book Book1() => _seed.Book1();
   public Book Book2() => _seed.Book2();
   public Book Book3() => _seed.Book3();
   public Book Book4() => _seed.Book4();

   public IReadOnlyList<Book> Books  => _seed.Books;
   #endregion

   #region -------------- Test BookItems (Entities) -----------------------------------------
   public string BookItem1Id => Seed.BookItem1Id;
   public string BookItem2Id => Seed.BookItem2Id;
   public string BookItem3Id => Seed.BookItem3Id;
   public string BookItem4Id => Seed.BookItem4Id;
   public string BookItem5Id => Seed.BookItem5Id;
   public string BookItem6Id => Seed.BookItem6Id;
   #endregion

   #region -------------- Test Loans (Aggregates) ------------------------------------------
   public string Loan1Id => Seed.Loan1Id;
   public string Loan2Id => Seed.Loan2Id;
   public string Loan3Id => Seed.Loan3Id;

   public Loan Loan1() => _seed.Loan1();
   public Loan Loan2() => _seed.Loan2();
   public Loan Loan3() => _seed.Loan3();

   public IReadOnlyList<Loan> Loans  => _seed.Loans;
   #endregion
}
