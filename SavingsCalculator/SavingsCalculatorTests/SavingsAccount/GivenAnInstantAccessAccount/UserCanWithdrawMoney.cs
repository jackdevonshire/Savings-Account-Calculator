using SavingsCalculator.Types;
using Account = SavingsCalculator.SavingsAccount.InstantAccessAccount;

namespace SavingsCalculatorTests.SavingsAccount.InstantAccessAccount
{   
    [TestFixture(100, false)] // Can withdraw a positive amount
    [TestFixture(-1, true)] // Cannot withdraw negative amount
    
    [Parallelizable]
    public class UserCanWithdrawMoney
    {
        private readonly double _amount;
        private readonly bool _expectingError;

        public UserCanWithdrawMoney(double amount, bool expectingError)
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
                Assert.Throws<Exception>(() => subject.Withdraw(endDate, _amount));
            }
            else
            {
                Assert.DoesNotThrow(() => subject.Withdraw(endDate, _amount));
            }
        }
    }
}