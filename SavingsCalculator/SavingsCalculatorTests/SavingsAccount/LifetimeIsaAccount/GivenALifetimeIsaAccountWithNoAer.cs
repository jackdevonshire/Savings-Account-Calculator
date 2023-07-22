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
                )
                .Deposit(new DateOnly(2024, 04, 06), 4000); // 4000 added within 2024 tax year
        }

        [Test]
        public void ThenSummaryAtStartOf2023TaxYearShowsNoBenefits()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2023, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(4000));
        }
        
        [Test]
        public void ThenSummaryAtEndOf2023TaxYearShowsBenefitsForThatTaxYear()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 04, 05));
            Assert.That(summary.FinalBalance, Is.EqualTo(5000));
        }
        
        [Test]
        public void ThenSummaryAtStartOf2024TaxYearOnlyShowsBenefitsFor2023TaxYear()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2024, 04, 06));
            Assert.That(summary.FinalBalance, Is.EqualTo(9000));
        }
        
        [Test]
        public void ThenSummaryAtEndOf2024TaxYearOnlyShowsBenefitsFor2024TaxYearAndPreviousYears()
        {
            var summary = _subject.GetAccountSummary(new DateOnly(2025, 04, 05));
            Assert.That(summary.FinalBalance, Is.EqualTo(10000));
        }
    }
}