using SavingsCalculator.SavingsAccount;
using Account = SavingsCalculator.SavingsAccount.LifetimeIsaAccount;

namespace SavingsCalculatorTests.SavingsAccount.LifetimeIsaAccount
{
    [Parallelizable]
    public class GivenALifetimeIsaAccountWithAnAer
    {
        private BaseSavingsAccount _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new Account(
                    "Lifetime ISA",
                    new DateOnly(2023, 04, 06),
                    4000, // Balance of 4000 added within 2023 tax year
                    5
                )
                .Deposit(new DateOnly(2025, 04, 06), 4000); // 4000 added within 2024 tax year
        }

        [Test]
        public void ThenOnTheSameDayAsTheFirstDepositNoInterestIsGained()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(4000));
        }
        
        [Test]
        public void ThenAMonthAfterTheFirstDepositSomeInterestIsGained()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 05, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(4017)); // Roughly 4016.66 recurring
        }
        
        [Test]
        public void ThenAMonthBeforeTheEndOfThe2023TaxYearSomeInterestIsGained()
        {
            /*
             * Interest Calculation Iss:
             * Start with £4000 deposited on the 4th Month of 2023
             * Interest gained each month of 2023 for £4000 is:
             *      £4000 * (0.05/12) = 16.66
             * At the end of 2023, including accrued interest for following 8 months:
             *      £4000 + (16.66*8) = £4133.28
             * Now to get to the current point, 3 months into 2024, we compound the current amount we have
             * including the interest from 2023
             *
             * The new monthly interest rate for 2024 is:
             *      £4133.28 * (0.05/12) = 17.22
             * Now we want to get to March, so calculate 3 months of interest added to balance
             *      £4133 + (17.22*3) = £4184.66
             * And here we have our number
             */
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 03, 05));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(4185));
        }
        
        [Test]
        public void ThenAtTheStartOfThe2024TaxYearSomeInterestIsGainedAndGovernmentBonusIsAdded()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 04, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5200)); //Account for jan, feb, march tax
        }
        
        [Test]
        public void ThenAMonthBeforeTheEndOfThe204TaxYearSomeInterestIsGained()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 03, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5429));
        }
        
        [Test]
        public void ThenAtTheStartOfThe2025TaxYearSomeInterestIsGainedAndGovernmentBonusIsAdded()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 04, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(9460));
        }
    }
}