using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.PremiumBondsAccount;

namespace SavingsCalculatorTests.SavingsAccount.PremiumBondsAccount
{
    [Parallelizable]
    public class GivenAPremiumBondsAccount
    {
        private BaseSavingsAccount _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Account("Premium Bonds", new DateOnly(2023, 07, 28), 0);
        }

        [Test]
        public void ThenAttemptToDepositFailsAsExpected()
        {
            Assert.Throws<Exception>(() => _subject.Deposit(new DateOnly(2023, 07, 28), 25));
        }
    }
}