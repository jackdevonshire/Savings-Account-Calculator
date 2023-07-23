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
                .Deposit(new DateOnly(2025, 04, 05), 4000); // 4000 added at end of 2024 tax year
        }

        [Test]
        public void ThenOnTheSameDayAsTheFirstDepositNoInterestIsGained()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(4000));
        }
        
        [Test]
        public void ThenAMonthBeforeTheEndOfThe2023TaxYearSomeInterestIsGained()
        {
            /*
             * Start with £4000 deposited on the 4th Month of 2023
             * Interest gained each month of 2023 for £4000 is:
             *      £4000 * (0.05/12) = 16.66
             * At the end of 2023, including accrued interest for following 8 months:
             *      £4000 + (16.66*8) = £4133.28
             * Now, we have to accrue interest on another 3 months. Notice that the interest does NOT compound,
             * the interest only compounds at the accounts birthday - at-least for this program.
             * including the interest from 2023
             *
             * Now we gain 3 months interest
             *      £4133.28 + (16.66*3) = £4183.26
             * And here we have our number
             */
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 03, 05));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(4183));
        }
        
        [Test]
        public void ThenAtTheStartOfThe2024TaxYearSomeInterestIsGainedAndGovernmentBonusIsAdded()
        {
            /*
             * Following on from the above test, we now add 1 month of interest because we have progressed a month
             * So £4183.26 + (16.66*1) = £4199.92 - basically £4200
             *
             * Two special things happen this month:
             *      Firstly it is the end of the 2023 tax year, and the account is an ISA. This means that
             *      the government will give a 25% bonus to the account (not including interest and past bonuses).
             *      The 25% will apply to a maximum amount of £4000.
             *
             *      The 25% bonus in this case is 25% of the £4199.92 balance excluding interest and past bonuses,
             *      so really it is just 25% of the total deposited amounts in this case, which is £4000
             *
             *      25% of £4000 = £1000, so we add this to the accounts total balance. Giving us a value of £5200
             *
             *
             *      The second important thing to note, is that at this point it is the accounts birthday. In most cases,
             *      (and for the purpose of this program), the interest gained can now be compounded. Previously,
             *      notice how we used a flat interest payment of £16.66? This is based on the accounts balance
             *      each month, excluding prior interest gained for that year.
             *
             *      Now it is the accounts birthday, the interest gained for the previous year can essentially be compounded,
             *      i.e. taken into consideration when calculating interest for the next year. This will be explained
             *      in subsequent tests.
             */
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 04, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5200));
        }
        
        [Test]
        public void ThenAMonthBeforeTheEndOfThe204TaxYearSomeInterestIsGained()
        {
            /*
             * So the account balance in April 2024 was £5200, and we do not make any deposits to change this balance
             * throughout the following year, leading us to the month before the end of the tax year. March 2025.
             *
             * The interest rate for each month is the accounts balance that month (minus interest payments within the
             * accounts birthday year), multiplied by (AER/100)/12
             *
             * In our case, this will be: £5200 * ((5/100)/12) = £21.66
             *
             * So after 11 months, we gain £21.66 * 11 = £238.26
             * Giving us a new balance in March 2025 of £5438
             */
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 03, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(5438));
        }
        
        [Test]
        public void ThenAtTheStartOfThe2025TaxYearSomeInterestIsGainedAndGovernmentBonusIsAdded()
        {
            /*
             * Now we add the final interest payment of £21.66 giving us an amount of £5460
             * On the last day of the tax year (in test setup), we deposit a further £4000 to benefit
             * Leaving us with a balance of £9460
             *
             * Also on the last day of the tax year for 2024, the government gives a further 25% bonus
             * up to £4000, leaving us with a bonus of £1000.
             *
             * Giving us a total expected balance of £10460
             */
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 04, 06));
            Assert.That(Math.Round(summary.FinalBalance), Is.EqualTo(10460));
        }
    }
}