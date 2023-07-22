using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.LifetimeIsaAccount
{
    [Parallelizable]
    public class GivenALifetimeIsaAccountWithNoAer
    {
        private BaseSavingsAccount _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Account(
                    "Lifetime ISA",
                    new DateOnly(2023, 04, 06),
                    4000,
                    0
                )
                .Deposit(new DateOnly(2024, 05, 06), 4000);
        }

        [Test]
        public void ThenInOneYearTheBalanceIsAsExpected()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(5000));
        }
        
        [Test]
        public void ThenInTwoYearsTheBalanceIsAsExpected()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 05, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(10000));
        }
    }
}