using SavingsCalculator.Reports;
using Account = SavingsCalculator.SavingsAccount.InstantAccessAccount;

namespace SavingsCalculatorTests.SavingsAccount.InstantAccessAccount
{
    // Change Annual Equivalent Rate
    [TestFixture(5, 100, 12, 105, 5)]
    [TestFixture(10, 100, 12, 110, 10)]
    [TestFixture(50, 100, 12, 150, 50)]
    // Change Starting Amount
    [TestFixture(5, 200, 12, 210, 10)]
    [TestFixture(5, 400, 12, 420, 20)]
    [TestFixture(5, 800, 12, 840, 40)]
    // Changing Duration - result values are rounded in the test
    [TestFixture(5, 100, 1, 100, 0)]
    [TestFixture(5, 100, 3, 101, 1)]
    [TestFixture(5, 100, 6, 103, 3)]
    [TestFixture(5, 100, 24, 110, 10)]
    [TestFixture(5, 100, 36, 116, 16)]
    [TestFixture(5, 100, 48, 122, 22)]
    
    [Parallelizable]
    public class GivenAnInstantAccessAccount
    {
        private readonly double _aer;
        private readonly double _expectedBalance;
        private readonly double _expectedInterest;
        private readonly double _openingBalance;
        private readonly int _monthsOpen;
        private AccountSummary _result;

        public GivenAnInstantAccessAccount(double aer, double openingBalance, int monthsOpen, double expectedBalance, double expectedInterest)
        {
            _expectedInterest = expectedInterest;
            _expectedBalance = expectedBalance;
            _monthsOpen = monthsOpen;
            _openingBalance = openingBalance;
            _aer = aer;
            _result = new AccountSummary();
        }
        
        [SetUp]
        public void Setup()
        {
            var startDate = new DateOnly(2023, 01, 01);
            var endDate = startDate.AddMonths(_monthsOpen);

            var subject = new Account(
                    "Instant Access Account",
                    startDate,
                    _openingBalance,
                    _aer
                )
                .Deposit(endDate, 0);

            _result = subject.GetAccountSummary(null);
        }

        [Test]
        public void ThenTheFinalBalanceAsExpected()
        {
            Assert.That(Math.Round(_result.FinalBalance, MidpointRounding.AwayFromZero), Is.EqualTo(_expectedBalance));
        }
        
        [Test]
        public void ThenTheGainedInterestAsExpected()
        {
            Assert.That(Math.Round(_result.TotalCumulativeInterestAndBenefits, MidpointRounding.AwayFromZero), Is.EqualTo(_expectedInterest));
        }
    }
}