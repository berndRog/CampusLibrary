using CampusLibraryApi._2_Shared._1_Ports;
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
   #endregion

   #region -------------- Test Readers (Enities) -------------------------------------------
   public Reader Reader1() => _seed.Reader1();
   public Reader Reader2() => _seed.Reader2();
   public Reader Reader3() => _seed.Reader3();
   public Reader Reader4() => _seed.Reader4();
   public Reader Reader5() => _seed.Reader5();
   public Reader Reader6() => _seed.Reader6();

   public Reader ReaderRegister() => _seed.ReaderRegister();

   public IReadOnlyList<Reader> Readers => [
      Reader1(), Reader2(), Reader3(), Reader4(), Reader5(), Reader6()
   ];
   #endregion
/*
   #region -------------- Test Iban (Value Objects) ------------------------------------------
   public string Iban1 => Seed.Iban1;
   public string Iban2 => Seed.Iban2;
   public string Iban3 => Seed.Iban3;
   public string Iban4 => Seed.Iban4;
   public string Iban5 => Seed.Iban5;
   public string Iban6 => Seed.Iban6;
   public string Iban7 => Seed.Iban7;
   public string Iban8 => Seed.Iban8;
   #endregion

   #region -------------- Test Accounts ------------------------------------------------------
   public Account Account1() => _seed.Account1();
   public Account Account2() => _seed.Account2();
   public Account Account3() => _seed.Account3();
   public Account Account4() => _seed.Account4();
   public Account Account5() => _seed.Account5();
   public Account Account6() => _seed.Account6();
   public Account Account7() => _seed.Account7();
   public Account Account8() => _seed.Account8();

   public IReadOnlyList<Account> Accounts => new List<Account> {
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8()
   };
   #endregion

   #region -------------- Test Beneficiaries -------------------------------------------------
   public Beneficiary Beneficiary1() => _seed.Beneficiary1();
   public Beneficiary Beneficiary2() => _seed.Beneficiary2();
   public Beneficiary Beneficiary3() => _seed.Beneficiary3();
   public Beneficiary Beneficiary4() => _seed.Beneficiary4();
   public Beneficiary Beneficiary5() => _seed.Beneficiary5();
   public Beneficiary Beneficiary6() => _seed.Beneficiary6();
   public Beneficiary Beneficiary7() => _seed.Beneficiary7();
   public Beneficiary Beneficiary8() => _seed.Beneficiary8();
   public Beneficiary Beneficiary9() => _seed.Beneficiary9();
   public Beneficiary Beneficiary10() => _seed.Beneficiary10();
   public Beneficiary Beneficiary11() => _seed.Beneficiary11();
   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>{
      Beneficiary1(), Beneficiary2(), Beneficiary3(), Beneficiary4(),
      Beneficiary5(), Beneficiary6(), Beneficiary7(), Beneficiary8(),
      Beneficiary9(), Beneficiary10(), Beneficiary11()
   };
   #endregion

   #region -------------- Test Transactions ---------------------------------------------------
   public string Transaction1dId => Seed.Transaction1DId;
   public string Transaction1cId => Seed.Transaction1CId;
   public string Transaction2dId => Seed.Transaction2DId;
   public string Transaction2cId => Seed.Transaction2CId;
   public string Transaction3dId => Seed.Transaction3DId;
   public string Transaction3cId => Seed.Transaction3CId;
   public string Transaction4dId => Seed.Transaction4DId;
   public string Transaction4cId => Seed.Transaction4CId;
   public string Transaction5dId => Seed.Transaction5DId;
   public string Transaction5cId => Seed.Transaction5CId;
   public string Transaction6dId => Seed.Transaction6DId;
   public string Transaction6cId => Seed.Transaction6CId;
   public string Transaction7dId => Seed.Transaction7DId;
   public string Transaction7cId => Seed.Transaction7CId;
   public string Transaction8dId => Seed.Transaction8DId;
   public string Transaction8cId => Seed.Transaction8CId;
   public string Transaction9dId => Seed.Transaction9DId;
   public string Transaction9cId => Seed.Transaction9CId;
   public string Transaction10dId => Seed.Transaction10DId;
   public string Transaction10cId => Seed.Transaction10CId;
   public string Transaction11dId => Seed.Transaction11DId;
   public string Transaction11cId => Seed.Transaction11CId;

   public Transaction Transaction1d() => _seed.Transaction1D();
   public Transaction Transaction1c() => _seed.Transaction1C();
   public Transaction Transaction2d() => _seed.Transaction2D();
   public Transaction Transaction2c() => _seed.Transaction2C();
   public IReadOnlyList<Transaction> Transaction => _seed.Transactions;
   #endregion

   #region -------------- Test Transfers -----------------------------------------------------
   public string Transfer1Id => Seed.Transfer1Id;
   public string Transfer2Id => Seed.Transfer2Id;
   public string Transfer3Id => Seed.Transfer3Id;
   public string Transfer4Id => Seed.Transfer4Id;
   public string Transfer5Id => Seed.Transfer5Id;
   public string Transfer6Id => Seed.Transfer6Id;
   public string Transfer7Id => Seed.Transfer7Id;
   public string Transfer8Id => Seed.Transfer8Id;
   public string Transfer9Id => Seed.Transfer9Id;
   public string Transfer10Id => Seed.Transfer10Id;
   public string Transfer11Id => Seed.Transfer11Id;

   public Transfer Transfer1() => _seed.Transfer1();
   public Transfer Transfer2() => _seed.Transfer2();

   public IReadOnlyList<Transfer> Transfers => _seed.Transfers;
   #endregion

   public List<Account> AddBeneficiariesToAccounts(List<Account> accounts)
      => _seed.AddBeneficiariesToAccounts(accounts);

   public (List<Account>, List<Transfer>) AddBeneficiariesAndTransactionsAndTransfersToAccounts(
      List<Account> accounts,
      List<Transfer> transfers
   ) => _seed.AddBeneficiariesAndTransactionsAndTransfersToAccounts(accounts, transfers);
   
   */
}