using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.GivenALifetimeIsaAccount
{
    [Parallelizable]
    public class UserCanDepositUpToLimit
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
                0,
                0
            );
        }

        [Test]
        public void ThenCanDepositToLimitForEachTaxYear()
        {
            Assert.DoesNotThrow(() => _subject.Deposit(_openingDate, 4000));
            Assert.Throws<Exception>(() => _subject.Deposit(_openingDate, 1));
            
            Assert.DoesNotThrow(() => _subject.Deposit(_openingDate.AddYears(1), 4000));
            Assert.Throws<Exception>(() => _subject.Deposit(_openingDate.AddYears(1), 1));
        }
    }
}