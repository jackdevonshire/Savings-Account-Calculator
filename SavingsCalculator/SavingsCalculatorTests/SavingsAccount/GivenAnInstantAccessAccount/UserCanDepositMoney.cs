using SavingsCalculator.Types;
using Account = SavingsCalculator.SavingsAccount.InstantAccessAccount;

namespace SavingsCalculatorTests.SavingsAccount.GivenAnInstantAccessAccount
{   
    [TestFixture(100, false)] // Can deposit a positive amount
    [TestFixture(-1, true)] // Cannot deposit negative amount
    
    [Parallelizable]
    public class UserCanDepositMoney
    {
        private readonly double _amount;
        private readonly bool _expectingError;

        public UserCanDepositMoney(double amount, bool expectingError)
        {
            _expectingError = expectingError;
            _amount = amount;
        }
        
        [Test]
        public void ThenTheResultAsExpected()
        {
            var startDate = new DateOnly(2023, 01, 01);
            var endDate = startDate.AddMonths(12);

            var subject = new Account(
                    "Instant Access Account",
                    startDate,
                    100,
                    5,
                    InterestPaidType.Monthly,
                    CompoundType.Annually
                );

            if (_expectingError)
            {
                Assert.Throws<Exception>(() => subject.Deposit(endDate, _amount));
                Assert.Throws<Exception>(() => subject.SetupMonthlyDeposit(startDate, endDate, 28, _amount));
            }
            else
            {
                Assert.DoesNotThrow(() => subject.Deposit(endDate, _amount));
                Assert.DoesNotThrow(() => subject.SetupMonthlyDeposit(startDate, endDate, 28, _amount));
            }
        }
    }
}