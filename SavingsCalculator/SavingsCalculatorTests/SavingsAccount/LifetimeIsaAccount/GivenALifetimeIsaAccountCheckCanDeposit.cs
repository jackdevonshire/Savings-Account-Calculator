using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.LifetimeIsaAccount
{
    [Parallelizable]
    public class GivenALifetimeIsaAccountCheckCanDeposit
    {
        private Account _subject;

        [SetUp]
        public void Setup()
        {
            //Account opens with 4k for tax year 2023-24, can't deposit more for this tax year
            _subject = new Account(
                "Lifetime ISA",
                new DateOnly(2023, 04, 06),
                4000,
                5
            );

            _subject
                // In 2024-25, 4k deposited, then 200 withdrawn. Can deposit up to 200, but no more for this tax year
                .Deposit(new DateOnly(2024, 04, 06), 4000)
                .Withdraw(new DateOnly(2024, 04, 08), 200)
                // In 2025-26, 200 deposited, none withdrawn. Can deposit up to 3800, but no more for this tax year
                .Deposit(new DateOnly(2025, 04, 06), 200);
        }

        [Test]
        public void ThenCannotDepositAnythingInTaxYearBeginning2023()
        {
            Assert.That(_subject.CanDeposit(new DateOnly(2023, 04, 06), 1), Is.False);
            Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 05), 1), Is.False);
        }
        
        [Test]
        public void ThenCanOnlyDepositUpTo200InTaxYearBeginning2024()
        {
            Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 1), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 200), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2024, 04, 06), 201), Is.False);
            
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 1), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 200), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 05), 201), Is.False);
        }
        
        [Test]
        public void ThenCanOnlyDepositUpTo3800InTaxYearBeginning2025()
        {
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 06), 1), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 06), 3800), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2025, 04, 06), 3801), Is.False);
            
            Assert.That(_subject.CanDeposit(new DateOnly(2026, 04, 05), 1), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2026, 04, 05), 3800), Is.True);
            Assert.That(_subject.CanDeposit(new DateOnly(2026, 04, 05), 3801), Is.False);
        }
    }
}