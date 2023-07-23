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
                4000, // Balance of 4000 added within 2023 tax year
                0
            );
        }

        [Test]
        public void ThenSummaryOnTheSameDayShowsSameBalance()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(4000));
        }
        
        [Test]
        public void ThenTheBenefitsOnThe28thMay()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 05, 28));
            Assert.That(summary.FinalBalance, Is.EqualTo(5000));
        }
        
        [Test]
        public void ThenTheBalanceRemainsTheSameForTheFollowingYears()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2028, 05, 28));
            Assert.That(summary.FinalBalance, Is.EqualTo(5000));
        }
    }
}