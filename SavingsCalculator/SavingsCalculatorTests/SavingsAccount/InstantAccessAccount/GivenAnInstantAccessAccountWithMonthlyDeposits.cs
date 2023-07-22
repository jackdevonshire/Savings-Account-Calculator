using SavingsCalculator.Reports;
using Account = SavingsCalculator.SavingsAccount.InstantAccessAccount;

namespace SavingsCalculatorTests.SavingsAccount.InstantAccessAccount
{
    // Change Annual Equivalent Rate
    [TestFixture(5, 100, 12, 1233, 33)]
    [TestFixture(10, 100, 12, 1267, 67)]
    [TestFixture(50, 100, 12, 1580, 380)]
    // Change Deposit Amountr
    [TestFixture(5, 200, 12, 2466, 66)]
    [TestFixture(5, 400, 12, 4932, 132)]
    [TestFixture(5, 800, 12, 9864, 264)]
    // Changing Duration - result values are rounded in the test
    [TestFixture(5, 100, 1, 100, 0)]
    [TestFixture(5, 100, 3, 303, 3)]
    [TestFixture(5, 100, 6, 609, 9)]
    [TestFixture(5, 100, 24, 2529, 129)]
    [TestFixture(5, 100, 36, 3891, 291)]
    [TestFixture(5, 100, 48, 5324, 524)]
    
    [Parallelizable]
    public class GivenAnInstantAccessAccountWithMonthlyDeposits
    {
        private readonly double _aer;
        private readonly double _expectedBalance;
        private readonly double _expectedInterest;
        private readonly double _monthlyDepositAmount;
        private readonly int _monthsOpen;
        private AccountSummary _result;

        public GivenAnInstantAccessAccountWithMonthlyDeposits(double aer, double monthlyDepositAmount, int monthsOpen, double expectedBalance, double expectedInterest)
        {
            _expectedInterest = expectedInterest;
            _expectedBalance = expectedBalance;
            _monthsOpen = monthsOpen;
            _monthlyDepositAmount = monthlyDepositAmount;
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
                    0,
                    _aer
                )
                .SetupMonthlyDeposit(startDate, endDate, 28, _monthlyDepositAmount);

            _result = subject.GetAccountSummary();
        }

        [Test]
        public void ThenTheFinalBalanceAsExpected()
        {
            Assert.That(Math.Round(_result.FinalBalance), Is.EqualTo(_expectedBalance));
        }
        
        [Test]
        public void ThenTheGainedInterestAsExpected()
        {
            Assert.That(Math.Round(_result.TotalCumulativeInterestAndBenefits), Is.EqualTo(_expectedInterest));
        }
    }
}