using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.LifetimeIsaAccount
{
    [Parallelizable]
    public class GivenALifetimeIsaCanWithdrawWithPenalty
    {
        private BaseSavingsAccount _subject;
        private DateOnly _openingDate = new DateOnly(2023, 04, 06);

        [SetUp]
        public void Setup()
        {
            var openingDate = _openingDate;
            _subject = new Account(
                "Lifetime ISA",
                openingDate,
                4000,
                0
            )
            .Withdraw(openingDate, 1000);
        }

        [Test]
        public void ThenTheNewBalanceReflectsWithdrawalPenalty()
        {
            var summary = _subject.GetAccountSummary(_openingDate);
            Assert.That(summary.FinalBalance, Is.EqualTo(2750));
        }
        
        [Test]
        public void ThenCannotWithdrawWhenPenaltyResultsInNegativeBalance()
        {
            Assert.Throws<Exception>(() => _subject.Withdraw(_openingDate, 2750));
        }
    }
}