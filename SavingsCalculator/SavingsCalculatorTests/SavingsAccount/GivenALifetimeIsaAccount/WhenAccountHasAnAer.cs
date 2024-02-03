using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.GivenALifetimeIsaAccount
{
    [Parallelizable]
    public class WhenAccountHasAnAer
    {
        private BaseSavingsAccount _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Account(
                "Lifetime ISA",
                new DateOnly(2023, 04, 06),
                4000,
                5
            );
        }

        [Test]
        public void ThenOnTheSameDayAsTheFirstDepositNoInterestIsGained()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(4000));
        }

        [Test]
        public void ThenOnThe6thOfMayInterestIsPaid()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 05, 6));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(4017));
        }
        
        [Test]
        public void ThenOnThe28thMayGovernmentBonusIsPaid()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 05, 28));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5017));
        }
        
        [Test]
        public void ThenOnThe6thJuneInterestIsPaid()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 06, 6));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5037));
        }
    }
}